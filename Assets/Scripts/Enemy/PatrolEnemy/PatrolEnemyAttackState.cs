using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1. 조준시간동안 플레이어를 향해 회전
/// 2. 조준이 끝나면 플레이어를 향해 발사
/// 3. 발사한 후에는 다시 순찰 상태로 전환
/// </summary>
public class PatrolEnemyAttackState : EnemyState
{
    private PatrolEnemy patrolEnemy;
    private PlayerStat player;

    [SerializeField] private float moveSpeed = 1f;
    [SerializeField] private float rotationSpeed = 360f;
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float aimingTime = 1f;
    private Vector2 attackDirection;
    private Quaternion targetRotation;
    public override void Enter()
    {
        patrolEnemy = GetComponent<PatrolEnemy>();
        player = FindObjectOfType<PlayerStat>();
    }

    public override void Execute()
    {
        targetRotation = Quaternion.LookRotation(Vector3.forward, player.transform.position - patrolEnemy.transform.position);
        // Aim at the player
        patrolEnemy.transform.rotation = Quaternion.RotateTowards(
            patrolEnemy.transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );

        // After aiming time, shoot the bullet
        if (aimingTime <= 0f)
        {
            attackDirection = (player.transform.position - patrolEnemy.transform.position).normalized;
            ShootProjectile();
            stateMachine.ChangeState(GetComponent<PatrolEnemyPatrolState>());
        }
        else
        {
            aimingTime -= Time.deltaTime;
        }
    }

    //상태를 탈출할때 0,90,180,270중 한방향을 바라보도록 회전 정렬
    public override void Exit()
    {
        // Reset the aiming time
        aimingTime = 1f;

        // 0,90,180,270 중 가장 가까운 방향으로 회전속도에 따라 회전
        float angle = Vector2.SignedAngle(Vector2.up, player.transform.position - patrolEnemy.transform.position);
        float targetAngle = Mathf.Round(angle / 90f) * 90f;
        float angleDifference = Mathf.DeltaAngle(patrolEnemy.transform.eulerAngles.z, targetAngle);
        while (Mathf.Abs(angleDifference) > 0.1f)
        {
            targetRotation = Quaternion.Euler(0, 0, targetAngle);
            angleDifference = Mathf.DeltaAngle(patrolEnemy.transform.eulerAngles.z, targetAngle);
            patrolEnemy.transform.rotation = Quaternion.RotateTowards(
                patrolEnemy.transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void ShootProjectile()
    {
        GameObject bullet = Instantiate(bulletPrefab, patrolEnemy.transform.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(10f, 7.5f, 10f, BulletType.Normal, false, 0f, 0f, attackDirection, LayerMask.GetMask("Player")); // Initialize the bullet with appropriate parameters
        }
        // Implement attack logic here, e.g., dealing damage to the player
    }
}
