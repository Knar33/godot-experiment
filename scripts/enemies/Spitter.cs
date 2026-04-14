using Godot;
using GodotExperiment.Enemies;

namespace GodotExperiment;

public partial class Spitter : BaseEnemy
{
    [Export] public float PreferredRange { get; set; } = 14f;
    [Export] public float RangeTolerance { get; set; } = 4f;
    [Export] public float AttackInterval { get; set; } = 3f;
    [Export] public float RepositionCheckInterval { get; set; } = 5f;

    private SpitterAIState _aiState = null!;
    private PackedScene _projectileScene = null!;
    private AudioStreamPlayer3D? _telegraphAudio;

    public override void _Ready()
    {
        base._Ready();
        _aiState = new SpitterAIState(PreferredRange, RangeTolerance, AttackInterval, RepositionCheckInterval);
        _projectileScene = GD.Load<PackedScene>("res://scenes/enemies/SpitterProjectile.tscn");
        _telegraphAudio = GetNodeOrNull<AudioStreamPlayer3D>("TelegraphAudio");
    }

    protected override void MoveTowardPlayer(float dt)
    {
        var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (player == null) return;

        Vector3 toPlayer = player.GlobalPosition - GlobalPosition;
        toPlayer.Y = 0;
        float distance = toPlayer.Length();

        bool shouldFire = _aiState.Update(dt, distance);

        if (shouldFire)
            FireProjectile(player);

        if (!_aiState.ShouldMove) return;

        Vector3 direction = distance > PreferredRange
            ? toPlayer.Normalized()
            : -toPlayer.Normalized();
        direction = ApplySeparationSteering(direction);

        if (toPlayer.LengthSquared() < 0.25f) return;

        Velocity = direction * MoveSpeed;
        MoveAndSlide();
    }

    private void FireProjectile(Node3D player)
    {
        _telegraphAudio?.Play();

        var projectile = _projectileScene.Instantiate<SpitterProjectile>();
        GetTree().CurrentScene.AddChild(projectile);

        Vector3 spawnPos = GlobalPosition + new Vector3(0, 1f, 0);
        projectile.GlobalPosition = spawnPos;
        projectile.Initialize(spawnPos, player.GlobalPosition);
    }
}
