using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Scripts
{
    public class SimpleCombatGUI : CombatGUI
    {
        [SerializeField]
        private GameObject m_textPanel;
        [SerializeField]
        private TextMeshProUGUI m_textPanelContent;
        [SerializeField]
        private GameObject m_textPanelConfirm;

        [SerializeField]
        private GameObject m_selectionPanel;
        [SerializeField]
        private GameObject m_selectionOptionPrefab;
        [SerializeField]
        private int m_optionsPerSection;

        public override async Awaitable<int> SelectActionAsync(string[] actions)
        {
            var sectionCount = Mathf.CeilToInt(actions.Length / (float)m_optionsPerSection);
            Debug.Log($"{actions.Length} actions -> {sectionCount} sections, {m_optionsPerSection} each");
            var options = new GameObject[actions.Length];
            m_selectionPanel.SetActive(true);
            //clear sections
            for (int i = m_selectionPanel.transform.childCount; i > 0; i--)
            {
                Destroy(m_selectionPanel.transform.GetChild(0).gameObject);
            }
            // instantiate new sections.
            for (int i = 0; i < sectionCount; i++)
            {
                var section = new GameObject();
                section.transform.parent = m_selectionPanel.transform;
                var sectionGroup = section.AddComponent<VerticalLayoutGroup>();
                sectionGroup.padding = new RectOffset(50, 0, 0, 0);
                sectionGroup.childControlHeight = false;
                sectionGroup.childControlWidth = false;

                for (int j = 0; j < m_optionsPerSection; j++)
                {
                    var index = i * m_optionsPerSection + j;
                    if (index >= actions.Length)
                        break;
                    options[index] = Instantiate(m_selectionOptionPrefab, section.transform);
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
                    selectionIndex -= m_optionsPerSection;
                else if (Input.GetKeyDown(KeyCode.RightArrow))
                    selectionIndex += m_optionsPerSection;
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
            m_selectionPanel.SetActive(false);

            return selectionIndex;
        }

        public override async Awaitable ShowDismissableTextAsync(string text)
        {
            //setup ui elements
            m_selectionPanel.SetActive(false);
            m_textPanel.SetActive(true);
            m_textPanelContent.text = text;
            //wait a minimum delay
            await Awaitable.WaitForSecondsAsync(2f);
            //show confirmation prompt
            m_textPanelConfirm.SetActive(true);
            //wait for input by user
            while (Input.anyKeyDown == false)
                await Awaitable.NextFrameAsync();
            //disable ui elements again
            m_textPanel.SetActive(false);
        }
    }
}