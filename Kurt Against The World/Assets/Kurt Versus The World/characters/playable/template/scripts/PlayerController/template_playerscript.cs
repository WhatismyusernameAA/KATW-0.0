using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class characterSetData
{
    // movement settings
    public float extraGravity;
    public float dragForce;
    public float deccelerationForce;
    public float accelerationForce;
    public float maximumSpeed;

    public float airDragForce;
    public float airDeccelerationForce;
    public float airAccelerationForce;
    public float airMaximumSpeed;

    public float ultimateMaximumSpeed;

    public Vector3 groundCheckOffset;
    public float groundCheckRadius;
    public LayerMask isGround;

    // Jump and slide
    public float jumpForce;
    public float jumpCutPercent;

    public float slideInitForce;
    public float slideCrouchPercent;

    // Attacks
    public float attackInputDelay;
    public float attackCooldown;

    public float attackAcceleration;
    public bool attackingDownwardsAppliesForce;

    public float chargeDuration;
    float currentChargeDuration;
    public float chargedAttackAcceleration;

    public float chargeAttackDrag;

    // Input
    public float jumpTime;
    public float slideTime;
    public float attackTime;
    public float coyoteTime;
}

public class template_playerscript : MonoBehaviour
{
    #region dependencies
    [Header("Dependencies")]
    public GameObject hitbox;
    float initHitboxHeight;

    public GameObject model;
    public SquashStretchHandler squashModel;
    public Animator animator;

    public Rigidbody2D rb;
    public AttackImpactScript attackImpact;
    #endregion

    #region movement settings
    [Header("Movement Settings")]
    public float extraGravity;
    public float dragForce;
    public float deccelerationForce;
    public float accelerationForce;
    public float maximumSpeed;

    [Header("Air Movement Settings")]
    public float airDragForce;
    public float airDeccelerationForce;
    public float airAccelerationForce;
    public float airMaximumSpeed;
    [Space]
    public float ultimateMaximumSpeed;
    #endregion

    #region ground check
    [Header("Ground Check")]
    public bool grounded;
    public bool playedGroundedAnimation;
    public Vector3 groundCheckOffset;
    public float groundCheckRadius;
    public LayerMask isGround;
    #endregion

    #region jump settings
    [Header("Jump Settings")]
    public float jumpForce;
    [Range(0.0f, 1.0f)]
    public float jumpCutPercent;
    bool jumpedUp;
    #endregion

    #region slide settings
    [Header("Slide Settings")]
    public float slideInitForce;
    public float slideStopPoint;
    public float slideDrag;

    [Range(0.0f, 1.0f)]
    public float slideCrouchPercent;

    [Space]
    public float slideCooldown;

    bool isSliding;
    #endregion

    #region input
    [Header("Input Settings")]
    public inputscript inputScript;
    public Vector2 inputDirection;
    public Vector2 inputAttackFacing = Vector3.right;
    [Space]
    public float jumpTime;
    float currentJumpTime;
    [Space]
    public float slideTime;
    float currentSlideTime;
    [Space]
    public float attackTime;
    float currentAttackTime;
    [Space]
    public float coyoteTime;
    float currentCoyoteTime;

    bool facingRight = true;
    #endregion

    #region particles
    [Header("Particles")]
    public ParticleSystem landParticles;
    public soundplayer landSound;

    public ParticleSystem jumpParticles;
    public soundplayer jumpSound;

    public ParticleSystem slideParticles;
    public soundplayer slideSound;

    public ParticleSystem runParticles;

    public ParticleSystem attackParticles;
    public soundplayer attackSound;
    #endregion

    private void Awake()
    {
        inputAttackFacing = Vector3.right;
        initHitboxHeight = hitbox.transform.localScale.y;
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + groundCheckOffset, groundCheckRadius);
    }

    private void Update()
    {
        HandleInput();
        if(!isSliding) HandleFacing();
        HandleTimers();
        HandleAnimation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void HandleInput()
    {
        #region input directions
        inputDirection.x = 0;
        if (inputScript.GetInput("Right"))
            inputDirection.x += 1;
        if (inputScript.GetInput("Left"))
            inputDirection.x -= 1;

        inputDirection.y = 0;
        if (inputScript.GetInput("Up"))
            inputDirection.y += 1;
        if (inputScript.GetInput("Down"))
            inputDirection.y -= 1;

        if(inputDirection.sqrMagnitude > 0.01)
        {
            if (inputDirection.y != 0)
            {
                inputAttackFacing.y = inputDirection.y;
                inputAttackFacing.x = 0;
            }
            else
            {
                inputAttackFacing.x = inputDirection.x;
                inputAttackFacing.y = 0;
            }
        }
        #endregion

        #region jumping
        if (inputScript.GetInputDown("Jump")) currentJumpTime = jumpTime;

        if(inputScript.GetInputUp("Jump") && jumpedUp)
        {
            // half velocity
            Vector3 velocity = rb.velocity;
            if(velocity.y > 0) velocity.y *= jumpCutPercent;
            rb.velocity = velocity;

            jumpedUp = false;
        }
        #endregion

        #region sliding
        if (inputScript.GetInputDown("Slide")) currentSlideTime = slideTime;

        if(isSliding)
            if (inputScript.GetInputUp("Slide")) StopSlide();
        #endregion

    }

    public void HandleFacing()
    {
        if(inputDirection.x > 0 && !facingRight)
        {
            squashModel.UnphysicsScale(-squashModel.InitialX, squashModel.InitialY);
            facingRight = true;
        }
        else if(inputDirection.x < 0 && facingRight)
        {
            squashModel.UnphysicsScale(-squashModel.InitialX, squashModel.InitialY);
            facingRight = false;
        }
    }

    public void HandleMovement()
    {
        // get x axis velocity
        Vector3 xAxisVelocity = rb.velocity;
        xAxisVelocity.y = 0;

        #region maximum velocity/extra gravity
        if (rb.velocity.magnitude > ultimateMaximumSpeed)
        {
            Vector3 cappedVelocity = rb.velocity.normalized * ultimateMaximumSpeed;
            rb.velocity = cappedVelocity;
        }

        if (rb.velocity.y < 0) rb.AddForce(Vector3.down * extraGravity);
        #endregion

        // override if sliding
        #region sliding logic
        if (isSliding)
        {
            rb.AddForce(-xAxisVelocity * slideDrag);

            // stop slide conditions
            if (Mathf.Abs(rb.velocity.x) < slideStopPoint) StopSlide();
            if (!grounded) StopSlide();

            return;
        }
        #endregion

        #region running logic

        if (Mathf.Abs(inputDirection.x) > 0.5f)
        {
            #region forces
            
            float currentMaxSpeed;
            float currentAccelForce;
            float currentDeccelForce;
            if(grounded)
            {
                currentMaxSpeed = maximumSpeed;
                currentAccelForce = accelerationForce;
                currentDeccelForce = deccelerationForce;
            }
            else
            {
                currentMaxSpeed = airMaximumSpeed;
                currentAccelForce = airAccelerationForce;
                currentDeccelForce = airDeccelerationForce;
            }

            // if turning left and velocity is less than max speed to the left, then add force there
            // same for the right
            if (inputDirection.x < 0 && xAxisVelocity.x > -currentMaxSpeed)
                rb.AddForce(-Vector3.right * (currentMaxSpeed + xAxisVelocity.x) * currentAccelForce);

            if (inputDirection.x > 0 && xAxisVelocity.x < currentMaxSpeed)
                rb.AddForce(Vector3.right * (currentMaxSpeed - xAxisVelocity.x) * currentAccelForce);

            // if going too fast, add force to dampen it
            if (Mathf.Abs(xAxisVelocity.x) > currentMaxSpeed + 0.01f)
            {
                Vector3 cappedVelocity = rb.velocity * currentDeccelForce;
                cappedVelocity.y = 0;
                rb.AddForce(-cappedVelocity);
            }
            #endregion

            // if running, play run particles
            if (!runParticles.isPlaying && grounded) runParticles.Play();
            if (runParticles.isPlaying && !grounded) runParticles.Stop();
        }
        else
        {
            // and stop particles
            if (runParticles.isPlaying) runParticles.Stop();
        }
        #endregion
    }

    public void HandleAnimation()
    {
        // Set velocity values
        float xVectorRatio = Mathf.Abs(rb.velocity.x) > (maximumSpeed * 0.5f) ? Mathf.Abs(rb.velocity.x) / maximumSpeed : 0;
        animator.SetFloat("xVelocity", Mathf.Abs(xVectorRatio));
        float yVectorRatio = rb.velocity.y / 100;
        animator.SetFloat("yVelocity", yVectorRatio);

        // Set input direction values
        animator.SetInteger("lookDirX",Mathf.RoundToInt(inputAttackFacing.x));
        animator.SetInteger("lookDirY",Mathf.RoundToInt(inputAttackFacing.y));

        // set grounded value
        animator.SetBool("onGround", grounded);

        // set "is sliding" value
        animator.SetBool("isSliding", isSliding);
    }

    // Jumping
    public void HandleTimers()
    {
        #region ground check
        // check if grounded by casting a circle check beneath the player
        grounded = Physics2D.OverlapCircle(transform.position + groundCheckOffset, groundCheckRadius, isGround);

        if (grounded)
        {
            // update coyote time while grounded
            currentCoyoteTime = coyoteTime;

            // play ground animation (once)
            if (!playedGroundedAnimation)
            {
                if (landParticles) landParticles.Play();
                if (landSound) landSound.PlaySound();
                playedGroundedAnimation = true;
            }
        }
        // reset ground animation
        else playedGroundedAnimation = false;
        #endregion

        // update timers
        currentJumpTime -= Time.deltaTime;
        currentSlideTime -= Time.deltaTime;
        currentCoyoteTime -= Time.deltaTime;

        // if coyote time is active, and jump was recently pressed, jump
        if (currentCoyoteTime >= 0 && currentJumpTime > 0)
        {
            Jump();
            currentJumpTime = 0;
            currentCoyoteTime = 0;
        }

        if (currentCoyoteTime >= 0 && currentSlideTime > 0)
        {
            Slide();
            currentSlideTime = 0;
        }
    }
    public void Jump()
    {
        transform.position += Vector3.up * (groundCheckRadius + 0.01f);
        // set y velocity to jump
        Vector3 addVelocity = rb.velocity;
        addVelocity.y = jumpForce;
        rb.velocity = addVelocity;

        // stretch the model (for that sweet animation)
        Vector3 scale = model.transform.localScale;
        scale.y *= 1.5f;
        model.transform.localScale = scale;

        jumpedUp = true;

        if (jumpParticles) jumpParticles.Play();
        if (jumpSound) jumpSound.PlaySound();
    }

    #region slide functions
    public void Slide()
    {
        // squash the model
        Vector3 scale = model.transform.localScale;
        scale.y *= 0.7f;
        model.transform.localScale = scale;

        // crouch hitbox
        Vector3 crouchingHeight = hitbox.transform.localScale;
        crouchingHeight.y = initHitboxHeight * slideCrouchPercent;
        hitbox.transform.localScale = crouchingHeight;

        if (!grounded) return;

        isSliding = true;

        // add inital force
        if (facingRight) rb.AddForce(Vector3.right * (slideStopPoint + slideInitForce), ForceMode2D.Impulse);
        else rb.AddForce(-Vector3.right * (slideStopPoint + slideInitForce), ForceMode2D.Impulse);

        // play slide particles
        slideParticles.Play();
        if (slideSound) slideSound.PlaySound();
    }

    public void StopSlide()
    {
        isSliding = false;

        Vector3 crouchingHeight = hitbox.transform.localScale;
        crouchingHeight.y = initHitboxHeight;
        hitbox.transform.localScale = crouchingHeight;

        // add force to stop player (if grounded)
        if (grounded) rb.AddForce(-Vector3.right * rb.velocity.x * dragForce);

        // stop particles
        slideParticles.Stop();
        if (slideSound) slideSound.StopAllSounds();
    }
    #endregion
}
