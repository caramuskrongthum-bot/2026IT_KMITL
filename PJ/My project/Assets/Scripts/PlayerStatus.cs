using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerStatus : MonoBehaviour
{
    [Header("Health Settings")]
    public int baseHealth = 3; // เลือดพื้นฐาน
    public int Health;

    [Header("UI System")]
    [Tooltip("Prefab ของไอคอนหัวใจ 1 ดวง")]
    public GameObject heartPrefab;
    [Tooltip("Transform ของ Container ใน UI ที่เป็นตัวเก็บหัวใจ (เช่น Panel ที่ใส่ Horizontal Layout Group)")]
    public Transform heartContainer;
    [Tooltip("Prefab ของ UI ตอนรับความเสียหาย")]
    public GameObject Damage_Ui_Prefab;

    [Header("Events")]
    public UnityEvent Player_Dead;

    // List สำหรับเก็บ GameObject ของหัวใจที่ถูกสร้างออกมา
    private List<GameObject> spawnedHearts = new List<GameObject>();

    private void Start()
    {
        // โหลดโบนัสหัวใจที่ซื้อจาก Shop (ถ้าไม่มีให้เป็น 0)
        int extraHealth = PlayerPrefs.GetInt("PLAYER_HEALTH_ADDITION", 0);

        // คำนวณเลือดสูงสุดรวม
        Health = baseHealth + extraHealth;

        // สร้างไอคอนหัวใจบน UI ตามจำนวน Health รวมที่มี
        SetupHearts();
    }

    /// <summary>
    /// สร้างไอคอนหัวใจตามจำนวน Health เริ่มต้น
    /// </summary>
    private void SetupHearts()
    {
        if (heartPrefab == null || heartContainer == null) return;

        // ลบหัวใจเก่าออกก่อน (ป้องกันการสร้างซ้ำ)
        foreach (Transform child in heartContainer)
        {
            Destroy(child.gameObject);
        }
        spawnedHearts.Clear();

        // สร้าง GameObject หัวใจดวงใหม่เข้า Container ตามจำนวน Health
        for (int i = 0; i < Health; i++)
        {
            GameObject newHeart = Instantiate(heartPrefab, heartContainer);
            spawnedHearts.Add(newHeart);
        }
    }

    public void Player_Got_Damage(int A)
    {
        if (Health > 0)
        {
            if (Damage_Ui_Prefab != null)
            {
                Instantiate(Damage_Ui_Prefab);
            }

            Health -= A;
            Health = Mathf.Max(0, Health); // ป้องกันไม่ให้โดนดาเมจจนเลือดติดลบ

            UpdateHeartUI();

            if (Health <= 0)
            {
                Player_Dead?.Invoke();
            }
        }
    }

    /// <summary>
    /// อัปเดตการเปิด/ปิด หัวใจตาม Health ปัจจุบัน
    /// </summary>
    private void UpdateHeartUI()
    {
        for (int i = 0; i < spawnedHearts.Count; i++)
        {
            if (spawnedHearts[i] != null)
            {
                // เปิดการทำงานดวงที่อยู่ในช่วง Health ปัจจุบัน และซ่อนดวงที่โดนดาเมจไปแล้ว
                spawnedHearts[i].SetActive(i < Health);
            }
        }
    }
}