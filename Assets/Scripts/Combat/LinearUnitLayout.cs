using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;

public class LinearUnitLayout : UnitLayout
{
    [SerializeField, Tooltip("Leave empty to use self")]
    private Transform _parent;

    [SerializeField]
    private float _spacing;

    [SerializeField]
    private Vector2 _direction;

    private readonly List<CombatUnitInstance> _managedUnits = new();

    private void Start()
    {
        _parent = _parent != null ? _parent : transform;
    }

    public override void Add(CombatUnit unit)
    {
        var instance = unit.PrefabInstance;
        Assert.IsNotNull(instance);
        var offset = _direction * (_spacing * _managedUnits.Count);
        _managedUnits.Add(instance);
        // Debug.Log("set instance parent");
        instance.transform.parent = _parent;
        instance.transform.localPosition = offset;
    }

    public override void AddAll(IEnumerable<CombatUnit> units)
    {
        foreach(var unit in units) 
            Add(unit);
    }

    public override void Clear()
    {
        foreach(var unit in _managedUnits)
        {
            unit.transform.parent = null;
            unit.transform.position = FarFarAway;
        }
        _managedUnits.Clear();
    }

    public override void Remove(CombatUnit unit)
    {
        var instance = unit.PrefabInstance;
        Assert.IsTrue(instance != null && _managedUnits.Contains(instance));
        var removeIndex = _managedUnits.IndexOf(instance);

        if (removeIndex < _managedUnits.Count - 1)
        {
            // re-layout units that come after.
        }
        _managedUnits.Remove(instance);
        instance.transform.parent = null;
        instance.transform.position = FarFarAway;
    }

    public override void RemoveAll(IEnumerable<CombatUnit> units)
    {
        foreach (var unit in units)
            Remove(unit);
    }

    public override void TransferUnit(CombatUnit unit, UnitLayout other)
    {
        var instance = unit.PrefabInstance;
        Assert.IsTrue(instance != null && _managedUnits.Contains(instance));
        var removeIndex = _managedUnits.IndexOf(instance);

        if (removeIndex < _managedUnits.Count - 1)
        {
            // re-layout units that come after.
        }

        _managedUnits.RemoveAt(removeIndex);

        instance.transform.parent = null;
        other.AcceptUnit(unit, this);
    }

    internal protected override void AcceptUnit(CombatUnit unit, UnitLayout source)
    {
        // keeping it simple for now.
        Add(unit);
    }
}
