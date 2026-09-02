using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class Ultimate : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider valueSlider;
    public TextMeshProUGUI TexttimeRemaining;

    float currentValue = 0f;
    float valuePerHit = 10f;
    int timeRemaining = 10;

    [Header("Unity Events")]
    public UnityEvent Bomb;
    public UnityEvent Finish;
    public UnityEvent GotAttack;

    private bool isTimerFinished = false;
    private float timer;

    public GameObject Bomb_effect;

    private void Start()
    {
        ResetUltimate();
    }

    private void Update()
    {
        if (isTimerFinished) return;

        // จัดการนับเวลาถอยหลัง
        if (timer > 0)
        {
            timer -= Time.deltaTime;
            UpdateTimerUI();
        }
        else
        {
            timer = 0;
            isTimerFinished = true;
            UpdateTimerUI();
            EvaluateFinalValue();
        }
    }

    /// <summary>
    /// เรียกใช้งานจาก SwordHitBox เมื่อฟันโดน Trigger 1 ครั้ง
    /// </summary>
    public void AddHitProgress()
    {
        if (isTimerFinished) return;

        currentValue += valuePerHit;
        currentValue = Mathf.Clamp(currentValue, 0f, 100f);
        GotAttack.Invoke();
        if (valueSlider != null)
        {
            valueSlider.value = currentValue;
        }

        if (currentValue >= 100f)
        {
            isTimerFinished = true;
            Finish.Invoke();
            EvaluateFinalValue();
        }
    }

    public void ResetUltimate()
    {
        timer = timeRemaining;
        currentValue = 0f;
        isTimerFinished = false;

        if (valueSlider != null)
        {
            valueSlider.minValue = 0f;
            valueSlider.maxValue = 100f;
            valueSlider.value = currentValue;
        }

        UpdateTimerUI();
    }

    private void UpdateTimerUI()
    {
        int displayTime = Mathf.CeilToInt(timer);
        TexttimeRemaining.text = displayTime.ToString();
    }

    private void EvaluateFinalValue()
    {
        int finalValue = Mathf.RoundToInt(currentValue);
        if (finalValue != 100)
        {
            Bomb?.Invoke();
            GameObject A = Instantiate(Bomb_effect);
            A.transform.position = transform.position;
        }
    }
}