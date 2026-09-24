using UnityEngine;
using TMPro;

[RequireComponent(typeof(TextMeshProUGUI))]
public class GameSettingsDisplayUI : MonoBehaviour
{
    private TextMeshProUGUI displayText;

    void Start()
    {
        displayText = GetComponent<TextMeshProUGUI>();
        UpdateSettingsText();
    }

    // เรียกฟังก์ชันนี้เพื่ออัปเดตข้อความบน UI (สามารถเรียกตอนกดเปลี่ยนค่าที่ปุ่ม/สไลเดอร์ได้ด้วยนะแม่)
    public void UpdateSettingsText()
    {
        if (displayText == null) return;

        // ดึงค่ามาจาก GameManager ถ้ามี (ถ้าไม่มีใช้ค่าดีฟอลต์กันพังจ่ะ)
        int roomGoal = (GameManager.Instance != null) ? GameManager.Instance.roomGoalCount : 50;
        int monsterDmg = (GameManager.Instance != null) ? GameManager.Instance.monsterDamageBonus : 0;
        int startHp = (GameManager.Instance != null) ? GameManager.Instance.startingHealth : 100;
        bool timeMode = (GameManager.Instance != null) && GameManager.Instance.isTimeMode;
        float timeLimit = (GameManager.Instance != null) ? GameManager.Instance.timeLimitSeconds : 60f;

        // จัดรูปแบบข้อความให้สวยงาม อลังการ สมฐานะตัวแม่
        string timeModeStr = timeMode ? $"Enabled ({timeLimit:F0}s)" : "Disabled";

        displayText.text = 
            $"<color=RED><b>-- CUSTOM GAME SETTINGS --</b></color>\n" +
            $"• Room Goal: <b>{roomGoal} Rooms</b>\n" +
            $"• Monster Damage Bonus: <b>+{monsterDmg}</b>\n" +
            $"• Starting Health: <b>{startHp} HP</b>\n" +
            $"• Time Mode: <b>{timeModeStr}</b>";
    }

    // ถ้าอยากให้มันคอยเช็คอัปเดตเรียลไทม์เวลาโยกสไลเดอร์ในหน้าเมนู เปิดใช้ Update นี้ได้เลยจ้า
    private void Update()
    {
        UpdateSettingsText();
    }
}