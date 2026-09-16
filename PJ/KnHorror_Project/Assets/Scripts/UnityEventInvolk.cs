using UnityEngine;
using UnityEngine.Events;

public class UnityEventInvolk : MonoBehaviour
{
    public UnityEvent Event_01;
    public UnityEvent Event_02;
    public void DoEvent_1()
    {
        Event_01.Invoke();
    }
    public void DoEvent_2()
    {
        Event_02.Invoke();
    }
}
