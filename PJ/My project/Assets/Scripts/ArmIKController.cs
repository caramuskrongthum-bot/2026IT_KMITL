using UnityEngine;

[RequireComponent(typeof(Animator))]
public class RacketHandIK : MonoBehaviour
{
    public enum HandSide { RightHand, LeftHand }

    [Header("Target References")]
    [Tooltip("ใส่ Transform ของ ไม้เทนนิส/ด้ามดาบ หรือ Target ที่ต้องการให้มือจับ")]
    public Transform racketHandleTarget;

    [Header("IK Settings")]
    [Tooltip("เลือกว่าจะใช้มือข้างไหนจับด้าม")]
    public HandSide handSide = HandSide.RightHand;

    [Range(0f, 1f)]
    [Tooltip("น้ำหนักการดึงตำแหน่งมือไปหาด้าม (1 = ติดมือเป๊ะ)")]
    public float positionWeight = 1.0f;

    [Range(0f, 1f)]
    [Tooltip("น้ำหนักการหมุนข้อมือตามด้าม (1 = ข้อมือหมุนตามไม้ 100%)")]
    public float rotationWeight = 1.0f;

    [Header("Elbow (Hint) Settings - ข้อศอก")]
    [Tooltip("เป้าหมายสำหรับให้ข้อศอกหันไปหา (ถ้ามี)")]
    public Transform elbowHintTarget;

    [Range(0f, 1f)]
    [Tooltip("น้ำหนักการดึงข้อศอกไปหา Hint")]
    public float elbowWeight = 0.5f;

    private Animator _animator;

    void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    // OnAnimatorIK จะถูกเรียกโดยอัตโนมัติจาก Unity เมื่อติ๊ก IK Pass ใน Animator
    void OnAnimatorIK(int layerIndex)
    {
        if (_animator == null) return;

        AvatarIKGoal goalHand = (handSide == HandSide.RightHand) ? AvatarIKGoal.RightHand : AvatarIKGoal.LeftHand;
        AvatarIKHint hintElbow = (handSide == HandSide.RightHand) ? AvatarIKHint.RightElbow : AvatarIKHint.LeftElbow;

        if (racketHandleTarget != null)
        {
            // Set น้ำหนักของ Position และ Rotation
            _animator.SetIKPositionWeight(goalHand, positionWeight);
            _animator.SetIKRotationWeight(goalHand, rotationWeight);

            // ส่งค่า Position และ Rotation ของด้ามไม้ให้มือยึดตาม
            _animator.SetIKPosition(goalHand, racketHandleTarget.position);
            _animator.SetIKRotation(goalHand, racketHandleTarget.rotation);
        }
        else
        {
            // ถ้าไม่มี Target ให้คืนค่าน้ำหนักเป็น 0 เพื่อให้แขนเล่นแอนิเมชันปกติ
            _animator.SetIKPositionWeight(goalHand, 0);
            _animator.SetIKRotationWeight(goalHand, 0);
        }

        // กำหนดทิศทางข้อศอก (Elbow Hint)
        if (elbowHintTarget != null)
        {
            _animator.SetIKHintPositionWeight(hintElbow, elbowWeight);
            _animator.SetIKHintPosition(hintElbow, elbowHintTarget.position);
        }
        else
        {
            _animator.SetIKHintPositionWeight(hintElbow, 0);
        }
    }
}