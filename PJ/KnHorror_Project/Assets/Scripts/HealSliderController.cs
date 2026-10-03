using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;

public class HealSliderController : MonoBehaviour
{
    [Header("UI & Settings")]
    public Slider healSlider;          // ลาก Slider มาใส่ตรงนี้
    public float healSpeed = 30f;      // ความเร็วในการเพิ่มเลือด
    public float maxValue = 100f;      // ค่าสูงสุดของ Slider

    [Header("Input & Mobile")]
    [Tooltip("ลาก Action ของปุ่มที่ใช้ฮีลมาใส่ตรงนี้ (ใช้เช็คตอนปล่อยปุ่ม)")]
    public InputActionReference healKey;
    [Tooltip("ลาก UI Button ของมือถือมาใส่ตรงนี้ (ถ้ามี)")]
    public Button BTN_for_mobile;

    [Header("Events")]
    public UnityEvent onHealComplete;  // Event ที่จะทำงานเมื่อฮีลเต็ม

    private bool isHealing = false;

    private void OnEnable()
    {
        if (healKey != null && healKey.action != null)
        {
            healKey.action.Enable();
            // ดักฟังเฉพาะตอนที่ "ปล่อยปุ่ม" (Canceled)
            healKey.action.canceled += OnKeyCanceled;
        }
    }

    private void OnDisable()
    {
        if (healKey != null && healKey.action != null)
        {
            healKey.action.canceled -= OnKeyCanceled;
            healKey.action.Disable();
        }
    }

    void Start()
    {
        if (healSlider != null)
        {
            healSlider.maxValue = maxValue;
            healSlider.value = 0f;
            healSlider.gameObject.SetActive(false); // ปิดไว้ก่อนตอนเริ่มเกม
        }

        SetupMobileButton();
    }

    private void SetupMobileButton()
    {
        if (BTN_for_mobile == null) return;

        EventTrigger trigger = BTN_for_mobile.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = BTN_for_mobile.gameObject.AddComponent<EventTrigger>();
        }

        // ดักจับเฉพาะตอนปล่อยมือจากปุ่มมือถือ (PointerUp) -> สั่งยกเลิกฮีล
        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => {
            if (isHealing) CancelHeal();
        });
        trigger.triggers.Add(pointerUp);
    }

    // เมื่อปล่อยปุ่มคีย์บอร์ด (ทำงานเฉพาะตอนกำลังฮีลอยู่แล้วผู้เล่นปล่อยปุ่ม)
    private void OnKeyCanceled(InputAction.CallbackContext context)
    {
        if (isHealing)
        {
            CancelHeal();
        }
    }

    void Update()
    {
        // เช็คเฉพาะตอนกำลังฮีลอยู่เท่านั้น
        if (isHealing && healSlider != null)
        {
            // เพิ่มค่าขึ้นเรื่อยๆ ตามเวลา
            healSlider.value += healSpeed * Time.deltaTime;

            // เช็คว่าเลือดเต็มหรือยัง
            if (healSlider.value >= maxValue)
            {
                healSlider.value = maxValue;
                TriggerHealComplete();
            }
        }
    }

    // ฟังก์ชันสั่งเริ่มฮีล (ให้เรียกใช้จากภายนอก เช่น ตอนกดปุ่มเริ่ม หรือเงื่อนไขอื่น)
    public void StartHeal()
    {
        if (isHealing) return;

        isHealing = true;
        if (healSlider != null)
        {
            healSlider.gameObject.SetActive(true);
        }
        Debug.Log("[HealSlider] เริ่มฮีล...");
    }

    // ฟังก์ชันสั่งหยุดฮีลกลางคัน (เมื่อปล่อยปุ่ม)
    public void CancelHeal()
    {
        if (!isHealing) return;

        isHealing = false;
        if (healSlider != null)
        {
            healSlider.value = 0f; // รีเซ็ตสไลเดอร์กลับเป็น 0
            healSlider.gameObject.SetActive(false);
        }
        Debug.Log("[HealSlider] ยกเลิกการฮีล (ปล่อยปุ่มแล้ว)");
    }

    // ฟังก์ชันจัดการตอนฮีลเต็ม
    private void TriggerHealComplete()
    {
        isHealing = false;
        onHealComplete?.Invoke();
        if (healSlider != null)
        {
            healSlider.value = 0f;
            healSlider.gameObject.SetActive(false);
        }
        Debug.Log("[HealSlider] ฮีลสำเร็จเต็มหลอด!");
    }
}