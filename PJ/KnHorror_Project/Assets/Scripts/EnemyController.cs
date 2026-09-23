using UnityEngine;
using UnityEngine.AI; 
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))] // บังคับให้มี NavMeshAgent บนตัวมอนสเตอร์
public class EnemyController : MonoBehaviour
{
    [Header("Detection & Combat Settings")]
    public float detectionRange = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public int attackDamage = 25;
    public int maxHealth = 100;
    private int currentHealth;

    [Header("Spawn Delay Settings")]
    public float startDelay = 2f; // ⏳ เวลาที่รอก่อนจะเริ่มเปิดใช้งาน AI
    private bool isAimedActive = false; // สถานะว่า AI พร้อมทำงานหรือยัง

    [Header("UI & Effect Settings")]
    public Slider enemyHealthBar;
    public Canvas enemyCanvas;

    [Header("Animation Settings")]
    public Animator animator; // 🎬 สำหรับควบคุมอนิเมชั่น (ลาก Animator Component มาใส่ หรือจะให้มัน GetComponent อัตโนมัติก็ได้)

    [Header("Target References")]
    public Transform playerTransform;
    private PlayerStatus playerStatus;

    [Header("Events")]
    public UnityEvent EventDead;

    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isPlayerDown = false;
    private Vector3 roamPosition;
    private bool isDead = false; // 💀 ตัวแปรเช็คสถานะความตาย

    public GameObject VFX_Blood;
    public Transform VFX_Player;

    public AudioSource AudioSource;
    public AudioClip AudioClip_SFX_Dead;
    public AudioClip AudioClip_SFX_Attack;
    private void Start()
    {
        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        // ถ้าไม่ได้ลาก Animator ไว้ใน Inspector ให้ลองค้นหาจากตัวมอนสเตอร์อัตโนมัติ
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // ตั้งค่า Slider เริ่มต้น
        if (enemyHealthBar != null)
        {
            enemyHealthBar.maxValue = maxHealth;
            enemyHealthBar.value = currentHealth;
        }

        ToggleHealthBar(false);
        FindPlayer();

        // 🛑 ปิดการใช้งาน NavMeshAgent ชั่วคราวในช่วงแรก
        agent.enabled = false;

        // เริ่มนับเวลาถอยหลังเพื่อเปิดใช้งาน AI
        Invoke("ActivateAI", startDelay);
    }

    private void ActivateAI()
    {
        // ถ้าตายไปตั้งแต่ยังไม่เริ่ม ก็ไม่ต้องเปิด AI
        if (isDead) return;

        // ✅ เปิดใช้งาน NavMeshAgent หลังจากครบเวลา
        if (agent != null)
        {
            agent.enabled = true;
        }
        isAimedActive = true;
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
    }

    private void Update()
    {
        // 💀 ถ้ายตายแล้ว ให้หยุดการทำงานใน Update ทั้งหมดทันที
        if (isDead) return;

        // ⏳ ถ้ายังไม่หมดเวลา 2 วินาที จะไม่ให้มอนสเตอร์ทำพฤติกรรมใดๆ
        if (!isAimedActive) return;

        if (playerTransform == null || playerStatus == null)
        {
            FindPlayer();
            if (playerTransform == null || playerStatus == null) return;
        }

        // เช็คว่าผู้เล่นหมดสภาพหรือยัง
        bool isPlayerDead = playerStatus.IsPlayerDead();
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // แสดง/ซ่อน หลอดเลือดตามระยะ
        ToggleHealthBar(distanceToPlayer <= detectionRange);

        // ถ้าผู้เล่นล้มหรือตาย ให้เดินหนีแบบปลอดภัยไม่หลุดแมพ
        if (isPlayerDown || isPlayerDead)
        {
            RoamAwayFromPlayer();
            return;
        }

        // กรณีเจอ Player และอยู่ในระยะตรวจจับ
        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer > attackRange)
            {
                // เดินไล่ล่าตาม NavMesh
                MoveTowardsPlayer();
            }
            else
            {
                // หยุดเดินและหันหน้าโจมตี
                LookAndAttackPlayer();
            }
        }
        else
        {
            // นอกระยะตรวจจับ ให้หยุดเดิน
            if (agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
            }
        }
    }

    private void MoveTowardsPlayer()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        agent.isStopped = false;
        agent.SetDestination(playerTransform.position);
    }

    private void LookAndAttackPlayer()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        // หันหน้าไปหา Player
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        // โจมตีตาม Cooldown
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    private void RoamAwayFromPlayer()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        // สุ่มจุดถอยหนีให้อยู่บน NavMesh เพื่อไม่ให้หลุดแมพ
        if (roamPosition == Vector3.zero || agent.remainingDistance <= agent.stoppingDistance)
        {
            Vector3 awayDirection = (transform.position - playerTransform.position).normalized;
            Vector3 randomPoint = transform.position + awayDirection * 8f + Random.insideUnitSphere * 3f;

            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 5f, NavMesh.AllAreas))
            {
                roamPosition = hit.position;
                agent.isStopped = false;
                agent.SetDestination(roamPosition);
            }
        }
    }

    private void AttackPlayer()
    {
        if (playerStatus == null || playerStatus.IsPlayerDead()) return;
        AudioSource.PlayOneShot(AudioClip_SFX_Attack);
        animator.Play("Attack");
        playerStatus.DamageToPlayer(attackDamage);

        if (playerStatus.IsPlayerDead())
        {
            isPlayerDown = true;
        }
    }

    public void ResetPlayerDown()
    {
        isPlayerDown = false;
        roamPosition = Vector3.zero;
        lastAttackTime = Time.time;
    }

    private void OnTriggerEnter(Collider other)
    {
        // ถ้ายตายแล้วจะไม่รับดาเมจเพิ่ม
        if (isDead) return;

        if (other.CompareTag("HitBoxForMonster"))
        {
            TakeDamage(25);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        AudioSource.PlayOneShot(AudioClip_SFX_Dead);
        currentHealth -= damage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        if (enemyHealthBar != null)
        {
            enemyHealthBar.value = currentHealth;
        }

        ToggleHealthBar(true);
        if (currentHealth <= 0)
        {
            Die();
            return;
        }
        StartCoroutine(DamageRoutine());
    }
    private IEnumerator DamageRoutine()
    {
        if (VFX_Blood != null && VFX_Player != null)
        {
            GameObject blood = Instantiate(VFX_Blood);
            blood.transform.position = VFX_Player.transform.position;
        }
        yield return new WaitForSeconds(1f);
        if (AudioSource != null && !AudioSource.isPlaying)
        {
            AudioSource.Play();
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
        if (isDead) return; // ป้องกันการเรียกซ้ำ
        isDead = true;

        // 🔊 ใช้ PlayClipAtPoint เพื่อให้เสียงเล่นลอยค้างไว้ แม้ตัวมอนสเตอร์จะตายหรือถูกทำลายไปแล้ว
        if (AudioClip_SFX_Dead != null)
        {
            AudioSource.PlayClipAtPoint(AudioClip_SFX_Dead, transform.position);
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        ToggleHealthBar(false);
        if (animator != null)
        {
            animator.Play("Dead_00");
        }

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddMonsterKill();
        }

        EventDead.Invoke();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}