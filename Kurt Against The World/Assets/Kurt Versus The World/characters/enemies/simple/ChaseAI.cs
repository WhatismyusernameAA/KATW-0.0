using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChaseAI : MonoBehaviour
{
    [Header("Dependencies")]
    public Rigidbody2D rb;
    public healthSystem aiHealth;
    public AttackImpactScript attackImpact;

    public SquashStretchHandler modelSquashStretch;
    public Animator animator;

    public Transform target;

    public bool facingRight;

    [Header("Movement Settings")]
    public float accelForce;
    public float deccelForce;

    public float dragForce;
    public float airDragForce;

    public float maxSpeed;
    
    [Header("Ground Check / Jump Settings")]
    public bool grounded;
    public LayerMask ground;
    public Vector3 groundCheckOffset;
    public float groundCheckRadius;

    [Space]
    public float jumpForce;

    [Header("Wall Check")]
    public Vector3 wallCheckOffset;
    public Vector3 jumpCheckOffset;
    public float wallCheckLength;

    [Header("Sight Settings")]

    public float sightRadius;
    public LayerMask blocksSight;

    public float minDistance;

    public float interestTimer;
    float currentInterestTimer;

    [Header("Attack Settings")]
    public float attackForwardAccel;
    public float minAttackTimer;
    public float maxAttackTimer;
    float currAttackTimer;

    [Header("States")]
    public bool isStateActive;
    public Vector3 chasePosition;
    Vector3 chaseDirection;
    public bool isChasing;

    [Header("Damage Stun Settings")]
    public float damagedTimer;
    public float parryStunnedTimer;

    private void Awake()
    {
        if (aiHealth) aiHealth.onTakeDamage += DamagedSleep;
        if (aiHealth) aiHealth.onKnockout += DeathSleep;

        currAttackTimer = maxAttackTimer;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleSight();

        HandleAnimator();
        HandleFacing();
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 chaseDirection = chasePosition - transform.position;

        Gizmos.DrawLine(transform.position, chasePosition);
        Gizmos.DrawSphere(transform.position + groundCheckOffset, groundCheckRadius);
        Gizmos.DrawLine(transform.position + wallCheckOffset, transform.position + wallCheckOffset + (Vector3.right * Mathf.Sign(chaseDirection.x) * wallCheckLength));
        Gizmos.DrawLine(transform.position + jumpCheckOffset, transform.position + jumpCheckOffset + (Vector3.right * Mathf.Sign(chaseDirection.x) * wallCheckLength));

    }

    #region movement functions
    void HandleSight()
    {
        if (!isStateActive) return;

        chasePosition = target.position;
        chaseDirection = chasePosition - transform.position;

        if (chaseDirection.magnitude > minDistance) isChasing = true;
        else isChasing = false;

        if(!isChasing)
        {
            currAttackTimer -= Time.deltaTime;
            if(currAttackTimer <= 0)
            {
                Attack();
                currAttackTimer = Random.Range(minAttackTimer,maxAttackTimer);
            }
        }
    }

    void HandleMovement()
    {
        Vector3 xVelocity = rb.velocity;
        xVelocity.y = 0;

        #region ground check
        grounded = Physics2D.OverlapCircle(transform.position + groundCheckOffset, groundCheckRadius, ground);
        #endregion

        if (isChasing && isStateActive)
        {
            #region walking logic
            rb.AddForce(Vector3.right * Mathf.Sign(chaseDirection.x) * accelForce);

            if (Mathf.Abs(xVelocity.x) > maxSpeed)
            {
                Vector3 cappedVelocity = rb.velocity * deccelForce;
                cappedVelocity.y = 0;
                rb.AddForce(-cappedVelocity);
            }
            #endregion

            #region jumping logic
            bool isObstructed = Physics2D.Raycast(transform.position + wallCheckOffset, Vector3.right * Mathf.Sign(chaseDirection.x), wallCheckLength, ground);
            bool cantJump = Physics2D.Raycast(transform.position + jumpCheckOffset, Vector3.right * Mathf.Sign(chaseDirection.x), wallCheckLength, ground);

            if (isObstructed && !cantJump && grounded)
            {
                Jump();
            }
            #endregion
        }
        else
        {
            if (grounded) rb.AddForce(-xVelocity * dragForce);
            else rb.AddForce(-xVelocity * airDragForce);
        }
    }
    #endregion

    public void HandleAttackAI()
    {

    }

    public void HandleFacing()
    {
        if (chaseDirection.x > 0 && !facingRight)
        {
            modelSquashStretch.UnphysicsScale(-modelSquashStretch.InitialX, modelSquashStretch.InitialY);
            facingRight = true;
        }
        else if (chaseDirection.x < 0 && facingRight)
        {
            modelSquashStretch.UnphysicsScale(-modelSquashStretch.InitialX, modelSquashStretch.InitialY);
            facingRight = false;
        }
    }

    public void HandleAnimator()
    {
        if (!animator) return;

        float velocityRatio = Mathf.Clamp(Mathf.Abs(rb.velocity.x), 0, maxSpeed) / maxSpeed;
        animator.SetFloat("velocityRatio",velocityRatio);
        animator.SetBool("grounded", grounded);
        animator.SetBool("stunned", !isStateActive);
    }

    public void Jump()
    {
        rb.AddForce(Vector3.up * jumpForce, ForceMode2D.Impulse);
    }

    public void Attack()
    {
        Vector3 attackTake = chaseDirection;
        attackTake.y = 0;
        attackTake.x = Mathf.Sign(attackTake.x);
        attackImpact.inputDir = attackTake;
        animator.SetTrigger("attack");
        rb.AddForce(chaseDirection * (attackForwardAccel), ForceMode2D.Impulse);
    }


    // Damaged states
    public void DamagedSleep()
    {
        currAttackTimer = minAttackTimer;
        StartCoroutine(SleepCoroutine(damagedTimer));
    }

    public void ParriedSleep()
    {
        StartCoroutine(SleepCoroutine(parryStunnedTimer));
    }

    public void DeathSleep()
    {
        isStateActive = false;
    }

    IEnumerator SleepCoroutine(float timer)
    {
        isStateActive = false;
        yield return new WaitForSeconds(timer);
        isStateActive = true;
    }
}
