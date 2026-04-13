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
    private float _telegraphFlashTime;

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
        {
            _telegraphAudio?.Stop();
            _chargeDirection = ComputeLeadDirection(player, distance);
        }

        if (_aiState.IsTelegraphing)
        {
            _telegraphFlashTime += dt;
            float pulse = (Mathf.Sin(_telegraphFlashTime * Mathf.Tau * 6f) + 1f) * 0.5f;
            TelegraphFlashIntensity = Mathf.Lerp(0.25f, 0.9f, pulse);

            if (_telegraphAudio?.Stream != null && !_telegraphAudio.Playing)
                _telegraphAudio.Play();
        }
        else
        {
            _telegraphFlashTime = 0f;
            TelegraphFlashIntensity = 0f;
        }

        SeparationEnabled = !_aiState.IsCharging && !_aiState.IsRecovering;

        if (!_aiState.ShouldMove) return;

        if (_aiState.IsCharging)
        {
            Velocity = _chargeDirection * MoveSpeed * _aiState.ChargeSpeedMultiplier;
            MoveAndSlide();
        }
        else
        {
            base.MoveTowardPlayer(dt);
        }
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
