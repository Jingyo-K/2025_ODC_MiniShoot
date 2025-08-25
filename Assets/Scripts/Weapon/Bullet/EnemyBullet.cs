using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : Bullet
{
    public GameObject lifeEffectPrefab;


    override protected void Update()
    {
        base.Update();
        // 추가적인 적 탄환 전용 로직
        //남은 거리에 따라서 0.1~0.9
        lifeEffectPrefab.transform.localScale = new Vector3(1, 1, 1) * Mathf.Lerp(0.1f, 0.9f, 1 - (distanceTraveled / range));
    }
}
