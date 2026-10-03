using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class KeyActionEvent : MonoBehaviour
{
    public InputActionReference Key;
    public UnityEvent UnityEvent;
    public Button Button;

    [Header("📱 Mobile Settings")]
    [Tooltip("ลาก UI Button ของมือถือมาใส่ตรงนี้")]
    public Button BTN_for_mobile;

    [Header("⏳ Hold Settings (ตั้งค่าการกดค้าง)")]
    [Tooltip("ถ้าเป็น true จะต้องกดปุ่มนี้ค้างไว้ถึงจะทำงาน (ค่าเริ่มต้นเป็น false)")]
    public bool RequireHold = false;

    [Tooltip("ระยะเวลาที่ต้องกดค้าง (วินาที)")]
    public float HoldDuration = 1.5f;

    [Header("❄️ Cooldown Settings (ระบบคูลดาวน์แบบ Smooth Float)")]
    [Tooltip("ระยะเวลาคูลดาวน์หลังใช้งาน (วินาที)")]
    public float CooldownDuration_ = 0.0f;

    [Tooltip("ลาก Slider UI มาใส่ตรงนี้เพื่อแสดงเวลาคูลดาวน์แบบสมูท")]
    public Slider CooldownSlider;

    private float _holdTimer = 0f;
    private float _cooldownTimer = 0f;
    private bool _hasTriggered = false;
    private bool _isMobilePressed = false;

    private void OnEnable()
    {
        if (Key != null && Key.action != null)
        {
            Key.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (Key != null && Key.action != null)
        {
            Key.action.Disable();
        }
        ResetHoldState();
    }

    private void Start()
    {
        if (CooldownSlider != null)
        {
            CooldownSlider.gameObject.SetActive(false);
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

        EventTrigger.Entry pointerDown = new EventTrigger.Entry();
        pointerDown.eventID = EventTriggerType.PointerDown;
        pointerDown.callback.AddListener((data) => { OnMobilePressed(); });
        trigger.triggers.Add(pointerDown);

        EventTrigger.Entry pointerUp = new EventTrigger.Entry();
        pointerUp.eventID = EventTriggerType.PointerUp;
        pointerUp.callback.AddListener((data) => { OnMobileReleased(); });
        trigger.triggers.Add(pointerUp);
    }

    private void OnMobilePressed()
    {
        _isMobilePressed = true;

        if (!RequireHold && _cooldownTimer <= 0f)
        {
            TriggerAction();
        }
    }

    private void OnMobileReleased()
    {
        _isMobilePressed = false;
        ResetHoldState();
    }

    private void ResetHoldState()
    {
        _holdTimer = 0f;
        _hasTriggered = false;
    }

    public void Update()
    {
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;

            if (CooldownSlider != null)
            {
                CooldownSlider.value = _cooldownTimer / CooldownDuration_;
            }

            if (_cooldownTimer <= 0f)
            {
                _cooldownTimer = 0f;
                if (CooldownSlider != null)
                {
                    CooldownSlider.gameObject.SetActive(false);
                }
            }
            return;
        }
        bool isPressed = false;
        bool isHeld = false;

        if (Key != null && Key.action != null)
        {
            if (Key.action.IsPressed()) isHeld = true;
            if (Key.action.WasPressedThisFrame()) isPressed = true;
        }
        if (_isMobilePressed)
        {
            isHeld = true;
        }
        if (RequireHold)
        {
            if (isHeld)
            {
                if (!_hasTriggered)
                {
                    _holdTimer += Time.deltaTime;

                    if (_holdTimer >= HoldDuration)
                    {
                        TriggerAction();
                        _hasTriggered = true;
                    }
                }
            }
            else
            {
                ResetHoldState();
            }
        }
        else
        {
            if (isPressed)
            {
                TriggerAction();
            }
        }
    }

    private void TriggerAction()
    {
        UnityEvent.Invoke();
        if (Button != null && Button.interactable == true)
        {
            Button.onClick.Invoke();
        }

        if (CooldownDuration_ > 0f)
        {
            _cooldownTimer = CooldownDuration_;

            if (CooldownSlider != null)
            {
                CooldownSlider.wholeNumbers = false;
                CooldownSlider.maxValue = 1.0f;
                CooldownSlider.value = 1.0f;
                CooldownSlider.gameObject.SetActive(true);
            }
        }
    }
}