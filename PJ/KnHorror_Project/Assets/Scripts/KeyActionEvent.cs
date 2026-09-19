using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class KeyActionEvent : MonoBehaviour
{
    public InputActionReference Key;
    public UnityEvent UnityEvent;
    public Button Button;
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
            if (Button != null && Button.interactable == true)
            {
                Button.onClick.Invoke();
            }
        }
    }
}