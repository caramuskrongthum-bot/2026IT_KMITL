using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class Enemy : MonoBehaviour
{
    [Header("Movement Settings (Walk Forward Only)")]
    [Tooltip("ความเร็วในการเดินตรงไปข้างหน้า")]
    public float moveSpeed = 2.0f;

    [Header("Knockback Settings (PvZ Style)")]
    [Tooltip("ระยะทาง/แรงถอยหลังเมื่อโดนโจมตี")]
    public float knockbackForce = 6.0f;
    [Tooltip("แรงกระโดดเด้งขึ้นเล็กน้อยตอนโดนฟัน")]
    public float jumpUpForce = 3.0f;
    [Tooltip("ระยะเวลาที่ Enemy จะชะงักถอยหลังก่อนจะเริ่มเดินต่อ (วินาที)")]
    public float knockbackStunDuration = 0.3f;

    [Header("Enemy Health Settings")]
    public float maxHealth = 100f;
    private float currentHealth;
    public Slider healthSlider;

    [Header("Skill Charge Settings")]
    public Slider skillSlider;
    [Tooltip("ความเร็วในการเพิ่มของหลอดสกิลต่อวินาที")]
    public float skillChargeSpeed = 20f;
    private float currentSkillValue = 0f;

    [Header("Skill Events")]
    public UnityEvent Enemy_Skill;

    [Header("Loot Drop System")]
    [Tooltip("ไอเทมที่ศัตรูตัวนี้ถืออยู่ (จะถูกยัดมาจาก EnemySpawner)")]
    public List<ItemData> carriedLoot = new List<ItemData>();

    private Rigidbody rb;
    private bool isKnockedBack = false;

    public UnityEvent Enemy_OnDead;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // ล็อกไม่ให้เคลื่อนที่แกน X (ซ้าย-ขวา) และล็อกการหมุนทุกแกน (X, Y, Z)
        // ผลลัพธ์: จะขยับได้เฉพาะแกน Y (ขึ้น-ลง/กระโดด) และแกน Z (เดินตรง/กระเด็นถอยหลัง) เท่านั้น
        rb.constraints = RigidbodyConstraints.FreezePositionX |
                         RigidbodyConstraints.FreezeRotationX |
                         RigidbodyConstraints.FreezeRotationY |
                         RigidbodyConstraints.FreezeRotationZ;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitPlayerTrigger"))
        {
            ApplyPvzKnockback(-transform.position);
            UnityEvent_ A = other.GetComponent<UnityEvent_>();
            if (A != null) A.DoEvent();
        }
    }

    void Start()
    {
        currentHealth = maxHealth;
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (skillSlider != null)
        {
            skillSlider.minValue = 0f;
            skillSlider.maxValue = 100f;
            skillSlider.value = 0f;
        }
    }

    void Update()
    {
        ChargeSkill();
    }

    void FixedUpdate()
    {
        if (!isKnockedBack)
        {
            MoveForward();
        }
    }

    private void MoveForward()
    {
        // คำนวณตำแหน่งถัดไปโดยยังคงล็อคตำแหน่งแกน X ให้ตรงกับตำแหน่งปัจจุบัน
        Vector3 targetPosition = transform.position + transform.forward * moveSpeed * Time.fixedDeltaTime;
        targetPosition.x = transform.position.x; // ป้องกันการเบี่ยงออกแกน X

        rb.MovePosition(targetPosition);
    }

    private void ChargeSkill()
    {
        if (skillSlider == null) return;

        currentSkillValue += skillChargeSpeed * Time.deltaTime;
        skillSlider.value = currentSkillValue;

        if (currentSkillValue >= skillSlider.maxValue)
        {
            currentSkillValue = 0f;
            skillSlider.value = 0f;

            Enemy_Skill?.Invoke();
            Debug.Log("⚡ Enemy_Skill Executed!");
        }
    }

    public void TakeDamage(float damage, Vector3 attackerPosition)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        ApplyPvzKnockback(attackerPosition);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void TakeDamage(float damage)
    {
        Vector3 attackerPos = transform.position + transform.forward;
        TakeDamage(damage, attackerPos);
    }

    private void ApplyPvzKnockback(Vector3 attackerPosition)
    {
        if (rb == null) return;

        Vector3 knockbackDir = (transform.position - attackerPosition);

        // บังคับให้กระเด็นเฉพาะแกน Z เท่านั้น
        knockbackDir.x = 0;
        knockbackDir.y = 0;
        knockbackDir.Normalize();

        if (Mathf.Approximately(knockbackDir.z, 0f))
        {
            knockbackDir.z = -1f;
        }

        rb.linearVelocity = Vector3.zero;
        Vector3 force = (knockbackDir * knockbackForce) + (Vector3.up * jumpUpForce);
        rb.AddForce(force, ForceMode.Impulse);

        StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        isKnockedBack = true;
        yield return new WaitForSeconds(knockbackStunDuration);

        // เคลียร์ความเร็วแกน X และ Z ให้เป็น 0 (คงเหลือความเร็วแกน Y สำหรับการตกลงพื้น)
        rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);

        isKnockedBack = false;
    }

    private void Die()
    {
        // 1. หา EnemySpawner ในฉากเพื่อเช็กจำนวนศัตรูที่เหลือ
        EnemySpawner spawner = FindObjectOfType<EnemySpawner>();

        if (spawner != null)
        {
            int remainingAlien = spawner.maxAlien - spawner.defeatedCount - 1;
            remainingAlien = Mathf.Max(0, remainingAlien); // ป้องกันไม่ให้ติดลบ

            Game_Score_Manager AlertManager = GameObject.FindGameObjectWithTag("Alert_Manager").GetComponent<Game_Score_Manager>();
            AlertManager.Get_Score($"{remainingAlien} alien left!");
        }
        else
        {
            // กรณีหา Spawner ไม่เจอ ให้แจ้ง Alert แบบเดิม
            Game_Score_Manager AlertManager = GameObject.FindGameObjectWithTag("Alert_Manager").GetComponent<Game_Score_Manager>();
            AlertManager.Get_Score("Enemy Defeated!");
        }

        Enemy_OnDead?.Invoke();
        DropCarriedLoot();

        Destroy(gameObject, 0.1f);
    }

    private void DropCarriedLoot()
    {
        if (carriedLoot == null || carriedLoot.Count == 0) return;

        foreach (var item in carriedLoot)
        {
            if (item == null) continue;

            GameObject prefabToSpawn = item.Item_Pick_Prefab;

            if (prefabToSpawn != null)
            {
                Vector3 randomOffset = new Vector3(Random.Range(-0.8f, 0.8f), 0.5f, Random.Range(-0.8f, 0.8f));
                GameObject spawnedItem = Instantiate(prefabToSpawn, transform.position + randomOffset, Quaternion.identity);

                if (spawnedItem.TryGetComponent<ItemPickup>(out var pickup))
                {
                    pickup.itemData = item;
                }
                if (spawnedItem.TryGetComponent<ItemWorldObject>(out var worldObj))
                {
                    worldObj.Setup(item);
                }
            }
        }

        carriedLoot.Clear();
    }
}