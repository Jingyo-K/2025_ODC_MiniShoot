using System;
using UnityEngine;
public abstract class EnemyState : MonoBehaviour
{
    protected Enemy enemy;
    protected EnemyStateMachine stateMachine;
    void Awake()
    {
        enemy = GetComponent<Enemy>();
        stateMachine = GetComponent<EnemyStateMachine>();
    }

    public virtual void Enter() { }
    public virtual void Execute() { }
    public virtual void Exit() { }  
}
