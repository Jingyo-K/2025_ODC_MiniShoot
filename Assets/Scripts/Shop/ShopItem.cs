using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public GameObject ItemSlot;
    public Image ItemEdgeHighlight;
    public Item item; // Reference to the item associated with this shop item
    public WeaponData weaponData; // Reference to the weapon data if this item is a weapon
    public PlayerStat playerStat; // Reference to the player's stats
    public Image itemIcon; // Reference to the item's icon for display
    public TextMeshProUGUI itemName; // Reference to the item's name for display
    public TextMeshProUGUI itemPrice; // Reference to the item's price for display
    public TextMeshProUGUI itemDescription; // Reference to the item's description for display
    public ShopItemPlacement shopItemPlacement; // Reference to the ShopItemPlacement component
    private Color colorAlpha;
    public bool isPurchased = false; // Flag to check if the item has been purchased

    private void Start()
    {
        colorAlpha = ItemEdgeHighlight.color;
        colorAlpha.a = 0f; // Set initial alpha to 0 (transparent)
        ItemEdgeHighlight.color = colorAlpha;
        playerStat = FindObjectOfType<PlayerStat>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Code to handle pointer entering the shop item
        colorAlpha.a = 1f; // Set alpha to 1 (fully opaque)
        ItemEdgeHighlight.color = colorAlpha;
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // Code to handle pointer exiting the shop item
        colorAlpha.a = 0f; // Set alpha to 0 (fully transparent)
        ItemEdgeHighlight.color = colorAlpha;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isPurchased)
        {
            return; // If the item is already purchased, do nothing
        }
        ScrapSystem playerScrapSystem = GameSceneController.Instance.playerInstance.GetComponent<ScrapSystem>();
        if (weaponData != null)
        {
            if (weaponData.weaponPrice > playerScrapSystem.scrapCount)
            {
                Debug.Log("Not enough scrap to purchase this weapon.");
                return; // Not enough scrap to purchase the weapon
            }
            else
            {
                playerScrapSystem.UseScrap(weaponData.weaponPrice); // Deduct the scrap cost from the player's scrap count
                playerStat.transform.GetComponent<PlayerFire>().EquipWeapon(weaponData);
            }
        }
        else
        {
            if (item.itemPrice > playerScrapSystem.scrapCount)
            {
                Debug.Log("Not enough scrap to purchase this item.");
                return; // Not enough scrap to purchase the item
            }
            else
            {
                playerScrapSystem.UseScrap(item.itemPrice); // Deduct the scrap cost from the player's scrap count
                playerStat.AddItem(item);
            }
        }
        shopItemPlacement.PurchaseItem(); // Call the method to handle item purchase in ShopItemPlacement
        ItemSlot.SetActive(!ItemSlot.activeSelf); // Toggle the visibility of the item slot
        isPurchased = true; // Mark the item as purchased
    }

    public void MakeItemInfo()
    {
        itemIcon.sprite = item.itemIcon; // Set the item's icon
        itemName.text = item.itemName; // Set the item's name
        itemPrice.text = "$" + item.itemPrice.ToString(); // Set the item's price
        itemDescription.text = item.itemDescription; // Set the item's description
    }

    public void MakeWeaponInfo()
    {
        itemIcon.sprite = weaponData.weaponIcon; // Set the weapon's icon
        itemName.text = weaponData.weaponName; // Set the weapon's name
        itemDescription.text = weaponData.weaponDescription; // Set the weapon's description
    }

    // Additional methods can be added here for further interactions
}

