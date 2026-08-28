using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DroppedItem : MonoBehaviour
{
    [Header("Item Reference")]
    [Tooltip("ข้อมูลไอเทมชิ้นนี้ (จะถูกตั้งค่าอัตโนมัติจาก ItemDropFountain)")]
    public ItemData itemData;

    [Header("Bouncing Settings (ตอนเด้งออก)")]
    [Tooltip("ระยะเวลาที่ไอเทมจะอยู่บนพื้นก่อนจะเริ่มลอยหาผู้เล่น")]
    public float groundedDuration = 0.5f;

    [Header("Magnet Settings (ตอนลอยหาผู้เล่น)")]
    [Tooltip("ความเร็วเริ่มต้นในการลอย")]
    public float initialMoveSpeed = 2f;
    [Tooltip("ความเร่ง (ความเร็วจะเพิ่มขึ้นเรื่อยๆ)")]
    public float acceleration = 5f;
    [Tooltip("ระยะห่างที่ถือว่าเก็บไอเทมได้แล้ว")]
    public float pickupDistance = 0.5f;

    private Rigidbody rb;
    private Transform playerTransform;
    private PlayerInventory playerInventory;
    private bool isFollowingPlayer = false;
    private float currentMoveSpeed;
    private GameObject player;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        currentMoveSpeed = initialMoveSpeed;
        player = GameObject.FindGameObjectWithTag("Player");
        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
    }

    /// <summary>
    /// รับค่า ItemData จาก ItemDropFountain
    /// </summary>
    public void SetItemData(ItemData data)
    {
        itemData = data;
    }

    /// <summary>
    /// รับแรงเด้งตอนถูกสปอว์นออกมา (เรียกโดย ItemDropFountain)
    /// </summary>
    public void ApplyInitialBounce(Vector3 force)
    {
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);

        StartCoroutine(WaitThenFollow());
    }

    private IEnumerator WaitThenFollow()
    {
        yield return new WaitForSeconds(groundedDuration);
        if (player != null)
        {
            playerTransform = player.transform;
            playerInventory = player.GetComponent<PlayerInventory>();
            isFollowingPlayer = true;

            // ปิดฟิสิกส์เพื่อไม่ให้ต้านการลอย
            rb.isKinematic = true;

            // หากมี Collider แบบ Solid ให้ปิดเพื่อกันดันผู้เล่น แต่ถ้ามี Trigger ในตัวให้เปิดไว้
            if (TryGetComponent<Collider>(out var col))
            {
                col.isTrigger = true;
            }
        }
    }

    void Update()
    {
            currentMoveSpeed += acceleration * Time.deltaTime;

            // เคลื่อนที่เข้าหาตำแหน่งผู้เล่น
            transform.position = Vector3.MoveTowards(transform.position, playerTransform.position, currentMoveSpeed * Time.deltaTime);

            // เมื่อถึงระยะเก็บไอเทม
            if (Vector3.Distance(transform.position, playerTransform.position) < pickupDistance)
            {
                CollectItem();
            }
    }

    private void CollectItem()
    {
        if (playerInventory != null && itemData != null)
        {
            bool wasPickedUp = playerInventory.AddItem(itemData);

            if (wasPickedUp)
            {
                Debug.Log($"เก็บไอเทม {itemData.itemName} เรียบร้อย!");
                Destroy(gameObject);
            }
            else
            {
                // กรณีกระเป๋าเต็ม ให้ยกเลิกการลอยแล้วปล่อยตกพื้นใหม่
                isFollowingPlayer = false;
                rb.isKinematic = false;
                if (TryGetComponent<Collider>(out var col))
                {
                    col.isTrigger = false;
                }
                Debug.LogWarning("กระเป๋าเต็ม! ไม่สามารถเก็บไอเทมได้");
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
}