using UnityEngine;

public class CombatTrigger : DependentBehaviour
{
    [SerializeField]
    private UnitDefinition[] _units = null;

    [Injected(true)]
    private CombatSystem _combatSystem = null;

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
