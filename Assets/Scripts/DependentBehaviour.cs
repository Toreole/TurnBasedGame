using System.Collections;
using UnityEngine;


/// <summary>
/// A MonoBehaviour script that is dependent on some global game state, which
/// will be injected into this object.
/// </summary>
public abstract class DependentBehaviour : MonoBehaviour
{

    protected virtual void Start()
    {
        DependencyService.FillDependencies(this);
    }

}
