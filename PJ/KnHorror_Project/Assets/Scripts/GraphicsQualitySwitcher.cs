using UnityEngine;
using UnityEngine.UI;
using TMPro; // รองรับ TextMeshPro (ถ้าใช้ Text ธรรมดาให้เปลี่ยนเป็น using UnityEngine.UI;)

public class GraphicsQualitySwitcher : MonoBehaviour
{
    [Header("UI Display (Optional)")]
    public TextMeshProUGUI qualityTextLabel; // ช่องแสดงชื่อกราฟฟิกปัจจุบัน (ถ้ามี)

    private void Start()
    {
        // อัปเดตแสดงผลข้อความเริ่มต้นตอนเปิดเกม
        UpdateQualityUI();
    }

    // ฟังก์ชันนี้เอาไว้ผูกกับปุ่ม (Button OnClick) เพื่อเลื่อนกราฟฟิกไปข้างหน้า
    public void NextQuality()
    {
        int currentLevel = QualitySettings.GetQualityLevel();
        int totalLevels = QualitySettings.names.Length;

        // เลื่อนไปข้างหน้า 1 สเต็ป ถ้าถึงอันสุดท้ายให้วนกลับมา 0 (Loop)
        int nextLevel = (currentLevel + 1) % totalLevels;

        // สั่งเปลี่ยนระดับกราฟฟิกของ Unity
        QualitySettings.SetQualityLevel(nextLevel, true);

        Debug.Log($"<color=cyan>[Graphics Quality]</color> เปลี่ยนกราฟฟิกเป็น: <b>{QualitySettings.names[nextLevel]}</b>");

        // อัปเดตข้อความบนจอ
        UpdateQualityUI();
    }

    // (แถม) ฟังก์ชันถอยหลัง เผื่อคุณน้าอยากทำปุ่มย้อนกลับด้วย
    public void PreviousQuality()
    {
        int currentLevel = QualitySettings.GetQualityLevel();
        int totalLevels = QualitySettings.names.Length;

        int prevLevel = (currentLevel - 1 + totalLevels) % totalLevels;

        QualitySettings.SetQualityLevel(prevLevel, true);

        Debug.Log($"<color=cyan>[Graphics Quality]</color> เปลี่ยนกราฟฟิกเป็น: <b>{QualitySettings.names[prevLevel]}</b>");

        UpdateQualityUI();
    }

    private void UpdateQualityUI()
    {
        if (qualityTextLabel != null)
        {
            int currentLevel = QualitySettings.GetQualityLevel();
            qualityTextLabel.text = QualitySettings.names[currentLevel];
        }
    }
}