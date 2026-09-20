using UnityEngine;

// ติดสคริปต์นี้กับ Main Camera หรือตัวผู้เล่น โดยให้ลาก Player_Basic_Controller มาใส่ช่อง Reference
public class PlayerItemPicker : MonoBehaviour
{
    [Header("References")]
    [Tooltip("กล้องผู้เล่น ถ้าไม่ตั้งจะใช้ Camera.main")]
    public Camera playerCamera;

    [Tooltip("ลากสคริปต์ Player_Basic_Controller ของผู้เล่นมาใส่ตรงนี้ เพื่อเช็คสถานะการกดปุ่ม (Fire2)")]
    public Player_Basic_Controller playerController;

    [Header("Pickup Settings")]
    [Tooltip("ระยะที่ยิง Ray ออกไปเพื่อตรวจจับไอเทม")]
    public float pickupRange = 3f;

    [Tooltip("Layer ของไอเทม (ตั้งใน Inspector ให้ตรงกับ Layer ที่ใช้กับพรีแฟบไอเทมเช็ค)")]
    public LayerMask itemLayer;

    [Header("Events / UI (Optional)")]
    public GameObject pickupPromptUI; // เช่น UI แสดงข้อความ "กดเพื่อเก็บ" ตอนเล็งโดนไอเทม

    private ItemPickup currentTarget;
    private bool wasPressingLastFrame = false; // เอาไว้เช็คจังหวะกดลงแค่ 1 ครั้ง (Trigger) ตอนผู้เล่นแตะปุ่ม

    private void Awake()
    {
        if (playerCamera == null)
            playerCamera = Camera.main;

        // ถ้าไม่ได้ลากใส่ Inspector ไว้ ลองหาจาก Scene อัตโนมัติ
        if (playerController == null)
        {
            playerController = FindObjectOfType<Player_Basic_Controller>();
        }
    }

    private void Update()
    {
        // 1. เช็คว่ากล้องมอง Object อยู่ไหม (อัปเดต currentTarget และ UI)
        DetectItem();

        // 2. เช็คสถานะการกดปุ่ม Fire2 จาก Controller
        if (playerController != null && currentTarget != null)
        {
            // เปลี่ยนมาใช้ PressingFire2 สำหรับระบบเก็บของโดยเฉพาะ
            bool isPressing = playerController.PressingFire2;

            // เช็คจังหวะกดลงพอดีในเฟรมนี้ (ป้องกันการรัวกดค้าง)
            bool isPressedThisFrame = isPressing && !wasPressingLastFrame;

            if (isPressedThisFrame)
            {
                currentTarget.OnPickedUp();
                currentTarget = null;
                SetPrompt(false);

                // ปิด UI โต้ตอบเมื่อเก็บไอเทมไปแล้ว
                Ui_Manager uiManager = FindObjectOfType<Ui_Manager>();
                if (uiManager != null) uiManager.ExitCanInteract();
            }

            wasPressingLastFrame = isPressing;
        }
        else
        {
            wasPressingLastFrame = false;
        }
    }

    private void DetectItem()
    {
        // ยิง Ray จากกึ่งกลางจอ (แกนกลางกล้อง) ตรงไปข้างหน้า เหมาะกับเกม First-Person บนมือถือ
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));

        if (Physics.Raycast(ray, out RaycastHit hit, pickupRange, itemLayer))
        {
            ItemPickup item = hit.collider.GetComponentInParent<ItemPickup>();

            if (item != null && item.canBePickedUp)
            {
                if (currentTarget != item)
                {
                    currentTarget = item;
                    SetPrompt(true); // เปิด UI บอกว่าเล็งโดนแล้ว

                    // เรียกใช้ UI Manager เมื่อเริ่มมองเห็นไอเทม
                    Ui_Manager uiManager = FindObjectOfType<Ui_Manager>();
                    if (uiManager != null) uiManager.EnterCanInteract();
                }
                return;
            }
        }

        // ไม่ได้เล็งโดนไอเทมที่เก็บได้ (หลุดจากเป้าหมาย)
        if (currentTarget != null)
        {
            currentTarget = null;
            SetPrompt(false); // ปิด UI

            // เรียกใช้ UI Manager เมื่อเลิกมองไอเทม
            Ui_Manager uiManager = FindObjectOfType<Ui_Manager>();
            if (uiManager != null) uiManager.ExitCanInteract();
        }
    }

    private void SetPrompt(bool show)
    {
        if (pickupPromptUI != null)
            pickupPromptUI.SetActive(show);
    }

    // วาดเส้น Gizmo เช็คระยะ Raycast ใน Scene View
    private void OnDrawGizmosSelected()
    {
        if (playerCamera == null) return;
        Gizmos.color = Color.yellow;
        Ray gizmoRay = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        Gizmos.DrawLine(gizmoRay.origin, gizmoRay.origin + gizmoRay.direction * pickupRange);
    }
}