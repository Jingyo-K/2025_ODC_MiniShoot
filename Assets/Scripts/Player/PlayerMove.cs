using UnityEngine;

/// <summary>
/// 이동명령에 따른 플레이어 이동 및 회전을 처리합니다.
/// 공격 조준이 입력될 경우 이 스크립트 내부에서의 회전 처리를 차단하여 조준 입력의 우선권을 보장합니다.
/// PlayerDash에서 속도 오버레이를 위해 SetSpeedModifier(float) API를 제공합니다.
/// </summary>
public class PlayerMove : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerStat playerStat;
    [SerializeField] private Transform playerModel;

    [Header("Move")]
    [SerializeField] private float baseSpeed = 5f;
    [SerializeField, Range(0f,1f)] private float inertia = 0.1f;
    [SerializeField] private float stopThreshold = 0.01f;

    [Header("Rotate (Basic on Move)")]
    [SerializeField] private float rotateSpeed = 360f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private float speedModifier = 1f;

    // 우선권/발사 상태
    private bool isFireHeld = false;        // 입력 핸들러가 세팅

    public float Speed
    {
        get
        {
            return Mathf.Max(0f, baseSpeed) * Mathf.Max(0f, speedModifier);
        }
    }
    public Vector2 ModelUp => playerModel ? (Vector2)playerModel.up : Vector2.up;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (inputHandler == null) inputHandler = FindObjectOfType<PlayerInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.OnMoveInput     += ApplyMove;
            // 발사 상태 구독으로 기본 회전 차단
            inputHandler.OnFirePressed   += OnFirePressed;
            inputHandler.OnFireReleased  += OnFireReleased;
        }
        if (playerStat != null)
        {
            baseSpeed = playerStat.stat.MoveSpeed;
            playerStat.OnStatChanged += OnStatChanged;
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnMoveInput     -= ApplyMove;
            inputHandler.OnFirePressed   -= OnFirePressed;
            inputHandler.OnFireReleased  -= OnFireReleased;
        }
        if (playerStat != null)
            playerStat.OnStatChanged -= OnStatChanged;
    }

    private void Update()
    {
        Vector2 targetVel = moveInput.sqrMagnitude > 0.0001f ? moveInput.normalized * Speed : Vector2.zero;
        rb.velocity = Vector2.Lerp(rb.velocity, targetVel, inertia);
        if (rb.velocity.magnitude < stopThreshold) rb.velocity = Vector2.zero;

        if (!isFireHeld && moveInput.sqrMagnitude > 0.0001f && playerModel != null)
        {
            float targetAngle = Mathf.Atan2(moveInput.y, moveInput.x) * Mathf.Rad2Deg - 90f;
            float current = playerModel.eulerAngles.z;
            float next = Mathf.MoveTowardsAngle(current, targetAngle, rotateSpeed * Time.deltaTime);
            playerModel.rotation = Quaternion.Euler(0, 0, next);
        }
    }

    private void OnStatChanged(PlayerStat s)
    {
        baseSpeed = s.stat.MoveSpeed;
    }

    // === 외부 API ===
    public void ApplyMove(Vector2 dir) => moveInput = dir;
    public void SetSpeedModifier(float m) => speedModifier = Mathf.Max(0f, m);

    // 발사 상태 토글(PlayerInputHandler가 호출)
    private void OnFirePressed(Vector2 dir)  => isFireHeld = true;
    private void OnFireReleased() => isFireHeld = false;
}
