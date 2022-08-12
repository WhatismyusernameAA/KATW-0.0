using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControllerV2 : MonoBehaviour
{
    [Header("Dependencies")]
    public inputscript currInput;

    public Rigidbody2D rb;
    public Collider2D hitbox;

    public Transform viewmodel;
    public Animator animator;

    [Header("Input")]
    public Vector2 inputVector;
    public Vector2 inputLookDirection;

    [Space]
    public float jumpBuffer;
    float currJumpBuffer;

    [Space]
    public bool pressingJump;

    [Header("Movement")]
    public float topSpeed;

    [Space]
    public float accelForce;
    public float deccelForce;
    [Space]
    public float airAccelForce;
    public float airDeccelForce;

    [Space]
    public float velPower;
    public float airVelPower;

    [Space]
    public float dragForce;
    public float airDragForce;


    [Header("Grounded And Jumping")]
    public bool grounded;

    [Space]
    public Vector2 groundCheckOffset;
    public Vector2 groundCheckDimentions;
    public LayerMask groundLayer;

    [Space]
    public float jumpForce;
    [Range(0.0f,1.0f)]
    public float jumpCutPercent;


    [Header("Crouching And Sliding")]
    public bool isCrouching;
    public bool isSliding;

    [Space]
    [Range(0.5f, 1.0f)]
    public float crouchShrinkPercent;
    Vector3 hitboxOriginSize;
    public float crouchWalkMultiplier;

    [Space]
    public float slideInitForce;
    public float slideDrag;
    public float slideVelocityCap;

    [Space]
    public bool slideToCrouch;
    public bool slideMidair;

    [Header("Rendering")]
    public bool isRight;

    [Header("Effects")]
    public ParticleSystem walkParticles;
    public ParticleSystem slideParticles;
    public ParticleSystem jumpParticles;

    private void Awake()
    {
        if (hitbox) hitboxOriginSize = hitbox.transform.localScale;
    }

    private void Update()
    {
        #region directional input
        inputVector.x = 0;
        if (currInput.GetInput("Right"))
            inputVector.x += 1;
        if (currInput.GetInput("Left"))
            inputVector.x -= 1;

        inputVector.y = 0;
        if (currInput.GetInput("Up"))
            inputVector.y += 1;
        if (currInput.GetInput("Down"))
            inputVector.y -= 1;
        #endregion

        #region button input
        #region jump input
        // reset jump buffer whenever jump button is pressed
        currJumpBuffer -= Time.deltaTime;
        if (currInput.GetInputDown("Jump")) currJumpBuffer = jumpBuffer;

        // handle jump cuts
        if (currInput.GetInputUp("Jump") && rb.velocity.y > 0)
        {
            Vector2 velocity = rb.velocity;
            velocity.y *= jumpCutPercent;
            rb.velocity = velocity;
        }
        #endregion

        #region slide input
        //check if crouch button is pressed, then set isCrouching to true/false
        if (currInput.GetInputDown("Slide"))
        {
            // set sliding if input while crouching is pressed
            if (Mathf.Abs(inputVector.x) > 0.1)
            {
                if (!slideMidair && !grounded) return;

                slideParticles.Play();
                isSliding = true;
                Vector2 slideForceVector = inputVector;
                slideForceVector.y = 0;
                rb.AddForce(slideForceVector * slideInitForce, ForceMode2D.Impulse);
            }
            isCrouching = true;
        }
        else if (currInput.GetInputUp("Slide"))
        {
            isCrouching = false;
            if (isSliding)
            {
                isSliding = false;
                slideParticles.Stop();
            }
        }
        #endregion
        #endregion
    }

    private void FixedUpdate()
    {
        #region movement
        // multiplier for crouching (and maybe speed effects?)
        float walkMultiplier = isCrouching ? crouchWalkMultiplier : 1;

        if (!isSliding)
        {
            // find target velocity and difference between current velocity and target velocity
            float targetVelo = inputVector.x * topSpeed * walkMultiplier;
            float speedDifference = targetVelo - rb.velocity.x;
            float accelRate;

            if (grounded)
            {
                // when the player is on the ground
                // check if we need to deccelerate to the target velocity or accelerate to the target velocity
                accelRate = (Mathf.Abs(targetVelo) > 0.01f) ? accelForce : deccelForce;

                // pull the brakes when no input is detected
                if (Mathf.Abs(inputVector.x) < 0.01)
                {
                    float dragAmount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(dragForce));
                    dragAmount *= Mathf.Sign(rb.velocity.x);
                    rb.AddForce(Vector2.right * -dragAmount, ForceMode2D.Impulse);
                }
            }
            else
            {
                // when the player is in the air
                // check if we need to deccelerate to the target velocity or accelerate to the target velocity
                accelRate = (Mathf.Abs(targetVelo) > 0.01f) ? airAccelForce : airDeccelForce;

                if (Mathf.Abs(inputVector.x) < 0.01)
                {
                    float dragAmount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(airDragForce));
                    dragAmount *= Mathf.Sign(rb.velocity.x);
                    rb.AddForce(Vector2.right * -dragAmount, ForceMode2D.Impulse);
                }
            }

            // calculate applied force and apply
            float appliedForce = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, velPower) * Mathf.Sign(speedDifference);
            rb.AddForce(appliedForce * Vector2.right);
        }
        #endregion

        #region grounded check
        Vector2 position = transform.position;
        grounded = Physics2D.OverlapBox(position - groundCheckOffset, groundCheckDimentions, 0, groundLayer);
        #endregion

        #region jumping
        // add upwards force when jump button is pressed (or the debounce is still active)
        if (grounded && currJumpBuffer > 0)
        {
            jumpParticles.Play();
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currJumpBuffer = 0;

            // stop sliding
            isSliding = false;
            isCrouching = false;
            slideParticles.Stop();
        }
        #endregion

        #region crouching and sliding
        if (isCrouching)
        {
            if (!hitbox) return;

            // if crouching, shrink hitbox
            Vector3 crouchedScale = hitboxOriginSize;
            crouchedScale.y *= crouchShrinkPercent;
            hitbox.transform.localScale = crouchedScale;
        }
        else
        {
            // reset hitbox size
            hitbox.transform.localScale = hitboxOriginSize;
        }

        if (isSliding)
        {
            // add drag to slide (so it doesn't go on forever)
            float dragAmount = Mathf.Abs(rb.velocity.x) * slideDrag;
            dragAmount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -dragAmount);

            // check if velocity cap is reached; so that slide stops when unnecessary
            if (Mathf.Abs(rb.velocity.x) < slideVelocityCap)
            {
                isSliding = false;
                slideParticles.Stop();
                if (!slideToCrouch) isCrouching = false;
            }
        }
        #endregion

        #region animation
        if(viewmodel && !isSliding)
        {
            if (inputVector.x > 0 && !isRight)
            {
                Vector2 scale = viewmodel.transform.localScale;
                scale.x = -scale.x;
                viewmodel.transform.localScale = scale;
                isRight = true;
            }
            else if (inputVector.x < 0 && isRight)
            {
                Vector2 scale = transform.localScale;
                scale.x = -scale.x;
                transform.localScale = scale;
                isRight = false;
            }
        }

        if(animator)
        {
            float velocityRatio = Mathf.Abs(rb.velocity.x / (topSpeed * walkMultiplier));
            animator.SetFloat("xVelocity", velocityRatio);
            animator.SetFloat("yVelocity", rb.velocity.y);

            Vector2Int integerInputVector = Vector2Int.CeilToInt(inputVector);
            animator.SetInteger("xDir", integerInputVector.x);
            animator.SetInteger("yDir", integerInputVector.y);

            animator.SetBool("grounded", grounded);
            animator.SetBool("crouching", isCrouching);
            animator.SetBool("sliding", isSliding);
        }    
        #endregion
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 position = transform.position;
        Gizmos.DrawWireCube(position - groundCheckOffset, groundCheckDimentions);
    }

}
