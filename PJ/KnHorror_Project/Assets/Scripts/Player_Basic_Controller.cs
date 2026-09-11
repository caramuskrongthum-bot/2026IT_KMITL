using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Basic_Controller : MonoBehaviour
{
    public InputActionReference Fire1;

    public bool PressingFire1 { get; private set; }

    private void OnEnable()
    {
        if (Fire1 != null)
        {
            Fire1.action.started += OnFire1Started;
            Fire1.action.canceled += OnFire1Canceled;
            Fire1.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (Fire1 != null)
        {
            Fire1.action.started -= OnFire1Started;
            Fire1.action.canceled -= OnFire1Canceled;
            Fire1.action.Disable();
        }
    }

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
}