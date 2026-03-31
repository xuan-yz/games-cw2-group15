using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class BulletLogic : MonoBehaviour
{
    public float life =1;
    public int damage = 10;
<<<<<<< Updated upstream
=======

>>>>>>> Stashed changes
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Destroy(gameObject, life);
    }

<<<<<<< Updated upstream
    void OnTriggerEnter(Collider other)
    {
        // Player UI/health logic may be on a parent object instead of the collider itself.
        playerUI ui = other.gameObject.GetComponentInParent<playerUI>();
        ui.TakeDamage(damage);
        Destroy(gameObject); 
    }
}
=======
    void OnTriggerEnter(Collider collision)
    {
        //layer UI/health logic may be on a parent object instead of the collider itself.
        if (collision.CompareTag("Player"))
        {
            playerUI ui = collision.gameObject.GetComponentInParent<playerUI>();
            ui.TakeDamage(damage);
            Destroy(gameObject); 
        }

    } 


}
>>>>>>> Stashed changes
