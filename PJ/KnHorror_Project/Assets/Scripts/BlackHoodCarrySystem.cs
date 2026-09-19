using UnityEngine;
using UnityEngine.AI;

public class BlackHoodCarrySystem : MonoBehaviour
{
    [Header("Movement")]
    public float stopDistance = 1.0f; // ปรับให้เท่ากับหรือใกล้เคียง carryDistance

    [Header("Carry")]
    public float carryDistance = 1.0f; // ระยะที่เริ่มอุ้ม

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
        StartWalk();
    }

    public void StartWalk()
    {
        if (agent == null || playerTransform == null)
            return;

        agent.isStopped = false;
        agent.stoppingDistance = stopDistance;
        agent.SetDestination(playerTransform.position);
    }

    private void TryCarryPlayer()
    {
        if (isCarrying || playerTransform == null)
            return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // เช็คว่าอยู่ในระยะอุ้มหรือยัง
        if (distance > carryDistance)
            return;

        PlayerStatus player = playerTransform.GetComponentInParent<PlayerStatus>();
        if (player == null)
            return;

        // เช็คเลือดผู้เล่น
        if (player.HealthBar != null && player.HealthBar.value <= 0f)
        {
            player.GotCarry(gameObject);
            player.transform.parent = transform;
            isCarrying = true;

            // หยุดเดินทันทีแบบเด็ดขาด
            agent.isStopped = true;
            agent.velocity = Vector3.zero;

            if (animator != null)
            {
                animator.Play("E_Carry");
            }

            Debug.Log("<color=magenta>[BlackHood] ถึงตัวและอุ้มผู้เล่นทันที!</color>");
        }
    }

    public void StartWalkingToTM()
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
        if (!isCarrying)
        {
            // พยายามเช็คเงื่อนไขการอุ้มตลอดเวลาในทุกๆ เฟรม ไม่ต้องรอให้หยุดเดินก่อน
            TryCarryPlayer();

            if (!isCarrying && playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);

                // อัปเดตเป้าหมายเดินตาม Player เรื่อยๆ ถ้ายังนอกระยะหยุด
                if (distance > stopDistance)
                {
                    if (agent.isStopped) agent.isStopped = false;
                    agent.SetDestination(playerTransform.position);
                }
                else
                {
                    // ถ้าอยู่ในระยะ stopDistance แล้ว ให้หยุดเดินเพื่อรอจังหวะอุ้ม (เลือดหมด)
                    if (!agent.isStopped) agent.isStopped = true;
                }
            }
        }

        // จัดการเรื่องเดินไปจุด TM ต่อ
        if (!isCarrying || targetTM == null)
            return;

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
            isCarrying = false;
        }
    }
}