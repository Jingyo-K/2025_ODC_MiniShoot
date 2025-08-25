using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnBasicEnemyMoveState : EnemyState
{
    private EnBasicEnemy basicEnemy; // Reference to the BasicEnemy script
    private PlayerStat player; // Reference to the PlayerStat script
    [SerializeField] private float moveSpeed = 1f; // Speed of the enemy movement

    Vector2 targetPosition; // Target position for the enemy to move towards

    private float lastAttackTime; // Time of the last attack
    private float attackAiming = 1f; // Aim time for the attack
    private Vector2 attackDirection; // Direction of the attack


    public override void Enter()
    {
        basicEnemy = GetComponent<EnBasicEnemy>();
        player = FindObjectOfType<PlayerStat>();
        lastAttackTime = Time.time;
        targetPosition = player.transform.position + new Vector3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), 0f); // Set the target position to the player's position
    }

    public override void Execute()
    {
        if (Time.time - lastAttackTime < attackAiming)
        {
            // Aim towards the player
            attackDirection = (player.transform.position - basicEnemy.transform.position).normalized;

            // 플레이어 주변을 회전하는 로직
            
            basicEnemy.transform.position = Vector2.MoveTowards(basicEnemy.transform.position, targetPosition, moveSpeed * Time.deltaTime);

            // --회전처리--
            float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg - 90f; // Adjust for sprite orientation
            float currentAngle = basicEnemy.transform.rotation.eulerAngles.z;
            float newAngle = Mathf.LerpAngle(currentAngle, angle, Time.deltaTime * 5f); // Smoothly rotate towards the target angle
            basicEnemy.transform.rotation = Quaternion.Euler(new Vector3(0, 0, newAngle));
        }
        else
        {
            stateMachine.ChangeState(GetComponent<EnBasicEnemyAttackState>()); // Return to idle state after attacking
        }
    }

    public override void Exit()
    {
        // Reset any attack-related parameters if needed
    }
}

