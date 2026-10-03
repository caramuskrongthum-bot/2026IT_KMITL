using UnityEngine;
using UnityEngine.UI;

public class Perk_Button : MonoBehaviour
{
    public string Perk_Name;
    public Button Perk_Ultimate_Button;
    void Start()
    {
        if (PlayerPrefs.GetInt(Perk_Name, 0) == 1)
        {
            Perk_Ultimate_Button.interactable = true;
        }
    }
}
