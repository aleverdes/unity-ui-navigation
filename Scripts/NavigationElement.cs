using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a selectable UI element that participates in navigation.
/// Automatically calculates its navigation priority based on position and registers
/// with the nearest parent NavigationGroup.
/// </summary>
/// <remarks>
/// <para>This component should be attached to any UI element that should be navigable
/// (Button, InputField, Toggle, Slider, etc.). It works with Unity's Selectable components.</para>
///
/// <para>Navigation priority determines the order in which elements are selected when
/// navigating with Tab/Shift+Tab. Priority is calculated automatically based on the
/// element's position using a left-to-right, top-to-bottom reading order.</para>
///
/// <para>Elements are automatically managed - they register with their NavigationGroup
/// when enabled and unregister when disabled.</para>
/// </remarks>
/// <example>
/// <code>
/// // Automatic setup (recommended):
/// var button = gameObject.AddComponent&lt;Button&gt;();
/// var navElement = gameObject.AddComponent&lt;NavigationElement&gt;();
/// // NavigationGroup is found automatically via GetComponentInParent
///
/// // Manual setup (advanced):
/// navElement.NavigationGroup = someSpecificGroup;
/// </code>
/// </example>
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Selectable))]
[DisallowMultipleComponent]
public class NavigationElement : MonoBehaviour
{
    /// <summary>
    /// Shared WaitForEndOfFrame instance for coroutines.
    /// </summary>
    private static readonly WaitForEndOfFrame WaitForEndOfFrame = new();

    [Header("Components")]
    /// <summary>
    /// The RectTransform of this element. Used for position-based priority calculation.
    /// </summary>
    [SerializeField]
    private RectTransform _rectTransform;

    /// <summary>
    /// The Selectable component that will be controlled by navigation.
    /// </summary>
    [SerializeField] private Selectable _selectable;

    /// <summary>
    /// The NavigationGroup this element belongs to. If not set, automatically finds
    /// the nearest parent NavigationGroup.
    /// </summary>
    [SerializeField] private NavigationGroup _navigationGroup;

    /// <summary>
    /// Gets the NavigationGroup this element is registered with.
    /// </summary>
    public NavigationGroup NavigationGroup => _navigationGroup;

    /// <summary>
    /// Gets the Selectable component associated with this navigation element.
    /// </summary>
    public Selectable Selectable => _selectable;

    /// <summary>
    /// Gets the navigation priority of this element.
    /// Lower values appear first in navigation order.
    /// </summary>
    /// <remarks>
    /// Priority is calculated automatically based on position using a left-to-right,
    /// top-to-bottom reading order. Priority 0 is the first element in navigation order.
    /// </remarks>
    public int Priority { get; private set; }

    private void Reset()
    {
        _rectTransform = (RectTransform)transform;
        _selectable = GetComponent<Selectable>();
        _navigationGroup = GetComponentInParent<NavigationGroup>(true);
    }

    private void OnEnable()
    {
        StartCoroutine(RegisterNavigationElement());
    }

    private IEnumerator RegisterNavigationElement()
    {
        yield return null;
        yield return WaitForEndOfFrame;
        Priority = CalculateNavigationPriority();
        _navigationGroup.RegisterNavigationElement(this);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _navigationGroup.UnregisterNavigationElement(this);
    }

    /// <summary>
    /// Forces recalculation of this element's navigation priority.
    /// </summary>
    /// <remarks>
    /// Call this method when the element's position or layout has changed significantly.
    /// The element will be temporarily unregistered and re-registered with its NavigationGroup
    /// to recalculate its priority based on current position.
    /// </remarks>
    [ContextMenu("Recalculate priority")]
    public void RecalculatePriority()
    {
        if (!isActiveAndEnabled)
        {
            return;
        }

        OnDisable();
        OnEnable();
    }

    /// <summary>
    /// Calculates the navigation priority based on element position.
    /// Uses left-to-right, top-to-bottom reading order.
    /// </summary>
    /// <returns>The calculated priority value (lower = appears first in navigation).</returns>
    /// <remarks>
    /// Priority is calculated by converting the element's position to screen coordinates
    /// and creating a Morton code (Z-order curve) for proper 2D ordering.
    /// This ensures natural reading order: left-to-right, then top-to-bottom.
    /// </remarks>
    private int CalculateNavigationPriority()
    {
        // Convert local position to screen position for consistent ordering
        Vector3 screenPos = _rectTransform.position;

        // Get canvas for proper coordinate conversion if available
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Camera cam = canvas.worldCamera ?? Camera.main;
            if (cam)
            {
                screenPos = cam.WorldToScreenPoint(_rectTransform.position);
            }
        }

        // Use Morton code (Z-order curve) for proper 2D ordering
        // This interleaves X and Y coordinates for natural reading order
        int x = Mathf.RoundToInt(screenPos.x);
        int y = Mathf.RoundToInt(Screen.height - screenPos.y); // Flip Y for top-to-bottom

        // Simple Morton code implementation for 2D ordering
        // Interleave bits of x and y coordinates
        int priority = 0;
        for (int i = 0; i < 16; i++) // 16 bits for reasonable coordinate range
        {
            priority |= ((x >> i) & 1) << (i * 2);
            priority |= ((y >> i) & 1) << (i * 2 + 1);
        }

        return priority;
    }

    private void OnCanvasGroupChanged()
    {
        RecalculatePriority();
    }
}