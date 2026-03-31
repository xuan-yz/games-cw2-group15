using UnityEngine;

public class MeleeHitbox : MonoBehaviour
{
    public float knockback;
    public int damage;
    public MeshCollider hitboxCollider;
    public MeshRenderer MR;
    
    void Awake()
    {
        hitboxCollider.enabled = false;

    }
    public void Activate(int dmg, float kb)
    {
        knockback = kb;
        damage = dmg;
        hitboxCollider.enabled = true;
        MR.enabled = true;
    }
    public void Deactivate()
    {
        hitboxCollider.enabled = false;
        MR.enabled= false;
    }
    private void OnTriggerEnter(Collider other)
    {
        
        // Check if we are mid-swing and hitting an Enemy
        if (hitboxCollider.enabled && other.CompareTag("Boss"))
        {
            Rigidbody enemyRb = other.GetComponent<Rigidbody>();

            if (enemyRb != null)
            {

                Vector3 direction = (other.transform.position - transform.position).normalized;
                direction.y = 0.2f;

                enemyRb.AddForce(direction * knockback, ForceMode.Impulse);

                Debug.Log("Knockback applied to " + other.name);
            }

            
            Deactivate();
        }
    }
    
}