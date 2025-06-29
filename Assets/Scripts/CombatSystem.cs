using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    [SerializeField]
    private CombatGUI _combatGUI;

    // for testing stuff.
    private void Start()
    {
        TestStuffAsync();
    }

    private async void TestStuffAsync()
    {
        var options = new string[] { "hello", "world", "this", "is", "a", "test", "lmao" };
        var selectedIndex = await _combatGUI.SelectActionAsync(options);
        await _combatGUI.ShowDismissableTextAsync($"Selected {options[selectedIndex]}!");
    }
}
