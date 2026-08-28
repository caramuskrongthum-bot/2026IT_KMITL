using System;
using System.Collections.Generic;
using UnityEngine;

public class LootManager : MonoBehaviour
{
    public static LootManager Instance { get; private set; }

    [Header("Stage Settings")]
    [Tooltip("ด่านปัจจุบัน (ใช้สำหรับแยกคีย์เซฟของแต่ละด่าน)")]
    public int stage = 1;

    [Header("Stolen Items Pool")]
    public List<ItemData> stolenItemsPool = new List<ItemData>();

    [Serializable]
    private class StolenDataSaveWrapper
    {
        public int savedStage;
        public List<string> itemResourcePaths = new List<string>();
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        LoadStolenItems();
    }

    public void SetStage(int newStage)
    {
        stage = newStage;
        LoadStolenItems();
    }

    /// <summary>
    /// ดึงชื่อ Key สำหรับ PlayerPrefs ตาม Stage ปัจจุบัน
    /// </summary>
    private string GetSaveKey()
    {
        return $"StolenItems_Stage_{stage}";
    }

    public void StealAllItemsFromPlayer(PlayerInventory playerInventory)
    {
        if (playerInventory == null || playerInventory.items.Count == 0) return;

        stolenItemsPool.AddRange(playerInventory.items);
        playerInventory.items.Clear();
        playerInventory.ClearInventoryUI();

        SaveStolenItems();

        Debug.Log($"☠️ ผู้เล่นตายใน Stage {stage}! มีไอเทมสะสมในคลังขโมยรวม {stolenItemsPool.Count} ชิ้น");
    }

    public ItemData PopOneStolenItem()
    {
        if (stolenItemsPool.Count == 0) return null;

        ItemData item = stolenItemsPool[0];
        stolenItemsPool.RemoveAt(0);

        SaveStolenItems();

        return item;
    }

    public int GetStolenItemCount()
    {
        return stolenItemsPool.Count;
    }

    #region Save & Load PlayerPrefs Logic (WebGL Supported)

    /// <summary>
    /// บันทึกคลังไอเทมโดนขโมยลง PlayerPrefs
    /// </summary>
    public void SaveStolenItems()
    {
        StolenDataSaveWrapper saveData = new StolenDataSaveWrapper();
        saveData.savedStage = stage;

        foreach (var item in stolenItemsPool)
        {
            if (item == null) continue;
            string resourcePath = "Items/" + item.name;
            saveData.itemResourcePaths.Add(resourcePath);
        }

        string json = JsonUtility.ToJson(saveData, true);
        string saveKey = GetSaveKey();

        // บันทึกลง PlayerPrefs แทนไฟล์
        PlayerPrefs.SetString(saveKey, json);
        PlayerPrefs.Save();

        Debug.Log($"💾 บันทึกข้อมูลคลังไอเทมขโมย Stage {stage} ลง PlayerPrefs เรียบร้อย Key: {saveKey}");
    }

    /// <summary>
    /// โหลดคลังไอเทมโดนขโมยจาก PlayerPrefs
    /// </summary>
    public void LoadStolenItems()
    {
        stolenItemsPool.Clear();
        string saveKey = GetSaveKey();

        if (!PlayerPrefs.HasKey(saveKey))
        {
            Debug.Log($"ℹ️ ไม่พบเซฟคลังไอเทมสำหรับ Stage {stage} (จะเริ่มด้วยคลังว่างเปล่า)");
            return;
        }

        try
        {
            string json = PlayerPrefs.GetString(saveKey);
            StolenDataSaveWrapper saveData = JsonUtility.FromJson<StolenDataSaveWrapper>(json);

            if (saveData != null && saveData.itemResourcePaths != null)
            {
                foreach (string path in saveData.itemResourcePaths)
                {
                    ItemData loadedItem = Resources.Load<ItemData>(path);
                    if (loadedItem != null)
                    {
                        stolenItemsPool.Add(loadedItem);
                    }
                    else
                    {
                        string itemNameOnly = System.IO.Path.GetFileName(path);
                        ItemData fallbackItem = Resources.Load<ItemData>(itemNameOnly);
                        if (fallbackItem != null)
                        {
                            stolenItemsPool.Add(fallbackItem);
                        }
                        else
                        {
                            Debug.LogWarning($"⚠️ ไม่พบไฟล์ ItemData ที่ตำแหน่ง: {path} หรือ Resources/{itemNameOnly}");
                        }
                    }
                }
            }

            Debug.Log($"📂 โหลดข้อมูลคลังไอเทมขโมย Stage {stage} สำเร็จ! จำนวน {stolenItemsPool.Count} ชิ้น");
        }
        catch (Exception ex)
        {
            Debug.LogError($"❌ โครงสร้างข้อมูลเซฟของ Stage {stage} มีปัญหา: {ex.Message}");
        }
    }

    /// <summary>
    /// เคลียร์ข้อมูลคลังเซฟของ Stage นั้นๆ ออกทั้งหมด
    /// </summary>
    public void ClearStolenItemsData()
    {
        stolenItemsPool.Clear();
        string saveKey = GetSaveKey();

        if (PlayerPrefs.HasKey(saveKey))
        {
            PlayerPrefs.DeleteKey(saveKey);
            Debug.Log($"🗑️ ลบเซฟ JSON ของ Stage {stage} จาก PlayerPrefs เรียบร้อยแล้ว");
        }
    }

    #endregion
}