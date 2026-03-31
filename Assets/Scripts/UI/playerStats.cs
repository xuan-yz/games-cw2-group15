using UnityEngine;
[CreateAssetMenu(fileName = "NewPlayerStats", menuName = "ScriptableObjects/PlayerStats")]
public class playerStats : ScriptableObject
{
    [Header("Stats")]
    public int maxHP = 100;
    public int currentHP = 100;
    public int damage = 10;
    public float moveSpeed = 5f;
    public int currentLevel = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

public void ResetStats()
    {
        maxHP = 100;
        currentHP = 100;
        damage = 10;
        moveSpeed = 5f;
        currentLevel = 1;
        currentXP = 0;
        xpToNextLevel = 100;
    }
}