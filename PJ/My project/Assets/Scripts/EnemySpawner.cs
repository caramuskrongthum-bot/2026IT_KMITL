using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [Tooltip("รายการ Prefab ทั้งหมดของศัตรูที่จะใช้สุ่มเสก")]
    public GameObject[] PrefabAllEnemy; // เปลี่ยนเป็น Array สำหรับเก็บศัตรูหลายประเภท
    [Tooltip("จุดเกิดของศัตรู")]
    public Transform[] spawnPoints;

    [Header("Queue & Wave Settings")]
    [Tooltip("จำนวนศัตรูทั้งหมดที่จะเกิดใน Wave/รอบนี้")]
    public int maxAlien = 10;

    [Tooltip("จำนวนศัตรูสูงสุดที่มีในฉากได้พร้อมกัน (เช่น 2 ตัว)")]
    public int maxActiveEnemies = 2;

    [Tooltip("ดีเลย์เวลาก่อนเสกตัวถัดไปเมื่อตัวเก่าตาย (วินาที)")]
    public float delayBeforeNextSpawn = 0.5f;

    [Header("Audio Settings")]
    public AudioSource AS;
    public AudioClip[] AC;
    public AudioClip ACBG;
    public int IndexAC;

    [Header("Events")]
    [Tooltip("จะถูกเรียกเมื่อกำจัด Enemy ครบทั้งหมดในคิวเรียบร้อยแล้ว")]
    public UnityEvent Clear_All_Alien;

    private int spawnedCount = 0;             // จำนวนที่เสกออกไปแล้วทั้งหมด
    public int defeatedCount = 0;            // จำนวนที่ถูกกำจัดไปแล้ว
    private int currentActiveEnemies = 0;     // จำนวนศัตรูที่ยังคงมีชีวิตอยู่ในฉากขณะนี้

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
        currentActiveEnemies = 0;

        // เริ่มเสกศัตรูระลอกแรกเข้าสู่ฉาก
        SpawnBatch();
    }

    /// <summary>
    /// เสกศัตรูเข้ามาในฉากตามจำนวน Max Active
    /// </summary>
    private void SpawnBatch()
    {
        int remainingInQueue = maxAlien - spawnedCount;
        int amountToSpawn = Mathf.Min(maxActiveEnemies, remainingInQueue);

        for (int i = 0; i < amountToSpawn; i++)
        {
            SpawnSingleEnemy();
        }
    }

    /// <summary>
    /// เสก Enemy 1 ตัวลงสนาม (สุ่มแบบจาก PrefabAllEnemy)
    /// </summary>
    private void SpawnSingleEnemy()
    {
        if (spawnedCount >= maxAlien) return;

        // Safety Check: หากไม่มี Prefab ใน Array จะหยุดการทำงาน
        if (PrefabAllEnemy == null || PrefabAllEnemy.Length == 0)
        {
            Debug.LogError("⚠️ [EnemySpawner] ยังไม่ได้ใส่ Prefab ใน PrefabAllEnemy!");
            return;
        }

        // 1. สุ่มเลือก Prefab จาก PrefabAllEnemy
        GameObject selectedPrefab = PrefabAllEnemy[Random.Range(0, PrefabAllEnemy.Length)];

        if (selectedPrefab == null)
        {
            Debug.LogWarning("⚠️ [EnemySpawner] พบ Element ที่เป็น null ใน PrefabAllEnemy!");
            return;
        }

        // 2. สุ่มจุดเกิด
        Transform spawnPoint = spawnPoints.Length > 0 ? spawnPoints[Random.Range(0, spawnPoints.Length)] : transform;
        Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), 0, Random.Range(-0.5f, 0.5f));

        // 3. Instantiate Enemy ที่สุ่มมาได้
        GameObject enemyObj = Instantiate(selectedPrefab, spawnPoint.position + randomOffset, spawnPoint.rotation);
        spawnedCount++;
        currentActiveEnemies++;

        // 4. ตรวจสอบคลังไอเทมขโมยใน LootManager
        if (LootManager.Instance != null && LootManager.Instance.GetStolenItemCount() > 0)
        {
            ItemData stolenItem = LootManager.Instance.PopOneStolenItem();

            if (stolenItem != null && enemyObj.TryGetComponent<Enemy>(out var enemyScript))
            {
                enemyScript.carriedLoot.Add(stolenItem);
            }
        }

        Debug.Log($"👾 Spawned Enemy ({selectedPrefab.name}) #{spawnedCount}/{maxAlien} (Active in Scene: {currentActiveEnemies})");

        // 5. เริ่ม Coroutine เฝ้ามองศัตรูตัวนี้
        StartCoroutine(TrackEnemyDeath(enemyObj));
    }

    /// <summary>
    /// คอยตรวจสอบเมื่อศัตรูตัวนี้ถูกลบ/ตายไป
    /// </summary>
    private IEnumerator TrackEnemyDeath(GameObject enemyObj)
    {
        while (enemyObj != null)
        {
            yield return null;
        }

        defeatedCount++;
        currentActiveEnemies--;

        // เล่นเสียงพร้อมปรับ Pitch
        PlayDeathSound();

        Debug.Log($"💀 Defeated Enemy #{defeatedCount}/{maxAlien} (Remaining in Scene: {currentActiveEnemies})");

        // ถ้ากำจัดศัตรูครบทั้งหมดในคิวแล้ว
        if (defeatedCount >= maxAlien)
        {
            Debug.Log("🎉 Clear All Aliens! Invoking Event...");
            Clear_All_Alien?.Invoke();
        }
        // เมื่อศัตรูในฉากตายหมดรอบแล้ว และยังมีคิวที่เหลือยังเสกไม่หมด
        else if (currentActiveEnemies == 0 && spawnedCount < maxAlien)
        {
            yield return new WaitForSeconds(delayBeforeNextSpawn);
            SpawnBatch();
        }
    }

    /// <summary>
    /// ฟังก์ชันคำนวณ Pitch และเล่นเสียง
    /// </summary>
    private void PlayDeathSound()
    {
        if (AS == null || AC == null || AC.Length == 0) return;

        IndexAC++;
        IndexAC = (int)Mathf.Repeat(IndexAC, AC.Length);

        if (AC[IndexAC] != null)
        {
            AS.PlayOneShot(AC[IndexAC]);
        }

        if (ACBG != null)
        {
            AS.PlayOneShot(ACBG);
        }
    }
}