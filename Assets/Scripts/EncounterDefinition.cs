using UnityEngine;

[CreateAssetMenu(fileName = "ED_EncounterDefinition", menuName = "GameData/EncounterDefinition")]
public class EncounterDefinition : ScriptableObject
{
    // should be something non-mutable but whatever lol
    [field: SerializeField]
    public UnitDefinition[] Enemies { get; private set; }

    // allies that are included for a specific encounter, but do not belong to the players party.
    [field: SerializeField]
    public UnitDefinition[] AdditionalAllies { get; private set; }

    // [field: SerializeField]
    // public Item[] Rewards or whatever

    // public string openingDiaogue;
    // public string closingDialogue;
}
