using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

namespace Toreole.Turnbased.GUI.Binding
{ 
    public class DataBinder : MonoBehaviour
    {
        [SerializeField]
        private UIBehaviour _target;

        [SerializeField]
        private List<DataBinding> _bindings = new();

        private IDataBindingSource _source;

        // 
        readonly static Regex bindRegex = new("{{\\s*([a-zA-Z0-9]+)(:.+)?\\s*}}");

        private readonly List<DataBinding> _dirtyBindings = new();

        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, PropertyInfo> _targetProperties = new();

        /// <summary>
        /// Cached values stored in the relevant source properties.
        /// </summary>
        private readonly Dictionary<string, object> _sourcePropertyValues = new();
        private readonly Dictionary<string, PropertyInfo> _sourceProperties = new();

        private readonly Dictionary<string, List<DataBinding>> _affectedBindingsBySourceProperty = new();

        // ensure we have a target. 
        private void Awake()
        {
            if (_target == null)
            {
                _target = GetComponent<UIBehaviour>();
            }
        }

        private void Start()
        {
            foreach (var binding in _bindings)
            {
                ParseBinding(binding);
            }
        }

        /// <summary>
        /// Adds a binding at runtime. Will instantly parse the binding and set up required fields.
        /// </summary>
        /// <param name="binding"></param>
        public void AddBinding(DataBinding binding)
        {
            _bindings.Add(binding);
            ParseBinding(binding);
            // Debug.Log("AddBinding");
            if (_source != null)
                binding.Apply(_target, _sourcePropertyValues);
        } 

        /// <summary>
        /// Parses a binding. This will register PropertyInfo objects for the targetProperty that will be set
        /// and will also 
        /// </summary>
        /// <param name="binding"></param>
        private void ParseBinding(DataBinding binding)
        {
            var targetProp = GetTargetProperty(binding.TargetPropertyName);
            if (targetProp == null)
                return; // error state

            // check what sourceProperties are used for this binding.
            if (targetProp.PropertyType == typeof(string))
            {
                var matches = bindRegex.Matches(binding.PropertyTemplate);
                foreach (Match match in matches)
                {
                    var sourcePropertyName = match.Groups[1].Value;
                    RegisterAffected(sourcePropertyName, binding);
                    // Debug.Log($"ParseBind - Register: {sourcePropertyName} -> {binding.TargetPropertyName}");
                    RegisterSourceProperty(sourcePropertyName);
                }
            } 
            else
            {
                var sourcePropertyName = binding.PropertyTemplate;
                RegisterAffected(sourcePropertyName, binding);
                // Debug.Log($"ParseBind - Register: {sourcePropertyName} -> {binding.TargetPropertyName}");
                RegisterSourceProperty(sourcePropertyName);
            }
        }

        /// <summary>
        /// Registers a data binding as being affected by changes in the given source property.
        /// </summary>
        /// <param name="sourcePropertyName"></param>
        /// <param name="binding"></param>
        private void RegisterAffected(string sourcePropertyName, DataBinding binding)
        {
            if (_affectedBindingsBySourceProperty.ContainsKey(sourcePropertyName))
            {
                _affectedBindingsBySourceProperty[sourcePropertyName].Add(binding);
            }
            else
            {
                _affectedBindingsBySourceProperty[sourcePropertyName] = new List<DataBinding>() { binding };
            }
        }

        /// <summary>
        /// Registers the source PropertyInfo and the current value of the property in local cache.
        /// </summary>
        /// <param name="sourcePropertyName"></param>
        private void RegisterSourceProperty(string sourcePropertyName)
        {
            if (_source == null)
                return;
            var prop = _source.GetType().GetProperty(sourcePropertyName);
            _sourceProperties[sourcePropertyName] = prop;
            _sourcePropertyValues[sourcePropertyName] = prop.GetValue(_source);
        }

        /// <summary>
        /// Gets a target property from cache, or obtains it directly and registers it in the cache.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private PropertyInfo GetTargetProperty(string propertyName)
        {
            if (_targetProperties.ContainsKey(propertyName))
                return _targetProperties[propertyName];
            var targetType = _target.GetType(); // !!!! target must not be null.
            var targetProp = targetType.GetProperty(propertyName);
            if (targetProp == null || !targetProp.GetSetMethod().IsPublic)
            {
                var nearMatch = targetType.GetProperties().FirstOrDefault(x => x.Name.ToLower() == propertyName.ToLower());
                if (nearMatch != null)
                {
                    Debug.LogError($"Property with name '{propertyName}' does not exist. " +
                        $"Did you mean ${nearMatch.Name}?", this);
                }
                else
                {
                    Debug.LogError(
                        $"Property with name '{propertyName}' does not exist " +
                        $"on target of type {_target.GetType().FullName} " +
                        "or has a non-public setter.", this);
                }
                return null;
            }
            // cache prop
            _targetProperties[propertyName] = targetProp;
            return targetProp;
        }


        /// <summary>
        /// Overrides the current data source.
        /// </summary>
        /// <param name="source"></param>
        public void BindTo(IDataBindingSource source)
        {
            if (_source != null)
            {
                _source.OnChange -= OnSourceChanged;
                _source = source;
            } 
            else
            {
                _source = source;
                foreach (var affects in _affectedBindingsBySourceProperty)
                {
                    // Debug.Log($"BindTo - Register: {affects.Key} -> {affects.Value.FirstOrDefault()?.TargetPropertyName}");
                    RegisterSourceProperty(affects.Key);
                }
            }
            source.OnChange += OnSourceChanged;

            // TODO: set up source properties and source property values.

            foreach (var binding in _bindings)
            {
                binding.Apply(_target, _sourcePropertyValues);
            }
        }

        private void LateUpdate()
        {
            if (_source == null)
                return;

            // update dirty bindings.
            foreach(var binding in _dirtyBindings)
            {
                binding.Apply(_target, _sourcePropertyValues);
            }
            _dirtyBindings.Clear();
        }

        private void OnSourceChanged(string propertyName, object value)
        {
            var affectedBindings = _affectedBindingsBySourceProperty[propertyName];
            if (affectedBindings == null || affectedBindings.Count == 0)
                return;
            _sourcePropertyValues[propertyName] = value;
            foreach (var binding in affectedBindings)
            {
                if (!_dirtyBindings.Contains(binding))
                    _dirtyBindings.Add(binding);
            }
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
        private string _targetPropertyName;

        /// <summary>
        /// For string properties this can be "hello {{Name}}". 
        /// For other types such as floats, it must only be "Number".
        /// </summary>
        [SerializeField]
        private string _propertyTemplate;

        public string TargetPropertyName => _targetPropertyName;
        public string PropertyTemplate => _propertyTemplate;

        public DataBinding(string targetPropName, string valueTemplate)
        {
            _targetPropertyName = targetPropName;
            _propertyTemplate = valueTemplate;
        }


        readonly static Regex bindRegex = new("{{\\s*([a-zA-Z0-9]+)(:.+)?\\s*}}");

        public void Apply(UIBehaviour target, Dictionary<string, object> sourceValues)
        {
            var targetProp = target.GetType().GetProperty(_targetPropertyName);
            if (targetProp == null || targetProp.GetSetMethod().IsPrivate)
            {
                Debug.LogError($"Property with name '{_targetPropertyName}' does not on exist on target of type {target.GetType().FullName} or has a non-public setter.", target);
                return;
            }

            if (targetProp.PropertyType == typeof(string))
            {
                var filledTemplate = _propertyTemplate;
                var boundValues = bindRegex.Matches(_propertyTemplate);

                foreach (Match boundValue in boundValues)
                {
                    string sourceName = boundValue.Groups[1].Value;
                    filledTemplate = filledTemplate.Replace(boundValue.Value, sourceValues[sourceName].ToString());
                }
                targetProp.SetValue(target, filledTemplate);
            }
            else
            {
                var targetPropType = targetProp.PropertyType;

                try
                {
                    var value = Convert.ChangeType(sourceValues[_propertyTemplate], targetPropType);
                    targetProp.SetValue(target, value);
                }
                catch (Exception ex)
                {
                    // could not convert
                    Debug.LogError(ex.ToString());
                }
            
            }
        }
    }
}
