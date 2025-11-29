using System;
using System.Collections.Generic;
using UnityEngine;

// this is supposed to represent units like the Player Character
// and permanent allies.
[CreateAssetMenu(fileName = "PersistentUnitDefinition", menuName = "GameData/PersistentUnitDefinition")]
public class PersistentUnitDefinition : UnitDefinition
{
    [NonSerialized] // otherwise unity fucks it up by assigning _instance = new()
    private CombatUnit _instance = null;

    internal override CombatUnit InstantiateUnit(int instanceId, int counter)
    {
        if (_instance == null)
            _instance = new CombatUnitPlayer(this.UnitName, this, instanceId);
        else // the unit needs to actually still receive the correct instanceId. not sure what i will use those IDs for yet, but hey.
            _instance.InstanceId = instanceId; 
        Debug.Log($"{UnitName} instantiated for combat. Has {_instance.CurrentHealth} health.");
        Debug.Log($"{UnitName}: {_instance.MaxHealth} max_health. expected {this.MaxHealth}");
        return _instance;
    }

    // for saving and loading unit state between sessions:
    // these probably have to be implemented via the _instance itself.
    // public PersistentUnitData SerializeUnit() {...}
    // public void InstantiateUnitFrom(PersistentUnitData data)
}
