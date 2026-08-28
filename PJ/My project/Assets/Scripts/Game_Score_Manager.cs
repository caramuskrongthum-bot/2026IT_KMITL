using TMPro;
using UnityEngine;

public class Game_Score_Manager : MonoBehaviour
{
    public GameObject Alert_Prefab;
    public Transform Canvas;
    public void Get_Score(string Score_Event)
    {
        GameObject obj = Instantiate(Alert_Prefab);
        obj.transform.SetParent(Canvas.transform, false);
        TextMeshProUGUI text = obj.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        text.text = Score_Event;
    }
}
