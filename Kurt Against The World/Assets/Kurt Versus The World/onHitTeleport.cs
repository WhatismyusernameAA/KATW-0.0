using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class onHitTeleport : MonoBehaviour
{
    public GameObject teleport;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        collision.transform.position = teleport.transform.position;
    }
}
