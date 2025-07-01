using System.Collections.Generic;
using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [SerializeField]
    private CombatGUI _combatGUI;

    // This should be a temporary measure for testing some basic functionality.
    [SerializeField]
    private List<CombatUnit> _allyUnits = new();
    [SerializeField]
    private List<CombatUnit> _enemyUnits = new();
     
    // this is all very crude.
    [SerializeField]
    private GameObject _allyPrefab;
    [SerializeField]
    private GameObject _enemyPrefab;
    [SerializeField]
    private Transform _allyArea;
    [SerializeField]
    private Transform _enemyArea;
    [SerializeField]
    private Vector2 _unitSpacing;

    private List<CombatUnit> _combatOrder;

    // for testing stuff.
    private void Start()
    {
        TestStuffAsync();
    }

    private async void TestStuffAsync()
    {
        var options = new string[] { "hello", "world", "this", "is", "a", "test", "lmao" };
        var selectedIndex = await _combatGUI.SelectActionAsync(options);
        await _combatGUI.ShowDismissableTextAsync($"Selected {options[selectedIndex]}!");
        await Awaitable.NextFrameAsync();

        var advOptions = new TempTest[] 
        { 
            new("Hello", "This is a thing now. Yeah wahoo."),
            new("Two", "Two"),
            new("Three", "Three"),
            new("Four", "Four"),
            new("Five", "5"),
            new("6", "6"),
            new("7", "7"),
            new("8", "8"),
            new("9", "9"),
            new("10", "10"),
            new("11", "11"),
            new("12", "12"),
            new("13", "13"),
            new("Goodbye", "This is other text instead of whatever was here before...") 
        };
        selectedIndex = await _combatGUI.SelectDescriptiveAsync(advOptions);
        await _combatGUI.ShowDismissableTextAsync($"Selected {advOptions[selectedIndex].Name}");
        await Awaitable.NextFrameAsync();
        //again with fewer options
        var advOptions2 = new TempTest[]
        {
            new("Hello", "This is a thing now. Yeah wahoo."),
            new("Two", "Two"),
            new("Goodbye", "This is other text instead of whatever was here before...")
        };
        selectedIndex = await _combatGUI.SelectDescriptiveAsync(advOptions2);
        await _combatGUI.ShowDismissableTextAsync($"Selected {advOptions2[selectedIndex].Name}");
        await Awaitable.NextFrameAsync();

        // combat test.
        _combatOrder = new();
        _combatOrder.AddRange(_allyUnits);
        _combatOrder.AddRange(_enemyUnits);
        for (int i = 0; i < _allyUnits.Count; i++)
            Instantiate(_allyPrefab, _allyArea).transform.localPosition = _unitSpacing * i;
        for (int i = 0; i < _allyUnits.Count; i++)
            Instantiate(_enemyPrefab, _enemyArea).transform.localPosition = _unitSpacing * i;
        // randomize order for now.
        for (int i = 0; i < _combatOrder.Count * 2; i++)
        {
            var a = Random.Range(0, _combatOrder.Count);
            var b = Random.Range(0, _combatOrder.Count);
            (_combatOrder[a], _combatOrder[b]) = (_combatOrder[b], _combatOrder[a]);
        }
        // now do each units turn after each other.
        while (_combatOrder.Count > 0) // while combat is still active
        {
            // TODO: this would not allow units to be taken out of combat
            for (int i = 0; i < _combatOrder.Count; i++)
            {
                await _combatOrder[i].DoTurnAsync(this, _combatGUI);
                // always also wait for at least one additional frame between turns to avoid key inputs mixing into both.
                await Awaitable.NextFrameAsync();
            }
        }
    }

    // NONE OF THESE ARE SAFE

    public List<CombatUnit> GetEnemies(CombatUnit unit)
    {
        return (_allyUnits.Contains(unit) ? _enemyUnits : _allyUnits);
    }
    public List<CombatUnit> GetAllies(CombatUnit unit)
    {
        return (_allyUnits.Contains(unit) ? _allyUnits : _enemyUnits);
    }

    public async Awaitable<CombatUnit> SelectEnemyUnitAsync(CombatUnit sourceUnit)
    {
        var unitIsAlly = _allyUnits.Contains(sourceUnit);
        var enemies = unitIsAlly ? _enemyUnits : _allyUnits;
        var parent = unitIsAlly ? _enemyArea : _allyArea;
        return await SelectUnitAsync(enemies, parent);
    }
    public async Awaitable<CombatUnit> SelectAllyUnitAsync(CombatUnit sourceUnit)
    {
        var unitIsAlly = _allyUnits.Contains(sourceUnit);
        var allies = unitIsAlly ? _allyUnits : _enemyUnits;
        var parent = unitIsAlly ? _allyArea : _allyArea;
        return await SelectUnitAsync(allies, parent);
    }

    private async Awaitable<CombatUnit> SelectUnitAsync(IList<CombatUnit> units, Transform instanceParent)
    {
        // var selectionIndex = await _combatGUI.SelectActionAsync(units.Select(x => x.UnitName).ToArray());
        // return units[selectionIndex];

        // in this scenario, an instanced prefab exists for each combat unit.
        var selectedIndex = 0;
        instanceParent.GetChild(selectedIndex).GetChild(0).gameObject.SetActive(true);
        while (Input.GetKeyDown(KeyCode.Return) == false)
        {
            var oldSelection = selectedIndex;

            if (Input.GetKeyDown(KeyCode.RightArrow))
                selectedIndex++;
            if (Input.GetKeyDown(KeyCode.LeftArrow))
                selectedIndex--;
            if (selectedIndex < 0 || selectedIndex >= units.Count)
            {
                selectedIndex = oldSelection;
            } 
            else
            {
                instanceParent.GetChild(oldSelection).GetChild(0).gameObject.SetActive(false);
                instanceParent.GetChild(selectedIndex).GetChild(0).gameObject.SetActive(true);
            }
            await Awaitable.NextFrameAsync();
        }
        instanceParent.GetChild(selectedIndex).GetChild(0).gameObject.SetActive(false);
        return units[selectedIndex];
    }

    class TempTest : INameAndDescription
    {
        public string Name { get; }
        public string Description { get; }
        public TempTest(string n, string d)
        {
            Name = n; Description = d;
        }
    }
}
