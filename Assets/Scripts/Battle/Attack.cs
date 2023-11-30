using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attack")]
public class Attack : ScriptableObject
{
    public string description;
    public int tokens;
    public float damage, charge;
    public Sprite buttonSprite;

    public enum TargetMode
    {
        Enemies,
        Heroes,
        Self,
        Other
    }
    public TargetMode targetMode;

    public enum DamageMode
    {
        Damage,
        Heal,
        Other,
        Delayed,
        Flash,
        Burn,
        StatDamage
    }
    public DamageMode damageMode;

    public enum SetStatus
    {
        None,
        Burning,
        Evasion,
        Regen,
        Vulnerable,
        Reset,
        Guard
    }
    public SetStatus setStatus;
}
