
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class InventorySlot : MonoBehaviour
{
    public static InventorySlot Instance;

    [Header("Inventory Configuration")]
    public Image[] iconImages = new Image[10];
    public TextMeshProUGUI[] nameTexts = new TextMeshProUGUI[10];
    public ItemData[] assignedItems = new ItemData[10];

    public int ItemValueOverAll;
    public TextMeshProUGUI ValueOverAll_Text_Display;

    [Header("Audio & Effects")]
    public AudioSource AS;
    public AudioClip SFX_Collect;
    public AudioClip SFX_Sell;
    public Animator Animator_BG_Glow;

    [Header("Drop Item Settings")]
    public Transform playerDropTransform;
    public float dropOffsetForward = 1.5f;
    public InputAction Drop;

    [Header("Selected Slot UI / Highlight")]
    public int selectedSlotIndex = -1;
    public Outline[] slotOutlines = new Outline[10];

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

        for (int i = 0; i < assignedItems.Length; i++)
        {
            ClearSlot(i);
        }
    }

    private void OnEnable()
    {
        if (Drop != null)
        {
            Drop.Enable();
            Drop.performed += OnDropSelectedItemSelected;
        }
    }

    private void OnDisable()
    {
        if (Drop != null)
        {
            Drop.performed -= OnDropSelectedItemSelected;
            Drop.Disable();
        }
    }

    private void Start()
    {
        CalculateTotalValue();
    }

    private void Update()
    {
        CheckNumberKeyBoardInput();
    }

    private void CheckNumberKeyBoardInput()
    {
        if (Keyboard.current == null)
            return;

        int keyPressed = -1;

        if (Keyboard.current.digit1Key.wasPressedThisFrame) keyPressed = 0;
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) keyPressed = 1;
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) keyPressed = 2;
        else if (Keyboard.current.digit4Key.wasPressedThisFrame) keyPressed = 3;
        else if (Keyboard.current.digit5Key.wasPressedThisFrame) keyPressed = 4;
        else if (Keyboard.current.digit6Key.wasPressedThisFrame) keyPressed = 5;
        else if (Keyboard.current.digit7Key.wasPressedThisFrame) keyPressed = 6;
        else if (Keyboard.current.digit8Key.wasPressedThisFrame) keyPressed = 7;
        else if (Keyboard.current.digit9Key.wasPressedThisFrame) keyPressed = 8;
        else if (Keyboard.current.digit0Key.wasPressedThisFrame) keyPressed = 9;

        if (keyPressed != -1)
        {
            SelectSlot(keyPressed);
        }
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= assignedItems.Length)
            return;

        selectedSlotIndex = index;

        Debug.Log(
            $"<color=yellow><b>[INVENTORY]</b></color> เลือกช่องที่ {index + 1}"
        );

        UpdateSlotHighlights();

        if (assignedItems[index] != null)
        {
            Debug.Log(
                $"ไอเทมในช่อง: {assignedItems[index].ITEM_NAME}"
            );
        }
    }

    private void UpdateSlotHighlights()
    {
        for (int i = 0; i < slotOutlines.Length; i++)
        {
            if (slotOutlines[i] != null)
            {
                slotOutlines[i].enabled = i == selectedSlotIndex;
            }
        }
    }

    public void OnDropSelectedItemSelected(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            DropCurrentSelectedItem();
        }
    }

    public void DropCurrentSelectedItem()
    {
        if (selectedSlotIndex < 0 ||
            selectedSlotIndex >= assignedItems.Length)
        {
            Debug.LogWarning("ยังไม่ได้เลือกช่องไอเทมที่จะทิ้ง");
            return;
        }

        ItemData itemToDrop = assignedItems[selectedSlotIndex];

        if (itemToDrop == null)
        {
            Debug.Log("ช่องนี้ว่าง ไม่มีไอเทมให้ทิ้ง");
            return;
        }

        if (itemToDrop.itemPrefab != null)
        {
            Vector3 spawnPos = transform.position;

            if (playerDropTransform != null)
            {
                spawnPos =
                    playerDropTransform.position +
                    playerDropTransform.forward * dropOffsetForward;
            }
            else
            {
                GameObject player =
                    GameObject.FindGameObjectWithTag("Player");

                if (player != null)
                {
                    spawnPos =
                        player.transform.position +
                        player.transform.forward * dropOffsetForward;
                }
            }

            Instantiate(
                itemToDrop.itemPrefab,
                spawnPos,
                Quaternion.Euler(0, Random.Range(0f, 360f), 0)
            );

            Debug.Log(
                $"<color=red><b>[DROP]</b></color> ทิ้งไอเทม {itemToDrop.ITEM_NAME} สำเร็จ"
            );
        }
        else
        {
            Debug.LogWarning(
                $"ItemData ของ {itemToDrop.ITEM_NAME} ไม่มี itemPrefab"
            );
        }

        ClearSlot(selectedSlotIndex);
        CalculateTotalValue();
    }

    public bool AddItem(ItemData newItem)
    {
        if (newItem == null)
            return false;

        for (int i = 0; i < assignedItems.Length; i++)
        {
            if (assignedItems[i] == null)
            {
                SetItem(i, newItem);

                Debug.Log(
                    $"<color=green>[INVENTORY]</color> เก็บ {newItem.ITEM_NAME} ลงช่องที่ {i + 1} สำเร็จ"
                );

                if (AS != null && SFX_Collect != null)
                {
                    AS.PlayOneShot(SFX_Collect);
                }

                if (Animator_BG_Glow != null)
                {
                    Animator_BG_Glow.Play("Glow_");
                }

                CalculateTotalValue();

                return true;
            }
        }

        Debug.LogWarning("Inventory เต็ม");
        return false;
    }

    public void SetItem(int slotIndex, ItemData newItem)
    {
        if (slotIndex < 0 ||
            slotIndex >= assignedItems.Length)
            return;

        assignedItems[slotIndex] = newItem;

        if (newItem != null)
        {
            if (slotIndex < iconImages.Length &&
                iconImages[slotIndex] != null)
            {
                iconImages[slotIndex].sprite = newItem.icon;
                iconImages[slotIndex].gameObject.SetActive(true);
            }

            if (slotIndex < nameTexts.Length &&
                nameTexts[slotIndex] != null)
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
        if (slotIndex < 0 ||
            slotIndex >= assignedItems.Length)
            return;

        assignedItems[slotIndex] = null;

        if (slotIndex < iconImages.Length &&
            iconImages[slotIndex] != null)
        {
            iconImages[slotIndex].sprite = null;
            iconImages[slotIndex].gameObject.SetActive(false);
        }

        if (slotIndex < nameTexts.Length &&
            nameTexts[slotIndex] != null)
        {
            nameTexts[slotIndex].text = "";
            nameTexts[slotIndex].gameObject.SetActive(false);
        }
    }

    public void OnSlotClicked(int slotIndex)
    {
        SelectSlot(slotIndex);
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
            ValueOverAll_Text_Display.text =
                ItemValueOverAll.ToString();
        }
    }

    public void SellAll()
    {
        if (ItemValueOverAll <= 0)
        {
            Debug.Log("ไม่มีไอเทมให้ขาย");
            return;
        }

        int currentMoney =
            PlayerPrefs.GetInt("MONEY_DATA", 0);

        int updatedMoney =
            currentMoney + ItemValueOverAll;

        PlayerPrefs.SetInt(
            "MONEY_DATA",
            updatedMoney
        );

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

        Debug.Log(
            $"<color=yellow>[SELL]</color> ขายของทั้งหมดได้เงิน {ItemValueOverAll}"
        );
    }

    // ============================================================
    // SAVE INVENTORY
    // ============================================================

    public void SaveItemGotThisMatch()
    {
        Debug.Log(
            "<color=cyan><b>========== SAVE INVENTORY ==========</b></color>"
        );

        int savedItemAmount = 0;

        for (int i = 0; i < assignedItems.Length; i++)
        {
            ItemData item = assignedItems[i];

            if (item == null)
                continue;

            string saveKey =
                "ItemCount_" + item.ITEM_NAME;

            int oldCount =
                PlayerPrefs.GetInt(saveKey, 0);

            int newCount =
                oldCount + 1;

            PlayerPrefs.SetInt(
                saveKey,
                newCount
            );

            savedItemAmount++;

            Debug.Log(
                $"[SAVE] Slot {i + 1} | " +
                $"Item = {item.ITEM_NAME} | " +
                $"Key = {saveKey} | " +
                $"Old = {oldCount} | " +
                $"New = {newCount}"
            );
        }

        PlayerPrefs.Save();

        Debug.Log(
            $"<color=green><b>[SAVE COMPLETE]</b></color> " +
            $"บันทึก {savedItemAmount} ชิ้นเรียบร้อย"
        );

        // Refresh UI ถ้ามี InventoryDisplayUI อยู่ใน Scene
        if (InventoryDisplayUI.Instance != null)
        {
            InventoryDisplayUI.Instance.RefreshInventoryUI();
        }
    }
}