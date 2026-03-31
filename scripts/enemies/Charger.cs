using Godot;
using GodotExperiment.Combat;
using GodotExperiment.Enemies;

namespace GodotExperiment;

public partial class Charger : BaseEnemy
{
    [Export] public float ChargeRange { get; set; } = 20f;
    [Export] public float TelegraphDuration { get; set; } = 1.5f;
    [Export] public float ChargeDuration { get; set; } = 0.7f;
    [Export] public float RecoveryDuration { get; set; } = 0.75f;
    [Export] public float ChargeSpeedMultiplier { get; set; } = 8f;

    private ChargerAIState _aiState = null!;
    private AudioStreamPlayer3D? _telegraphAudio;
    private Vector3 _chargeDirection;

    public override void _Ready()
    {
        base._Ready();
        _aiState = new ChargerAIState(
            ChargeRange, TelegraphDuration, ChargeDuration,
            RecoveryDuration, ChargeSpeedMultiplier);
        _telegraphAudio = GetNodeOrNull<AudioStreamPlayer3D>("TelegraphAudio");
    }

    protected override void MoveTowardPlayer(float dt)
    {
        var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (player == null) return;

        Vector3 toPlayer = player.GlobalPosition - GlobalPosition;
        toPlayer.Y = 0;
        float distance = toPlayer.Length();

        var transition = _aiState.Update(dt, distance);

        if (transition == ChargerAIState.Phase.Telegraph)
            _telegraphAudio?.Play();

        if (transition == ChargerAIState.Phase.Charging)
            _chargeDirection = ComputeLeadDirection(player, distance);

        if (!_aiState.ShouldMove) return;

        if (_aiState.IsCharging)
        {
            Velocity = _chargeDirection * MoveSpeed * _aiState.ChargeSpeedMultiplier;
        }
        else
        {
            if (toPlayer.LengthSquared() < 0.25f) return;
            Velocity = toPlayer.Normalized() * MoveSpeed;
        }

        MoveAndSlide();
    }

    private Vector3 ComputeLeadDirection(Node3D player, float distance)
    {
        float chargeSpeed = MoveSpeed * ChargeSpeedMultiplier;
        float timeToReach = distance / chargeSpeed;

        Vector3 playerVel = Vector3.Zero;
        if (player is CharacterBody3D charBody)
        {
            playerVel = charBody.Velocity;
            playerVel.Y = 0;
        }

        Vector3 predictedPos = player.GlobalPosition + playerVel * timeToReach;
        Vector3 toTarget = predictedPos - GlobalPosition;
        toTarget.Y = 0;

        return toTarget.LengthSquared() > 0.01f
            ? toTarget.Normalized()
            : GlobalTransform.Basis.Z.Normalized();
    }
}
