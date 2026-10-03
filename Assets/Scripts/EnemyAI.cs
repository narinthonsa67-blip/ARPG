using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHealth = 80f;
    private float currentHealth;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackRange = 2.0f;     // ระยะยืนตี
    [SerializeField] private float chaseRange = 8.0f;      // ระยะมองเห็น (ต้องเข้าใกล้ระยะนี้ถึงจะวิ่งหา)
    [SerializeField] private float attackCooldown = 1.0f;

    [Header("Drops")]
    [SerializeField] private GameObject expOrbPrefab;

    private NavMeshAgent agent;
    private Animator anim;
    private Transform playerTransform;
    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        float distance = Vector3.Distance(transform.position, playerTransform.position);

        // ส่งความเร็วไปคุมแอนิเมชันเดิน/ยืนของศัตรู
        if (anim != null && agent != null)
        {
            anim.SetFloat("Speed", agent.velocity.magnitude);
        }

        // กรณีที่ 1: อยู่นอกระยะมองเห็น -> ยืนเฉยๆ ไม่วิ่งตาม
        if (distance > chaseRange)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
            return;
        }

        // กรณีที่ 2: อยู่ในระยะมองเห็น แต่นอกระยะโจมตี -> วิ่งไล่ตาม
        if (distance > attackRange)
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = false;
                agent.SetDestination(playerTransform.position);
            }
        }
        // กรณีที่ 3: เข้าระยะโจมตีแล้ว -> หยุดเดินแล้วตี
        else
        {
            if (agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            // หันหน้าหาผู้เล่น
            Vector3 targetDir = (playerTransform.position - transform.position).normalized;
            targetDir.y = 0;
            if (targetDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(targetDir);
            }

            // โจมตีตามรอบคูลดาวน์
            if (Time.time >= nextAttackTime)
            {
                nextAttackTime = Time.time + attackCooldown;
                AttackPlayer();
            }
        }
    }

    void AttackPlayer()
    {
        // สั่งเล่นแอนิเมชันโจมตีทันที
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        if (playerTransform.TryGetComponent<PlayerStats>(out PlayerStats playerStats))
        {
            playerStats.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"Enemy took {amount} damage! Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (expOrbPrefab != null)
        {
            Instantiate(expOrbPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        Destroy(gameObject);
    }

    // วาดวงระยะมองเห็นในหน้า Scene ให้สังเกตง่ายๆ
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}