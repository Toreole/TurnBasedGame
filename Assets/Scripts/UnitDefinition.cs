using UnityEngine;

[CreateAssetMenu(fileName = "UnitDefinition", menuName = "Scriptable Objects/UnitDefinition")]
public class UnitDefinition : ScriptableObject
{
    [field: SerializeField]
    public GameObject Prefab { get; }

    [field: SerializeField]
    public string UnitName { get; }
    [field: SerializeField]
    public float MaxHealth { get; }
    [field: SerializeField]
    public float MaxMana { get; }
    [field: SerializeField]
    public bool IsPlayerControlled { get; }

    // this is kinda silly atm
    // for basic attacks.
    // replace this with a weapon with those attributes.
    [field: SerializeField]
    public float MinDamage { get; }
    [field: SerializeField]
    public float MaxDamage { get; }
}
