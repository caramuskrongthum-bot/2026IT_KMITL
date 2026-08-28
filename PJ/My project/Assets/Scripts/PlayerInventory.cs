using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class InventorySaveData
{
    // ลิสต์เก็บชื่อ/รหัสไฟล์ Asset ของไอเทมแต่ละช่อง
    public List<string> itemKeys = new List<string>();
}

public class PlayerInventory : MonoBehaviour
{
    // C# Event สำหรับส่งสัญญาณอัปเดต UI แบบ Real-time
    public event Action OnInventoryChanged;

    [Header("Inventory Settings")]
    public int maxCapacity = 7;
    public List<ItemData> items = new List<ItemData>();
    public UnityEvent onInventoryFull;
    public TextMeshProUGUI Inventory_Display;

    private string saveFilePath;

    private void Awake()
    {
        // กำหนดที่อยู่ไฟล์ Save JSON
        saveFilePath = Path.Combine(Application.persistentDataPath, "inventory_save.json");
    }

    private void Start()
    {
        // โหลดข้อมูลกระเป๋าเมื่อเริ่มเกม
        LoadInventory();
    }

    private void Update()
    {
        Inventory_Display.text = items.Count + "/" + maxCapacity;
    }

    /// <summary>
    /// ฟังก์ชันเพิ่มไอเทมเข้ากระเป๋า
    /// </summary>
    public bool AddItem(ItemData itemToAdd)
    {
        // 1. ตรวจสอบว่ากระเป๋าเต็ม 7 ช่องหรือยัง
        if (items.Count >= maxCapacity)
        {
            Debug.Log("Inventory เต็มแล้ว! ไม่สามารถเก็บเพิ่มได้");
            onInventoryFull?.Invoke();
            return false;
        }

        // 2. เพิ่มไอเทมลง List
        items.Add(itemToAdd);
        Debug.Log($"เก็บไอเทม: {itemToAdd.itemName} สำเร็จ!");

        // 3. บันทึกข้อมูลและส่งสัญญาณอัปเดต UI
        SaveInventory();
        OnInventoryChanged?.Invoke();

        return true;
    }

    /// <summary>
    /// ฟังก์ชันลบไอเทมออกจากกระเป๋า
    /// </summary>
    public bool RemoveItem(ItemData itemToRemove)
    {
        if (items.Contains(itemToRemove))
        {
            items.Remove(itemToRemove);

            // บันทึกข้อมูลและส่งสัญญาณอัปเดต UI
            SaveInventory();
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }

    public int CalculateTotalSellValue()
    {
        int totalValue = 0;
        foreach (var item in items)
        {
            if (item != null)
            {
                totalValue += item.value;
            }
        }
        return totalValue;
    }

    public void OnPlayerDeath()
    {
        if (LootManager.Instance != null)
        {
            LootManager.Instance.StealAllItemsFromPlayer(this);
        }

        // บันทึกกระเป๋าที่ว่างเปล่าหลังจากโดนขโมย
        SaveInventory();
    }

    public void ClearInventoryUI()
    {
        // บันทึกการล้างกระเป๋า และแจ้งเตือน UI ให้รีเฟรช
        SaveInventory();
        OnInventoryChanged?.Invoke();
    }

    #region Save & Load System (JSON)

    /// <summary>
    /// เซฟข้อมูลไอเทมในกระเป๋าลงไฟล์ JSON
    /// </summary>
    public void SaveInventory()
    {
        InventorySaveData saveData = new InventorySaveData();

        foreach (var item in items)
        {
            if (item != null)
            {
                saveData.itemKeys.Add(item.name);
            }
        }

        string json = JsonUtility.ToJson(saveData, true);

        // เปลี่ยนจาก File.WriteAllText เป็น PlayerPrefs
        PlayerPrefs.SetString("InventorySaveData", json);
        PlayerPrefs.Save(); // สั่งบันทึกลง Browser Storage

        Debug.Log("💾 บันทึก Inventory ลง PlayerPrefs สำเร็จ!");
    }

    /// <summary>
    /// โหลดข้อมูลไอเทมจากไฟล์ JSON กลับเข้ากระเป๋า
    /// </summary>
    public void LoadInventory()
    {
        // เปลี่ยนจาก File.Exists เป็น PlayerPrefs.HasKey
        if (!PlayerPrefs.HasKey("InventorySaveData"))
        {
            Debug.Log("📂 ไม่พบไฟล์เซฟกระเป๋า (เริ่มต้นกระเป๋าว่างเปล่า)");
            return;
        }

        try
        {
            // เปลี่ยนจาก File.ReadAllText เป็น PlayerPrefs.GetString
            string json = PlayerPrefs.GetString("InventorySaveData");
            InventorySaveData saveData = JsonUtility.FromJson<InventorySaveData>(json);

            items.Clear();

            foreach (string itemKey in saveData.itemKeys)
            {
                ItemData loadedItem = Resources.Load<ItemData>($"Items/{itemKey}");

                if (loadedItem != null)
                {
                    items.Add(loadedItem);
                }
                else
                {
                    Debug.LogWarning($"⚠️ ไม่พบไฟล์ ItemData ชื่อ '{itemKey}' ใน Resources/Items/");
                }
            }

            Debug.Log($"📂 โหลด Inventory สำเร็จ! จำนวนไอเทม: {items.Count} ชิ้น");
            OnInventoryChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ เกิดข้อผิดพลาดในการโหลด Inventory: {ex.Message}");
        }
    }

    /// <summary>
    /// ลบไฟล์เซฟกระเป๋า (ใช้เมื่อต้องการ Reset เกมใหม่)
    /// </summary>
    public void DeleteSaveFile()
    {
        if (PlayerPrefs.HasKey("InventorySaveData"))
        {
            PlayerPrefs.DeleteKey("InventorySaveData");
            items.Clear();
            OnInventoryChanged?.Invoke();
            Debug.Log("🗑️ ลบข้อมูลเซฟกระเป๋าเรียบร้อยแล้ว");
        }
    }

    #endregion
}