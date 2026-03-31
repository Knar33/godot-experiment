# Settings Architecture

## Code Split

- **`src/GodotExperiment.Core/Settings/SettingsData.cs`** — Pure C# data class holding all settings values. No Godot dependency. Namespace: `GodotExperiment.Settings`.
- **`scripts/managers/SettingsManager.cs`** — Godot autoload. Loads/saves `SettingsData` as JSON to `user://settings.json`. Applies settings to Godot systems (AudioServer bus volumes, DisplayServer window mode, Input remaps). Exposes `Current` property for other scripts to read.
- **`scripts/ui/SettingsMenu.cs`** — Godot `Control` script. Builds the settings UI, binds controls to `SettingsData` fields, calls `SettingsManager.Save()` on change.

## SettingsData (Core)

All fields have defaults. Missing fields in a saved JSON are filled from defaults on load (forward-compatible).

```csharp
public class SettingsData
{
    // Controls
    public float MouseSensitivity = 0.002f;
    public bool InvertY = false;
    public float GamepadDeadzone = 0.15f;
    public Dictionary<string, string> KeyBindings = new(); // action -> key scancode

    // Audio
    public float MasterVolume = 1.0f;
    public float MusicVolume = 0.7f;
    public float SfxVolume = 1.0f;
    public bool MuteOnFocusLoss = true;

    // Video
    public int ResolutionWidth = 0;   // 0 = native
    public int ResolutionHeight = 0;
    public int DisplayMode = 2;       // 0=Fullscreen, 1=Windowed, 2=Borderless
    public bool VSync = true;
    public int MaxFps = 0;            // 0 = match refresh rate
    public float FieldOfView = 75f;

    // Game Feel
    public float ScreenShakeIntensity = 1.0f;
    public float HitStopIntensity = 1.0f;
    public bool SpeedLinesEnabled = true;
    public int CrosshairStyle = 0;    // 0=Cross, 1=Dot, 2=Circle
    public string CrosshairColor = "FFFFFF";

    // Accessibility
    public int ColorblindMode = 0;    // 0=Off, 1=Protanopia, 2=Deuteranopia, 3=Tritanopia
    public float HudScale = 1.0f;
    public bool ReduceMotion = false;
    public bool HighContrastOutlines = false;
}
```

## Persistence

- Saved to `user://settings.json` via `System.Text.Json`.
- Loaded on `SettingsManager._Ready()` (autoload). If no file exists, defaults are used.
- Saved immediately on any change (no "Apply" button). `SettingsManager.Save()` serializes `Current` and writes to disk.

## Settings Application

`SettingsManager` applies settings to Godot systems:

| Setting | Godot API |
|---------|-----------|
| Master/Music/SFX Volume | `AudioServer.SetBusVolumeDb()` on the corresponding bus |
| Display Mode | `DisplayServer.WindowSetMode()` |
| Resolution | `DisplayServer.WindowSetSize()` |
| VSync | `DisplayServer.WindowSetVsyncMode()` |
| Max FPS | `Engine.MaxFps` |
| Mouse Sensitivity | Read by `PlayerCamera.cs` from `SettingsManager.Current` |
| FOV | Read by `PlayerCamera.cs` as `BaseFov` |
| Screen Shake / Hit Stop | Read by `ScreenShake.cs` / `HitStopManager.cs` as multipliers |
| Key Bindings | `InputMap.ActionEraseEvents()` + `InputMap.ActionAddEvent()` |
| Mute on Focus Loss | Handled via `NotificationApplicationFocusIn/Out` in `SettingsManager` |
| Colorblind Mode | Sets a global shader uniform or swaps a color palette resource |
| HUD Scale | Sets `CanvasLayer.Scale` on the HUD |
| Reduce Motion | Read by `SpeedLines.cs`, `ScreenShake.cs`, `DodgeRollTrail.cs` to disable/reduce effects |
| High Contrast Outlines | Toggles an outline shader pass on enemy and projectile materials |

## Settings Menu Scene

```
SettingsMenu (Control) [scripts/ui/SettingsMenu.cs]
├── TabContainer
│   ├── Controls (VBoxContainer) — Sensitivity slider, Invert Y toggle, key rebind buttons
│   ├── Audio (VBoxContainer) — Master/Music/SFX sliders, mute toggle
│   ├── Video (VBoxContainer) — Resolution dropdown, display mode, VSync, FPS, FOV
│   ├── Game Feel (VBoxContainer) — Shake/hit stop sliders, speed lines toggle, crosshair selector
│   └── Accessibility (VBoxContainer) — Colorblind dropdown, HUD scale, reduce motion, high contrast
└── BackButton (Button) — Returns to pause menu / main menu
```

Accessible from the pause menu during gameplay or from a future main menu.

## Accessibility Implementation

### Colorblind Mode

A `ColorPalette` resource defines key gameplay colors (player projectile, enemy types, gems, threat indicator, UI accents). Four palette variants exist: Default, Protanopia, Deuteranopia, Tritanopia. On mode change, `SettingsManager` swaps the active palette resource. Materials and UI elements reference the palette.

### Reduce Motion

When enabled:
- `SpeedLinesEnabled` forced to `false`.
- `ScreenShakeIntensity` clamped to max 0.25.
- Dodge roll afterimage disabled.
- `GpuParticles3D` nodes have `AmountRatio` reduced to 0.3.

### High Contrast Outlines

A secondary render pass using a Sobel edge-detection shader on enemy and projectile meshes. Toggled by enabling/disabling a `CanvasLayer` with the outline post-process effect.

## Tests

`tests/GodotExperiment.Tests/SettingsDataTests.cs` — Serialization round-trip, default value correctness, forward-compatibility with missing fields.
