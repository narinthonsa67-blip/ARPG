using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f;
    private float currentHealth;

    [Header("Experience Settings")]
    [SerializeField] private float currentExp = 0f;

    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log($"Player took {amount} damage! Current HP: {currentHealth}");

        // ไม่สั่งเล่นท่า Hit เพื่อให้เคลื่อนไหวได้อย่างต่อเนื่อง

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void AddExperience(float amount)
    {
        currentExp += amount;
        Debug.Log($"Player gained {amount} EXP! Total EXP: {currentExp}");
    }

    private void Die()
    {
        Debug.Log("Player has died!");

        if (anim != null)
        {
            anim.SetTrigger("Die");
        }

        // ปิดการควบคุมเมื่อตัวละครตาย
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