using UnityEngine;

// สร้างไอเทมใหม่ได้จากเมนู Assets > Create > Item System > Item Data
[CreateAssetMenu(fileName = "New Item", menuName = "Item System/Item Data")]
public class ItemData : ScriptableObject
{
    [Header("Item Info")]
    public string ITEM_NAME;
    public int ITEM_VALUE;

    [Header("Visual (Optional)")]
    public GameObject itemPrefab; // โมเดล/พรีแฟบของไอเทมนี้ (ใช้ตอน Drop)
    public Sprite icon;           // ไอคอนสำหรับ UI (ถ้ามี)
}
