using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class KeyActionEvent : MonoBehaviour
{
    public InputActionReference Key;
    public UnityEvent UnityEvent;

    private void OnEnable()
    {
        Key.action.Enable();
    }

    private void OnDisable()
    {
        Key.action.Disable();
    }

    public void Update()
    {
        if (Key.action.WasPressedThisFrame())
        {
            UnityEvent.Invoke();
        }
    }
}