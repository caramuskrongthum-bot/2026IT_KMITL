using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public int value;
    public Sprite icon;
    public GameObject Item_Pick_Prefab;
}