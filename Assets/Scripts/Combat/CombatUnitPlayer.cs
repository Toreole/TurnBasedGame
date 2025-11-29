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
        bool actionHasTakenPlace = false;
        while (!actionHasTakenPlace)
        {
            var actions = new string[] { "Attack", "Ability", "Do nothing" };
            var selectedIndex = await gui.SelectActionAsync(actions);
            // TODO: you should be able to cancel actions with ESCAPE (or otherwise) BEFORE anything really happens.
            // so: before target selection, before action selection. In layers, go up one layer. (How can we do this reliably?)
            if (selectedIndex == 0) // "attack 
            {
                await Awaitable.NextFrameAsync(); // to avoid one input counting for both selections.
                // TODO: allow combat.SelectEnemyUnitAsync to return null. check if it is null, then continue to next iteration of while loop
                var target = await combat.SelectEnemyUnitAsync(this);
                var damage = GetAttackDamage();
                target.Damage(damage);
                actionHasTakenPlace = true;
                await gui.ShowDismissableTextAsync($"{Name} hit {target.Name} for {damage:0.0} damage");
            }
            else if (selectedIndex == 1) // ability
            {
                var abilityIndex = await gui.SelectDescriptiveAsync(UnitDefinition.Abilities);
                await Awaitable.NextFrameAsync();
                var ability = UnitDefinition.Abilities[abilityIndex];

                IReadOnlyList<CombatUnit> targets;
                // TODO: Same thing here; allow combat.Select(Enemies/Allies)Async to return null.
                // check if it is null, then continue to next iteration of while loop
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
                actionHasTakenPlace = true;
                await Awaitable.NextFrameAsync();

            }
            else if (selectedIndex == 2) // do nothing
            {
                actionHasTakenPlace = true;
                await gui.ShowDismissableTextAsync($"{Name} decided to do nothing.");
            }
        }
        
    } // DoTurnAsync
}

