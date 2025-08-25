using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/WeaponData")]
public class WeaponData : ScriptableObject
{
    public float atkMultiplier = 1f;
    public float fireRateMultiplier = 1f;
    public float rangeMultiplier = 1f;
    public float bulletSpeedMultiplier = 1f;
    public FireType fireType;
    public BulletType bulletType;
    public bool isHoming;
    public float homingRange;
    public float homingTurnSpeed;
    public Bullet bulletPrefab;

    public Sprite weaponIcon; // 무기 아이콘
    public string weaponName; // 무기 이름
    public int weaponPrice; // 무기 가격
    public string weaponDescription; // 무기 설명
}

