using UnityEngine;
using System.Collections.Generic;

public class PlayerInventoryHandler : MonoBehaviour
{
    public List<GameObject> handItems = new List<GameObject>();

    private int currentEquippedIndex = -1;

    public GameObject Hand;

    void Start()
    {
        UnequipAll();
    }

    public void EquipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= handItems.Count) return;

        // กดซ้ำช่องเดิม = เก็บของเข้าตัว
        if (currentEquippedIndex == slotIndex)
        {
            UnequipAll();
            currentEquippedIndex = -1;
            return;
        }

        UnequipAll();

        if (handItems[slotIndex] != null)
        {
            handItems[slotIndex].SetActive(true);
            currentEquippedIndex = slotIndex;
        }
    }

    public void UnequipAll()
    {
        foreach (GameObject item in handItems)
        {
            if (item != null) item.SetActive(false);
        }
    }

    public void Attack_()
    {
        if (currentEquippedIndex == 0 && Hand.transform.localScale != Vector3.zero)
        {
            Animator animator = GetComponent<Animator>();
            animator.Play("Attack");
        }
    }

    public void ShowItemHand()
    {
        Hand.transform.localScale = Vector3.one;
    }
    public void HideItemHand()
    {
        Hand.transform.localScale = Vector3.zero;
    }
}
