using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Internal combat state for the combat system
/// </summary>
// this may or may not need a lot of events for GUI updates that 
// display aspects of the combat state (i.e. turn order)
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
    private readonly List<CombatUnit> _combatOrder = new();
    private int _currentUnitIndex = 0;
    private int _currentTurn = 0;

    private int _nextId = 1;

    private readonly List<CombatUnit> _allyUnits = new();
    private readonly List<CombatUnit> _enemyUnits = new();

    private readonly List<CombatUnit> _deadUnits = new();

    private readonly CombatSystem _combatSystem;
    // encounters may have special triggers attached to them.
    private EncounterDefinition _encounterReference; 

    // for avoiding name conflicts. // combines three dictionaries into one
    // private Dictionary<string, InstanceCounter> _unitsByName;

    public CombatState(CombatSystem context)
    {
        _combatSystem = context;
    }

    public void Init(List<UnitDefinition> allyUnits, EncounterDefinition encounter)
    {
        _encounterReference = encounter;
        foreach (var unit in allyUnits)
        {
            AddUnit(unit, true);
        }
        
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
            var newUnit = ed.InstantiateUnit(_nextId++, instancesByName[ed.UnitName]++);
            AddUnitToSet(newUnit, _enemyUnits);
            
            // TODO: instantiate prefabs via the CombatSystem.
            // how would we manage the prefabs as a whole?
            // Instantiate(ed.Prefab, _enemyArea).transform.localPosition = _unitSpacing * iter;
            iter++;
        }
        _combatSystem.InstantiateUnits(_allyUnits, true);
        _combatSystem.InstantiateUnits(_enemyUnits, false);

        _combatOrder.AddRange(_allyUnits);
        _combatOrder.AddRange(_enemyUnits);
    }

    public bool AddUnit(UnitDefinition unit, bool asAlly)
    {
        return AddUnitToSet(
            unit.InstantiateUnit(_nextId++, 0), 
            asAlly ? _allyUnits : _enemyUnits
        );
    }

    private bool AddUnitToSet(CombatUnit unit, List<CombatUnit> unitSet)
    {
        unit.OnUnitDied += OnUnitDeath;
        unitSet.Add(unit);
        // Create Instance from Prefab.

        return true;
    }

    private void OnUnitDeath(CombatUnit unit)
    {
        _deadUnits.Add(unit);
        _combatSystem.RemoveUnitInstance(unit, _allyUnits.Contains(unit));
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


    internal void ShuffleCombatOrder(int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            var a = Random.Range(0, _combatOrder.Count);
            var b = Random.Range(0, _combatOrder.Count);
            (_combatOrder[a], _combatOrder[b]) = (_combatOrder[b], _combatOrder[a]);
        }
    }

    // returns a
    public IReadOnlyList<CombatUnit> GetEnemies(CombatUnit unit)
    {
        return (_allyUnits.Contains(unit) ? _enemyUnits : _allyUnits).ToList();
    }
    public IReadOnlyList<CombatUnit> GetAllies(CombatUnit unit)
    {
        return (_allyUnits.Contains(unit) ? _allyUnits : _enemyUnits).ToList();
    }

    public bool IsAlly(CombatUnit unit)
    {
        return _allyUnits.Contains(unit);
    }

    internal void Cleanup()
    {
        Debug.Log($"CombatState.Cleanup(): {_allyUnits.Count}, {_enemyUnits.Count}, {_deadUnits.Count} ");
        foreach (var unit in _allyUnits.Concat(_enemyUnits).Concat(_deadUnits))
        {
            if (unit.PrefabInstance != null)
            {
                Object.Destroy(unit.PrefabInstance.gameObject);
                unit.PrefabInstance = null;
            }
        }
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
