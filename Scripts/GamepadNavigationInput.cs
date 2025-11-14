using UnityEngine;

/// <summary>
/// Gamepad-based navigation input implementation.
/// Uses D-pad and face buttons for navigation and activation.
/// </summary>
public class GamepadNavigationInput : INavigationInput
{
    /// <summary>
    /// Gets the navigation direction based on gamepad D-pad input.
    /// </summary>
    /// <returns>
    /// 1 for forward navigation (D-pad right/down),
    /// -1 for backward navigation (D-pad left/up),
    /// 0 for no navigation input.
    /// </returns>
    public int GetNavigationDirection()
    {
        // Check horizontal D-pad
        float horizontal = Input.GetAxis("Horizontal");
        if (Input.GetButtonDown("Horizontal") || Mathf.Abs(horizontal) > 0.5f)
        {
            if (horizontal > 0.5f || Input.GetKeyDown(KeyCode.JoystickButton14)) // D-pad right
            {
                return 1;
            }
            if (horizontal < -0.5f || Input.GetKeyDown(KeyCode.JoystickButton13)) // D-pad left
            {
                return -1;
            }
        }

        // Check vertical D-pad
        float vertical = Input.GetAxis("Vertical");
        if (Input.GetButtonDown("Vertical") || Mathf.Abs(vertical) > 0.5f)
        {
            if (vertical < -0.5f || Input.GetKeyDown(KeyCode.JoystickButton11)) // D-pad down
            {
                return 1;
            }
            if (vertical > 0.5f || Input.GetKeyDown(KeyCode.JoystickButton12)) // D-pad up
            {
                return -1;
            }
        }

        // Alternative: Use bumpers/triggers for navigation
        if (Input.GetKeyDown(KeyCode.JoystickButton5)) // Right bumper
        {
            return 1;
        }
        if (Input.GetKeyDown(KeyCode.JoystickButton4)) // Left bumper
        {
            return -1;
        }

        return 0;
    }

    /// <summary>
    /// Gets whether submit/activate input is pressed (A button / South face button).
    /// </summary>
    public bool GetSubmitPressed()
    {
        return Input.GetKeyDown(KeyCode.JoystickButton0) || // A button
               Input.GetButtonDown("Submit");
    }

    /// <summary>
    /// Gets whether cancel/back input is pressed (B button / East face button).
    /// </summary>
    public bool GetCancelPressed()
    {
        return Input.GetKeyDown(KeyCode.JoystickButton1) || // B button
               Input.GetButtonDown("Cancel");
    }
}
