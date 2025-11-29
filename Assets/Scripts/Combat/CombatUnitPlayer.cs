using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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
}

