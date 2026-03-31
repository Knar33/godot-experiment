using Godot;

namespace GodotExperiment;

public partial class Crosshair : Control
{
    [Export] public float ArmLength { get; set; } = 8f;
    [Export] public float Thickness { get; set; } = 2f;
    [Export] public float Gap { get; set; } = 3f;
    [Export] public Color CrosshairColor { get; set; } = new(1f, 1f, 1f, 0.85f);

    public override void _Draw()
    {
        Vector2 c = Size / 2f;

        DrawLine(new(c.X - Gap - ArmLength, c.Y), new(c.X - Gap, c.Y), CrosshairColor, Thickness);
        DrawLine(new(c.X + Gap, c.Y), new(c.X + Gap + ArmLength, c.Y), CrosshairColor, Thickness);
        DrawLine(new(c.X, c.Y - Gap - ArmLength), new(c.X, c.Y - Gap), CrosshairColor, Thickness);
        DrawLine(new(c.X, c.Y + Gap), new(c.X, c.Y + Gap + ArmLength), CrosshairColor, Thickness);
    }
}
