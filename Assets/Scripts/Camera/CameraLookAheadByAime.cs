using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

public class CameraLookAheadByAime : CinemachineExtension
{
    public float lookAheadDistance = 2.0f;
    public float saveLookAheadDistance = 0.0f;
    public Vector2 lookDirection = Vector2.right;
    private Coroutine distanceCoroutine;
    protected override void OnEnable()
    {
        base.OnEnable();
        saveLookAheadDistance = lookAheadDistance; // Save the initial look ahead distance
        PlayerStateController.OnAttackStateChanged += OnAttackStateChanged;
    }
    void OnDisable()
    {
        PlayerStateController.OnAttackStateChanged -= OnAttackStateChanged;
    }

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage,
        ref CameraState state, float deltaTime)
    {
        if (stage == CinemachineCore.Stage.Body)
        {
            {
                Vector3 offset = (Vector3)lookDirection * lookAheadDistance;
                state.PositionCorrection += offset;
            }
        }
    }

    private void OnAttackStateChanged(AttackState newState)
    {
        if (newState == AttackState.Fire)
        {
            if (distanceCoroutine != null)
            {
                StopCoroutine(distanceCoroutine);
            }
            distanceCoroutine = StartCoroutine(LerpLookAheadDistance(saveLookAheadDistance * 1.4f));
        }
        else if (newState == AttackState.Idle)
        {
            if (distanceCoroutine != null)
            {
                StopCoroutine(distanceCoroutine);
            }
            distanceCoroutine = StartCoroutine(LerpLookAheadDistance(saveLookAheadDistance));
        }
        else
        {
            Debug.LogWarning("Unhandled attack state: " + newState);
        }
    }
    private IEnumerator LerpLookAheadDistance(float targetDistance)
    {
        float startDistance = lookAheadDistance;
        float elapsedTime = 0.0f;
        float duration = 0.5f; // Duration of the lerp

        while (elapsedTime < duration)
        {
            lookAheadDistance = Mathf.Lerp(startDistance, targetDistance, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        lookAheadDistance = targetDistance; // Ensure we end exactly at the target distance
    }
}
