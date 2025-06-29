using UnityEngine;

public class CombatUnit
{
    [SerializeField]
    private float m_maxHealth;
    [SerializeField]
    private float m_maxMana;

    // for basic attacks.
    // replace this with a weapon with those attributes.
    [SerializeField]
    private float minDamage, maxDamage;

    // special abilities / spells that do unique things.
    //[SerializeField]
    //private Ability[] m_abilities;

    public float GetAttackDamage()
    {
        return Mathf.Lerp(minDamage, maxDamage, Random.value);
    }
}

