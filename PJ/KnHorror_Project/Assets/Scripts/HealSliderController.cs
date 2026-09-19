using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class HealSliderController : MonoBehaviour
{
    [Header("UI & Settings")]
    public Slider healSlider;          // ลาก Slider มาใส่ตรงนี้
    public float healSpeed = 30f;      // ความเร็วในการเพิ่มเลือด
    public float maxValue = 100f;      // ค่าสูงสุดของ Slider

    [Header("Events")]
    public UnityEvent onHealComplete;  // Event ที่จะทำงานเมื่อฮีลเต็ม

    private bool isHealing = false;

    void Start()
    {
        if (healSlider != null)
        {
            healSlider.maxValue = maxValue;
            healSlider.value = 0f;
        }
    }

    void Update()
    {
        if (isHealing && healSlider != null)
        {
            // เพิ่มค่าขึ้นเรื่อยๆ ตามเวลา
            healSlider.value += healSpeed * Time.deltaTime;

            // เช็คว่าเลือดเต็มหรือยัง
            if (healSlider.value >= maxValue)
            {
                healSlider.value = maxValue; // ดึงให้ชนเพดานพอดี
                TriggerHealComplete();
            }
        }
    }

    // ฟังก์ชันสั่งเริ่มฮีล (เรียกใช้ตอนกดปุ่มหรือเงื่อนไขเริ่มฮีล)
    public void StartHeal()
    {
        isHealing = true;
        healSlider.gameObject.SetActive(true);
    }

    // ฟังก์ชันสั่งหยุดฮีลกลางคัน
    public void StopHeal()
    {
        isHealing = false;
        healSlider.gameObject.SetActive(false);
    }

    // ฟังก์ชันจัดการตอนฮีลเต็ม
    private void TriggerHealComplete()
    {
        isHealing = false;
        onHealComplete?.Invoke();
        healSlider.value = 0f;
        healSlider.gameObject.SetActive(false);
    }
}