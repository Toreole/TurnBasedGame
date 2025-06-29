using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [SerializeField]
    private CombatGUI m_CombatGUI;

    // for testing stuff.
    private void Start()
    {
        TestStuffAsync();
    }

    private async void TestStuffAsync()
    {
        var options = new string[] { "hello", "world", "this", "is", "a", "test", "lmao" };
        var selectedIndex = await m_CombatGUI.SelectActionAsync(options);
        await m_CombatGUI.ShowDismissableTextAsync($"Selected {options[selectedIndex]}!");
    }
}
