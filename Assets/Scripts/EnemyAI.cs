using UnityEngine;
using UnityEngine.AI;

public enum EnemyState
{
    Idle,
    Chase,
    Attack
}

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHealth = 60f;
    private float currentHealth;

    [Header("State Machine & Detection")]
    [SerializeField] private float chaseRange = 8f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;

    [Header("Loot Drop")]
    [SerializeField] private GameObject expOrbPrefab; // ลาก Prefab ลูกแก้ว EXP มาใส่ตรงนี้

    private NavMeshAgent agent;
    private Transform playerTransform;
    private EnemyState currentState = EnemyState.Idle;
    private float lastAttackTime;
    private Animator animator;

    private static readonly int SpeedHash = Animator.StringToHash("Speed");
    private static readonly int AttackTriggerHash = Animator.StringToHash("Attack");

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        // ค้นหาตำแหน่ง Player อัตโนมัติจาก Tag หรือคอมโพเนนต์
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case EnemyState.Idle:
                agent.isStopped = true;
                if (distanceToPlayer <= chaseRange)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);

                if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.Attack;
                }
                else if (distanceToPlayer > chaseRange)
                {
                    currentState = EnemyState.Idle;
                }
                break;

            case EnemyState.Attack:
                agent.isStopped = true;
                // หันหน้าเข้าหาผู้เล่น
                Vector3 lookDirection = (playerTransform.position - transform.position).normalized;
                lookDirection.y = 0;
                if (lookDirection != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 10f);
                }

                // โจมตีตาม Cooldown
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    PerformAttack();
                    lastAttackTime = Time.time;
                }

                if (distanceToPlayer > attackRange)
                {
                    currentState = EnemyState.Chase;
                }
                break;
        }

        UpdateAnimation();
    }

    private void PerformAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger(AttackTriggerHash);
        }

        // ลดเลือดผู้เล่นถ้ามีคอมโพเนนต์ PlayerStats
        if (playerTransform.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(attackDamage);
        }
    }

    // ฟังก์ชันรับดาเมจที่ PlayerCombat.cs เรียกหา
    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage! Remaining HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // ดรอปลูกแก้ว EXP ตรงจุดที่ศัตรูตาย
        if (expOrbPrefab != null)
        {
            Instantiate(expOrbPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    private void UpdateAnimation()
    {
        if (animator != null)
        {
            animator.SetFloat(SpeedHash, agent.velocity.magnitude);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // วาดระยะตรวจจับ (สีเหลือง) และระยะตี (สีแดง)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}