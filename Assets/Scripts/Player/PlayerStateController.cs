using UnityEngine;
using System;

/// <summary>
/// 플레이어의 현재 State를 관리하는 컨트롤러.
/// </summary>
public enum MoveState
{
    Idle,
    Move,
    Fire
}

public enum AttackState
{
    Idle
    , Fire
}
/// <summary>
/// Player State Controller.
/// 상태 변화시 OnStateChanged 이벤트를 발생시킴.
/// </summary>
public class PlayerStateController : MonoBehaviour
{
    public MoveState CurrentState { get; private set; } = MoveState.Idle;
    public AttackState CurrentAttackState { get; private set; } = AttackState.Idle;
    [SerializeField] private MoveState curState = MoveState.Idle;
    public event Action<MoveState> OnStateChanged;
    public static event Action<AttackState> OnAttackStateChanged;

    /// <summary>
    /// 상태 전환 함수.
    /// </summary>
    public void SetState(MoveState newState)
    {
        if (CurrentState != newState)
        {
            CurrentState = newState;
            curState = newState; // Update the serialized field for inspector visibility
            OnStateChanged?.Invoke(newState);
        }
    }

    public void SetState(AttackState newState)
    {
        if (CurrentAttackState != newState)
        {
            CurrentAttackState = newState;
            OnAttackStateChanged?.Invoke(newState); // Notify state change
        }
    }


}
