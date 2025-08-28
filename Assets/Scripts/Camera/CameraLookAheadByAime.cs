using System.Collections;
using UnityEngine;
using Cinemachine;

/// <summary>
/// 카메라의 시점 이동을 보조하는 클래스입니다.
/// 기본적으로 플레이어가 바라보는 방향을 향해 카메라가 부드럽게 앞서나갑니다
/// 만약 발사 조준이 입력될 경우, 조준 방향을 우선하여 카메라가 앞서나가도록 합니다.
/// </summary>
public class CameraLookAheadByAime : CinemachineExtension
{
    [Header("Facing Source")]
    [SerializeField] private Transform facingSource;

    [Header("Input")]
    [SerializeField] private PlayerInputHandler inputHandler;        // OnFirePressed/Released 구독

    [Header("LookAhead Distance")]
    public float lookAheadDistance = 2.0f;
    public float saveLookAheadDistance = 0.0f;
    private Coroutine distanceCoroutine;

    [Header("Direction Smoothing")]
    [SerializeField] private float lookSmooth = 0.15f;
    [SerializeField] private float minSpeed = 0.2f;
    [SerializeField] private float maxSpeed = 2.0f;
    [SerializeField] private float distScale = 2.0f;

    // 내부 상태
    public Vector2 lookDirection = Vector2.right; // 최종 오프셋 방향
    private Vector2 smoothedLookDirection;
    private Vector2 cameraLookDirection;
    private bool initialized;

    //구독자 핸들러
    private void OnFirePressed(Vector2 _) => StartDistanceLerp(saveLookAheadDistance * 1.4f);
    private void OnFireReleased() => StartDistanceLerp(saveLookAheadDistance);

    protected override void OnEnable()
    {
        base.OnEnable();
        saveLookAheadDistance = lookAheadDistance;

        if (inputHandler == null) inputHandler = FindObjectOfType<PlayerInputHandler>();
        if (inputHandler != null)
        {
            inputHandler.OnFirePressed += OnFirePressed;
            inputHandler.OnFireReleased += OnFireReleased;
        }

        InitializeDirection();
    }

    protected void OnDisable()
    {
        if (inputHandler != null)
        {
            inputHandler.OnFirePressed -= OnFirePressed;
            inputHandler.OnFireReleased -= OnFireReleased;
        }
    }

    private void InitializeDirection()
    {
        if (facingSource != null)
        {
            Vector2 dir = facingSource.up;
            smoothedLookDirection = dir;
            cameraLookDirection = dir;
            lookDirection = dir;
            initialized = true;
        }
        else
        {
            initialized = false;
        }
    }

    private void StartDistanceLerp(float target)
    {
        if (distanceCoroutine != null) StopCoroutine(distanceCoroutine);
        distanceCoroutine = StartCoroutine(LerpLookAheadDistance(target));
    }

    private IEnumerator LerpLookAheadDistance(float targetDistance)
    {
        float startDistance = lookAheadDistance;
        float elapsedTime = 0f;
        float duration = 0.5f;

        while (elapsedTime < duration)
        {
            lookAheadDistance = Mathf.Lerp(startDistance, targetDistance, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        lookAheadDistance = targetDistance;
        distanceCoroutine = null;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body) return;

        if (!initialized)
            InitializeDirection();

        // 방향 스무딩
        if (facingSource != null)
        {
            Vector2 target = facingSource.up;

            smoothedLookDirection = Vector2.Lerp(smoothedLookDirection, target, lookSmooth);

            float dist = Vector2.Distance(cameraLookDirection, smoothedLookDirection);
            float t = Mathf.Clamp01(dist / Mathf.Max(0.0001f, distScale));
            float speed = Mathf.Lerp(minSpeed, maxSpeed, t);

            cameraLookDirection = Vector2.MoveTowards(cameraLookDirection, smoothedLookDirection, speed * deltaTime);

            lookDirection = cameraLookDirection;
        }

        // 카메라 보정 
        Vector3 offset = (Vector3)lookDirection * lookAheadDistance;
        state.PositionCorrection += offset;
    }
}
