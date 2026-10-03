using StarterAssets;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;

public class Door : MonoBehaviour
{
    [Header("Door Settings")]
    public Animator Door_Animator;
    public string Anim_Name = "PushDoor";
    public string Fast_Anim_Name = "PushDoor_Faster"; // ⚡ ชื่ออนิเมชั่นแบบเฟียสๆ ตอนใส่ Perk
    public string Door_Open_Anim_Name = "Door_Open_00"; // 🚪 ชื่ออนิเมชั่นเปิดประตูของฝั่ง Door
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

        // 🔍 เช็ค Perk จาก PlayerPrefs (ถ้ามีค่าเป็น 1 แสดงว่าปลดล็อค Perk แล้ว)
        bool hasPerk01 = PlayerPrefs.GetInt("PERK_01", 0) == 1;

        // ⚙️ คำนวณความเร็ว: ถ้ามี Perk จะลดเวลาเคลื่อนที่ลง 15% (เร็วขึ้น)
        float currentMoveDuration = moveDuration;
        float currentWaitTime = waitTime;
        float doorAnimSpeed = 1f;
        float audioPitch = 1f; // 🎵 ตัวแปรคุมความเร็ว/Pitch ของเสียง

        if (hasPerk01)
        {
            currentMoveDuration *= 0.85f;
            currentWaitTime *= 0.85f;
            doorAnimSpeed = 1.9f;
            audioPitch = 1.25f; // 🎵 เร่ง Pitch ของเสียงขึ้น 25% (เพื่อให้เสียงกระชับและจบไวขึ้นตามอนิเมชั่น)
            Debug.Log("<color=cyan>✨ [Perk Active] PERK_01 ทำงาน! สลับไปเล่นอนิเมชั่น PushDoor_Faster และเร่งเสียงให้ไวขึ้น!</color>");
        }

        if (isLoadNextRoom)
        {
            RoomManager roomManager = FindAnyObjectByType<RoomManager>();
            if (roomManager != null)
                roomManager.LoadNextRoom();
        }

        // 🏃‍♂️ เช็คเงื่อนไขเลือกอนิเมชั่นผู้เล่น
        if (animator != null)
        {
            string targetAnim = hasPerk01 ? Fast_Anim_Name : Anim_Name;
            animator.speed = 1f;
            animator.Play(targetAnim);
        }

        // 🚪 สั่งเล่นอนิเมชั่นประตูพร้อมปรับ Speed ตาม Perk
        if (Door_Animator != null)
        {
            Door_Animator.enabled = true;
            Door_Animator.speed = doorAnimSpeed;
            Door_Animator.Play(Door_Open_Anim_Name);
        }

        if (tpc != null)
            tpc.CanMove = false;

        if (cc != null)
            cc.enabled = false;

        Vector3 startPosition = player.transform.position;
        Vector3 targetPosition = Player_P_T.position;
        Quaternion startRotation = player.transform.rotation;
        Quaternion targetRotation = Player_P_T.rotation;

        float elapsed = 0f;

        // 🎵 เล่นเสียงพร้อมกำหนด Pitch (ถ้าใส่ Perk เสียงจะเล่นไวและกระชับขึ้น)
        if (AS != null && AC != null)
        {
            AS.pitch = audioPitch; // ปรับความเร็วเสียงตาม Perk
            AS.PlayOneShot(AC);
        }

        // 🏃‍♂️ ใช้ค่า currentMoveDuration ที่คำนวณ Perk แล้วในการ Lerp ขยับตัวผู้เล่น
        while (elapsed < currentMoveDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / currentMoveDuration);
            t = Mathf.SmoothStep(0f, 1f, t);

            player.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            player.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);

            yield return null;
        }

        player.transform.SetPositionAndRotation(targetPosition, targetRotation);

        // 🔄 คืนค่า Pitch ของเสียงกลับมาเป็น 1 (ปกติ) เพื่อไม่ให้เสียงอื่นในเกมเพี้ยนตามไปด้วย
        if (AS != null)
        {
            AS.pitch = 1f;
        }

        if (cc != null)
            cc.enabled = true;

        if (tpc != null)
            tpc.enabled = true;

        if (Door_Animator != null)
            Door_Animator.enabled = true;

        yield return new WaitForSeconds(currentWaitTime);

        if (tpc != null)
            tpc.CanMove = true;

        isLoading = false;
    }
}