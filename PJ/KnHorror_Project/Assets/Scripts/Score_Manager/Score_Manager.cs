using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [Header("Player Target for Distance Tracking")]
    public Transform playerTransform;

    [Header("UI Pop-up Settings")]
    public GameObject popUpPrefab; // ลาก Prefab ที่มี TextMeshProUGUI มาใส่ตรงนี้
    public Transform canvasTransform; // ลาก Canvas หรือ Panel ที่ต้องการให้ Spawn มาใส่ตรงนี้

    [Header("Score Data")]
    public int passedRoomsCount = 0;       // จำนวนห้องที่ผ่านมาแล้ว
    public int successfulHealsCount = 0;   // จำนวนการฮีลสำเร็จ
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
    }

    private void Update()
    {
        TrackPlayerMovement();
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
    // 🔤 HELPER METHOD: SPAWN POP-UP UI
    // ----------------------------------------------------
    private void ShowPopUp(string message)
    {
        if (popUpPrefab == null)
        {
            Debug.LogWarning("ScoreManager: ยังไม่ได้ใส่ popUpPrefab ใน Inspector จ้า!");
            return;
        }

        // ถ้าไม่ได้ตั้ง canvasTransform ไว้ ให้ลองหาจาก Tag "Ui_Manager" หรือ Find Canvas ในฉากอัตโนมัติ[cite: 1, 7]
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

            // ดึง TextMeshProUGUI จากตัว Prefab หรือ Child ตัวแรก
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
    // 🔔 PUBLIC METHODS (บวกคะแนน + เด้ง Pop-up UI)
    // ----------------------------------------------------

    // เรียกตอนผ่านห้องสำเร็จ
    public void AddPassedRoom(int amount = 1)
    {
        passedRoomsCount += amount;
        ShowPopUp($"Room Cleared! +{amount}");
    }

    // เรียกตอนฮีลสำเร็จ
    public void AddSuccessfulHeal(int amount = 1)
    {
        successfulHealsCount += amount;
        ShowPopUp($"Heal successful! +{amount}");
    }

    // เรียกตอนขัดขืนสำเร็จ
    public void AddSuccessfulResist(int amount = 1)
    {
        successfulResistsCount += amount;
        ShowPopUp($"Resist successful! +{amount}");
    }

    // เรียกตอนมอนสเตอร์ตาย
    public void AddMonsterKill(int amount = 1)
    {
        monsterKillsCount += amount;
        ShowPopUp($"Monster Defeated! +{amount}");
    }

    // Reset สถิติทั้งหมด
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