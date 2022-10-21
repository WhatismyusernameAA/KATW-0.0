using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackState : PlayerState
{
    // player dependencies
    Rigidbody2D playerRb;
    AnimationManager playerAnimManager;
    Transform playerViewmodel;
    
    // state-stored values
    float currAttackBuffer;
    float attackDur;
    float comboDur;
    float comboCooldown;

    bool pressedAttack;
    bool charged;

    public override void OnStateInit(PlayerStateManager currentPlayer){
        playerRb = currentPlayer.rb;
        playerAnimManager = currentPlayer.animator;
        playerViewmodel = currentPlayer.viewmodel;

        attackDur = currentPlayer.attackDuration;
    }
    public override void OnStateEntered(PlayerStateManager currentPlayer){
        currAttackBuffer = Time.time + attackDur;
        comboDur = Time.time + currentPlayer.comboDuration;

        PlayAttack(currentPlayer);
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
        if (currentPlayer.pressingAttack) {
            pressedAttack = true;

            // debounce on attack pressing, by checking if button is pressed within attacking frames
            //currAttackBuffer = Time.time + currentPlayer.attackBuffer;

            #region charge logic
            // add to charge duration
            currentPlayer.currChargeTime += Time.deltaTime;

            // play charging animation if player is still pressing button (they want to start charging)
            if (currentPlayer.currChargeTime >= attackDur) {
                playerAnimManager.TransitionAnim(7);
                
                // play charge finished animation when charge limit is reached
                if (currentPlayer.currChargeTime >= currentPlayer.chargeEnd && !charged) {
                    //playerAnimManager.TransitionAnim();
                    
                    charged = true;

                    //play effects
                    currentPlayer.chargeEndFx.PlaySound();
                    //ShakeManager.instance.SetNoiseProperties(currentPlayer.chargingShakeSettings);
                }
            }
            #endregion
        }
        else {
            // plays charging attack if charge duration limit is reached and attack is released
            if (charged)
                PlayPAttack(currentPlayer);

            // resets everything
            charged = false;
            currentPlayer.currChargeTime = 0;
        }

        // only transitions to movement if button is not pressed within attack duration
        if (currAttackBuffer <= Time.time) {
            if (pressedAttack) {
                currAttackBuffer += attackDur;
            }
            // else, add another attack to the timer    
            else {
                currentPlayer.SwitchState(currentPlayer.movementState);
            }
            pressedAttack = false;
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

    void PlayAttack(PlayerStateManager player)
    {
        player.AttackDir = player.inputLookDirection;
        // attack substates, if grounded and if input is upwards
        if (player.grounded)
        {
            if (player.inputLookDirection.x != 0)
                playerAnimManager.PlayOverrideAnim(9, 0.2f);
            else
                playerAnimManager.PlayOverrideAnim(10, 0.2f);
        }
        else
        {
            if (player.inputLookDirection.x != 0)
                playerAnimManager.PlayOverrideAnim(13, 0.2f);
            else
                playerAnimManager.PlayOverrideAnim(14, 0.2f);
        }
    }

    void PlayPAttack(PlayerStateManager player)
    {
        player.AttackDir = player.inputLookDirection;
        if (player.grounded)
        {
            if (player.inputLookDirection.x != 0)
                playerAnimManager.PlayOverrideAnim(15, 0.2f);
            else
                playerAnimManager.PlayOverrideAnim(16, 0.2f);
        }
        else
        {
            if (player.inputLookDirection.x != 0)
                playerAnimManager.PlayOverrideAnim(15, 0.2f);
            else
                playerAnimManager.PlayOverrideAnim(16, 0.2f);
        }
    }
}
