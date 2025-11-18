using System;
using UnityEngine;

// temporary. in the long run, everything should have one deterministic source of randomness
// and for that even System.Random is preferable over this.
using Random = UnityEngine.Random;

[Serializable]
public class CombatUnit : INameAndDescription
{
    public CombatUnit()
    {

    }
    // all of these are taken from UnitDefinition is the idea.
    // about the name im not so sure.
    public CombatUnit(string unitName, float maxHealth, float maxMana, bool isPlayerControlled, float minDamage, float maxDamage)
    {
        _unitName = unitName;
        _maxHealth = maxHealth;
        _maxMana = maxMana;
        _isPlayerControlled = isPlayerControlled;
        _minDamage = minDamage;
        _maxDamage = maxDamage;
    }

    [SerializeField]
    private string _unitName;
    [SerializeField]
    private float _maxHealth;
    [SerializeField]
    private float _maxMana;
    [SerializeField]
    private bool _isPlayerControlled;

    // for basic attacks.
    // replace this with a weapon with those attributes.
    [SerializeField]
    private float _minDamage, _maxDamage;

    // special abilities / spells that do unique things.
    //[SerializeField]
    //private Ability[] m_abilities;

    public string Name => _unitName;

    public string Description => "";

    // TODO
    private float _currentHealth;


    public float GetAttackDamage()
    {
        return Mathf.Lerp(_minDamage, _maxDamage, Random.value);
    }

    private void Damage(float damage)
    {
        // TODO
    }

    public async Awaitable DoTurnAsync(CombatSystem combat, CombatGUI gui)
    {
        if (_isPlayerControlled)
        {
            var actions = new string[] { "Attack", "Do nothing" };
            var selectedIndex = await gui.SelectActionAsync(actions);
            if (selectedIndex == 0) // "attack 
            {
                await Awaitable.NextFrameAsync(); // to avoid one input counting for both selections.
                var target = await combat.SelectEnemyUnitAsync(this);
                var damage = GetAttackDamage();
                target.Damage(damage);
                await gui.ShowDismissableTextAsync($"{Name} hit {target.Name} for {damage:0.0} damage");
            }
            else
            {
                await gui.ShowDismissableTextAsync($"{Name} decided to do nothing.");
            }
        }
        else
        {
            var action = Random.Range(0, 2);
            if (action == 0)
            {
                var enemies = combat.GetEnemies(this);
                var target = enemies[Random.Range(0, enemies.Count)];
                var damage = GetAttackDamage();
                target.Damage(damage);
                await gui.ShowDismissableTextAsync($"{Name} hit {target.Name} for {damage:0.0} damage");
            }
            else
            {
                await gui.ShowDismissableTextAsync($"{Name} decided to do nothing.");
            }
        }
    }
}

