using System.Collections;
using UnityEngine;



/// <summary>
/// 플레이어의 발사 관련 기능을 처리하는 클래스입니다.
/// 발사 방향은 PlayerModel의 방향에 따른 채 발사만을 담당합니다.
/// </summary>
[RequireComponent(typeof(PlayerStat))]
public class PlayerFire : MonoBehaviour
{
    [Header("Weapon")]
    public WeaponData currentWeaponData;
    [SerializeField] private Transform muzzle;

    [Header("Refs")]
    [SerializeField] private PlayerInputHandler inputHandler;
    private PlayerStat playerStat;
    private Weapon runtimeWeapon;
    private Coroutine fireCo;

    private void Awake()
    {
        playerStat = GetComponent<PlayerStat>();
        runtimeWeapon = gameObject.AddComponent<Weapon>();
    }

    private void OnEnable()
    {
        runtimeWeapon.Init(playerStat, currentWeaponData);
        if (inputHandler == null) inputHandler = FindObjectOfType<PlayerInputHandler>();
        if (inputHandler)
        {
            inputHandler.OnFirePressed  += StartFire;
            inputHandler.OnFireReleased += StopFire;
        }

        playerStat.OnStatChanged += OnStatChanged;
    }

    private void OnDisable()
    {
        if (inputHandler)
        {
            inputHandler.OnFirePressed  -= StartFire;
            inputHandler.OnFireReleased -= StopFire;
        }

        playerStat.OnStatChanged -= OnStatChanged;
        StopFire();
    }

    private void OnStatChanged(PlayerStat s)
    {
        runtimeWeapon.Init(playerStat, currentWeaponData);
    }

    private void StartFire(Vector2 _)
    {
        if (!muzzle) { Debug.LogWarning("[PlayerFire] muzzle 참조가 필요합니다."); return; }
        if (fireCo == null) fireCo = StartCoroutine(FireLoop());
    }

    private void StopFire()
    {
        if (fireCo != null) { StopCoroutine(fireCo); fireCo = null; }
    }

    private IEnumerator FireLoop()
    {
        if (playerStat == null || currentWeaponData == null) yield break;

        float rate = playerStat.stat.FireInterval * currentWeaponData.fireRateMultiplier;
        if (rate <= 0f) rate = 1f;
        float interval = 1f / rate;

        while (true)
        {
            Vector2 dir = muzzle.up;            // 방향은 모델 기준
            runtimeWeapon.Fire(dir);
            yield return new WaitForSeconds(interval);
        }
    }

    public void EquipWeapon(WeaponData newData)
    {
        currentWeaponData = newData;
        runtimeWeapon.Init(playerStat, currentWeaponData);
    }
}
