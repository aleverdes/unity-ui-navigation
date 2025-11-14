# Unity UI Navigation System

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![Unity 2019.1+](https://img.shields.io/badge/unity-2019.1+-black.svg)](https://unity.com/)
[![PRs Welcome](https://img.shields.io/badge/PRs-welcome-brightgreen.svg)](http://makeapullrequest.com)

A powerful, flexible, and extensible UI navigation system for Unity that provides keyboard, gamepad, and custom input navigation for your user interfaces. Supports nested groups, modal dialogs, multiple input schemes, and advanced navigation modes.

## ✨ Features

- 🚀 **Multiple Navigation Modes**: Automatic, Horizontal, Vertical, and Grid navigation
- 🎯 **Nested Navigation Groups**: Hierarchical UI navigation with proper isolation
- 🎪 **Modal Navigation**: Exclusive navigation for dialogs and popups
- 🎮 **Custom Input Systems**: Keyboard, Gamepad, VR controllers, and custom implementations
- 📱 **Cross-Platform Support**: Works on all Unity-supported platforms
- 🎨 **Smooth Animations**: Built-in support for navigation transitions
- 🔧 **Unity Editor Integration**: Custom inspectors and debugging tools
- 📚 **Comprehensive Documentation**: Full XML documentation and examples
- 🧪 **Unit Tests**: Extensive test coverage for reliability

## 📦 Installation

### Unity Package Manager (Recommended)

Add this package to your Unity project via Package Manager:

1. Open Window → Package Manager
2. Click the "+" button → "Add package from git URL"
3. Enter: `https://github.com/AleVerDes/unity-ui-navigation.git`

### Manual Installation

1. Download the latest release from [GitHub Releases](https://github.com/AleVerDes/unity-ui-navigation/releases)
2. Import the `.unitypackage` into your Unity project

## 🚀 Quick Start

### Basic Setup

1. **Add NavigationGroup** to your UI root:
   ```csharp
   var navigationGroup = uiRoot.AddComponent<NavigationGroup>();
   navigationGroup.CycleNavigation = true;
   ```

2. **Add NavigationElement** to each interactive UI element:
   ```csharp
   var button = myButton.AddComponent<NavigationElement>();
   // NavigationElement automatically finds its NavigationGroup
   ```

3. **Add Input Handler** (optional, keyboard is default):
   ```csharp
   var keyboardInput = gameObject.AddComponent<KeyboardNavigationInput>();
   navigationGroup.NavigationInput = keyboardInput;
   ```

That's it! Your UI now supports Tab/Shift+Tab navigation and Space/Enter activation.

## 📖 Documentation

### Core Components

#### NavigationGroup
Manages a collection of navigation elements and handles input processing.

**Key Properties:**
- `CycleNavigation`: Whether to wrap around at boundaries
- `NavigationMode`: Navigation behavior (Automatic, Horizontal, Vertical, Grid)
- `IsModal`: Whether this group is modal
- `NavigationInput`: Custom input handler

**Example:**
```csharp
var group = gameObject.AddComponent<NavigationGroup>();
group.NavigationMode = NavigationMode.Grid;
group.CycleNavigation = true;
```

#### NavigationElement
Represents an individual navigable UI element.

**Key Properties:**
- `Priority`: Navigation order (automatically calculated)
- `NavigationGroup`: Parent navigation group
- `Selectable`: Associated Unity Selectable component

**Example:**
```csharp
var element = button.AddComponent<NavigationElement>();
// Priority calculated automatically based on position
```

### Navigation Modes

#### Automatic Mode (Default)
Natural left-to-right, top-to-bottom navigation based on element positions.

```csharp
group.NavigationMode = NavigationMode.Automatic;
```

#### Horizontal Mode
Navigation only moves horizontally within rows.

```csharp
group.NavigationMode = NavigationMode.Horizontal;
// Tab moves to next element in same row
// Shift+Tab moves to previous element in same row
```

#### Vertical Mode
Navigation only moves vertically within columns.

```csharp
group.NavigationMode = NavigationMode.Vertical;
// Tab moves down in same column
// Shift+Tab moves up in same column
```

#### Grid Mode
2D grid navigation with row/column wrapping.

```csharp
group.NavigationMode = NavigationMode.Grid;
// Tab moves across row, then to next row
// Shift+Tab moves backward through grid
```

### Nested Groups

Create hierarchical navigation structures:

```csharp
// Parent group (main menu)
var mainMenu = mainPanel.AddComponent<NavigationGroup>();
mainMenu.NavigationMode = NavigationMode.Horizontal;

// Child group (settings panel)
var settingsPanel = settingsObject.AddComponent<NavigationGroup>();
// Automatically becomes child of mainMenu due to hierarchy
```

### Modal Navigation

Create exclusive navigation contexts:

```csharp
var dialogGroup = dialogPanel.AddComponent<NavigationGroup>();
dialogGroup.IsModal = true;

// Activate modal navigation
dialogGroup.SetModalActive(true);
// Parent groups are now disabled until modal is deactivated
```

### Custom Input

Implement `INavigationInput` for custom input schemes:

```csharp
public class CustomInput : MonoBehaviour, INavigationInput
{
    public int GetNavigationDirection()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow)) return 1;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) return -1;
        return 0;
    }

    public bool GetSubmitPressed() => Input.GetKeyDown(KeyCode.Return);
    public bool GetCancelPressed() => Input.GetKeyDown(KeyCode.Escape);
}

// Usage
var customInput = gameObject.AddComponent<CustomInput>();
navigationGroup.NavigationInput = customInput;
```

### Built-in Input Implementations

#### KeyboardNavigationInput
Standard keyboard navigation with Tab/Shift+Tab and arrow key support.

#### GamepadNavigationInput
Gamepad navigation using D-pad and face buttons.

```csharp
var gamepadInput = gameObject.AddComponent<GamepadNavigationInput>();
navigationGroup.NavigationInput = gamepadInput;
```

## 🎯 Advanced Examples

### Complex UI Layout

```csharp
// Main menu with horizontal navigation
var mainMenu = mainPanel.AddComponent<NavigationGroup>();
mainMenu.NavigationMode = NavigationMode.Horizontal;

// Settings submenu with vertical navigation
var settingsMenu = settingsPanel.AddComponent<NavigationGroup>();
settingsMenu.NavigationMode = NavigationMode.Vertical;

// Modal confirmation dialog
var confirmDialog = dialog.AddComponent<NavigationGroup>();
confirmDialog.IsModal = true;
confirmDialog.NavigationMode = NavigationMode.Horizontal;
```

### Dynamic Content

```csharp
public class DynamicMenu : MonoBehaviour
{
    private NavigationGroup _navGroup;

    void Start()
    {
        _navGroup = gameObject.AddComponent<NavigationGroup>();

        // Create menu items dynamically
        for (int i = 0; i < menuItems.Count; i++)
        {
            var button = CreateMenuButton(menuItems[i]);
            button.AddComponent<NavigationElement>();
        }

        // Recalculate priorities after adding elements
        _navGroup.Recalculate();
    }
}
```

### Navigation Callbacks

```csharp
public class MenuController : MonoBehaviour
{
    private NavigationGroup _navGroup;

    void Start()
    {
        _navGroup = GetComponent<NavigationGroup>();

        // Listen for navigation events
        // Note: Event system integration coming in future version
    }

    void Update()
    {
        // Custom input handling
        if (Input.GetKeyDown(KeyCode.F1))
        {
            FocusFirstElement();
        }
    }

    void FocusFirstElement()
    {
        if (_navGroup.ChildGroups.Count > 0)
        {
            var firstElement = _navGroup.ChildGroups[0]
                .GetComponentInChildren<NavigationElement>();
            firstElement?.Selectable.Select();
        }
    }
}
```

## 🎨 Visual Customization

### Navigation Indicators

Add visual feedback for navigation:

```csharp
public class NavigationHighlighter : MonoBehaviour
{
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color selectedColor = Color.yellow;

    private NavigationElement _navElement;
    private Graphic _graphic;

    void Awake()
    {
        _navElement = GetComponent<NavigationElement>();
        _graphic = GetComponent<Graphic>();
    }

    void Update()
    {
        if (_graphic != null)
        {
            bool isSelected = EventSystem.current.currentSelectedGameObject == gameObject;
            _graphic.color = isSelected ? selectedColor : normalColor;
        }
    }
}
```

### Smooth Transitions

```csharp
public class SmoothNavigationTransition : MonoBehaviour
{
    [SerializeField] private float transitionDuration = 0.2f;

    private NavigationElement _navElement;
    private RectTransform _rectTransform;
    private Vector3 _targetScale = Vector3.one;

    void Awake()
    {
        _navElement = GetComponent<NavigationElement>();
        _rectTransform = GetComponent<RectTransform>();
    }

    void Update()
    {
        bool isSelected = EventSystem.current.currentSelectedGameObject == gameObject;
        _targetScale = isSelected ? Vector3.one * 1.1f : Vector3.one;

        _rectTransform.localScale = Vector3.Lerp(
            _rectTransform.localScale,
            _targetScale,
            Time.deltaTime / transitionDuration
        );
    }
}
```

## 🔧 API Reference

### NavigationGroup

| Property | Type | Description |
|----------|------|-------------|
| `CycleNavigation` | bool | Whether navigation wraps at boundaries |
| `NavigationMode` | NavigationMode | Navigation behavior type |
| `IsModal` | bool | Whether this group is modal |
| `IsModalActive` | bool | Whether modal navigation is active |
| `ParentGroup` | NavigationGroup | Parent navigation group |
| `ChildGroups` | IReadOnlyList&lt;NavigationGroup&gt; | Child navigation groups |
| `EventSystem` | EventSystem | Associated EventSystem |
| `NavigationInput` | INavigationInput | Input handler |

| Method | Description |
|--------|-------------|
| `RegisterNavigationElement(element)` | Register a navigation element |
| `UnregisterNavigationElement(element)` | Unregister a navigation element |
| `Recalculate()` | Recalculate all element priorities |
| `SetModalActive(active)` | Activate/deactivate modal navigation |

### NavigationElement

| Property | Type | Description |
|----------|------|-------------|
| `Priority` | int | Navigation priority (lower = first) |
| `NavigationGroup` | NavigationGroup | Parent navigation group |
| `Selectable` | Selectable | Associated Unity Selectable |

| Method | Description |
|--------|-------------|
| `RecalculatePriority()` | Recalculate this element's priority |

### INavigationInput

| Method | Return Type | Description |
|--------|-------------|-------------|
| `GetNavigationDirection()` | int | Get navigation direction (0, 1, -1) |
| `GetSubmitPressed()` | bool | Check if submit/activate pressed |
| `GetCancelPressed()` | bool | Check if cancel/back pressed |

## 🧪 Testing

Run the included unit tests to verify functionality:

```bash
# Via Unity Test Runner
Window → General → Test Runner
# Run all tests in PlayMode
```

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guide](CONTRIBUTING.md) for details.

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests if applicable
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Thanks to the Unity community for inspiration and feedback
- Special thanks to contributors and beta testers

## 📞 Support

- 📖 [Documentation](https://github.com/AleVerDes/unity-ui-navigation/wiki)
- 🐛 [Issue Tracker](https://github.com/AleVerDes/unity-ui-navigation/issues)
- 💬 [Discussions](https://github.com/AleVerDes/unity-ui-navigation/discussions)

---

**Made with ❤️ for the Unity community**