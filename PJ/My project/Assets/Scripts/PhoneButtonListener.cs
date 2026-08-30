using UnityEngine;
using UnityEngine.Events;

public class PhoneButtonListener : MonoBehaviour
{
    [Header("Button Press Event")]
    [Tooltip("ใส่ ฟังก์ชัน หรือ Event ที่ต้องการให้ทำงานเมื่อกดปุ่มบนมือถือ")]
    public UnityEvent OnPhoneButtonPressed;

    private void Update()
    {
        // คอยเช็กว่า PhoneSensorSender พร้อมทำงานหรือยัง
        if (PhoneSensorSender.Instance != null)
        {
            // สามารถเรียกใช้งาน Debug Log หรือ Event ได้ทันที
        }
    }

    /// <summary>
    /// ฟังก์ชันนี้จะถูกเรียกโดย PhoneSensorSender เมื่อมีการส่งค่า ACTION_CLICK เข้ามา
    /// </summary>
    public void HandleButtonClick()
    {
        Debug.Log("🔔 [PhoneButtonListener] ปุ่มบนมือถือถูกกดแล้ว!");

        // สั่งให้ Event ทำงาน (สามารถไปผูกฟังก์ชันใน Unity Inspector ต่อได้)
        OnPhoneButtonPressed?.Invoke();
    }
}