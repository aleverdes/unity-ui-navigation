using UnityEngine;

/// <summary>
/// Interface for navigation input handling.
/// Implement this interface to create custom input schemes for navigation.
/// </summary>
public interface INavigationInput
{
    /// <summary>
    /// Gets the navigation direction based on current input.
    /// </summary>
    /// <returns>
    /// Returns the navigation direction:
    /// 1 for forward/next navigation,
    /// -1 for backward/previous navigation,
    /// 0 for no navigation input.
    /// </returns>
    /// <remarks>
    /// This method is called every frame to check for navigation input.
    /// Return 0 when no navigation input is detected.
    /// </remarks>
    int GetNavigationDirection();

    /// <summary>
    /// Gets whether the submit/activate input is pressed.
    /// </summary>
    /// <returns>True if submit/activate input is detected, false otherwise.</returns>
    /// <remarks>
    /// This is typically used for activating buttons or confirming selections.
    /// </remarks>
    bool GetSubmitPressed();

    /// <summary>
    /// Gets whether the cancel/back input is pressed.
    /// </summary>
    /// <returns>True if cancel/back input is detected, false otherwise.</returns>
    /// <remarks>
    /// This is typically used for closing dialogs or going back in menus.
    /// </remarks>
    bool GetCancelPressed();
}
