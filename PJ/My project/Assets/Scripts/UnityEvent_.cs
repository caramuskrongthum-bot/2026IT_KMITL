using UnityEngine;
using UnityEngine.Events;

public class UnityEvent_ : MonoBehaviour
{
    public UnityEvent Event;
    public UnityEvent Event2;
    public void DoEvent()
    {
        Event.Invoke();
    }
    public void DoEvent2()
    {
        Event2.Invoke();
    }

    public void DestroyMe()
    {
        Destroy(gameObject);
    }
}
