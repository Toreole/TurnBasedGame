using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
///  
/// </summary>
[CreateAssetMenu(fileName = "A_Ability", menuName = "GameData/Ability")]
public class Ability : ScriptableObject, INameAndDescription
{
    [field: SerializeField]
    public string Name { get; private set; }
    [field: SerializeField]
    public string Description { get; private set; }

    [field: SerializeField]
    public Targetting Targetting { get; private set; }

    [field: SerializeField]
    public int TargetCount { get; private set; }

    // for the future:
    // [field: SerializeField]
    // public AbilityEffect Effect { get; private set; }

    // for now to get a baseline implementation

    // future: base amounts and scalings
    [field: SerializeField]
    public int MinAmount { get; private set; }
    [field: SerializeField]
    public int MaxAmount { get; private set; }

    [field: SerializeField]
    public EffectType EffectType { get; private set; }

    [field: SerializeField]
    public int AbilityCost { get; private set; }

}

[Flags]
public enum Targetting
{
    Enemies = 1,
    Allies = 2,
    Self = 4
}

public enum EffectType
{
    None,
    Damage,
    Healing
}