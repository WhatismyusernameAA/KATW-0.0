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
    public bool pressingSlide;
    public bool pressingAttack;


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
    bool landed;

    [Space]
    public Vector2 groundCheckOffset;
    public Vector2 groundCheckDimentions;
    public LayerMask groundLayer;

    [Space]
    public float jumpForce;
    [Range(0.0f,1.0f)]
    public float jumpCutPercent;


    [Header("Crouching And Sliding")]
    public bool canCrouchAndSlide;
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
    public float slideCooldown;

    [Space]
    public bool slideToCrouch;
    public bool slideMidair;

    [Header("Attacks and Charged Attacks")]
    bool attacked;

    public float toChargeDelay;
    public float chargeDuration;
    float currChargeTime;


    [Header("Rendering and Animation")]
    public bool isRight;

    // grounded movement animations
    int idleAnim = Animator.StringToHash("Idle");
    int walkAnim = Animator.StringToHash("Walk");
    int runAnim = Animator.StringToHash("Run");

    // airborne movement animations
    int airUpAnim = Animator.StringToHash("Jump");
    int airDownAnim = Animator.StringToHash("Fall");

    int landAnim = Animator.StringToHash("Land");
    public float landAnimDur;

    // sliding and crouching animations
    int slideAnim = Animator.StringToHash("Slide");
    int crouchAnim = Animator.StringToHash("Crouch");
    int crouchWalkAnim = Animator.StringToHash("Sneak");

    // attack animations
    int chargeAnim = Animator.StringToHash("Charging");

    int attackAnim = Animator.StringToHash("Attack");
    int attackUpAnim = Animator.StringToHash("Attack Up");
    public float attackAnimDur;
    int powerAttackAnim = Animator.StringToHash("Parry");
    public float parryAnimDur;



    float animLockTime;
    int currentAnimState;

    [Header("Effects")]
    public soundplayer jumpSound;
    public soundplayer landSound;
    public soundplayer slideSound;

    public ParticleSystem slideParticles;


    // FUNCTIONS ------
    private void Awake() {
        // get original hitbox size for later use
        if (hitbox) hitboxOriginSize = hitbox.transform.localScale;
    }

    private void Update() {
        #region directional input
        // sets inputvector to arrow keys
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

        if(inputVector.sqrMagnitude > 0)
        {
            inputLookDirection = inputVector;
            inputLookDirection.x *= inputLookDirection.y == 0 ? 1 : 0;
        }
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
        if (currInput.GetInputDown("Slide") && canCrouchAndSlide)
        {
            pressingSlide = true;

            // starts slide if there is directional input
            if (Mathf.Abs(inputVector.x) > 0.1)
            {
                // prevents slides in midair (unless overriden)
                if (!slideMidair && !grounded) return;

                isSliding = true;

                // adds slide force in direction of input
                if (rb.velocity.x >= topSpeed)
                {
                    Vector2 slideForceVector = inputVector;
                    slideForceVector.y = 0;
                    rb.AddForce(slideForceVector * slideInitForce, ForceMode2D.Impulse);
                }
                // if full speed is not reached, immediately set speed to that limit (so no slow slide)
                else
                {
                    Vector2 slideVelocityVector = inputVector;
                    slideVelocityVector.y = rb.velocity.y;
                    slideVelocityVector.x *= topSpeed + slideInitForce;
                    rb.velocity = slideVelocityVector;
                }

                // Unity Particle System TM
                slideSound.PlaySound(); 
                slideParticles.Play();
            }

            // only permits crouching if on ground
            if (grounded)
                isCrouching = true;
        }
        // if crouch button is released, release sliding and crouching
        else if (currInput.GetInputUp("Slide"))
        {
            pressingSlide = false;

            // deactivates slide (if slide is currently activated)
            if (isSliding)
            {
                isSliding = false;
                slideSound.StopAllSounds();
                slideParticles.Stop();

                // disables slide and crouch for a short time
                StartCoroutine(StartSlideCooldown(slideCooldown));
            }

            isCrouching = false;
        }
        #endregion

        #region attack input
        if (currInput.GetInput("Attack") && !isSliding)
        {
            canCrouchAndSlide = false;
            currChargeTime += Time.deltaTime;
        }
        else if (currInput.GetInputUp("Attack"))
        {
            currChargeTime = 0;
            attacked = false;
        }

        #endregion
        #endregion
    }

    private void FixedUpdate() {
        #region movement
        // multiplier for crouching (and maybe speed effects?)
        float walkMultiplier = 
            (isCrouching ? crouchWalkMultiplier : 1) *
            (attacked ? 0 : 1);

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
        // casts box towards feet to check for ground
        Vector2 position = transform.position;
        grounded = Physics2D.OverlapBox(position - groundCheckOffset, groundCheckDimentions, 0, groundLayer);

        // checks if land animation plays, which means it can say it isnt grounded anymore
        if (!grounded) landed = false;
        #endregion

        #region jumping
        // add upwards force when jump button is pressed (or the debounce is still active)
        if (grounded && currJumpBuffer > 0)
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            currJumpBuffer = 0;

            // stops sliding and crouching
            isSliding = false;
            isCrouching = false;

            // stops Unity's Particle System TM
            slideSound.StopAllSounds();
            slideParticles.Stop();
            jumpSound.PlaySound();

            // disables slide and crouch for a short time
            StartCoroutine(StartSlideCooldown(slideCooldown));
        }
        #endregion

        #region crouching and sliding

        // shrink hitbox while sliding or crouching
        if (isCrouching || isSliding)
        {
            if (!hitbox) return;

            Vector3 crouchedScale = hitboxOriginSize;
            crouchedScale.y *= crouchShrinkPercent;
            hitbox.transform.localScale = crouchedScale;
        }
        else
        {
            // reset hitbox size
            hitbox.transform.localScale = hitboxOriginSize;
        }

        // if in slide state
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

                // moves slide to crouch state if enabled
                if (!slideToCrouch) isCrouching = false;

                // disables slide and crouch for a short time
                StartCoroutine(StartSlideCooldown(slideCooldown));

                // stops particles --and sound
                slideSound.StopAllSounds();
                slideParticles.Stop();
            }
        }
        #endregion

        #region attacking

        #endregion

        #region animation
        // handles facing.
        //if input is opposite to where player is currently facing, invert the scale
        if (viewmodel && !isSliding)
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


        // Hardcoded Animation System from Tarodev : https://www.youtube.com/watch?v=ZwLekxsSY3Y
        // Mechanim is too wildin man
        if (animator)
        {

            // decides state using conditions
            int animState = GetState();

            // plays state - if not the same state that's currently playing kekw
            if (animState == currentAnimState) return;
            animator.CrossFade(animState, 0f, 0);
            currentAnimState = animState;
        }    
        #endregion
    }

    // gets animation state from conditions
    private int GetState() {
        if (Time.time < animLockTime) return currentAnimState;

        // priority system... sorts out the current state from condition trees
        if (isSliding) return slideAnim;

        if (currChargeTime > 0 && !attacked)
        {
            attacked = true;
            StartCoroutine(StartSlideCooldown(attackAnimDur));
            return LockState(inputLookDirection.y == 0 ? attackAnim : attackUpAnim, attackAnimDur);
        }

        if (currChargeTime > toChargeDelay) return chargeAnim;

        if (grounded) {
            // I feel like there's a better way to do this...
            if (!landed) {
                landed = true;
                landSound.PlaySound();
                return LockState(landAnim, landAnimDur);
            }


            if (isCrouching) return inputVector.x == 0 ? crouchAnim : crouchWalkAnim;
            return inputVector.x == 0 ? idleAnim : runAnim;
        }
        return rb.velocity.y > 0 ? airUpAnim : airDownAnim;

        // when state is locked, cannot transition to other states until this state is finished.
        int LockState(int s, float t) {
            animLockTime = Time.time + t;
            return s;
        }
    }

    // disables slide and crouch for a short time
    IEnumerator StartSlideCooldown(float cooldown)
    {
        canCrouchAndSlide = false;
        yield return new WaitForSeconds(cooldown);
        canCrouchAndSlide = true;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 position = transform.position;
        Gizmos.DrawWireCube(position - groundCheckOffset, groundCheckDimentions);
    }

}
