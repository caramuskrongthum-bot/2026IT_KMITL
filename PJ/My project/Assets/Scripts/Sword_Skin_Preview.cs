using UnityEngine;

public class Sword_Skin_Preview : MonoBehaviour
{
    public GameObject[] AllSwordSkin;
    public int index;
    public void Next()
    {
        index++;
        index = Mathf.Clamp(index,0,AllSwordSkin.Length);
        foreach (var sword in AllSwordSkin)
        {
            sword.SetActive(false);
        }
        AllSwordSkin[index].SetActive(true);
    }
    public void Back()
    {
        index--;
        index = Mathf.Clamp(index, 0, AllSwordSkin.Length);
        foreach (var sword in AllSwordSkin)
        {
            sword.SetActive(false);
        }
        AllSwordSkin[index].SetActive(true);
    }
    public void SelectSwordSkin()
    {
        PlayerPrefs.SetInt("SWORD_SKIN_SELECT", index);
    }
}
