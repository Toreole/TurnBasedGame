using System.Collections;
using UnityEngine;


/// <summary>
/// A Monobehaviour script that needs to be available to be injected in dependent scripts.
/// </summary>
public abstract class ProviderBehaviour : MonoBehaviour
{
    protected virtual void Awake()
    {
        if(!Register())
        {
            // should this gameObject / script just be destroyed instead?
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Body should be `return DependencyService.Register(this)`
    /// </summary>
    protected abstract bool Register();
    /// <summary>
    /// Clean up used resources, created objects, etc.
    /// </summary>
    public abstract void Dispose();


    protected virtual void OnDestroy()
    {
        DependencyService.Unregister(this);
    }

}
