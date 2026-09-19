using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemCountButton : MonoBehaviour
{
    public Button Button;
    public int Count = 0;
    public TextMeshProUGUI textMeshPro;
    private void Start()
    {
        textMeshPro.text = "X" + Count.ToString();
    }
    void Update()
    {
        if (Count == 0)
        {
            Button.interactable = false;
        }
        else
        {
            Button.interactable = true;
        }
    }
    public void AddItem(int index)
    {
        Count += index;
        textMeshPro.text = "X" + Count.ToString();
    }
    public void RemoveItem(int index)
    {
        Count -= index;
        textMeshPro.text = "X" + Count.ToString();
    }
}
