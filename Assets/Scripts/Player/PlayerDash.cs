using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// PlayerDash:
/// - 입력: PlayerInputHandler.OnDashPressed/Released 구독
/// - 속도: PlayerMove.SetSpeedModifier(multiplier) 로만 임시 오버레이
/// - 게이지: 대시 중 소모, 비대시 상태에서 서서히 회복
/// - 쿨타임: 게이지 바닥시 쿨타임 코루틴(시각 효과 포함), 회복 후 자동 해제
/// - 회전/조준에는 개입하지 않음
/// </summary>
[RequireComponent(typeof(PlayerMove))]
public class PlayerDash : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private Image dashGaugeImage; // 선택: null이면 UI 갱신 생략

    [Header("Dash Settings")]
    [SerializeField, Tooltip("대시 속도 배율 (기본 속도의 배수)")]
    private float dashSpeedMultiplier = 2f;

    [SerializeField, Tooltip("게이지가 1에서 0까지 소모되는 최대 지속시간(초)")]
    private float maxDashDuration = 2f;

    [SerializeField, Tooltip("게이지 자연 회복 속도(초당 게이지)")]
    private float regenPerSecond = 0.6f;

    [SerializeField, Tooltip("게이지가 0이 되었을 때의 쿨타임(초)")]
    private float cooldownSeconds = 2f;

    [Header("Cooldown Visual")]
    [SerializeField, Tooltip("쿨타임 동안 게이지 색상 깜빡임 사용")]
    private bool flickerOnCooldown = true;

    private float gauge = 1f;
    private bool isDashing = false;
    private bool inCooldown = false;
    private Coroutine cooldownCo;

    // (선택) 일시정지 처리에 사용할 게임 상태가 있다면 참조
    // GameSceneController가 있다면 true 반환하는 헬퍼
    private bool IsPaused()
    {
        return GameSceneController.Instance != null
            && GameSceneController.CurrentGameState == GameSceneController.GameState.Paused;
    }

    private void Reset()
    {
        playerMove = GetComponent<PlayerMove>();
    }

    private void OnEnable()
    {
        if (!playerMove) playerMove = GetComponent<PlayerMove>();
        if (inputHandler == null) inputHandler = FindObjectOfType<PlayerInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.OnDashPressed  += HandleDashPressed;
            inputHandler.OnDashReleased += HandleDashReleased;
        }
        UpdateUI();
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnDashPressed  -= HandleDashPressed;
            inputHandler.OnDashReleased -= HandleDashReleased;
        }
        EndDashImmediate(); // 안전 정리
    }

    private void Update()
    {
        if (IsPaused())
        {
            if (isDashing) EndDashImmediate();
            return;
        }

        // 대시 중: 게이지 소모
        if (isDashing && !inCooldown)
        {
            float consumePerSec = (maxDashDuration <= 0f) ? 1f : (1f / maxDashDuration);
            gauge -= consumePerSec * Time.deltaTime;
            if (gauge <= 0f)
            {
                gauge = 0f;
                // 즉시 대시 종료 + 쿨타임
                EndDashImmediate();
                StartCooldown();
            }
            UpdateUI();
        }
        // 비대시 & 쿨타임 아님: 자연 회복
        else if (!isDashing && !inCooldown)
        {
            if (gauge < 1f)
            {
                gauge = Mathf.Min(1f, gauge + regenPerSecond * Time.deltaTime);
                UpdateUI();
            }
        }
    }

    // ===== 입력 핸들러에서 호출 =====
    private void HandleDashPressed()
    {
        if (IsPaused()) return;
        if (inCooldown) return;
        if (gauge <= 0.05f) return; // 시작 최소치

        BeginDash();
    }

    private void HandleDashReleased()
    {
        if (IsPaused()) return;
        if (!isDashing) return;

        // 버튼 해제: 대시 종료 (게이지 남아있으면 쿨타임 없이 자연 회복)
        EndDashImmediate();
    }

    // ===== 대시 상태 전환 =====
    private void BeginDash()
    {
        if (isDashing) return;
        isDashing = true;
        playerMove.SetSpeedModifier(dashSpeedMultiplier);
    }

    private void EndDashImmediate()
    {
        if (!isDashing) return;
        isDashing = false;
        playerMove.SetSpeedModifier(1f);
    }

    private void StartCooldown()
    {
        if (inCooldown) return;
        inCooldown = true;
        if (cooldownCo != null) StopCoroutine(cooldownCo);
        cooldownCo = StartCoroutine(CooldownRoutine());
    }

    private IEnumerator CooldownRoutine()
    {
        Color? original = dashGaugeImage ? dashGaugeImage.color : (Color?)null;

        float t = 0f;
        while (t < cooldownSeconds)
        {
            t += Time.deltaTime;

            if (flickerOnCooldown && dashGaugeImage)
            {
                // 간단한 깜빡임
                float f = Mathf.PingPong(t * 2f, 1f);
                dashGaugeImage.color = Color.Lerp(original.Value, Color.white, f);
                dashGaugeImage.fillAmount = Mathf.Clamp01(t / Mathf.Max(0.01f, cooldownSeconds));
            }

            yield return null;
        }

        // 쿨타임 종료: 게이지 풀 회복
        gauge = 1f;
        inCooldown = false;
        cooldownCo = null;

        if (dashGaugeImage && original.HasValue)
        {
            dashGaugeImage.color = original.Value;
            UpdateUI();
        }
    }

    // ===== UI =====
    private void UpdateUI()
    {
        if (dashGaugeImage)
            dashGaugeImage.fillAmount = gauge;
    }

    // ===== 외부에서 튜닝 시 유틸 =====
    public void SetGauge(float normalized) 
    {
        gauge = Mathf.Clamp01(normalized);
        UpdateUI();
    }

    public float GetGauge01() => gauge;
}
