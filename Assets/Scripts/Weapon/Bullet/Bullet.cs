using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    protected float atk, speed, range;
    protected BulletType type;
    protected bool isHoming;
    protected float homingRange, homingTurnSpeed;
    protected Vector2 moveDir;
    protected float distanceTraveled = 0f;
    LayerMask targetLayer;

    public void Init(float _atk, float _speed, float _range, BulletType _type,
                     bool _homing, float _homingRange, float _homingTurnSpeed, Vector2 _dir, LayerMask _targetLayer)
    {
        atk = _atk;
        speed = _speed;
        range = _range;
        type = _type;
        isHoming = _homing;
        homingRange = _homingRange;
        homingTurnSpeed = _homingTurnSpeed;
        moveDir = _dir.normalized;
        targetLayer = _targetLayer;
    }

    protected virtual void Update()
    {
        // --- 이동 ---
        Vector2 prevPos = transform.position;
        if (isHoming)
        {
            // 타겟 추적 및 방향 전환 로직(가장 가까운 적 찾아서 보정 등)
        }
        transform.position += (Vector3)(moveDir * speed * Time.deltaTime);
        // --- 회전 ---
        if (moveDir.sqrMagnitude > 0.01f)
        {
            float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
            angle -= 90f; // Adjust for sprite orientation
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else
        {
            // 방향이 없을 때는 회전하지 않음
            transform.rotation = Quaternion.identity;
        }

        // --- 거리 체크 ---
            distanceTraveled += Vector2.Distance(prevPos, transform.position);
        if (distanceTraveled > range)
            Destroy(gameObject);
    }

    // --- 충돌/피격 처리 ---
    void OnTriggerEnter2D(Collider2D col)
    {
        // 적 또는 벽에 맞으면 데미지, 파괴 등 처리
        IHittable hittable = col.GetComponent<IHittable>();
        if (hittable != null)
        {
            if ((targetLayer & (1 << col.gameObject.layer)) != 0)
            {
                hittable.TakeDamage(atk);
                Destroy(gameObject); // Hit enemy, destroy bullet
            }
        }
            else if (col.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
    }
}

