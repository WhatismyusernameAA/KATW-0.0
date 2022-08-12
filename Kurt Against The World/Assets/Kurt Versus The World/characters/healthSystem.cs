using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class healthSystem : MonoBehaviour
{
    [Header("Attack Dependencies")]
    public Rigidbody2D rb;

    [Header("Health Settings")]
    public int maxHitPoints;
    public int currHitPoints;
    public bool dead;
    void Awake()
    {
        currHitPoints = maxHitPoints;
    }

    private void FixedUpdate()
    {
        
    }

    public void TakeDamage(int damagePoints)
    {
        if (dead) return;

        currHitPoints -= damagePoints;
        if(currHitPoints <= 0)
        {
            if (onKnockout != null) onKnockout();
            dead = true;
            return;
        }

        if (onTakeDamage != null) onTakeDamage();
    }

    public event Action onTakeDamage;
    public event Action onKnockout;

}
