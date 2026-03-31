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
        {
            _chargeDirection = toPlayer.LengthSquared() > 0.01f
                ? toPlayer.Normalized()
                : GlobalTransform.Basis.Z.Normalized();
            _telegraphAudio?.Play();
        }

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
}
