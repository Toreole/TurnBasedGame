using System;
using System.Collections.Generic;
using UnityEngine;

// this is supposed to represent units like the Player Character
// and permanent allies.
[CreateAssetMenu(fileName = "PersistentUnitDefinition", menuName = "GameData/PersistentUnitDefinition")]
public class PersistentUnitDefinition : UnitDefinition
{
    [NonSerialized]
    private CombatUnit _instance = null;

    internal override CombatUnit InstantiateUnit(int instanceId, int counter)
    {
        if (_instance != null)
        {
            Debug.Log($"instance of {UnitName} was not null??");
        }
        if (_instance == null)
            _instance = new CombatUnit(this.UnitName, this, instanceId);
        Debug.Log($"{UnitName} instantiated for combat. Has {_instance.CurrentHealth} health.");
        Debug.Log($"{UnitName}: {_instance.MaxHealth} max_health. expected {this.MaxHealth}");
        return _instance;
    }

}
