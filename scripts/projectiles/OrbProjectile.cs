using Godot;

public partial class OrbProjectile : Area3D
{
    [Export] public float LifetimeSeconds = 3.0f;

    private Vector3 _direction = Vector3.Forward;
    private float _speed = 20.0f;
    private float _timeAlive;

    public void Initialize(Vector3 direction, float speed)
    {
        _direction = direction.Normalized();
        _speed = speed;
    }

    public override void _PhysicsProcess(double delta)
    {
        GlobalPosition += _direction * _speed * (float)delta;
        _timeAlive += (float)delta;

        if (_timeAlive >= LifetimeSeconds)
        {
            QueueFree();
        }
    }
}
