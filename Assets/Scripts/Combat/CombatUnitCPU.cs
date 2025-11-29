using UnityEngine;

/// <summary>
/// A combat unit controlled by the CPU
/// </summary>
internal class CombatUnitCPU : CombatUnit
{
    public CombatUnitCPU(string name, UnitDefinition unitDefinition, int instanceId)
        : base(name, unitDefinition, instanceId)
    {

    }

    public override async Awaitable DoTurnAsync(CombatSystem combat, CombatGUI gui)
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

