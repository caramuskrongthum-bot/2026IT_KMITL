using StarterAssets;
using UnityEngine;
using UnityEngine.Events;

public class InteractiveObject : MonoBehaviour
{
    [Header("Settings")]
    public bool isOneTimeUse = false;
    public float interactionCooldown = 1f;
    private bool canInteract = true;
    private bool hasBeenUsed = false;
    private bool isPlayerInZone = false;
    private Player_Basic_Controller currentPlayerController;

    [Header("UI References")]
    public GameObject uiInteractable; // ✨ จะปิดเมื่อเข้าเขต และเปิดเมื่อออกจากเขต
    public GameObject KeyDisplay;     // ✨ จะเปิดเมื่อเข้าเขต และปิดเมื่อออกจากเขต

    [Header("Events")]
    public UnityEvent onPlayerInteracted;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (isOneTimeUse && hasBeenUsed) return;

            isPlayerInZone = true;
            currentPlayerController = other.GetComponent<Player_Basic_Controller>();

            // ✨ เมื่อเข้าเขต: เปิด KeyDisplay และปิด uiInteractable
            if (KeyDisplay != null)
                KeyDisplay.SetActive(true);

            if (uiInteractable != null)
                uiInteractable.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInZone = false;
            currentPlayerController = null;

            // ✨ เมื่อออกจากเขต: ปิด KeyDisplay และเปิด uiInteractable กลับมา
            if (KeyDisplay != null)
                KeyDisplay.SetActive(false);

            if (uiInteractable != null)
                uiInteractable.SetActive(true);
        }
    }

    private void Update()
    {
        if (!isPlayerInZone || !canInteract || currentPlayerController == null) return;
        if (isOneTimeUse && hasBeenUsed) return;

        if (currentPlayerController.PressingFire1)
        {
            TriggerInteraction();
        }
    }

    private void TriggerInteraction()
    {
        if (!canInteract) return;
        if (isOneTimeUse && hasBeenUsed) return;

        // ปิด KeyDisplay ทันทีที่กดโต้ตอบ
        if (KeyDisplay != null)
            KeyDisplay.SetActive(false);

        // สั่งทำงาน Event
        onPlayerInteracted.Invoke();

        if (isOneTimeUse)
        {
            hasBeenUsed = true;
            isPlayerInZone = false;

            // ถ้าใช้แล้วหมดไปเลย ให้เปิด uiInteractable กลับมา (หรือจะปล่อยปิดไว้ตามใจชอบได้เลยนะแม่)
            if (uiInteractable != null)
                uiInteractable.SetActive(true);

            return;
        }

        StartCoroutine(CooldownRoutine());
    }

    private System.Collections.IEnumerator CooldownRoutine()
    {
        canInteract = false;
        yield return new WaitForSeconds(interactionCooldown);
        canInteract = true;

        // เปิด KeyDisplay กลับมาถ้าผู้เล่นยังยืนอยู่ในเขต
        if (isPlayerInZone && KeyDisplay != null)
        {
            KeyDisplay.SetActive(true);
        }
    }

    public void AddmedicineInventory()
    {
        ItemCountButton itemCountButton = GameObject.FindGameObjectWithTag("medicine").GetComponent<ItemCountButton>();
        itemCountButton.AddItem(1);
    }
}