using UnityEngine;

public class ParticlePlayer : MonoBehaviour
{
    public ParticleSystem[] VFX;

    public void PlayerParticle(int index)
    {
        VFX[index].Play();
    }
}
