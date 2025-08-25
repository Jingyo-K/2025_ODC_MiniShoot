using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class ShopItemPlacement : MonoBehaviour
{
    private PlayerInput playerInput;
    public GameObject NextButtonPrefab; // Prefab for the Next button
    public GameObject NextButtonHighLight;
    public GameObject shopBackground;   // 부모 컨테이너
    public GameObject itemSlotPrefab;   // 아이템 슬롯 프리팹
    public List<Item> items;            // 아이템 6개 사용 전제
    public List<WeaponData> weaponDatas; // 무기 데이터 리스트 (필요시 사용)

    private const int Columns = 3;      // 가로 3
    private const int Rows = 2;         // 세로 2
    private const int MaxItemCount = Columns * Rows; // 6
    private const int LastIndex = MaxItemCount - 1;  // 5
    private const int NextIndex = 6;

    private int currentIndex = 0;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        playerInput.onControlsChanged += OnControlsChanged;
    }

private void PlaceItems()
{
    if (GameSceneController.Instance.CurrentStage == 1)
    {
    return; // 스테이지 1에서는 아이템 배치 안함
    }
    // 기존 자식 제거
    for (int i = shopBackground.transform.childCount - 1; i >= 0; i--)
    {
        Destroy(shopBackground.transform.GetChild(i).gameObject);
    }

    // ===== items에서 중복 없는 5개 뽑고 삭제 =====
    List<Item> pickedItems = new List<Item>();

    for (int i = 0; i < 6 && items.Count > 0; i++)
    {
        int randIndex = Random.Range(0, items.Count);
        pickedItems.Add(items[randIndex]);
        items.RemoveAt(randIndex); // 뽑은 아이템은 원본에서 삭제
    }

    // ===== weaponDatas에서 1개 뽑고 삭제 =====
    /*WeaponData pickedWeapon = null;
    if (weaponDatas != null && weaponDatas.Count > 0)
    {
        int randIndex = Random.Range(0, weaponDatas.Count);
        pickedWeapon = weaponDatas[randIndex];
        weaponDatas.RemoveAt(randIndex); // 무기도 원본에서 삭제
    }*/

    // ===== 슬롯 생성 =====
    int count = 0;
    foreach (var item in pickedItems)
    {
        GameObject slot = Instantiate(itemSlotPrefab, shopBackground.transform, false);
        var shop = slot.GetComponent<ShopItem>();
        if (shop != null)
        {
            shop.shopItemPlacement = this;
            shop.item = item;
            shop.MakeItemInfo();
        }
        slot.SetActive(true);
        count++;
    }

        // 마지막 슬롯은 무기로
    /*
    if (pickedWeapon != null && count < MaxItemCount)
        {
            GameObject slot = Instantiate(itemSlotPrefab, shopBackground.transform, false);
            var shop = slot.GetComponent<ShopItem>();
            if (shop != null)
            {
                shop.shopItemPlacement = this;
                shop.weaponData = pickedWeapon; // ShopItem에 weaponData 처리 로직 필요
                shop.MakeWeaponInfo();          // 무기 전용 UI 생성 함수
            }
            slot.SetActive(true);
            count++;
        }
        */

    // 시작 포커스
    currentIndex = 0;
}

    public void Initialize()
    {
        PlaceItems();
    }

    public void PurchaseItem()
    {
        // 구매 처리 필요 시 구현
        // ex) items[currentIndex] 구매, UI 갱신 등
        Debug.Log($"Purchased index {currentIndex}");
    }

    // ===== 포커스 유틸 (ShopItem만 전제) =====
    private void Highlight(int index)
    {
        if (!IndexValid(index))
        {
            NextButtonHighLight.SetActive(true);
            return;
        }
        var t = shopBackground.transform.GetChild(index);
        var shop = t.GetComponent<ShopItem>();
        if (shop != null)
        {
            shop.OnPointerEnter(null);
        }
    }

    private void Unhighlight(int index)
    {
        if (!IndexValid(index))
        {
            NextButtonHighLight.SetActive(false);
            return;
        }
        var t = shopBackground.transform.GetChild(index);
        var shop = t.GetComponent<ShopItem>();
        if (shop != null)
        {
            shop.OnPointerExit(null);
        }
    }

    private void FocusIndex(int newIndex)
    {
        if (currentIndex == newIndex) return;

        Unhighlight(currentIndex);
        currentIndex = newIndex;
        Highlight(currentIndex);
        Debug.Log("Focus -> " + currentIndex);
    }

    private bool IndexValid(int i)
    {
        int total = shopBackground.transform.childCount;
        return i >= 0 && i < total;
    }

    // ===== 입력 처리 (3×2 래핑) =====
    public void OnLeft(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        int next = currentIndex;

        if (currentIndex == NextIndex)
        {
            next = 5; // 6에서 왼쪽 -> 5
        }
        else
        {
            int col = currentIndex % Columns;
            next = (col == 0) ? currentIndex + (Columns - 1) : currentIndex - 1; // 행 내 좌측 래핑
        }

        FocusIndex(next);
    }

    public void OnRight(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        int next = currentIndex;

        if (currentIndex == 5)
        {
            next = NextIndex; // 5에서 오른쪽 -> 6
        }
        else if (currentIndex != NextIndex)
        {
            int col = currentIndex % Columns;
            next = (col == Columns - 1) ? currentIndex - (Columns - 1) : currentIndex + 1; // 행 내 우측 래핑
        }
        // currentIndex가 6이고 Right면: 아무 것도 하지 않음(요청사항)

        FocusIndex(next);
    }

    // UP: 6에서는 무시, 그 외엔 기존 3열 래핑
    public void OnUp(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (currentIndex == NextIndex) return; // 6에서 Up 비활성

        int cand = currentIndex - Columns;
        int next = (cand < 0) ? currentIndex + Columns : cand;
        FocusIndex(next);
    }

    // DOWN: 6에서는 무시, 그 외엔 기존 3열 래핑
    public void OnDown(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (currentIndex == NextIndex) return; // 6에서 Down 비활성

        int cand = currentIndex + Columns;
        int next = (cand > LastIndex) ? currentIndex - Columns : cand;
        FocusIndex(next);
    }

    public void OnSelect(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        // 현재 포커스된 아이템 선택 처리
        if (!IndexValid(currentIndex))
        {
            GameSceneController.Instance.StartGame();
            Unhighlight(currentIndex);
            return; // 6번 인덱스에서 선택 시 게임 시작
        }
        var t = shopBackground.transform.GetChild(currentIndex);
        var shop = t.GetComponent<ShopItem>();
        if (shop != null && !shop.isPurchased)
        {
            shop.OnPointerClick(null);
        }
    }

    private void OnControlsChanged(PlayerInput obj)
    {
        Debug.Log("Controls changed to: " + obj.currentControlScheme);
        if (obj.currentControlScheme != "GamePad")
        {
            // 마우스/키보드로 전환 시 포커스 비주얼 제거 (필요하다면 유지해도 됨)
            Unhighlight(currentIndex);
        }
    }
}
