using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1. 90도 방향으로 레이캐스트를 쏘아 "Wall" 태그 확인 해서 회전 후 직진 가능한지 확인
/// 2. 회전 후 직진 가능한 방향으로 이동
/// 3. 3번 반복
/// 4. 근처에 플레이어가 있으면 공격 상태로 전환
/// 5. 플레이어가 없으면 계속 순찰 상태 유지
/// </summary>
public class PatrolEnemyPatrolState : EnemyState
{
    private PatrolEnemy patrolEnemy;
    private PlayerStat player;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private float AttackDistance = 5f;
    [SerializeField] private float waitTime = 1f; // 대기 시간
    private Vector2 targetPosition;
    private Vector2 moveDirection;
    private Quaternion targetRotation;
    private int currentPatrolCount = 0;
    private int maxPatrolCount = 3;
    private float patrolDistance = 2f;

    private enum PatrolPhase { Idle, ChooseDirection, Rotating, Moving, Waiting }
    private PatrolPhase phase = PatrolPhase.Idle;

    public override void Enter()
    {
        patrolEnemy = GetComponent<PatrolEnemy>();
        player = FindObjectOfType<PlayerStat>();
        targetPosition = patrolEnemy.transform.position;
        phase = PatrolPhase.ChooseDirection;
    }

    public override void Execute()
    {

        switch (phase)
        {
            case PatrolPhase.ChooseDirection:
                ChooseRandomDirection();
                break;

            case PatrolPhase.Rotating:
                RotateTowardsDirection();
                break;

            case PatrolPhase.Moving:
                MoveTowardsTarget();
                break;

            case PatrolPhase.Waiting:
                Waiting();
                break;

            case PatrolPhase.Idle:
                break;
        }
    }

    private void ChooseRandomDirection()
    {
        List<Vector2> directions = new List<Vector2>();

        TryAddDirection(transform.right, directions);
        TryAddDirection(-transform.right, directions);

        if (directions.Count > 0)
        {
            moveDirection = directions[Random.Range(0, directions.Count)];
            targetPosition = (Vector2)patrolEnemy.transform.position + moveDirection * patrolDistance;

            float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg - 90f;
            targetRotation = Quaternion.Euler(0, 0, angle);

            phase = PatrolPhase.Rotating;
        }
        else
        {
            // 모든 방향이 막혔을 경우 → 패스
            phase = PatrolPhase.Idle;
        }
    }

    private void RotateTowardsDirection()
    {
        patrolEnemy.transform.rotation = Quaternion.RotateTowards(
            patrolEnemy.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        if (Quaternion.Angle(patrolEnemy.transform.rotation, targetRotation) < 1f)
        {
            patrolEnemy.transform.rotation = targetRotation;
            phase = PatrolPhase.Moving;
        }
    }

    private void MoveTowardsTarget()
    {
        patrolEnemy.transform.position = Vector2.MoveTowards(
            patrolEnemy.transform.position,
            targetPosition,
            moveSpeed * Time.deltaTime
        );

        if (Vector2.Distance(patrolEnemy.transform.position, targetPosition) < 0.05f)
        {
            patrolEnemy.transform.position = targetPosition;
            currentPatrolCount++;

            if (currentPatrolCount >= maxPatrolCount)
            {
                currentPatrolCount = 0;
                //phase = PatrolPhase.Idle;
                // 상태 전환 필요 시 여기에 추가
                if (Vector2.Distance(patrolEnemy.transform.position, player.transform.position) < AttackDistance)
                {
                    // 플레이어가 공격 범위 내에 있을 때
                    phase = PatrolPhase.ChooseDirection;
                    stateMachine.ChangeState(GetComponent<PatrolEnemyAttackState>());
                }
                else
                {
                    // 대기 상태로 전환
                    phase = PatrolPhase.Waiting;
                }
            }
            else
            {
                phase = PatrolPhase.ChooseDirection;
            }
        }
    }

    private void Waiting()
    {
        // 대기 시간 동안 대기
        waitTime -= Time.deltaTime;
        if (waitTime <= 0f)
        {
            waitTime = 1f; // 초기화
            phase = PatrolPhase.ChooseDirection; // 다시 방향 선택 단계로 전환
        }

        // 플레이어가 공격 범위 내에 있을 때 공격 상태로 전환
        if (Vector2.Distance(patrolEnemy.transform.position, player.transform.position) < AttackDistance)
        {
            waitTime = 1f; // 대기 시간 초기화
            phase = PatrolPhase.ChooseDirection; // 다시 방향 선택 단계로 전환
            stateMachine.ChangeState(GetComponent<PatrolEnemyAttackState>());
        }
    }
    private void TryAddDirection(Vector2 dir, List<Vector2> list)
    {
        RaycastHit2D hit = Physics2D.Raycast(patrolEnemy.transform.position, dir, patrolDistance, LayerMask.GetMask("Wall"));
        if (!hit.collider)
        {
            list.Add(dir);
        }
    }
}
