using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot : MonoBehaviour
{
    public static InventorySlot Instance;
    public Image[] iconImages = new Image[10];
    public TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[10];
    public ItemData[] assignedItems = new ItemData[10];
    public int ItemValueOverAll;
    public TextMeshProUGUI ValueOverAll_Text_Display;
    public AudioSource AS;
    public AudioClip SFX_Collect;
    public AudioClip SFX_Sell; // เพิ่มเสียง SFX ตอนขายของ (ถ้ามี)
    public Animator Animator_BG_Glow;

    private void Awake()
    {
        CalculateTotalValue();
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        for (int i = 0; i < 10; i++)
        {
            ClearSlot(i);
        }
    }

    public bool AddItem(ItemData newItem)
    {
        if (newItem == null) return false;
        for (int i = 0; i < 10; i++)
        {
            if (assignedItems[i] == null)
            {
                SetItem(i, newItem);
                Debug.Log($"เก็บไอเทม {newItem.ITEM_NAME} ลงช่องที่ {i + 1} สำเร็จค่ะแม่!");
                if (AS != null && SFX_Collect != null) AS.PlayOneShot(SFX_Collect);
                if (Animator_BG_Glow != null) Animator_BG_Glow.Play("Glow_");
                CalculateTotalValue();
                return true;
            }
        }
        return false;
    }

    public void SetItem(int slotIndex, ItemData newItem)
    {
        if (slotIndex < 0 || slotIndex >= 10) return;

        assignedItems[slotIndex] = newItem;

        if (newItem != null)
        {
            if (slotIndex < iconImages.Length && iconImages[slotIndex] != null)
            {
                iconImages[slotIndex].sprite = newItem.icon;
                iconImages[slotIndex].gameObject.SetActive(true);
            }

            if (slotIndex < nameTexts.Length && nameTexts[slotIndex] != null)
            {
                nameTexts[slotIndex].text = newItem.ITEM_NAME;
                nameTexts[slotIndex].gameObject.SetActive(true);
            }
        }
        else
        {
            ClearSlot(slotIndex);
        }
    }

    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 10) return;

        assignedItems[slotIndex] = null;

        if (slotIndex < iconImages.Length && iconImages[slotIndex] != null)
        {
            iconImages[slotIndex].sprite = null;
            iconImages[slotIndex].gameObject.SetActive(false);
        }

        if (slotIndex < nameTexts.Length && nameTexts[slotIndex] != null)
        {
            nameTexts[slotIndex].text = string.Empty;
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        if (slotIndex >= 0 && slotIndex < 10 && assignedItems[slotIndex] != null)
        {
            ItemData clickedItem = assignedItems[slotIndex];
            Debug.Log($"คลิกช่องที่ {slotIndex + 1} | ไอเทม: {clickedItem.ITEM_NAME} | มูลค่า: {clickedItem.ITEM_VALUE}");
        }
    }

    public void CalculateTotalValue()
    {
        ItemValueOverAll = 0;
        for (int i = 0; i < assignedItems.Length; i++)
        {
            if (assignedItems[i] != null)
            {
                ItemValueOverAll += assignedItems[i].ITEM_VALUE;
            }
        }
        if (ValueOverAll_Text_Display != null)
        {
            ValueOverAll_Text_Display.text = ItemValueOverAll.ToString();
        }
    }
    public void SellAll()
    {
        if (ItemValueOverAll <= 0)
        {
            Debug.Log("ไม่มีไอเทมให้ขายเลยค่ะคุณแม่!");
            return;
        }

        int currentMoney = PlayerPrefs.GetInt("MONEY_DATA", 0);
        int updatedMoney = currentMoney + ItemValueOverAll;
        PlayerPrefs.SetInt("MONEY_DATA", updatedMoney);
        PlayerPrefs.Save();
        for (int i = 0; i < assignedItems.Length; i++)
        {
            ClearSlot(i);
        }
        if (AS != null && SFX_Sell != null)
        {
            AS.PlayOneShot(SFX_Sell);
        }
        CalculateTotalValue();
    }
}