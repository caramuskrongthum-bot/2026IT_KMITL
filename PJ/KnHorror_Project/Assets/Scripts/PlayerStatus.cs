using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;
using System.Collections;

public class PlayerStatus : MonoBehaviour
{
    public Slider HealthBar; //[cite: 6]
    public UnityEvent Down; //[cite: 6]
    public GameObject VFX_Blood; //[cite: 6]
    public Transform VFX_Point; //[cite: 6]
    public AudioSource SFX_Source; //[cite: 6]
    public AudioSource SFX_HeartBeat; //[cite: 6]
    public AudioClip SFX_Damage; //[cite: 6]
    public Volume Volume; //[cite: 6]
    public GameObject HoodBlackPrefab; //[cite: 6]
    public PlayerInventoryHandler PlayerInventoryHandler; //[cite: 6]

    [Header("Smooth Settings")]
    public float lerpSpeed = 5f; // ความเร็วในการเลื่อนหลอดเลือดและเอฟเฟกต์[cite: 6]
    private float targetHealth; //[cite: 6]

    [Header("Invincible Shield Settings")]
    public bool isInvincible = false; // เช็คว่ากำลังอยู่ในสถานะอมตะหรือไม่
    public GameObject VFX_Shield;    // (Optional) VFX โล่ป้องกันสวยๆ ใส่หรือไม่ใส่ก็ได้ค่ะ

    // 👑 ตัวแปรเช็คสถานะ เพื่อให้ฝั่งมอนสเตอร์เช็คได้ทันที
    private bool isDead = false; //[cite: 6]

    void Start()
    {
        if (HealthBar != null)
        {
            targetHealth = HealthBar.value; //[cite: 6]
        }
    }

    void Update()
    {
        if (HealthBar != null)
        {
            // ถ้า targetHealth เป็น 0 ให้หลอดเลือดดิ่งลง 0 ทันที ไม่ให้คาอยู่เศษ 1
            if (targetHealth <= 0f)
            {
                HealthBar.value = 0f; //[cite: 6]
            }
            else
            {
                // ทำให้เลือดลด/เพิ่มแบบสมูท (Smooth Lerp ปกติ)
                HealthBar.value = Mathf.Lerp(HealthBar.value, targetHealth, Time.deltaTime * lerpSpeed); //[cite: 6]
            }
        }

        // ทำให้ Volume (Post-processing) เปลี่ยนความหนักหน่วงแบบสมูท
        if (Volume != null)
        {
            float targetVolumeWeight = 0f;
            if (HealthBar.value <= 30f) targetVolumeWeight = 1f; //[cite: 6]
            else if (HealthBar.value <= 50f) targetVolumeWeight = 0.5f; //[cite: 6]

            Volume.weight = Mathf.Lerp(Volume.weight, targetVolumeWeight, Time.deltaTime * 3); //[cite: 6]
        }

        // ทำให้ Animator Layer Weight เปลี่ยนแบบสมูท
        Animator A = GetComponent<Animator>();
        if (A != null)
        {
            float targetLayerWeight = (HealthBar.value <= 50f) ? 1f : 0f; //[cite: 6]
            float currentWeight = A.GetLayerWeight(1); //[cite: 6]
            A.SetLayerWeight(1, Mathf.Lerp(currentWeight, targetLayerWeight, Time.deltaTime * lerpSpeed)); //[cite: 6]
        }
    }

    // ----------------------------------------------------
    // 🛡️ PUBLIC METHOD: เรียกใช้โล่อมตะ 5 วินาที
    // ----------------------------------------------------
    public void StartInvincibleShield(float duration = 5f)
    {
        // ปลดสถานะตาย (กรณีขัดขืนสำเร็จแล้วกลับมาเคลื่อนไหว)
        isDead = false;

        // เติมเลือดคืนนิดหน่อย หรือรีเซ็ตค่าเพื่อให้เล่นต่อได้
        if (targetHealth <= 0)
        {
            targetHealth = 30f; // ตั้งเลือดเริ่มต้นหลังหลุดขัดขืน (ปรับตัวเลขได้ตามใจชอบค่ะ)
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

        Debug.Log($"[PlayerStatus] เริ่มใช้งานโล่อมตะเป็นเวลา {duration} วินาที!");

        yield return new WaitForSeconds(duration);

        isInvincible = false;

        if (VFX_Shield != null)
        {
            VFX_Shield.SetActive(false);
        }

        Debug.Log("[PlayerStatus] โล่อมตะหมดเวลาแล้ว!");
    }

    public void DamageToPlayer(int Damage)
    {
        // 🛑 ถ้าติดสถานะอมตะอยู่ (isInvincible) จะไม่รับความเสียหายใดๆ ทั้งสิ้น!
        if (isInvincible || isDead || (HealthBar != null && HealthBar.value < 1f)) return;

        if (VFX_Blood != null && VFX_Point != null)
        {
            Instantiate(VFX_Blood).transform.position = VFX_Point.transform.position; //[cite: 6]
        }

        targetHealth -= Damage; //[cite: 6]
        targetHealth = Mathf.Clamp(targetHealth, 0, 100); //[cite: 6]

        if (SFX_Source != null && SFX_Damage != null)
        {
            SFX_Source.PlayOneShot(SFX_Damage); //[cite: 6]
        }

        if (targetHealth == 0 && !isDead)
        {
            StartCoroutine(HandlePlayerDownRoutine()); //[cite: 6]
            isDead = true; //[cite: 6]
            Animator A = GetComponent<Animator>();
            if (A != null) A.Play("Down"); //[cite: 6]
            Down.Invoke(); //[cite: 6]

            if (HoodBlackPrefab != null)
            {
                GameObject B = Instantiate(HoodBlackPrefab).gameObject; //[cite: 6]
                B.transform.position = Vector3.zero; //[cite: 6]
            }
        }

        if (targetHealth <= 30)
        {
            if (SFX_HeartBeat != null && !SFX_HeartBeat.isPlaying) SFX_HeartBeat.Play(); //[cite: 6]
        }
        else
        {
            if (SFX_HeartBeat != null) SFX_HeartBeat.Stop(); //[cite: 6]
        }
    }

    private IEnumerator HandlePlayerDownRoutine()
    {
        Animator A = GetComponent<Animator>();
        A.applyRootMotion = true; //[cite: 6]
        yield return new WaitForSeconds(1.5f); //[cite: 6]
        A.applyRootMotion = false; //[cite: 6]
    }

    public void HealToPlayer(int Heal)
    {
        if (isDead) return; //[cite: 6]
        targetHealth += Heal; //[cite: 6]
        targetHealth = Mathf.Clamp(targetHealth, 0, 100); //[cite: 6]

        if (targetHealth > 30)
        {
            if (SFX_HeartBeat != null) SFX_HeartBeat.Stop(); //[cite: 6]
        }
    }

    public void GotCarry(GameObject B)
    {
        Animator A = GetComponent<Animator>();
        if (A != null) A.Play("P_Carry"); //[cite: 6]
        transform.position = B.transform.position; //[cite: 6]
        transform.rotation = B.transform.rotation; //[cite: 6]
        transform.parent = B.transform.parent; //[cite: 6]
    }

    public bool IsPlayerDead()
    {
        return isDead || targetHealth <= 0f; //[cite: 6]
    }
}