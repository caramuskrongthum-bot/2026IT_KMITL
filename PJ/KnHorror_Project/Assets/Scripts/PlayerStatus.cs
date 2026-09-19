using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;

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
    public GameObject HoodBlackPrefab;
    public PlayerInventoryHandler PlayerInventoryHandler;
    [Header("Smooth Settings")]
    public float lerpSpeed = 5f; // ความเร็วในการเลื่อนหลอดเลือดและเอฟเฟกต์
    private float targetHealth;

    // 👑 ตัวแปรเช็คสถานะ เพื่อให้ฝั่งมอนสเตอร์เช็คได้ทันที
    private bool isDead = false;

    void Start()
    {
        if (HealthBar != null)
        {
            targetHealth = HealthBar.value;
        }
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
        if (isDead || (HealthBar != null && HealthBar.value < 1f)) return;

        if (VFX_Blood != null && VFX_Point != null)
        {
            Instantiate(VFX_Blood).transform.position = VFX_Point.transform.position;
        }

        targetHealth -= Damage;
        targetHealth = Mathf.Clamp(targetHealth, 0, 100);

        if (SFX_Source != null && SFX_Damage != null)
        {
            SFX_Source.PlayOneShot(SFX_Damage);
        }

        if (targetHealth == 0 && !isDead)
        {
            StartCoroutine(HandlePlayerDownRoutine());
            isDead = true;
            Animator A = GetComponent<Animator>();
            if (A != null) A.Play("Down");
            Down.Invoke();

            if (HoodBlackPrefab != null)
            {
                GameObject B = Instantiate(HoodBlackPrefab).gameObject;
                B.transform.position = Vector3.zero;
            }
        }
        if (targetHealth <= 30)
        {
            if (SFX_HeartBeat != null && !SFX_HeartBeat.isPlaying) SFX_HeartBeat.Play();
        }
        else
        {
            if (SFX_HeartBeat != null) SFX_HeartBeat.Stop();
        }
    }

    private IEnumerator HandlePlayerDownRoutine()
    {
        Animator A = GetComponent<Animator>();
        A.applyRootMotion = true;
        yield return new WaitForSeconds(1.5f);
        A.applyRootMotion = false;
    }
    public void HealToPlayer(int Heal)
    {
        if (isDead) return;
        targetHealth += Heal;
        targetHealth = Mathf.Clamp(targetHealth, 0, 100);

        if (targetHealth > 30)
        {
            if (SFX_HeartBeat != null) SFX_HeartBeat.Stop();
        }
    }

    public void GotCarry(GameObject B)
    {
        Animator A = GetComponent<Animator>();
        if (A != null) A.Play("P_Carry");
        transform.position = B.transform.position;
        transform.rotation = B.transform.rotation;
        transform.parent = B.transform.parent;
    }

    // 👑 เมธอดเสริมให้มอนสเตอร์เช็คว่าผู้เล่นม่องเท่งหรือยังแบบชัวร์ๆ
    public bool IsPlayerDead()
    {
        return isDead || targetHealth <= 0f;
    }
}