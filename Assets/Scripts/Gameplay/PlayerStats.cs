using UnityEngine;
using UnityEngine.UI;
using TMPro; // เพิ่มบรรทัดนี้เพื่อใช้งาน TextMeshPro

public class PlayerStats : MonoBehaviour
{
    [Header("Level Settings")]
    [SerializeField] private int currentLevel = 0;
    [SerializeField] private TextMeshProUGUI levelText; // ช่องสำหรับใส่ UI ข้อความเลเวล

    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;
    [SerializeField] private Slider healthSlider;

    [Header("Experience Settings")]
    [SerializeField] private float maxExp = 100f;
    [SerializeField] private float currentExp = 0f;
    [SerializeField] private float expGrowthMultiplier = 1.25f;
    [SerializeField] private Slider expSlider;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        UpdateUI();
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        UpdateUI();
        Debug.Log($"Player took {amount} damage! Current HP: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void AddExperience(float amount)
    {
        currentExp += amount;
        Debug.Log($"Player gained {amount} EXP! Current EXP: {currentExp}/{maxExp}");

        while (currentExp >= maxExp)
        {
            LevelUp();
        }

        UpdateUI();
    }

    private void LevelUp()
    {
        currentExp -= maxExp;
        currentLevel++;

        maxExp = Mathf.Round(maxExp * expGrowthMultiplier);
        maxHealth += 20f;
        currentHealth = maxHealth;

        Debug.Log($"<color=cyan>★ LEVEL UP! ★ Now Level: {currentLevel} | MaxHP: {maxHealth} | Next EXP: {maxExp}</color>");
    }

    private void UpdateUI()
    {
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

        // อัปเดตตัวเลขเลเวลบนหน้าจอแบบเรียลไทม์
        if (levelText != null)
        {
            levelText.text = $"Lv. {currentLevel}";
        }
    }

    private void Die()
    {
        Debug.Log("Player has died!");

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        if (TryGetComponent<PlayerController>(out PlayerController controller))
        {
            controller.enabled = false;
        }

        if (TryGetComponent<UnityEngine.AI.NavMeshAgent>(out UnityEngine.AI.NavMeshAgent agent))
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        if (TryGetComponent<Collider>(out Collider col))
        {
            col.enabled = false;
        }
    }
}