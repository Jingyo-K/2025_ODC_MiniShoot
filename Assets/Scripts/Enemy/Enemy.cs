using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour, IHittable
{
    public EnemyStateMachine stateMachine { get; private set; }
    public int DropScrap = 3; // 스크랩 드랍 수량
    public int CollisonDamage = 10; // 적이 플레이어와 충돌 시 입히는 피해량
    [SerializeField] private EnemyHP enemyHP; // Reference to EnemyHP script

    protected virtual void Start()
    {
        stateMachine = GetComponent<EnemyStateMachine>();
        enemyHP = GetComponent<EnemyHP>();
    }
    void Update()
    {
        stateMachine.Update();
    }
    public void TakeDamage(float amount)
    {
        enemyHP.TakeDamage(amount);
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHP player = collision.gameObject.GetComponent<PlayerHP>();
            if (player != null)
            {
                player.TakeDamage(CollisonDamage); // 플레이어에게 충돌 피해를 입힘
                GetComponent<EnemyHP>().Die();
            }
        }
    }

    public void Sleep()
    {
        stateMachine.ChangeState(GetComponent<EnemySleepState>());
    }
    public virtual void WakeUp()
    {
        
    }
    
}
