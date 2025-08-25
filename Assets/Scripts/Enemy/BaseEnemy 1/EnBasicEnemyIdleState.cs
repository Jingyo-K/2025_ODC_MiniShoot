using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnBasicEnemyIdleState : EnemyState
{
    [SerializeField]private EnBasicEnemy basicEnemy;
    private PlayerStat player;


    private float detectRange = 20f; // 적이 플레이어를 감지할 수 있는 거리

    public override void Enter()
    {
        // Idle 상태에 진입할 때 실행할 로직
        player = FindObjectOfType<PlayerStat>();
        basicEnemy = GetComponent<EnBasicEnemy>();
        //Debug.Log("BasicEnemy is now idle.");
    }

    public override void Execute()
    {
        if(Vector2.Distance(basicEnemy.transform.position, player.transform.position) < detectRange)
        {
            // 플레이어가 감지 범위 내에 있으면 공격 상태로 전환
            stateMachine.ChangeState(GetComponent<EnBasicEnemyMoveState>());
        }
        else
        {

        }
    }

    public override void Exit()
    {
        // Idle 상태를 벗어날 때 실행할 로직
        //Debug.Log("BasicEnemy is exiting idle state.");
    }
}

