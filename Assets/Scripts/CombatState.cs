using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Internal combat state for the combat system
/// </summary>
internal class CombatState
{
    public CombatStatus Status
    {
        get
        {
            if (_allyUnits.Count == 0)
                return CombatStatus.PlayerLose;
            else if (_enemyUnits.Count == 0)
                return CombatStatus.PlayerWin;
            else if (_combatOrder.Count > 0)
                return CombatStatus.InProgress;
            return CombatStatus.Error;
        }
    }

    // ObservableList?
    private List<CombatUnit> _combatOrder;
    private int _currentUnitIndex = 0;
    private int _currentTurn = 0;

    private List<CombatUnit> _allyUnits;
    private List<CombatUnit> _enemyUnits;

    private CombatSystem _combatSystem;

    // for avoiding name conflicts. // combines three dictionaries into one
    // private Dictionary<string, InstanceCounter> _unitsByName;

    public CombatState(CombatSystem context)
    {
        _combatSystem = context;
    }

    public void Init(List<UnitDefinition> allyUnits, EncounterDefinition encounter)
    {
        _allyUnits = allyUnits.Select(it => new CombatUnit(it.UnitName, it)).ToList();

        var enemyDefs = encounter.Enemies;
        // i feel like theres something better you can do here, but it will do.
        var countByName = new Dictionary<string, int>();
        var instancesByName = new Dictionary<string, int>();
        foreach (var ed in enemyDefs)
        {
            if (countByName.ContainsKey(ed.UnitName))
                countByName[ed.UnitName]++;
            else
            {
                countByName[ed.UnitName] = 1;
                instancesByName[ed.UnitName] = 0;
            }
        }
        var iter = 0;
        foreach (var ed in enemyDefs)
        {
            var unitName = ed.UnitName;
            if (countByName[ed.UnitName] > 1)
            {
                char discriminator = (char)('A' + instancesByName[ed.UnitName]++);
                unitName = $"{unitName} {discriminator}";
            }
            _enemyUnits.Add(new CombatUnit(unitName, ed));
            // TODO: instantiate prefabs via the CombatSystem.
            // how would we manage the prefabs as a whole?
            // Instantiate(ed.Prefab, _enemyArea).transform.localPosition = _unitSpacing * iter;
            iter++;
        }
    }

    public bool AddUnit(UnitDefinition unit, bool asAlly)
    {
        return AddUnit(
            new CombatUnit(unit.UnitName, unit), 
            asAlly ? _allyUnits : _enemyUnits
        );
    }

    public bool AddUnit(UnitDefinition unit, CombatUnit owner)
    {
        return AddUnit(
            new CombatUnit(unit.UnitName, unit), 
            _allyUnits.Contains(owner) ? _allyUnits : _enemyUnits
        );
    }

    private bool AddUnit(CombatUnit unit, List<CombatUnit> unitSet)
    {
        unit.OnUnitDied += OnUnitDeath;

        // Create Instance from Prefab.

        return true;
    }

    private void OnUnitDeath(CombatUnit unit)
    {
        RemoveUnitFromCombat(unit);
    }

    public bool RemoveUnitFromCombat(CombatUnit unit)
    {
        var index = _combatOrder.IndexOf(unit);
        if (index == -1) return false;
        _combatOrder.RemoveAt(index);
        if (index <= _currentUnitIndex)
            _currentUnitIndex--;
        // remove from order and from sets
        index = _allyUnits.IndexOf(unit);
        if (index != -1)
        {
            // Destroy(_allyArea.GetChild(index).gameObject);
            _allyUnits.RemoveAt(index);
            return true;
        }
        index = _enemyUnits.IndexOf(unit);
        if (index != -1)
        {
            // Destroy(_enemyArea.GetChild(index).gameObject);
            _enemyUnits.RemoveAt(index);
            return true;
        }
        return false;
    }

    public CombatUnit GetNextTurn()
    {
        if (_combatOrder.Count == 0)
            return null;

        _currentUnitIndex = (_currentUnitIndex + 1) % _combatOrder.Count;
        if (_currentUnitIndex == 0)
        {
            // we can trigger events on turn end / begin
            _currentTurn++;
        }
        return _combatOrder[_currentUnitIndex];
    }

    //private class InstanceCounter
    //{
    //    public List<CombatUnit> units;
    //    public int count;
    //    public int instanceCount;
    //}
}
public enum CombatStatus
{
    Error, InProgress, PlayerWin, PlayerLose
}

