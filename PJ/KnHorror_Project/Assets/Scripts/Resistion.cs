using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Resistion : MonoBehaviour
{
    public Slider UiSlider;
    public UnityEvent ResistionEvent;
    void Update()
    {
        UiSlider.value += 0.01f;
        UiSlider.value = Mathf.Clamp(UiSlider.value, 0, UiSlider.maxValue);
    }

    public void Resistion_Press()
    {
        UiSlider.value += 5f;
        UiSlider.value = Mathf.Clamp(UiSlider.value, 0, UiSlider.maxValue);
        if (UiSlider.value == UiSlider.maxValue)
        {
            GameObject P = GameObject.FindGameObjectWithTag("Player").gameObject;
            P.transform.parent = null;
            UiSlider.value = 0f;
            ResistionEvent.Invoke();
        }
    }
}
