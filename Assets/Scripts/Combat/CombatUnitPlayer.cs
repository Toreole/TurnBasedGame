using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

/// <summary>
/// a combat unit controlled by the player.
/// </summary>
public class CombatUnitPlayer : CombatUnit
{
    public CombatUnitPlayer(string name, UnitDefinition unitDefinition, int instanceId) 
        : base(name, unitDefinition, instanceId)
    {

    }

    public override async Awaitable DoTurnAsync(CombatSystem combat, CombatGUI gui)
    {
        var actions = new string[] { "Attack", "Ability", "Do nothing" };
        var selectedIndex = await gui.SelectActionAsync(actions);
        if (selectedIndex == 0) // "attack 
        {
            await Awaitable.NextFrameAsync(); // to avoid one input counting for both selections.
            var target = await combat.SelectEnemyUnitAsync(this);
            var damage = GetAttackDamage();
            target.Damage(damage);
            await gui.ShowDismissableTextAsync($"{Name} hit {target.Name} for {damage:0.0} damage");
        } 
        else if (selectedIndex == 1) // ability
        {
            var abilityIndex = await gui.SelectDescriptiveAsync(UnitDefinition.Abilities);
            await Awaitable.NextFrameAsync();
            var ability = UnitDefinition.Abilities[abilityIndex];

            IReadOnlyList<CombatUnit> targets;
            switch (ability.Targetting)
            {
                case Targetting.Enemies:
                    targets = await combat.SelectEnemiesAsync(this, ability.TargetCount);
                    break;
                case Targetting.Allies:
                    targets = await combat.SelectAlliesAsync(this, ability.TargetCount);
                    break;
                case Targetting.Self or _:
                    targets = Utility.ListOf(this);
                    break;
            }
            await gui.ShowDismissableTextAsync($"{Name} used {ability.Name}");

            var amount = UnityEngine.Random.Range(ability.MinAmount, ability.MaxAmount);
            foreach (var target in targets)
                if (ability.EffectType == EffectType.Damage)
                {
                    target.Damage(amount);
                    await gui.ShowDismissableTextAsync($"{Name} hit {target.Name} for {amount:0} damage");
                }
                else if (ability.EffectType == EffectType.Healing)
                {
                    target.Damage(-amount);
                    await gui.ShowDismissableTextAsync($"{Name} healed {target.Name} for {amount:0} HP");
                }
            await Awaitable.NextFrameAsync();

        }
        else
        {
            await gui.ShowDismissableTextAsync($"{Name} decided to do nothing.");
        }
    }
}

