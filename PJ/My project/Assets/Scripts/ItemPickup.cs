using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item Settings")]
    [Tooltip("ใส่ ScriptableObject ของไอเทมชิ้นนี้")]
    public ItemData itemData;

    [Header("Interaction Settings")]
    [Tooltip("true = เดินชนแล้วเก็บเลย / false = ต้องกดปุ่มตาม keyToInteract ถึงจะเก็บ")]
    public bool autoPickupOnTrigger = false;
    public KeyCode keyToInteract = KeyCode.E;

    private bool isPlayerInRange = false;
    private PlayerInventory playerInventory;

    private void Update()
    {
        if (!isPlayerInRange || autoPickupOnTrigger) return;

        if (Input.GetKeyDown(keyToInteract))
        {
            TryPickupItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInventory = other.GetComponent<PlayerInventory>();

            if (playerInventory != null)
            {
                isPlayerInRange = true;

                if (autoPickupOnTrigger)
                {
                    TryPickupItem();
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            playerInventory = null;
        }
    }

    private void TryPickupItem()
    {
        if (playerInventory == null || itemData == null) return;

        bool wasPickedUp = playerInventory.AddItem(itemData);

        if (wasPickedUp)
        {
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("เก็บไอเทมไม่ได้เนื่องจากกระเป๋าเต็ม!");
        }
    }
}