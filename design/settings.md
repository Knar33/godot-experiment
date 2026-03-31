# Settings

Accessible from the pause menu during gameplay or from the main menu. Changes apply immediately (no "apply" button required). All settings persist between sessions.

## Controls

- **Mouse Sensitivity**: Slider (low to high). Default: medium. Affects camera rotation speed per pixel of mouse movement.
- **Invert Y-Axis**: Toggle. Default: off.
- **Key Rebinding**: All gameplay actions (move, jump, dodge roll, shoot, pause) are rebindable. Display current binding and allow the player to press a new key to reassign. Conflicts are flagged visually.
- **Gamepad Support**: Controller bindings shown separately. Stick deadzone adjustment (slider, default 0.15). Aim assist is not provided — the game is designed around precise mouse aim, and gamepad players accept the challenge.

## Audio

- **Master Volume**: Slider (0-100%). Default: 100%.
- **Music Volume**: Slider (0-100%). Default: 70%.
- **SFX Volume**: Slider (0-100%). Default: 100%.
- **Mute on Focus Loss**: Toggle. Default: on. Mutes all audio when the game window is not in focus.

## Video

- **Resolution**: Dropdown of available resolutions. Default: native desktop resolution.
- **Display Mode**: Fullscreen / Borderless Window / Windowed. Default: Borderless Window.
- **VSync**: Toggle. Default: on.
- **Max FPS**: Slider or presets (60 / 120 / 144 / 240 / Unlimited). Default: matches monitor refresh rate. Only visible when VSync is off.
- **Field of View**: Slider (60-110 degrees). Default: 75. This is the base FOV — speed-based FOV scaling (see `game-feel.md`) is additive on top of this.

## Game Feel

These sliders let the player tune feedback intensity to personal comfort.

- **Screen Shake Intensity**: Slider (0-100%). Default: 100%. At 0%, all screen shake is disabled.
- **Hit Stop Intensity**: Slider (0-100%). Default: 100%. At 0%, hit stop (frame pauses) is disabled.
- **Speed Lines**: Toggle. Default: on. Disables the radial speed lines during bhop if they cause discomfort.
- **Crosshair Style**: Preset selector (default cross, dot, circle). Color picker for crosshair color. Default: white cross.

## Accessibility

- **Colorblind Mode**: Off / Protanopia / Deuteranopia / Tritanopia. Adjusts key gameplay colors (player projectiles, enemy indicators, gems, threat direction markers) to be distinguishable under each type of color vision deficiency.
- **HUD Scale**: Slider (75-150%). Default: 100%. Scales all HUD elements (timer, gem counter, upgrade meter, crosshair) proportionally.
- **Reduce Motion**: Toggle. Default: off. When enabled, disables speed lines, reduces screen shake to 25%, removes dodge roll afterimage, and reduces particle density. For players sensitive to rapid visual motion.
- **High Contrast Outlines**: Toggle. Default: off. Adds bright outlines to enemies and projectiles to improve readability against the dark arena background.
