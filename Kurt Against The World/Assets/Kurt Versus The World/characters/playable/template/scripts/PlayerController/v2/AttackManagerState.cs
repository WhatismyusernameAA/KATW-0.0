using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManagerState : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerStateManager playerController;
    public Rigidbody2D rb;

    public soundplayer attackFx;
    public soundplayer pAttackFx;
    public soundplayer parryFx;

    [Header("Settings")]
    public Vector3 hitcheckOffset;
    public float upwardsHitcheckOffset;
    public float hitchecksize;
    public LayerMask attackable;
    public LayerMask parriable;

    [Space]
    public float attackMomentum;
    float currAttackMomentum;

    [Space]
    public float attackKb;
    float currAttackKb;
    public float pAttackKb;
    float currPAttackkb;
    public float pAttackUpwardsKb;
    float currPAttackUpwardsKb;

    public int attackDmg;
    public int pAttackDmg;
    public int parryDmg;

    public int dmgMultiplier;




    public void Fling()
    {
        if (!playerController.grounded) return;

        Vector2 lookDir = playerController.inputLookDirection;
        lookDir.y = 0;
        rb.AddForce(lookDir * attackMomentum, ForceMode2D.Impulse);
    }

    public void HitCheck()
    {
        // set offset
        // multiply x by input direction
        // add to y if input direction is upwards
        Vector3 setOffset = hitcheckOffset;
        setOffset.x *= playerController.inputLookDirection.x;
        setOffset.y += playerController.inputLookDirection.y > 0 ? upwardsHitcheckOffset : 0;
        setOffset += transform.position;

        Collider2D hit = Physics2D.OverlapCircle(setOffset, hitchecksize, attackable);

        if(hit)
        {
            // check for health system, then apply damage if health system is found
            Health targetHealthSystem = hit.GetComponent<Health>();

            if(targetHealthSystem)
            {
                targetHealthSystem.AddDamage(attackDmg);

                Vector3 hitForce = playerController.inputLookDirection * attackKb;
                targetHealthSystem.rb.AddForce(Vector3.up * -targetHealthSystem.rb.velocity.y, ForceMode2D.Impulse);
                targetHealthSystem.rb.AddForce(hitForce, ForceMode2D.Impulse);

                // play attack effects

                attackFx.transform.position = setOffset;
                attackFx.PlaySound();
            }
        }
    }

    public void PAttackCheck()
    {
        // set offset
        // multiply x by input direction
        // add to y if input direction is upwards
        Vector3 setOffset = hitcheckOffset;
        setOffset.x *= playerController.inputLookDirection.x;
        setOffset.y += playerController.inputLookDirection.y > 0 ? upwardsHitcheckOffset : 0;
        setOffset += transform.position;

        Collider2D hit = Physics2D.OverlapCircle(setOffset, hitchecksize, attackable);

        if (hit)
        {
            // check for health system, then apply damage if health system is found
            Health targetHealthSystem = hit.GetComponent<Health>();

            if (targetHealthSystem)
            {
                targetHealthSystem.AddDamage(pAttackDmg);
                targetHealthSystem.Stun(1f);

                Vector3 hitForce = playerController.inputLookDirection * pAttackKb;
                hitForce += Vector3.up * pAttackUpwardsKb;
                targetHealthSystem.rb.AddForce(Vector3.up * -targetHealthSystem.rb.velocity.y, ForceMode2D.Impulse);
                targetHealthSystem.rb.AddForce(hitForce, ForceMode2D.Impulse);


                // play attack effects

                pAttackFx.transform.position = setOffset;
                pAttackFx.PlaySound();
            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 setOffset = hitcheckOffset;
        setOffset.x *= playerController.inputLookDirection.x;
        setOffset.y += playerController.inputLookDirection.y > 0 ? upwardsHitcheckOffset : 0;
        setOffset += transform.position;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(setOffset, hitchecksize);
    }
}
