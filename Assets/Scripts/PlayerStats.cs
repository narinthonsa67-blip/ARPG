using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Stats Data")]
    public CharacterStatsData statsData;

    private void Awake()
    {
        if (statsData != null)
        {
            statsData.ResetStats();
        }
    }

    public void TakeDamage(float amount)
    {
        if (statsData == null) return;

        statsData.currentHealth -= amount;
        Debug.Log($"Player took {amount} damage! Current HP: {statsData.currentHealth}/{statsData.maxHealth}");

        if (statsData.currentHealth <= 0)
        {
            Die();
        }
    }

    public void AddExperience(int amount)
    {
        if (statsData == null) return;

        statsData.currentExp += amount;
        Debug.Log($"Gained {amount} EXP! Total EXP: {statsData.currentExp}/{statsData.expToNextLevel}");

        // ถ้า EXP เต็มหรือเกิน ให้ Level Up ทันทีตามโจทย์
        while (statsData.currentExp >= statsData.expToNextLevel)
        {
            LevelUp();
        }
    }

    private void LevelUp()
    {
        statsData.currentExp -= statsData.expToNextLevel;
        statsData.level++;
        statsData.expToNextLevel = Mathf.RoundToInt(statsData.expToNextLevel * statsData.expGrowthMultiplier);
        statsData.maxHealth += 20f;
        statsData.currentHealth = statsData.maxHealth;

        Debug.Log($"<color=green>LEVEL UP! Reached Level {statsData.level}!</color> Next EXP: {statsData.expToNextLevel}");
    }

    private void Die()
    {
        Debug.Log("<color=red>Player Died!</color>");
    }
}