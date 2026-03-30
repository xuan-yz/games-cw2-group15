using UnityEngine;
using System.Collections;

public class MacheteEnemy : BaseEnemy
{
    [Header("Machete Settings")]
    public float knockbackForce = 8f;

    protected override void Start()
    {
        maxHealth = 180f;
        attackDamage = 28f;
        attackRange = 2.2f;
        attackCooldown = 2.2f;
        moveSpeed = 2.5f;

        base.Start();
    }

    protected override IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Long wind-up — player has time to dodge
        animator.SetTrigger("WindUp");
        yield return new WaitForSeconds(1.0f);

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(0.3f);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
            ApplyKnockback();
        }

        yield return new WaitForSeconds(0.9f);

        EnemyManager.Instance.ReleaseAttackToken();
        isAttacking = false;
    }

    // Machete is slow and heavy — release token early so others
    // can attack during the long recovery animation
    protected override void Die()
    {
        EnemyManager.Instance.ReleaseAttackToken();
        base.Die();
    }

    private void ApplyKnockback()
    {
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            playerRb.AddForce(direction * knockbackForce, ForceMode.Impulse);
        }
    }
}