using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : PlayerState
{
    Rigidbody2D playerRb;
    AnimationManager playerAnimManager;
    Transform playerViewmodel;

    float currAttackBuffer;

    bool charged;

    public override void OnStateInit(PlayerStateManager currentPlayer){
        playerRb = currentPlayer.rb;
        playerAnimManager = currentPlayer.animator;
        playerViewmodel = currentPlayer.viewmodel;  
    }
    public override void OnStateEntered(PlayerStateManager currentPlayer){

        // attack substates, if grounded and if input is upwards
        if(currentPlayer.grounded)
        {
            if (currentPlayer.inputLookDirection.x != 0)
                playerAnimManager.PlayOverrideAnim(9, 0.2f);
            else
                playerAnimManager.PlayOverrideAnim(10, 0.2f);
        }
        else
        {
            if (currentPlayer.inputLookDirection.x != 0)
                playerAnimManager.PlayOverrideAnim(13, 0.2f);
            else
                playerAnimManager.PlayOverrideAnim(14, 0.2f);
        }
    }
    public override void OnStateExit(PlayerStateManager currentPlayer){

    }

    public override void StateFixedUpdate(PlayerStateManager currentPlayer){

        #region drag
        // add drag to player while in attack poise (if grounded)
        if (currentPlayer.grounded)
        {
            float dragAmount = Mathf.Min(Mathf.Abs(playerRb.velocity.x), Mathf.Abs(currentPlayer.dragForce));
            dragAmount *= Mathf.Sign(playerRb.velocity.x);

            playerRb.AddForce(Vector2.right * -dragAmount, ForceMode2D.Impulse);
        }
        #endregion

        #region charged attack and looping attack
        if (currentPlayer.pressingAttack)
        {
            // debounce on attack pressing, by checking if button is pressed within attacking frames
            currAttackBuffer = Time.time + currentPlayer.attackBuffer;

            // add to charge duration, and play charging animation if charge limit is reached
            currentPlayer.currChargeTime += Time.deltaTime;

            if (currentPlayer.currChargeTime >= currentPlayer.toChargeDelay)
            {
                playerAnimManager.TransitionAnim(7);
                if(currentPlayer.currChargeTime >= currentPlayer.chargeEnd && !charged)
                {
                    //playerAnimManager.TransitionAnim();
                    currentPlayer.chargeEndFx.PlaySound();
                    charged = true;
                }
            }
        }
        else
        {
            // plays charging attack if charge duration limit is reached
            if (charged)
            {
                if (currentPlayer.grounded)
                {
                    if (currentPlayer.inputLookDirection.x != 0)
                        playerAnimManager.PlayOverrideAnim(11, 0.2f);
                    else
                        playerAnimManager.PlayOverrideAnim(12, 0.2f);
                }
                else
                {
                    if (currentPlayer.inputLookDirection.x != 0)
                        playerAnimManager.PlayOverrideAnim(15, 0.2f);
                    else
                        playerAnimManager.PlayOverrideAnim(16, 0.2f);
                }
            }

            charged = false;
            currentPlayer.currChargeTime = 0;
        }

        // only transitions to movement if button is not pressed within the time debounce
        if (currAttackBuffer <= Time.time)
        {
            currentPlayer.SwitchState(currentPlayer.movementState);
        }
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
    }

    IEnumerator PAttackDebounce()
    {
        // will use this for adding animation debounce after charged attack
        // or some other implement, i'm still skeptical
        yield return null;
    }
}
