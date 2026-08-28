using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInventory playerInventory;
    [SerializeField] private Transform slotContainer;
    [SerializeField] private InventorySlotUI[] slots;

    [Header("Drop Settings")]
    [Tooltip("ตำแหน่งตรงหน้าผู้เล่นที่จะให้ไอเทมตกลงพื้น")]
    [SerializeField] private Transform dropPoint;

    [Tooltip("ใส่ Prefab ไอเทมบนพื้นทั้งหมดในเกม เพื่อให้อ้างอิงเสกวัตถุได้ถูกต้อง")]
    [SerializeField] private List<GameObject> itemPrefabs;

    private void Awake()
    {
        if (slots == null || slots.Length == 0)
        {
            slots = slotContainer.GetComponentsInChildren<InventorySlotUI>();
        }
    }

    private void OnEnable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged += RefreshUI;
        }
        RefreshUI();
    }

    private void OnDisable()
    {
        if (playerInventory != null)
        {
            playerInventory.OnInventoryChanged -= RefreshUI;
        }
    }

    /// <summary>
    /// รีเฟรชรูปไอคอนทั้ง 7 ช่อง
    /// </summary>
    public void RefreshUI()
    {
        if (playerInventory == null) return;

        List<ItemData> currentItems = playerInventory.items;

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < currentItems.Count)
            {
                slots[i].SetItem(currentItems[i], this);
            }
            else
            {
                slots[i].ClearSlot();
            }
        }
    }

    /// <summary>
    /// สั่งทิ้งไอเทมลงพื้น
    /// </summary>
    public void DropItem(ItemData itemToDrop)
    {
        if (itemToDrop == null) return;

        bool removed = playerInventory.RemoveItem(itemToDrop);

        if (removed)
        {
            SpawnItemOnGround(itemToDrop);
        }
    }

    /// <summary>
    /// สั่งขายไอเทม และเพิ่มเงินใน PlayerPrefs
    /// </summary>
    public void SellItem(ItemData itemToSell)
    {
        if (itemToSell == null) return;

        bool removed = playerInventory.RemoveItem(itemToSell);

        if (removed)
        {
            // ดึงเงินเดิม -> เพิ่มตามราคา item.value -> บันทึกลง PlayerPrefs
            int currentMoney = PlayerPrefs.GetInt("MONEY", 0);
            int newMoney = currentMoney + itemToSell.value;
            PlayerPrefs.SetInt("MONEY", newMoney);
            PlayerPrefs.Save();

            Debug.Log($"ขาย {itemToSell.itemName} ได้รับเงิน {itemToSell.value} | เงินรวม: {newMoney}");
        }
    }

    private void SpawnItemOnGround(ItemData itemData)
    {
        GameObject prefabToSpawn = FindItemPrefab(itemData);

        if (prefabToSpawn != null)
        {
            Vector3 spawnPosition = dropPoint != null ? dropPoint.position : playerInventory.transform.position + playerInventory.transform.forward * 1.5f;

            GameObject spawnedItem = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

            if (spawnedItem.TryGetComponent<ItemWorldObject>(out var worldObj))
            {
                worldObj.Setup(itemData);
            }
        }
        else
        {
            Debug.LogWarning($"ไม่พบ Prefab ของไอเทม: {itemData.itemName} ใน Item Prefabs List!");
        }
    }

    private GameObject FindItemPrefab(ItemData targetData)
    {
        foreach (var prefab in itemPrefabs)
        {
            if (prefab.TryGetComponent<ItemWorldObject>(out var worldObj))
            {
                if (worldObj.itemData == targetData)
                {
                    return prefab;
                }
            }
        }
        return null;
    }
}