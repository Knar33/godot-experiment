using Godot;
using GodotExperiment.Combat;

namespace GodotExperiment;

public partial class PlayerProjectile : Area3D
{
    [Export] public float Speed { get; set; } = 50f;
    [Export] public float MaxRange { get; set; } = 25f;
    [Export] public AudioStream? SurfaceImpactSound { get; set; }
    [Export] public AudioStream? EnemyImpactSound { get; set; }

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
        if (body.IsInGroup("player")) return;

        bool isEnemy = body.IsInGroup("enemy");
        PlayImpactSound(isEnemy);
        QueueFree();
    }

    private void PlayImpactSound(bool isEnemyHit)
    {
        var impactAudio = GetNodeOrNull<AudioStreamPlayer3D>("ImpactAudio");
        if (impactAudio == null) return;

        AudioStream? sound = isEnemyHit ? EnemyImpactSound : SurfaceImpactSound;
        if (sound == null) return;

        impactAudio.Stream = sound;

        // Reparent audio to a temporary node so it outlives the projectile
        RemoveChild(impactAudio);
        var temp = new Node3D();
        GetTree().CurrentScene.AddChild(temp);
        temp.GlobalPosition = GlobalPosition;
        temp.AddChild(impactAudio);
        impactAudio.Play();
        impactAudio.Finished += () => temp.QueueFree();
    }
}
