using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class FadingSlider : MonoBehaviour
{
    [Header("Slider Settings")]
    public Slider slider;

    [Header("Alpha Settings")]
    [Range(0f, 1f)] public float idleAlpha = 0.15f; // ความจางตอนปกติ (0.15 = จางมากกก)
    public float fadeDuration = 0.5f;              // ระยะเวลาที่ใช้ค่อยๆ จางลง
    public float keepSolidDuration = 1.0f;          // ชะลอเวลาให้ชัดเต็มร้อยกี่วินาทีก่อนจะเริ่มจาง

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        // ดึง CanvasGroup มาใช้ควบคุม Alpha ทั้งแผง (รวมทั้ง Fill, Background และ Handle)
        canvasGroup = GetComponent<CanvasGroup>();
        if (slider == null) slider = GetComponent<Slider>();
    }

    private void OnEnable()
    {
        // สั่งจางตั้งแต่ออกสตาร์ท
        canvasGroup.alpha = idleAlpha;

        // ผูก Event เมื่อค่า Slider เปลี่ยนแปลง
        if (slider != null)
        {
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }
    }

    private void OnDisable()
    {
        if (slider != null)
        {
            slider.onValueChanged.RemoveListener(OnSliderValueChanged);
        }
    }

    // เรียกทำงานเมื่อค่า value เปลี่ยนแปลง
    private void OnSliderValueChanged(float value)
    {
        // 1. ปรับ Alpha กลับมาเต็ม 100% ทันที!
        canvasGroup.alpha = 1.0f;

        // 2. หยุดนับเวลาเดิม แล้วเริ่มนับถอยหลังเพื่อเฟดจางใหม่
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }
        fadeCoroutine = StartCoroutine(FadeToIdle());
    }

    private IEnumerator FadeToIdle()
    {
        // รอให้หยุดปรับค่าก่อนแป๊บนึง
        yield return new WaitForSeconds(keepSolidDuration);

        // ค่อยๆ คล่อง Alpha ลงจนถึงค่า idleAlpha
        float startAlpha = canvasGroup.alpha;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, idleAlpha, time / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = idleAlpha;
    }
}