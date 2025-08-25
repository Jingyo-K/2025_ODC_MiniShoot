using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
/// <summary>
/// PlayerMove 클래스: 이동, 회전, 입력, Look 처리 등 전반적인 플레이어 움직임 관리.
/// </summary>
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private PlayerStat playerStat; // 플레이어 스탯
    [SerializeField] private GameObject playerModel; // 플레이어 모델 (애니메이션 적용용)
    [SerializeField] private float playerSpeed = 5f;
    [SerializeField] private float playerInertia = 0.1f; //0~1;
    [SerializeField] private float stopThreshold = 0.01f;
    [SerializeField] private float rotateSpeed = 360f; // Degrees per second
    public Image dashGaugeImage; // 대시 게이지 UI 이미지

    private Coroutine dashCoroutine;
    private float dashGauge = 1f;
    private float tempSpeed = 5f;
    private float dashSpeed = 10f;
    private float maxDashTime = 2f;
    private bool isDashing = false;

    private Rigidbody2D rb;
    public PlayerInput playerInput;
    private string currentControlScheme = "KeyboardMouse"; // Default control scheme
    private PlayerStateController stateController;

    private Vector2 lookDirection;
    [SerializeField] private bool isLooking = false;

    [SerializeField] private CameraLookAheadByAime cameraLookAhead; // Reference to CameraLookAheadByAime script
    private Vector2 cameraLookDirection;
    private Vector2 smoothedLookDirection;

    [SerializeField] private float aimAssistRadius = 5f; // Aim assist radius
    [SerializeField] private float aimAssistrange = 10f; // Aim assist range
    [SerializeField] private LayerMask enemyLayerMask; // Layer mask for enemies

    private Coroutine fireCoroutine;

    Vector2 targetVelocity;
    Vector2 inputDir;


    void OnEnable()
    {
        if (playerStat != null)
        {
            playerStat.OnStatChanged += UpdateSpeed;
            playerSpeed = playerStat.stat.MoveSpeed; // Initialize player speed from PlayerStat
        }
        else
        {
            Debug.LogError("PlayerStat is not assigned in PlayerMove.");
        }
    }
    void OnDisable()
    {
        if (playerStat != null)
        {
            playerStat.OnStatChanged -= UpdateSpeed;
        }
    }
    void Start()
    {
        playerSpeed = playerStat.stat.MoveSpeed; // Initialize player speed from PlayerStat
        rb = GetComponent<Rigidbody2D>();
        playerInput = GetComponent<PlayerInput>();
        currentControlScheme = playerInput.currentControlScheme;
        stateController = GetComponent<PlayerStateController>();
        if (stateController == null) Debug.LogError("PlayerStateController 필요!");
    }

    void Update()
    {
        if (GameSceneController.Instance != null && GameSceneController.CurrentGameState == GameSceneController.GameState.Paused && isDashing)
        {
            isDashing = false; // 대시 중지
            playerSpeed = tempSpeed; // 대시가 끝나면 원래 속도로 복원
            return; // 게임이 일시정지 상태일 때는 입력을 무시
        }
        else
        {
            if (isDashing && dashGauge > 0f)
            {
                dashGauge -= Time.deltaTime / maxDashTime;
                dashGaugeImage.fillAmount = dashGauge; // 대시 게이지 UI 업데이트
            }
            else if (isDashing && dashGauge <= 0f)
            {
                playerSpeed = tempSpeed; // 대시가 끝나면 원래 속도로 복원
                if (dashCoroutine == null)
                {
                    dashCoroutine = StartCoroutine(DashCooldown()); // 대시 쿨타임 시작
                }
            }
            else if (!isDashing && dashGauge < 1f)
            {
                dashGauge += Time.deltaTime;
                dashGaugeImage.fillAmount = dashGauge; // 대시 게이지 UI 업데이트
            }
            // 입력 방식 체크
            if (playerInput.currentControlScheme != currentControlScheme)
            {
                currentControlScheme = playerInput.currentControlScheme;
            }

            // --- [실시간 입력 읽기: Gamepad] ---
            if (currentControlScheme == "Gamepad")
            {
                inputDir = Gamepad.current != null ? Gamepad.current.leftStick.ReadValue() : Vector2.zero;

                // FireG가 발동되어 있을 때는 RightStick 조준/발사
                if (isLooking)
                {
                    Vector2 right = Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() : Vector2.zero;
                    if (right.sqrMagnitude > 0.01f)
                    {
                        lookDirection = right.normalized;

                        // lookDirection 근처에 Enemy가 있다면 그 방향으로 조준
                        RaycastHit2D[] hits = Physics2D.CircleCastAll(rb.position, aimAssistRadius, lookDirection, aimAssistrange, enemyLayerMask);
                        if (hits.Length > 0)
                        {
                            Vector2 closestEnemyPosition = hits[0].point;
                            foreach (var hit in hits)
                            {
                                if (Vector2.Distance(rb.position, hit.point) < Vector2.Distance(rb.position, closestEnemyPosition))
                                {
                                    closestEnemyPosition = hit.point;
                                }
                            }
                            lookDirection = (closestEnemyPosition - rb.position).normalized;
                        }
                        GetComponent<PlayerFire>().SetFireDirection(lookDirection); // Set fire direction for PlayerFire
                                                                                    //Camera.main.GetComponent<CameraFollowDamping>().SetLookDirection(lookDirection); // Update camera look direction
                    }
                }
                else if (inputDir.sqrMagnitude > 0.01f)
                {
                    lookDirection = inputDir.normalized;
                }
            }
            // --- [실시간 입력 읽기: Keyboard/Mouse] ---
            else if (currentControlScheme == "KeyboardMouse")
            // 마우스 위치를 월드 좌표로 변환하여 LookDirection 계산
            {
                if (isLooking)
                {
                    Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                    Vector2 playerPosition = rb.position;
                    lookDirection = (worldMousePos - (Vector3)playerPosition).normalized;

                    //lookDirection 근처에 Enemy가 있다면 그 방향으로 조준
                    RaycastHit2D[] hits = Physics2D.CircleCastAll(playerPosition, aimAssistRadius, lookDirection, aimAssistrange, enemyLayerMask);
                    if (hits.Length > 0)
                    {
                        Vector2 closestEnemyPosition = hits[0].point;
                        foreach (var hit in hits)
                        {
                            if (Vector2.Distance(playerPosition, hit.point) < Vector2.Distance(playerPosition, closestEnemyPosition))
                            {
                                closestEnemyPosition = hit.point;
                            }
                        }
                        lookDirection = (closestEnemyPosition - playerPosition).normalized;
                    }

                    GetComponent<PlayerFire>().SetFireDirection(lookDirection); // Set fire direction for PlayerFire
                                                                                //Camera.main.GetComponent<CameraFollowDamping>().SetLookDirection(lookDirection); // Update camera look direction
                }
                else if (inputDir.sqrMagnitude > 0.01f)
                {
                    lookDirection = inputDir.normalized;
                }
            }

            // --- [이동 처리] ---
            if (inputDir.sqrMagnitude > 0.01f)
            {
                targetVelocity = inputDir.normalized * playerSpeed;
                rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, playerInertia);
            }
            else
            {
                targetVelocity = Vector2.zero;
                rb.velocity = Vector2.Lerp(rb.velocity, targetVelocity, playerInertia);

                if (rb.velocity.magnitude < stopThreshold)
                {
                    rb.velocity = Vector2.zero;
                }
            }

            // --- [회전 처리] ---
            if (lookDirection.sqrMagnitude > 0.01f)
            {
                float targetAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 90f;
                float currentAngle = playerModel.transform.eulerAngles.z;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle, rotateSpeed * Time.deltaTime);
                playerModel.transform.rotation = Quaternion.Euler(0, 0, newAngle);
            }
            // --- [카메라 LookAhead 처리] ---
            float lookSmooth = 0.15f;
            smoothedLookDirection = Vector2.Lerp(smoothedLookDirection, lookDirection, lookSmooth);

            if (cameraLookAhead != null)
            {
                float dist = Vector2.Distance(cameraLookDirection, smoothedLookDirection);
                float minSpeed = 0.2f;
                float maxSpeed = 2f;
                float t = Mathf.Clamp01(dist / 2f);
                float speed = Mathf.Lerp(minSpeed, maxSpeed, t);

                cameraLookDirection = Vector2.MoveTowards(cameraLookDirection, smoothedLookDirection, speed * Time.deltaTime);

                cameraLookAhead.lookDirection = cameraLookDirection;
            }
            else
            {
                Debug.LogError("CameraLookAheadByAime is not assigned in PlayerMove.");
            }
        }
    
    }

    /// <summary>
    /// 밸류만 받고 속도 보간은 Update에서 처리
    /// </summary>
    public void OnMove(InputAction.CallbackContext value)
    {
        if (GameSceneController.Instance != null && GameSceneController.CurrentGameState == GameSceneController.GameState.Paused)
        {
            return; // 게임이 일시정지 상태일 때는 입력을 무시
        }
        if (currentControlScheme != "Gamepad")
        {
            inputDir = value.ReadValue<Vector2>();
            if (inputDir.sqrMagnitude > 0.01f)
                stateController.SetState(MoveState.Move);
            else
                stateController.SetState(MoveState.Idle);
        }
        else
        {
            if (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f)
                stateController.SetState(MoveState.Move);
            else
                stateController.SetState(MoveState.Idle);
        }
    }

    /// <summary>
    /// 키보드/마우스: Fire(마우스 버튼), 게임패드: 트리거(버튼)
    /// </summary>
    public void OnFire(InputAction.CallbackContext value)
    {
        if (GameSceneController.Instance != null && GameSceneController.CurrentGameState == GameSceneController.GameState.Paused)
        {
            return; // 게임이 일시정지 상태일 때는 입력을 무시
        }
        if (value.performed)
        {
            isLooking = true; // Player is looking when firing
            stateController.SetState(AttackState.Fire);
            fireCoroutine = StartCoroutine(FireCoroutine());
        }
        else if (value.canceled)
        {
            isLooking = false; // Player stops looking when fire is released
            stateController.SetState(AttackState.Idle);
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
            }
        }
    }

    /// <summary>
    /// 게임패드용 - RightStick 방향 입력이 0이 아니면 발사 시작, 0이면 발사 중지.
    /// </summary>
    public void OnFireG(InputAction.CallbackContext value)
    {
        if (GameSceneController.Instance != null && GameSceneController.CurrentGameState == GameSceneController.GameState.Paused)
        {
            return; // 게임이 일시정지 상태일 때는 입력을 무시
        }
        Vector2 fireDirection = value.ReadValue<Vector2>();
        if (fireDirection.sqrMagnitude > 0.01f)
        {
            isLooking = true; // Player is looking when firing
            stateController.SetState(AttackState.Fire);
            if (fireCoroutine == null)
                fireCoroutine = StartCoroutine(FireCoroutine());
        }
        else
        {
            isLooking = false; // Player stops looking when fire is released
            stateController.SetState(AttackState.Idle);
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
            }
        }
    }

    public void OnDash(InputAction.CallbackContext value)
    {
        if (GameSceneController.Instance != null && GameSceneController.CurrentGameState == GameSceneController.GameState.Paused)
        {
            return; // 게임이 일시정지 상태일 때는 입력을 무시
        }
        if (value.started && !isDashing && dashGauge >= 0.1f)
        {
            tempSpeed = playerSpeed; // 대시 중에는 플레이어 속도를 임시로 저장
            dashSpeed = playerSpeed * 2f; // 대시 속도는 플레이어 속도의 2배
            playerSpeed = dashSpeed; // 대시 속도로 변경
            isDashing = true;
        }
        else if (value.canceled && isDashing)
        {
            if (dashCoroutine == null)
            {
                isDashing = false;
            }
            playerSpeed = tempSpeed; // 대시가 끝나면 원래 속도로 복원
            Debug.Log("Dashing stopped, playerSpeed: " + playerSpeed);
        }
    }

    private IEnumerator DashCooldown()
    {
        Color originalColor = dashGaugeImage.color;
        float elapsed = 0f;

        while (elapsed < 2f)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.PingPong(elapsed * 2f, 1f); // Flicker speed = 2, adjust as needed
            dashGaugeImage.color = Color.Lerp(originalColor, Color.white, t);
            dashGaugeImage.fillAmount = elapsed / 2f; // Fill amount based on elapsed time
            yield return null;
        }
        dashGaugeImage.color = originalColor; // Reset color to original
        isDashing = false; // 대시 상태를 false로 설정
        dashGauge = 1f; // 대시 게이지 초기화
        dashGaugeImage.fillAmount = dashGauge; // 대시 게이지 UI 업데이트
        dashCoroutine = null; // 대시 코루틴 초기화
    }

    public float getSpeed()
    {
        return playerSpeed;
    }

    /// <summary>
    /// Implement firing logic here
    /// </summary>
    private IEnumerator FireCoroutine()
    {
        Debug.Log("Firing logic started");
        // Implement firing logic (projectile, cooldown, etc) here
        yield return null; // Placeholder for coroutine logic
    }

    private void UpdateSpeed(PlayerStat playerStat)
    {
        playerSpeed = playerStat.stat.MoveSpeed;
        Debug.Log("Player speed updated: " + playerSpeed);
    }
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, lookDirection.normalized * aimAssistrange);
        Gizmos.DrawWireSphere(transform.position + (Vector3)lookDirection.normalized * aimAssistrange, aimAssistRadius);
    }
}
