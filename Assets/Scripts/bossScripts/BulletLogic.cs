using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    public float life =1;
    public int damage = 10;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Destroy(gameObject, life);
    }

//     void OnTriggerEnter(Collider other)
//     {
//         // Player UI/health logic may be on a parent object instead of the collider itself.
//         playerUI ui = other.gameObject.GetComponentInParent<playerUI>();
//         ui.TakeDamage(damage);
//         Destroy(gameObject); 
//     }
}