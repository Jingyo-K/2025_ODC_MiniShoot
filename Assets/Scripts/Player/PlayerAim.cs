using UnityEngine;

/// <summary>
/// 플레이어의 조준 관련 기능을 처리하는 클래스입니다.
/// </summary>
[RequireComponent(typeof(PlayerMove))]
public class PlayerAim : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInputHandler inputHandler;
    [SerializeField] private PlayerMove playerMove;
    [SerializeField] private Transform playerModel;
    [SerializeField] private Rigidbody2D rb;

    [Header("Aim")]
    [SerializeField] private float aimRotateSpeedDegPerSec = 720f;
    [SerializeField] private float aimHoldSeconds = 0.15f;

    [Header("Assist")]
    [SerializeField] private bool enableAimAssist = true;
    [SerializeField] private float assistRadius = 5f;
    [SerializeField] private float assistRange = 10f;
    [SerializeField] private LayerMask enemyMask = ~0;
    [SerializeField] private float snapAngleDeg = 15f;

    private Vector2 targetAimDir = Vector2.up;
    private bool aimActive = false;

    private void OnEnable()
    {
        if (!inputHandler) inputHandler = FindObjectOfType<PlayerInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.OnFirePressed += AimOnce;
            inputHandler.OnFireAiming += AimOnce;
            inputHandler.OnFireReleased += AimReleased;
        }
    }

    private void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnFirePressed -= AimOnce;
            inputHandler.OnFireAiming -= AimOnce;
            inputHandler.OnFireReleased -= AimReleased;
        }
    }

    private void AimOnce(Vector2 dir)
    {
        aimActive = true;
        if (dir.sqrMagnitude < 0.0001f || playerModel == null) return;

        Vector2 origin = rb ? rb.position : (Vector2)transform.position;
        Vector2 handleDir = dir.normalized;
        if (enableAimAssist) handleDir = ApplyAimAssist(origin, handleDir);

        targetAimDir = handleDir;
        RotateModelTowards(targetAimDir, aimRotateSpeedDegPerSec);
    }

    private void AimReleased()
    {
        aimActive = false;
    }

    private void Update()
    {
        if (aimActive && playerModel != null)
            RotateModelTowards(targetAimDir, aimRotateSpeedDegPerSec);
    }

    private void RotateModelTowards(Vector2 dir, float degPerSec)
    {
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float current = playerModel.eulerAngles.z;
        float next = Mathf.MoveTowardsAngle(current, targetAngle, degPerSec * Time.deltaTime);
        playerModel.rotation = Quaternion.Euler(0, 0, next);
    }

    private Vector2 ApplyAimAssist(Vector2 origin, Vector2 inputDir)
    {
        var hits = Physics2D.CircleCastAll(origin, assistRadius, inputDir, assistRange, enemyMask);
        if (hits == null || hits.Length == 0) return inputDir;

        Vector2 best = hits[0].point; float bestDist = Vector2.Distance(origin, best);
        for (int i = 1; i < hits.Length; i++)
        {
            float d = Vector2.Distance(origin, hits[i].point);
            if (d < bestDist) { best = hits[i].point; bestDist = d; }
        }
        Vector2 toTarget = (best - origin).normalized;

        // 과도한 지원으로 인한 위화감 방지
        return Vector2.Angle(inputDir, toTarget) <= snapAngleDeg ? toTarget : inputDir;
    }
}
