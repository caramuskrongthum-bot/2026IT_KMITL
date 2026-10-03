using UnityEngine;
using System;

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    [Header("Quest Slots (ใส่ Quest ได้ 3 ช่องตามสั่งจ่ะแม่)")]
    public Quest[] activeQuests = new Quest[3];

    public int[] currentProgress = new int[3];
    public float[] currentFloatProgress = new float[3];
    public bool[] isQuestCompleted = new bool[3];

    // คีย์สำหรับเซฟข้อมูลลง PlayerPrefs
    private const string QUEST_DATE_KEY = "QUEST_LAST_COMPLETED_DATE_";
    private const string QUEST_STATUS_KEY = "QUEST_STATUS_COMPLETED_";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        CheckAndResetDailyQuests();
    }

    private void Update()
    {
        // อัปเดตเควสประเภทระยะทางแบบเรียลไทม์จาก ScoreManager (ถ้ายังไม่เสร็จ)
        if (ScoreManager.Instance != null)
        {
            float currentDist = ScoreManager.Instance.GetWalkedDistanceInMeters();
            CheckDistanceQuests(currentDist);
        }
    }

    // ----------------------------------------------------
    // 📅 ระบบตรวจสอบวันใหม่ และ โหลด/รีเซ็ต เซฟข้อมูลเควส
    // ----------------------------------------------------
    private void CheckAndResetDailyQuests()
    {
        // 1. ดึงวันที่ปัจจุบันของเครื่อง (เช่น "28/9/2026")
        string todayString = DateTime.Now.ToString("dd/MM/yyyy");

        for (int i = 0; i < activeQuests.Length; i++)
        {
            string dateKey = QUEST_DATE_KEY + i;
            string statusKey = QUEST_STATUS_KEY + i;

            // ดึงวันที่บันทึกไว้ครั้งล่าสุดของเควสช่องนี้
            string savedDate = PlayerPrefs.GetString(dateKey, "");

            // เช็คว่า วันล่าสุดที่ทำสำเร็จ ตรงกับ "วันนี้" หรือไม่?
            if (savedDate == todayString)
            {
                // ถ้าเป็นวันเดียวกัน: โหลดสถานะว่าทำเสร็จไปแล้ว
                isQuestCompleted[i] = PlayerPrefs.GetInt(statusKey, 0) == 1;
                // ถ้าทำเสร็จแล้ว เซ็ต progress ให้เต็มเป้าหมายไว้เลย
                if (isQuestCompleted[i] && activeQuests[i] != null)
                {
                    currentProgress[i] = GetTargetAmount(activeQuests[i]);
                }
            }
            else
            {
                // ถ้าเป็นวันใหม่ (หรือเปิดเกมครั้งแรก): ทำการรีเซ็ตเควสใหม่ทั้งหมด!
                isQuestCompleted[i] = false;
                currentProgress[i] = 0;
                currentFloatProgress[i] = 0f;

                // ล้างค่าเซฟเก่าของวันนี้ทิ้ง
                PlayerPrefs.SetInt(statusKey, 0);
                PlayerPrefs.SetString(dateKey, "");
            }
        }
        PlayerPrefs.Save();
        Debug.Log($"<color=cyan>📅 [Quest System] ตรวจสอบรอบวันสำเร็จ! วันนี้คือ: {todayString}</color>");
    }

    // ----------------------------------------------------
    // 🗡️ ฟังก์ชันรับค่าความคืบหน้าจากภายนอก
    // ----------------------------------------------------
    public void NotifyMonsterKilled()
    {
        UpdateProgress(QuestType.KillMonster, 1);
    }

    public void NotifyRoomOpened()
    {
        UpdateProgress(QuestType.OpenRoom, 1);
    }

    public void NotifyHeal()
    {
        UpdateProgress(QuestType.Heal, 1);
    }

    public void NotifyResist()
    {
        UpdateProgress(QuestType.Resist, 1);
    }

    private void CheckDistanceQuests(float distance)
    {
        for (int i = 0; i < activeQuests.Length; i++)
        {
            if (activeQuests[i] != null && !isQuestCompleted[i] && activeQuests[i].questType == QuestType.WalkDistance)
            {
                currentFloatProgress[i] = distance;
                if (currentFloatProgress[i] >= activeQuests[i].targetWalkDistance)
                {
                    CompleteQuest(i);
                }
            }
        }
    }

    // ----------------------------------------------------
    // ⚙️ ระบบคำนวณและเช็คเงื่อนไขเควส
    // ----------------------------------------------------
    private void UpdateProgress(QuestType type, int amount)
    {
        for (int i = 0; i < activeQuests.Length; i++)
        {
            Quest q = activeQuests[i];
            if (q != null && !isQuestCompleted[i] && q.questType == type)
            {
                currentProgress[i] += amount;
                int target = GetTargetAmount(q);

                Debug.Log($"<color=yellow>📜 [Quest] ความคืบหน้าเควสช่องที่ {i + 1} ({q.questTitle}): {currentProgress[i]}/{target}</color>");

                if (currentProgress[i] >= target)
                {
                    CompleteQuest(i);
                }
            }
        }
    }

    private int GetTargetAmount(Quest q)
    {
        if (q == null) return 0;
        switch (q.questType)
        {
            case QuestType.KillMonster: return q.targetMonsterKills;
            case QuestType.OpenRoom: return q.targetRoomsToOpen;
            case QuestType.Heal: return q.targetHeals;
            case QuestType.Resist: return q.targetResists;
            default: return 0;
        }
    }

    private void CompleteQuest(int index)
    {
        if (isQuestCompleted[index]) return;

        isQuestCompleted[index] = true;
        Quest q = activeQuests[index];
        string todayString = DateTime.Now.ToString("dd/MM/yyyy");
        PlayerPrefs.SetInt(QUEST_STATUS_KEY + index, 1);
        PlayerPrefs.SetString(QUEST_DATE_KEY + index, todayString);
        PlayerPrefs.Save();
        Debug.Log($"<color=green>🎉 [Quest Success!] เควสสำเร็จ: {q.questTitle}! วันที่ทำสำเร็จ: {todayString} | ได้รับรางวัล {q.coinReward} Coin</color>");
    }
}