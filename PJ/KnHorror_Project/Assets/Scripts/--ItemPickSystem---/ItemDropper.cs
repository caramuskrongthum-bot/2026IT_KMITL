using UnityEngine;

public class ItemDropper : MonoBehaviour
{
    [Header("Item Config for Events")]
    public ItemData targetItemData;

    [Header("Base Prefab Settings")]
    [Tooltip("ลาก Prefab กลางตัวเดียวที่จะใช้แทนไอเทมทุกชิ้นในเกมมาใส่ตรงนี้")]
    public GameObject baseItemPrefab;

    [Header("Bounce / Drop Force")]
    public float bounceForceMin = 2f;
    public float bounceForceMax = 4f;
    public float spreadForceMin = 1f;
    public float spreadForceMax = 3f;
    public float torqueForce = 5f;

    [Header("Physics Settings")]
    public float spawnHeightOffset = 0.3f;

    public void DropTargetItem()
    {
        if (targetItemData != null)
        {
            DropItem(targetItemData, transform.position);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] ItemDropper: ยังไม่ได้ใส่ targetItemData เลยค่ะคุณแม่!");
        }
    }

    public GameObject DropItem(ItemData itemData, Vector3 position)
    {
        if (itemData == null)
        {
            Debug.LogWarning("ItemDropper: itemData เป็น null");
            return null;
        }

        // 1. ตรวจสอบว่าใช้ Prefab เฉพาะชิ้น (ถ้ามีใส่ไว้ใน ItemData) หรือใช้ Prefab กลาง
        GameObject prefabToSpawn = (itemData.itemPrefab != null) ? itemData.itemPrefab : baseItemPrefab;

        if (prefabToSpawn == null)
        {
            Debug.LogError("ItemDropper: ไม่พบ Prefab สำหรับสร้างไอเทม (กรุณาตั้งค่า baseItemPrefab บน ItemDropper)");
            return null;
        }

        // 2. Instantiate ไอเทมออกมา
        Vector3 spawnPos = position + Vector3.up * spawnHeightOffset;
        GameObject dropped = Instantiate(prefabToSpawn, spawnPos, Random.rotation);

        // 3. กำหนดค่า ItemData ให้กับ ItemPickup[cite: 3, 4]
        ItemPickup pickup = dropped.GetComponent<ItemPickup>();
        if (pickup == null)
        {
            pickup = dropped.AddComponent<ItemPickup>();
        }
        pickup.Initialize(itemData); // เรียกฟังก์ชัน setup รูปภาพและข้อมูล[cite: 2, 4]

        // 4. คำนวณแรงสะท้อน/กระเด้ง[cite: 3]
        Rigidbody rb = dropped.GetComponent<Rigidbody>();
        if (rb != null)
        {
            ApplyBounceForce(rb);
        }

        return dropped;
    }

    private void ApplyBounceForce(Rigidbody rb)
    {
        float bounce = Random.Range(bounceForceMin, bounceForceMax);
        float spread = Random.Range(spreadForceMin, spreadForceMax);

        Vector2 randomDir2D = Random.insideUnitCircle.normalized;
        Vector3 spreadDir = new Vector3(randomDir2D.x, 0f, randomDir2D.y);

        Vector3 force = Vector3.up * bounce + spreadDir * spread;
        rb.AddForce(force, ForceMode.Impulse);

        Vector3 randomTorque = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ) * torqueForce;
        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }
}