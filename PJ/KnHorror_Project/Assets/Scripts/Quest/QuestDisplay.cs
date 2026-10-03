using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestDisplay : MonoBehaviour
{
    [Header("UI Quest Icons")]
    public Image Image_Q1;
    public Image Image_Q2;
    public Image Image_Q3;

    [Header("UI Quest Titles")]
    public TextMeshProUGUI TextMeshProUGUI_Q1;
    public TextMeshProUGUI TextMeshProUGUI_Q2;
    public TextMeshProUGUI TextMeshProUGUI_Q3;

    [Header("UI Quest Descriptions / Progress")]
    public TextMeshProUGUI TextMeshProUGUI_Q1_Des;
    public TextMeshProUGUI TextMeshProUGUI_Q2_Des;
    public TextMeshProUGUI TextMeshProUGUI_Q3_Des;

    public Sprite Sprite_Quest_Clear;

    private void Update()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        if (QuestManager.Instance == null) return;

        UpdateSingleSlot(Image_Q1, TextMeshProUGUI_Q1, TextMeshProUGUI_Q1_Des, 0);
        UpdateSingleSlot(Image_Q2, TextMeshProUGUI_Q2, TextMeshProUGUI_Q2_Des, 1);
        UpdateSingleSlot(Image_Q3, TextMeshProUGUI_Q3, TextMeshProUGUI_Q3_Des, 2);
    }

    private void UpdateSingleSlot(Image imgComponent, TextMeshProUGUI titleText, TextMeshProUGUI descText, int index)
    {
        // เช็คว่ามีเควสในช่องนั้นไหม
        if (index < QuestManager.Instance.activeQuests.Length)
        {
            Quest q = QuestManager.Instance.activeQuests[index];
            bool isCompleted = QuestManager.Instance.isQuestCompleted[index];
            int currentProg = QuestManager.Instance.currentProgress[index];

            if (q != null)
            {
                // เปิดการแสดงผล UI ของช่องนี้
                if (imgComponent != null) imgComponent.gameObject.SetActive(true);
                if (titleText != null) titleText.gameObject.SetActive(true);
                if (descText != null) descText.gameObject.SetActive(true);

                // 1. แสดงชื่อเควส
                if (titleText != null)
                {
                    titleText.text = q.questTitle;
                }

                // 2. แสดงรายละเอียด + ความคืบหน้า (เช่น กำจัดมอนสเตอร์ 2/5)
                if (descText != null)
                {
                    int targetAmount = GetTargetAmount(q);
                    if (!isCompleted)
                    {
                        descText.text = $"{q.description}";
                    }
                    else
                    {
                        descText.text = $"{q.description}\n<color=#4CCB59>Completed!</color>";
                    }
                }
                if (imgComponent != null)
                {
                    if (!isCompleted)
                    {
                        if (q.QuestIcon != null)
                            imgComponent.sprite = q.QuestIcon;
                    }
                    else
                    {
                        if (Sprite_Quest_Clear != null)
                            imgComponent.sprite = Sprite_Quest_Clear;
                    }
                }
            }
            else
            {
                if (imgComponent != null) imgComponent.gameObject.SetActive(false);
                if (titleText != null) titleText.gameObject.SetActive(false);
                if (descText != null) descText.gameObject.SetActive(false);
            }
        }
    }

    // ฟังก์ชันช่วยดึงเป้าหมายของเควสแต่ละประเภทมาแสดงโชว์
    private int GetTargetAmount(Quest q)
    {
        if (q == null) return 0;
        switch (q.questType)
        {
            case QuestType.KillMonster: return q.targetMonsterKills;
            case QuestType.OpenRoom: return q.targetRoomsToOpen;
            case QuestType.WalkDistance: return Mathf.RoundToInt(q.targetWalkDistance);
            case QuestType.Heal: return q.targetHeals;
            case QuestType.Resist: return q.targetResists;
            default: return 0;
        }
    }
}