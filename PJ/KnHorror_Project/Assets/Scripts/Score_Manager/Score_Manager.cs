using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Player Target for Distance Tracking")]
    public Transform playerTransform;

    [Header("UI Pop-up Settings")]
    public GameObject popUpPrefab; // Prefab ที่มี TextMeshProUGUI สำหรับทำ Pop-up เด้ง
    public Transform canvasTransform; // Canvas ที่ให้ Pop-up ไปเกิด

    [Header("UI Display Texts (ลาก TextMeshProUGUI มาใส่ตรงนี้แม่)")]
    public TextMeshProUGUI roomText;          // แสดงจำนวนห้องที่ผ่านมา
    public TextMeshProUGUI healText;          // แสดงจำนวนการฮีล
    public TextMeshProUGUI resistText;        // แสดงจำนวนการขัดขืน
    public TextMeshProUGUI monsterKillText;   // แสดงจำนวนมอนสเตอร์ที่กำจัด
    public TextMeshProUGUI distanceText;      // แสดงระยะทางที่เดิน

    [Header("Score Data")]
    public int passedRoomsCount = 0;        // จำนวนห้องที่ผ่านมาแล้ว
    public int successfulHealsCount = 0;    // จำนวนการฮีลสำเร็จ
    public int successfulResistsCount = 0; // จำนวนสำเร็จการขัดขืน
    public int monsterKillsCount = 0;      // จำนวนที่สามารถทำให้มอนสเตอร์ตายได้
    public float totalDistanceWalked = 0f; // ระยะทางที่เดินได้ (เมตร)

    private Vector3 lastPlayerPosition;
    private bool isTrackingDistance = false;

    private void Awake()
    {
        // Singleton Pattern
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        InitPlayerTracking();
        UpdateAllUI(); // อัปเดตหน้าจอตั้งแต่เริ่มเกม
    }

    private void Update()
    {
        TrackPlayerMovement();
        UpdateDistanceUI(); // อัปเดตระยะทางแบบเรียลไทม์
    }

    private void InitPlayerTracking()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
            isTrackingDistance = true;
        }
    }

    private void TrackPlayerMovement()
    {
        if (!isTrackingDistance || playerTransform == null)
        {
            InitPlayerTracking();
            return;
        }

        float distanceThisFrame = Vector3.Distance(playerTransform.position, lastPlayerPosition);

        if (distanceThisFrame > 0.001f && distanceThisFrame < 5f)
        {
            totalDistanceWalked += distanceThisFrame;
        }

        lastPlayerPosition = playerTransform.position;
    }

    // ----------------------------------------------------
    // 🖥️ UI UPDATE HELPER METHODS
    // ----------------------------------------------------
    private void UpdateAllUI()
    {
        if (roomText != null) roomText.text = $"Rooms: {passedRoomsCount}";
        if (healText != null) healText.text = $"Heals: {successfulHealsCount}";
        if (resistText != null) resistText.text = $"Resists: {successfulResistsCount}";
        if (monsterKillText != null) monsterKillText.text = $"Monsters: {monsterKillsCount}";
        if (distanceText != null) distanceText.text = $"Distance: {GetWalkedDistanceInMetersRounded()} m";
    }

    private void UpdateDistanceUI()
    {
        if (distanceText != null)
        {
            distanceText.text = $"Distance: {GetWalkedDistanceInMetersRounded()} m";
        }
    }

    // ----------------------------------------------------
    // 🔤 HELPER METHOD: SPAWN POP-UP UI
    // ----------------------------------------------------
    private void ShowPopUp(string message)
    {
        if (popUpPrefab == null)
        {
            Debug.LogWarning("ScoreManager: ยังไม่ได้ใส่ popUpPrefab ใน Inspector จ้าแม่!");
            return;
        }

        if (canvasTransform == null)
        {
            Canvas canvas = FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                canvasTransform = canvas.transform;
            }
        }

        if (canvasTransform != null)
        {
            GameObject popUpObj = Instantiate(popUpPrefab, canvasTransform, false);

            TextMeshProUGUI textComp = popUpObj.GetComponent<TextMeshProUGUI>();
            if (textComp == null)
            {
                textComp = popUpObj.GetComponentInChildren<TextMeshProUGUI>();
            }

            if (textComp != null)
            {
                textComp.text = message;
            }
        }
    }

    // ----------------------------------------------------
    // 🔔 PUBLIC METHODS (บวกคะแนน + อัปเดต UI + เด้ง Pop-up)
    // ----------------------------------------------------

    public void AddPassedRoom(int amount = 1)
    {
        passedRoomsCount += amount;
        if (roomText != null) roomText.text = $"Rooms: {passedRoomsCount}";
        ShowPopUp($"Room Cleared! +{amount}");
    }

    public void AddSuccessfulHeal(int amount = 1)
    {
        successfulHealsCount += amount;
        if (healText != null) healText.text = $"Heals: {successfulHealsCount}";
        ShowPopUp($"Heal successful! +{amount}");
    }

    public void AddSuccessfulResist(int amount = 1)
    {
        successfulResistsCount += amount;
        if (resistText != null) resistText.text = $"Resists: {successfulResistsCount}";
        ShowPopUp($"Resist successful! +{amount}");
    }

    public void AddMonsterKill(int amount = 1)
    {
        monsterKillsCount += amount;
        if (monsterKillText != null) monsterKillText.text = $"Monsters: {monsterKillsCount}";
        ShowPopUp($"Monster Defeated! +{amount}");
    }

    public void ResetScore()
    {
        passedRoomsCount = 0;
        successfulHealsCount = 0;
        successfulResistsCount = 0;
        monsterKillsCount = 0;
        totalDistanceWalked = 0f;

        if (playerTransform != null)
        {
            lastPlayerPosition = playerTransform.position;
        }

        UpdateAllUI(); // รีเซ็ตหน้าจอ UI ทั้งหมดด้วย
    }

    // ----------------------------------------------------
    // 📊 GETTERS FOR UI / SUMMARY MENU
    // ----------------------------------------------------

    public float GetWalkedDistanceInMeters()
    {
        return totalDistanceWalked;
    }

    public int GetWalkedDistanceInMetersRounded()
    {
        return Mathf.RoundToInt(totalDistanceWalked);
    }
}