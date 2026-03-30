
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Agent : MonoBehaviour
{
    public bool Knockback = false;   
    public float knockbackRecoveryTime = 4f; 
    public float escapeDelay = 1f;            
    public float escapeSpeed = 5f;  
    private float knockbackTimer; 

    private NavMeshAgent agent;
    private bool isEscaping = false;  
    private Transform target;
    private Transform player;
    private Rigidbody rb;

    

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        agent.updateRotation = false; 
        // agent.updateUpAxis = false; // prevent NavMeshAgent from controlling up axis
        agent.updatePosition = false; 

        target = GameObject.FindGameObjectWithTag("target").transform;
        if (target == null)
        {
            Debug.LogError("Target not found. Please assign a GameObject with the tag 'target'.");
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogError("Player not found. Please assign a GameObject with the tag 'Player'.");
        }
    }
    void Face(Vector3 Velocity)
    {
        Vector3 direction = Velocity;
        direction.y = 0; // keep only horizontal direction
        if (direction.sqrMagnitude > 0.01f) // avoid zero-length direction
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    void Update()
    {   
        
        if (Knockback)
        {
            knockbackTimer = knockbackRecoveryTime;
            Knockback = false;
            isEscaping = false;
            agent.isStopped = true; 
        }
        if (knockbackTimer > 0)
        {

            knockbackTimer -= Time.deltaTime;

            float elapsedTime = knockbackRecoveryTime - knockbackTimer;
            if (elapsedTime >= escapeDelay && !isEscaping)
            {
                isEscaping = true;
                
            }

            if (isEscaping && transform.position.y < 0.5f && transform.position.z <= 10f) {// only escape if the agent is on the ground and far away from the goal
            
                // agent.SetDestination(player.position);
                // Vector3 desiredVelocity = agent.desiredVelocity;
                // Vector3 newVelocity = rb.velocity;
                // newVelocity.x = -desiredVelocity.x;
                // newVelocity.z = -desiredVelocity.z;
                // newVelocity.y = rb.velocity.y;
                // rb.velocity = newVelocity;//running away from player when knocked back to the ground
                Vector3 awayDir = transform.position - player.position;
                awayDir.Normalize();

                Vector3 newVelocity = awayDir * escapeSpeed;
                newVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = newVelocity;
                Face(rb.linearVelocity);
            }
        }
        else
        {
            agent.nextPosition = transform.position;
            agent.isStopped = false;
            agent.SetDestination(target.position);
            Vector3 desiredVelocity = agent.desiredVelocity;
            Vector3 newVelocity = rb.linearVelocity;
            newVelocity.x = desiredVelocity.x;
            newVelocity.z = desiredVelocity.z;
            rb.linearVelocity = newVelocity;
            Face(newVelocity);
        }
}
 public void Teleport()
    {
        float minimunX = -14f;
        float maximumX= 13f;
        float minimumZ = -21f;
        float maximumZ= 10f;
        float randomX = Random.Range(minimunX, maximumX);
        float randomZ = Random.Range(minimumZ, maximumZ);
        Vector3 randomPosition = new Vector3(randomX, 0.5f, randomZ);
        agent.Warp(randomPosition); // teleport the agent to the new position


        // reset velocity and state
        rb.linearVelocity = Vector3.zero; 
        Knockback = false; 
        knockbackTimer = 0f;
        isEscaping = false;
        agent.isStopped = false;

     
    }
}