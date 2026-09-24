using StarterAssets;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public Animator Door_Animator;
    public string Anim_Name = "PushDoor";
    public Transform Player_P_T;
    public bool isLoadNextRoom = true;

    [Header("Timing Settings")]
    public float moveDuration = 0.5f;
    public float waitTime = 1f;

    [Header("Audio Settings")]
    public AudioSource AS;
    public AudioClip AC;

    [Header("UI References")]
    public GameObject Ui_Interactable; // ✨ ตอนนี้จะสลับการทำงานให้ปิดเมื่อเข้าเขต
    public GameObject KeyDisplay;
    public UnityEvent EventDone;

    [Header("References")]
    public GameObject G;

    private bool isLoading = false;

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Player_Basic_Controller playerController = other.GetComponent<Player_Basic_Controller>();
        if (playerController == null) return;

        if (playerController.PressingFire1)
        {
            StartPushDoor();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Ui_Manager uiManager = GameObject.FindGameObjectWithTag("Ui_Manager").GetComponent<Ui_Manager>();
            if (uiManager != null)
                uiManager.EnterCanInteract();

            if (KeyDisplay != null)
                KeyDisplay.SetActive(true);

            // ✨ เมื่อผู้เล่นเข้าเขต ให้ปิด Ui_Interactable (Active = false)
            if (Ui_Interactable != null)
                Ui_Interactable.SetActive(false);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Ui_Manager uiManager = GameObject.FindGameObjectWithTag("Ui_Manager").GetComponent<Ui_Manager>();
            if (uiManager != null)
                uiManager.ExitCanInteract();

            if (KeyDisplay != null)
                KeyDisplay.SetActive(false);

            // ✨ เมื่อผู้เล่นออกจากเขต ให้เปิด Ui_Interactable กลับมา (Active = true)
            if (Ui_Interactable != null)
                Ui_Interactable.SetActive(true);
        }
    }

    public void StartPushDoor()
    {
        EventDone.Invoke();
        Ui_Manager uiManager = GameObject.FindGameObjectWithTag("Ui_Manager").GetComponent<Ui_Manager>();
        if (uiManager != null)
            uiManager.ExitCanInteract();

        if (KeyDisplay != null)
            KeyDisplay.SetActive(false);

        if (isLoading) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        isLoading = true;

        StartCoroutine(TeleportPlayer(player));
    }

    private IEnumerator TeleportPlayer(GameObject player)
    {
        if (G != null)
            G.tag = "Untagged";

        if (Ui_Interactable != null)
            Ui_Interactable.SetActive(false);

        ThirdPersonController tpc = player.GetComponent<ThirdPersonController>();
        CharacterController cc = player.GetComponent<CharacterController>();
        Animator animator = player.GetComponentInChildren<Animator>();

        if (isLoadNextRoom)
        {
            RoomManager roomManager = FindAnyObjectByType<RoomManager>();
            if (roomManager != null)
                roomManager.LoadNextRoom();
        }

        if (animator != null)
            animator.Play(Anim_Name);

        if (tpc != null)
            tpc.CanMove = false;

        if (cc != null)
            cc.enabled = false;

        Vector3 startPosition = player.transform.position;
        Vector3 targetPosition = Player_P_T.position;
        Quaternion startRotation = player.transform.rotation;
        Quaternion targetRotation = Player_P_T.rotation;

        float elapsed = 0f;

        if (AS != null && AC != null)
            AS.PlayOneShot(AC);

        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / moveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            player.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            player.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        player.transform.SetPositionAndRotation(targetPosition, targetRotation);

        if (cc != null)
            cc.enabled = true;

        if (tpc != null)
            tpc.enabled = true;

        if (Door_Animator != null)
            Door_Animator.enabled = true;

        yield return new WaitForSeconds(waitTime);

        if (tpc != null)
            tpc.CanMove = true;

        isLoading = false;
    }
}