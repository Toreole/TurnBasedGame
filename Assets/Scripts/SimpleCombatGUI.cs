using System;
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

        [Header("Selection Shared")]
        [SerializeField]
        private GameObject _selectionOptionPrefab;
        [SerializeField]
        private int _optionsPerSection;

        [Header("Simple Selection")]
        [SerializeField]
        private GameObject _selectionPanel;

        [Header("Advanced Selection")]
        [SerializeField]
        private GameObject _advancedSelectionPanel;
        [SerializeField]
        private GameObject _advSelArrowUp;
        [SerializeField]
        private GameObject _advSelArrowDown;
        [SerializeField]
        private TextMeshProUGUI _advSelectionDescription;
        [SerializeField]
        private GridLayoutGroup _advSelectionGrid;

        [SerializeField]
        private CanvasGroup _group;

        protected override bool Register()
        {
            return DependencyService.Register<CombatGUI>(this);
        }
        protected override void Unregister()
        {
            DependencyService.Unregister<CombatGUI>(this);
        }

        public override async Awaitable<int> SelectActionAsync(string[] actions)
        {
            var sectionCount = Mathf.CeilToInt(actions.Length / (float)_optionsPerSection);
            var options = new GameObject[actions.Length];
            _selectionPanel.SetActive(true);
            //clear sections
            _selectionPanel.transform.DestroyChildren();
            // instantiate new sections.
            for (int i = 0; i < sectionCount; i++)
            {
                var section = new GameObject();
                section.transform.parent = _selectionPanel.transform;
                section.transform.localScale = Vector3.one; //for some reason this comes out at 1.8 otherwise
                var sectionGroup = section.AddComponent<VerticalLayoutGroup>();
                //sectionGroup.padding = new RectOffset(50, 0, 0, 0); not necessary anymore
                sectionGroup.childControlHeight = false;
                sectionGroup.childControlWidth = false;

                for (int j = 0; j < _optionsPerSection; j++)
                {
                    var index = i * _optionsPerSection + j;
                    if (index >= actions.Length)
                        break;
                    var option = Instantiate(_selectionOptionPrefab, section.transform);
                    option.GetComponent<TextMeshProUGUI>().text = actions[index];
                    // from now on just the indicator
                    options[index] = option.transform.GetChild(0).gameObject;
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
                } 
                else
                {
                    selectionIndex = oldSelection;
                }
                await Awaitable.NextFrameAsync();
            }
            await Awaitable.NextFrameAsync();
            // hide selection UI again.
            _selectionPanel.SetActive(false);

            return selectionIndex;
        }

        public async override Awaitable<int> SelectDescriptiveAsync(INameAndDescription[] items)
        {
            using (new Utility.TemporarilyActiveObject(_advancedSelectionPanel))
            {

            }
            _advancedSelectionPanel.SetActive(true);
            
            var gridTransform = _advSelectionGrid.transform;
            // Reset everything that isnt overridden by default.
            gridTransform.DestroyChildren();
            _advSelArrowDown.SetActive(false);
            _advSelArrowUp.SetActive(false);
            // create new
            var options = new GameObject[items.Length];
            for (int i = 0; i < items.Length; i++)
            {
                var option = Instantiate(_selectionOptionPrefab, gridTransform);
                option.GetComponent<TextMeshProUGUI>().text = items[i].Name;
                // from now on just the indicator
                options[i] = option.transform.GetChild(0).gameObject;
            }
            int rowCount = options.Length / 2 + 1;
            if (rowCount > _optionsPerSection)
                _advSelArrowDown.SetActive(true);
            //helpers for scolling.
            int firstRowToScroll = _optionsPerSection;
            int amountOfScrollRows = Mathf.Max(0, rowCount + 1 - 2 * _optionsPerSection); //0 or above.

            var selectedIndex = 0; 
            _advSelectionDescription.text = items[selectedIndex].Description;
            options[selectedIndex].SetActive(true);
            // wait until selection confirmation.
            while (Input.GetKeyDown(KeyCode.Return) == false)
            {
                var oldSelection = selectedIndex;
                if (Input.GetKeyDown(KeyCode.RightArrow))
                    selectedIndex++;
                else if (Input.GetKeyDown(KeyCode.LeftArrow))
                    selectedIndex--;
                else if (Input.GetKeyDown(KeyCode.UpArrow))
                    selectedIndex -= 2;
                else if (Input.GetKeyDown(KeyCode.DownArrow))
                    selectedIndex += 2;
                if (selectedIndex >= 0 && selectedIndex < items.Length)
                {
                    options[oldSelection].SetActive(false);
                    options[selectedIndex].SetActive(true);
                    _advSelectionDescription.text = items[selectedIndex].Description;

                    // there could be a possible way of handling up/down scrolling differently
                    // that requires tracking currentScrollRowIndex, but this is fine as it is.
                    // calculate grid "scroll" offset.
                    int row = selectedIndex / 2;
                    int rowScroll = Mathf.Clamp(row - firstRowToScroll, 0, amountOfScrollRows);
                    _advSelectionGrid.padding = new RectOffset(0, 0, -(rowScroll * Mathf.CeilToInt(_advSelectionGrid.cellSize.y)), 0);

                    _advSelArrowUp.SetActive(row > firstRowToScroll);
                    _advSelArrowDown.SetActive(rowCount > _optionsPerSection && row - _optionsPerSection < amountOfScrollRows);
                }
                else
                {
                    selectedIndex = oldSelection;
                }
                await Awaitable.NextFrameAsync();
            }
            await Awaitable.NextFrameAsync();
            _advancedSelectionPanel.SetActive(false);
            return selectedIndex;
        }

        public override async Awaitable ShowDismissableTextAsync(string text)
        {
            //setup ui elements
            _selectionPanel.SetActive(false);
            _textPanel.SetActive(true);
            _textPanelContent.text = text;
            //wait a minimum delay
            await Awaitable.WaitForSecondsAsync(1f);
            //show confirmation prompt
            _textPanelConfirm.SetActive(true);
            //wait for input by user
            while (Input.anyKeyDown == false)
                await Awaitable.NextFrameAsync();
            //disable ui elements again
            _textPanel.SetActive(false);
            _textPanelConfirm.SetActive(false);
        }

        public override void Dispose()
        {
            throw new NotImplementedException();
        }

        public override void Activate()
        {
            _group.alpha = 1;
            _group.blocksRaycasts = true;
            _group.interactable = true;
        }

        public override void Deactivate()
        {
            _group.alpha = 0;
            _group.blocksRaycasts = false;
            _group.interactable = false;
        }
    }
}