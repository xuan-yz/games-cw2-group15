using UnityEngine;
using System.Collections;

public class PlayerCombat : MonoBehaviour
{
    public playerStats stats; 
    public Transform origin; 
    public GameObject hitEffectPrefab;
    public float attackRange = 2f;

    [Header("Kick Attack")]
    public float kickForce = 20f;

    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            Shoot();
        }

        if (Input.GetMouseButtonDown(1))
        {
            Kick();
        }
    }

    void Shoot()
    {
        HitTarget(applyKnockback: false);
    }

    void Kick()
    {
        HitTarget(applyKnockback: true);
    }

    void HitTarget(bool applyKnockback)
    {
        RaycastHit hit;

        if (Physics.Raycast(origin.position, origin.forward, out hit, attackRange))
        {
            Debug.Log("We hit: " + hit.transform.name);

            BaseEnemy target = hit.transform.GetComponentInParent<BaseEnemy>();
            target.TakeDamage(stats.damage);
            if (applyKnockback)
            {
                ApplyKnockback(hit);
            }
            Instantiate(hitEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
        }
    }

    void ApplyKnockback(RaycastHit hit)
    {
        Vector3 kickDirection = (hit.transform.position - transform.position).normalized;
        kickDirection.y = 0f;

        Vector3 kickImpulse = kickDirection * kickForce + Vector3.up;

        Rigidbody targetRigidbody = hit.transform.GetComponentInParent<Rigidbody>();
        targetRigidbody.AddForce(kickImpulse, ForceMode.Impulse);
    
    }
}