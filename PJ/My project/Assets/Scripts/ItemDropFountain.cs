using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDropFountain : MonoBehaviour
{
    // โครงสร้างข้อมูลสำหรับกำหนดการดรอปของไอเทมแต่ละชนิด
    [System.Serializable]
    public class DropData
    {
        [Tooltip("ข้อมูลไอเทมแบบ ScriptableObject")]
        public ItemData itemData;

        [Tooltip("จำนวนชิ้นที่จะเด้งออกมาเมื่อดรอปสำเร็จ")]
        public int dropCount = 1;

        [Range(0f, 1f)]
        [Tooltip("โอกาสดรอป (0.0 = ไม่ดรอปเลย, 1.0 = ดรอปแน่นอน 100%)")]
        public float rateDrop = 1.0f;
    }

    [Header("Drop Items List")]
    [Tooltip("รายการไอเทมทั้งหมดที่มีโอกาสดรอปจากศัตรูตัวนี้")]
    public DropData[] dropList;

    [Header("Spawn Settings")]
    [Tooltip("ตำแหน่งที่จะให้ไอเทมเริ่มเกิด (เช่น กลางตัวศัตรู)")]
    public Transform spawnPoint;

    [Header("Bounce Settings")]
    [Tooltip("แรงเด้งขึ้นด้านบนต่ำสุด")]
    public float minUpwardForce = 5f;
    [Tooltip("แรงเด้งขึ้นด้านบนสูงสุด")]
    public float maxUpwardForce = 10f;
    [Tooltip("แรงพุ่งออกด้านข้าง (แนวนอน) มากที่สุด")]
    public float maxHorizontalForce = 3f;

    /// <summary>
    /// ฟังก์ชันหลักสำหรับเรียกใช้งานตอนศัตรูตาย (ระบบจะคำนวณ Rate และ Count ตามที่ตั้งไว้ใน List)
    /// </summary>
    public void DropItems()
    {
        if (dropList == null || dropList.Length == 0) return;

        Vector3 finalSpawnPos = spawnPoint != null ? spawnPoint.position : transform.position;

        // วนลูปเช็คไอเทมทุกชิ้นใน dropList
        foreach (DropData item in dropList)
        {
            // เช็คว่าตั้งค่า ItemData ไว้หรือไม่ และ มี Prefab อยู่ใน ItemData หรือไม่
            if (item.itemData == null || item.itemData.Item_Pick_Prefab == null) continue;

            // 1. เช็คโอกาสดรอป (RateDrop)
            if (item.rateDrop <= 0f || Random.value > item.rateDrop)
            {
                continue;
            }

            // 2. หากสุ่มผ่าน จะเสกไอเทมออกมาตามจำนวน dropCount
            for (int i = 0; i < item.dropCount; i++)
            {
                // ส่ง ItemData ไปให้ฟังก์ชันเสกไอเทม
                SpawnAndBounceItem(item.itemData, finalSpawnPos);
            }
        }
    }

    /// <summary>
    /// ฟังก์ชันช่วยในการ Instantiate และใส่แรงเด้งน้ำพุ
    /// </summary>
    private void SpawnAndBounceItem(ItemData data, Vector3 spawnPosition)
    {
        GameObject spawnedItem = Instantiate(data.Item_Pick_Prefab, spawnPosition, Quaternion.identity);

        if (spawnedItem.TryGetComponent<DroppedItem>(out var droppedItemScript))
        {
            // 1. ส่ง ItemData ให้ไอเทมที่เกิดมา
            droppedItemScript.SetItemData(data);

            // 2. ส่งแรงเด้งน้ำพุ
            Vector2 randomCircle = Random.insideUnitCircle.normalized * maxHorizontalForce;
            float randomUpForce = Random.Range(minUpwardForce, maxUpwardForce);
            Vector3 bounceForce = new Vector3(randomCircle.x, randomUpForce, randomCircle.y);

            droppedItemScript.ApplyInitialBounce(bounceForce);
        }
        else
        {
            Debug.LogWarning($"Prefab ใน {data.itemName} ไม่มีสคริปต์ DroppedItem ติดอยู่!");
        }
    }

    // ** ฟังก์ชันตัวอย่างสำหรับทดสอบกดดรอปใน Editor **
    [ContextMenu("Test Drop All Items")]
    private void TestDrop()
    {
        DropItems();
    }
}