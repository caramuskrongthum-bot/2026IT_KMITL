using UnityEngine;
using UnityEngine.Events;

public class UnityEvent_ : MonoBehaviour
{
    public UnityEvent Event;
    public UnityEvent Event2;
    public UnityEvent Event3;
    public void DoEvent()
    {
        Event.Invoke();
    }
    public void DoEvent2()
    {
        Event2.Invoke();
    }

    public void DoEvent3()
    {
        Event3.Invoke();
    }

    public void DestroyMe()
    {
        Destroy(gameObject);
    }
}
