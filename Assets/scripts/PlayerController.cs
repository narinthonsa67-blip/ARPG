using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private Camera mainCamera;

    [Header("Movement Settings")]
    public LayerMask groundLayer;

    [Header("Combat Settings")]
    [SerializeField] private float attackRadius = 2.5f;
    [SerializeField] private float playerDamage = 20f;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private LayerMask enemyLayer;

    private float nextAttackTime = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        mainCamera = Camera.main;
    }

    void Update()
    {
        HandleMovement();
        HandleAnimations();
        HandleCombat();
    }

    void HandleMovement()
    {
        // คลิกขวาเพื่อสั่งเดิน
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100f, groundLayer))
            {
                agent.SetDestination(hit.point);
            }
        }
    }

    void HandleAnimations()
    {
        if (anim != null && agent != null)
        {
            float currentSpeed = agent.velocity.magnitude;
            anim.SetFloat("Speed", currentSpeed);
        }
    }

    void HandleCombat()
    {
        // คลิกซ้ายโจมตีพร้อมเช็กคูลดาวน์
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime)
        {
            nextAttackTime = Time.time + attackCooldown;

            if (agent != null && agent.isOnNavMesh)
            {
                agent.ResetPath();
            }

            if (anim != null)
            {
                anim.SetTrigger("Attack");
            }

            RotateTowardsMouse();
            DealDamageToEnemies();
        }
    }

    void RotateTowardsMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, groundLayer))
        {
            Vector3 targetDir = (hit.point - transform.position).normalized;
            targetDir.y = 0;
            if (targetDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(targetDir);
            }
        }
    }

    void DealDamageToEnemies()
    {
        Vector3 attackPoint = transform.position + transform.forward * 1.2f + Vector3.up * 1.0f;
        Collider[] hitEnemies = Physics.OverlapSphere(attackPoint, attackRadius, enemyLayer);

        // ป้องกันการคิดดาเมจซ้ำกับศัตรูตัวเดิมในการกวาดฟันครั้งเดียว
        HashSet<EnemyAI> hitList = new HashSet<EnemyAI>();

        foreach (Collider col in hitEnemies)
        {
            EnemyAI enemy = col.GetComponentInParent<EnemyAI>();
            if (enemy != null && !hitList.Contains(enemy))
            {
                hitList.Add(enemy);
                enemy.TakeDamage(playerDamage);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * 1.2f + Vector3.up * 1.0f, attackRadius);
    }
}