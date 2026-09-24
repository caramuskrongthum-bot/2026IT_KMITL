using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GameTimerManager : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI timerText; // ลาก TextMeshPro ที่ใช้โชว์เวลามาใส่ตรงนี้
    public GameObject timerPanel;   // แผง UI สำหรับซ่อน/แสดงเวลา (ถ้ามี)

    [Header("Events")]
    public UnityEvent OnTimeOut;    // อีเวนต์เรียกตอนเวลาหมด (เช่น Game Over)

    private float currentTime;
    private bool isTimerRunning = false;

    void Start()
    {
        // เช็คว่าใน GameManager เปิด Time Mode ไว้ไหม
        if (GameManager.Instance != null && GameManager.Instance.isTimeMode)
        {
            isTimerRunning = true;
            currentTime = GameManager.Instance.timeLimitSeconds;

            if (timerPanel != null) timerPanel.SetActive(true);
        }
        else
        {
            // ถ้าไม่ได้เปิดโหมดเวลา ซ่อน UI เวลาทิ้งซะ
            isTimerRunning = false;
            if (timerPanel != null) timerPanel.SetActive(false);
            if (timerText != null) timerText.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (!isTimerRunning) return;

        if (currentTime > 0)
        {
            currentTime -= Time.deltaTime;

            // อัปเดตข้อความบน UI
            UpdateTimerUI(currentTime);

            if (currentTime <= 0)
            {
                currentTime = 0;
                isTimerRunning = false;
                TriggerTimeOut();
            }
        }
    }

    void UpdateTimerUI(float timeToDisplay)
    {
        if (timerText == null) return;

        // แปลงวินาทีเป็นรูปแบบ นาที:วินาที (เช่น 01:30)
        int minutes = Mathf.FloorToInt(timeToDisplay / 60f);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60f);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    void TriggerTimeOut()
    {
        Debug.Log("<color=red><b>[TIME OUT!]</b> หมดเวลาแล้วแม่! เกมโอเวอร์แบบตัวมัม!</color>");

        // 👉 สั่งทำงานอีเวนต์หมดเวลา หรือจะเรียกเปิดหน้า Game Over Panel ของแม่ตรงนี้ได้เลย
        OnTimeOut.Invoke();
    }
}