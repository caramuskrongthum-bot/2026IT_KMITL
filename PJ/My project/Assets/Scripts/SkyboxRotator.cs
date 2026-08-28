using UnityEngine;

public class SkyboxRotator : MonoBehaviour
{
    [SerializeField] private float rotationSpeed = 1.2f;

    void Update()
    {
        // Rotates the skybox around the Y-axis over time
        RenderSettings.skybox.SetFloat("_Rotation", Time.time * rotationSpeed);
    }
}