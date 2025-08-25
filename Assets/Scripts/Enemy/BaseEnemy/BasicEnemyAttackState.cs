using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicEnemyAttackState : EnemyState
{
    private BasicEnemy basicEnemy; // Reference to the BasicEnemy script
    private PlayerStat player; // Reference to the PlayerStat script
    [SerializeField] private GameObject bulletPrefab; // Reference to the bullet prefab
    [SerializeField] private float attackDistance = 10f; // Distance at which the enemy can attack

    private Vector2 attackDirection; // Direction of the attack
    private float attackCooldown = 1f; // Cooldown time between attacks
    private float lastAttackTime;
    private bool isAttacked = false; // Flag to check if the enemy is currently attacking


    public override void Enter()
    {
        basicEnemy = GetComponent<BasicEnemy>();
        player = FindObjectOfType<PlayerStat>();
        lastAttackTime = Time.time;
        isAttacked = false; // Set attacking flag to false when entering attack state
        attackCooldown = Random.Range(0.8f, 1.2f); // Randomize attack cooldown slightly
    }

    public override void Execute()
    {
        if (player != null)
        {        
            if (!isAttacked)
            {
                attackDirection = (player.transform.position - basicEnemy.transform.position).normalized;
                isAttacked = true; // Set attacking flag to true
                lastAttackTime = Time.time; // Update the last attack time
                Attack();
            }
            else if (isAttacked && Time.time - lastAttackTime <= attackCooldown)
            {
                float angle = Mathf.Atan2(attackDirection.y, attackDirection.x) * Mathf.Rad2Deg - 90f; // Adjust for sprite orientation
                float currentAngle = basicEnemy.transform.rotation.eulerAngles.z;
                float newAngle = Mathf.LerpAngle(currentAngle, angle, Time.deltaTime * 5f); // Smoothly rotate towards the target angle
                basicEnemy.transform.rotation = Quaternion.Euler(new Vector3(0, 0, newAngle));
            }
            else if (attackDistance < Vector2.Distance(basicEnemy.transform.position, player.transform.position))
                stateMachine.ChangeState(GetComponent<BasicEnemyIdleState>()); // Return to move state after attacking
            else
                stateMachine.ChangeState(GetComponent<BasicEnemyMoveState>()); // Return to move state if player is out of attack range

        }
    }
    public override void Exit()
    {
        // Reset any attack-related parameters if needed
    }

    private void Attack()
    {
        GameObject bullet = Instantiate(bulletPrefab, basicEnemy.transform.position, Quaternion.identity);
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        if (bulletScript != null)
        {
            bulletScript.Init(10f, 7.5f, 10f, BulletType.Normal, false, 0f, 0f, attackDirection, LayerMask.GetMask("Player")); // Initialize the bullet with appropriate parameters
        }
        // Implement attack logic here, e.g., dealing damage to the player
    }
}
