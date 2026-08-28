using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageSelect : MonoBehaviour
{
    public int Stage;
    public TextMeshProUGUI TextMeshProUGUI;
    public GameObject[] Mark_Select;
    private void Start()
    {
        Stage = PlayerPrefs.GetInt("STAGE_SELECT",1);

        foreach (GameObject go in Mark_Select)
        {
            go.SetActive(false);
        }
        Mark_Select[Stage - 1].SetActive(true);
    }

    public void SetStage(int stage)
    {
        Stage = stage;
        PlayerPrefs.SetInt("STAGE_SELECT", stage);
        TextMeshProUGUI.text = stage.ToString();
        foreach (GameObject go in Mark_Select)
        {
            go.SetActive(false);
        }
        Mark_Select[Stage - 1].SetActive(true);
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Stage"+Stage);
    }
}
