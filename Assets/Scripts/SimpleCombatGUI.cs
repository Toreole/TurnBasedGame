using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class SimpleCombatGUI : CombatGUI
    {
        [SerializeField]
        private GameObject _textPanel;
        [SerializeField]
        private TextMeshProUGUI _textPanelContent;
        [SerializeField]
        private GameObject _textPanelConfirm;

        [SerializeField]
        private GameObject _selectionPanel;
        [SerializeField]
        private GameObject _selectionOptionPrefab;
        [SerializeField]
        private int _optionsPerSection;

        public override async Awaitable<int> SelectActionAsync(string[] actions)
        {
            var sectionCount = Mathf.CeilToInt(actions.Length / (float)_optionsPerSection);
            Debug.Log($"{actions.Length} actions -> {sectionCount} sections, {_optionsPerSection} each");
            var options = new GameObject[actions.Length];
            _selectionPanel.SetActive(true);
            //clear sections
            for (int i = _selectionPanel.transform.childCount; i > 0; i--)
            {
                Destroy(_selectionPanel.transform.GetChild(0).gameObject);
            }
            // instantiate new sections.
            for (int i = 0; i < sectionCount; i++)
            {
                var section = new GameObject();
                section.transform.parent = _selectionPanel.transform;
                var sectionGroup = section.AddComponent<VerticalLayoutGroup>();
                sectionGroup.padding = new RectOffset(50, 0, 0, 0);
                sectionGroup.childControlHeight = false;
                sectionGroup.childControlWidth = false;

                for (int j = 0; j < _optionsPerSection; j++)
                {
                    var index = i * _optionsPerSection + j;
                    if (index >= actions.Length)
                        break;
                    options[index] = Instantiate(_selectionOptionPrefab, section.transform);
                    options[index].GetComponent<TextMeshProUGUI>().text = actions[index];
                    // from now on just the indicator
                    options[index] = options[index].transform.GetChild(0).gameObject;
                }
            }
            //default selection is 0
            var selectionIndex = 0;
            options[selectionIndex].SetActive(true);

            //wait until Return is pressed to submit selection.
            while (Input.GetKeyDown(KeyCode.Return) == false)
            {
                var oldSelection = selectionIndex;
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    selectionIndex--;
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    selectionIndex++;
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    selectionIndex -= _optionsPerSection;
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                    selectionIndex += _optionsPerSection;
                if (selectionIndex >= 0 && selectionIndex < options.Length)
                {
                    options[oldSelection].SetActive(false);
                    options[selectionIndex].SetActive(true);
                } else
                {
                    selectionIndex = oldSelection;
                }
                await Awaitable.NextFrameAsync();
            }

            // hide selection UI again.
            _selectionPanel.SetActive(false);

            return selectionIndex;
        }

        public override async Awaitable ShowDismissableTextAsync(string text)
        {
            //setup ui elements
            _selectionPanel.SetActive(false);
            _textPanel.SetActive(true);
            _textPanelContent.text = text;
            //wait a minimum delay
            await Awaitable.WaitForSecondsAsync(2f);
            //show confirmation prompt
            _textPanelConfirm.SetActive(true);
            //wait for input by user
            while (Input.anyKeyDown == false)
                await Awaitable.NextFrameAsync();
            //disable ui elements again
            _textPanel.SetActive(false);
        }
    }
}