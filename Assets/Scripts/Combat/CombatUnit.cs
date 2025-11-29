using System;
using UnityEngine;

// temporary. in the long run, everything should have one deterministic source of randomness
// and for that even System.Random is preferable over this.
using Random = UnityEngine.Random;

// TODO:
// Computer controller Units need some parameters for how the unit should behave.
// stuff like preferred abilities, aggression, strategy / role etc, which affect what
// the unit will end up doing.
[Serializable]
public abstract class CombatUnit : INameAndDescription
{
    public CombatUnit()
    {

    }

    // all of these are taken from UnitDefinition is the idea.
    // about the name im not so sure.
    public CombatUnit(string unitName, UnitDefinition def, int instanceId)
    {
        _unitName = unitName;
        _maxHealth = def.MaxHealth;
        _maxMana = def.MaxMana;

        _minDamage = def.MinDamage;
        _maxDamage = def.MaxDamage;

        _currentHealth = _maxHealth;
        _currentMana = _maxMana;

        UnitDefinition = def;
        InstanceId = instanceId;
    }

    [SerializeField]
    private string _unitName;
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

    // TODO
    private float _currentHealth;
    private float _currentMana;

    public float CurrentHealth => _currentHealth;
    public float MaxHealth => _maxHealth;
    public string Name => _unitName;
    public string Description => "";

    public CombatUnitInstance PrefabInstance { get; internal set; }
    public UnitDefinition UnitDefinition { get; private set; }

    /// <summary>
    /// Starts at 1, counts up, unique to every Combat
    /// </summary>
    public int InstanceId { get; internal set; }

    internal event Action<CombatUnit> OnUnitDied;

    public float GetAttackDamage()
    {
        return Mathf.Lerp(_minDamage, _maxDamage, Random.value);
    }

    internal protected void Damage(float damage)
    {
        _currentHealth -= damage;
        if (_currentHealth < 0)
        {
            // OVERKILL hahahahaha

            // TODO: do something on death.
            OnUnitDied(this);
        }
    }

    // TODO: i feel like this should not be in here
    public abstract Awaitable DoTurnAsync(CombatSystem combat, CombatGUI gui);
}

