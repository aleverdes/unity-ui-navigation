using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// Legacy helper component for button activation.
/// This component is now deprecated - button activation is handled automatically
/// by NavigationGroup when using INavigationInput implementations.
/// </summary>
/// <remarks>
/// <para>This component is kept for backward compatibility but is no longer needed
/// when using NavigationGroup with proper INavigationInput implementations.</para>
///
/// <para>For new projects, use NavigationGroup with KeyboardNavigationInput or
/// GamepadNavigationInput components instead.</para>
///
/// <para>This component should be attached to any GameObject in the scene (typically the main camera
/// or event system). It works alongside Unity's EventSystem to provide keyboard accessibility
/// for UI buttons.</para>
///
/// <para>When a button is selected and the user presses Space, Enter, or Keypad Enter,
/// this component will invoke the button's onClick event, just as if it was clicked with the mouse.</para>
///
/// <para>This provides essential accessibility support for keyboard-only navigation,
/// which is important for users who cannot use a mouse or prefer keyboard navigation.</para>
/// </remarks>
/// <example>
/// <code>
/// // Modern approach (recommended):
/// var navGroup = gameObject.AddComponent&lt;NavigationGroup&gt;();
/// var keyboardInput = gameObject.AddComponent&lt;KeyboardNavigationInput&gt;();
/// navGroup.NavigationInput = keyboardInput;
///
/// // Legacy approach (deprecated):
/// var helper = Camera.main.gameObject.AddComponent&lt;ButtonNavigationHelper&gt;();
/// </code>
/// </example>
[Obsolete("ButtonNavigationHelper is deprecated. Use NavigationGroup with INavigationInput implementations instead.")]
public class ButtonNavigationHelper : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (!EventSystem.current)
            {
                return;
            }

            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;

            if (!EventSystem.current.currentSelectedGameObject)
            {
                return;
            }

            var currentSelectedButton = currentSelectedGameObject.GetComponent<Button>();

            if (!currentSelectedButton)
            {
                return;
            }

            currentSelectedButton.onClick?.Invoke();
            if (currentSelectedButton)
            {
                currentSelectedButton.Select();
            }
        }
    }
}