using UnityEngine;

/// <summary>
/// Abstract class that provides the interface for the CombatGUI to the CombatSystem.
/// This would have been an interface, if unity could serialize those by reference.
/// Can technically be used for more than just Combat.
/// </summary>
// loose coupling, you know how it is.
public abstract class CombatGUI : MonoBehaviour
{
    /// <summary>
    /// Show a dismissable text on the GUI. 
    /// Await this method to wait until the text has been dismissed.
    /// </summary>
    /// <param name="text">The text to show the user.</param>
    /// <returns>N/A</returns>
    public abstract Awaitable ShowDismissableTextAsync(string text);

    /// <summary>
    /// Asynchronously select one of a given set of actions.
    /// </summary>
    /// <param name="actions">List of actions by name.</param>
    /// <returns>Index of the selected action.</returns>
    public abstract Awaitable<int> SelectActionAsync(string[] actions);
}

