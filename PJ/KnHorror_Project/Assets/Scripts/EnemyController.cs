using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(NavMeshAgent))]
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
    public float startDelay = 2f;
    private bool isAimedActive = false;

    [Header("UI & Effect Settings")]
    public Slider enemyHealthBar;
    public Canvas enemyCanvas;

    [Header("Animation Settings")]
    public Animator animator;

    [Header("Target References")]
    public Transform playerTransform;
    private PlayerStatus playerStatus;

    [Header("Events")]
    public UnityEvent EventDead;
    public UnityEvent EventEvery5Seconds; // ⏱️ อีเวนต์ที่จะทำงานทุกๆ 5 วินาที

    private NavMeshAgent agent;
    private float lastAttackTime;
    private bool isPlayerDown = false;
    private Vector3 roamPosition;
    private bool isDead = false;

    public GameObject VFX_Blood;
    public Transform VFX_Player;

    public AudioSource AudioSource;
    public AudioClip AudioClip_SFX_Dead;
    public AudioClip AudioClip_SFX_Attack;

    [Header("Jump Attack Settings")]
    public float jumpSpeed = 5f;
    public float jumpHeight = 2f;
    private bool isJumping = false;

    public float WaitEvent;

    [Header("Destroy Settings")]
    public float destroyDelay = 1.5f; // ⏳ เวลาหลังจากตายก่อนที่จะ Destroy (1-2 วินาที)

    private void Start()
    {
        // ✨ ดึงค่าโบนัสดาเมจมอนสเตอร์จาก GameManager มาบวกเพิ่ม
        if (GameManager.Instance != null)
        {
            attackDamage += GameManager.Instance.monsterDamageBonus;
        }

        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (enemyHealthBar != null)
        {
            enemyHealthBar.maxValue = maxHealth;
            enemyHealthBar.value = currentHealth;
        }

        ToggleHealthBar(false);
        FindPlayer();

        agent.enabled = false;
        Invoke("ActivateAI", startDelay);

        // ⏱️ เริ่มต้นลูปการทำงานทุกๆ 5 วินาที
        StartCoroutine(TimerEventRoutine());
    }

    private void ActivateAI()
    {
        if (isDead) return;

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
        if (isDead || !isAimedActive) return;

        if (playerTransform == null || playerStatus == null)
        {
            FindPlayer();
            if (playerTransform == null || playerStatus == null) return;
        }

        bool isPlayerDead = playerStatus.IsPlayerDead();
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        ToggleHealthBar(distanceToPlayer <= detectionRange);

        if (isPlayerDown || isPlayerDead)
        {
            RoamAwayFromPlayer();
            return;
        }

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer > attackRange)
            {
                MoveTowardsPlayer();
            }
            else
            {
                LookAndAttackPlayer();
            }
        }
        else
        {
            if (agent.isOnNavMesh && !agent.isStopped)
            {
                agent.isStopped = true;
            }
        }
    }

    private IEnumerator TimerEventRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(WaitEvent);

            // ทำงานเฉพาะตอนที่ AI เปิดใช้งานแล้ว และยังไม่ตาย
            if (isAimedActive && !isDead)
            {
                EventEvery5Seconds.Invoke();
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

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0f;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            AttackPlayer();
            lastAttackTime = Time.time;
        }
    }

    private void RoamAwayFromPlayer()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

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
        if (isDead) return;
        isDead = true;

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

        // 🗑️ เริ่ม Coroutine เพื่อรอเวลาแล้วทำลาย GameObject ทิ้ง
        StartCoroutine(DestroyRoutine());
    }

    // ⏳ Coroutine หน่วงเวลาก่อน Destroy ตัวละคร
    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }

    public void JumpAttackPlayer()
    {
        if (isDead || isJumping || playerTransform == null) return;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Vector3 startPos = transform.position;
        Vector3 targetPos = playerTransform.position;

        Vector3 lookDir = (targetPos - startPos);
        lookDir.y = 0f;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(lookDir);
        }

        if (animator != null)
        {
            animator.Play("Jump");
        }

        StartCoroutine(JumpRoutine(startPos, targetPos));
    }

    private IEnumerator JumpRoutine(Vector3 startPos, Vector3 targetPos)
    {
        isJumping = true;
        float journeyLength = Vector3.Distance(new Vector3(startPos.x, 0, startPos.z), new Vector3(targetPos.x, 0, targetPos.z));
        float totalTime = journeyLength / jumpSpeed;
        float elapsedTime = 0f;

        while (elapsedTime < totalTime)
        {
            if (isDead) yield break;
            elapsedTime += Time.deltaTime;
            float linearProgress = elapsedTime / totalTime;
            linearProgress = Mathf.Clamp01(linearProgress);
            Vector3 currentPos = Vector3.Lerp(startPos, targetPos, linearProgress);
            float heightOffset = 4 * jumpHeight * linearProgress * (1f - linearProgress);
            currentPos.y = Mathf.Lerp(startPos.y, targetPos.y, linearProgress) + heightOffset;
            transform.position = currentPos;
            yield return null;
        }

        transform.position = new Vector3(targetPos.x, targetPos.y, targetPos.z);
        isJumping = false;

        if (agent != null && !isDead)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}