using UnityEngine;

public class Footstep : MonoBehaviour
{
    public AudioSource AudioSource;
    public AudioClip[] audioClips;

    public void PlayFootStep()
    {
        int A = Random.Range(0, audioClips.Length);
       AudioSource.PlayOneShot(audioClips[A]);
       AudioSource.PlayOneShot(audioClips[A]);
       AudioSource.PlayOneShot(audioClips[A]);
    }
}
