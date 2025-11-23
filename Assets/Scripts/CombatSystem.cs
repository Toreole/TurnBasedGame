using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CombatSystem : ProviderBehaviour
{
    [Injected(isFatal: true)]
    private readonly CombatGUI _combatGUI;

    // This should be a temporary measure for testing some basic functionality.
    [SerializeField]
    private List<UnitDefinition> _debugAllies;

    [SerializeField]
    private GameObject _allyPrefab;
    [SerializeField]
    private Transform _combatArea;
    // this is all very crude.
    [SerializeField]
    private Transform _allyArea;
    [SerializeField]
    private Transform _enemyArea;
    [SerializeField]
    private Vector2 _unitSpacing;

    [SerializeField]
    private BlackScreenTransitioner _screenTransitioner;
    [SerializeField]
    private ScreenMeltTransition _screenTransitioner1;
    [SerializeField]
    private float _screenTransitionDuration = 2f;


    private List<CombatUnit> _allyUnits = new();
    private List<CombatUnit> _enemyUnits = new();
    private List<CombatUnit> _combatOrder;

    private Vector3 _oldCameraPosition;

    protected override bool Register()
    {
        return DependencyService.Register(this);
    }

    /// <summary>
    /// Sets up enemies for combat based on any Enumerable of UnitDefinitions.
    /// Arrays or Lists preferred for simplicity i guess.
    /// </summary>
    /// <param name="enemyDefs"></param>
    private void SetupEnemies(IEnumerable<UnitDefinition> enemyDefs)
    {
        _enemyArea.DestroyChildren();
        _enemyUnits.Clear();

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
            Instantiate(ed.Prefab, _enemyArea).transform.localPosition = _unitSpacing * iter;
            iter++;
        }
    }

    public void Engage(EncounterDefinition encounter)
    {
        SetupEnemies(encounter.Enemies);
        SetupCombatAsync();
    }

    private async void SetupCombatAsync()
    {
        // transitions
        _screenTransitioner1.Trigger();
        await Awaitable.NextFrameAsync();
        //_screenTransitioner.Trigger(_screenTransitionDuration);
        //await Awaitable.WaitForSecondsAsync(_screenTransitionDuration + 0.2f);
        //_screenTransitioner.End();

        // bla bla setup.
        //_allyArea.gameObject.SetActive(true);
        //_enemyArea.gameObject.SetActive(true);
        _combatGUI.Activate();
        _combatArea.gameObject.SetActive(true);

        _oldCameraPosition = Camera.main.transform.position;
        var pos = _combatArea.position;
        pos.z = -100;
        Camera.main.transform.position = pos;

        var player = DependencyService.RequestDependency<PlayerMovement>();
        player.enabled = false;

        DoCombatAsync();
    }

    private async void DoCombatAsync()
    {
        // TODO: let units die and remove them from combat.

        await _combatGUI.ShowDismissableTextAsync("You've been attacked!");

        // combat test.
        _allyUnits = _debugAllies.Select(x => new CombatUnit(x.UnitName, x)).ToList();
        _combatOrder = new();
        _combatOrder.AddRange(_allyUnits);
        _combatOrder.AddRange(_enemyUnits);

        _allyArea.DestroyChildren();
        for (int i = 0; i < _allyUnits.Count; i++)
            Instantiate(_allyPrefab, _allyArea).transform.localPosition = _unitSpacing * i;

        //for (int i = 0; i < _enemyUnits.Count; i++)
        //    Instantiate(_enemyPrefab, _enemyArea).transform.localPosition = _unitSpacing * i;
        // randomize order for now.
        for (int i = 0; i < _combatOrder.Count * 2; i++)
        {
            var a = Random.Range(0, _combatOrder.Count);
            var b = Random.Range(0, _combatOrder.Count);
            (_combatOrder[a], _combatOrder[b]) = (_combatOrder[b], _combatOrder[a]);
        }
        // now do each units turn after each other.
        bool activeCombat = true;
        while (activeCombat)
        {
            // TODO: this would not allow units to be taken out of combat
            for (int i = 0; i < _combatOrder.Count; i++)
            {
                await _combatOrder[i].DoTurnAsync(this, _combatGUI);
                // always also wait for at least one additional frame between turns to avoid key inputs mixing into both.
                await Awaitable.NextFrameAsync();

                for (int j = _combatOrder.Count - 1; j >= 0; j--)
                {
                    var unit = _combatOrder[j];
                    if (unit.CurrentHealth <= 0)
                    {
                        if (j <= i)
                        {
                            i--;
                        }
                        _combatOrder.RemoveAt(j);
                        // remove from order and from sets
                        if (_allyUnits.Contains(unit))
                        {
                            var index = _allyUnits.IndexOf(unit);
                            Destroy(_allyArea.GetChild(index).gameObject);
                            _allyUnits.RemoveAt(index);
                        }
                        else if (_enemyUnits.Contains(unit))
                        {
                            var index = _enemyUnits.IndexOf(unit);
                            Destroy(_enemyArea.GetChild(index).gameObject);
                            _enemyUnits.RemoveAt(index);

                        }

                        await _combatGUI.ShowDismissableTextAsync($"{unit.Name} has died.");
                        await Awaitable.NextFrameAsync();
                    }
                }

                activeCombat = _allyUnits.Count > 0 && _enemyUnits.Count > 0;
                if (!activeCombat)
                    break;
                //TODO: how do i detect whether a unit has died?
                // on damage -> health < 0 would be easiest, but the combat system needs to know about it, and then also correctly
                // handle removing the unit from:
                // - the combat order
                // - from the ally / enemy units lists
                // - and also remove the gameobject from the respective areas in the world.
            }
        }
        // rewards and stuff would be given out in here i suppose.
        EndCombatAsync();
    }

    private async void EndCombatAsync()
    {
        await _combatGUI.ShowDismissableTextAsync("You win!");
        _combatGUI.Deactivate();
        await Awaitable.NextFrameAsync();
        Camera.main.transform.position = _oldCameraPosition ;

        var player = DependencyService.RequestDependency<PlayerMovement>();
        player.enabled = true;
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

    public override void Dispose()
    {
        throw new System.NotImplementedException();
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
