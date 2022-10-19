using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackManagerV2 : MonoBehaviour
{
    [Header("Dependencies")]
    public PlayerControllerV2 playerController;
    public Rigidbody2D rb;

    [Header("Settings")]
    public float attackMomentum;

    public void Fling()
    {
        //if (!playerController.grounded) return;

        //Vector2 lookDir = playerController.inputLookDirection;
        //lookDir.y = 0;
        //rb.AddForce(lookDir * attackMomentum, ForceMode2D.Impulse);
    }

    public void HitCheck()
    {

    }
}
