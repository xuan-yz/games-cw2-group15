using UnityEngine;
using System.Collections;

public class FistEnemy : BaseEnemy
{
    
    protected override void Start()
    {
        maxHealth = 80f;
        attackDamage = 10f;
        attackRange = 0.9f;
        attackCooldown = 0.7f;
        moveSpeed = 3.5f;

        base.Start();
    }
    protected override void Update()
    {
        base.Update();
    }

    protected override void HandleCombat(float distanceToPlayer)
    {
        if(distanceToPlayer<= attackRange)
        {
            agent.SetDestination(transform.position);
            agent.speed = moveSpeed;
            FacePlayer();

            if (CanAttack())
                StartCoroutine(PerformAttack());
        }
        else
        {
            agent.SetDestination(player.position);
            agent.speed = moveSpeed;
        }
    }
    protected override IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Fast punch — minimal wind-up
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown * 0.3f);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.2f)
            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);

        yield return new WaitForSeconds(attackCooldown * 0.7f);

        EnemyManager.Instance.ReleaseAttackToken();
        isAttacking = false;
    }   

    protected override void Die()
    {
        EnemyManager.Instance.ReleaseAttackToken();
        base.Die();
    } 

    // Fully inherits base combat and token logic — nothing extra needed
}