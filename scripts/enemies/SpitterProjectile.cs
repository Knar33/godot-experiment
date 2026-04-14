using Godot;
using GodotExperiment.Combat;

namespace GodotExperiment;

public partial class SpitterProjectile : Area3D
{
    [Export] public float FlightTime { get; set; } = 1.2f;
    [Export] public float ProjectileGravity { get; set; } = 20f;
    [Export] public float MaxLifetime { get; set; } = 5f;

    private Vector3 _velocity;
    private float _lifetime;
    private bool _initialized;
    private PackedScene? _hazardScene;

    public void Initialize(Vector3 from, Vector3 to)
    {
        Vector3 displacement = to - from;
        Vector2 horizontal = new(displacement.X, displacement.Z);
        float hDist = horizontal.Length();

        float hSpeed = hDist / FlightTime;
        Vector3 hDir = hDist > 0.01f
            ? new Vector3(displacement.X, 0, displacement.Z).Normalized()
            : Vector3.Forward;

        float vy = (displacement.Y + 0.5f * ProjectileGravity * FlightTime * FlightTime) / FlightTime;

        _velocity = hDir * hSpeed + new Vector3(0, vy, 0);
        _initialized = true;
    }

    public override void _Ready()
    {
        AddToGroup("projectiles");
        _hazardScene = GD.Load<PackedScene>("res://scenes/enemies/SpitterGroundHazard.tscn");
        BodyEntered += OnBodyEntered;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_initialized) return;

        float dt = (float)delta;
        _lifetime += dt;

        if (_lifetime >= MaxLifetime)
        {
            QueueFree();
            return;
        }

        _velocity.Y -= ProjectileGravity * dt;
        GlobalPosition += _velocity * dt;

        if (GlobalPosition.Y <= 0.05f)
        {
            SpawnHazard();
            QueueFree();
        }
    }

    private void OnBodyEntered(Node3D body)
    {
        if (body.IsInGroup("enemy")) return;

        if (body.IsInGroup("player") && body is Player player)
            player.TakeDamage(DamageSource.Projectile);

        SpawnHazard();
        QueueFree();
    }

    private void SpawnHazard()
    {
        if (_hazardScene == null) return;

        var hazard = _hazardScene.Instantiate<Node3D>();
        GetTree().CurrentScene.AddChild(hazard);
        hazard.GlobalPosition = new Vector3(GlobalPosition.X, 0.05f, GlobalPosition.Z);
    }
}
