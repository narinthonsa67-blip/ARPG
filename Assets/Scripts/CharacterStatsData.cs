using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacterStats", menuName = "ARPG/Character Stats")]
public class CharacterStatsData : ScriptableObject
{
    [Header("Base Stats")]
    public int level = 1;
    public float maxHealth = 100f;
    public float currentHealth = 100f;

    [Header("Experience & Progression")]
    public int currentExp = 0;
    public int expToNextLevel = 100;
    public float expGrowthMultiplier = 1.5f;

    public void ResetStats()
    {
        level = 1;
        maxHealth = 100f;
        currentHealth = maxHealth;
        currentExp = 0;
        expToNextLevel = 100;
    }
}