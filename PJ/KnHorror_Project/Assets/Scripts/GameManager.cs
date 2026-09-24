using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Custom Game Settings")]
    public int roomGoalCount = 50;         // จำนวนห้องเป้าหมาย
    public int monsterDamageBonus = 0;     // ดาเมจเพิ่มเติมของมอนสเตอร์
    public int startingHealth = 100;       // เลือดเริ่มต้นของ Player
    public bool isTimeMode = false;        // เปิด/ปิด โหมดจำกัดเวลา
    public float timeLimitSeconds = 60f;   // ระยะเวลาจำกัด (วินาที) จาก Slider

    private void Awake()
    {
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

    // ฟังก์ชันสำหรับเซ็ตค่าต่างๆ จากหน้า Custom Game UI ของแม่
    public void SetRoomGoal(int goal) { roomGoalCount = goal; }
    public void SetMonsterDamage(int damageBonus) { monsterDamageBonus = damageBonus; }
    public void SetStartingHealth(int health) { startingHealth = health; }
    public void SetTimeMode(bool timeMode) { isTimeMode = timeMode; }

    // ⏱️ ฟังก์ชันรับค่าจาก Slider เวลา (แปลงค่าเป็น float วินาทีให้เรียบร้อย)
    public void SetTimeLimit(float seconds) { timeLimitSeconds = seconds; }
}