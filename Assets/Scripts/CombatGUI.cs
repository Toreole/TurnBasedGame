using UnityEngine;

/// <summary>
/// Abstract class that provides the interface for the CombatGUI to the CombatSystem.
/// This would have been an interface, if unity could serialize those by reference.
/// Can technically be used for more than just Combat.
/// </summary>
// loose coupling, you know how it is.
// CombatGUI on its own could technically just as well be an interface at this point, since we dont serialize references to it directly anyway,
// and instead just fill injected fields via the DependencyService

// This is not really specific to Combat tbh.

// TODO: CombatGUI needs capability of showing Resource Bars for Allies.
public abstract class CombatGUI : ProviderBehaviour
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

    /// <summary>
    /// Asynchronously select one of a given set of items, displaying their name and descriptions.
    /// </summary>
    /// <param name="items">List of options.</param>
    /// <returns>index of selected option.</returns>
    public abstract Awaitable<int> SelectDescriptiveAsync(INameAndDescription[] items);

    public abstract void Activate();
    public abstract void Deactivate();
}
