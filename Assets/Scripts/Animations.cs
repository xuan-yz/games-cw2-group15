using Unity.VisualScripting;
using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem.XR;

public class AnimationStateController : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public CharacterController controller;
    public PlayerController movementScript;

    [Header("Attack Settings")]
    public float punchCooldown = 0.5f;
    public float kickCooldown = 0.8f;

    [Header("Damage Values")]
    public int punchDamage = 10;
    public int kickDamage = 20;

    [Header("Settings")]
    public float dampening = 0.1f;

    private float nextAttackTime = 0f;

    void Start()
    {
        //controller = GetComponent<CharacterController>();
        animator = GetComponentInChildren<Animator>();
        //movementScript = GetComponent<PlayerController>();
        //I was trying to figure out why this wasnt working and it was because i hadnt linked the code to the character....
        //Debug.LogError("working");
        //checks because it doesnt seem to work
        if (animator == null) Debug.LogError("Animator not found on child");
        if (movementScript == null) Debug.LogError("PlayerController script not found!");
    }

    void Update()
    {
        AnimationState();
        Combat();
    }

    void Combat()
    {
        if (Time.time >= nextAttackTime)
        {
            // Left Click or Ctrl
            if (Input.GetButtonDown("Fire1"))
            {
                StartCoroutine(PerformAttack("Punch", punchCooldown));
            }
            // Right Click or Alt
            else if (Input.GetButtonDown("Fire2"))
            {
                StartCoroutine(PerformAttack("Kick", kickCooldown));
            }
        }
    }

    private IEnumerator PerformAttack(string triggerName, float cooldown)
    {
        nextAttackTime = Time.time + cooldown;

        animator.SetBool("isAttacking", true);
        animator.SetTrigger(triggerName);

        Debug.Log("Performed " + triggerName + "!");

        yield return new WaitForSeconds(cooldown);

        animator.SetBool("isAttacking", false);
    }

    void AnimationState()
    {
        bool isGrounded = movementScript.IsGrounded;
        bool isAttacking = animator.GetBool("isAttacking");

        Vector3 horizontalVelocity = new Vector3(controller.velocity.x, 0, controller.velocity.z);
        float currentSpeed = horizontalVelocity.magnitude;

        //the speed goes to crazy low values like 1e-44 so added to remove that from bugging movment
        if (!isAttacking)
        {
            if (currentSpeed < 0.1f)
            {
                animator.SetFloat("Speed", 0);
            }
            else
            {
                animator.SetFloat("Speed", currentSpeed, dampening, Time.deltaTime);
            }
        }
        else
        {
            // Force speed to 0 while attacking to prevent animation blending conflicts
            animator.SetFloat("Speed", 0);
        }

        animator.SetBool("isGround", isGrounded);

        if (Input.GetButtonDown("Jump") && isGrounded && !isAttacking)
        {
            animator.SetTrigger("Jump");
        }

        //cant use else because it updates too quickly and you loop
        if (!isGrounded)
        {
            animator.SetBool("Jump", false);
        }
    }
}