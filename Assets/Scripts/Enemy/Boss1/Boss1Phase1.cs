using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 1. 조준탄 3회 발사 후 방사탄 2회 발사
/// 2. 5개영역 경고후 포격
/// 3. 전방향탄 발사
/// </summary>
public class Boss1Phase1 : EnemyState
{
    private Boss1 boss1;
    private PlayerStat player;
    public GameObject aimingBulletPrefab;
    public GameObject spreadBulletPrefab;
    public GameObject shellingPrefab;
    public float waitTime = 1f;

    private enum Phase1Patterns { Aiming, Spread, Shelling }
    private Phase1Patterns pattern = Phase1Patterns.Aiming;
    private Vector3 attackPos;
    private Vector2 targetPosition;
    public float RotateSpeed = 5f;

    [Header("Bullet Settings")]
    public float bulletDamage = 10f;
    public float bulletSpeed = 10f;
    public float bulletRange = 2f;


    public override void Enter()
    {
        boss1 = GetComponent<Boss1>();
        player = FindObjectOfType<PlayerStat>();
        SetStartTime();
    }
    public override void Execute()
    {
        switch (pattern)
        {
            case Phase1Patterns.Aiming:
                AimingBullet();
                break;
            case Phase1Patterns.Spread:
                SpreadBullet();
                break;
            case Phase1Patterns.Shelling:
                Shelling();
                break;
        }
    }
    public override void Exit()
    {
        base.Exit();
    }

    /// <summary>
    /// 플레이어 조준 후 탄환 발사
    /// </summary>
    
    private float startTime;
    private bool isAimingComplete = false;
    private void AimingBullet()
    {
        if (!isAimingComplete)
        {
            targetPosition = player.transform.position;
            boss1.transform.rotation = Quaternion.RotateTowards(
                boss1.transform.rotation,
                Quaternion.LookRotation(Vector3.forward, targetPosition - (Vector2)boss1.transform.position),
                RotateSpeed * Time.deltaTime
            );

            if (Time.time - startTime > waitTime)
            {
                StartCoroutine(FireAimingBullet());
                isAimingComplete = true;
            }
        }
        
    }

    private IEnumerator FireAimingBullet()
    {
        attackPos = boss1.transform.GetChild(0).position;
        for (int i = 0; i < 3; i++)
        {
            GameObject bullet = Instantiate(aimingBulletPrefab, attackPos, Quaternion.identity);
            bullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, targetPosition - (Vector2)attackPos);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            bulletComponent.Init(
                bulletDamage,
                bulletSpeed,
                bulletRange,
                BulletType.Normal,
                false, // No homing for aiming bullets
                0f, // No homing range
                0f, // No homing turn speed
                (Vector2)bullet.transform.up, // Move direction based on rotation
                LayerMask.GetMask("Player") // Target layer for player
            );
            yield return new WaitForSeconds(0.3f); // Wait before firing the next bullet
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < 10; j++)
            {
                GameObject bullet = Instantiate(spreadBulletPrefab, attackPos, Quaternion.identity);
                bullet.transform.rotation = Quaternion.Euler(0, 0, boss1.transform.rotation.eulerAngles.z + (j - 5) * 20f);
                Bullet bulletComponent = bullet.GetComponent<Bullet>();
                bulletComponent.Init(
                    bulletDamage,
                    bulletSpeed,
                    bulletRange,
                    BulletType.Normal,
                    false, // No homing for spread bullets
                    0f, // No homing range
                    0f, // No homing turn speed
                    (Vector2)bullet.transform.up, // Move direction based on rotation
                    LayerMask.GetMask("Player") // Target layer for player
                );
            }
            yield return new WaitForSeconds(0.25f);
        }
        Debug.Log("Aiming bullets fired, now switching to next pattern.");
        StartCoroutine(ChangeNextPattern());
    }

    private void SpreadBullet()
    {
        if(!isAimingComplete)
        {
            StartCoroutine(SpreadBulletCoroutine());
            isAimingComplete = true;
        }
    }
    
    private IEnumerator SpreadBulletCoroutine()
    {
        attackPos = boss1.transform.GetChild(0).position;
        for (int i = 0; i < 108; i++)
        {
            GameObject bullet = Instantiate(spreadBulletPrefab, boss1.transform.position, Quaternion.identity);
            bullet.transform.rotation = Quaternion.Euler(0, 0, boss1.transform.rotation.eulerAngles.z + i * 25f);
            Bullet bulletComponent = bullet.GetComponent<Bullet>();
            bulletComponent.Init(
                bulletDamage,
                bulletSpeed, // Increased speed for spread bullets
                bulletRange,
                BulletType.Normal,
                false, // No homing for spread bullets
                0f, // No homing range
                0f, // No homing turn speed
                (Vector2)bullet.transform.up, // Move direction based on rotation
                LayerMask.GetMask("Player") // Target layer for player
            );
            yield return new WaitForSeconds(0.01f); // Short delay between bullets
        }
        Debug.Log("Spread bullets fired, now switching to next pattern.");
        StartCoroutine(ChangeNextPattern());
    }

    private void Shelling()
    {
        // Implement the shelling behavior here
        if (!isAimingComplete)
        {
            StartCoroutine(ShellingCoroutine());
            isAimingComplete = true;
        }
    }

    private IEnumerator ShellingCoroutine()
    {
        Vector2[] warningPositions = new Vector2[5];
        for (int i = 0; i < warningPositions.Length; i++)
        {
            warningPositions[i] = new Vector2(player.transform.position.x + Random.Range(-10f, 10f), player.transform.position.y + Random.Range(-10f, 10f));
            GameObject warning = Instantiate(shellingPrefab, warningPositions[i], Quaternion.identity);
            Warning warningComponent = warning.GetComponent<Warning>();
            warningComponent.SetWarning();
        }
        yield return new WaitForSeconds(1f); // Wait for the warnings to be set
        foreach (var pos in warningPositions)
        {
            for (int j = 0; j < 12; j++)
            {
                GameObject bullet = Instantiate(spreadBulletPrefab, pos, Quaternion.identity);
                bullet.transform.rotation = Quaternion.Euler(0, 0, j * 30f); // Random rotation for each bullet
                Bullet bulletComponent = bullet.GetComponent<Bullet>();
                bulletComponent.Init(
                    bulletDamage,
                    bulletSpeed,
                bulletRange,
                BulletType.Normal,
                false,
                0f,
                0f,
                (Vector2)bullet.transform.up,
                LayerMask.GetMask("Player")
                );
            }
        }
        Debug.Log("Shelling completed, now switching to next pattern.");
        StartCoroutine(ChangeNextPattern());
    }

    private void SetStartTime()
    {
        startTime = Time.time;
    }
    private IEnumerator ChangeNextPattern()
    {
        yield return new WaitForSeconds(waitTime); // Wait for 1 second before changing pattern
        isAimingComplete = false; // Reset aiming completion status
        pattern++;
        if (pattern > Phase1Patterns.Shelling)
        {
            pattern = Phase1Patterns.Aiming;
        }
        Debug.Log($"Changing pattern to: {pattern}");
        SetStartTime(); // Reset start time for the next pattern
    }
}
