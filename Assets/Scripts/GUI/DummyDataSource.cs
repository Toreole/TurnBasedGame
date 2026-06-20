using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace Assets.Scripts.GUI
{
    public class DummyDataSource : MonoBehaviour, IDataBindingSource
    {
        public event Action<string, object> OnChange;

        [SerializeField]
        private new string name;

        [SerializeField]
        private int fontSize = 25;

        public string Name { 
            get => name; 
            set { 
                if (name != value) 
                    OnChange?.Invoke(nameof(Name), value);
                name = value;
            } 
        }

        public int FontSize
        {
            get => fontSize; set
            {
                if (fontSize != value)
                    OnChange?.Invoke(nameof(FontSize), value);
                fontSize = value;
            }
        }

        // Use this for initialization
        void Start()
        {
            var binder = GetComponent<DataBinder>();
            binder.BindTo(this);
            binder.AddBinding(new DataBinding(nameof(TextMeshProUGUI.text), "hello, {{Name}}. The current font size is: {{FontSize}}"));
            binder.AddBinding(new DataBinding(nameof(TextMeshProUGUI.fontSize), nameof(FontSize)));

        }

    }
}