using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI; // จำเป็นสำหรับการใช้งาน UI Slider

public class EnemyController : MonoBehaviour
{
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float moveSpeed = 3.5f;
    public int attackDamage = 25;
    public float attackCooldown = 1.5f;
    public int maxHealth = 100;
    private int currentHealth;
    public float knockbackForce = 5f;
    public Slider enemyHealthBar;
    public Canvas enemyCanvas;
    public Transform playerTransform;
    private Animator playerAnimator;
    private PlayerStatus playerStatus;
    private CharacterController playerController;
    private Rigidbody rb;
    private float lastAttackTime;
    private bool isPlayerDown = false;
    private Vector3 roamDirection;
    public UnityEvent EventDead;

    private void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody>();

        // ตั้งค่า Slider เริ่มต้น
        if (enemyHealthBar != null)
        {
            enemyHealthBar.maxValue = maxHealth;
            enemyHealthBar.value = currentHealth;
        }

        ToggleHealthBar(false);

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
    }

    private void Update()
    {
        if (playerTransform == null || playerStatus == null)
        {
            return;
        }

        // 🛑 เช็คว่าผู้เล่นเลือดหมดหรือยัง
        bool isPlayerDead = (playerStatus.HealthBar != null && playerStatus.HealthBar.value <= 0f);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // เช็คว่าอยู่ในระยะแสดงหลอดเลือดไหม (ใช้ detectionRange เป็นเกณฑ์)
        if (distanceToPlayer <= detectionRange)
        {
            ToggleHealthBar(true);
        }
        else
        {
            ToggleHealthBar(false);
        }

        if (isPlayerDown || isPlayerDead)
        {
            RoamAwayFromPlayer();
            return;
        }

        if (distanceToPlayer <= detectionRange && distanceToPlayer > attackRange)
        {
            MoveTowardsPlayer();
        }
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

        if (direction.sqrMagnitude <= 0.001f) return;

        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        transform.position += direction * moveSpeed * Time.deltaTime;
    }

    private void LookAtPlayer()
    {
        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude <= 0.001f) return;

        direction.Normalize();

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
    }

    private void RoamAwayFromPlayer()
    {
        if (roamDirection == Vector3.zero) return;

        Quaternion targetRotation = Quaternion.LookRotation(roamDirection);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);

        transform.position += roamDirection * moveSpeed * Time.deltaTime;
    }

    private void AttackPlayer(Vector3 hitDirection)
    {
        if (playerStatus == null) return;
        if (playerStatus.HealthBar != null && playerStatus.HealthBar.value <= 0f) return;
        playerStatus.DamageToPlayer(attackDamage);
        if (playerStatus.HealthBar != null && playerStatus.HealthBar.value <= 0f)
        {
            PlayerDown();
        }
    }

    private void PlayerDown()
    {
        if (isPlayerDown) return;

        isPlayerDown = true;
        Vector3 awayFromPlayer = transform.position - playerTransform.position;
        awayFromPlayer.y = 0f;
        roamDirection = (awayFromPlayer + new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f))).normalized;
    }

    public void ResetPlayerDown()
    {
        isPlayerDown = false;
        roamDirection = Vector3.zero;
        lastAttackTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("HitBoxForMonster"))
        {
            TakeDamage(25, other.transform.position);
        }
    }

    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // อัปเดตค่า Slider เลือดมอนสเตอร์
        if (enemyHealthBar != null)
        {
            enemyHealthBar.value = currentHealth;
        }

        // แสดงหลอดเลือดทันทีเมื่อโดนตี
        ToggleHealthBar(true);

        // ทำการกระเด็นถอยหลัง (Knockback)
        Vector3 knockbackDir = (transform.position - attackerPosition).normalized;
        knockbackDir.y = 0f;

        if (rb != null)
        {
            rb.AddForce(knockbackDir * knockbackForce, ForceMode.Impulse);
        }
        else
        {
            transform.position += knockbackDir * (knockbackForce * 0.2f);
        }

        // เช็คว่าเลือดหมดหรือยัง
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void ToggleHealthBar(bool isVisible)
    {
        if (enemyCanvas != null)
        {
            enemyCanvas.gameObject.SetActive(isVisible);
        }
        else if (enemyHealthBar != null)
        {
            enemyHealthBar.gameObject.SetActive(isVisible);
        }
    }

    private void Die()
    {
        EventDead.Invoke();
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}