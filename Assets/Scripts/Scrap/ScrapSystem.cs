using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ScrapSystem은 플레이어가 획득한 스크랩을 관리하는 시스템입니다.
/// 이 시스템은 스크랩의 수집, 사용, 그리고 상점에서의 거래 등을 처리합니다.
/// </summary>
public class ScrapSystem : MonoBehaviour
{
    public int scrapCount { get; private set; } = 0; // 현재 플레이어가 보유한 스크랩의 수
    public int ScrapMagnetRange = 5; // 스크랩을 자동으로 수집할 범위
    public TextMeshProUGUI scrapCountText; // UI에 표시할 스크랩 수 텍스트

    /// <summary>
    /// 스크랩을 획득합니다.
    /// </summary>
    /// <param name="amount">획득할 스크랩의 양</param>
    public void CollectScrap(int amount)
    {
        scrapCount += amount;
        UpdateScrapCountUI(); // UI 업데이트
    }

    /// <summary>
    /// 스크랩을 사용합니다.
    /// </summary>
    /// <param name="amount">사용할 스크랩의 양</param>
    public void UseScrap(int amount)
    {
        if (scrapCount >= amount)
        {
            scrapCount -= amount;
        }
        else
        {
            Debug.LogWarning("스크랩이 부족합니다.");
        }
        UpdateScrapCountUI(); // UI 업데이트
    }

    void Update()
    {
        {
            Collider2D[] scrapObjects = Physics2D.OverlapCircleAll(GameSceneController.Instance.playerInstance.transform.position, ScrapMagnetRange);
            foreach (var scrap in scrapObjects)
            {
                ScrapObject scrapObject = scrap.GetComponent<ScrapObject>();
                if (scrapObject != null)
                {
                    scrapObject.SetGoToPlayer(true);
                }
            }
        }
    }
    private void UpdateScrapCountUI()
    {
        scrapCountText.text = $"{scrapCount}";
    }
}
