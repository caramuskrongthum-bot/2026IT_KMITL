using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class PlayerStatus : MonoBehaviour
{
    public Slider HealthBar;
    public UnityEvent Down;
    public GameObject VFX_Blood;
    public Transform VFX_Point;
    public AudioSource SFX_Source;
    public AudioSource SFX_HeartBeat;
    public AudioClip SFX_Damage;
    public Volume Volume;

    [Header("Smooth Settings")]
    public float lerpSpeed = 5f; // ความเร็วในการเลื่อนหลอดเลือดและเอฟเฟกต์
    private float targetHealth;

    void Start()
    {
        if (HealthBar != null)
            targetHealth = HealthBar.value;
    }

    void Update()
    {
        if (HealthBar != null)
        {
            // ทำให้เลือดลด/เพิ่มแบบสมูท (Smooth Lerp)
            HealthBar.value = Mathf.Lerp(HealthBar.value, targetHealth, Time.deltaTime * lerpSpeed);
        }

        // ทำให้ Volume (Post-processing) เปลี่ยนความหนักหน่วงแบบสมูท
        if (Volume != null)
        {
            float targetVolumeWeight = 0f;
            if (HealthBar.value <= 30f) targetVolumeWeight = 1f;
            else if (HealthBar.value <= 50f) targetVolumeWeight = 0.5f;

            Volume.weight = Mathf.Lerp(Volume.weight, targetVolumeWeight, Time.deltaTime * lerpSpeed);
        }

        // ทำให้ Animator Layer Weight เปลี่ยนแบบสมูท
        Animator A = GetComponent<Animator>();
        if (A != null)
        {
            float targetLayerWeight = (HealthBar.value <= 50f) ? 1f : 0f;
            float currentWeight = A.GetLayerWeight(1);
            A.SetLayerWeight(1, Mathf.Lerp(currentWeight, targetLayerWeight, Time.deltaTime * lerpSpeed));
        }
    }

    public void DamageToPlayer(int Damage)
    {
        Instantiate(VFX_Blood).transform.position = VFX_Point.transform.position;

        targetHealth -= Damage;
        targetHealth = Mathf.Clamp(targetHealth, 0, 100);

        SFX_Source.PlayOneShot(SFX_Damage);

        if (targetHealth <= 0)
        {
            Animator A = GetComponent<Animator>();
            A.Play("Down");
            Down.Invoke();
        }

        if (targetHealth <= 30)
        {
            if (!SFX_HeartBeat.isPlaying) SFX_HeartBeat.Play();
        }
        else
        {
            SFX_HeartBeat.Stop();
        }
    }

    public void HealToPlayer(int Heal)
    {
        targetHealth += Heal;
        targetHealth = Mathf.Clamp(targetHealth, 0, 100);

        if (targetHealth > 30)
        {
            SFX_HeartBeat.Stop();
        }
    }

    public void GotCarry(GameObject B)
    {
        Animator A = GetComponent<Animator>();
        A.Play("P_Carry");
        transform.position = B.transform.position;
        transform.rotation = B.transform.rotation;
        transform.parent = B.transform;
    }
}