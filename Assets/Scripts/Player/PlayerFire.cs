using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerFire : MonoBehaviour
{
    public WeaponData currentWeaponData; // 인스펙터에서 SO 할당

    private Weapon runtimeWeapon;
    private PlayerStat playerstat;
    private PlayerStateController stateController;
    private Coroutine fireCoroutine;
    private Vector2 fireDirection = Vector2.right;
    private bool isCooldown = false;
    private bool isFirstShot = true; // 첫 발사 여부
    private float lastFireTime = 0f;
    void OnEnable()
    {
        GetComponent<PlayerStat>().OnStatChanged += UpdateWeaponStats;

    }

    private void UpdateWeaponStats(PlayerStat stat)
    {
        // Update weapon stats based on player stats
        if (runtimeWeapon != null)
        {
            runtimeWeapon.Init(stat, currentWeaponData);
        }
    }

    private void OnDisable()
    {
        GetComponent<PlayerStat>().OnStatChanged -= UpdateWeaponStats;
    }

    void Start()
    {
        stateController = GetComponent<PlayerStateController>();
        playerstat = GetComponent<PlayerStat>();

        // 무기 프리팹이 아니라, 런타임 Weapon 컴포넌트 할당 (혹은 Instantiate로 생성)
        runtimeWeapon = gameObject.AddComponent<Weapon>();
        runtimeWeapon.Init(playerstat, currentWeaponData);
    }

    void Update()
    {
        if (stateController.CurrentAttackState == AttackState.Fire)
        {
            if (fireCoroutine == null)
            {
                fireCoroutine = StartCoroutine(FireCoroutine());
            }
        }
        else
        {
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
                isCooldown = false;
            }
        }
    }

    public void SetFireDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.01f)
            fireDirection = dir.normalized;
    }

    private IEnumerator FireCoroutine()
    {
        float fireInterval = 1 / (playerstat.stat.FireInterval * currentWeaponData.fireRateMultiplier);
        Debug.Log($"PlayerStat: " + playerstat.stat + $"Fire Interval: {fireInterval}");
        while (true)
        {
            if (!isCooldown)
            {
                // 발사 처리
                runtimeWeapon.Fire(fireDirection);

                // 발사 후 시간 기록

                // 쿨타임 활성화
                isCooldown = true;

                // 쿨타임 대기
                yield return new WaitForSeconds(fireInterval);

                // 쿨타임 해제
                isCooldown = false;
            }
            else
            {
                // 발사 간격 대기
                yield return null;
            }
        }
    }

    // 무기 교체 함수 (예: 아이템 획득, 인벤토리 스왑)
    public void EquipWeapon(WeaponData newWeaponData)
    {
        currentWeaponData = newWeaponData;
        runtimeWeapon.Init(playerstat, currentWeaponData);
    }
}

