using UnityEngine;

/// <summary>
/// 트레일 연출 (TrailRenderer, Particle 등)에 대응. 
/// 각종 커스텀 효과 확장 가능.
/// </summary>
[RequireComponent(typeof(TrailRenderer))]
public class PlayerTrailEffect : MonoBehaviour
{
    private TrailRenderer trail;

    void Awake()
    {
        trail = GetComponent<TrailRenderer>();
    }

    // 원하는 효과 추가
    public void SetTrailColor(Color color)
    {
        if (trail != null)
        {
            trail.startColor = color;
            trail.endColor = color * 0.5f; // 예시: 점점 투명하게
        }
    }

    public void SetTrailLength(float length)
    {
        if (trail != null)
            trail.time = length; // 트레일 지속시간(길이)
    }

    // 등등 다양한 파라미터 대응!
}
