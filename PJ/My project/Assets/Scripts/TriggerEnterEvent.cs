using UnityEngine;
using UnityEngine.Events;

public class TriggerEnterEvent : MonoBehaviour
{
    public UnityEvent UnityEvent;
    public string TriggerName;
    public void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag(TriggerName))
        {
            UnityEvent.Invoke();
        }
    }
}
