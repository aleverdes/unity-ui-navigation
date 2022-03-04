using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class NavigationGroup : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool _cycleNavigation;

    [Header("Components")] 
    [SerializeField] private EventSystem _eventSystem;
    
    private readonly Dictionary<GameObject, NavigationElement> _gameObjectsToNavigationElements = new();
    private readonly SortedList<int, NavigationElement> _priorityToNavigationElements = new();

    public EventSystem EventSystem => _eventSystem ? _eventSystem : EventSystem.current;
    
    private void Update()
    {
        if (!EventSystem)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            var altIsPressed = Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt);
            var ctrlIsPressed = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (altIsPressed || ctrlIsPressed)
            {
                return;
            }

            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            if (!currentSelectedGameObject || !_gameObjectsToNavigationElements.TryGetValue(currentSelectedGameObject, out var selectedNavigationElement))
            {
                return;
            }

            var shiftIsPressed = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
            if (shiftIsPressed)
            {
                SelectPreviousNavigationElement(selectedNavigationElement);
            }
            else
            {
                SelectNextNavigationElement(selectedNavigationElement);
            }
        }
    }

    private void SelectNextNavigationElement(NavigationElement currentNavigationElement)
    {
        int nextNavigationElementIndex;

        var currentNavigationElementIndex = _priorityToNavigationElements.IndexOfKey(currentNavigationElement.Priority);
        if (currentNavigationElementIndex == _priorityToNavigationElements.Keys.Count - 1)
        {
            nextNavigationElementIndex = _cycleNavigation ? 0 : _priorityToNavigationElements.Keys.Count - 1;
        }
        else
        {
            nextNavigationElementIndex = currentNavigationElementIndex + 1;
        }

        _priorityToNavigationElements.Values[nextNavigationElementIndex].Selectable.Select();
    }

    private void SelectPreviousNavigationElement(NavigationElement currentNavigationElement)
    {
        int prevNavigationElementIndex;

        var currentNavigationElementIndex = _priorityToNavigationElements.IndexOfKey(currentNavigationElement.Priority);
        if (currentNavigationElementIndex == 0)
        {
            prevNavigationElementIndex = _cycleNavigation ? _priorityToNavigationElements.Keys.Count - 1 : 0;
        }
        else
        {
            prevNavigationElementIndex = currentNavigationElementIndex - 1;
        }

        _priorityToNavigationElements.Values[prevNavigationElementIndex].Selectable.Select();
    }

    public bool RegisterNavigationElement(NavigationElement navigationElement)
    {
        _gameObjectsToNavigationElements.Add(navigationElement.gameObject, navigationElement);

        if (_priorityToNavigationElements.ContainsKey(navigationElement.Priority))
        {
            return false;
        }

        _priorityToNavigationElements.Add(navigationElement.Priority, navigationElement);
        return true;
    }

    public bool UnregisterNavigationElement(NavigationElement navigationElement)
    {
        _gameObjectsToNavigationElements.Remove(navigationElement.gameObject);

        return _priorityToNavigationElements.Remove(navigationElement.Priority);
    }

    public void Recalculate()
    {
        var navigationElements = _gameObjectsToNavigationElements.Values.ToArray();
        foreach (var navigationElement in navigationElements)
        {
            navigationElement.RecalculatePriority();
        }
    }
}