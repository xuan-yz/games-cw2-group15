using UnityEngine;
using System.Collections;

public class PistolEnemy : BaseEnemy
{
    [Header("Pistol Settings")]
    public float preferredRange = 10f;
    public float aimDuration = 3f;
    public float bulletDamage = 25f;
    public float retreatSpeed = 4f;
    public GameObject bulletPrefab;
    public Transform gunBarrel;

    [Header("Laser")]
    public LineRenderer laserRenderer;
    public Color laserColor = Color.red;
    public float laserWidth = 0.02f;

    private bool isAiming = false;

    protected override void Start()
    {
        maxHealth = 60f;
        attackDamage = bulletDamage;
        attackRange = 15f;
        attackCooldown = 4f;
        moveSpeed = 3f;

        base.Start();
        SetupLaser();
    }

    private void SetupLaser()
    {
        if (laserRenderer == null)
            laserRenderer = gameObject.AddComponent<LineRenderer>();

        laserRenderer.startWidth = laserWidth;
        laserRenderer.endWidth = laserWidth;
        laserRenderer.material = new Material(Shader.Find("Sprites/Default"));
        laserRenderer.startColor = laserColor;
        laserRenderer.endColor = laserColor;
        laserRenderer.enabled = false;
        laserRenderer.positionCount = 2;
    }

    protected override void HandleCombat(float distanceToPlayer)
    {
        if (isAiming) return;

        if (distanceToPlayer < preferredRange * 0.6f)
        {
            // Too close — retreat without needing attack token
            Vector3 retreatDir = (transform.position - player.position).normalized;
            agent.SetDestination(transform.position + retreatDir * 3f);
            agent.speed = retreatSpeed;
        }
        else if (distanceToPlayer > preferredRange)
        {
            agent.SetDestination(player.position);
            agent.speed = moveSpeed;
        }
        else
        {
            agent.SetDestination(transform.position);
            FacePlayer();

            if (CanAttack())
                StartCoroutine(AimAndShoot());
        }
    }

    private IEnumerator AimAndShoot()
    {
        isAttacking = true;
        isAiming = true;
        lastAttackTime = Time.time;
        // Token already claimed in CanAttack()

        laserRenderer.enabled = true;
        animator.SetBool("Aiming", true);

        float elapsed = 0f;

        while (elapsed < aimDuration)
        {
            UpdateLaser();
            elapsed += Time.deltaTime;

            float t = elapsed / aimDuration;
            laserRenderer.startColor = Color.Lerp(Color.red, Color.white, t);
            laserRenderer.endColor = laserRenderer.startColor;

            yield return null;
        }

        laserRenderer.enabled = false;
        animator.SetBool("Aiming", false);
        animator.SetTrigger("Shoot");

        ShootBullet();

        yield return new WaitForSeconds(0.5f);

        EnemyManager.Instance.ReleaseAttackToken();
        isAttacking = false;
        isAiming = false;
    }

    // Pistol enemy doesn't consume an aggression token for retreating
    // Token is only used during the aim and shoot sequence
    protected override void Die()
    {
        if (isAiming)
        {
            laserRenderer.enabled = false;
            EnemyManager.Instance.ReleaseAttackToken();
        }
        base.Die();
    }

    private void UpdateLaser()
    {
        laserRenderer.SetPosition(0, gunBarrel.position);

        if (Physics.Raycast(gunBarrel.position, gunBarrel.forward, out RaycastHit hit, 50f))
            laserRenderer.SetPosition(1, hit.point);
        else
            laserRenderer.SetPosition(1, gunBarrel.position + gunBarrel.forward * 50f);
    }

    private void ShootBullet()
    {
        if (bulletPrefab == null) return;

        GameObject bullet = Instantiate(bulletPrefab, gunBarrel.position, gunBarrel.rotation);
        bullet.GetComponent<Bullet>().Init(bulletDamage, player);
    }
}