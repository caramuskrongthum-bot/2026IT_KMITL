using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;
using StarterAssets;

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

    [Header("Events & Timers")]
    public UnityEvent EventDead;
    public UnityEvent EventEvery5Seconds;
    public UnityEvent MakePlayerDizzy; // 🌀 อีเวนต์ทำให้ผู้เล่นสตั๊น/มึนงง
    public float eventInterval = 5f;   // ⏱️ ปรับตั้งเวลากี่วินาทีให้อีเวนต์ทำงานตรงนี้ได้เลยแม่!

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
    public float destroyDelay = 1.5f;

    [Header("🔥 Custom Weird Attack Modes (False by Default)")]
    public bool isSCPMode = false;                  // 1.1) ระบบ SCP
    public bool isTeleportAssassinate = false;     // 1.2) วาปไปข้างหลังผู้เล่น
    public bool isBullRushMode = false;            // 1.3) โหมดพุ่งชนแบบกระทิง (ทำงานตาม Timer ตัวนี้ด้วย)
    public bool isGazeStunMode = false;            // 1.4) จ้องหน้าแล้วทำให้สตั๊น

    [Header("🐂 Bull Rush Settings")]
    public float bullRushSpeed = 15f;              // ⚡ ความเร็วในการพุ่งชน
    public float bullRushDelay = 1.5f;             // ⏱️ เวลารอก่อนพุ่ง (ยืนเล็งเป้ากี่วินาทีก่อนพุ่งชน ปรับตรงนี้ได้เลยแม่!)

    [Header("👁️ SCP Extra Rules")]
    public float scpTriggerDistance = 3.5f;        // ระยะประชิดอันตราย
    public float scpStopDelay = 1.0f;              // ดีเลย์ 1 วินาทีก่อนหยุดเดินเมื่อหันไปมอง

    private Camera mainCamera;
    private bool isExecutingSpecialAction = false;
    private bool wasVisibleLastFrame = false;
    private float scpDelayTimer = 0f;

    public ThirdPersonController ThirdPersonController;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            attackDamage += GameManager.Instance.monsterDamageBonus;
        }

        currentHealth = maxHealth;
        agent = GetComponent<NavMeshAgent>();
        mainCamera = Camera.main;

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

        StartCoroutine(TimerEventRoutine());
        StartCoroutine(WeirdMechanicsRoutine());
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
        ThirdPersonController = playerObj.GetComponent<ThirdPersonController>();
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
        if (isDead || !isAimedActive || isExecutingSpecialAction) return;

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

        // ==========================================
        // 👁️✨ ระบบ SCP
        // ==========================================
        if (isSCPMode)
        {
            bool isCurrentlyVisible = IsEnemyVisibleByCamera();
            bool isTooClose = distanceToPlayer <= scpTriggerDistance;

            if (isCurrentlyVisible && !isTooClose)
            {
                if (!wasVisibleLastFrame)
                {
                    scpDelayTimer = scpStopDelay;
                }

                if (scpDelayTimer > 0f)
                {
                    scpDelayTimer -= Time.deltaTime;
                    MoveTowardsPlayer();
                }
                else
                {
                    if (agent.isOnNavMesh && !agent.isStopped)
                    {
                        agent.isStopped = true;
                    }
                }
            }
            else
            {
                scpDelayTimer = 0f;

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

            wasVisibleLastFrame = isCurrentlyVisible;
            return;
        }

        // ระบบการเคลื่อนไหวปกติ
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

    private bool IsEnemyVisibleByCamera()
    {
        if (mainCamera == null) mainCamera = Camera.main;
        if (mainCamera == null) return false;

        Plane[] planes = GeometryUtility.CalculateFrustumPlanes(mainCamera);
        Collider enemyCollider = GetComponent<Collider>();

        if (enemyCollider != null)
        {
            return GeometryUtility.TestPlanesAABB(planes, enemyCollider.bounds);
        }
        return false;
    }

    private IEnumerator TimerEventRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(WaitEvent);

            if (isAimedActive && !isDead)
            {
                EventEvery5Seconds.Invoke();
            }
        }
    }

    // ⏱️ ลูปจัดการเวลาอิสระ (ใช้ eventInterval) สำหรับ Event และ โหมดกระทิง
    private IEnumerator WeirdMechanicsRoutine()
    {
        while (true)
        {
            // ใช้ eventInterval เป็นตัวหน่วงเวลา แทนการฟิกซ์เลข 5
            yield return new WaitForSeconds(eventInterval);

            if (!isAimedActive || isDead || playerTransform == null) continue;

            // ถ้าเปิดโหมดกระทิงไว้ ให้สั่งพุ่งชนตามรอบเวลาของ eventInterval นี้เลยแม่!
            if (isBullRushMode)
            {
                TriggerBullRushAttack();
            }

            if (isTeleportAssassinate)
            {
                isExecutingSpecialAction = true;
                if (agent.isOnNavMesh && agent.enabled) agent.isStopped = true;

                yield return new WaitForSeconds(0.8f);

                if (!isDead)
                {
                    Vector3 behindPosition = playerTransform.position - (playerTransform.forward * 2f);
                    transform.position = behindPosition;

                    Vector3 lookDir = (playerTransform.position - transform.position);
                    lookDir.y = 0f;
                    if (lookDir != Vector3.zero) transform.rotation = Quaternion.LookRotation(lookDir);
                }

                yield return new WaitForSeconds(0.5f);
                isExecutingSpecialAction = false;
            }

            if (isGazeStunMode)
            {
                isExecutingSpecialAction = true;
                if (agent.isOnNavMesh && agent.enabled) agent.isStopped = true;

                float timer = 0f;
                while (timer < 2f)
                {
                    if (isDead) break;
                    Vector3 dir = (playerTransform.position - transform.position);
                    dir.y = 0f;
                    if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
                    timer += Time.deltaTime;
                    yield return null;
                }

                if (!isDead && playerTransform != null)
                {
                    float dist = Vector3.Distance(transform.position, playerTransform.position);
                    Vector3 toPlayer = (playerTransform.position - transform.position).normalized;
                    float dot = Vector3.Dot(transform.forward, toPlayer);

                    if (dist <= detectionRange && dot > 0.5f)
                    {
                        MakePlayerDizzy.Invoke();
                    }
                }

                isExecutingSpecialAction = false;
            }
        }
    }

    public void TriggerBullRushAttack()
    {
        if (isDead || isExecutingSpecialAction || playerTransform == null) return;
        StartCoroutine(BullRushRoutine());
    }

    private IEnumerator BullRushRoutine()
    {
        isExecutingSpecialAction = true;
        if (agent.isOnNavMesh && agent.enabled) agent.isStopped = true;

        // 1. ยืนนิ่งและหันหน้าเล็งเป้าใส่ Player ตามเวลา bullRushDelay ที่ตั้งไว้ใน Inspector
        float timer = 0f;
        while (timer < bullRushDelay)
        {
            if (isDead) yield break;
            if (playerTransform != null)
            {
                Vector3 dir = (playerTransform.position - transform.position);
                dir.y = 0f;
                if (dir != Vector3.zero) transform.rotation = Quaternion.LookRotation(dir);
            }
            timer += Time.deltaTime;
            yield return null;
        }

        // 2. ล็อกตำแหน่งล่าสุดของ Player ไว้ แล้วหันหน้าไปมองจุดนั้นตรงๆ เป็นครั้งสุดท้ายก่อนพุ่ง
        Vector3 targetLastPos = playerTransform != null ? playerTransform.position : transform.position;
        targetLastPos.y = transform.position.y;

        Vector3 initialDirection = (targetLastPos - transform.position).normalized;
        if (initialDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(initialDirection);
        }

        if (agent.enabled) agent.enabled = false;

        // เล่นอนิเมชั่นพุ่ง (Rush)
        if (animator != null)
        {
            animator.Play("Rush");
        }

        // 3. พุ่งตรงไปที่จุดที่ล็อกไว้ด้วยความเร็ว bullRushSpeed
        while (Vector3.Distance(transform.position, targetLastPos) > 0.5f)
        {
            if (isDead) yield break;

            transform.position = Vector3.MoveTowards(transform.position, targetLastPos, bullRushSpeed * Time.deltaTime);

            if (playerTransform != null && Vector3.Distance(transform.position, playerTransform.position) <= attackRange)
            {
                AttackPlayer();
                break;
            }
            yield return null;
        }

        // 4. เปิด NavMesh กลับมาปกติเมื่อพุ่งสุดทาง
        if (agent != null && !isDead)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        isExecutingSpecialAction = false;
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
        if (AudioSource != null && AudioClip_SFX_Attack != null) AudioSource.PlayOneShot(AudioClip_SFX_Attack);
        if (animator != null) animator.Play("Attack");
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
            if (ThirdPersonController.AttackerMode)
            {
                TakeDamage(25);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        if (AudioSource != null && AudioClip_SFX_Dead != null) AudioSource.PlayOneShot(AudioClip_SFX_Dead);
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

        StartCoroutine(DestroyRoutine());
    }

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

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, scpTriggerDistance);
    }
}