using System.Collections.Generic;
using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventorySelection : MonoBehaviour
{
    public PlayerInventoryHandler inventoryHandler;

    [Header("Input Actions (Buttons)")]
    public InputActionReference slot1Action;
    public InputActionReference slot2Action;

    public ThirdPersonController T;
    private void OnEnable()
    {
        if (slot1Action != null && slot1Action.action != null)
        {
            slot1Action.action.Enable();
            slot1Action.action.performed += OnSlot1Performed;
        }

        if (slot2Action != null && slot2Action.action != null)
        {
            slot2Action.action.Enable();
            slot2Action.action.performed += OnSlot2Performed;
        }
    }

    private void OnDisable()
    {
        if (slot1Action != null && slot1Action.action != null)
        {
            slot1Action.action.performed -= OnSlot1Performed;
            slot1Action.action.Disable();
        }

        if (slot2Action != null && slot2Action.action != null)
        {
            slot2Action.action.performed -= OnSlot2Performed;
            slot2Action.action.Disable();
        }
    }

    void Update()
    {
        if (inventoryHandler == null) return;

        // เช็คการเลื่อน Scroll เมาส์ตรงๆ ผ่าน Mouse.current ตัวใหม่ (ไม่ต้องพึ่ง InputAction)
        if (Mouse.current != null && T.CanMove == true && T.IsCatching == false)
        {
            Vector2 scrollValue = Mouse.current.scroll.ReadValue();

            if (scrollValue.y > 0f)
            {
                // เลื่อนเมาส์ขึ้น -> เลือกช่อง 1 (Index 0)
                inventoryHandler.EquipSlot(0);
            }
            else if (scrollValue.y < 0f)
            {
                // เลื่อนเมาส์ลง -> เลือกช่อง 2 (Index 1)
                inventoryHandler.EquipSlot(1);
            }
        }
    }

    private void OnSlot1Performed(InputAction.CallbackContext context)
    {
        if (inventoryHandler != null && T.CanMove == true && T.IsCatching == false) inventoryHandler.EquipSlot(0);
    }

    private void OnSlot2Performed(InputAction.CallbackContext context)
    {
        if (inventoryHandler != null && T.CanMove == true && T.IsCatching == false) inventoryHandler.EquipSlot(1);
    }
}