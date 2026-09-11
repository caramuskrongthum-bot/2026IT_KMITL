using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("✨ Detection & Attack Settings ✨")]
    [Tooltip("ระยะที่ศัตรูจะเริ่มมองเห็นและเดินตามผู้เล่น")]
    public float detectionRange = 10f;

    [Tooltip("ระยะที่ศัตรูจะหยุดเดินแล้วเริ่มทำการโจมตี")]
    public float attackRange = 2f;

    [Tooltip("ความเร็วในการเดิน")]
    public float moveSpeed = 3.5f;

    [Tooltip("ดาเมจที่จะทำต่อการโจมตี 1 ครั้ง")]
    public int attackDamage = 25;

    [Tooltip("ความถี่ในการโจมตี")]
    public float attackCooldown = 1.5f;

    [Header("✨ References ✨")]
    public Transform playerTransform;

    private Animator playerAnimator;
    private PlayerStatus playerStatus;
    private CharacterController playerController;

    private float lastAttackTime;
    private bool isPlayerDown = false;
    private Vector3 roamDirection; // ทิศทางที่จะเดินไปตอนผู้เล่นล้ม


    private void Start()
    {
        FindPlayer();
    }


    private void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
        {
            Debug.LogWarning("EnemyController: ไม่พบ Player");
            return;
        }

        playerTransform = playerObj.transform;
        playerStatus = playerObj.GetComponent<PlayerStatus>();
        playerController = playerObj.GetComponent<CharacterController>();

        if (playerAnimator == null)
        {
            playerAnimator = playerObj.GetComponentInChildren<Animator>();
        }

        if (playerStatus == null)
        {
            Debug.LogWarning("EnemyController: Player ไม่มี PlayerStatus");
        }

        if (playerController == null)
        {
            Debug.LogWarning("EnemyController: Player ไม่มี CharacterController");
        }

        if (playerAnimator == null)
        {
            Debug.LogWarning("EnemyController: ไม่พบ Animator ของ Player");
        }
    }


    private void Update()
    {
        if (playerTransform == null || playerStatus == null)
        {
            return;
        }

        // ถ้าผู้เล่นล้ม ให้ศัตรูเดินไปทางอื่นแทน
        if (isPlayerDown)
        {
            RoamAwayFromPlayer();
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // เดินเข้าหา Player
        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
        // อยู่ในระยะโจมตี
        else if (distanceToPlayer <= attackRange)
        {
            LookAtPlayer();

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                Vector3 direction = playerTransform.position - transform.position;
                direction.y = 0f;

                AttackPlayer(direction);

                lastAttackTime = Time.time;
            }
        }
    }


    private void MoveTowardsPlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        transform.position += direction * moveSpeed * Time.deltaTime;
    }


    private void LookAtPlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f)
        {
            return;
        }

        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }


    // ฟังก์ชันเดินหนี/เดินไปทางอื่นตอนผู้เล่นล้ม
    private void RoamAwayFromPlayer()
    {
        if (roamDirection == Vector3.zero)
        {
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(roamDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        transform.position += roamDirection * moveSpeed * Time.deltaTime;
    }


    private void AttackPlayer(Vector3 hitDirection)
    {
        Debug.Log("💥 Enemy โจมตี Player!");

        if (playerStatus == null)
        {
            return;
        }

        float currentHealth = playerStatus.HealthBar.value;

        playerStatus.DamageToPlayer(attackDamage);

        if (playerStatus.HealthBar.value <= 0f && currentHealth > 0f)
        {
            PlayerDown();
        }
    }


    private void PlayerDown()
    {
        if (isPlayerDown)
        {
            return;
        }

        isPlayerDown = true;

        // สุ่มทิศทางเดินออกห่างจากตัวผู้เล่นตอนที่ล้ม (เดินถอยหลังหรือเดินเฉียงไปทางอื่น)
        Vector3 awayFromPlayer = transform.position - playerTransform.position;
        awayFromPlayer.y = 0f;

        // ถ้าไม่อยากให้เดินถอยหลังตรงๆ สามารถสุ่มเพิ่มมุมองศาได้ หรือให้เดินหนีไปทิศทางตรงข้ามเยื้องๆ
        roamDirection = (awayFromPlayer + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f))).normalized;

        StartCoroutine(HandlePlayerDownRoutine());
    }

    private System.Collections.IEnumerator HandlePlayerDownRoutine()
    {
        if (playerAnimator != null)
        {
            playerAnimator.applyRootMotion = true;
        }

        // รอ 1.5 วินาที
        yield return new WaitForSeconds(1.5f);

        if (playerAnimator != null)
        {
            playerAnimator.applyRootMotion = false;
        }

        Debug.Log("💀 Player ล้มและจบระยะ Root Motion แล้ว");
    }


    // เรียกใช้ฟังก์ชันนี้เมื่อผู้เล่นฟื้นคืนชีพ เพื่อให้ศัตรูกลับมาล่าต่อ
    public void ResetPlayerDown()
    {
        isPlayerDown = false;
        roamDirection = Vector3.zero;
        lastAttackTime = Time.time;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}