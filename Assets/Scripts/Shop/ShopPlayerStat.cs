using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopPlayerStat : MonoBehaviour
{
    public PlayerStat playerStat;
    public PlayerHP playerHP;
    private float currentHealth;
    private float maxHealth;
    public TextMeshProUGUI HPText;
    public TextMeshProUGUI AtkText;
    public TextMeshProUGUI FireIntervalText;
    public TextMeshProUGUI BulletSpeedText;
    public TextMeshProUGUI RangeText;
    public TextMeshProUGUI MoveSpeedText;
    void Awake()
    {
        playerStat = FindObjectOfType<PlayerStat>();
        playerHP = playerStat.GetComponent<PlayerHP>();
    }
    void OnEnable()
    {
        playerStat.OnStatChanged += UpdatePlayerStats;
        playerHP.OnHit += UpdatePlayerHP;
        UpdatePlayerStats(playerStat); // 초기화 시에도 현재 스탯을 업데이트
    }

    void OnDisable()
    {
        playerStat.OnStatChanged -= UpdatePlayerStats;
        playerHP.OnHit -= UpdatePlayerHP;
    }


    private void UpdatePlayerStats(PlayerStat playerStat)
    {
        currentHealth = playerStat.stat.CurrentHp;
        maxHealth = playerStat.stat.MaxHp;
        HPText.text = currentHealth + "/" + maxHealth;
        AtkText.text = playerStat.stat.Atk.ToString();
        FireIntervalText.text = playerStat.stat.FireInterval.ToString();
        BulletSpeedText.text = playerStat.stat.BulletSpeed.ToString();
        RangeText.text = playerStat.stat.Range.ToString();
        MoveSpeedText.text = playerStat.stat.MoveSpeed.ToString();
    }

    private void UpdatePlayerHP()
    {
        currentHealth = playerHP.GetCurrentHP();
        HPText.text = currentHealth + "/" + maxHealth;
    }
}
