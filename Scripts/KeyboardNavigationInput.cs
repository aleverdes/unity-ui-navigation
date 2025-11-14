using UnityEngine;

/// <summary>
/// Standard keyboard-based navigation input implementation.
/// Handles Tab/Shift+Tab for navigation and Space/Enter for activation.
/// </summary>
public class KeyboardNavigationInput : INavigationInput
{
    /// <summary>
    /// Gets the navigation direction based on Tab key input.
    /// </summary>
    /// <returns>
    /// 1 if Tab is pressed (forward navigation),
    /// -1 if Shift+Tab is pressed (backward navigation),
    /// 0 if no navigation input.
    /// </returns>
    public int GetNavigationDirection()
    {
        // Check for Tab navigation
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            // Ignore if Alt or Ctrl modifiers are pressed
            var altIsPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            var ctrlIsPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (!altIsPressed && !ctrlIsPressed)
            {
                var shiftIsPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
                return shiftIsPressed ? -1 : 1;
            }
        }

        // Check for arrow key navigation as alternative
        if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.DownArrow))
        {
            return 1;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.UpArrow))
        {
            return -1;
        }

        return 0;
    }

    /// <summary>
    /// Gets whether submit/activate input is pressed (Space, Enter, or Keypad Enter).
    /// </summary>
    public bool GetSubmitPressed()
    {
        return Input.GetKeyDown(KeyCode.Space) ||
               Input.GetKeyDown(KeyCode.Return) ||
               Input.GetKeyDown(KeyCode.KeypadEnter);
    }

    /// <summary>
    /// Gets whether cancel/back input is pressed (Escape key).
    /// </summary>
    public bool GetCancelPressed()
    {
        return Input.GetKeyDown(KeyCode.Escape);
    }
}
