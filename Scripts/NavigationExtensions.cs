using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Extension methods for Unity UI Navigation components.
/// Provides convenient shortcuts and utilities for common navigation operations.
/// </summary>
public static class NavigationExtensions
{
    #region NavigationGroup Extensions

    /// <summary>
    /// Adds a NavigationGroup component to the GameObject with automatic setup.
    /// </summary>
    /// <param name="gameObject">The GameObject to add the NavigationGroup to.</param>
    /// <param name="navigationMode">The navigation mode to use.</param>
    /// <param name="cycleNavigation">Whether navigation should cycle at boundaries.</param>
    /// <returns>The added NavigationGroup component.</returns>
    public static NavigationGroup AddNavigationGroup(
        this GameObject gameObject,
        NavigationMode navigationMode = NavigationMode.Automatic,
        bool cycleNavigation = true)
    {
        var navGroup = gameObject.AddComponent<NavigationGroup>();
        navGroup.NavigationMode = navigationMode;
        navGroup.CycleNavigation = cycleNavigation;
        return navGroup;
    }

    /// <summary>
    /// Gets all NavigationElement components in the children of this GameObject.
    /// </summary>
    /// <param name="navGroup">The NavigationGroup to get elements from.</param>
    /// <returns>An enumerable of NavigationElement components.</returns>
    public static IEnumerable<NavigationElement> GetNavigationElements(this NavigationGroup navGroup)
    {
        return navGroup.GetComponentsInChildren<NavigationElement>(true)
            .Where(element => element.NavigationGroup == navGroup);
    }

    /// <summary>
    /// Finds the NavigationElement with the specified priority.
    /// </summary>
    /// <param name="navGroup">The NavigationGroup to search in.</param>
    /// <param name="priority">The priority to search for.</param>
    /// <returns>The NavigationElement with the specified priority, or null if not found.</returns>
    public static NavigationElement FindElementByPriority(this NavigationGroup navGroup, int priority)
    {
        return navGroup.GetNavigationElements()
            .FirstOrDefault(element => element.Priority == priority);
    }

    /// <summary>
    /// Gets the first NavigationElement in navigation order.
    /// </summary>
    /// <param name="navGroup">The NavigationGroup to get the first element from.</param>
    /// <returns>The first NavigationElement, or null if the group is empty.</returns>
    public static NavigationElement GetFirstElement(this NavigationGroup navGroup)
    {
        return navGroup.GetNavigationElements()
            .OrderBy(element => element.Priority)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the last NavigationElement in navigation order.
    /// </summary>
    /// <param name="navGroup">The NavigationGroup to get the last element from.</param>
    /// <returns>The last NavigationElement, or null if the group is empty.</returns>
    public static NavigationElement GetLastElement(this NavigationGroup navGroup)
    {
        return navGroup.GetNavigationElements()
            .OrderByDescending(element => element.Priority)
            .FirstOrDefault();
    }

    /// <summary>
    /// Selects the first element in this navigation group.
    /// </summary>
    /// <param name="navGroup">The NavigationGroup to select the first element in.</param>
    /// <returns>True if an element was selected, false otherwise.</returns>
    public static bool SelectFirst(this NavigationGroup navGroup)
    {
        var firstElement = navGroup.GetFirstElement();
        if (firstElement != null)
        {
            firstElement.Selectable.Select();
            return true;
        }
        return false;
    }

    /// <summary>
    /// Selects the last element in this navigation group.
    /// </summary>
    /// <param name="navGroup">The NavigationGroup to select the last element in.</param>
    /// <returns>True if an element was selected, false otherwise.</returns>
    public static bool SelectLast(this NavigationGroup navGroup)
    {
        var lastElement = navGroup.GetLastElement();
        if (lastElement != null)
        {
            lastElement.Selectable.Select();
            return true;
        }
        return false;
    }

    #endregion

    #region NavigationElement Extensions

    /// <summary>
    /// Adds a NavigationElement component to the GameObject with automatic setup.
    /// </summary>
    /// <param name="gameObject">The GameObject to add the NavigationElement to.</param>
    /// <param name="navigationGroup">Optional specific NavigationGroup to assign to.</param>
    /// <returns>The added NavigationElement component.</returns>
    public static NavigationElement AddNavigationElement(
        this GameObject gameObject,
        NavigationGroup navigationGroup = null)
    {
        var navElement = gameObject.AddComponent<NavigationElement>();
        if (navigationGroup != null)
        {
            // Note: NavigationElement automatically finds its group,
            // but we can set it explicitly if needed
            navElement.GetComponent<NavigationElement>().NavigationGroup = navigationGroup;
        }
        return navElement;
    }

    /// <summary>
    /// Selects this navigation element.
    /// </summary>
    /// <param name="navElement">The NavigationElement to select.</param>
    public static void Select(this NavigationElement navElement)
    {
        navElement.Selectable.Select();
    }

    /// <summary>
    /// Checks if this navigation element is currently selected.
    /// </summary>
    /// <param name="navElement">The NavigationElement to check.</param>
    /// <returns>True if the element is selected, false otherwise.</returns>
    public static bool IsSelected(this NavigationElement navElement)
    {
        return UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject == navElement.gameObject;
    }

    /// <summary>
    /// Gets the next NavigationElement in the same group.
    /// </summary>
    /// <param name="navElement">The NavigationElement to get the next element for.</param>
    /// <returns>The next NavigationElement, or null if at the end.</returns>
    public static NavigationElement GetNextElement(this NavigationElement navElement)
    {
        var navGroup = navElement.NavigationGroup;
        if (navGroup == null) return null;

        return navGroup.GetNavigationElements()
            .Where(element => element.Priority > navElement.Priority)
            .OrderBy(element => element.Priority)
            .FirstOrDefault();
    }

    /// <summary>
    /// Gets the previous NavigationElement in the same group.
    /// </summary>
    /// <param name="navElement">The NavigationElement to get the previous element for.</param>
    /// <returns>The previous NavigationElement, or null if at the beginning.</returns>
    public static NavigationElement GetPreviousElement(this NavigationElement navElement)
    {
        var navGroup = navElement.NavigationGroup;
        if (navGroup == null) return null;

        return navGroup.GetNavigationElements()
            .Where(element => element.Priority < navElement.Priority)
            .OrderByDescending(element => element.Priority)
            .FirstOrDefault();
    }

    #endregion

    #region GameObject Extensions

    /// <summary>
    /// Sets up a complete navigation system on a GameObject and its children.
    /// Adds NavigationGroup to the root and NavigationElement to all Selectable children.
    /// </summary>
    /// <param name="rootObject">The root GameObject to set up navigation for.</param>
    /// <param name="navigationMode">The navigation mode to use.</param>
    /// <param name="cycleNavigation">Whether navigation should cycle at boundaries.</param>
    /// <returns>The NavigationGroup component added to the root.</returns>
    public static NavigationGroup SetupNavigation(
        this GameObject rootObject,
        NavigationMode navigationMode = NavigationMode.Automatic,
        bool cycleNavigation = true)
    {
        // Add NavigationGroup to root
        var navGroup = rootObject.AddNavigationGroup(navigationMode, cycleNavigation);

        // Add NavigationElement to all Selectable components in children
        foreach (var selectable in rootObject.GetComponentsInChildren<Selectable>(true))
        {
            if (selectable.gameObject.GetComponent<NavigationElement>() == null)
            {
                selectable.gameObject.AddNavigationElement(navGroup);
            }
        }

        return navGroup;
    }

    /// <summary>
    /// Sets up navigation for a collection of GameObjects.
    /// Creates a NavigationGroup on the first object and adds NavigationElement to all objects.
    /// </summary>
    /// <param name="gameObjects">The GameObjects to set up navigation for.</param>
    /// <param name="navigationMode">The navigation mode to use.</param>
    /// <param name="cycleNavigation">Whether navigation should cycle at boundaries.</param>
    /// <returns>The NavigationGroup component created.</returns>
    public static NavigationGroup SetupNavigation(
        this IEnumerable<GameObject> gameObjects,
        NavigationMode navigationMode = NavigationMode.Automatic,
        bool cycleNavigation = true)
    {
        var objectsList = gameObjects.ToList();
        if (!objectsList.Any()) return null;

        // Create NavigationGroup on the first object
        var navGroup = objectsList[0].AddNavigationGroup(navigationMode, cycleNavigation);

        // Add NavigationElement to all objects
        foreach (var obj in objectsList)
        {
            if (obj.GetComponent<NavigationElement>() == null)
            {
                obj.AddNavigationElement(navGroup);
            }
        }

        return navGroup;
    }

    #endregion

    #region Selectable Extensions

    /// <summary>
    /// Adds navigation support to a Selectable component.
    /// </summary>
    /// <param name="selectable">The Selectable to add navigation to.</param>
    /// <param name="navigationGroup">Optional specific NavigationGroup to assign to.</param>
    /// <returns>The NavigationElement component added.</returns>
    public static NavigationElement AddNavigation(this Selectable selectable, NavigationGroup navigationGroup = null)
    {
        return selectable.gameObject.AddNavigationElement(navigationGroup);
    }

    /// <summary>
    /// Checks if a Selectable has navigation support.
    /// </summary>
    /// <param name="selectable">The Selectable to check.</param>
    /// <returns>True if the Selectable has a NavigationElement, false otherwise.</returns>
    public static bool HasNavigation(this Selectable selectable)
    {
        return selectable.GetComponent<NavigationElement>() != null;
    }

    #endregion

    #region Utility Extensions

    /// <summary>
    /// Creates a navigation grid from a 2D array of GameObjects.
    /// Each GameObject should have a Selectable component.
    /// </summary>
    /// <param name="gridObjects">2D array of GameObjects to create navigation for.</param>
    /// <param name="rootObject">The GameObject to add the NavigationGroup to.</param>
    /// <param name="cycleNavigation">Whether navigation should cycle at boundaries.</param>
    /// <returns>The NavigationGroup component created.</returns>
    public static NavigationGroup CreateNavigationGrid(
        this GameObject[,] gridObjects,
        GameObject rootObject,
        bool cycleNavigation = true)
    {
        var navGroup = rootObject.AddNavigationGroup(NavigationMode.Grid, cycleNavigation);

        int rows = gridObjects.GetLength(0);
        int cols = gridObjects.GetLength(1);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var obj = gridObjects[row, col];
                if (obj != null && obj.GetComponent<Selectable>() != null)
                {
                    obj.AddNavigationElement(navGroup);
                }
            }
        }

        return navGroup;
    }

    /// <summary>
    /// Creates a navigation menu from a list of buttons with vertical layout.
    /// </summary>
    /// <param name="buttons">The buttons to create navigation for.</param>
    /// <param name="rootObject">The GameObject to add the NavigationGroup to.</param>
    /// <param name="cycleNavigation">Whether navigation should cycle at boundaries.</param>
    /// <returns>The NavigationGroup component created.</returns>
    public static NavigationGroup CreateVerticalMenu(
        this IEnumerable<Button> buttons,
        GameObject rootObject,
        bool cycleNavigation = true)
    {
        var navGroup = rootObject.AddNavigationGroup(NavigationMode.Vertical, cycleNavigation);

        foreach (var button in buttons)
        {
            if (button.GetComponent<NavigationElement>() == null)
            {
                button.gameObject.AddNavigationElement(navGroup);
            }
        }

        return navGroup;
    }

    /// <summary>
    /// Creates a navigation toolbar from a list of buttons with horizontal layout.
    /// </summary>
    /// <param name="buttons">The buttons to create navigation for.</param>
    /// <param name="rootObject">The GameObject to add the NavigationGroup to.</param>
    /// <param name="cycleNavigation">Whether navigation should cycle at boundaries.</param>
    /// <returns>The NavigationGroup component created.</returns>
    public static NavigationGroup CreateHorizontalToolbar(
        this IEnumerable<Button> buttons,
        GameObject rootObject,
        bool cycleNavigation = true)
    {
        var navGroup = rootObject.AddNavigationGroup(NavigationMode.Horizontal, cycleNavigation);

        foreach (var button in buttons)
        {
            if (button.GetComponent<NavigationElement>() == null)
            {
                button.gameObject.AddNavigationElement(navGroup);
            }
        }

        return navGroup;
    }

    #endregion
}
