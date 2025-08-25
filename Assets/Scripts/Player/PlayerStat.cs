using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stat
{
    public float MaxHp;
    public float CurrentHp;
    public float Atk;
    public float FireInterval;
    public float BulletSpeed;
    public float Range;
    public float MoveSpeed;
    public float CritChance;

    // 덧셈 연산자 오버로딩
    public static Stat operator +(Stat stat1, Stat stat2)
    {
        return new Stat
        {
            MaxHp = stat1.MaxHp + stat2.MaxHp,
            CurrentHp = stat1.CurrentHp + stat2.CurrentHp,
            Atk = stat1.Atk + stat2.Atk,
            FireInterval = stat1.FireInterval + stat2.FireInterval,
            BulletSpeed = stat1.BulletSpeed + stat2.BulletSpeed,
            Range = stat1.Range + stat2.Range,
            MoveSpeed = stat1.MoveSpeed + stat2.MoveSpeed,
            CritChance = stat1.CritChance + stat2.CritChance
        };
    }
}
public class PlayerStat : MonoBehaviour
{
    public float sMaxHp = 100f;
    public float sCurrentHp = 100f;
    public float sAtk = 10f;
    public float sFireInterval = 3f;
    public float sBulletSpeed = 10f;
    public float sRange = 10f;
    public float sMoveSpeed = 5f;
    public float sCritChance = 0.1f;
    public Stat stat;

    private void Awake()
    {
        stat = new Stat
        {
            MaxHp = sMaxHp,
            CurrentHp = sCurrentHp,
            Atk = sAtk,
            FireInterval = sFireInterval,
            BulletSpeed = sBulletSpeed,
            Range = sRange,
            MoveSpeed = sMoveSpeed,
            CritChance = sCritChance
        };
    }

    // 아이템/버프 적용 예시
    public virtual void AddItem(Item item)
    {
        // 아이템의 Stat을 현재 PlayerStat에 덧셈
        stat.CurrentHp = GetComponent<PlayerHP>().GetCurrentHP(); // 현재 체력 업데이트
        stat = stat + item.SetStat();

        OnStatChanged?.Invoke(this);
        Debug.Log("PlayerStat updated: " + stat.MaxHp + ", " + stat.Atk + ", " + (float)stat.FireInterval
            + ", " + stat.BulletSpeed + ", " + stat.Range + ", " + stat.MoveSpeed + ", " + stat.CritChance);
    }

    public event Action<PlayerStat> OnStatChanged;
}