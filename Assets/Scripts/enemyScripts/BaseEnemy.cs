using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public enum EnemyState
{
    Idle,
    Chasing,
    Attacking,
    Dead
}

public class BaseEnemy : MonoBehaviour
{
    [Header("Stats")]
    public float maxHealth = 100f;
    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 1.5f;
    public float moveSpeed = 3.5f;
    public float detectionRange = 15f;

    protected NavMeshAgent agent;
    protected Transform player;
    protected playerUI playerHealth;

    protected Animator animator;

    protected float currentHealth;
    protected float lastAttackTime;
    protected EnemyState currentState = EnemyState.Idle;

    protected virtual void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        playerHealth = player.GetComponent<playerUI>();

        currentHealth = maxHealth;
        agent.speed = moveSpeed;

        EnemyManager.Instance.RegisterEnemy(this);
    }

    protected virtual void Update()
    {
        switch (currentState)
        {
            case EnemyState.Idle:      HandleIdle();      break;
            case EnemyState.Chasing:   HandleChasing();   break;
            case EnemyState.Attacking: HandleAttacking(); break;
            case EnemyState.Dead:                         break;
        }
    }

    protected virtual void HandleIdle()
    {
        if (DistanceToPlayer() <= detectionRange)
            currentState = EnemyState.Chasing;
    }

    protected virtual void HandleChasing()
    {
        float distance = DistanceToPlayer();

        if (distance > detectionRange)
        {
            agent.SetDestination(transform.position);
            animator.SetFloat("Speed", 0f);
            currentState = EnemyState.Idle;
            return;
        }

        if (distance <= attackRange)
        {
            agent.SetDestination(transform.position);
            animator.SetFloat("Speed", 0f);
            currentState = EnemyState.Attacking;
            return;
        }

        agent.SetDestination(player.position);
        animator.SetFloat("Speed", agent.velocity.magnitude);
        FacePlayer();
    }

    protected virtual void HandleAttacking()
    {
        float distance = DistanceToPlayer();

        if (distance > attackRange + 0.5f)
        {
            currentState = EnemyState.Chasing;
            return;
        }

        FacePlayer();

        if (CanAttack())
            StartCoroutine(PerformAttack());
    }

    protected virtual IEnumerator PerformAttack()
    {
        lastAttackTime = Time.time;
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackCooldown * 0.5f);

        if (DistanceToPlayer() <= attackRange + 0.5f)
            playerHealth.TakeDamage((int)attackDamage);

        yield return new WaitForSeconds(attackCooldown * 0.5f);
    }

    protected bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public virtual void TakeDamage(float damage)
    {
        if (currentState == EnemyState.Dead) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
    }

    protected virtual void Die()
    {
        currentState = EnemyState.Dead;
        agent.enabled = false;
        GetComponent<Collider>().enabled = false;
        EnemyManager.Instance.UnregisterEnemy(this);
        Destroy(gameObject, 3f);
    }

    protected float DistanceToPlayer()
    {
        return Vector3.Distance(transform.position, player.position);
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
}