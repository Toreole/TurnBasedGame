using System;
using System.Collections;
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

        // Use this for initialization
        void Start()
        {
            GetComponent<DataBinder>().BindTo(this);
        }

    }
}