using UnityEngine;
using System.Collections;

public class SmoothPositionSwitcher : MonoBehaviour
{
    [Header("UI RectTransform (ถ้าไม่ใส่ จะดึงจาก Object นี้อัตโนมัติ)")]
    public RectTransform targetRectTransform;

    [Header("Position Settings (Anchored Position)")]
    [Tooltip("ตำแหน่ง Before (เช่น ตอนซ่อน หรืออยู่ด้านล่าง)")]
    public Vector2 beforePosition;

    [Tooltip("ตำแหน่ง After (เช่น ตอนแสดงขึ้นมา หรืออยู่ตำแหน่งปกติ)")]
    public Vector2 afterPosition;

    [Header("Smooth Settings")]
    [Tooltip("กราฟความเร็วในการเคลื่อนที่ (ปรับความสมูท ความหน่วง หรือดีดเด้งได้ตามใจชอบ)")]
    public AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Tooltip("ระยะเวลาในการเคลื่อนที่ (วินาที)")]
    public float duration = 0.5f;

    [Header("Current State")]
    [Tooltip("สถานะปัจจุบัน (ติ๊กถูก = After, ไม่ติ๊ก = Before)")]
    public bool isAfterState = false;

    private Coroutine transitionCoroutine;

    private void Awake()
    {
        if (targetRectTransform == null)
        {
            targetRectTransform = GetComponent<RectTransform>();
        }
    }

    /// <summary>
    /// ฟังก์ชันสำหรับเปลี่ยนค่า bool และสั่งสไลด์ UI แบบ Smooth ตามกราฟ
    /// </summary>
    public void SetBool(bool value)
    {
        isAfterState = value;

        if (targetRectTransform == null) return;

        // หยุด Coroutine เก่าก่อนเพื่อกันการเคลื่อนที่ซ้อนทับกัน
        if (transitionCoroutine != null)
        {
            StopCoroutine(transitionCoroutine);
        }

        // เริ่มเคลื่อนที่ไปยังตำแหน่งเป้าหมายด้วย anchoredPosition
        Vector2 targetPos = isAfterState ? afterPosition : beforePosition;
        transitionCoroutine = StartCoroutine(MoveSmoothly(targetRectTransform.anchoredPosition, targetPos, duration));
    }

    /// <summary>
    /// ฟังก์ชันสลับค่า bool สลับไป-มา (Toggle)
    /// </summary>
    public void ToggleState()
    {
        SetBool(!isAfterState);
    }

    private IEnumerator MoveSmoothly(Vector2 startPos, Vector2 endPos, float time)
    {
        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            elapsedTime += Time.deltaTime;
            float rawPercent = Mathf.Clamp01(elapsedTime / time);

            // ดึงค่าจาก AnimationCurve มาใช้คำนวณความสมูท
            float curvePercent = transitionCurve.Evaluate(rawPercent);

            // ขยับตำแหน่ง UI ด้วย anchoredPosition ไม่ให้พิกเซลเพี้ยน
            targetRectTransform.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, curvePercent);

            yield return null;
        }

        // ล็อกตำแหน่งสุดท้ายให้ตรงเป๊ะ 100%
        targetRectTransform.anchoredPosition = endPos;
    }
}