using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// abstract UnitLayout. 
/// It is entirely up to the implementing class to determine how
/// and where the prefabs are layed out.
/// </summary>
public abstract class UnitLayout : MonoBehaviour
{
    protected static readonly Vector2 FarFarAway = new Vector2(10000, 10000);

    public abstract void Add(CombatUnit unit);
    public abstract void AddAll(IEnumerable<CombatUnit> units);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="unit"></param>
    public abstract void Remove(CombatUnit unit);
    public abstract void RemoveAll(IEnumerable<CombatUnit> units);
    public abstract void Clear();

    public abstract void TransferUnit(CombatUnit unit, UnitLayout other);
    internal protected abstract void AcceptUnit(CombatUnit unit, UnitLayout source);
}

