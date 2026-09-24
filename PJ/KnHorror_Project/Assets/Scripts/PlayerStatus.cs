using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    [Header("UI & References")]
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
    public float lerpSpeed = 5f;
    private float targetHealth;

    [Header("Invincible Shield Settings")]
    public bool isInvincible = false;
    public GameObject VFX_Shield;

    private bool isDead = false;

    void Start()
    {
        // ✨ ดึงค่าเลือดเริ่มต้นจาก GameManager ถ้ามี
        if (GameManager.Instance != null)
        {
            targetHealth = GameManager.Instance.startingHealth;
        }
        else
        {
            targetHealth = 100f;
        }

        if (HealthBar != null)
        {
            HealthBar.maxValue = 100f; // หรือตั้งตามต้องการ
            HealthBar.value = targetHealth;
        }
    }

    void Update()
    {
        if (HealthBar != null)
        {
            if (targetHealth <= 0f)
            {
                HealthBar.value = 0f;
            }
            else
            {
                HealthBar.value = Mathf.Lerp(HealthBar.value, targetHealth, Time.deltaTime * lerpSpeed);
            }
        }

        // ✨ สลับค่า Volume Weight ให้ตรงข้าม: เลือดเต็ม (100) = Volume เป็น 0, เลือดหมด (0) = Volume เป็น 1 (เต็มแมกซ์)
        if (Volume != null)
        {
            float targetVolumeWeight = 1f - (HealthBar.value / 100f);
            Volume.weight = Mathf.Lerp(Volume.weight, targetVolumeWeight, Time.deltaTime * 3);
        }

        Animator animator = GetComponent<Animator>();
        if (animator != null)
        {
            float targetLayerWeight = (HealthBar.value <= 50f) ? 1f : 0f;
            float currentWeight = animator.GetLayerWeight(1);
            animator.SetLayerWeight(1, Mathf.Lerp(currentWeight, targetLayerWeight, Time.deltaTime * lerpSpeed));
        }
    }

    public void StartInvincibleShield(float duration = 5f)
    {
        isDead = false;

        if (targetHealth <= 0)
        {
            targetHealth = 30f;
        }

        StartCoroutine(InvincibleRoutine(duration));
    }

    private IEnumerator InvincibleRoutine(float duration)
    {
        isInvincible = true;

        if (VFX_Shield != null)
        {
            VFX_Shield.SetActive(true);
        }

        yield return new WaitForSeconds(duration);

        isInvincible = false;

        if (VFX_Shield != null)
        {
            VFX_Shield.SetActive(false);
        }
    }

    public void DamageToPlayer(int damage)
    {
        if (isInvincible || isDead || (HealthBar != null && HealthBar.value < 1f)) return;

        if (VFX_Blood != null && VFX_Point != null)
        {
            Instantiate(VFX_Blood).transform.position = VFX_Point.transform.position;
        }

        targetHealth -= damage;
        targetHealth = Mathf.Clamp(targetHealth, 0, 100);

        if (SFX_Source != null && SFX_Damage != null)
        {
            SFX_Source.PlayOneShot(SFX_Damage);
        }

        if (targetHealth == 0 && !isDead)
        {
            StartCoroutine(HandlePlayerDownRoutine());
            isDead = true;

            Animator animator = GetComponent<Animator>();
            if (animator != null) animator.Play("Down");

            Down.Invoke();

            if (HoodBlackPrefab != null)
            {
                GameObject hood = Instantiate(HoodBlackPrefab);
                hood.transform.position = Vector3.zero;
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
        Animator animator = GetComponent<Animator>();
        animator.applyRootMotion = true;
        yield return new WaitForSeconds(1.5f);
        animator.applyRootMotion = false;
    }

    public void HealToPlayer(int heal)
    {
        if (isDead) return;

        targetHealth += heal;
        targetHealth = Mathf.Clamp(targetHealth, 0, 100);

        if (targetHealth > 30)
        {
            if (SFX_HeartBeat != null) SFX_HeartBeat.Stop();
        }
    }

    public void GotCarry(GameObject target)
    {
        Animator animator = GetComponent<Animator>();
        if (animator != null) animator.Play("P_Carry");

        transform.position = target.transform.position;
        transform.rotation = target.transform.rotation;
        transform.parent = target.transform.parent;
    }

    public bool IsPlayerDead()
    {
        return isDead || targetHealth <= 0f;
    }
}