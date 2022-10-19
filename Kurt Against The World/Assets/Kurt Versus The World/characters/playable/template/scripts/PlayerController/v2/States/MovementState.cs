using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementState : PlayerState
{
    Rigidbody2D playerRb;
    AnimationManager playerAnimManager;
    Transform playerViewmodel;
    Collider2D playerHitbox;

    Vector3 hitboxOriginSize;

    bool isCrouching;

    // -- METHODS --
    public override void OnStateInit(PlayerStateManager currentPlayer)
    {
        // set all values
        playerRb = currentPlayer.rb;
        playerAnimManager = currentPlayer.animator;
        playerViewmodel = currentPlayer.viewmodel;
        playerHitbox = currentPlayer.hitbox;

        hitboxOriginSize = playerHitbox.transform.localScale;
    }

    public override void OnStateEntered(PlayerStateManager currentPlayer)
    {
    }

    public override void OnStateExit(PlayerStateManager currentPlayer)
    {
        
    }

    public override void StateFixedUpdate(PlayerStateManager currentPlayer)
    {
        #region movement
        // multiplier for crouching (and maybe speed effects?)
        float walkMultiplier = (currentPlayer.isCrouching ? currentPlayer.crouchWalkMultiplier : 1);

        // find target velocity and difference between current velocity and target velocity
        float targetVelo = currentPlayer.inputVector.x * currentPlayer.topSpeed; //* walkMultiplier;
        float speedDifference = targetVelo - playerRb.velocity.x;
        float accelRate;

        if (currentPlayer.grounded)
        {
            // when the player is on the ground
            // check if we need to deccelerate to the target velocity or accelerate to the target velocity
            accelRate = (Mathf.Abs(targetVelo) > 0.01f) ? currentPlayer.accelForce : currentPlayer.deccelForce;

            // pull the brakes when no input is detected
            if (Mathf.Abs(currentPlayer.inputVector.x) < 0.01)
            {
                float dragAmount = Mathf.Min(Mathf.Abs(playerRb.velocity.x), Mathf.Abs(currentPlayer.dragForce));
                dragAmount *= Mathf.Sign(playerRb.velocity.x);

                playerRb.AddForce(Vector2.right * -dragAmount, ForceMode2D.Impulse);
            }
        }
        else
        {
            // when the player is in the air
            // check if we need to deccelerate to the target velocity or accelerate to the target velocity
            accelRate = (Mathf.Abs(targetVelo) > 0.01f) ? currentPlayer.airAccelForce : currentPlayer.airDeccelForce;

            if (Mathf.Abs(currentPlayer.inputVector.x) < 0.01)
            {
                float dragAmount = Mathf.Min(Mathf.Abs(playerRb.velocity.x), Mathf.Abs(currentPlayer.airDragForce));
                dragAmount *= Mathf.Sign(playerRb.velocity.x);
                playerRb.AddForce(Vector2.right * -dragAmount, ForceMode2D.Impulse);
            }
        }

        // calculate applied force and apply
        float appliedForce = Mathf.Pow(Mathf.Abs(speedDifference) * accelRate, currentPlayer.velPower) * Mathf.Sign(speedDifference);
        playerRb.AddForce(appliedForce * Vector2.right);
        #endregion

        #region jumping
        // add upwards force when jump button is pressed (or the debounce is still active)
        if (currentPlayer.grounded){
            if (currentPlayer.currJumpBuffer > Time.time){
                playerRb.AddForce(Vector2.up * currentPlayer.jumpForce, ForceMode2D.Impulse);
                currentPlayer.currJumpBuffer = 0;

                currentPlayer.jumpFx.PlaySound();
                // stops sliding and crouching
                //isSliding = false;
                //isCrouching = false;

                // stops Unity's Particle System TM
                //slideSound.StopAllSounds();
                //slideParticles.Stop();
                //jumpSound.PlaySound();
            }

            #region crouching and sliding
            // if pressing crouch/slide button, and player can crouch, then make player crouch.
            if (currentPlayer.pressingSlide && !currentPlayer.isCrouching && currentPlayer.canCrouchAndSlide)
            {
                Vector3 crouchedScale = hitboxOriginSize;
                crouchedScale.y *= currentPlayer.crouchShrinkPercent;
                playerHitbox.transform.localScale = crouchedScale;

                currentPlayer.isCrouching = true;

                if (currentPlayer.inputVector.x != 0)
                    currentPlayer.SwitchState(currentPlayer.slideState);

            }
            else if (!currentPlayer.pressingSlide)
            {
                playerHitbox.transform.localScale = hitboxOriginSize;
                currentPlayer.isCrouching = false;
            }
            #endregion
        }

        // handle jump cuts
        if (currentPlayer.jumpReleased){
            if (playerRb.velocity.y > 0){
                // cut off y velocity
                Vector2 velocity = playerRb.velocity;
                velocity.y *= currentPlayer.jumpCutPercent;
                playerRb.velocity = velocity;
            }

            // feedback the release button
            currentPlayer.jumpReleased = false;
            }

        #endregion

        #region attacking transition
        if (currentPlayer.pressingAttack)
            currentPlayer.SwitchState(currentPlayer.attackingState);
        #endregion

        #region facing
        // if facing left, turn viewmodel left
        if (currentPlayer.inputVector.x > 0 && !currentPlayer.isRight)
        {
            Vector2 scale = playerViewmodel.localScale;
            scale.x = -scale.x;
            playerViewmodel.localScale = scale;

            currentPlayer.isRight = true;
        }
        // vice versa
        else if (currentPlayer.inputVector.x < 0 && currentPlayer.isRight)
        {
            Vector2 scale = playerViewmodel.localScale;
            scale.x = -scale.x;
            playerViewmodel.localScale = scale;

            currentPlayer.isRight = false;
        }
        #endregion

        #region animation
        // if landed on ground, play landing animation and particles
        if(currentPlayer.grounded && !currentPlayer.landed)
        {
            playerAnimManager.PlayOverrideAnim(8, 0.05f);

            currentPlayer.landFx.PlaySound();
            currentPlayer.landed = true;
        }
        // if grounded, play ground animations, else, play air animations
        // 0 = idle, 1 = running, 2 = jumping, 3 = falling
        if (currentPlayer.grounded)
            if (!currentPlayer.isCrouching)
                playerAnimManager.TransitionAnim(currentPlayer.inputVector.x == 0 ? 0 : 1);
        // if crouched, play crouched animations
        // 5 = crouching, 6 = sneaking
            else
                playerAnimManager.TransitionAnim(currentPlayer.inputVector.x == 0 ? 5 : 6);
        else
            playerAnimManager.TransitionAnim(playerRb.velocity.y > 0 ? 2 : 3);
        #endregion
    }
}

