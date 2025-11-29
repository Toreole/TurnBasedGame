using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

// temporary measure
using Random = UnityEngine.Random;

public class CombatSystem : ProviderBehaviour
{
    [Injected(isFatal: true)]
    private readonly CombatGUI _combatGUI;

    // This should be a temporary measure for testing some basic functionality.
    [SerializeField]
    private List<UnitDefinition> _debugAllies;

    [SerializeField]
    private Transform _combatArea;
    // this is all very crude.
    [SerializeField]
    private UnitLayout _allyLayout;
    [SerializeField]
    private UnitLayout _enemyLayout;

    [SerializeField]
    private ScreenTransition _screenTransitioner;
    [SerializeField]
    private float _screenTransitionDuration = 2f;

    private EncounterDefinition _currentEncounter;
    private CombatState _combatState;

    private Vector3 _oldCameraPosition;

    protected override bool Register()
    {
        return DependencyService.Register(this);
    }
    protected override void Unregister()
    {
        DependencyService.Unregister(this);
    }

    public void Engage(EncounterDefinition encounter)
    {
        _currentEncounter = encounter;
        SetupCombatAsync();
    }

    private async void SetupCombatAsync()
    {
        var player = DependencyService.RequestDependency<PlayerMovement>();
        player.enabled = false;
        // transitions
        await _screenTransitioner.TriggerAsync(SetupCamera);
        await DoCombatAsync();
    }

    private void SetupCamera()
    {
        _oldCameraPosition = Camera.main.transform.position;
        var pos = _combatArea.position;
        pos.z = -100;
        Camera.main.transform.position = pos;
    }

    private async Awaitable DoCombatAsync()
    {
        // bla bla setup.
        //_allyArea.gameObject.SetActive(true);
        //_enemyArea.gameObject.SetActive(true);
        _combatGUI.Activate();
        _combatArea.gameObject.SetActive(true);

        await _combatGUI.ShowDismissableTextAsync("You've been attacked!");

        // now do each units turn after each other.
        await CombatLoopAsync();
        // rewards and stuff would be given out in here i suppose.
        EndCombatAsync();
    }

    private async Awaitable CombatLoopAsync()
    {
        _combatState = new(this);
        _combatState.Init(_debugAllies, _currentEncounter);
        _combatState.ShuffleCombatOrder(10);
        Assert.IsNotNull(_combatState);
        while (_combatState.Status == CombatStatus.InProgress)
        {
            var unit = _combatState.GetNextTurn() 
                ?? throw new NullReferenceException("Expected unit");
            unit.PrefabInstance.SetHighlight(true);
            await unit.DoTurnAsync(this, _combatGUI);
            unit.PrefabInstance.SetHighlight(false);
        }
        switch (_combatState.Status)
        {
            case CombatStatus.PlayerWin:
                await _combatGUI.ShowDismissableTextAsync("yippie you win");
                break;
            case CombatStatus.PlayerLose:
                Debug.Log("Deadge");
                break;
            default:
                Debug.LogError("yikes");
                break;
        }
        Assert.IsNotNull(_combatState);
        await EndCombatAsync();
    }

    // TODO: persist health, etc. of player and persistent allies.
    private async Awaitable EndCombatAsync()
    {
        _allyLayout.Clear();
        _enemyLayout.Clear();
        Assert.IsNotNull(_combatState);
        _combatState.Cleanup();
        _combatState = null;

        await _combatGUI.ShowDismissableTextAsync("You win!");
        _combatGUI.Deactivate();
        await Awaitable.NextFrameAsync();
        Camera.main.transform.position = _oldCameraPosition;

        var player = DependencyService.RequestDependency<PlayerMovement>();
        player.enabled = true;
    }

    internal void InstantiateUnit(CombatUnit unit, bool isAlly)
    {
        Assert.IsNotNull(unit);
        Assert.IsNotNull(unit.UnitDefinition);
        var instance = Instantiate(unit.UnitDefinition.Prefab)
            .GetComponent<CombatUnitInstance>();
        unit.PrefabInstance = instance;

        var layout = isAlly ? _allyLayout : _enemyLayout;
        layout.Add(unit);
    }

    internal void InstantiateUnits(IEnumerable<CombatUnit> units, bool asAlly)
    {
        // TODO use layout.AddAll() instead.
        foreach (var unit in units) 
            InstantiateUnit(unit, asAlly);
    }

    internal void RemoveUnitInstance(CombatUnit unit, bool wasAlly)
    {
        var layout = wasAlly ? _allyLayout : _enemyLayout;
        layout.Remove(unit);
    }

    // Methods for CombatUnit.DoTurnAsync() to work

    public IReadOnlyList<CombatUnit> GetEnemies(CombatUnit unit)
    {
        Assert.IsNotNull(_combatState);
        return _combatState.GetEnemies(unit);
    }
    public IReadOnlyList<CombatUnit> GetAllies(CombatUnit unit)
    {
        Assert.IsNotNull(_combatState);
        return _combatState.GetAllies(unit);
    }

    public async Awaitable<CombatUnit> SelectEnemyUnitAsync(CombatUnit sourceUnit)
    {
        Assert.IsNotNull(_combatState);
        var enemies = _combatState.GetEnemies(sourceUnit);
        return await SelectUnitAsync(enemies);
    }
    public async Awaitable<CombatUnit> SelectAllyUnitAsync(CombatUnit sourceUnit)
    {
        var allies = _combatState.GetAllies(sourceUnit);
        return await SelectUnitAsync(allies);
    }

    // == SelectUnitsAsync(units, 1)
    private async Awaitable<CombatUnit> SelectUnitAsync(IReadOnlyList<CombatUnit> units)
    {
        // in this scenario, an instanced prefab exists for each combat unit.
        var selectedIndex = 0;
        // TODO: this whole hard coded instance get child gameobject bla SUCKS.
        units[selectedIndex].PrefabInstance.SetSelected(true);
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
                units[oldSelection].PrefabInstance.SetSelected(false);
                units[selectedIndex].PrefabInstance.SetSelected(true);
            }
            await Awaitable.NextFrameAsync();
        }
        units[selectedIndex].PrefabInstance.SetSelected(false);
        return units[selectedIndex];
    }

    // NOTE: allow "circular" selections? i.e. wrap from right side back to left?
    private async Awaitable<IReadOnlyList<CombatUnit>> SelectUnitsAsync(IReadOnlyList<CombatUnit> units, int maxCount)
    {
        var selectedIndex = 0;
        var maxIndex = Math.Max(0, units.Count - maxCount);

        for (int i = selectedIndex; i < units.Count && i < selectedIndex + maxCount; i++)
            units[i].PrefabInstance.SetSelected(true);

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
                if (oldSelection < selectedIndex) // selection shifts right
                {
                    units[oldSelection].PrefabInstance.SetSelected(false);
                    units[selectedIndex+maxCount].PrefabInstance.SetSelected(true);
                } 
                else // selection shifts left
                {
                    units[oldSelection].PrefabInstance.SetSelected(true);
                    units[selectedIndex + maxCount].PrefabInstance.SetSelected(false);
                }
            }
            await Awaitable.NextFrameAsync();
        }
        // disable selection
        for (int i = selectedIndex; i < units.Count && i < selectedIndex + maxCount; i++)
            units[i].PrefabInstance.SetSelected(false);

        // meh
        var resultList = units.Skip(selectedIndex).Take(maxCount).ToList();
        return resultList;
    }

    public override void Dispose()
    {
        throw new System.NotImplementedException();
    }
}
