using Godot;

namespace GodotExperiment;

public partial class GemPickup : Area3D
{
    private Vector3 _scatterVelocity;
    private float _scatterTime;
    private const float ScatterDuration = 0.3f;
    private const float ScatterDamping = 5f;

    public override void _Ready()
    {
        AddToGroup("gems");
    }

    public void SetScatterVelocity(Vector3 velocity)
    {
        _scatterVelocity = velocity;
        _scatterTime = ScatterDuration;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (_scatterTime <= 0f) return;

        float dt = (float)delta;
        _scatterTime -= dt;

        GlobalPosition += _scatterVelocity * dt;
        _scatterVelocity *= 1f - ScatterDamping * dt;

        if (_scatterTime <= 0f)
        {
            _scatterVelocity = Vector3.Zero;
            var pos = GlobalPosition;
            pos.Y = 0.15f;
            GlobalPosition = pos;
        }
    }
}
