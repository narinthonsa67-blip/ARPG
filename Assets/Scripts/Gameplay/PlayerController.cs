using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator anim;
    private Camera mainCamera;

    // --- เพิ่มระบบ AudioSource ---
    private AudioSource audioSource;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip attackSound;      // คลิปเสียงโจมตี
    [SerializeField] private AudioClip footstepSound;    // คลิปเสียงเดิน
    [SerializeField] private float footstepInterval = 0.4f; // ระยะห่างเวลาเสียงเดินดังแต่ละก้าว
    private float footstepTimer = 0f;

    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    [SerializeField] private float healthIncreasePerLevel = 20f;
    [SerializeField] private string gameOverSceneName = "GameOver_Menu";
    [SerializeField] private Slider healthSlider;

    [Header("EXP & Level Settings")]
    public int currentLevel = 1;
    public float currentExp = 0f;
    public float maxExp = 100f;
    [SerializeField] private float expMultiplierPerLevel = 1.2f;
    [SerializeField] private Slider expSlider;
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Ultimate Settings")]
    [SerializeField] private float ultCooldown = 10f;
    [SerializeField] private float ultRadius = 7f;
    [SerializeField] private float ultDamage = 500f;
    [SerializeField] private Slider ultSlider;
    private float ultTimer = 0f;

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

        // ดึงคอมโพเนนต์ AudioSource (หากไม่มีจะสร้างให้อัตโนมัติ)
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        currentHealth = maxHealth;

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (expSlider != null)
        {
            expSlider.maxValue = maxExp;
            expSlider.value = currentExp;
        }

        if (ultSlider != null)
        {
            ultSlider.maxValue = ultCooldown;
            ultSlider.value = ultTimer;
        }

        UpdateLevelUI();
    }

    void Update()
    {
        HandleMovement();
        HandleAnimations();
        HandleFootstepSounds(); // ตรวจจับเสียงเดิน
        HandleCombat();
        HandleUltimate();
    }


    void HandleFootstepSounds()
    {
        bool isMoving = agent != null &&
                         agent.isOnNavMesh &&
                         !agent.isStopped &&
                         agent.velocity.magnitude > 0.5f &&
                         agent.remainingDistance > agent.stoppingDistance;

        if (isMoving)
        {
            footstepTimer += Time.deltaTime;

            // เมื่อถึงจังหวะก้าวเท้า
            if (footstepTimer >= footstepInterval)
            {
                footstepTimer = 0f;

                if (footstepSound != null && audioSource != null)
                {
                    audioSource.pitch = Random.Range(0.95f, 1.05f);
                    audioSource.PlayOneShot(footstepSound, 0.4f);
                }
            }
        }
        else
        {
            // เมื่อหยุดเดินทันที ให้รีเซ็ตเวลาเพื่อหยุดจังหวะเสียงเดิน
            footstepTimer = 0f;
        }
    }

    void HandleCombat()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time >= nextAttackTime)
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

            // --- เล่นเสียงโจมตี ---
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound, 1.0f);
            }

            RotateTowardsMouse();
            DealDamageToEnemies();
        }
    }

    void HandleUltimate()
    {
        if (ultTimer < ultCooldown)
        {
            ultTimer += Time.deltaTime;
            if (ultTimer > ultCooldown) ultTimer = ultCooldown;

            if (ultSlider != null)
            {
                ultSlider.value = ultTimer;
            }
        }

        if (Input.GetKeyDown(KeyCode.R) && ultTimer >= ultCooldown)
        {
            ExecuteUltimate();
        }
    }

    void ExecuteUltimate()
    {
        ultTimer = 0f;
        if (ultSlider != null)
        {
            ultSlider.value = 0f;
        }

        if (agent != null && agent.isOnNavMesh)
        {
            agent.ResetPath();
        }

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound, 1.0f);
        }

        Collider[] hitEnemies = Physics.OverlapSphere(transform.position, ultRadius, enemyLayer);
        HashSet<EnemyAI> hitList = new HashSet<EnemyAI>();

        foreach (Collider col in hitEnemies)
        {
            EnemyAI enemy = col.GetComponentInParent<EnemyAI>();
            if (enemy != null && !hitList.Contains(enemy))
            {
                hitList.Add(enemy);
                enemy.TakeDamage(ultDamage);
            }
        }
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (healthSlider != null)
        {
            healthSlider.value = currentHealth;
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public void AddExp(float amount)
    {
        currentExp += amount;

        while (currentExp >= maxExp)
        {
            currentExp -= maxExp;
            LevelUp();
        }

        if (expSlider != null)
        {
            expSlider.maxValue = maxExp;
            expSlider.value = currentExp;
        }
    }

    private void LevelUp()
    {
        currentLevel++;

        maxHealth += healthIncreasePerLevel;
        currentHealth = maxHealth;

        maxExp = Mathf.Round(maxExp * expMultiplierPerLevel);

        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }

        if (expSlider != null)
        {
            expSlider.maxValue = maxExp;
            expSlider.value = currentExp;
        }

        UpdateLevelUI();
    }

    private void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = $"LV. {currentLevel}";
        }
    }

    private void Die()
    {
        SceneManager.LoadScene(gameOverSceneName);
    }

    void HandleMovement()
    {
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

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, ultRadius);
    }
}