using System;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DataBinder : MonoBehaviour
{
    [SerializeField]
    private UIBehaviour target;

    [SerializeField]
    private DataBinding[] bindings;

    private IDataBindingSource source;

    // ensure we have a target. 
    private void Awake()
    {
        if (target == null)
        {
            target = GetComponent<UIBehaviour>();
        }
    }

    private void Start()
    {
        
    }

    public void BindTo(IDataBindingSource source)
    {
        if (this.source != null)
        {
            this.source.OnChange -= OnSourceChanged;
        }
        this.source = source;
        source.OnChange += OnSourceChanged;

        foreach (var binding in bindings)
        {
            binding.SetProperty(target, source);
        }
    }

    private void OnSourceChanged(string propertyName, object value)
    {

    }

}

public interface IDataBindingSource
{
    /// <summary>
    /// Name of the property that changed, and its current value.
    /// </summary>
    public event Action<string, object> OnChange;
}

[Serializable]
public class DataBinding
{
    [SerializeField]
    private string targetPropertyName;

    /// <summary>
    /// For string properties this can be "hello {{Name}}". 
    /// For other types such as floats, it must only be "{{Number}}".
    /// </summary>
    [SerializeField]
    private string propertyTemplate;


    public DataBinding(string targetPropName, string valueTemplate)
    {
        targetPropertyName = targetPropName;
        propertyTemplate = valueTemplate;
    }

    readonly static Regex bindRegex = new("{{\\s*([a-zA-Z0-9]+)(:.+)?\\s*}}");

    public void SetProperty(UIBehaviour target, IDataBindingSource source)
    {
        var targetProp = target.GetType().GetProperty(targetPropertyName);
        if (targetProp == null || targetProp.GetGetMethod().IsPrivate)
            throw new InvalidOperationException($"Property with name '{targetPropertyName}' does not on exist on target of type {target.GetType().FullName}");

        var sourceType = source.GetType();
        if (targetProp.PropertyType == typeof(string))
        {
            var filledTemplate = propertyTemplate;
            var boundValues = bindRegex.Matches(propertyTemplate);

            foreach (Match boundValue in boundValues)
            {
                filledTemplate = filledTemplate.Replace(boundValue.Value, sourceType.GetProperty(boundValue.Groups[1].Value).GetValue(source).ToString());
            }
            targetProp.SetValue(target, filledTemplate);
        }

    }
}
