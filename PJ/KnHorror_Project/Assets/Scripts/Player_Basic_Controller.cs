using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Basic_Controller : MonoBehaviour
{
    [Header("Fire 1 Settings")]
    public InputActionReference Fire1;
    public bool PressingFire1 { get; private set; }

    [Header("Fire 2 Settings (สำหรับเก็บของ / แอคชันเสริม)")]
    public InputActionReference Fire2;
    public bool PressingFire2 { get; private set; }

    private void OnEnable()
    {
        // เปิดใช้งาน Fire1
        if (Fire1 != null && Fire1.action != null)
        {
            Fire1.action.started += OnFire1Started;
            Fire1.action.canceled += OnFire1Canceled;
            Fire1.action.Enable();
        }

        // เปิดใช้งาน Fire2
        if (Fire2 != null && Fire2.action != null)
        {
            Fire2.action.started += OnFire2Started;
            Fire2.action.canceled += OnFire2Canceled;
            Fire2.action.Enable();
        }
    }

    private void OnDisable()
    {
        // ปิดใช้งาน Fire1
        if (Fire1 != null && Fire1.action != null)
        {
            Fire1.action.started -= OnFire1Started;
            Fire1.action.canceled -= OnFire1Canceled;
            Fire1.action.Disable();
        }

        // ปิดใช้งาน Fire2
        if (Fire2 != null && Fire2.action != null)
        {
            Fire2.action.started -= OnFire2Started;
            Fire2.action.canceled -= OnFire2Canceled;
            Fire2.action.Disable();
        }
    }

    #region Fire 1 Callbacks & Methods
    private void OnFire1Started(InputAction.CallbackContext context)
    {
        PressFire1();
    }

    private void OnFire1Canceled(InputAction.CallbackContext context)
    {
        ReleaseFire1();
    }

    public void PressFire1()
    {
        PressingFire1 = true;
    }

    public void ReleaseFire1()
    {
        PressingFire1 = false;
    }
    #endregion

    #region Fire 2 Callbacks & Methods
    private void OnFire2Started(InputAction.CallbackContext context)
    {
        PressFire2();
    }

    private void OnFire2Canceled(InputAction.CallbackContext context)
    {
        ReleaseFire2();
    }

    public void PressFire2()
    {
        PressingFire2 = true;
    }

    public void ReleaseFire2()
    {
        PressingFire2 = false;
    }
    #endregion
}