using System.Collections;
using UnityEngine;


/// <summary>
/// Just a MonoBehaviour script with a predefined Start() method
/// that calls DependencyService.FillDependencies(this);
/// </summary>
public abstract class DependentBehaviour : MonoBehaviour
{

    protected virtual void Start()
    {
        DependencyService.FillDependencies(this);
    }

}
