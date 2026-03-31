using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class BaseEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth;
    public float attackDamage;
    public float attackRange;
    public float attackCooldown;
    public float moveSpeed;
    public float detectionRange = 15f;

    [Header("References")]
    protected NavMeshAgent agent;
    protected Animator animator;
    protected Transform player;

    protected float currentHealth;
    protected float lastAttackTime;
    protected bool isDead = false;
    protected bool isAttacking = false;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        currentHealth = maxHealth;
        agent.speed = moveSpeed;

        EnemyManager.Instance.RegisterEnemy(this);
    }

    protected virtual void Update()
    {
        if (isDead) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
            HandleCombat(distanceToPlayer);

        // UpdateAnimations(distanceToPlayer);
    }

    protected virtual void HandleCombat(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange)
        {
            agent.SetDestination(transform.position);
            FacePlayer();

            if (CanAttack())
                StartCoroutine(PerformAttack());
        }
        else
        {
            agent.SetDestination(player.position);
        }
    }

    protected virtual IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetTrigger("Attack");
        yield return new WaitForSeconds(attackCooldown * 0.6f);

        if (Vector3.Distance(transform.position, player.position) <= attackRange + 0.5f)
            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);

        yield return new WaitForSeconds(attackCooldown * 0.4f);

        EnemyManager.Instance.ReleaseAttackToken();
        isAttacking = false;
    }

    protected bool CanAttack()
    {
        if (!isAttacking && Time.time >= lastAttackTime + attackCooldown)
            return EnemyManager.Instance.RequestAttackToken();
        return false;
    }

    public virtual void TakeDamage(float damage)
    {
        if (isDead) return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        if (isDead) return;
        isDead = true;

        agent.enabled = false;
        animator.SetTrigger("Die");
        GetComponent<Collider>().enabled = false;

        EnemyManager.Instance.ReleaseAttackToken();
        EnemyManager.Instance.UnregisterEnemy(this);

        Destroy(gameObject, 3f);
    }

    protected void FacePlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * 10f
        );
    }

    protected virtual void UpdateAnimations(float distanceToPlayer)
    {
        animator.SetFloat("Speed", agent.velocity.magnitude);
        animator.SetBool("InRange", distanceToPlayer <= attackRange);
    }
}