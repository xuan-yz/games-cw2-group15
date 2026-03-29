using UnityEngine;

public class BulletLogic : MonoBehaviour
{

    public float life =1;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Destroy(gameObject, life);
    }

    void OnCollisionEnter(Collision collision)
    {

        
        
     
    } 


}
