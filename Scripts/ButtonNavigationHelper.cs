using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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