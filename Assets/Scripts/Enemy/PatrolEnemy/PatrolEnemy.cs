using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatrolEnemy : Enemy
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(GetComponent<PatrolEnemyPatrolState>());
    }

    public override void WakeUp()
    {
        // Transition to the patrol state when the enemy wakes up
        stateMachine.ChangeState(GetComponent<PatrolEnemyPatrolState>());
    }
}
