using UnityEngine;
using UnityEngine.Events;

public class UnityEventInvolk : MonoBehaviour
{
    public UnityEvent Event_01;
    public UnityEvent Event_02;
    public UnityEvent Event_03;
    public UnityEvent Event_04;
    public UnityEvent Event_05;
    public void DoEvent_1()
    {
        Event_01.Invoke();
    }
    public void DoEvent_2()
    {
        Event_02.Invoke();
    }
    public void DoEvent_3()
    {
        Event_03.Invoke();
    }
    public void DoEvent_4()
    {
        Event_04.Invoke();
    }
    public void DoEvent_5()
    {
        Event_05.Invoke();
    }
    public void DestroyMe()
    {
        Destroy(gameObject);
    }
}
