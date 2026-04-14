using Godot;
using GodotExperiment.Combat;
using GodotExperiment.Enemies;

namespace GodotExperiment;

public partial class Bloater : BaseEnemy
{
    [Export] public float ExplosionDelaySeconds { get; set; } = 0.5f;
    [Export] public float ExplosionRadius { get; set; } = 6.5f;
    [Export] public int ExplosionDamageToEnemies { get; set; } = 6;

    [Export] public float DeathTelegraphFlashMin { get; set; } = 0.25f;
    [Export] public float DeathTelegraphFlashMax { get; set; } = 0.9f;
    [Export] public float DeathTelegraphFlashFrequency { get; set; } = 8f;

    private DelayedExplosionState _explosion = null!;
    private AudioStreamPlayer3D? _telegraphAudio;
    private float _deathTelegraphTime;
    private bool _isDying;

    public override void _Ready()
    {
        base._Ready();
        _telegraphAudio = GetNodeOrNull<AudioStreamPlayer3D>("TelegraphAudio");
        _explosion = new DelayedExplosionState(Mathf.Max(0f, ExplosionDelaySeconds));
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (_isDying && !_explosion.HasExploded)
        {
            _deathTelegraphTime += dt;

            float pulse = (Mathf.Sin(_deathTelegraphTime * Mathf.Tau * DeathTelegraphFlashFrequency) + 1f) * 0.5f;
            TelegraphFlashIntensity = Mathf.Lerp(DeathTelegraphFlashMin, DeathTelegraphFlashMax, pulse);

            if (_explosion.Update(dt))
            {
                Explode();
                return;
            }
        }

        base._PhysicsProcess(delta);
    }

    protected override void OnDied()
    {
        if (_isDying) return;
        _isDying = true;

        DisableBodyCollision();
        _telegraphAudio?.Play();
        _explosion.Arm();
    }

    private void Explode()
    {
        if (!IsInsideTree() || IsQueuedForDeletion()) return;

        TelegraphFlashIntensity = 0f;
        _telegraphAudio?.Stop();

        ApplyExplosionDamage();
        FinalizeDeath();
    }

    private void ApplyExplosionDamage()
    {
        float radius = Mathf.Max(0.01f, ExplosionRadius);
        float r2 = radius * radius;
        Vector3 origin = GlobalPosition;

        var player = GetTree().GetFirstNodeInGroup("player") as Player;
        if (player != null && origin.DistanceSquaredTo(player.GlobalPosition) <= r2)
            player.TakeDamage(DamageSource.Explosion);

        var enemies = GetTree().GetNodesInGroup("enemy");
        for (int i = 0; i < enemies.Count; i++)
        {
            var node = enemies[i];
            if (node == this) continue;
            if (node is not BaseEnemy enemy) continue;
            if (enemy.Health.IsDead) continue;

            if (origin.DistanceSquaredTo(enemy.GlobalPosition) <= r2)
                enemy.TakeDamage(Mathf.Max(1, ExplosionDamageToEnemies));
        }
    }

    private void DisableBodyCollision()
    {
        var shape = GetNodeOrNull<CollisionShape3D>("CollisionShape3D");
        if (shape != null)
            shape.Disabled = true;
    }
}

