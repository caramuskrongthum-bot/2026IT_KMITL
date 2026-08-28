using UnityEngine;

public class SwordHitBox : MonoBehaviour
{
    [Header("Collider Reference")]
    [SerializeField] private Collider swordCollider;

    [Header("Swing Speed Detection Settings")]
    [Tooltip("ความเร็วในการตวัดดาบขั้นต่ำที่จะเปิดใช้งาน HitBox (ปรับตั้งค่าตามความเหมาะสม)")]
    public float swingSpeedThreshold = 2.5f;

    [Header("Damage Settings")]
    public float damageAmount = 15f;

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
                enemy.TakeDamage(damageAmount, transform.position);
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