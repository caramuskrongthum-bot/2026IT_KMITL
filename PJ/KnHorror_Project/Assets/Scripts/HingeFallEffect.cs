using UnityEngine;

public class HingeFallEffect : MonoBehaviour
{
    private bool isDropping = false;
    private float elapsedTime = 0f;

    [Header("Settings")]
    public float duration = 1.2f;        // เวลาทั้งหมดที่ใช้ในการตกจนนิ่ง
    public float maxAngle = 90f;         // มุมสูงสุดที่ตกลงมา (เช่น พับลงมา 90 องศา)
    public float bounceCount = 2f;       // จำนวนครั้งที่จะแกว่งเด้งกลับเบาๆ ก่อนจะนิ่ง

    private Quaternion startRotation;

    void Start()
    {
        // บันทึกมุมเริ่มต้นไว้
        startRotation = transform.localRotation;
    }

    // ฟังก์ชันสั่งให้ตกและแกว่ง
    public void TriggerDrop()
    {
        isDropping = true;
        elapsedTime = 0f;
    }

    void Update()
    {
        if (!isDropping) return;

        elapsedTime += Time.deltaTime;
        float t = elapsedTime / duration;

        if (t >= 1f)
        {
            t = 1f;
            isDropping = false; // จบการแกว่ง กลับมานิ่งสนิท
        }

        // ใช้สูตรคณิตศาสตร์จำลองการแกว่งของลูกตุ้มและแรงโน้มถ่วง (Ease-out + Elastic bounce)
        // ยิ่งเวลาผ่านไป ค่าการแกว่งจะค่อยๆ ลดลงจนเป็น 0
        float dampening = 1f - t;
        float angle = maxAngle * Mathf.Cos(t * Mathf.PI * (bounceCount + 0.5f)) * dampening;

        // สลับค่าเครื่องหมายเพื่อให้มันแกว่งกลับตัวได้สมจริง
        // หรือถ้าอยากให้พับลงมาแล้วแกว่งขึ้น-ลง ปรับแกนหมุนตรงนี้ได้เลย (เช่น แกน X, Y, หรือ Z)
        transform.localRotation = startRotation * Quaternion.Euler(angle, 0f, 0f);
    }
}