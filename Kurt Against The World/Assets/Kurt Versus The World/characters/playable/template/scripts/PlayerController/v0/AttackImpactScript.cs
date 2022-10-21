using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class AttackImpactScript : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    public SquashStretchHandler modelSquashStretch;

    [Header("Attack Hit Registration")]
    public Vector3 inputDir;
    public Vector3 checkOffset;
    public float checkRadius;

    public LayerMask enemies;
    public LayerMask enemyProjectiles;

    [Header("Damage Settings")]
    public int normalAttackDamage;
    public int chargedAttackDamage;


    [Header("Knockback and Velocity Changes Settings")]
    public float forwardsLungeForce;
    public float upwardsLungeForce;

    public float cForwardsLungeForce;
    public float cUpwardsLungeForce;


    public float backwardsKb;
    public float cBackwardsKb;
    public float DownBounceKb;

    [Space]
    public float attackKb;
    public float cAttackKb;
    public float upwardskb;

    [Range(0.0f, 1.0f)]
    public float velocityRetentionPercent;

    bool charged;
    bool hit;

    [Header("Particools")]
    public ParticleSystem chargingInitParticles;
    public ParticleSystem punchParticles;
    public ParticleSystem chargedPunchParticles;

    [Header("Hit Stop Settings")]
    public float haltTimeScale;
    public float punchSlowDuration;
    public float chargedPunchSlowDuration;

    [Header("Shake Settings")]
    public float punchShakeDuration;
    public float punchShakeAmplitude;
    public float punchShakeFrequency;

    [Space]
    public float ChargedpunchShakeDuration;
    public float ChargedpunchShakeAmplitude;
    public float ChargedpunchShakeFrequency;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + getCheckOffsetWithScale(),checkRadius);
    }

    public void StartCharge()
    {
        charged = true;
        if (chargingInitParticles)
        {
            chargingInitParticles.Play();
            soundplayer chargesound = chargingInitParticles.GetComponent<soundplayer>();
            if (chargesound) chargesound.PlaySound();
        }
    }

    public void Lunge()
    {
        Vector3 lungeVelocity = new Vector3(
                inputDir.x * (charged ? cForwardsLungeForce : forwardsLungeForce),
                inputDir.y * (charged ? cUpwardsLungeForce : upwardsLungeForce),
                0
            );

        rb.AddForce(lungeVelocity, ForceMode2D.Impulse);
    }

    public void CheckForTargets()
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position + getCheckOffsetWithScale(),checkRadius, enemies);

        if (hitColliders.Length == 0) return;
        if (hit) return;

        hit = true;

        for(int i = 0; i < hitColliders.Length; i++)
        {
            Debug.Log("Ive hit " + hitColliders[i].name);
            onImpact(hitColliders[i].gameObject);
        }

        rb.AddForce(
            -inputDir * (charged ? cBackwardsKb : backwardsKb),
            ForceMode2D.Impulse);

        if (rb.velocity.y < 0 && inputDir.y < 0)
            BounceUp();

        // Play Particles
        #region particles
        if (!charged)
        {
            if (punchParticles)
            {
                punchParticles.transform.position = transform.position + getCheckOffsetWithScale();
                punchParticles.Play();
                soundplayer punchsound = punchParticles.GetComponent<soundplayer>();
                if (punchsound) punchsound.PlaySound();
            }
        }
        else
        {
            if (chargedPunchParticles)
            {
                chargedPunchParticles.transform.position = transform.position + getCheckOffsetWithScale();
                chargedPunchParticles.Play();
                soundplayer punchsound = chargedPunchParticles.GetComponent<soundplayer>();
                if (punchsound) punchsound.PlaySound();
            }
        }
        #endregion

        // Add Time Pause to really sell a hit
        #region timestop and shake camera
        if (TimescaleManager.instance)
        {
            if (charged)
                TimescaleManager.instance.HaltTime(haltTimeScale, chargedPunchSlowDuration);
            else
                TimescaleManager.instance.HaltTime(haltTimeScale, punchSlowDuration);
        }

        if (ShakeManager.instance)
        {
            if (!charged)
                ShakeManager.instance.ShakeCamera(new Shake()
                {
                    shakeAmplitude = punchShakeAmplitude,
                    shakeDuration = punchShakeDuration,
                    shakeFrequency = punchShakeFrequency
                });
            else
                ShakeManager.instance.ShakeCamera(new Shake()
                {
                    shakeAmplitude = punchShakeAmplitude,
                    shakeDuration = punchShakeDuration,
                    shakeFrequency = punchShakeFrequency
                });
        }
        #endregion
    }
    public void onImpact(GameObject hitObject)
    {
        #region damage logic
        healthSystem targetHealth = hitObject.GetComponent<healthSystem>();

        if(targetHealth)
        {
            targetHealth.TakeDamage(charged ? chargedAttackDamage : normalAttackDamage);
        }
        #endregion

        #region knockback logic
        // Get rigidbody, calculate knockback and then apply it
        Rigidbody2D rigidbody = targetHealth.rb;

        if (rigidbody)
        {
            Vector3 addedVelocity = rb.velocity * velocityRetentionPercent;
            addedVelocity +=
                (inputDir * (charged ? cAttackKb : attackKb))
            + (Vector3.up * upwardskb);
            rigidbody.AddForce(addedVelocity, ForceMode2D.Impulse);
        }
        #endregion
    }
    void BounceUp()
    {
        Vector3 velocity = rb.velocity;
        velocity.y = DownBounceKb + (charged ? cBackwardsKb : backwardsKb);
        rb.velocity = velocity;
    }
    public void ResetAttack()
    {
        charged = false;
        hit = false;
        if(chargingInitParticles) chargingInitParticles.Stop();
    }

    // value get
    Vector3 getCheckOffsetWithScale()
    {
        Vector3 checkOffsetWithScale = checkOffset;
        checkOffsetWithScale.x *= inputDir.x;
        checkOffsetWithScale += Vector3.up * inputDir.y * checkOffset.x;
        return checkOffsetWithScale;
    }
    float getDistanceFromTransform(Vector3 position)
    {
        return Vector3.Distance(position,transform.position) / checkRadius;
    }

}
