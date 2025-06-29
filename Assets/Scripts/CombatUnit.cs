using UnityEngine;

public class CombatUnit
{
    [SerializeField]
    private float _maxHealth;
    [SerializeField]
    private float _maxMana;

    // for basic attacks.
    // replace this with a weapon with those attributes.
    [SerializeField]
    private float _minDamage, _maxDamage;

    // special abilities / spells that do unique things.
    //[SerializeField]
    //private Ability[] m_abilities;

    public float GetAttackDamage()
    {
        return Mathf.Lerp(_minDamage, _maxDamage, Random.value);
    }
}

