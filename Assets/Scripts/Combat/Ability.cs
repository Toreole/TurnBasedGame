using System;
using UnityEngine;

namespace Toreole.Turnbased.Combat
{
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

        // it would be really awesome to have a scripting language
        // to define the *behaviour* of effects.
        // so you can change behaviour on the fly without having to recompile and all that stuff.
        // i kind of want to do debuffs and stuff too, which are going to have to get processed each turn. (CombatUnit->DoTurnAsync)

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

}