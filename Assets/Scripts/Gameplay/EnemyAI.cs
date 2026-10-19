using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Stats")]
    [SerializeField] private float maxHealth = 50f;
    private float currentHealth;
    private bool isDead = false;

    [Header("Detection & Movement Settings")]
    [SerializeField] private float chaseRange = 8f;   // ระยะตรวจจับ
    [SerializeField] private float attackRange = 1.5f; // ระยะโจมตีประชิด

    [Header("Combat Settings")]
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    private float nextAttackTime;

    [Header("Drop Settings")]
    [SerializeField] private GameObject expOrbPrefab; // Prefab ลูกแก้ว EXP
    [SerializeField] private int dropOrbCount = 6;     // จำนวนลูกแก้วที่จะแตกกระจาย

    private Transform playerTransform;
    private NavMeshAgent agent;
    private Animator anim;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
    }

    private void Update()
    {
        if (isDead || playerTransform == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > chaseRange)
        {
            StopMoving();
        }
        else if (distanceToPlayer > attackRange)
        {
            ChasePlayer();
        }
        else
        {
            PerformAttack();
        }

        // --- แก้ไขจุดสำคัญ: ส่งค่าความเร็วจริง (Float) ไปยัง Parameter "Speed" ใน Animator ---
        UpdateAnimation();
    }

    private void UpdateAnimation()
    {
        if (anim != null && agent != null)
        {
            // ดึงความเร็วการเคลื่อนที่จริงส่งให้ตัวแปร Speed ของ Animator
            float currentSpeed = agent.velocity.magnitude;
            anim.SetFloat("Speed", currentSpeed);
        }
    }

    private void StopMoving()
    {
        if (agent.isOnNavMesh && !agent.isStopped)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }
    }

    private void ChasePlayer()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.SetDestination(playerTransform.position);
        }
    }

    private void PerformAttack()
    {
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
        }

        Vector3 direction = (playerTransform.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 10f * Time.deltaTime);
        }

        if (Time.time >= nextAttackTime)
        {
            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            PlayerController player = playerTransform.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(attackDamage);
            }

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    public void TakeDamage(float amount)
    {
        if (isDead) return;

        currentHealth -= amount;
        Debug.Log($"{gameObject.name} took {amount} damage! Current HP: {currentHealth}");

        chaseRange = 30f; // โดนยิงแล้ววิ่งกวดทันที

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        if (expOrbPrefab != null)
        {
            for (int i = 0; i < dropOrbCount; i++)
            {
                Vector3 spawnOffset = new Vector3(
                    Random.Range(-0.25f, 0.25f),
                    0.2f,
                    Random.Range(-0.25f, 0.25f)
                );

                Instantiate(expOrbPrefab, transform.position + spawnOffset, Quaternion.identity);
            }
        }

        Destroy(gameObject, 0.8f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}