using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Selectable))]
[DisallowMultipleComponent]
public class NavigationElement : MonoBehaviour
{
    private static readonly WaitForEndOfFrame WaitForEndOfFrame = new();

    [Header("Components")] [SerializeField]
    private RectTransform _rectTransform;

    [SerializeField] private Selectable _selectable;
    [SerializeField] private NavigationGroup _navigationGroup;

    public NavigationGroup NavigationGroup => _navigationGroup;
    public Selectable Selectable => _selectable;

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
        Priority = Mathf.RoundToInt(_rectTransform.position.x - Screen.width * (Screen.height + _rectTransform.position.y));
        _navigationGroup.RegisterNavigationElement(this);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _navigationGroup.UnregisterNavigationElement(this);
    }

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

    private void OnCanvasGroupChanged()
    {
        RecalculatePriority();
    }
}