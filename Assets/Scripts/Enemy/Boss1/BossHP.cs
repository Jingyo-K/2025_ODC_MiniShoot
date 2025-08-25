using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BossHP : EnemyHP
{
    public float Phase1Health; // Phase 1 체력
    public float Phase2Health; // Phase 2 체력
    public float Phase3Health; // Phase 3 체력
    public Image healthBarImage;
    private float currentPhase = 1; // 현재 페이즈

    // Start is called before the first frame update

    public override void TakeDamage(float amount)
    {
        base.TakeDamage(amount);
        if (currentHealth <= Phase2Health && currentPhase == 1)
        {
            currentPhase = 2;
            // Phase 2로 전환
            GetComponent<Boss1>().GoToPhase2();
            Debug.Log("Phase 2 activated");
        }
        if (currentHealth <= Phase3Health && currentPhase == 2)
        {
            currentPhase = 3;
            GetComponent<Boss1>().GoToPhase3();
            Debug.Log("Phase 3 activated");
        }
        healthBarImage.fillAmount = currentHealth / maxHealth;
    }

}
