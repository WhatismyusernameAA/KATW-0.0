using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : ScriptableObject
{
    [Header("Outside Appearance")]
    public string abilityName;
    public Sprite itemSprite;

    [Header("Execution")]
    public float cooldownTime;
    public float energyConsumption;

    public virtual void Activate() { }
}
