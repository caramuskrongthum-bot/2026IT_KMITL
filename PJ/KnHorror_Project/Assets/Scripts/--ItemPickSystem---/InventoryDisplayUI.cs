using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // ต้องใช้อันนี้เพื่อรับค่าคลิกปุ่มบน UI จ่ะแม่

public class InventoryDisplayUI : MonoBehaviour
{
    public static InventoryDisplayUI Instance;

    [Header("Database")]
    [Tooltip("ใส่ ItemData ทั้งหมดที่ต้องการให้ระบบแสดง")]
    public ItemData[] allGameItems;

    [Header("UI Slot Template")]
    public GameObject itemSlotUIPrefab;
    public Transform contentPanel;

    [Header("Sell Mode Settings")]
    [Tooltip("ถ้าเป็น True กดที่ไอเทมในหน้าจอ Inventory นี้ จะเป็นการขายไอเทมทันทีจ่ะแม่")]
    public bool Sell_Mode = false;

    [Header("Audio & Effects (Optional)")]
    public AudioSource AS;
    public AudioClip SFX_Sell;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        RefreshInventoryUI();
    }

    // ============================================================
    // REFRESH
    // ============================================================

    public void RefreshInventoryUI()
    {
        Debug.Log(
            "<color=cyan><b>[INVENTORY UI]</b> Refresh Inventory</color>"
        );

        DisplayAllCollectedItems();
        DebugPrintSavedItems();
    }

    // ============================================================
    // DEBUG SAVE DATA
    // ============================================================

    public void DebugPrintSavedItems()
    {
        Debug.Log(
            "<color=yellow>" +
            "========== SAVED INVENTORY ==========" +
            "</color>"
        );

        if (allGameItems == null ||
            allGameItems.Length == 0)
        {
            Debug.LogWarning(
                "[INVENTORY UI] allGameItems ยังว่าง!"
            );

            return;
        }

        bool hasItem = false;

        foreach (ItemData item in allGameItems)
        {
            if (item == null)
                continue;

            string saveKey =
                "ItemCount_" + item.ITEM_NAME;

            int count =
                PlayerPrefs.GetInt(saveKey, 0);

            if (count > 0)
            {
                hasItem = true;

                Debug.Log(
                    $"<color=green>[FOUND]</color> " +
                    $"{item.ITEM_NAME} = X{count}"
                );
            }
        }

        if (!hasItem)
        {
            Debug.Log(
                "<color=red>[INVENTORY UI]</color> " +
                "ไม่พบไอเทมที่บันทึกไว้"
            );
        }

        Debug.Log(
            "<color=yellow>" +
            "====================================" +
            "</color>"
        );
    }

    // ============================================================
    // DISPLAY
    // ============================================================

    public void DisplayAllCollectedItems()
    {
        if (contentPanel == null)
        {
            Debug.LogError(
                "[INVENTORY UI] contentPanel ยังไม่ได้ใส่!"
            );

            return;
        }

        if (itemSlotUIPrefab == null)
        {
            Debug.LogError(
                "[INVENTORY UI] itemSlotUIPrefab ยังไม่ได้ใส่!"
            );

            return;
        }

        if (allGameItems == null ||
            allGameItems.Length == 0)
        {
            Debug.LogWarning(
                "[INVENTORY UI] allGameItems ไม่มีข้อมูล!"
            );

            return;
        }

        // ลบ UI เดิม
        for (int i = contentPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(contentPanel.GetChild(i).gameObject);
        }

        int displayedAmount = 0;

        foreach (ItemData item in allGameItems)
        {
            if (item == null)
                continue;

            string saveKey =
                "ItemCount_" + item.ITEM_NAME;

            int count =
                PlayerPrefs.GetInt(saveKey, 0);

            if (count <= 0)
                continue;

            GameObject slotObj =
                Instantiate(
                    itemSlotUIPrefab,
                    contentPanel
                );

            displayedAmount++;

            // ====================================================
            // ICON
            // ====================================================

            Transform iconTransform =
                slotObj.transform.Find("Icon");

            if (iconTransform != null)
            {
                Image iconImg =
                    iconTransform.GetComponent<Image>();

                if (iconImg != null)
                {
                    iconImg.sprite = item.icon;
                    iconImg.gameObject.SetActive(
                        item.icon != null
                    );
                }
            }

            // ====================================================
            // COUNT
            // ====================================================

            Transform countTransform =
                slotObj.transform.Find("CountText");

            if (countTransform != null)
            {
                TextMeshProUGUI countText =
                    countTransform.GetComponent<TextMeshProUGUI>();

                if (countText != null)
                {
                    countText.text = "X" + count;
                }
            }

            // ====================================================
            // SELL BUTTON BINDING (ผูกปุ่มคลิกขายของในแต่ละ Slot)
            // ====================================================
            // เช็คว่าที่ตัว Prefab (หรือปุ่มใน Slot) มี Button Component ไหม ถ้ามีให้แอดฟังชันก์คลิกเข้าไป
            Button slotButton = slotObj.GetComponent<Button>();
            if (slotButton == null)
            {
                // ถ้าตัว Prefab หลักไม่มี ลองหาที่ลูกหรือใส่ให้เองอัตโนมัติก็ได้จ่ะ แต่ถ้าแม่ใส่ปุ่มไว้แล้วมันจะเจอเลย
                slotButton = slotObj.GetComponentInChildren<Button>();
            }

            if (slotButton != null)
            {
                // เก็บค่า Item ตัวนี้ไว้ในตัวแปรท้องถิ่นเพื่อส่งเข้า Listener
                ItemData clickedItem = item;
                slotButton.onClick.AddListener(() => OnInventorySlotClicked(clickedItem));
            }

            Debug.Log(
                $"<color=green>[UI CREATED]</color> " +
                $"{item.ITEM_NAME} X{count}"
            );
        }

        Debug.Log(
            $"<color=cyan>[INVENTORY UI]</color> " +
            $"สร้าง UI ทั้งหมด {displayedAmount} รายการ"
        );
    }

    // ============================================================
    // CLICK SLOT TO SELL (ระบบกดขายไอเทม)
    // ============================================================

    public void OnInventorySlotClicked(ItemData clickedItem)
    {
        if (clickedItem == null) return;

        // ถ้าเปิด Sell_Mode เป็น True ถึงจะขายจ่ะแม่!
        if (Sell_Mode)
        {
            string saveKey = "ItemCount_" + clickedItem.ITEM_NAME;
            int currentCount = PlayerPrefs.GetInt(saveKey, 0);

            if (currentCount > 0)
            {
                // 1. ลดจำนวนไอเทมใน PlayerPrefs ลง 1 ชิ้น (หรือจะขายหมดทีเดียวทั้งสแต็คก็ได้นะแม่ อันนี้ลดทีละ 1 ชิ้นกำลังดีสับๆ)
                int newCount = currentCount - 1;
                PlayerPrefs.SetInt(saveKey, newCount);

                // 2. บวกเงินเข้ากระเป๋า (ใช้ key "MONEY_DATA" ตามระบบเดิม)
                int currentMoney = PlayerPrefs.GetInt("MONEY_DATA", 0);
                int itemValue = clickedItem.ITEM_VALUE;
                int updatedMoney = currentMoney + itemValue;
                PlayerPrefs.SetInt("MONEY_DATA", updatedMoney);

                PlayerPrefs.Save();

                Debug.Log($"<color=green><b>[UI SELL]</b></color> ขาย {clickedItem.ITEM_NAME} สำเร็จ! ได้เงินเพิ่ม {itemValue} บาท (เหลือในคลัง: {newCount})");

                // 3. เล่นเสียงขายของ (ถ้ามี)
                if (AS != null && SFX_Sell != null)
                {
                    AS.PlayOneShot(SFX_Sell);
                }

                // 4. รีเฟรชหน้าจอ UI ใหม่ทันทีเพื่อให้ตัวเลขลดลงหรือหายไป
                RefreshInventoryUI();
            }
            else
            {
                Debug.LogWarning($"[UI SELL] ไอเทม {clickedItem.ITEM_NAME} หมดเกลี้ยงแล้วจ่ะแม่ ขายไม่ได้แล้ว!");
            }
        }
        else
        {
            Debug.Log($"[INVENTORY UI] คลิกที่ไอเทม {clickedItem.ITEM_NAME} (ตอนนี้ Sell_Mode ปิดอยู่ เลยยังไม่ขายนะจ๊ะ)");
        }
    }

    // ============================================================
    // CLEAR ALL SAVED DATA
    // ============================================================

    public void ClearAllSavedItems()
    {
        if (allGameItems == null)
            return;

        foreach (ItemData item in allGameItems)
        {
            if (item == null)
                continue;

            string saveKey =
                "ItemCount_" + item.ITEM_NAME;

            PlayerPrefs.DeleteKey(saveKey);
        }

        PlayerPrefs.Save();

        Debug.Log(
            "<color=red><b>[INVENTORY]</b></color> " +
            "ลบข้อมูลไอเทมทั้งหมดแล้ว"
        );

        RefreshInventoryUI();
    }

    public void SetSell_Mode()
    {
        Sell_Mode = !Sell_Mode;
    }
}