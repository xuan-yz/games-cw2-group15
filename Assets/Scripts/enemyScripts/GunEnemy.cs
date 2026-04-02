using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class GunEnemy : BaseEnemy
{
    [Header("Pistol Settings")]
    public float preferredRange = 10f;
    public float retreatDistance = 5f;

    [Header("Laser & Timing")]
    public float trackingDuration = 1.0f;
    public float lockDuration = 1.0f;
    public float laserWidth = 0.2f;
    
    public Color laserAimColor = Color.red;
    public Color laserFireColor = Color.white;

    private LineRenderer laserRenderer;
    public Transform headBone;
    private bool isAiming = false;
    private Vector3 lockedTargetPosition;

    protected override void Start()
    {
        // Default stats
        maxHealth = 60f;
        attackDamage = 20f;
        attackRange = 15f;
        attackCooldown = 4f;
        moveSpeed = 3f;
        detectionRange = 20f;

        base.Start();
        SetupLaser();
    }

    private void SetupLaser()
    {
        if (headBone == null)
        {
            Debug.LogError("GunEnemy: headBone not assigned in Inspector");
            return;
        }

        // Add or get LineRenderer
        laserRenderer = headBone.gameObject.GetComponent<LineRenderer>();
        if (laserRenderer == null)
            laserRenderer = headBone.gameObject.AddComponent<LineRenderer>();

        laserRenderer.startWidth = laserWidth;
        laserRenderer.endWidth = laserWidth;
        laserRenderer.material = new Material(Shader.Find("Sprites/Default"));
        laserRenderer.startColor = laserAimColor;
        laserRenderer.endColor = laserAimColor;
        laserRenderer.enabled = false;
        laserRenderer.positionCount = 2;
        laserRenderer.useWorldSpace = true;
    }

    protected override void HandleChasing()
    {
        if (isAiming) return;

        float distance = DistanceToPlayer();

        if (distance > detectionRange)
        {
            agent.ResetPath();
            animator.SetFloat("Speed", 0f);
            currentState = EnemyState.Idle;
            return;
        }

        if (distance < preferredRange * 0.5f)
        {
            Vector3 retreatDir = (transform.position - player.position).normalized;
            agent.SetDestination(transform.position + retreatDir * retreatDistance);
            agent.speed = moveSpeed;
            animator.SetFloat("Speed", agent.velocity.magnitude);
            return;
        }

        if (distance <= preferredRange)
        {
            agent.ResetPath();
            animator.SetFloat("Speed", 0f);
            currentState = EnemyState.Attacking;
            return;
        }

        agent.SetDestination(player.position);
        agent.speed = moveSpeed;
        animator.SetFloat("Speed", agent.velocity.magnitude);
        FacePlayer();
    }

    protected override void HandleAttacking()
    {
        if (isAiming) return;

        float distance = DistanceToPlayer();

        if (distance > preferredRange + 1f || distance < preferredRange * 0.5f)
        {
            currentState = EnemyState.Chasing;
            return;
        }

        FacePlayer();

        if (CanAttack())
            StartCoroutine(PerformAttack());
    }

    protected override IEnumerator PerformAttack()
    {
        isAiming = true;
        lastAttackTime = Time.time;
        laserRenderer.enabled = true;

        float elapsed = 0f;
        while (elapsed < trackingDuration)
        {
            lockedTargetPosition = player.position; 
            FacePlayer();
            UpdateLaserVisuals(lockedTargetPosition);

            laserRenderer.startColor = laserAimColor;
            laserRenderer.endColor = laserAimColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < lockDuration)
        {
            UpdateLaserVisuals(lockedTargetPosition);

            float t = elapsed / lockDuration;
            Color currentColor = Color.Lerp(laserAimColor, laserFireColor, t);
            laserRenderer.startColor = currentColor;
            laserRenderer.endColor = currentColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        FireAtPosition(lockedTargetPosition);
        
        if(AudioManager.Instance != null)
            AudioManager.Instance.PlayGunshot();

        laserRenderer.startColor = laserFireColor;
        laserRenderer.endColor = laserFireColor;
        yield return new WaitForSeconds(0.1f);

        laserRenderer.enabled = false;
        isAiming = false;
    }

    private void UpdateLaserVisuals(Vector3 targetPoint)
    {
        if (headBone == null) return;

        Vector3 origin = headBone.position;
        Vector3 direction = (targetPoint - origin).normalized;

        laserRenderer.SetPosition(0, origin);

        if (Physics.Raycast(origin, direction, out RaycastHit hit, attackRange))
            laserRenderer.SetPosition(1, hit.point);
        else
            laserRenderer.SetPosition(1, origin + direction * attackRange);
    }

    private void FireAtPosition(Vector3 targetPoint)
    {
        if (headBone == null) return;

        Vector3 origin = headBone.position;
        Vector3 direction = (targetPoint - origin).normalized;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, attackRange))
        {
            if (hit.collider.CompareTag("Player"))
            {
                if(playerUI != null) playerUI.TakeDamage((int)attackDamage);
                if(DamageVignette.Instance != null) DamageVignette.Instance.Flash();
                if(AudioManager.Instance != null) AudioManager.Instance.PlayPlayerHurt();
                Debug.Log("GunEnemy hit player!");
            }
        }
    }

    protected override void HandleIdle()
    {
        if (laserRenderer != null && laserRenderer.enabled && !isAiming)
            laserRenderer.enabled = false;

        base.HandleIdle();
    }

    public override void TakeDamage(float damage)
    {
        base.TakeDamage(damage);
    }
}