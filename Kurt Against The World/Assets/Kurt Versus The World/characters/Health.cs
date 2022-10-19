using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Health : MonoBehaviour
{
    [Header("Dependencies")]
    public Rigidbody2D rb;
    public SpriteRenderer viewmodel;

    [Header("Settings")]
    public int maxHealth;
    int currHealth;

    [Space]
    public bool invulnerable;
    public int defense;

    private void Start()
    {
        currHealth = maxHealth;
    }

    public event Action onDamage;
    public void AddDamage(int damage) {
        currHealth -= Mathf.Clamp(damage - defense, 0, 10000);
        onDamage();
    }

    public event Action onStun;
    public void Stun(float stunDuration) {
        onStun();
    }

    public event Action onDeath;
}
