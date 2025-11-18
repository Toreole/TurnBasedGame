using UnityEngine;

public class CombatTrigger : MonoBehaviour
{
    [SerializeField]
    private UnitDefinition[] _units = null;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.otherRigidbody.CompareTag("Player"))
        {
            // TODO: trigger CombatSystem with units defined above.
        }
    }
}
