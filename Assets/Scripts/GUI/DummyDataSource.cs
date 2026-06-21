using System;
using System.Collections;
using TMPro;
using UnityEngine;
using Toreole.Turnbased.GUI.Binding;

namespace Toreole.Turnbased.GUI
{
    public partial class DummyDataSource : MonoBehaviour, IDataBindingSource
    {
        // IDataBindingSource event
        public event Action<string, object> OnChange;

        [SerializeField]
        [ReactiveProperty(PrivateSetter = true)]
        private string _name;

        [SerializeField]
        [ReactiveProperty(PrivateSetter = true)]
        private int _fontSize = 25;

        [ReactiveProperty(PrivateSetter = true)]
        private float _health = 50f;


        // Use this for initialization
        void Start()
        {
            var binder = GetComponent<DataBinder>();
            binder.BindTo(this);
            binder.AddBinding(new DataBinding(nameof(TextMeshProUGUI.text), "hello, {{Name}}. The current font size is: {{Health:0.0}}"));
            binder.AddBinding(new DataBinding(nameof(TextMeshProUGUI.fontSize), "Health"));

            StartCoroutine(DoRandomizeHealth());
        }

        private IEnumerator DoRandomizeHealth()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.4f);
                Health = UnityEngine.Random.Range(0, 50);
            }
        }


    }
}