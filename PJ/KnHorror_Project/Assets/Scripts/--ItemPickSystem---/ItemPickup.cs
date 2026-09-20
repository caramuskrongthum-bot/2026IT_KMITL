using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class ItemPickup : MonoBehaviour
{
    [Header("Item Data")]
    public ItemData itemData;

    [Header("Pickup Settings")]
    public float pickupDelay = 0.5f;
    public bool canBePickedUp { get; private set; } = false;

    [Header("Visual Feedback (Optional)")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.1f;
    public bool bobWhenIdle = true;

    [Header("Renderers")]
    public SpriteRenderer spriteRenderer; // ใช้ดึงภาพ Sprite Icon มาแสดงในฉาก 3D/2D[cite: 2]

    private Rigidbody rb;
    private Vector3 restPosition;
    private bool isSettled = false;
    private float settleCheckTimer = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    /// <summary>
    /// ใช้ตั้งค่าไอเทมและ Sprite ตาม ItemData[cite: 2]
    /// </summary>
    public void Initialize(ItemData data)
    {
        itemData = data;

        if (itemData != null && spriteRenderer != null && itemData.icon != null)
        {
            spriteRenderer.sprite = itemData.icon; // ดึง Sprite จาก ItemData[cite: 2]
        }
    }

    private void Start()
    {
        Invoke(nameof(EnablePickup), pickupDelay);

        // กรณีที่วางไอเทมใน Scene ไว้ล่วงหน้า ให้เรียก Initialize ตั้งแต่เริ่มต้น[cite: 2]
        if (itemData != null)
        {
            Initialize(itemData);
        }
    }

    private void EnablePickup()
    {
        canBePickedUp = true;
    }

    private void Update()
    {
        if (!isSettled)
        {
            settleCheckTimer += Time.deltaTime;
            if (settleCheckTimer > 0.2f)
            {
                settleCheckTimer = 0f;
                if (rb.linearVelocity.sqrMagnitude < 0.01f && rb.angularVelocity.sqrMagnitude < 0.01f)
                {
                    isSettled = true;
                    restPosition = transform.position;
                    rb.isKinematic = true;
                }
            }
        }
        else if (bobWhenIdle)
        {
            float newY = restPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            transform.Rotate(Vector3.up, 30f * Time.deltaTime, Space.World);
        }
    }

    public void OnPickedUp()
    {
        if (!canBePickedUp) return;
        Debug.Log($" Pick : {itemData.ITEM_NAME} (Value : {itemData.ITEM_VALUE})");
        bool success = InventorySlot.Instance.AddItem(itemData);
        if (success)
        {
            Destroy(gameObject);
        }
    }
}