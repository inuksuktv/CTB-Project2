using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Attack", menuName = "Attack")]
public class Attack : ScriptableObject
{
    public string attackName, description;
    public int fireTokens, waterTokens, earthTokens, skyTokens;
    public float attackDamage, stateCharge;
    public Sprite buttonSprite;
}
