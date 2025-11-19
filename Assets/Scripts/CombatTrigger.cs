using UnityEngine;

public class CombatTrigger : DependentBehaviour
{
    [SerializeField]
    private UnitDefinition[] _units = null;

    // unintuitive, but the dependency service can still fill this field
    // with the correctly value despite the readonly declaration.
    [Injected(true)]
    private readonly CombatSystem _combatSystem = null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log("Collision");
        if (collision.rigidbody.CompareTag("Player"))
        {
            // TODO: trigger CombatSystem with units defined above.
            _combatSystem.SetupEnemies(_units);
            _combatSystem.Engage();
        }
    }
}
