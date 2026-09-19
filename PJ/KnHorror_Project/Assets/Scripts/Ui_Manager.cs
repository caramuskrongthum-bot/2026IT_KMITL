using UnityEngine;
using UnityEngine.UI;

public class Ui_Manager : MonoBehaviour
{
    public Image Int_Button;

    private void Start()
    {
        Int_Button.color = new Color(0.61f, 0.61f, 0.61f);
    }
    public void EnterCanInteract()
    {
        Int_Button.color = new Color(255, 255, 255);
    }
    public void ExitCanInteract()
    {
        Int_Button.color = new Color(0.61f, 0.61f, 0.61f);
    }
}
