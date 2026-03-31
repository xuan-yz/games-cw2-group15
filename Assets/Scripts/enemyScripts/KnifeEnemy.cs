using UnityEngine;
using System.Collections;

public class KnifeEnemy : BaseEnemy
{
    [Header("Knife Settings")]
    public float sprintSpeed = 9f;
    public float sprintRange = 8f;
    private bool isSprinting = false;

    protected override void Start()
    {
        maxHealth = 45f;
        attackDamage = 15f;
        attackRange = 1.4f;
        attackCooldown = 0.7f;
        moveSpeed = 45f;

        base.Start();
    }

    protected override void HandleCombat(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange)
        {
            isSprinting = false;
            agent.speed = moveSpeed;
            agent.SetDestination(transform.position);
            FacePlayer();

            if (CanAttack())
                StartCoroutine(PerformAttack());
        }
        else if (distanceToPlayer <= sprintRange)
        {
            // Sprinting — no attack token needed, just movement
            isSprinting = true;
            agent.speed = sprintSpeed;
            agent.SetDestination(player.position);
        }
        else
        {
            isSprinting = false;
            agent.speed = moveSpeed;
            agent.SetDestination(player.position);
        }
    }

    protected override IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Quick stab — short wind-up
        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown * 0.4f);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.3f)
            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);

        yield return new WaitForSeconds(attackCooldown * 0.6f);

        EnemyManager.Instance.ReleaseAttackToken();
        isAttacking = false;
    }

    protected override void UpdateAnimations(float distanceToPlayer)
    {
        base.UpdateAnimations(distanceToPlayer);
        animator.SetBool("Sprinting", isSprinting);
    }
}