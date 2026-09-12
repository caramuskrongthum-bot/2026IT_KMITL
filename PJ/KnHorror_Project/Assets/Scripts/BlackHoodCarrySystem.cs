using UnityEngine;
using UnityEngine.AI;

public class BlackHoodCarrySystem : MonoBehaviour
{
    [Header("Movement")]
    public float stopDistance = 1.5f;

    private Animator animator;
    private NavMeshAgent agent;

    private Transform targetTM;
    private bool isCarrying;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();

        agent.stoppingDistance = stopDistance;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerStatus player = other.GetComponentInParent<PlayerStatus>();

        if (player == null)
            return;

        if (player.HealthBar.value <= 0)
        {
            player.GotCarry(gameObject);

            isCarrying = true;

            agent.isStopped = true;

            animator.Play("E_Carry");

            Invoke(nameof(StartWalkingToTM), 1f);
        }
    }

    private void StartWalkingToTM()
    {
        targetTM = FindNearestTM();

        if (targetTM == null)
        {
            Debug.LogWarning("BlackHood: ไม่พบ TM");
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogWarning("BlackHood: Agent ไม่ได้อยู่บน NavMesh");
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(targetTM.position);
    }

    private Transform FindNearestTM()
    {
        GameObject[] targets = GameObject.FindGameObjectsWithTag("TM");

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
        if (!isCarrying || targetTM == null)
            return;

        if (!agent.pathPending &&
            agent.remainingDistance <= agent.stoppingDistance)
        {
            agent.isStopped = true;
            isCarrying = false;

            Debug.Log("BlackHood: ถึง TM แล้ว");
        }
    }
}