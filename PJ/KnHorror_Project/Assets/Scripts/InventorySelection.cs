using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InventorySelection : MonoBehaviour
{
    public PlayerInventoryHandler inventoryHandler;

    void Update()
    {
        if (inventoryHandler == null || Keyboard.current == null) return;

        // เช็กการกดปุ่มด้วย Keyboard.current ของ Input System ตัวใหม่
        if (Keyboard.current.digit1Key.wasPressedThisFrame) inventoryHandler.EquipSlot(0);
        else if (Keyboard.current.digit2Key.wasPressedThisFrame) inventoryHandler.EquipSlot(1);
        else if (Keyboard.current.digit3Key.wasPressedThisFrame) inventoryHandler.EquipSlot(2);
    }
}