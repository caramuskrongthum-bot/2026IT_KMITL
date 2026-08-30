using TMPro;
using UnityEngine;

public class PlayerPrefDisplay : MonoBehaviour
{
    public string DataName;
    public TextMeshProUGUI TextMeshProUGUI;
    private void Update()
    {
        TextMeshProUGUI.text = PlayerPrefs.GetInt(DataName).ToString();
    }
}
