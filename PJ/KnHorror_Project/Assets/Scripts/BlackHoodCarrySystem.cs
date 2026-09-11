using UnityEngine;

public class BlackHoodCarrySystem : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2.5f;
    public float stopDistance = 1.5f;
    public float rotationSpeed = 8f;

    private Animator animator;
    private Transform targetTM;
    private bool isMoving;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        PlayerStatus player = other.GetComponent<PlayerStatus>();

        if (player == null)
            return;

        if (player.HealthBar.value <= 0)
        {
            player.GotCarry(gameObject);

            animator.Play("E_Carry");
        }
    }

    public void StartWalking()
    {
        targetTM = FindNearestTM();

        if (targetTM != null)
        {
            isMoving = true;
        }
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
        if (!isMoving || targetTM == null)
            return;

        Vector3 direction = targetTM.position - transform.position;

        direction.y = 0f;

        if (direction.magnitude <= stopDistance)
        {
            isMoving = false;
            return;
        }

        direction.Normalize();

        transform.position += direction * moveSpeed * Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            targetRotation,
            rotationSpeed * Time.deltaTime
        );
    }
}