using UnityEngine;
using Unity.AI.Navigation;
using System.Collections;

public class RuntimeNavMeshManager : MonoBehaviour
{
    public static RuntimeNavMeshManager Instance;

    public NavMeshSurface surface;

    private Coroutine buildCoroutine;

    private void Awake()
    {
        Instance = this;

        if (surface == null)
            surface = GetComponent<NavMeshSurface>();
    }

    public void RebuildNavMesh()
    {
        if (buildCoroutine != null)
            StopCoroutine(buildCoroutine);

        buildCoroutine = StartCoroutine(BuildNavMeshRoutine());
    }

    private IEnumerator BuildNavMeshRoutine()
    {
        yield return null;

        surface.BuildNavMesh();

        buildCoroutine = null;

        Debug.Log("NavMesh Build Complete");
    }
}