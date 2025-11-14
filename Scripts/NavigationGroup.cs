using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Defines the navigation behavior modes for a NavigationGroup.
/// </summary>
public enum NavigationMode
{
    /// <summary>
    /// Automatic navigation based on element positions (left-to-right, top-to-bottom).
    /// </summary>
    Automatic,

    /// <summary>
    /// Horizontal navigation only. Tab moves to next element in the same row,
    /// Shift+Tab moves to previous element in the same row.
    /// </summary>
    Horizontal,

    /// <summary>
    /// Vertical navigation only. Tab moves to next element in the same column,
    /// Shift+Tab moves to previous element in the same column.
    /// </summary>
    Vertical,

    /// <summary>
    /// Grid navigation. Elements are arranged in a 2D grid and navigation
    /// wraps within rows and columns.
    /// </summary>
    Grid
}

/// <summary>
/// Manages a group of navigation elements that can be cycled through using keyboard input.
/// Provides automatic tab-based navigation between UI elements within the group.
/// </summary>
/// <remarks>
/// <para>This component should be attached to the root object of your UI form or panel.
/// It automatically discovers NavigationElement components in its children and manages
/// the navigation flow between them.</para>
///
/// <para>Navigation order is determined by the priority calculation in NavigationElement,
/// which follows a left-to-right, top-to-bottom reading order by default.</para>
///
/// <para>Use multiple NavigationGroup instances for complex UI layouts with separate
/// navigation contexts (e.g., main menu vs. settings panel).</para>
/// </remarks>
/// <example>
/// <code>
/// // Basic setup:
/// var navigationGroup = gameObject.AddComponent&lt;NavigationGroup&gt;();
/// navigationGroup.CycleNavigation = true;
///
/// // Elements will be automatically registered when NavigationElement components are added
/// </code>
/// </example>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class NavigationGroup : MonoBehaviour
{
    [Header("Settings")]
    /// <summary>
    /// Determines whether navigation should cycle back to the beginning when reaching the end,
    /// or stop at the last/first element.
    /// </summary>
    /// <remarks>
    /// When enabled, pressing Tab on the last element will select the first element,
    /// and Shift+Tab on the first element will select the last element.
    /// When disabled, navigation stops at the boundaries.
    /// </remarks>
    [SerializeField] private bool _cycleNavigation;

    /// <summary>
    /// The navigation mode that determines how Tab/Shift+Tab behave within this group.
    /// </summary>
    /// <remarks>
    /// <para>Automatic: Natural left-to-right, top-to-bottom navigation based on element positions.</para>
    /// <para>Horizontal: Tab navigates horizontally within rows only.</para>
    /// <para>Vertical: Tab navigates vertically within columns only.</para>
    /// <para>Grid: Elements form a 2D grid with wrapping navigation in both directions.</para>
    /// </remarks>
    [SerializeField] private NavigationMode _navigationMode = NavigationMode.Automatic;

    /// <summary>
    /// When enabled, this navigation group becomes modal - it captures all navigation input
    /// and prevents parent groups from receiving navigation events until deactivated.
    /// </summary>
    /// <remarks>
    /// <para>Modal groups are useful for dialogs, popups, and other overlay UI elements
    /// that should take exclusive control of navigation.</para>
    /// <para>When a modal group is active, parent groups are temporarily disabled for navigation.</para>
    /// <para>Use SetModalActive() to control modal state programmatically.</para>
    /// </remarks>
    [SerializeField] private bool _isModal;

    [Header("Components")] 
    /// <summary>
    /// The EventSystem to use for navigation. If not set, uses EventSystem.current.
    /// </summary>
    /// <remarks>
    /// Useful when you have multiple EventSystems in your scene (e.g., for multiple canvases).
    /// </remarks>
    [SerializeField] private EventSystem _eventSystem;

    /// <summary>
    /// The input handler for navigation. If not set, uses default keyboard input.
    /// </summary>
    /// <remarks>
    /// Implement INavigationInput to create custom input schemes (keyboard, gamepad, VR controllers, etc.).
    /// </remarks>
    [SerializeField] private MonoBehaviour _navigationInput;
    
    /// <summary>
    /// Internal mapping of GameObjects to their NavigationElement components.
    /// </summary>
    private readonly Dictionary<GameObject, NavigationElement> _gameObjectsToNavigationElements = new();

    /// <summary>
    /// Internal sorted collection of navigation elements by priority.
    /// Lower priority values appear first in navigation order.
    /// </summary>
    private readonly SortedList<int, NavigationElement> _priorityToNavigationElements = new();

    /// <summary>
    /// Child navigation groups for hierarchical navigation.
    /// </summary>
    private readonly List<NavigationGroup> _childGroups = new();

    /// <summary>
    /// Parent navigation group, if this is a nested group.
    /// </summary>
    private NavigationGroup _parentGroup;

    /// <summary>
    /// Whether this modal group is currently active and capturing navigation.
    /// </summary>
    private bool _isModalActive;

    /// <summary>
    /// Cached navigation input interface.
    /// </summary>
    private INavigationInput _cachedNavigationInput;

    /// <summary>
    /// Gets the EventSystem used by this navigation group.
    /// </summary>
    /// <returns>The assigned EventSystem, or EventSystem.current if none assigned.</returns>
    public EventSystem EventSystem => _eventSystem ? _eventSystem : EventSystem.current;

    /// <summary>
    /// Gets or sets whether navigation should cycle when reaching boundaries.
    /// </summary>
    public bool CycleNavigation
    {
        get => _cycleNavigation;
        set => _cycleNavigation = value;
    }

    /// <summary>
    /// Gets or sets the navigation mode for this group.
    /// </summary>
    public NavigationMode NavigationMode
    {
        get => _navigationMode;
        set
        {
            if (_navigationMode != value)
            {
                _navigationMode = value;
                Recalculate();
            }
        }
    }

    /// <summary>
    /// Gets the parent navigation group, if this group is nested.
    /// </summary>
    public NavigationGroup ParentGroup => _parentGroup;

    /// <summary>
    /// Gets the child navigation groups.
    /// </summary>
    public IReadOnlyList<NavigationGroup> ChildGroups => _childGroups;

    /// <summary>
    /// Gets whether this navigation group is configured as modal.
    /// </summary>
    public bool IsModal => _isModal;

    /// <summary>
    /// Gets whether this modal navigation group is currently active.
    /// </summary>
    public bool IsModalActive => _isModalActive;

    /// <summary>
    /// Gets the current navigation input handler.
    /// </summary>
    /// <returns>The navigation input handler, or a default keyboard handler if none is assigned.</returns>
    public INavigationInput NavigationInput
    {
        get
        {
            if (_cachedNavigationInput == null)
            {
                _cachedNavigationInput = GetNavigationInput();
            }
            return _cachedNavigationInput;
        }
    }

    /// <summary>
    /// Activates or deactivates modal navigation for this group.
    /// </summary>
    /// <param name="active">Whether to activate modal navigation.</param>
    /// <remarks>
    /// <para>When activated, this group captures all navigation input and prevents
    /// parent groups from receiving navigation events.</para>
    /// <para>When deactivated, normal navigation hierarchy resumes.</para>
    /// <para>This method only works if the group is configured as modal (IsModal = true).</para>
    /// </remarks>
    public void SetModalActive(bool active)
    {
        if (!_isModal || _isModalActive == active)
            return;

        _isModalActive = active;

        if (active)
        {
            // Notify parent hierarchy that modal navigation is starting
            PropagateModalStateChange(true);
        }
        else
        {
            // Notify parent hierarchy that modal navigation is ending
            PropagateModalStateChange(false);
        }
    }

    /// <summary>
    /// Propagates modal state changes up the hierarchy.
    /// </summary>
    private void PropagateModalStateChange(bool modalActive)
    {
        // If activating modal, disable all ancestors
        // If deactivating modal, re-enable ancestors if no other modal siblings are active
        NavigationGroup current = _parentGroup;
        while (current != null)
        {
            if (modalActive)
            {
                // When a descendant becomes modal, this ancestor should ignore navigation
                // (handled in Update() method by checking for active modal descendants)
            }
            else
            {
                // When modal is deactivated, check if any other descendants are still modal
                bool hasActiveModalDescendant = HasActiveModalDescendant(current);
                // (this would be used to determine if navigation should be re-enabled)
            }
            current = current._parentGroup;
        }
    }

    /// <summary>
    /// Checks if this group or any of its descendants has an active modal group.
    /// </summary>
    private bool HasActiveModalDescendant(NavigationGroup group)
    {
        // Check child groups
        foreach (var child in group._childGroups)
        {
            if (child._isModalActive || HasActiveModalDescendant(child))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Gets the navigation input interface from the assigned component or creates a default.
    /// </summary>
    private INavigationInput GetNavigationInput()
    {
        if (_navigationInput != null && _navigationInput is INavigationInput input)
        {
            return input;
        }

        // Return default keyboard input
        return new KeyboardNavigationInput();
    }

    private void Awake()
    {
        // Find parent group for nested navigation
        _parentGroup = FindParentNavigationGroup();
        if (_parentGroup != null)
        {
            _parentGroup._childGroups.Add(this);
        }
    }

    private void OnDestroy()
    {
        // Remove from parent group when destroyed
        if (_parentGroup != null)
        {
            _parentGroup._childGroups.Remove(this);
        }
    }

    /// <summary>
    /// Finds the nearest parent NavigationGroup in the hierarchy.
    /// </summary>
    private NavigationGroup FindParentNavigationGroup()
    {
        Transform current = transform.parent;
        while (current != null)
        {
            var group = current.GetComponent<NavigationGroup>();
            if (group != null && group != this)
            {
                return group;
            }
            current = current.parent;
        }
        return null;
    }
    
    private void Update()
    {
        if (!EventSystem)
        {
            return;
        }

        // Check if any descendant has active modal navigation
        if (HasActiveModalDescendant(this))
        {
            return; // Don't process navigation if a descendant modal is active
        }

        // Check if this group should be disabled due to modal state
        if (ShouldDisableNavigationDueToModal())
        {
            return;
        }

        // Handle navigation input
        int navigationDirection = NavigationInput.GetNavigationDirection();
        if (navigationDirection != 0)
        {
            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            if (!currentSelectedGameObject || !_gameObjectsToNavigationElements.TryGetValue(currentSelectedGameObject, out var selectedNavigationElement))
            {
                return;
            }

            NavigateToDirection(selectedNavigationElement, navigationDirection);
        }

        // Handle submit/activate input
        if (NavigationInput.GetSubmitPressed())
        {
            var currentSelectedGameObject = EventSystem.current.currentSelectedGameObject;
            if (currentSelectedGameObject != null)
            {
                var button = currentSelectedGameObject.GetComponent<UnityEngine.UI.Button>();
                if (button != null && button.interactable)
                {
                    button.onClick?.Invoke();
                    // Re-select the button after activation
                    button.Select();
                }
            }
        }
    }

    /// <summary>
    /// Determines if this group's navigation should be disabled due to modal state.
    /// </summary>
    private bool ShouldDisableNavigationDueToModal()
    {
        // If this group itself is modal and not active, it shouldn't process navigation
        if (_isModal && !_isModalActive)
        {
            return true;
        }

        // If this group is not modal but has a modal ancestor that's not this group,
        // check if any ancestor has an active modal that would block this group
        NavigationGroup current = _parentGroup;
        while (current != null)
        {
            if (current._isModal && current._isModalActive)
            {
                return true; // Blocked by active modal ancestor
            }
            current = current._parentGroup;
        }

        return false;
    }

    /// <summary>
    /// Navigates to the next element in the specified direction based on the current navigation mode.
    /// Handles nested group navigation when reaching boundaries.
    /// </summary>
    /// <param name="currentElement">The currently selected navigation element.</param>
    /// <param name="direction">Direction of navigation (1 = forward/next, -1 = backward/previous).</param>
    private void NavigateToDirection(NavigationElement currentElement, int direction)
    {
        if (_priorityToNavigationElements.Count == 0)
            return;

        NavigationElement nextElement = null;

        switch (_navigationMode)
        {
            case NavigationMode.Automatic:
                nextElement = GetNextElementAutomatic(currentElement, direction);
                break;

            case NavigationMode.Horizontal:
                nextElement = GetNextElementHorizontal(currentElement, direction);
                break;

            case NavigationMode.Vertical:
                nextElement = GetNextElementVertical(currentElement, direction);
                break;

            case NavigationMode.Grid:
                nextElement = GetNextElementGrid(currentElement, direction);
                break;
        }

        if (nextElement != null && nextElement != currentElement)
        {
            nextElement.Selectable.Select();
        }
        else if (_cycleNavigation)
        {
            // Try to navigate to child groups or parent group
            HandleBoundaryNavigation(currentElement, direction);
        }
    }

    /// <summary>
    /// Handles navigation when reaching the boundary of the current group.
    /// Attempts to navigate to child groups or parent group.
    /// </summary>
    private void HandleBoundaryNavigation(NavigationElement currentElement, int direction)
    {
        // If we have child groups, try to navigate between them
        if (_childGroups.Count > 0)
        {
            NavigationElement boundaryElement = direction > 0 ?
                _priorityToNavigationElements.Values.Last() :
                _priorityToNavigationElements.Values.First();

            if (currentElement == boundaryElement)
            {
                // Find the appropriate child group to navigate to
                var targetGroup = direction > 0 ? _childGroups.First() : _childGroups.Last();
                if (targetGroup._priorityToNavigationElements.Count > 0)
                {
                    var targetElement = direction > 0 ?
                        targetGroup._priorityToNavigationElements.Values.First() :
                        targetGroup._priorityToNavigationElements.Values.Last();

                    targetElement.Selectable.Select();
                    return;
                }
            }
        }

        // If no child groups helped, try parent group navigation
        if (_parentGroup != null)
        {
            _parentGroup.HandleBoundaryNavigationFromChild(this, direction);
        }
    }

    /// <summary>
    /// Handles navigation requests from child groups.
    /// </summary>
    private void HandleBoundaryNavigationFromChild(NavigationGroup childGroup, int direction)
    {
        int childIndex = _childGroups.IndexOf(childGroup);
        if (childIndex == -1) return;

        if (direction > 0) // Child wants to go forward
        {
            // Try next sibling child group
            if (childIndex < _childGroups.Count - 1)
            {
                var nextGroup = _childGroups[childIndex + 1];
                if (nextGroup._priorityToNavigationElements.Count > 0)
                {
                    nextGroup._priorityToNavigationElements.Values.First().Selectable.Select();
                    return;
                }
            }

            // Try our own elements after the child groups
            if (_priorityToNavigationElements.Count > 0)
            {
                _priorityToNavigationElements.Values.First().Selectable.Select();
                return;
            }
        }
        else // Child wants to go backward
        {
            // Try previous sibling child group
            if (childIndex > 0)
            {
                var prevGroup = _childGroups[childIndex - 1];
                if (prevGroup._priorityToNavigationElements.Count > 0)
                {
                    prevGroup._priorityToNavigationElements.Values.Last().Selectable.Select();
                    return;
                }
            }

            // Try our own elements before the child groups
            if (_priorityToNavigationElements.Count > 0)
            {
                _priorityToNavigationElements.Values.Last().Selectable.Select();
                return;
            }
        }

        // If we can't handle it, pass to parent
        if (_parentGroup != null)
        {
            _parentGroup.HandleBoundaryNavigationFromChild(this, direction);
        }
    }

    /// <summary>
    /// Gets the next element in automatic mode (linear priority-based navigation).
    /// </summary>
    private NavigationElement GetNextElementAutomatic(NavigationElement currentElement, int direction)
    {
        var currentIndex = _priorityToNavigationElements.IndexOfKey(currentElement.Priority);

        if (direction > 0) // Forward
        {
            if (currentIndex == _priorityToNavigationElements.Count - 1)
            {
                return _cycleNavigation ? _priorityToNavigationElements.Values[0] : currentElement;
            }
            return _priorityToNavigationElements.Values[currentIndex + 1];
        }
        else // Backward
        {
            if (currentIndex == 0)
            {
                return _cycleNavigation ? _priorityToNavigationElements.Values[_priorityToNavigationElements.Count - 1] : currentElement;
            }
            return _priorityToNavigationElements.Values[currentIndex - 1];
        }
    }

    /// <summary>
    /// Gets the next element in horizontal mode (same row navigation).
    /// </summary>
    private NavigationElement GetNextElementHorizontal(NavigationElement currentElement, int direction)
    {
        // Group elements by their Y position (row)
        var elementsByRow = GroupElementsByRow();

        // Find current element's row
        float currentY = GetElementScreenY(currentElement);
        if (!elementsByRow.TryGetValue(currentY, out var rowElements))
            return GetNextElementAutomatic(currentElement, direction); // Fallback

        // Sort row elements by X position
        rowElements.Sort((a, b) => GetElementScreenX(a).CompareTo(GetElementScreenX(b)));

        int currentIndex = rowElements.IndexOf(currentElement);
        if (currentIndex == -1)
            return GetNextElementAutomatic(currentElement, direction); // Fallback

        if (direction > 0) // Forward
        {
            if (currentIndex == rowElements.Count - 1)
            {
                return _cycleNavigation ? rowElements[0] : currentElement;
            }
            return rowElements[currentIndex + 1];
        }
        else // Backward
        {
            if (currentIndex == 0)
            {
                return _cycleNavigation ? rowElements[rowElements.Count - 1] : currentElement;
            }
            return rowElements[currentIndex - 1];
        }
    }

    /// <summary>
    /// Gets the next element in vertical mode (same column navigation).
    /// </summary>
    private NavigationElement GetNextElementVertical(NavigationElement currentElement, int direction)
    {
        // Group elements by their X position (column)
        var elementsByColumn = GroupElementsByColumn();

        // Find current element's column
        float currentX = GetElementScreenX(currentElement);
        if (!elementsByColumn.TryGetValue(currentX, out var columnElements))
            return GetNextElementAutomatic(currentElement, direction); // Fallback

        // Sort column elements by Y position (top to bottom)
        columnElements.Sort((a, b) => GetElementScreenY(a).CompareTo(GetElementScreenY(b)));

        int currentIndex = columnElements.IndexOf(currentElement);
        if (currentIndex == -1)
            return GetNextElementAutomatic(currentElement, direction); // Fallback

        if (direction > 0) // Forward (down)
        {
            if (currentIndex == columnElements.Count - 1)
            {
                return _cycleNavigation ? columnElements[0] : currentElement;
            }
            return columnElements[currentIndex + 1];
        }
        else // Backward (up)
        {
            if (currentIndex == 0)
            {
                return _cycleNavigation ? columnElements[columnElements.Count - 1] : currentElement;
            }
            return columnElements[currentIndex - 1];
        }
    }

    /// <summary>
    /// Gets the next element in grid mode (2D navigation with wrapping).
    /// </summary>
    private NavigationElement GetNextElementGrid(NavigationElement currentElement, int direction)
    {
        // For grid mode, treat tab as moving to next element in reading order within the current row,
        // or to the first element of the next row if at end of current row
        var elementsByRow = GroupElementsByRow();
        var sortedRows = elementsByRow.Keys.OrderBy(y => y).ToList();

        float currentY = GetElementScreenY(currentElement);
        int currentRowIndex = sortedRows.IndexOf(currentY);

        if (currentRowIndex == -1)
            return GetNextElementAutomatic(currentElement, direction); // Fallback

        var currentRow = elementsByRow[currentY];
        currentRow.Sort((a, b) => GetElementScreenX(a).CompareTo(GetElementScreenX(b)));

        int currentColIndex = currentRow.IndexOf(currentElement);
        if (currentColIndex == -1)
            return GetNextElementAutomatic(currentElement, direction); // Fallback

        if (direction > 0) // Forward
        {
            // Try next element in current row
            if (currentColIndex < currentRow.Count - 1)
            {
                return currentRow[currentColIndex + 1];
            }
            // Move to next row
            else if (currentRowIndex < sortedRows.Count - 1)
            {
                return elementsByRow[sortedRows[currentRowIndex + 1]][0];
            }
            // Wrap to first element
            else if (_cycleNavigation)
            {
                return elementsByRow[sortedRows[0]][0];
            }
            return currentElement;
        }
        else // Backward
        {
            // Try previous element in current row
            if (currentColIndex > 0)
            {
                return currentRow[currentColIndex - 1];
            }
            // Move to previous row
            else if (currentRowIndex > 0)
            {
                var prevRow = elementsByRow[sortedRows[currentRowIndex - 1]];
                return prevRow[prevRow.Count - 1];
            }
            // Wrap to last element
            else if (_cycleNavigation)
            {
                var lastRow = elementsByRow[sortedRows[sortedRows.Count - 1]];
                return lastRow[lastRow.Count - 1];
            }
            return currentElement;
        }
    }

    /// <summary>
    /// Groups navigation elements by their Y position (row).
    /// </summary>
    private Dictionary<float, List<NavigationElement>> GroupElementsByRow()
    {
        var groups = new Dictionary<float, List<NavigationElement>>();
        float tolerance = 10f; // Pixels tolerance for considering elements in same row

        foreach (var element in _priorityToNavigationElements.Values)
        {
            float y = GetElementScreenY(element);
            // Find existing group within tolerance
            float? matchingKey = null;
            foreach (var key in groups.Keys)
            {
                if (Mathf.Abs(key - y) <= tolerance)
                {
                    matchingKey = key;
                    break;
                }
            }

            if (matchingKey.HasValue)
            {
                groups[matchingKey.Value].Add(element);
            }
            else
            {
                groups[y] = new List<NavigationElement> { element };
            }
        }

        return groups;
    }

    /// <summary>
    /// Groups navigation elements by their X position (column).
    /// </summary>
    private Dictionary<float, List<NavigationElement>> GroupElementsByColumn()
    {
        var groups = new Dictionary<float, List<NavigationElement>>();
        float tolerance = 10f; // Pixels tolerance for considering elements in same column

        foreach (var element in _priorityToNavigationElements.Values)
        {
            float x = GetElementScreenX(element);
            // Find existing group within tolerance
            float? matchingKey = null;
            foreach (var key in groups.Keys)
            {
                if (Mathf.Abs(key - x) <= tolerance)
                {
                    matchingKey = key;
                    break;
                }
            }

            if (matchingKey.HasValue)
            {
                groups[matchingKey.Value].Add(element);
        }
        else
        {
                groups[x] = new List<NavigationElement> { element };
            }
        }

        return groups;
    }

    /// <summary>
    /// Gets the screen X coordinate of a navigation element.
    /// </summary>
    private float GetElementScreenX(NavigationElement element)
    {
        Vector3 screenPos = element.transform.position;
        Canvas canvas = element.GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Camera cam = canvas.worldCamera ?? Camera.main;
            if (cam)
            {
                screenPos = cam.WorldToScreenPoint(element.transform.position);
            }
        }
        return screenPos.x;
    }

    /// <summary>
    /// Gets the screen Y coordinate of a navigation element.
    /// </summary>
    private float GetElementScreenY(NavigationElement element)
    {
        Vector3 screenPos = element.transform.position;
        Canvas canvas = element.GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Camera cam = canvas.worldCamera ?? Camera.main;
            if (cam)
            {
                screenPos = cam.WorldToScreenPoint(element.transform.position);
            }
        }
        return screenPos.y;
    }

    /// <summary>
    /// Registers a NavigationElement with this navigation group.
    /// </summary>
    /// <param name="navigationElement">The NavigationElement to register.</param>
    /// <returns>True if registration was successful, false if priority conflict occurred.</returns>
    /// <remarks>
    /// Registration fails if another element with the same priority already exists.
    /// Elements are automatically registered when enabled, so this method is typically
    /// not called directly.
    /// </remarks>
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

    /// <summary>
    /// Unregisters a NavigationElement from this navigation group.
    /// </summary>
    /// <param name="navigationElement">The NavigationElement to unregister.</param>
    /// <returns>True if the element was successfully unregistered, false otherwise.</returns>
    /// <remarks>
    /// Elements are automatically unregistered when disabled, so this method is typically
    /// not called directly.
    /// </remarks>
    public bool UnregisterNavigationElement(NavigationElement navigationElement)
    {
        _gameObjectsToNavigationElements.Remove(navigationElement.gameObject);

        return _priorityToNavigationElements.Remove(navigationElement.Priority);
    }

    /// <summary>
    /// Forces recalculation of navigation priorities for all registered elements.
    /// </summary>
    /// <remarks>
    /// Call this method when the layout changes significantly (e.g., after repositioning
    /// elements or changing canvas scaling). Priorities are normally calculated automatically
    /// when elements are enabled.
    /// </remarks>
    public void Recalculate()
    {
        var navigationElements = _gameObjectsToNavigationElements.Values.ToArray();
        foreach (var navigationElement in navigationElements)
        {
            navigationElement.RecalculatePriority();
        }
    }
}