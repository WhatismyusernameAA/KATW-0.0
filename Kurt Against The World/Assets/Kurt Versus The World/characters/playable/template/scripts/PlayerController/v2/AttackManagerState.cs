using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManagerState : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerStateManager playerController;
    public Rigidbody2D rb;

    [Header("Camera And Effects")]
    [SerializeField] public Shake attackShakeSettings;
    [SerializeField] public Shake pAttackShakeSettings;

    [Space]
    public soundplayer attackFx;
    public soundplayer pAttackFx;
    public soundplayer parryFx;

    [Header("Settings")]
    public Vector3 hitcheckOffset;
    public float upwardsHitcheckOffset;
    public float hitchecksize;

    [Space]
    public Vector3 pHitcheckOffset;
    public float upwardsPHitcheckOffset;
    public float pHitchecksize;

    [Space]
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

        Vector2 lookDir = playerController.AttackDir;
        lookDir.y = 0;
        rb.AddForce(lookDir * attackMomentum, ForceMode2D.Impulse);
    }

    public void HitCheck()
    {
        // set offset
        // multiply x by input direction
        // add to y if input direction is upwards
        Vector3 setOffset = hitcheckOffset;
        setOffset.x *= playerController.AttackDir.x;
        setOffset.y += playerController.AttackDir.y > 0 ? upwardsHitcheckOffset : 0;
        setOffset += transform.position;

        Collider2D hit = Physics2D.OverlapCircle(setOffset, hitchecksize, attackable);

        if(hit)
        {
            // check for health system, then apply damage if health system is found
            Health targetHealthSystem = hit.GetComponent<Health>();

            if(targetHealthSystem)
            {
                targetHealthSystem.AddDamage(attackDmg);

                // calculate kb
                Vector3 hitForce = playerController.AttackDir * attackKb;
                targetHealthSystem.rb.AddForce(Vector3.up * -targetHealthSystem.rb.velocity.y, ForceMode2D.Impulse);
                targetHealthSystem.rb.AddForce(hitForce, ForceMode2D.Impulse);

                // play attack effects
                attackFx.transform.position = setOffset;
                attackFx.PlaySound();

                ShakeManager.instance.ShakeCamera(attackShakeSettings);
            }
        }
    }

    public void PAttackCheck()
    {
        // set offset
        // multiply x by input direction
        // add to y if input direction is upwards
        Vector3 setOffset = pHitcheckOffset;
        setOffset.x *= playerController.AttackDir.x;
        setOffset.y += playerController.AttackDir.y > 0 ? upwardsPHitcheckOffset : 0;
        setOffset += transform.position;

        Collider2D hit = Physics2D.OverlapCircle(setOffset, pHitchecksize, attackable);

        if (hit)
        {
            // check for health system, then apply damage if health system is found
            Health targetHealthSystem = hit.GetComponent<Health>();

            if (targetHealthSystem)
            {
                // add damage and stun
                targetHealthSystem.AddDamage(pAttackDmg);
                targetHealthSystem.Stun(1f);

                // calculate knockback
                Vector3 hitForce = playerController.AttackDir * pAttackKb;
                hitForce += Vector3.up * pAttackUpwardsKb;
                targetHealthSystem.rb.AddForce(Vector3.up * -targetHealthSystem.rb.velocity.y, ForceMode2D.Impulse);
                targetHealthSystem.rb.AddForce(hitForce, ForceMode2D.Impulse);


                // play attack effects
                pAttackFx.transform.position = setOffset;
                pAttackFx.PlaySound();

                ShakeManager.instance.ShakeCamera(pAttackShakeSettings);

            }
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 setOffset = hitcheckOffset;
        setOffset.x *= playerController.AttackDir.x;
        setOffset.y += playerController.AttackDir.y > 0 ? upwardsHitcheckOffset : 0;
        setOffset += transform.position;

        Vector3 pSetOffset = pHitcheckOffset;
        pSetOffset.x *= playerController.AttackDir.x;
        pSetOffset.y += playerController.AttackDir.y > 0 ? upwardsPHitcheckOffset : 0;
        pSetOffset += transform.position;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(setOffset, hitchecksize);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(pSetOffset, pHitchecksize);

    }
}
