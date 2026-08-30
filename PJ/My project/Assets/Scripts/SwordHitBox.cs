using UnityEngine;

public class SwordHitBox : MonoBehaviour
{
    [Header("Collider Reference")]
    [SerializeField] private Collider swordCollider;

    [Header("Swing Speed Detection Settings")]
    [Tooltip("ความเร็วในการตวัดดาบขั้นต่ำที่จะเปิดใช้งาน HitBox")]
    public float swingSpeedThreshold = 2.5f;

    [Tooltip("ความเร็วในการตวัดดาบสูงสุดที่ใช้คำนวณโบนัสดาเมจเต็ม (เช่น เหวี่ยงเร็ว 10 m/s ได้โบนัสเต็ม)")]
    public float maxSwingSpeed = 10f;

    [Header("Damage Settings")]
    public float damageAmount = 15f;

    [Tooltip("ดาเมจโบนัสสูงสุดที่จะได้เพิ่มตามความเร็วการเหวี่ยง")]
    public float maxSpeedBonusDamage = 20f;

    private Vector3 previousPosition;
    private float currentSwingSpeed;
    public bool IsSwinging { get; private set; }

    public GameObject ImpactEffect;

    private void Start()
    {
        DisableHitBox();
        previousPosition = transform.position;
    }

    private void Update()
    {
        DetectSwingSpeed();
    }

    /// <summary>
    /// คำนวณความเร็วการเคลื่อนที่ของดาบในแต่ละเฟรม
    /// </summary>
    private void DetectSwingSpeed()
    {
        // คำนวณระยะทางที่ขยับใน 1 เฟรม หารด้วย Time.deltaTime จะได้ความเร็ว (m/s)
        Vector3 displacement = transform.position - previousPosition;
        currentSwingSpeed = displacement.magnitude / Time.deltaTime;

        // บันทึกตำแหน่งเฟรมนี้ไว้ใช้เทียบในเฟรมถัดไป
        previousPosition = transform.position;

        // เช็คว่าความเร็วเกิน Threshold หรือไม่
        if (currentSwingSpeed >= swingSpeedThreshold)
        {
            if (!IsSwinging)
            {
                EnableHitBox();
            }
        }
        else
        {
            if (IsSwinging)
            {
                DisableHitBox();
            }
        }
    }

    /// <summary>
    /// คำนวณดาเมจรวมตามความเร็วการเหวี่ยงและค่าจาก PlayerPrefs
    /// </summary>
    private float CalculateTotalDamage()
    {
        // 1. คำนวณสัดส่วนความเร็วจาก threshold ถึง maxSwingSpeed (คืนค่าช่วง 0.0 ถึง 1.0)
        float speedRatio = Mathf.InverseLerp(swingSpeedThreshold, maxSwingSpeed, currentSwingSpeed);

        // 2. คำนวณโบนัสดาเมจตามความเร็ว (สูงสุดไม่เกิน maxSpeedBonusDamage หรือ +20)
        float speedBonusDamage = speedRatio * maxSpeedBonusDamage;

        // 3. ดึงค่าโบนัสดาเมจเพิ่มเติมจาก PlayerPrefs
        int playerAddDamage = PlayerPrefs.GetInt("PLAYER_DAMAGE_ADDITION", 0);

        // รวมดาเมจทั้งหมด: ดาเมจพื้นฐาน + โบนัสความเร็ว + โบนัส PlayerPrefs
        return damageAmount + speedBonusDamage + playerAddDamage;
    }

    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Monster"))
        {
            Vector3 hitPoint = other.ClosestPoint(transform.position);
            if (ImpactEffect != null)
            {
                Instantiate(ImpactEffect, hitPoint, Quaternion.identity);
            }

            Enemy enemy = other.GetComponent<Enemy>();
            if (enemy != null)
            {
                // คำนวณดาเมจรวมแบบไดนามิกตอนโจมตีโดน
                float finalDamage = CalculateTotalDamage();

                enemy.TakeDamage(finalDamage, transform.position);

                PhoneSensorSender sender = FindObjectOfType<PhoneSensorSender>();
                if (sender != null)
                {
                    sender.TriggerVibrate();
                }
            }
        }
    }

    public void EnableHitBox()
    {
        IsSwinging = true;
        if (swordCollider != null)
            swordCollider.enabled = true;
    }

    public void DisableHitBox()
    {
        IsSwinging = false;
        if (swordCollider != null)
            swordCollider.enabled = false;
    }
}