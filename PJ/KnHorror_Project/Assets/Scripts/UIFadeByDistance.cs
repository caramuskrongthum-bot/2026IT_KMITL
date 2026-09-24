using UnityEngine;
using UnityEngine.UI;

public class UIFadeByDistance : MonoBehaviour
{
    [Header("References")]
public Graphic uiGraphic; // รองรับทั้ง Image และ RawImage จ่ะแม่ (ลาก Component มาใส่ได้เลย)
private Transform mainCameraTransform;

[Header("Distance Ranges")]
[Tooltip("ระยะที่ใกล้เกินไป UI จะเริ่มหาย")]
public float tooCloseDistance = 2f;
[Tooltip("ระยะที่เริ่มเห็น UI ชัดเต็มตา")]
public float optimalMinDistance = 4f;
[Tooltip("ระยะที่เริ่มไกล UI จะเริ่มจางลง")]
public float optimalMaxDistance = 10f;
[Tooltip("ระยะที่ไกลเกินไป UI จะหายไปเลย")]
public float tooFarDistance = 15f;

void Start()
{
    if (Camera.main != null)
    {
        mainCameraTransform = Camera.main.transform;
    }

    if (uiGraphic == null)
    {
        uiGraphic = GetComponent<Graphic>();
    }
}

void Update()
{
    if (mainCameraTransform == null || uiGraphic == null) return;

    // คำนวณระยะห่างระหว่าง UI กับกล้อง
    float distance = Vector3.Distance(transform.position, mainCameraTransform.position);
    float alpha = 0f;

    // 1. ระยะใกล้สุด (ต่ำกว่า tooCloseDistance หรือไกลกว่า tooFarDistance) = หาย (Alpha = 0)
    // 2. ระยะกำลังดี (ระหว่าง optimalMin ถึง optimalMax) = เห็นชัด (Alpha = 1)
    // 3. ช่วงคาดเกี่ยว (ระหว่าง tooClose ถึง optimalMin และ optimalMax ถึง tooFar) = ค่อยๆ ไล่ระดับความโปร่งใส (Smooth Fade)

    if (distance >= tooCloseDistance && distance <= optimalMinDistance)
    {
        // ช่วงขาขึ้น: จากใกล้ไปกลาง (ค่อยๆ ชัดขึ้น)
        alpha = Mathf.InverseLerp(tooCloseDistance, optimalMinDistance, distance);
    }
    else if (distance > optimalMinDistance && distance < optimalMaxDistance)
    {
        // ช่วงกลาง: เห็นชัดเต็ม 100%
        alpha = 1f;
    }
    else if (distance >= optimalMaxDistance && distance <= tooFarDistance)
    {
        // ช่วงขาลง: จากกลางไปไกล (ค่อยๆ จางลง)
        alpha = Mathf.InverseLerp(tooFarDistance, optimalMaxDistance, distance);
    }
    else
    {
        alpha = 0f;
    }

    // ปรับค่า Alpha ของ Image/RawImage ตามที่คำนวณได้
    Color currentColor = uiGraphic.color;
    currentColor.a = alpha;
    uiGraphic.color = currentColor;
}
}