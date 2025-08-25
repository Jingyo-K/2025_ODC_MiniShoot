using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Boss1 : Enemy
{
    protected override void Start()
    {
        base.Start();
        // Initialize the enemy state machine with the idle state
        stateMachine.Initialize(GetComponent<Boss1Phase1>());
    }
    public void GoToPhase2()
    {
       // Transition to Phase 2 state
        stateMachine.ChangeState(GetComponent<Boss1Phase2>());
    }
    public void GoToPhase3()
    {
        stateMachine.ChangeState(GetComponent<Boss1Phase3>());
    }
}
