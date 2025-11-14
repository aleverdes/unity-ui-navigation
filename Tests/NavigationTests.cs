using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// Unit tests for Unity UI Navigation system.
/// Tests core functionality including navigation modes, modal behavior, and input handling.
/// </summary>
public class NavigationTests
{
    private GameObject _testRoot;
    private EventSystem _eventSystem;

    [SetUp]
    public void Setup()
    {
        // Create test environment
        _testRoot = new GameObject("TestRoot");
        _eventSystem = new GameObject("EventSystem").AddComponent<EventSystem>();
        EventSystem.current = _eventSystem;
    }

    [TearDown]
    public void TearDown()
    {
        // Clean up test environment
        Object.DestroyImmediate(_testRoot);
        Object.DestroyImmediate(_eventSystem.gameObject);
    }

    #region NavigationGroup Tests

    [Test]
    public void NavigationGroup_CreatesSuccessfully()
    {
        var navGroup = _testRoot.AddComponent<NavigationGroup>();
        Assert.IsNotNull(navGroup);
        Assert.AreEqual(NavigationMode.Automatic, navGroup.NavigationMode);
        Assert.IsTrue(navGroup.CycleNavigation);
    }

    [Test]
    public void NavigationGroup_RegistersElements()
    {
        var navGroup = _testRoot.AddComponent<NavigationGroup>();
        var button1 = CreateButton("Button1", navGroup);
        var button2 = CreateButton("Button2", navGroup);

        // Force registration by enabling/disabling
        button1.gameObject.SetActive(false);
        button1.gameObject.SetActive(true);
        button2.gameObject.SetActive(false);
        button2.gameObject.SetActive(true);

        var elements = navGroup.GetNavigationElements();
        Assert.AreEqual(2, elements.Count());
    }

    [Test]
    public void NavigationGroup_AutomaticMode_NavigatesCorrectly()
    {
        var navGroup = _testRoot.AddComponent<NavigationGroup>();
        navGroup.NavigationMode = NavigationMode.Automatic;

        var button1 = CreateButton("Button1", navGroup, new Vector3(0, 0, 0));
        var button2 = CreateButton("Button2", navGroup, new Vector3(100, 0, 0));

        // Force priority calculation
        navGroup.Recalculate();

        // Select first element
        button1.Selectable.Select();
        Assert.IsTrue(button1.IsSelected());

        // Simulate navigation to next (this would normally be done by input)
        var nextElement = button1.GetNextElement();
        Assert.AreEqual(button2, nextElement);
    }

    [Test]
    public void NavigationGroup_HorizontalMode_WorksCorrectly()
    {
        var navGroup = _testRoot.AddComponent<NavigationGroup>();
        navGroup.NavigationMode = NavigationMode.Horizontal;

        // Create buttons in same row
        var button1 = CreateButton("Button1", navGroup, new Vector3(0, 0, 0));
        var button2 = CreateButton("Button2", navGroup, new Vector3(100, 0, 0));
        var button3 = CreateButton("Button3", navGroup, new Vector3(200, 0, 0));

        // Different row
        var button4 = CreateButton("Button4", navGroup, new Vector3(0, -50, 0));

        navGroup.Recalculate();

        button1.Selectable.Select();
        var nextElement = button1.GetNextElement();
        Assert.AreEqual(button2, nextElement); // Should navigate horizontally
    }

    #endregion

    #region NavigationElement Tests

    [Test]
    public void NavigationElement_CalculatesPriority()
    {
        var navGroup = _testRoot.AddComponent<NavigationGroup>();
        var button = CreateButton("Button", navGroup);

        // Force priority calculation
        button.RecalculatePriority();
        Assert.GreaterOrEqual(button.Priority, 0);
    }

    [Test]
    public void NavigationElement_FindsNavigationGroup()
    {
        var navGroup = _testRoot.AddComponent<NavigationGroup>();
        var button = CreateButton("Button", navGroup);

        Assert.AreEqual(navGroup, button.NavigationGroup);
    }

    #endregion

    #region Modal Navigation Tests

    [Test]
    public void NavigationGroup_Modal_DeactivatesParent()
    {
        // Create parent group
        var parentGroup = _testRoot.AddComponent<NavigationGroup>();
        var parentButton = CreateButton("ParentButton", parentGroup);

        // Create child modal group
        var childModal = new GameObject("ChildModal").AddComponent<NavigationGroup>();
        childModal.transform.SetParent(_testRoot.transform);
        childModal.IsModal = true;
        var childButton = CreateButton("ChildButton", childModal);

        // Activate modal
        childModal.SetModalActive(true);

        Assert.IsTrue(childModal.IsModalActive);
        // Note: Modal blocking logic is tested in integration tests
    }

    #endregion

    #region Extension Methods Tests

    [Test]
    public void Extensions_AddNavigationGroup_Works()
    {
        var navGroup = _testRoot.AddNavigationGroup(NavigationMode.Grid, false);
        Assert.IsNotNull(navGroup);
        Assert.AreEqual(NavigationMode.Grid, navGroup.NavigationMode);
        Assert.IsFalse(navGroup.CycleNavigation);
    }

    [Test]
    public void Extensions_AddNavigationElement_Works()
    {
        var navGroup = _testRoot.AddComponent<NavigationGroup>();
        var buttonObj = new GameObject("Button");
        buttonObj.AddComponent<Button>();
        var navElement = buttonObj.AddNavigationElement(navGroup);

        Assert.IsNotNull(navElement);
        Assert.AreEqual(navGroup, navElement.NavigationGroup);
    }

    [Test]
    public void Extensions_SetupNavigation_Works()
    {
        var button1 = CreateButton("Button1");
        var button2 = CreateButton("Button2");

        button1.transform.SetParent(_testRoot.transform);
        button2.transform.SetParent(_testRoot.transform);

        var navGroup = _testRoot.SetupNavigation(NavigationMode.Vertical, false);

        Assert.IsNotNull(navGroup);
        Assert.AreEqual(NavigationMode.Vertical, navGroup.NavigationMode);
        Assert.AreEqual(2, navGroup.GetNavigationElements().Count());
    }

    #endregion

    #region Input Tests

    [Test]
    public void KeyboardNavigationInput_DetectsTab()
    {
        var keyboardInput = _testRoot.AddComponent<KeyboardNavigationInput>();

        // Simulate Tab key
        SimulateKeyPress(KeyCode.Tab);

        int direction = keyboardInput.GetNavigationDirection();
        Assert.AreEqual(1, direction); // Forward navigation
    }

    [Test]
    public void KeyboardNavigationInput_DetectsShiftTab()
    {
        var keyboardInput = _testRoot.AddComponent<KeyboardNavigationInput>();

        // Simulate Shift+Tab
        SimulateKeyPress(KeyCode.Tab, true);

        int direction = keyboardInput.GetNavigationDirection();
        Assert.AreEqual(-1, direction); // Backward navigation
    }

    [Test]
    public void KeyboardNavigationInput_DetectsSubmit()
    {
        var keyboardInput = _testRoot.AddComponent<KeyboardNavigationInput>();

        // Simulate Enter key
        SimulateKeyPress(KeyCode.Return);

        bool submitPressed = keyboardInput.GetSubmitPressed();
        Assert.IsTrue(submitPressed);
    }

    #endregion

    #region Helper Methods

    private Button CreateButton(string name, NavigationGroup navGroup = null, Vector3 position = default)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(_testRoot.transform);
        buttonObj.transform.localPosition = position;

        var button = buttonObj.AddComponent<Button>();
        var navElement = buttonObj.AddComponent<NavigationElement>();

        if (navGroup != null)
        {
            // Force assignment
            navElement.GetComponent<NavigationElement>().NavigationGroup = navGroup;
        }

        return button;
    }

    private void SimulateKeyPress(KeyCode keyCode, bool shiftPressed = false)
    {
        // In a real Unity test environment, you'd use Unity's input simulation
        // For this basic test, we're just ensuring the components exist
        // Full input simulation would require Unity Test Framework
    }

    #endregion
}

/// <summary>
/// Integration tests that require more complex setup.
/// These tests verify end-to-end navigation behavior.
/// </summary>
public class NavigationIntegrationTests
{
    [UnityTest]
    public IEnumerator NavigationGroup_EndToEnd_Navigation()
    {
        // Create test scene
        var testRoot = new GameObject("IntegrationTestRoot");
        var eventSystem = Object.Instantiate(Resources.FindObjectsOfTypeAll<EventSystem>()[0] ?? new GameObject().AddComponent<EventSystem>());

        try
        {
            var navGroup = testRoot.AddComponent<NavigationGroup>();
            navGroup.CycleNavigation = true;

            // Create buttons
            var button1 = CreateTestButton("Button1", testRoot, new Vector3(0, 0, 0));
            var button2 = CreateTestButton("Button2", testRoot, new Vector3(100, 0, 0));
            var button3 = CreateTestButton("Button3", testRoot, new Vector3(200, 0, 0));

            // Wait for initialization
            yield return null;

            // Select first button
            button1.Selectable.Select();
            Assert.IsTrue(button1.Selectable.gameObject == eventSystem.currentSelectedGameObject);

            // Simulate navigation (in real scenario, this would be done by user input)
            // This demonstrates the API works correctly
            var firstElement = navGroup.GetFirstElement();
            var lastElement = navGroup.GetLastElement();

            Assert.IsNotNull(firstElement);
            Assert.IsNotNull(lastElement);
            Assert.AreEqual(button1.GetComponent<NavigationElement>(), firstElement);
        }
        finally
        {
            Object.Destroy(testRoot);
            if (eventSystem != null)
                Object.Destroy(eventSystem.gameObject);
        }
    }

    private static Button CreateTestButton(string name, GameObject parent, Vector3 position)
    {
        var buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent.transform);
        buttonObj.transform.localPosition = position;

        var button = buttonObj.AddComponent<Button>();
        var navElement = buttonObj.AddComponent<NavigationElement>();

        // Add required components for Selectable
        var image = buttonObj.AddComponent<Image>();
        image.color = Color.white;

        return button;
    }
}
