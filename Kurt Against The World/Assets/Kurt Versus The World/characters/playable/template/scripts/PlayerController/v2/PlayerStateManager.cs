using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateManager : MonoBehaviour
{
    PlayerState currentState;
    public MovementState movementState = new MovementState();
    public SlideState slideState = new SlideState();
    public AttackState attackingState = new AttackState();

    // --- SETTINGS ---
    // Imported from the original PlayerControllerV2 script
    // (this statemanager stores all the variables, altho I might reimplement as time goes on)
    // 30-09-2022 TO DO - turn all of this into state-stored variables once I'm satisfied with the movement

    [Header("Dependencies")]
    public inputscript currInput;

    public Rigidbody2D rb;
    public Collider2D hitbox;

    public Transform viewmodel;
    public AnimationManager animator;


    [Header("Input")]
    public Vector2 inputVector;
    public Vector2 inputLookDirection;
    public float inputLookX;

    [Space]
    public float jumpBuffer;
    public float currJumpBuffer;
    public bool jumpReleased;

    [Space]
    public bool pressingSlide;

    public float attackBuffer;
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
    public bool landed;


    [Space]
    public Vector2 groundCheckOffset;
    public Vector2 groundCheckDimentions;
    public LayerMask groundLayer;

    [Space]
    public float jumpForce;
    [Range(0.0f, 1.0f)]
    public float jumpCutPercent;



    [Header("Crouching And Sliding")]
    public bool canCrouchAndSlide;
    public bool isCrouching;
    public bool isSliding;

    [Space]
    [Range(0.5f, 1.0f)]
    public float crouchShrinkPercent;
    //Vector3 hitboxOriginSize;
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
    //bool attacked;

    public float toChargeDelay;
    public float chargeEnd;
    public float currChargeTime;


    [Header("Rendering and Animation")]
    public bool isRight;

    [Header("Effects")]
    public soundplayer jumpFx;
    public soundplayer landFx;
    public soundplayer slideFx;

    [Space]
    public soundplayer chargeEndFx;

    // --- METHODS ---
    private void Awake()
    {
        // initialize each state
        movementState.OnStateInit(this);
        slideState.OnStateInit(this);
        attackingState.OnStateInit(this);

        // set current state to movement
        SwitchState(movementState);
    }

    private void Update()
    {
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

        if (inputVector.x != 0)
            inputLookX = inputVector.x;

        if (inputVector.y != 0)
        {
            inputLookDirection.y = inputVector.y;
            inputLookDirection.x = 0;
        }
        else
        {
            inputLookDirection.y = 0;
            inputLookDirection.x = inputLookX;
        }
        #endregion

        #region jump input
        // reset jump buffer whenever jump button is pressed
        if (currInput.GetInputDown("Jump")) currJumpBuffer = Time.time + jumpBuffer;

        if (currInput.GetInputUp("Jump")) jumpReleased = true;
        #endregion

        #region crouch input
        if (currInput.GetInputDown("Slide")) pressingSlide = true;
        if (currInput.GetInputUp("Slide")) pressingSlide = false;
        #endregion

        #region attacking
        if (currInput.GetInputDown("Attack")) pressingAttack = true;
        if (currInput.GetInputUp("Attack")) pressingAttack = false;
        #endregion
    }

    private void FixedUpdate()
    {
        #region grounded check
        // casts box towards feet to check for ground
        Vector2 position = transform.position;
        grounded = Physics2D.OverlapBox(position - groundCheckOffset, groundCheckDimentions, 0, groundLayer);

        // checks if land animation plays, which means it can say it isnt grounded anymore
        if (!grounded) landed = false;
        #endregion

        currentState.StateFixedUpdate(this);
    }

    public void SwitchState(PlayerState state)
    {
        // switch state, then execute enter method
        currentState = state;
        currentState.OnStateEntered(this);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 position = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawCube(position - groundCheckOffset, groundCheckDimentions);
    }

}
