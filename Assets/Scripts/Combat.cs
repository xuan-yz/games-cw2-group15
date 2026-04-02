using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public playerStats stats;
    public Transform origin;
    public GameObject hitEffectPrefab;

    [Header("Attack Settings")]
    public float attackRange = 2f;
    public float sphereRadius = 1f;
    public float attackCooldown = 0.6f;

    [Header("Kick Attack")]
    public float kickForce = 15;
    public float kickCooldown = 0.8f;

    private bool canPunch = true;
    private bool canKick = true;

    void Update()
    {
        if (Input.GetButtonDown("Fire1") && canPunch)
            StartCoroutine(Punch());

        if (Input.GetMouseButtonDown(1) && canKick)
            StartCoroutine(Kick());
    }

    IEnumerator Punch()
    {
        canPunch = false;
        try
        {
            HitTarget(applyKnockback: false);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Punch error: " + e.Message);
        }
        yield return new WaitForSeconds(attackCooldown);
        canPunch = true;
    }

    IEnumerator Kick()
    {
        canKick = false;
        try
        {
            HitTarget(applyKnockback: true);
        }
        catch (System.Exception e)
        {
            Debug.LogError("Kick error: " + e.Message);
        }
        yield return new WaitForSeconds(kickCooldown);
        canKick = true;
    }

    void HitTarget(bool applyKnockback)
    {
        RaycastHit hit;

        if (!Physics.SphereCast(origin.position, sphereRadius, origin.forward, out hit, attackRange))
            return;

        if (hit.transform.root == transform.root) return;

        Debug.Log("We hit: " + hit.transform.name);

        // Check for regular enemy
        BaseEnemy enemy = hit.transform.GetComponentInParent<BaseEnemy>();
        if (enemy != null && !enemy.IsDead)
        {
            enemy.TakeDamage(stats.damage);
            SpawnHitEffect(hit);
            if (applyKnockback) ApplyKnockback(hit);
            return;
        }

        // Check for boss
        BossManager boss = hit.transform.GetComponentInParent<BossManager>();
        if (boss != null && !boss.IsDead)
        {
            boss.TakeDamage(stats.damage);
            SpawnHitEffect(hit);
            if (applyKnockback) ApplyKnockback(hit);
            return;
        }
    }

    void SpawnHitEffect(RaycastHit hit)
    {
        if (hitEffectPrefab != null && hit.normal != Vector3.zero)
            Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
    }

    void ApplyKnockback(RaycastHit hit)
    {
        Rigidbody targetRigidbody = hit.transform.GetComponentInParent<Rigidbody>();
        if (targetRigidbody == null) return;

        Vector3 kickDirection = (hit.transform.position - transform.position).normalized;
        kickDirection.y = 0f;
        targetRigidbody.AddForce(kickDirection * kickForce + Vector3.up, ForceMode.Impulse);
    }
}