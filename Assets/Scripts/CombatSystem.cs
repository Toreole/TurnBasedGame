using System.Collections.Generic;
using System.Linq;
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

        // combat test.
        _combatOrder = new();
        _combatOrder.AddRange(_allyUnits);
        _combatOrder.AddRange(_enemyUnits);
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
        var enemies = _allyUnits.Contains(sourceUnit) ? _enemyUnits : _allyUnits;
        return await SelectUnitAsync(enemies);
    }
    public async Awaitable<CombatUnit> SelectAllyUnitAsync(CombatUnit sourceUnit)
    {
        var allies = _allyUnits.Contains(sourceUnit) ? _allyUnits : _enemyUnits;
        return await SelectUnitAsync(allies);
    }

    private async Awaitable<CombatUnit> SelectUnitAsync(IList<CombatUnit> units)
    {
        var selectionIndex = await _combatGUI.SelectActionAsync(units.Select(x => x.UnitName).ToArray());
        return units[selectionIndex];
    }
}
