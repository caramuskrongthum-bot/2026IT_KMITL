using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

public class BlackHoodCarrySystem : MonoBehaviour
{
    [Header("Movement")]
    public float stopDistance = 1.2f; // ขยายระยะหยุดนิดหน่อยให้กว้างกว่าระยะอุ้มเล็กหน่อย
    public float baseSpeed = 3.5f;     // 🏃‍♂️ ความเร็วเริ่มต้นของ Black Hood

    [Header("Carry & Speed Scaling")]
    public float carryDistance = 1.5f; // ขยายระยะอุ้มให้หยิบถึงง่ายขึ้น ไม่ต้องแนบชิดเป๊ะๆ
    public float speedIncreaseRate = 2f; // ⚡ ทุก 1 วินาที ความเร็วจะเพิ่มขึ้นเท่านี้ (+2 ต่อวิ)
    public float maxSpeed = 15f;         // 🚀 ความเร็วสูงสุด (กันไม่ให้มันบินเร็วเกินหลุด NavMesh)
    public float maxSearchTimeBeforeTeleport = 15f; // ⏳ ถ้าเดินหาเกิน 15 วินาที วาร์ปมาหาผู้เล่นทันที!

    private Animator animator;
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Transform targetTM;
    private bool isCarrying = false;
    private bool hasTriggeredCarry = false; // ป้องกันการเรียกซ้ำซ้อน

    private float currentSpeed;
    private float searchTimer = 0f;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
        else
        {
            Debug.LogWarning("BlackHood: ไม่พบ Player ในฉาก!");
        }
    }

    private void Start()
    {
        StartWalk();
    }

    public void StartWalk()
    {
        if (agent == null || playerTransform == null)
            return;

        hasTriggeredCarry = false;
        isCarrying = false;
        agent.enabled = true;
        agent.isStopped = false;
        agent.stoppingDistance = stopDistance;

        // เริ่มต้นด้วยความเร็วพื้นฐานและรีเซ็ตเวลาค้นหา
        currentSpeed = baseSpeed;
        agent.speed = currentSpeed;
        searchTimer = 0f;

        agent.SetDestination(playerTransform.position);
    }

    private void TryCarryPlayer()
    {
        if (hasTriggeredCarry || playerTransform == null)
            return;

        // ใช้ Vector3.Distance แบบไม่คิดแกน Y (Flat Distance) เพื่อป้องกันปัญหาความสูงไม่เท่ากันแล้ววัดระยะพลาด
        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                          new Vector3(playerTransform.position.x, 0, playerTransform.position.z));

        // เช็คว่าอยู่ในระยะอุ้มหรือยัง
        if (distance > carryDistance)
            return;

        PlayerStatus player = playerTransform.GetComponentInParent<PlayerStatus>();
        if (player == null)
        {
            Debug.LogWarning("BlackHood: ไม่พบ Component PlayerStatus บนตัว Player หรือ Parent!");
            return;
        }

        // เช็คเลือดผู้เล่น (ถ้าเลือดหมดหรือน้อยกว่าหรือเท่ากับ 0)
        if (player.HealthBar != null && player.HealthBar.value <= 0f)
        {
            TriggerCarrySuccess(player);
        }
    }

    private void TriggerCarrySuccess(PlayerStatus player)
    {
        hasTriggeredCarry = true;
        isCarrying = true;

        player.GotCarry(gameObject);
        player.transform.parent = transform;

        // หยุดเดินทันทีแบบเด็ดขาด และรีเซ็ตความเร็วกลับเป็นค่าเริ่มต้นตอนอุ้มเสร็จ
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.speed = baseSpeed;
        agent.enabled = false; // ปิด Agent ชั่วคราวตอนอุ้มเพื่อไม่ให้มันฝืนขยับ

        if (animator != null)
        {
            animator.Play("E_Carry");
        }

        Debug.Log("BlackHood: อุ้มผู้เล่นสำเร็จแล้วจ้า! รีเซ็ตความเร็วกลับปกติเรียบร้อย");
    }

    public void StartWalkingToTM()
    {
        // เปิด NavMeshAgent กลับมาใช้งานกรณีที่เคยปิดตอนอุ้ม
        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        targetTM = FindNearestTM();

        if (targetTM == null)
        {
            Debug.LogWarning("BlackHood: ไม่พบจุด TM (Target Marker) ในฉาก!");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("BlackHood: Agent ไม่ได้อยู่บน NavMesh ตอนพยายามไป TM");
            return;
        }

        if (animator != null)
        {
            animator.Play("Walk_Lower");
        }

        agent.isStopped = false;
        agent.speed = baseSpeed; // เดินเอาศพไปส่งที่ TM ด้วยความเร็วปกติ
        agent.stoppingDistance = 0.5f;
        agent.SetDestination(targetTM.position);
    }

    private Transform FindNearestTM()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("TM");
        if (targets.Length == 0) return null;

        Transform nearest = null;
        float nearestDistance = Mathf.Infinity;

        foreach (GameObject target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.transform.position);
            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearest = target.transform;
            }
        }
        return nearest;
    }

    private void Update()
    {
        // ถ้ายังไม่ได้อุ้มและยังทำภารกิจช่วงแรกอยู่
        if (!hasTriggeredCarry)
        {
            // ตรวจสอบเงื่อนไขการอุ้มตลอดเวลา
            TryCarryPlayer();

            if (!hasTriggeredCarry && playerTransform != null && agent.enabled && agent.isOnNavMesh)
            {
                // ⏱️ ระบบจับเวลาค้นหา (ถ้าเกิน 15 วินาทีแล้วยังไม่ถึงตัวผู้เล่น ให้วาร์ปไปโผล่ข้างหลังเลย!)
                searchTimer += Time.deltaTime;
                if (searchTimer >= maxSearchTimeBeforeTeleport)
                {
                    TeleportBehindPlayer();
                    return;
                }

                // ⚡ ระบบเพิ่มความไวขึ้นเรื่อยๆ (+2 ต่อวินาที จนกว่าจะถึง maxSpeed)
                if (currentSpeed < maxSpeed)
                {
                    currentSpeed += speedIncreaseRate * Time.deltaTime;
                    currentSpeed = Mathf.Min(currentSpeed, maxSpeed); // จำกัดไม่ให้เกินค่าสูงสุด
                    agent.speed = currentSpeed;
                }

                float distance = Vector3.Distance(transform.position, playerTransform.position);

                // อัปเดตเป้าหมายเดินตาม Player เรื่อยๆ
                if (distance > stopDistance)
                {
                    if (agent.isStopped) agent.isStopped = false;
                    agent.SetDestination(playerTransform.position);
                }
                else
                {
                    // ถ้าถึงระยะหยุด ให้หยุดแล้วหันหน้าหา Player
                    if (!agent.isStopped) agent.isStopped = true;

                    Vector3 direction = (playerTransform.position - transform.position).normalized;
                    direction.y = 0;
                    if (direction != Vector3.zero)
                    {
                        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 10f);
                    }
                }
            }
            return;
        }

        // จัดการเรื่องเดินไปจุด TM ต่อ (หลังจากอุ้มแล้ว และมี targetTM)
        if (isCarrying && targetTM != null)
        {
            if (agent.enabled && agent.isOnNavMesh && !agent.pathPending)
            {
                if (agent.remainingDistance <= agent.stoppingDistance)
                {
                    agent.isStopped = true;
                    isCarrying = false;
                    SceneManager.LoadScene("GameOver_01");
                }
            }
        }
    }

    // 🌀 ฟังก์ชันวาร์ปดักหลังผู้เล่น กรณีที่เดินหากินเวลานานเกิน 15 วินาที
    private void TeleportBehindPlayer()
    {
        if (playerTransform == null) return;

        // คำนวณตำแหน่งด้านหลังผู้เล่น (ถอยหลังจากหน้าผู้เล่นไป 1.5 เมตร)
        Vector3 teleportPos = playerTransform.position - (playerTransform.forward * 1.5f);

        // เช็คว่าตำแหน่งวาร์ปอยู่บน NavMesh ไหม เพื่อป้องกันจมพื้น
        NavMeshHit hit;
        if (NavMesh.SamplePosition(teleportPos, out hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
        else
        {
            transform.position = teleportPos; // ถ้าหาไม่เจอให้วางตำแหน่งนั้นตรงๆ เลย
        }

        Debug.Log("<color=magenta>👻 BlackHood: เดินหาไม่เจอ วาร์ปมาดักหลังผู้เล่นแบบเซอร์ไพรส์แล้วจ่ะแม่!</color>");

        // รีเซ็ตเวลาจับความเร็วนิดหน่อยหลังวาร์ป
        searchTimer = 0f;
    }
}