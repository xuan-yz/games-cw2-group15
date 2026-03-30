using UnityEngine;

public class FistEnemy : BaseEnemy
{
    protected override void Start()
    {
        maxHealth = 80f;
        attackDamage = 10f;
        attackRange = 1.8f;
        attackCooldown = 1.4f;
        moveSpeed = 3.5f;

        base.Start();
    }

    // Fully inherits base combat and token logic — nothing extra needed
}