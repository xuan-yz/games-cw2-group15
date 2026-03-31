using UnityEngine;

public class GunLogic : MonoBehaviour
{
    public Transform bulletSpawnPoint;
    public GameObject punchPrefab;
    public GameObject bulletPrefab;
    public float bulletSpeed = 10;
    public float fireRate = 0.5f; // Time between shots

    public float punchSpeed = 10;
    public float punchRate = 0.5f; // Time between shots
    private float nextFireTime = 0f;
    
    // Call this method from the attack state
    public void Shoot()
    {
        if (Time.time >= nextFireTime)
        {
            
            GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.AddComponent<BulletLogic>();
            bullet.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.forward * bulletSpeed;
            
            
            nextFireTime = Time.time + fireRate;
        }
    }

     public void Punch()
    {
        if (Time.time >= nextFireTime)
        {
            
            GameObject bullet = Instantiate(punchPrefab, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            bullet.AddComponent<BulletLogic>();
            bullet.GetComponent<Rigidbody>().linearVelocity = bulletSpawnPoint.forward * punchSpeed;
            
            
            nextFireTime = Time.time + punchRate;
        }
    }
}