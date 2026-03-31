using UnityEngine;

public class Test : MonoBehaviour
{
    public MeshRenderer MR;
    public Collider col;
    void Start()
    {

    }
    void Update()
    {
        testing();
    }
    public void Toggle(bool state)
    {
        col.enabled = state;
        MR.enabled = state;
    }
    void testing()
    {
            // Left Click or Ctrl
            if (Input.GetButtonDown("Fire1"))
            {
            MR.enabled = true;
            }
            // Right Click or Alt
            else if (Input.GetButtonDown("Fire2"))
            {
                MR.enabled = true;
            }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Rigidbody rb))
        {
            Vector3 dir = (other.transform.position - transform.position).normalized;
            rb.AddForce((dir + Vector3.up * 0.2f) * 1, ForceMode.Impulse);
            Debug.Log("Hit: " + other.name);
            Toggle(false); // Disable after hit to prevent double-damage
        }
    }
}
