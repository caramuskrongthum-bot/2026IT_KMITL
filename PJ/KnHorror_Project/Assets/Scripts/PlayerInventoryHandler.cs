using UnityEngine;
using System.Collections.Generic;
using Tiny;

public class PlayerInventoryHandler : MonoBehaviour
{
    public List<GameObject> handItems = new List<GameObject>();

    private int currentEquippedIndex = -1;

    public GameObject Hand;
    public GameObject HitBox;

    private Transform mainCameraTransform;
    private bool isAimingWithCamera = false;

    public AudioSource AS;
    public AudioClip AC;

    public Trail Trail;
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

    private float lastAttackTime;
    public float attackCooldown = 1f;

    public void Attack_()
    {
        if (Time.time < lastAttackTime + attackCooldown) return;

        if (currentEquippedIndex == 0 && Hand.transform.localScale != Vector3.zero)
        {
            Animator animator = GetComponent<Animator>();
            if (animator != null)
            {
                lastAttackTime = Time.time;
                AS.PlayOneShot(AC);
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
        Trail.enabled = true;
        isAimingWithCamera = true;
    }

    public void DisableHitBox()
    {
        HitBox.SetActive(false);
        Trail.enabled = false;
        isAimingWithCamera = false;
    }
}