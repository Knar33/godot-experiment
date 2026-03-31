using Godot;
using GodotExperiment.Combat;

namespace GodotExperiment;

public partial class PlayerProjectile : Area3D
{
    [Export] public float Speed { get; set; } = 50f;
    [Export] public float MaxRange { get; set; } = 25f;

    private ProjectileState _state = null!;
    private Vector3 _direction;
    private bool _initialized;

    public void Initialize(Vector3 direction)
    {
        _direction = direction.Normalized();
        _initialized = true;
    }

    public override void _Ready()
    {
        _state = new ProjectileState(MaxRange);
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_initialized) return;

        float dt = (float)delta;
        float distance = Speed * dt;
        GlobalPosition += _direction * distance;
        _state.AddDistance(distance);

        if (_state.IsExpired)
            QueueFree();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body.IsInGroup("enemy"))
            QueueFree();
    }
}
