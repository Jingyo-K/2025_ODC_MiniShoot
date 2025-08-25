using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyStateMachine : MonoBehaviour
{
    [SerializeField] private EnemyState currentState;

    public void Initialize(EnemyState initialState)
    {
        currentState = initialState;
        currentState.Enter();
    }

    public void ChangeState(EnemyState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void Update()
    {
        currentState?.Execute();
    }
}
