using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager Instance { get; private set; }

    [Header("Aggression Settings")]
    public int maxActiveAttackers = 2;
    private int currentActiveAttackers = 0;

    [Header("Room Tracking")]
    private List<BaseEnemy> activeEnemies = new List<BaseEnemy>();
    public bool RoomCleared => activeEnemies.Count == 0;

    [Header("Events")]
    public UnityEvent onRoomCleared;    // Hook this up to open doors, trigger upgrade screen etc

    // Singleton setup
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // --- Aggression Token System ---

    public bool RequestAttackToken()
    {
        if (currentActiveAttackers < maxActiveAttackers)
        {
            currentActiveAttackers++;
            return true;
        }
        return false;
    }

    public void ReleaseAttackToken()
    {
        currentActiveAttackers = Mathf.Max(0, currentActiveAttackers - 1);
    }

    // --- Enemy Registration ---

    public void RegisterEnemy(BaseEnemy enemy)
    {
        if (!activeEnemies.Contains(enemy))
            activeEnemies.Add(enemy);
    }

    public void UnregisterEnemy(BaseEnemy enemy)
    {
        activeEnemies.Remove(enemy);

        if (RoomCleared)
        {
            currentActiveAttackers = 0; // Safety reset
            onRoomCleared?.Invoke();
        }
    }

    // --- Run Reset ---

    public void ResetForNewRoom()
    {
        activeEnemies.Clear();
        currentActiveAttackers = 0;
    }
}