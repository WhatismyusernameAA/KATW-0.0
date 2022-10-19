using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideState : PlayerState
{
    Rigidbody2D playerRb;
    AnimationManager playerAnimManager;
    Transform playerViewmodel;
    Collider2D playerHitbox;

    Vector3 hitboxOriginSize;

    public override void OnStateInit(PlayerStateManager currentPlayer)
    {
        playerRb = currentPlayer.rb;
        playerAnimManager = currentPlayer.animator;
        playerViewmodel = currentPlayer.viewmodel;
        playerHitbox = currentPlayer.hitbox;

        hitboxOriginSize = playerHitbox.transform.localScale;
    }
    public override void OnStateEntered(PlayerStateManager currentPlayer)
    {
        currentPlayer.isSliding = true;

        Vector2 slideForceVector = currentPlayer.inputVector;

        // adds slide force in direction of input
        if (playerRb.velocity.x >= currentPlayer.topSpeed)
        {
            slideForceVector.y = 0;
            playerRb.AddForce(slideForceVector * currentPlayer.slideInitForce, ForceMode2D.Impulse);
        }
        // if full speed is not reached, immediately set speed to that limit (so no slow slide)
        else
        {
            slideForceVector.y = playerRb.velocity.y;
            slideForceVector.x *= currentPlayer.topSpeed + currentPlayer.slideInitForce;
            playerRb.velocity = slideForceVector;
        }

        currentPlayer.slideFx.PlaySound();
    }

    public override void OnStateExit(PlayerStateManager currentPlayer)
    {

    }

    public override void StateFixedUpdate(PlayerStateManager currentPlayer)
    {
        #region slide
        // add drag to slide (so it doesn't go on forever)
        float dragAmount = Mathf.Abs(playerRb.velocity.x) * currentPlayer.slideDrag;
        dragAmount *= Mathf.Sign(playerRb.velocity.x);
        playerRb.AddForce(Vector2.right * -dragAmount);

        // check if velocity cap is reached; so that slide stops when unnecessary
        if (Mathf.Abs(playerRb.velocity.x) < currentPlayer.slideVelocityCap)
        {
            OnStateExit(currentPlayer);
            currentPlayer.SwitchState(currentPlayer.movementState);
            // disables slide and crouch for a short time
            //StartCoroutine(StartSlideCooldown(slideCooldown));

            // stops particles --and sound
            //slideSound.StopAllSounds();
            //slideParticles.Stop();
        }

        if (!currentPlayer.pressingSlide || (currentPlayer.grounded && currentPlayer.currJumpBuffer > Time.time))
        {
            // return to movement state when not pressing slide anymore
            playerHitbox.transform.localScale = hitboxOriginSize;

            currentPlayer.slideFx.StopAllSounds();

            currentPlayer.SwitchState(currentPlayer.movementState);
        }
        #endregion

        #region animation
        playerAnimManager.TransitionAnim(4);
        #endregion
    }
}
