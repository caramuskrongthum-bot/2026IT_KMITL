using UnityEngine;

public class SkinManager : MonoBehaviour
{
    public GameObject[] Sword_Skin;
    void Start()
    {
        foreach (var sword in Sword_Skin)
        {
            sword.SetActive(false);
        }
        Sword_Skin[PlayerPrefs.GetInt("SWORD_SKIN_SELECT", 0)].SetActive(true);
        Debug.Log(PlayerPrefs.GetInt("SWORD_SKIN_SELECT"));
    }
}
