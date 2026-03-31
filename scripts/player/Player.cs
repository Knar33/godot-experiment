using Godot;
using GodotExperiment.Combat;
using GodotExperiment.GameLoop;
using GodotExperiment.PlayerMovement;

namespace GodotExperiment;

public partial class Player : CharacterBody3D
{
    [Export] public float BaseSpeed { get; set; } = 10f;
    [Export] public float JumpVelocity { get; set; } = 8f;
    [Export] public float Gravity { get; set; } = 20f;
    [Export] public float AirStrafeInfluence { get; set; } = 8f;
    [Export] public float DodgeRollSpeed { get; set; } = 18f;

    public BhopState Bhop { get; } = new();
    public DodgeRollState DodgeRoll { get; } = new();
    public AutoFireState AutoFire { get; } = new();
    public PlayerHealthState Health { get; } = new();

    [Signal]
    public delegate void PlayerDiedEventHandler();

    private bool _wasGrounded = true;
    private float _groundedTime;
    private float _jumpBufferTime = float.MaxValue;
    private Vector3 _dodgeRollDirection;

    private PackedScene _projectileScene = null!;
    private PlayerCamera? _playerCamera;
    private Node3D? _projectilesContainer;
    private AudioStreamPlayer3D? _fireAudio;
    private RandomNumberGenerator _rng = new();

    private static readonly Vector3 MuzzleOffset = new(0f, 1.2f, 0f);

    public override void _Ready()
    {
        AddToGroup("player");
        _projectileScene = GD.Load<PackedScene>("res://scenes/player/PlayerProjectile.tscn");
    }

    /// <summary>
    /// Called by enemy damage sources (contact, projectile, explosion, ground hazard).
    /// Checks i-frame protection before applying lethal damage.
    /// </summary>
    public void TakeDamage(DamageSource source)
    {
        if (!Health.TakeDamage(source, DodgeRoll.IsInvulnerable))
            return;

        OnPlayerDeath();
    }

    private void OnPlayerDeath()
    {
        StopAllAudio();
        HidePlayerMesh();
        Velocity = Vector3.Zero;
        GameManager.Instance?.TriggerPlayerDeath();
        EmitSignal(SignalName.PlayerDied);
    }

    private void StopAllAudio()
    {
        foreach (var child in GetChildren())
        {
            if (child is AudioStreamPlayer3D audio)
                audio.Stop();
        }
    }

    private void HidePlayerMesh()
    {
        var mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
        if (mesh != null)
            mesh.Visible = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!Health.IsAlive) return;

        var state = GameManager.Instance?.CurrentState;
        if (state == GameState.Countdown)
        {
            Velocity = Vector3.Zero;
            MoveAndSlide();
            return;
        }

        float dt = (float)delta;
        DodgeRoll.Update(dt);

        Vector3 velocity = Velocity;
        bool grounded = IsOnFloor();

        if (grounded && !_wasGrounded)
            _groundedTime = 0f;

        if (grounded)
            _groundedTime += dt;

        if (grounded && _groundedTime > Bhop.TimingWindow && !DodgeRoll.IsRolling)
            Bhop.DecaySpeed(dt);

        if (!grounded)
            velocity.Y -= Gravity * dt;

        _jumpBufferTime += dt;
        if (Input.IsActionJustPressed("jump"))
            _jumpBufferTime = 0f;

        bool wantsJump = _jumpBufferTime <= Bhop.TimingWindow;
        if (wantsJump && grounded && !DodgeRoll.IsRolling)
        {
            Bhop.TryBhop(_groundedTime);
            velocity.Y = JumpVelocity;
            _jumpBufferTime = float.MaxValue;
        }

        if (Input.IsActionJustPressed("dodge_roll") && grounded && !DodgeRoll.IsRolling)
        {
            if (DodgeRoll.TryStartRoll())
            {
                Vector2 inputDir = GetInputDirection();
                _dodgeRollDirection = inputDir.LengthSquared() > 0.01f
                    ? InputToWorldDirection(inputDir)
                    : GetCameraForward();
            }
        }

        if (DodgeRoll.IsRolling)
        {
            velocity.X = _dodgeRollDirection.X * DodgeRollSpeed;
            velocity.Z = _dodgeRollDirection.Z * DodgeRollSpeed;
        }
        else if (grounded)
        {
            Vector2 inputDir = GetInputDirection();
            if (inputDir.LengthSquared() > 0.01f)
            {
                Vector3 worldDir = InputToWorldDirection(inputDir);
                float speed = BaseSpeed * Bhop.SpeedMultiplier;
                velocity.X = worldDir.X * speed;
                velocity.Z = worldDir.Z * speed;
            }
            else
            {
                velocity.X = 0f;
                velocity.Z = 0f;
            }
        }
        else
        {
            ApplyAirStrafe(ref velocity, dt);
        }

        _wasGrounded = grounded;
        Velocity = velocity;
        MoveAndSlide();

        UpdateShooting(dt);
    }

    private void UpdateShooting(float dt)
    {
        if (GameManager.Instance?.CurrentState != GameState.Playing)
            return;

        if (Input.IsActionJustPressed("shoot"))
            AutoFire.ResetToReady();

        if (!Input.IsActionPressed("shoot"))
            return;

        AutoFire.Update(dt);

        if (AutoFire.TryFire())
            SpawnProjectile();
    }

    private void SpawnProjectile()
    {
        _playerCamera ??= GetNodeOrNull<PlayerCamera>("../PlayerCamera");
        _projectilesContainer ??= GetNodeOrNull<Node3D>("../Projectiles");

        Camera3D? camera = GetViewport().GetCamera3D();
        if (camera == null || _playerCamera == null || _projectilesContainer == null)
            return;

        Vector3 spawnPos = GlobalPosition + MuzzleOffset;
        Vector3 aimTarget = _playerCamera.AimPoint;

        Vector3 cameraForward = (-camera.GlobalTransform.Basis.Z).Normalized();
        if (spawnPos.DistanceTo(aimTarget) < 2f)
            aimTarget = spawnPos + cameraForward * 50f;

        Vector3 direction = (aimTarget - spawnPos).Normalized();

        var projectile = _projectileScene.Instantiate<PlayerProjectile>();
        _projectilesContainer.AddChild(projectile);
        projectile.GlobalPosition = spawnPos;
        projectile.Initialize(direction);

        PlayFireSound();
    }

    private void PlayFireSound()
    {
        _fireAudio ??= GetNodeOrNull<AudioStreamPlayer3D>("FireAudio");
        if (_fireAudio == null) return;

        _fireAudio.PitchScale = 1.0f + _rng.RandfRange(-0.05f, 0.05f);
        _fireAudio.Play();
    }

    private void ApplyAirStrafe(ref Vector3 velocity, float dt)
    {
        float strafeInput = Input.GetAxis("move_left", "move_right");
        if (Mathf.Abs(strafeInput) < 0.01f) return;

        Vector3 right = GetCameraRight();
        velocity.X += right.X * strafeInput * AirStrafeInfluence * dt;
        velocity.Z += right.Z * strafeInput * AirStrafeInfluence * dt;

        float maxSpeed = BaseSpeed * Bhop.SpeedMultiplier;
        Vector2 hVel = new(velocity.X, velocity.Z);
        if (hVel.Length() > maxSpeed)
        {
            hVel = hVel.Normalized() * maxSpeed;
            velocity.X = hVel.X;
            velocity.Z = hVel.Y;
        }
    }

    private Vector2 GetInputDirection()
    {
        return Input.GetVector("move_left", "move_right", "move_forward", "move_backward");
    }

    private Vector3 InputToWorldDirection(Vector2 input)
    {
        Camera3D? camera = GetViewport().GetCamera3D();
        if (camera == null)
            return new Vector3(input.X, 0, input.Y).Normalized();

        Vector3 forward = -camera.GlobalTransform.Basis.Z;
        Vector3 right = camera.GlobalTransform.Basis.X;

        forward.Y = 0;
        right.Y = 0;
        forward = forward.Normalized();
        right = right.Normalized();

        return (right * input.X + forward * -input.Y).Normalized();
    }

    private Vector3 GetCameraForward()
    {
        Camera3D? camera = GetViewport().GetCamera3D();
        if (camera == null) return Vector3.Forward;

        Vector3 forward = -camera.GlobalTransform.Basis.Z;
        forward.Y = 0;
        return forward.Normalized();
    }

    private Vector3 GetCameraRight()
    {
        Camera3D? camera = GetViewport().GetCamera3D();
        if (camera == null) return Vector3.Right;

        Vector3 right = camera.GlobalTransform.Basis.X;
        right.Y = 0;
        return right.Normalized();
    }
}
