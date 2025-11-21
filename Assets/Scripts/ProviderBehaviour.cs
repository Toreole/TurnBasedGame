using System.Collections;
using UnityEngine;


/// <summary>
/// A Monobehaviour script that needs to be available to be injected in dependent scripts.
/// </summary>
// It just made sense to me to make provider a subclass of dependent.
// after all, any class that provides a resource might need other resources that came before it.
// e.g. the combatsystem might need a GUI system provided to it. and just resolving that through
// the dependency system in here is simpler than having it serialized in the editor.
// you just have to make sure that both exist (there will be errors when a fatal dependency is not present)
public abstract class ProviderBehaviour : DependentBehaviour
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
