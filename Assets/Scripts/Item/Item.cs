using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Item/ItemData")]
public class Item : ScriptableObject
{
    public string itemName;
    public Sprite itemIcon;
    public int itemPrice;
    public string itemDescription;

    public float MaxHp;
    public float CurrentHp;
    public float Atk;
    public float FireInterval;
    public float BulletSpeed;
    public float Range;
    public float MoveSpeed;
    public float CritChance;

    public Stat itemStat;
    public Stat SetStat()
    {
        itemStat = new Stat
        {
            MaxHp = MaxHp,
            CurrentHp = CurrentHp,
            Atk = Atk,
            FireInterval = FireInterval,
            BulletSpeed = BulletSpeed,
            Range = Range,
            MoveSpeed = MoveSpeed,
            CritChance = CritChance
        };
        return itemStat;
    }

}

