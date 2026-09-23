using UnityEngine;

public class SpawnPrefab : MonoBehaviour
{
    public void SpawnPrefabHere(GameObject prefab)
    {
        Instantiate(prefab).transform.position = transform.position;
    }
}
