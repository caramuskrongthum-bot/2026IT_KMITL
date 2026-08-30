using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
public class Ultimate : MonoBehaviour
{
    [Header("Detection Settings")]
    [SerializeField] private string targetTag = "Player";

    [Header("UI Elements")]
    [SerializeField] private Slider valueSlider;

    [Header("Value & Speed Settings")]
    [SerializeField] private float currentValue = 0f;
    [SerializeField] private float increaseSpeed = 20f; // ความเร็วในการเพิ่มค่าต่อวินาที
    [SerializeField] private float decreaseSpeed = 20f; // ความเร็วในการลดค่าต่อวินาที

    [Header("Timer Settings")]
    [SerializeField] private float timeRemaining = 30f; // เวลาทั้งหมด (วินาที)

    [Header("Unity Events")]
    public UnityEvent EventUnity30; // ทำงานเมื่อเวลามด และ value < 30
    public UnityEvent EventUnity50; // ทำงานเมื่อเวลาหมด และ value < 50 (และ >= 30)
    public UnityEvent EventUnity80; // ทำงานเมื่อเวลาหมด และ value < 80 (และ >= 50)

    private bool isInsideTrigger = false;
    private bool isTimerFinished = false;

    public TextMeshProUGUI TexttimeRemaining;
    private void Start()
    {
        if (valueSlider != null)
        {
            valueSlider.minValue = 0f;
            valueSlider.maxValue = 100f;
            valueSlider.value = currentValue;
        }
    }

    private void Update()
    {
        if (isTimerFinished) return;

        // 1. จัดการการเพิ่ม/ลด Value
        if (isInsideTrigger)
        {
            currentValue += increaseSpeed * Time.deltaTime;
        }
        else
        {
            currentValue -= decreaseSpeed * Time.deltaTime;
        }

        currentValue = Mathf.Clamp(currentValue, 0f, 100f);

        if (valueSlider != null)
        {
            valueSlider.value = currentValue;
        }

        // 2. จัดการนับเวลาถอยหลัง
        if (timeRemaining > 0)
        {
            timeRemaining -= Time.deltaTime;
        }
        else
        {
            timeRemaining = 0;
            isTimerFinished = true;
            EvaluateFinalValue();
        }
        TexttimeRemaining.text = timeRemaining.ToString();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            isInsideTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(targetTag))
        {
            isInsideTrigger = false;
        }
    }

    private void EvaluateFinalValue()
    {
        int finalValue = Mathf.RoundToInt(currentValue);

        // เช็กเงื่อนไขตามลำดับจากน้อยไปมาก
        if (finalValue < 30)
        {
            PlayerStatus P = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();
            Game_Score_Manager A = GameObject.FindGameObjectWithTag("Alert_Manager").GetComponent<Game_Score_Manager>();
            A.Get_Score("-2 Heart!");
            P.Player_Got_Damage(2);
            EventUnity30?.Invoke();
        }
        else if (finalValue < 50)
        {
            PlayerStatus P = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();
            Game_Score_Manager A = GameObject.FindGameObjectWithTag("Alert_Manager").GetComponent<Game_Score_Manager>();
            A.Get_Score("-1 Heart!");
            P.Player_Got_Damage(1);
            EventUnity50?.Invoke();
        }
        else if (finalValue < 80)
        {
            PlayerStatus P = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerStatus>();
            Game_Score_Manager A = GameObject.FindGameObjectWithTag("Alert_Manager").GetComponent<Game_Score_Manager>();
            A.Get_Score("+1 Heart!");
            P.Player_Got_Damage(-1);
            EventUnity80?.Invoke();
        }
    }
}