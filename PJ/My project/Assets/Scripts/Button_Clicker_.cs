using UnityEngine;
using UnityEngine.UI;

public class Button_Clicker_ : MonoBehaviour
{
    public void ClickButton()
    {
        Button Btn = GetComponent<Button>();
        Btn.onClick.Invoke();
    }
}
