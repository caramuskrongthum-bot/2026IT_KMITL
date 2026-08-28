using System.Collections;
using UnityEngine;

public class CameraFacingPoint : MonoBehaviour
{
    [Header("Target Points Settings")]
    [Tooltip("รายการจุดที่ต้องการให้กล้องหันไปมอง")]
    public Transform[] lookPointIndex;

    [Header("Smooth & Curve Settings")]
    [Tooltip("ระยะเวลาที่ใช้ในการหมุนกล้องไปยังจุดเป้าหมาย (วินาที)")]
    public float transitionDuration = 2.0f;

    [Tooltip("กราฟควบคุมความนุ่มนวลในการหมุน (Smooth Graph)")]
    public AnimationCurve smoothCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Coroutine _lookCoroutine;
    private int _currentIndex = -1;

    /// <summary>
    /// สั่งให้กล้องหันไปยัง LookPoint ตาม Index ที่ระบุ
    /// </summary>
    /// <param name="index">ลำดับใน Array lookPointIndex</param>
    public void SwitchToPoint(int index)
    {
        if (lookPointIndex == null || lookPointIndex.Length == 0)
        {
            Debug.LogWarning("[CameraFacingPoint] ไม่มีข้อมูล LookPointIndex ในรายการ!");
            return;
        }

        if (index < 0 || index >= lookPointIndex.Length)
        {
            Debug.LogWarning($"[CameraFacingPoint] Index {index} เกินขอบเขตของ Array!");
            return;
        }

        if (lookPointIndex[index] == null)
        {
            Debug.LogWarning($"[CameraFacingPoint] Transform ที่ Index {index} เป็น Null!");
            return;
        }

        _currentIndex = index;

        // หากมีการหมุนกล้องเดิมทำงานอยู่ ให้ยกเลิกก่อนแล้วเริ่มอันใหม่
        if (_lookCoroutine != null)
        {
            StopCoroutine(_lookCoroutine);
        }

        _lookCoroutine = StartCoroutine(RotateCameraToTarget(lookPointIndex[index].position));
    }

    /// <summary>
    /// Coroutine คำนวณการหมุนกล้องตาม Animation Curve
    /// </summary>
    private IEnumerator RotateCameraToTarget(Vector3 targetPosition)
    {
        Quaternion startRotation = transform.rotation;

        // คำนวณทิศทางที่ต้องหันไปมอง
        Vector3 direction = (targetPosition - transform.position).normalized;

        // ป้องกันกรณีที่ตำแหน่งกล้องและจุดเป้าหมายตรงกันเป๊ะ
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            float elapsedTime = 0f;

            while (elapsedTime < transitionDuration)
            {
                elapsedTime += Time.deltaTime;

                // 1. คำนวณค่า progress (0.0 ถึง 1.0)
                float rawProgress = Mathf.Clamp01(elapsedTime / transitionDuration);

                // 2. ส่งค่า progress เข้า Animation Curve เพื่อให้ได้ความนุ่มนวลตามรูปกราฟ
                float curveEvaluatedValue = smoothCurve.Evaluate(rawProgress);

                // 3. หมุนกล้องโดยอิงจากค่าที่ประเมินได้จากกราฟ
                transform.rotation = Quaternion.Slerp(startRotation, targetRotation, curveEvaluatedValue);

                yield return null;
            }

            // บังคับให้การหมุนจบลงที่ตำแหน่งเป้าหมายแบบเป๊ะๆ
            transform.rotation = targetRotation;
        }

        _lookCoroutine = null;
    }
}