using UnityEngine;
using System;

public enum FireType { Normal, Charging }
public enum BulletType { Normal, Laser, Pierce, Homing }

public class Weapon : MonoBehaviour
{
    public WeaponData data;
    private PlayerStat playerStat;

    public void Init(PlayerStat _stat, WeaponData _data)
    {
        playerStat = _stat;
        data = _data;
    }

    public void Fire(Vector2 dir)
    {
        if (data == null || playerStat.stat == null) return;

        // 무기/플레이어 스탯 합산 수치
        float realAtk = playerStat.stat.Atk * data.atkMultiplier;
        float realBulletSpeed = playerStat.stat.BulletSpeed * data.bulletSpeedMultiplier;
        float realRange = playerStat.stat.Range * data.rangeMultiplier;

        // 발사체 생성
        Bullet bullet = Instantiate(data.bulletPrefab, transform.position, Quaternion.identity);
        bullet.Init(realAtk, realBulletSpeed, realRange, data.bulletType, data.isHoming, data.homingRange, data.homingTurnSpeed, dir, LayerMask.GetMask("Enemy"));


    }
}

