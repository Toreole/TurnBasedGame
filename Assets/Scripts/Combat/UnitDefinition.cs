using UnityEngine;

[CreateAssetMenu(fileName = "UnitDefinition", menuName = "GameData/UnitDefinition")]
public class UnitDefinition : ScriptableObject
{
    [field: SerializeField]
    public GameObject Prefab { get; private set; }

    [field: SerializeField]
    public string UnitName { get; private set; }
    [field: SerializeField]
    public float MaxHealth { get; private set; }
    [field: SerializeField]
    public float MaxMana { get; private set; }
    [field: SerializeField]
    public bool IsPlayerControlled { get; private set; }

    // this is kinda silly atm
    // for basic attacks.
    // replace this with a weapon with those attributes.
    [field: SerializeField]
    public float MinDamage { get; private set; }
    [field: SerializeField]
    public float MaxDamage { get; private set; }

    internal virtual CombatUnit InstantiateUnit(int instanceId, int counter)
    {
        var name = $"{UnitName} {(char)('A' + counter)}";
        return new CombatUnit(name, this, instanceId);
    }

}
