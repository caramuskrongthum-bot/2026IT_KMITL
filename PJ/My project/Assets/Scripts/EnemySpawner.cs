using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{

    [Header("Spawn Settings")]
    [Tooltip("Prefab ของศัตรู")]
    public GameObject enemyPrefab;
    [Tooltip("จุดเกิดของศัตรู")]
    public Transform[] spawnPoints;

    [Header("Queue & Wave Settings")]
    [Tooltip("จำนวนศัตรูทั้งหมดที่จะเกิดใน Wave/รอบนี้")]
    public int maxAlien = 10;
    [Tooltip("ดีเลย์เวลาก่อนเสกตัวถัดไปเมื่อตัวเก่าตาย (วินาที)")]
    public float delayBeforeNextSpawn = 0.5f;

    [Header("Events")]
    [Tooltip("จะถูกเรียกเมื่อกำจัด Enemy ครบทั้งหมดในคิวเรียบร้อยแล้ว")]
    public UnityEvent Clear_All_Alien;

    private int spawnedCount = 0;      // จำนวนที่เสกออกไปแล้ว
                                       // ใน EnemySpawner.cs
    [HideInInspector] public int defeatedCount = 0; // เปลี่ยนจาก private เป็น public
    private GameObject currentEnemy;   // อ้างอิงถึง Enemy ตัวปัจจุบันที่อยู่ในฉาก

    private void Start()
    {
        StartSpawner();
    }

    /// <summary>
    /// เริ่มต้นคิวการเสก Enemy
    /// </summary>
    public void StartSpawner()
    {
        spawnedCount = 0;
        defeatedCount = 0;

        // เริ่มเสกตัวแรก
        SpawnNextEnemyInQueue();
    }

    /// <summary>
    /// เสก Enemy ตัวถัดไปในคิว
    /// </summary>
    private void SpawnNextEnemyInQueue()
    {

        // ถ้าเสกครบตามจำนวน MaxAlien แล้ว ไม่ต้องเสกเพิ่ม
        if (spawnedCount >= maxAlien)
        {
            return;
        }

        // 1. สุ่มจุดเกิด
        Transform spawnPoint = spawnPoints.Length > 0 ? spawnPoints[Random.Range(0, spawnPoints.Length)] : transform;
        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

        // 2. Instantiate Enemy
        currentEnemy = Instantiate(enemyPrefab, spawnPoint.position + randomOffset, spawnPoint.rotation);
        spawnedCount++;

        // 3. ตรวจสอบคลังไอเทมขโมยใน LootManager (ถ้ามีจะดึงออกมาใส่ให้ Enemy ตัวนี้ 1 ชิ้น)
        if (LootManager.Instance != null && LootManager.Instance.GetStolenItemCount() > 0)
        {
            ItemData stolenItem = LootManager.Instance.PopOneStolenItem();

            if (stolenItem != null && currentEnemy.TryGetComponent<Enemy>(out var enemyScript))
            {
                enemyScript.carriedLoot.Add(stolenItem);
            }
        }

        Debug.Log($"👾 Spawned Enemy #{spawnedCount}/{maxAlien}");

        // 4. เริ่ม Coroutine เฝ้ามองว่า Enemy ตัวนี้ตายเมื่อไหร่
        StartCoroutine(TrackEnemyDeath(currentEnemy));
    }

    /// <summary>
    /// คอยตรวจสอบว่า Enemy ตัวปัจจุบันถูกลบ/ตายไปหรือยัง
    /// </summary>
    private IEnumerator TrackEnemyDeath(GameObject enemyObj)
    {
        // รอจนกว่า GameObject ของ Enemy จะโดน Destroy
        while (enemyObj != null)
        {
            yield return null;
        }

        defeatedCount++;
        Debug.Log($"💀 Defeated Enemy #{defeatedCount}/{maxAlien}");

        // ถ้ากำจัดศัตรูครบตามจำนวน maxAlien แล้ว
        if (defeatedCount >= maxAlien)
        {
            Debug.Log("🎉 Clear All Aliens! Invoking Event...");
            Clear_All_Alien?.Invoke();
        }
        else
        {
            // รอด้านเวลาเล็กน้อยก่อนเสกตัวถัดไป
            yield return new WaitForSeconds(delayBeforeNextSpawn);
            SpawnNextEnemyInQueue();
        }
    }
}