using UnityEngine;

public class mouse_Lock_Unlock : MonoBehaviour
{
    public bool Lock;
    public bool Lock_On_Start = true;
    void Start()
    {
        if (Lock_On_Start)
        {
            if (Lock)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }
    void OnEnable()
    {
        if (Lock_On_Start)
        {
            if (Lock)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
        }
    }

    public void Lock_()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void UnLock_()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
