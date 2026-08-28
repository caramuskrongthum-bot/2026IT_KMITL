using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class InventorySlotUI : MonoBehaviour, IPointerClickHandler
{
    [Header("UI References")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Button dropButton; // ปุ่ม Drop ใน UI Slot (ใส่หรือไม่ใส่ก็ได้)
    [SerializeField] private Button sellButton; // ปุ่ม Sell ใน UI Slot (ใส่หรือไม่ใส่ก็ได้)

    private ItemData currentItem;
    private InventoryUI parentInventoryUI;

    private void Awake()
    {
        // หากผูก UI Button ไว้ใน Inspector จะทำการลบและเพิ่ม Listener อัตโนมัติ
        if (dropButton != null)
        {
            dropButton.onClick.RemoveAllListeners();
            dropButton.onClick.AddListener(OnDropButtonClicked);
        }

        if (sellButton != null)
        {
            sellButton.onClick.RemoveAllListeners();
            sellButton.onClick.AddListener(OnSellButtonClicked);
        }
    }

    public void SetItem(ItemData item, InventoryUI inventoryUI)
    {
        currentItem = item;
        parentInventoryUI = inventoryUI;

        if (item != null && item.icon != null)
        {
            itemIcon.sprite = item.icon;
            itemIcon.enabled = true;

            if (dropButton != null) dropButton.gameObject.SetActive(true);
            if (sellButton != null) sellButton.gameObject.SetActive(true);
        }
        else
        {
            ClearSlot();
        }
    }

    public void ClearSlot()
    {
        currentItem = null;
        if (itemIcon != null)
        {
            itemIcon.sprite = null;
            itemIcon.enabled = false;
        }

        if (dropButton != null) dropButton.gameObject.SetActive(false);
        if (sellButton != null) sellButton.gameObject.SetActive(false);
    }

    // ฟังก์ชันสำหรับกดปุ่ม Drop บน UI
    public void OnDropButtonClicked()
    {
        if (currentItem != null && parentInventoryUI != null)
        {
            parentInventoryUI.DropItem(currentItem);
        }
    }

    // ฟังก์ชันสำหรับกดปุ่ม Sell บน UI
    public void OnSellButtonClicked()
    {
        if (currentItem != null && parentInventoryUI != null)
        {
            parentInventoryUI.SellItem(currentItem);
        }
    }

    // สำหรับการสั่งการผ่านการคลิกเม้าส์โดยตรง (คลิกขวา = Drop / คลิกซ้าย = Sell)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null || parentInventoryUI == null) return;

        if (eventData.button == PointerEventData.InputButton.Right)
        {
            parentInventoryUI.DropItem(currentItem);
        }
        else if (eventData.button == PointerEventData.InputButton.Left)
        {
            // หากไม่ได้ใช้ปุ่ม UI ให้ปลดล็อกบรรทัดนี้เพื่อคลิกซ้ายขายของได้
            // parentInventoryUI.SellItem(currentItem);
        }
    }
}