using Godot;
using System.Collections.Generic;

public partial class InputActions : Node
{
    public const string MoveForward = "move_forward";
    public const string MoveBack = "move_back";
    public const string MoveLeft = "move_left";
    public const string MoveRight = "move_right";
    public const string Jump = "jump";
    public const string Sprint = "sprint";
    public const string Crouch = "crouch";
    public const string Fire = "fire";
    public const string CameraZoomIn = "camera_zoom_in";
    public const string CameraZoomOut = "camera_zoom_out";

    public override void _EnterTree()
    {
        EnsureDefaultBindings();
    }

    public static void EnsureDefaultBindings()
    {
        EnsureAction(MoveForward, new InputEvent[] { KeyEvent(Key.W), KeyEvent(Key.Up) });
        EnsureAction(MoveBack, new InputEvent[] { KeyEvent(Key.S), KeyEvent(Key.Down) });
        EnsureAction(MoveLeft, new InputEvent[] { KeyEvent(Key.A), KeyEvent(Key.Left) });
        EnsureAction(MoveRight, new InputEvent[] { KeyEvent(Key.D), KeyEvent(Key.Right) });
        EnsureAction(Jump, new InputEvent[] { KeyEvent(Key.Space) });
        EnsureAction(Sprint, new InputEvent[] { KeyEvent(Key.Shift) });
        EnsureAction(Crouch, new InputEvent[] { KeyEvent(Key.C) });
        EnsureAction(Fire, new InputEvent[] { MouseButtonEvent(MouseButton.Left) });
        EnsureAction(CameraZoomIn, new InputEvent[] { MouseButtonEvent(MouseButton.WheelUp) });
        EnsureAction(CameraZoomOut, new InputEvent[] { MouseButtonEvent(MouseButton.WheelDown) });
    }

    public static void RebindAction(string actionName, IEnumerable<InputEvent> events)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }

        InputMap.ActionEraseEvents(actionName);
        foreach (var inputEvent in events)
        {
            InputMap.ActionAddEvent(actionName, inputEvent);
        }
    }

    private static void EnsureAction(string actionName, IEnumerable<InputEvent> defaults)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }

        if (InputMap.ActionGetEvents(actionName).Count > 0)
        {
            return;
        }

        foreach (var inputEvent in defaults)
        {
            InputMap.ActionAddEvent(actionName, inputEvent);
        }
    }

    private static InputEventKey KeyEvent(Key key)
    {
        return new InputEventKey
        {
            PhysicalKeycode = key
        };
    }

    private static InputEventMouseButton MouseButtonEvent(MouseButton button)
    {
        return new InputEventMouseButton
        {
            ButtonIndex = button
        };
    }
}
