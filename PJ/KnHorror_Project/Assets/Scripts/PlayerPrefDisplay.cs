using TMPro;
using UnityEngine;

public class PlayerPrefDisplay : MonoBehaviour
{
    public TextMeshProUGUI MoneyDisplay;
    void Update()
    {
        MoneyDisplay.text = PlayerPrefs.GetInt("MONEY_DATA").ToString();
    }
    public void RemoveData()
    {
        PlayerPrefs.DeleteAll();
    }
    public void AddMoney()
    {
        int currentMoney = PlayerPrefs.GetInt("MONEY_DATA", 0);
        currentMoney += 132;
        PlayerPrefs.SetInt("MONEY_DATA", currentMoney);
        PlayerPrefs.Save();
    }
}
