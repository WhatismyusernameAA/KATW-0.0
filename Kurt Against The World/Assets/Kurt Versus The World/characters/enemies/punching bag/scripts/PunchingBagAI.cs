using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PunchingBagAI : MonoBehaviour
{
    [Header("Dependencies")]
    public Rigidbody2D rb;
    public Transform viewModel;
    public Animator animator;

    [Space]
    public Health healthSystem;

    [Header("Settings")]
    bool grounded;
    public Vector2 groundCheckOffset;
    public Vector2 groundCheckDimentions;
    public LayerMask groundLayer;

    [Space]
    public float dragForce;
    public float airDragForce;

    int idleAnim = Animator.StringToHash("Idle");
    int hurtAnim = Animator.StringToHash("Hurt");
    int stunAnim = Animator.StringToHash("Stunned");

    private void Start()
    {
        // set up event trigger
        healthSystem.onStun += onStun;
        healthSystem.onDamage += onDamage;
    }

    private void FixedUpdate()
    {
        // casts box towards feet to check for ground
        Vector2 position = transform.position;
        grounded = Physics2D.OverlapBox(position - groundCheckOffset, groundCheckDimentions, 0, groundLayer);

        float dragAmount = Mathf.Min(Mathf.Abs(rb.velocity.x), Mathf.Abs(grounded ? dragForce : airDragForce));
        dragAmount *= Mathf.Sign(rb.velocity.x);

        rb.AddForce(Vector2.right * -dragAmount, ForceMode2D.Impulse);
    }

    void onStun()
    {
        StartCoroutine(stunCoroutine());
    }

    IEnumerator stunCoroutine()
    {
        animator.CrossFade(stunAnim, 0);
        yield return new WaitForSeconds(0.5f);
        animator.CrossFade(idleAnim, 0);
    }

    void onDamage()
    {
        animator.CrossFade(hurtAnim, 0);
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 position = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawCube(position - groundCheckOffset, groundCheckDimentions);
    }
}
