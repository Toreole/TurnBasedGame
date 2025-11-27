using System.Collections;
using UnityEngine;


public class CombatUnitInstance : MonoBehaviour
{
    [SerializeField]
    private GameObject _selector;

    [SerializeField]
    private GameObject _highlight;

    public void SetSelected(bool selected) => _selector.SetActive(selected);

    public void SetHighlight(bool highlight) => _highlight.SetActive(highlight);
}
