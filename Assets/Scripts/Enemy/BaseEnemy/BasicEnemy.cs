using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemy : Enemy
{
    protected override void Start()
    {
        base.Start();
        // Initialize the enemy state machine with the idle state
        stateMachine.Initialize(GetComponent<BasicEnemyIdleState>());
    }

    public override void WakeUp()
    {
        // Transition to the awake state when the enemy wakes up
        stateMachine.ChangeState(GetComponent<BasicEnemyIdleState>());
    }
}
