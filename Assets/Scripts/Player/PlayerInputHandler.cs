using System;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// 플레이어의 입력을 처리하는 클래스입니다.
/// 게임패드와 마우스 상관 없이 동일한 이벤트를 발생시키도록 함으로써 기존의 비직관성을 해소합니다.
/// </summary>
[RequireComponent(typeof(PlayerInput))]
public class PlayerInputHandler : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera worldCamera;
    [SerializeField] private PlayerStateController stateController;

    [Header("Tuning")]
    [SerializeField] private float enterDeadzone = 0.15f; // 활성화 임계치
    [SerializeField] private float exitDeadzone  = 0.10f; // 비활성 임계치(히스테리시스)

    // 표준 이벤트
    public event Action<Vector2> OnMoveInput;
    public event Action<Vector2> OnFirePressed;   // 눌린 순간 조준각 1회
    public event Action<Vector2> OnFireAiming;    // 눌러두는 동안 연속 조준각
    public event Action OnFireReleased;
    public event Action OnDashPressed;
    public event Action OnDashReleased;

    private PlayerInput playerInput;
    private bool isFiringByMouse = false;
    private bool isFiringByStick = false;

    

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (!worldCamera) worldCamera = Camera.main;
        if (!playerTransform) Debug.LogError("[PlayerInputHandler] playerTransform 참조 필요!");
    }

    // ===== Move (WASD / Left Stick) =====
    public void OnMove(InputAction.CallbackContext ctx)
    {
        Vector2 dir = ctx.ReadValue<Vector2>();
        if (ctx.performed || ctx.canceled)
        {
            OnMoveInput?.Invoke(dir);
            if (stateController != null)
                stateController.SetState(dir.sqrMagnitude > 0.0001f ? MoveState.Move : MoveState.Idle);
        }
    }

    // ===== Fire (Mouse): 눌린 순간 + 홀드 중 연속 에이밍(Update 폴링) =====
    public void OnFireMouse(InputAction.CallbackContext ctx)
    {
        if (!playerTransform || !worldCamera) return;

        if (ctx.started)
        {
            isFiringByMouse = true;

            // 눌린 순간 1회 샘플
            var dir = GetMouseAimDir();
            if (dir.sqrMagnitude > 0.0001f)
                OnFirePressed?.Invoke(dir);

            if (stateController) stateController.SetState(AttackState.Fire);
        }
        else if (ctx.canceled)
        {
            isFiringByMouse = false;
            OnFireReleased?.Invoke();
            if (stateController) stateController.SetState(AttackState.Idle);
        }
    }

    private Vector2 GetMouseAimDir()
    {
        Vector2 screenPos = Mouse.current != null ? Mouse.current.position.ReadValue() : Vector2.zero;
        float z = Mathf.Abs(worldCamera.transform.position.z - playerTransform.position.z);
        Vector3 worldPos = worldCamera.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, z));
        return ((Vector2)worldPos - (Vector2)playerTransform.position).normalized;
    }

    private void Update()
    {
        // 마우스 클릭 유지 중엔 폴링으로 연속 에이밍 이벤트 발행
        if (isFiringByMouse)
        {
            var dir = GetMouseAimDir();
            if (dir.sqrMagnitude > 0.0001f)
                OnFireAiming?.Invoke(dir);
        }
    }

    // ===== Fire (Gamepad R-stick): 활성 중 매 프레임 연속 에이밍 =====
    public void OnFireG(InputAction.CallbackContext ctx)
    {
        
        Vector2 stick = ctx.ReadValue<Vector2>();
        float mag = stick.magnitude;

        
        if (!isFiringByStick && mag > enterDeadzone)
        {
            isFiringByStick = true;
            Vector2 dir = stick.normalized;
            OnFirePressed?.Invoke(dir);
            if (stateController) stateController.SetState(AttackState.Fire);
            // 즉시 한 번 에이밍
            OnFireAiming?.Invoke(dir);
            return;
        }

        // 2) 활성 유지: 연속 에이밍 (Performed 콜백마다)
        if (isFiringByStick && mag > exitDeadzone)
        {
            OnFireAiming?.Invoke(stick.normalized);
            return;
        }

        
        if (isFiringByStick && mag <= exitDeadzone)
        {
            isFiringByStick = false;
            OnFireReleased?.Invoke();
            if (stateController) stateController.SetState(AttackState.Idle);
            return;
        }
    }

    // ===== Dash =====
    public void OnDash(InputAction.CallbackContext ctx)
    {
        if (ctx.started) OnDashPressed?.Invoke();
        else if (ctx.canceled) OnDashReleased?.Invoke();
    }
}
