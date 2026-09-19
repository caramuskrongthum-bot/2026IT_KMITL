using UnityEngine;
using System.Collections.Generic;
using Tiny;
using StarterAssets;

public class PlayerInventoryHandler : MonoBehaviour
{
    public List<GameObject> handItems = new List<GameObject>();

    private int currentEquippedIndex = -1;

    public GameObject Hand;
    public GameObject HitBox;

    public GameObject MOBILE_ATTACK_BTN;

    public GameObject Light;

    private Transform mainCameraTransform;
    private bool isAimingWithCamera = false;

    public AudioSource AS;
    public AudioClip AC;

    public Trail Trail;

    private Animator animator;
    public float layerTransitionSpeed = 5f;

    [Header("Fog Settings")]
    public float normalFogDensity = 0.4f;
    public float item2FogDensity = 0.123f;
    public float fogTransitionSpeed = 2f;

    [Header("Catching")]
    private ThirdPersonController thirdPersonController;
    private bool lastCatchingState = false;

    void Start()
    {
        animator = GetComponent<Animator>();

        thirdPersonController = GetComponent<ThirdPersonController>();

        UnequipAll();

        if (Camera.main != null)
        {
            mainCameraTransform = Camera.main.transform;
        }

        if (handItems.Count > 0)
        {
            EquipSlot(0);
        }

        if (thirdPersonController != null)
        {
            lastCatchingState = thirdPersonController.IsCatching;

            if (lastCatchingState)
                HideItemHand();
            else
                ShowItemHand();
        }
    }

    void Update()
    {
        // ==========================================
        // CHECK CATCHING STATE
        // ==========================================

        if (thirdPersonController != null)
        {
            bool currentCatchingState = thirdPersonController.IsCatching;

            if (currentCatchingState != lastCatchingState)
            {
                lastCatchingState = currentCatchingState;

                if (currentCatchingState)
                {
                    HideItemHand();
                }
                else
                {
                    ShowItemHand();
                }
            }
        }

        // ==========================================
        // CAMERA AIM
        // ==========================================

        if (isAimingWithCamera && mainCameraTransform != null)
        {
            Vector3 camForward = mainCameraTransform.forward;
            camForward.y = 0f;

            if (camForward.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(camForward);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * 15f
                );
            }
        }

        // ==========================================
        // UI
        // ==========================================

        MOBILE_ATTACK_BTN.SetActive(currentEquippedIndex == 0);
        Light.SetActive(currentEquippedIndex == 1);

        // ==========================================
        // ANIMATOR LAYER
        // ==========================================

        if (animator != null && animator.layerCount > 2)
        {
            float targetLayerWeight = (currentEquippedIndex == 1) ? 1f : 0f;

            float currentWeight = animator.GetLayerWeight(2);

            float newWeight = Mathf.MoveTowards(
                currentWeight,
                targetLayerWeight,
                Time.deltaTime * layerTransitionSpeed
            );

            animator.SetLayerWeight(2, newWeight);
        }

        // ==========================================
        // FOG
        // ==========================================

        if (RenderSettings.fog)
        {
            float targetFog =
                (currentEquippedIndex == 1)
                ? item2FogDensity
                : normalFogDensity;

            RenderSettings.fogDensity = Mathf.MoveTowards(
                RenderSettings.fogDensity,
                targetFog,
                Time.deltaTime * fogTransitionSpeed
            );
        }
    }

    public void EquipSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= handItems.Count)
            return;

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
            if (item != null)
                item.SetActive(false);
        }
    }

    private float lastAttackTime;
    public float attackCooldown = 1f;

    public void Attack_()
    {
        if (Time.time < lastAttackTime + attackCooldown)
            return;

        ThirdPersonController T = GetComponent<ThirdPersonController>();

        if (currentEquippedIndex == 0 &&
            T.CanMove == true &&
            T.IsCatching == false)
        {
            if (animator != null)
            {
                lastAttackTime = Time.time;

                AS.PlayOneShot(AC);

                animator.Play("Attack");
            }
        }
    }

    // ==========================================
    // HAND ITEM
    // ==========================================

    public void ShowItemHand()
    {
        if (Hand != null)
        {
            Hand.transform.localScale = Vector3.one;
        }
    }

    public void HideItemHand()
    {
        if (Hand != null)
        {
            Hand.transform.localScale = Vector3.zero;
        }
    }

    // ==========================================
    // HIT BOX
    // ==========================================

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