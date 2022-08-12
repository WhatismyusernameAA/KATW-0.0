using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewPlayerScript : MonoBehaviour
{
    public inputscript playerInput;
    public Rigidbody2D rb;
    public Collider2D hitbox;
    float initColliderHeight;

    [Header("Input Settings")]
    public float inputJumpDelay;
    float currentInputJumpDelay;

    public float inputSlideDelay;
    float currentInputSlideDelay;

    [Header("Movement Settings")]
    public float topSpeed;
    public float accelerationForce;
    public float deccelerationForce;
    public float velPower;
    [Space]
    public float stopDragForce;
    public float airDeccelerationMultiplier;

    [Header("Grounded Settings")]
    public bool isGrounded;
    public Vector3 groundCheckOffset;
    public Vector2 groundCheckDimentions;
    public LayerMask isGround;

    [Header("Jump Settings")]
    public float initialJumpForce;
    [Range(0.0f,1.0f)]
    public float jumpCutPercent;
    public float fallJumpMultiplier;

    float currentGravityScale;

    [Header("Slide Settings")]
    bool isSliding;
    public float initalSlideForce;
    [Range(0.0f, 1.0f)]
    public float hitboxCrouchPercent;

    public Vector3 InputDirection;


    // Start is called before the first frame update
    void Start()
    {
        currentGravityScale = rb.gravityScale;
        initColliderHeight = hitbox.transform.localScale.y;
    }

    private void Update()
    {
        HandleInput();
    }
    private void FixedUpdate()
    {
        HandleMovement();
    }

    public void HandleInput()
    {
        if (!playerInput) return;

        #region Directional Input
        InputDirection.x = 0;
        if(playerInput.GetInput("Right"))
            InputDirection.x += 1;
        if (playerInput.GetInput("Left"))
            InputDirection.x -= 1;

        InputDirection.y = 0;
        if (playerInput.GetInput("Up"))
            InputDirection.y += 1;
        if (playerInput.GetInput("Down"))
            InputDirection.y -= 1;
        #endregion

        #region Input Timers
        if (playerInput.GetInputDown("Jump")) 
            currentInputJumpDelay = inputJumpDelay;
        if (playerInput.GetInputUp("Jump")) JumpCut();

        if (playerInput.GetInputDown("Slide"))
            currentInputSlideDelay = inputSlideDelay;
        if (playerInput.GetInputUp("Slide") && isSliding) EndSlide();
        #endregion
    }

    public void HandleMovement()
    {
        // Excerpts from "Improve Your Platformer with Forces | Examples in Unity" : https://www.youtube.com/watch?v=KbtcEVCM7bw
        #region Run
        if(!isSliding)
        {
            float targetSpeed = InputDirection.x * topSpeed;
            float speedDifference = targetSpeed - rb.velocity.x;

            float accelerationRate = (Mathf.Abs(speedDifference) > 0.01f) ? accelerationForce : deccelerationForce  * (isGrounded ? 1 : airDeccelerationMultiplier);

            float xMovement = Mathf.Pow(Mathf.Abs(speedDifference) * accelerationRate, velPower) * Mathf.Sign(speedDifference);

            rb.AddForce(Vector2.right * xMovement);
        }
        #endregion

        #region dragForce
        if (isGrounded && !isSliding && Mathf.Abs(InputDirection.magnitude) < 0.01f)
        {
            float dragAmount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(stopDragForce));
            dragAmount *= Mathf.Sign(rb.velocity.x);
            rb.AddForce(Vector2.right * -dragAmount, ForceMode2D.Impulse);
        }
        #endregion
        //

        #region ground check
        isGrounded = Physics2D.OverlapBox(
            transform.position + groundCheckOffset,
            groundCheckDimentions,
            0,
            isGround
            );
        #endregion

        #region Jumping
        currentInputJumpDelay -= Time.deltaTime;
        if (isGrounded && currentInputJumpDelay > 0)
            Jump();

        if (rb.velocity.y < 0)
            rb.gravityScale = currentGravityScale * fallJumpMultiplier;
        else
            rb.gravityScale = currentGravityScale;
        #endregion

        #region Sliding
        currentInputSlideDelay -= Time.deltaTime;
        if(isGrounded && !isSliding && currentInputSlideDelay > 0)
            StartSlide();
        
        #endregion
    }

    public void StartSlide()
    {
        currentInputSlideDelay = 0;
        isSliding = true;
        rb.AddForce(Vector2.right * InputDirection.x * initalSlideForce, ForceMode2D.Impulse);
        Vector3 crouchingHeight = hitbox.transform.localScale;
        crouchingHeight.y = initColliderHeight * hitboxCrouchPercent;
        hitbox.transform.localScale = crouchingHeight;
    }

    public void EndSlide()
    {
        isSliding = false;
        Vector3 crouchingHeight = hitbox.transform.localScale;
        crouchingHeight.y = initColliderHeight;
        hitbox.transform.localScale = crouchingHeight;
    }

    // Jumping
    public void Jump()
    {
        currentInputJumpDelay = 0;
        Vector3 setVelocity = rb.velocity;
        setVelocity.y = initialJumpForce;
        rb.velocity = setVelocity;
        if (isSliding) EndSlide();
    }

    public void JumpCut()
    {
        if(rb.velocity.y > 0 && !isGrounded)
        {
            rb.AddForce(Vector2.down * rb.velocity.y * (1 - jumpCutPercent), ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawCube(transform.position + groundCheckOffset, groundCheckDimentions);
    }
}
