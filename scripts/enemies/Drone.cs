using Godot;
using GodotExperiment.Enemies;

namespace GodotExperiment;

public partial class Drone : BaseEnemy
{
    [Export] public float HoverHeight { get; set; } = 2.5f;
    [Export] public float HoverCorrection { get; set; } = 1.6f;

    [Export] public float OrbitRadius { get; set; } = 9f;
    [Export] public float OrbitRadiusTolerance { get; set; } = 3f;
    [Export] public float OrbitTangentialWeight { get; set; } = 1.2f;
    [Export] public float OrbitRadialWeight { get; set; } = 1.0f;
    [Export] public float JitterStrength { get; set; } = 0.35f;
    [Export] public float JitterChangeInterval { get; set; } = 0.35f;

    [Export] public float DiveTriggerRange { get; set; } = 14f;
    [Export] public float DiveInterval { get; set; } = 3.5f;
    [Export] public float TelegraphDuration { get; set; } = 0.25f;
    [Export] public float DiveDuration { get; set; } = 0.35f;
    [Export] public float RecoveryDuration { get; set; } = 0.6f;
    [Export] public float DiveSpeedMultiplier { get; set; } = 7f;
    [Export] public float DiveAimHeightOffset { get; set; } = 0.9f;

    private DroneAIState _aiState = null!;
    private AudioStreamPlayer3D? _telegraphAudio;
    private Vector3 _diveDirection;
    private float _telegraphFlashTime;

    private readonly RandomNumberGenerator _rng = new();
    private Vector3 _jitterDir;
    private float _jitterTimer;
    private float _orbitSign = 1f;

    public override void _Ready()
    {
        base._Ready();

        _rng.Randomize();
        _orbitSign = _rng.Randf() < 0.5f ? -1f : 1f;

        _telegraphAudio = GetNodeOrNull<AudioStreamPlayer3D>("TelegraphAudio");

        float initialAttack = _rng.RandfRange(0f, Mathf.Max(0.01f, DiveInterval));
        _aiState = new DroneAIState(
            DiveTriggerRange, DiveInterval,
            TelegraphDuration, DiveDuration, RecoveryDuration,
            DiveSpeedMultiplier,
            initialAttackTimer: initialAttack);

        GlobalPosition = new Vector3(GlobalPosition.X, HoverHeight, GlobalPosition.Z);

        _jitterDir = RandomHorizontalUnit();
        _jitterTimer = _rng.RandfRange(0f, Mathf.Max(0.01f, JitterChangeInterval));
    }

    protected override void MoveTowardPlayer(float dt)
    {
        var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (player == null) return;

        Vector3 toPlayer = player.GlobalPosition - GlobalPosition;
        Vector3 toPlayerFlat = new(toPlayer.X, 0f, toPlayer.Z);
        float flatDistance = toPlayerFlat.Length();

        var transition = _aiState.Update(dt, flatDistance);

        if (transition == DroneAIState.Phase.Telegraph)
            _telegraphAudio?.Play();

        if (transition == DroneAIState.Phase.Diving)
        {
            _telegraphAudio?.Stop();
            _diveDirection = ComputeDiveDirection(player, flatDistance);
        }

        UpdateTelegraphFlash(dt);

        SeparationEnabled = !_aiState.IsDiving;

        if (_aiState.IsTelegraphing)
        {
            Velocity = Vector3.Zero;
            return;
        }

        UpdateJitter(dt);

        if (_aiState.IsDiving)
        {
            Velocity = _diveDirection * MoveSpeed * _aiState.DiveSpeedMultiplier;
            MoveAndSlide();
            return;
        }

        Vector3 direction = ComputeOrbitDirection(toPlayerFlat, flatDistance);
        Velocity = direction * MoveSpeed;
        MoveAndSlide();
    }

    private void UpdateTelegraphFlash(float dt)
    {
        if (_aiState.IsTelegraphing)
        {
            _telegraphFlashTime += dt;
            float pulse = (Mathf.Sin(_telegraphFlashTime * Mathf.Tau * 8f) + 1f) * 0.5f;
            TelegraphFlashIntensity = Mathf.Lerp(0.35f, 0.95f, pulse);

            if (_telegraphAudio?.Stream != null && !_telegraphAudio.Playing)
                _telegraphAudio.Play();
        }
        else
        {
            _telegraphFlashTime = 0f;
            TelegraphFlashIntensity = 0f;
        }
    }

    private void UpdateJitter(float dt)
    {
        if (JitterStrength <= 0f) return;

        _jitterTimer -= dt;
        if (_jitterTimer > 0f) return;

        _jitterTimer = Mathf.Max(0.01f, JitterChangeInterval);
        _jitterDir = RandomHorizontalUnit();
    }

    private Vector3 ComputeOrbitDirection(Vector3 toPlayerFlat, float flatDistance)
    {
        Vector3 radialToward = toPlayerFlat.LengthSquared() > 0.0001f
            ? toPlayerFlat.Normalized()
            : -GlobalTransform.Basis.Z.Normalized();

        Vector3 tangent = new Vector3(-radialToward.Z, 0f, radialToward.X) * _orbitSign;

        float radialFactor = 0f;
        if (OrbitRadiusTolerance > 0.001f)
        {
            float error = flatDistance - OrbitRadius;
            radialFactor = Mathf.Clamp(error / OrbitRadiusTolerance, -1f, 1f);
        }

        Vector3 horizontal = tangent * OrbitTangentialWeight + radialToward * radialFactor * OrbitRadialWeight;

        if (JitterStrength > 0f)
            horizontal += _jitterDir * JitterStrength;

        if (SeparationEnabled)
            horizontal += ComputeSeparationFromGroup3D();

        float yError = HoverHeight - GlobalPosition.Y;
        float vertical = Mathf.Clamp(yError * HoverCorrection, -1f, 1f);

        Vector3 desired = new(horizontal.X, vertical, horizontal.Z);
        return desired.LengthSquared() > 0.0001f ? desired.Normalized() : Vector3.Zero;
    }

    private Vector3 ComputeDiveDirection(Node3D player, float flatDistance)
    {
        float diveSpeed = MoveSpeed * DiveSpeedMultiplier;
        float timeToReach = diveSpeed > 0.01f ? flatDistance / diveSpeed : 0f;

        Vector3 playerVel = Vector3.Zero;
        if (player is CharacterBody3D charBody)
        {
            playerVel = charBody.Velocity;
            playerVel.Y = 0;
        }

        Vector3 aimPoint = player.GlobalPosition + playerVel * timeToReach + Vector3.Up * DiveAimHeightOffset;
        Vector3 toTarget = aimPoint - GlobalPosition;

        return toTarget.LengthSquared() > 0.01f
            ? toTarget.Normalized()
            : -GlobalTransform.Basis.Z.Normalized();
    }

    private Vector3 ComputeSeparationFromGroup3D()
    {
        var enemies = GetTree().GetNodesInGroup("enemy");
        Vector3 accumulated = Vector3.Zero;
        int count = 0;

        for (int i = 0; i < enemies.Count; i++)
        {
            var node = enemies[i];
            if (node == this) continue;
            if (node is not Node3D other) continue;

            Vector3 away3D = GlobalPosition - other.GlobalPosition;
            float distance3D = away3D.Length();
            if (distance3D > SeparationRadius) continue;

            float strength = distance3D <= 0.001f
                ? 1f
                : 1f - distance3D / SeparationRadius;

            Vector3 awayFlat = new(away3D.X, 0f, away3D.Z);
            if (awayFlat.LengthSquared() <= 0.0001f)
                awayFlat = RandomHorizontalUnit();
            else
                awayFlat = awayFlat.Normalized();

            Vector3 tangent = new(-awayFlat.Z, 0f, awayFlat.X);
            accumulated += (awayFlat + tangent * SeparationTangent) * strength;
            count++;
        }

        if (count == 0)
            return Vector3.Zero;
        return accumulated * (SeparationWeight / count);
    }

    private Vector3 RandomHorizontalUnit()
    {
        float angle = _rng.RandfRange(0f, Mathf.Tau);
        return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
    }
}

