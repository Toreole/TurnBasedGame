using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;


// TODO: would it be useful to have a list of fallback prefabs for specific 
// necessary components?
internal static class DependencyService
{
    private static Dictionary<Type, ProviderBehaviour> dependencies = new ();

    public static bool Register<T>(T obj, bool replace = false) where T : ProviderBehaviour
    {
        ProviderBehaviour existing = null;
        if (dependencies.TryGetValue(typeof(T), out existing))
        {
            if (replace)
            {
                existing.Dispose();
                dependencies[typeof(T)] = obj;
                Debug.Log($"Replaced registered Object for Type {typeof(T).FullName}");
                return true;
            }
            return false;
        } 
        dependencies[typeof(T)] = obj;
        Debug.Log($"Registered Object for Type {typeof(T).FullName}");
        return true;
    }

    /// <summary>
    /// This should only ever be called from the registered object itself.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns></returns>
    public static bool Unregister(ProviderBehaviour obj)
    {
        var type = obj.GetType();
        if (dependencies.TryGetValue(type, out var registered) && registered == obj)
        {
            dependencies.Remove(type);
            return true;
        } 
        else
        {
            Debug.LogWarning($"Object {obj.name}#{obj.GetInstanceID()} attempted to unregister {type.FullName}, which was not registered or a different value than itself.");
        }
        return false;
    }

    /// <summary>
    /// Requests all dependencies of an object to be filled if possible.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns>true if all dependencies could be filled. false otherwise.</returns>
    public static bool FillDependencies(UnityEngine.Object obj)
    {
        Debug.Log($"Fill {obj.name}");
        var type = obj.GetType();
        foreach(var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            Debug.Log($"Inspecting {type.Name}.{field.Name}");
            var att = field.GetCustomAttribute<InjectedAttribute>();
            if (att == null)
                continue;
            Debug.Log("Field has InjectedAttribute");
            var dependency = dependencies[field.FieldType];
            if (dependency is null)
            {
                var logMessage = $"Object {obj.name}#{obj.GetInstanceID()} depends on {type.FullName}, which was not registered.";
                if (att.IsFatal)
                {
                    Debug.LogError(logMessage);
                    return false;
                } 
                else
                {
                    Debug.LogWarning(logMessage);
                }
                    
            }
            field.SetValue(obj, dependency);
        }

        return true;
    }

}

