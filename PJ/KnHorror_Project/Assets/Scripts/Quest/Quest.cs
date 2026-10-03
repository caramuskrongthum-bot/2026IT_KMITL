using UnityEngine;

public enum QuestType
{
    KillMonster,
    OpenRoom,
    WalkDistance,
    Heal,
    Resist
}

[CreateAssetMenu(fileName = "NewQuest", menuName = "QuestSystem/Quest")]
public class Quest : ScriptableObject
{
    [Header("Quest Info")]
    public string questId;          // รหัสเควส (เอาไว้เช็คเซฟข้อมูลถ้าจำเป็น)
    public string questTitle;       // ชื่อเควสที่จะแสดงให้ผู้เล่นเห็น
    [TextArea] public string description; // รายละเอียดเควส
    public QuestType questType;     // ประเภทของเควส

    [Header("Target Requirements")]
    public int targetMonsterKills;  // จำนวนมอนสเตอร์ที่ต้องกำจัด
    public int targetRoomsToOpen;   // จำนวนห้องที่ต้องเปิด/ผ่าน
    public float targetWalkDistance;// ระยะทางที่ต้องเดิน (เมตร)
    public int targetHeals;         // จำนวนครั้งที่ต้องฮีล
    public int targetResists;       // จำนวนครั้งที่ต้องขัดขืน

    [Header("Rewards")]
    public int coinReward;          // รางวัลเงินที่จะได้รับเมื่อทำเควสสำเร็จ

    public Sprite QuestIcon;
}