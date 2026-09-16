using UnityEngine;
using UnityEngine.AI;

public class BlackHoodCarrySystem : MonoBehaviour
{
    [Header("Movement")]
    public float stopDistance = 1.5f;

    private Animator animator;
    private NavMeshAgent agent;
    private Transform playerTransform;
    private Transform targetTM;
    private bool isCarrying = false;

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
        // เรียกใช้งาน StartWalk ตอนเริ่มเกมเพื่อให้เดินไปหาผู้เล่นทันที
        StartWalk();
    }

    /// <summary>
    /// 👑 ฟังก์ชัน public สำหรับสั่งให้ BlackHood เริ่มออกเดินตามหา Player
    /// </summary>
    public void StartWalk()
    {
        if (agent == null || playerTransform == null) return;

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("BlackHood: Agent ไม่ได้อยู่บน NavMesh ตอนพยายามเดิน!");
            return;
        }

        agent.isStopped = false;
        agent.stoppingDistance = stopDistance;
        agent.SetDestination(playerTransform.position);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isCarrying)
            return;

        PlayerStatus player = other.GetComponentInParent<PlayerStatus>();

        if (player != null)
        {
            if (player.HealthBar != null && player.HealthBar.value <= 0f)
            {
                player.GotCarry(gameObject);
                player.transform.parent = transform;

                isCarrying = true;
                agent.isStopped = true;

                if (animator != null)
                {
                    animator.Play("E_Carry");
                }
            }
        }
    }

    private void StartWalkingToTM()
    {
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
        agent.stoppingDistance = 0.5f; // ระยะประชิดจุดส่ง
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
            float distance = Vector3.Distance(
                transform.position,
                target.transform.position
            );

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
        // เช็คสถานะระหว่างเดินไปหาผู้เล่น (ก่อนจะอุ้ม)
        if (!isCarrying && playerTransform != null && !agent.pathPending)
        {
            // อัปเดตเป้าหมายหาผู้เล่นเรื่อยๆ จนกว่าจะถึงระยะหยุด
            if (Vector3.Distance(transform.position, playerTransform.position) > stopDistance && agent.isStopped == false)
            {
                agent.SetDestination(playerTransform.position);
            }
        }

        // เช็คสถานะตอนอุ้มผู้เล่นแล้ว และกำลังเดินไปส่งที่ TM
        if (!isCarrying || targetTM == null)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
            isCarrying = false;

            Debug.Log("BlackHood: ถึง TM เรียบร้อยแม่!");
        }
    }
}