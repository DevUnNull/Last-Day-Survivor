# AnimationUI

AnimationUI is a Unity tool to create UI animations easily with no code. You can simply drag and drop to create smooth UI animations with easing, duration control, and multi-step sequences.

## Features
- Animate values of `RectTransform`, `Image`, `Camera`, `CanvasGroup`, `Transform`, `TextMeshPro`, and `UnityEventDynamic`.
- Instant action sequences: Set Active GameObject, Play SFX, Wait, Load Scene, UnityEvent.
- Edit-mode preview with progress bar.
- Reorderable sequences.
- Easy API for calling via C# code.

## Quick Start
1. Right click in Hierarchy -> **UI > Create AnimationUI** (or add `AnimationUI` component to any GameObject).
2. Add sequences in the Inspector, choose target component and animation type.
3. Use **Set Start** and **Set End** to capture properties.
4. Drag the progress bar or press **Preview Animation** to test in Edit Mode.

## C# Usage
```csharp
using DhafinFawwaz.AnimationUILib;

public class MyMenu : MonoBehaviour
{
    [SerializeField] private AnimationUI _animationUI;

    public void OpenMenu()
    {
        _animationUI.Play();
    }

    public void CloseMenu()
    {
        _animationUI.PlayReversed();
    }
}
```

## Structure
- `Script/`: Core runtime classes (`AnimationUI`, `Sequence`, `Ease`, `Customizable`) + `DhafinFawwaz.AnimationUI.asmdef`
- `Editor/`: Custom Inspector and Property Drawers + `DhafinFawwaz.AnimationUI.Editor.asmdef`
- `Demo/`: Example scenes, scripts, and assets + asmdef files
- `package.json`: Unity Package Manager manifest
