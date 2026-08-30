using UnityEngine;

public class DontDestroyThis : MonoBehaviour
{
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
