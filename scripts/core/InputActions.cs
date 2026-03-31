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

    /// <summary>
    /// Merges default keys into each action. Unlike a one-shot fill, this always adds a physical
    /// key if it is missing, so a half-configured project.godot (events: [...] with bad data) still works.
    /// </summary>
    public static void EnsureDefaultBindings()
    {
        AddPhysicalKeyToAction(MoveForward, Key.W);
        AddPhysicalKeyToAction(MoveForward, Key.Up);
        AddPhysicalKeyToAction(MoveBack, Key.S);
        AddPhysicalKeyToAction(MoveBack, Key.Down);
        AddPhysicalKeyToAction(MoveLeft, Key.A);
        AddPhysicalKeyToAction(MoveLeft, Key.Left);
        AddPhysicalKeyToAction(MoveRight, Key.D);
        AddPhysicalKeyToAction(MoveRight, Key.Right);
        AddPhysicalKeyToAction(Jump, Key.Space);
        // Shift is a modifier: IsActionPressed can misbehave; E is a non-modifier sprint alternate.
        AddPhysicalKeyToAction(Sprint, Key.Shift);
        AddPhysicalKeyToAction(Sprint, Key.E);
        AddPhysicalKeyToAction(Crouch, Key.C);
        AddPhysicalKeyToAction(Crouch, Key.Ctrl);
        AddMouseButtonToAction(Fire, MouseButton.Left);
        AddMouseButtonToAction(CameraZoomIn, MouseButton.WheelUp);
        AddMouseButtonToAction(CameraZoomOut, MouseButton.WheelDown);
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

    private static void AddPhysicalKeyToAction(string actionName, Key physicalKey)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }

        if (ActionHasPhysicalKey(actionName, physicalKey))
        {
            return;
        }

        InputMap.ActionAddEvent(actionName, PhysicalKeyEvent(physicalKey));
    }

    private static void AddMouseButtonToAction(string actionName, MouseButton button)
    {
        if (!InputMap.HasAction(actionName))
        {
            InputMap.AddAction(actionName);
        }

        if (ActionHasMouseButton(actionName, button))
        {
            return;
        }

        InputMap.ActionAddEvent(actionName, MouseButtonEvent(button));
    }

    private static bool ActionHasPhysicalKey(string action, Key physicalKey)
    {
        foreach (var e in InputMap.ActionGetEvents(action))
        {
            if (e is InputEventKey k && k.PhysicalKeycode == physicalKey)
            {
                return true;
            }
        }

        return false;
    }

    private static bool ActionHasMouseButton(string action, MouseButton button)
    {
        foreach (var e in InputMap.ActionGetEvents(action))
        {
            if (e is InputEventMouseButton m && m.ButtonIndex == button)
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>Maps by physical location (WASD). Only physical_keycode is set per Godot docs.</summary>
    private static InputEventKey PhysicalKeyEvent(Key physicalKey)
    {
        return new InputEventKey { PhysicalKeycode = physicalKey };
    }

    private static InputEventMouseButton MouseButtonEvent(MouseButton button)
    {
        return new InputEventMouseButton { ButtonIndex = button };
    }
}
