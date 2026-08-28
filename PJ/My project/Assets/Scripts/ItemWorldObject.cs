using UnityEngine;

public class ItemWorldObject : MonoBehaviour
{
    public ItemData itemData;

    public void Setup(ItemData newItemData)
    {
        itemData = newItemData;
    }
}