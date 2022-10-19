using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    public Animator animator;
    // --- ANIMATIONS ---

    // grounded movement animations
    int idleAnim = Animator.StringToHash("Idle"); // 0
    int runAnim = Animator.StringToHash("Run"); // 1

    // airborne movement animations
    int airUpAnim = Animator.StringToHash("Jump"); // 2
    int airDownAnim = Animator.StringToHash("Fall"); // 3

    int landAnim = Animator.StringToHash("Land"); // 8
    public float landAnimDur;

    // sliding and crouching animations
    int slideAnim = Animator.StringToHash("Slide"); // 4
    int crouchAnim = Animator.StringToHash("Crouch"); // 5
    int crouchWalkAnim = Animator.StringToHash("Sneak"); // 6

    // attack animations
    int chargeAnim = Animator.StringToHash("Charging"); // 7

    int attackAnim = Animator.StringToHash("Attack"); // 9
    int attackUpAnim = Animator.StringToHash("Attack Up"); // 10
    int airAttackAnim = Animator.StringToHash("Air Attack"); // 13
    int airAttackUpAnim = Animator.StringToHash("Air Attack Up"); // 14
    public float attackAnimDur;
    int pAttackAnim = Animator.StringToHash("Parry"); // 11
    int pAttackUpAnim = Animator.StringToHash("Parry Up"); // 12
    int airPAttackAnim = Animator.StringToHash("Air Parry"); // 15
    int airPAttackUpAnim = Animator.StringToHash("Air Parry Up"); // 16
    public float parryAnimDur;

    int[] animations;
    float animLockTime;
    int currentAnimState;

    private void Awake()
    {
        animations = new int[]
        {
            idleAnim,
            runAnim,
            airUpAnim,
            airDownAnim,
            slideAnim,
            crouchAnim,
            crouchWalkAnim,
            chargeAnim,
            landAnim,
            attackAnim,
            attackUpAnim,
            pAttackAnim,
            pAttackUpAnim,
            airAttackAnim,
            airAttackUpAnim,
            airPAttackAnim,
            airPAttackUpAnim
        };
    }

    public void TransitionAnim(int animIndex)
    {
        if (Time.time < animLockTime) return;

        // plays state - if not the same state that's currently playing kekw
        if (animIndex == currentAnimState) return;
        animator.CrossFade(animations[animIndex], 0f, 0);
        currentAnimState = animIndex;
    }

    public void PlayOverrideAnim(int animIndex, float duration)
    {
        animLockTime = Time.time + duration;
        animator.CrossFade(animations[animIndex], 0f, 0);
        currentAnimState = animIndex;
    }
}
