using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;


// TODO: would it be useful to have a list of fallback prefabs for specific 
// necessary components?
internal static class DependencyService
{
    // idk it feels like this shouldnt be limited to ProviderBehaviour
    private static readonly Dictionary<Type, ProviderBehaviour> dependencies = new ();

    // generic syntax alternative
    public static bool Register<T>(T obj, bool replace = false) where T : ProviderBehaviour
    {
        return Register(obj, typeof(T), replace);
    }

    /// <summary>
    /// Register a Provider with the dependency service.
    /// </summary>
    /// <param name="obj">concrete service to register</param>
    /// <param name="asType">what type to register it as</param>
    /// <param name="replace"></param>
    /// <returns></returns>
    public static bool Register(ProviderBehaviour obj, Type asType, bool replace = false)
    {
        if(!asType.IsAssignableFrom(obj.GetType()))
        {
            throw new ArgumentException(
                $"Provided object {obj.name}#{obj.GetInstanceID()} " +
                $"of type {obj.GetType().FullName} is not assignable " +
                $"to registering type {asType.FullName}"
            );
        }
        ProviderBehaviour existing = null;
        if (dependencies.TryGetValue(asType, out existing) && existing != null)
        {
            // TODO: This currently leads to undefined behaviour
            // when a dependency is replaced, while there are still
            // objects that hold a reference to the old one.
            // There should be some form of event to notify dependants of an update.
            if (replace)
            {
                existing.Dispose();
                dependencies[asType] = obj;
                Debug.Log($"Replaced registered Object for Type {asType.FullName}");
                return true;
            }
            return false;
        }
        dependencies[asType] = obj;
        Debug.Log($"Registered Object for Type {asType.FullName}");
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
    /// Dependencies are fields with a [Injected] Attribute.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="obj"></param>
    /// <returns>true if all dependencies could be filled. false otherwise.</returns>
    public static bool FillDependencies(UnityEngine.Object obj)
    {
        var type = obj.GetType();
        foreach(var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            var att = field.GetCustomAttribute<InjectedAttribute>();
            if (att == null)
                continue;
            if (dependencies.ContainsKey(field.FieldType))
            {
                var dependency = dependencies[field.FieldType];
                if (dependency == null) // dependency in dictionary, but destroyed or otherwise null
                {
                    FailedDependencyLogError(obj, field.FieldType, att);
                    if (att.IsFatal)
                        return false;
                }
                field.SetValue(obj, dependency);
            } 
            else // dependency not in dictionary
            {
                FailedDependencyLogError(obj, field.FieldType, att);
                if (att.IsFatal)
                    return false;
            }
        }
        return true;
    }

    // not sure whether to actually allow this or not.
    public static T RequestDependency<T>(bool isFatal = true) where T : ProviderBehaviour
    {
        var type = typeof(T);
        if (dependencies.TryGetValue(type, out var dependency))
        {
            if (dependency == null && isFatal)
            {
                throw new KeyNotFoundException(type.FullName);
            } 
            return dependency as T;
        } else if (isFatal)
            throw new KeyNotFoundException(type.FullName);
        return null;
    }

    private static void FailedDependencyLogError(UnityEngine.Object obj, Type type, InjectedAttribute att)
    {
        var logMessage = $"Object {obj.name}#{obj.GetInstanceID()} depends on {type.FullName}, which was not registered. " +
            "Make sure the dependency exists and is enabled.";
        if (att.IsFatal)
            Debug.LogError(logMessage);
        else
            Debug.LogWarning(logMessage);
        
    }
}
