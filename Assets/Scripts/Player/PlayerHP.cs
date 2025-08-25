using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour, IHittable
{
    [SerializeField] private float maxHp; // 최대 체력
    [SerializeField] private float currentHealth;
    public Image healthBarImage;

    void OnEnable()
    {
        GetComponent<PlayerStat>().OnStatChanged += UpdateCurrentHealth;
    }

    void OnDisable()
    {
        GetComponent<PlayerStat>().OnStatChanged -= UpdateCurrentHealth;
    }

    void Start()
    {
        // PlayerStat에서 Stat 덧셈 후 적용
        var updatedStat = GetComponent<PlayerStat>().stat;
        maxHp = updatedStat.MaxHp;
        currentHealth = maxHp;
    }


    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        healthBarImage.fillAmount = currentHealth / maxHp;
        if (currentHealth <= 0)
        {
            Die();
        }
        OnHit?.Invoke();
    }

    public float GetCurrentHP()
    {
        return currentHealth;
    }

    private void UpdateCurrentHealth(PlayerStat playerStat)
    {
        // PlayerStat의 MaxHp 변경 시 현재 체력 업데이트
        maxHp = playerStat.stat.MaxHp;
        currentHealth = playerStat.stat.CurrentHp;
        healthBarImage.fillAmount = currentHealth / maxHp;
    }

    private void Die()
    {
        // 플레이어 사망 처리 로직 (예: 애니메이션 재생, 게임 오버 등)
        Debug.Log("Player has died");
        GameSceneController.Instance.GameOver();
        SceneManager.LoadScene("TitleScene");
    }
    public Action OnHit { get; set; }
}
