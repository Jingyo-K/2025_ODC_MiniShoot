using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyHP : MonoBehaviour, IHittable
{
    [SerializeField] protected float maxHealth = 100f; // 최대 체력
    [SerializeField] protected float currentHealth;
    protected Color originalColor; // 원래 색상
    protected Color RedColor = Color.red; // 피해를 입었을 때의 색상
    protected SpriteRenderer spriteRenderer; // 스프라이트 렌더러 참조

    void Start()
    {
        currentHealth = maxHealth; // 초기 체력 설정
        spriteRenderer = GetComponent<SpriteRenderer>(); // 스프라이트 렌더러 참조 가져오기
        originalColor = spriteRenderer.color; // 원래 색상 저장
    }

    public virtual void TakeDamage(float amount)
    {
        currentHealth -= amount;
        spriteRenderer.color = Color.Lerp(originalColor, RedColor, 1f - (currentHealth / maxHealth)); // 피해를 입었을 때 색상 변경
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    public void SetStageScale(int stage)
    {
        // 스테이지에 따라 적의 체력 조정
        maxHealth *= (float)(stage + 1) / 2;
        currentHealth = maxHealth; // 체력 초기화
    }

    public void Die()
    {
        // 적 사망 처리 로직 (예: 애니메이션 재생, 오브젝트 파괴 등)
        GameObject scrapObject = Resources.Load<GameObject>("Scrap"); // 스크랩 오브젝트 프리팹 로드
        for (int i = 0; i < GetComponent<Enemy>().DropScrap; i++)
        {
            GameObject scrap = Instantiate(scrapObject, transform.position, Quaternion.identity); // 스크랩 오브젝트 생성
            scrap.GetComponent<ScrapObject>().scrapAmount = 1; // 스크랩 양 설정
            scrap.GetComponent<Rigidbody2D>().velocity = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(-1f, 1f)).normalized * 5f; // 스크랩이 랜덤한 방향으로 튕겨나가도록 설정
        }
        Destroy(gameObject);
    }
}
