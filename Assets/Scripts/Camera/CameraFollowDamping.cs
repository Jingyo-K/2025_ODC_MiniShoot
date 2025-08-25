using UnityEngine;

public class CameraFollowDamping : MonoBehaviour
{
    [SerializeField] private Transform target;          // 플레이어 Transform
    [SerializeField] private float followSpeed = 10f;   // 카메라 댐핑 속도
    [SerializeField] private float lookAheadDistance = 2.5f; // 카메라가 앞서갈 거리

    private Vector3 offset = Vector3.zero;
    private Vector2 lookDirection = Vector2.right; // Player에서 전달받기

    void LateUpdate()
    {
        if (target == null) return;
        if (target.GetComponent<PlayerStateController>().CurrentState == MoveState.Move)
            followSpeed = target.GetComponent<PlayerMove>().getSpeed(); // 플레이어 속도에 따라 카메라 속도 조정

        // 플레이어 바라보는 방향(lookDirection)은 PlayerMove/PlayerFire 등에서 계속 전달!
        Vector3 desiredPosition = target.position + (Vector3)lookDirection.normalized * lookAheadDistance;

        // 부드럽게 따라가기
        transform.position = Vector3.Lerp(transform.position, desiredPosition.WithZ(-10f), followSpeed * Time.deltaTime);
    }

    // 외부에서 조준/이동 방향 실시간 업데이트
    public void SetLookDirection(Vector2 dir)
    {
        if (dir.sqrMagnitude > 0.01f)
            lookDirection = dir.normalized;
    }
}

// --- Vector3의 z값 고정 유틸 ---
public static class Vector3Extensions
{
    public static Vector3 WithZ(this Vector3 v, float z) => new Vector3(v.x, v.y, z);
}