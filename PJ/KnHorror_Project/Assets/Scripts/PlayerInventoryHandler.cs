using UnityEngine;
using System.Collections.Generic;

public class PlayerInventoryHandler : MonoBehaviour
{
    public List<GameObject> handItems = new List<GameObject>();

    private int currentEquippedIndex = -1;

    public GameObject Hand;
    public GameObject HitBox;

    private Transform mainCameraTransform;
    private bool isAimingWithCamera = false;

    void Start()
    {
        UnequipAll();

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }
    }

    void Update()
    {
        // ถ้าเปิด HitBox อยู่ ให้ผู้เล่นหันตาม Main Camera เฉพาะแกน Y แบบ Smooth
        if (isAimingWithCamera && mainCameraTransform != null)
        {
            Vector3 camForward = mainCameraTransform.forward;
            camForward.y = 0f; // ล็อคแกน Y ไม่ให้ก้มเงย

            if (camForward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(camForward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 15f);
            }
        }
    }

    public void EquipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= handItems.Count) return;

        // กดซ้ำช่องเดิม = เก็บของเข้าตัว
        if (currentEquippedIndex == slotIndex)
        {
            UnequipAll();
            currentEquippedIndex = -1;
            return;
        }

        UnequipAll();

        if (handItems[slotIndex] != null)
        {
            handItems[slotIndex].SetActive(true);
            currentEquippedIndex = slotIndex;
        }
    }

    public void UnequipAll()
    {
        foreach (GameObject item in handItems)
        {
            if (item != null) item.SetActive(false);
        }
    }

    public void Attack_()
    {
        if (currentEquippedIndex == 0 && Hand.transform.localScale != Vector3.zero)
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                animator.Play("Attack");
            }
        }
    }

    public void ShowItemHand()
    {
        Hand.transform.localScale = Vector3.one;
    }

    public void HideItemHand()
    {
        Hand.transform.localScale = Vector3.zero;
    }

    public void EnableHitBox()
    {
        HitBox.SetActive(true);

        // เริ่มต้นหันตามกล้องตอนเปิด HitBox
        isAimingWithCamera = true;
    }

    public void DisableHitBox()
    {
        HitBox.SetActive(false);

        // หยุดการหันตามกล้อง ปล่อยให้ผู้เล่นกลับไปควบคุมการหมุนปกติ
        isAimingWithCamera = false;
    }
}