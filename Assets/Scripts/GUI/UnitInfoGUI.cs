using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Toreole.Turnbased.GUI
{
    // This is scuffed and could be replaced by better unique bindings. this just passes
    // values along to where they actually are supposed to go.
    public class UnitInfoGUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _unitNameText;

        [SerializeField]
        private Slider _healthSlider;

        [SerializeField]
        private Slider _resourceSlider;

        [SerializeField]
        private TextMeshProUGUI _healthText;
        [SerializeField]
        private TextMeshProUGUI _resourceText;

        public string Name { set => _unitNameText.text = value; }

        public float Health { set => _healthSlider.value = value; }
        public float MaxHealth { set => _healthSlider.maxValue = value; }

        public float Resource { set => _resourceSlider.value = value; }
        public float MaxResource { set => _resourceSlider.maxValue = value; }

        public string HealthText { set => _healthText.text = value; }
        public string ResourceText { set => _resourceText.text = value; }
    }
}
