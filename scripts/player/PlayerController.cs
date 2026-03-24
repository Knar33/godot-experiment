using Godot;

public partial class PlayerController : CharacterBody3D
{
    [Export] public float WalkSpeed = 5.0f;
    [Export] public float SprintSpeed = 9.0f;
    [Export] public float CrouchSpeed = 2.5f;
    [Export] public float JumpVelocity = 5.0f;
    [Export] public float MouseSensitivity = 0.0025f;
    [Export] public float CameraMinPitch = -80.0f;
    [Export] public float CameraMaxPitch = 60.0f;
    [Export] public float CameraMinDistance = 1.5f;
    [Export] public float CameraMaxDistance = 8.0f;
    [Export] public float CameraZoomStep = 0.5f;
    [Export] public float ShoulderOffsetX = 0.9f;
    [Export] public float ShoulderOffsetY = 0.2f;
    [Export] public float CameraPivotBaseHeight = 1.4f;
    [Export] public PackedScene OrbProjectileScene = null!;
    [Export] public float OrbSpeed = 25.0f;
    [Export] public float OrbSpawnForwardOffset = 0.9f;
    [Export] public float OrbSpawnHeightOffset = 1.55f;

    private Node3D _cameraPivot = null!;
    private SpringArm3D _springArm = null!;
    private Camera3D _camera = null!;
    private CollisionShape3D _collisionShape = null!;
    private MeshInstance3D _visualMesh = null!;
    private float _gravity = 9.8f;
    private float _cameraPitchRadians;
    private bool _isCrouching;
    private float _standingCapsuleHeight = 1.8f;
    private float _crouchingCapsuleHeight = 1.1f;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        _cameraPivot = GetNode<Node3D>("CameraPivot");
        _springArm = GetNode<SpringArm3D>("CameraPivot/SpringArm3D");
        _camera = GetNode<Camera3D>("CameraPivot/SpringArm3D/Camera3D");
        _collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        _visualMesh = GetNode<MeshInstance3D>("MeshInstance3D");

        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        _cameraPivot.Position = new Vector3(ShoulderOffsetX, CameraPivotBaseHeight + ShoulderOffsetY, 0.0f);
        _camera.Position = Vector3.Zero;

        if (_collisionShape.Shape is CapsuleShape3D capsule)
        {
            _standingCapsuleHeight = capsule.Height;
            _crouchingCapsuleHeight = capsule.Height * 0.6f;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        }

        if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && mouseButton.ButtonIndex == MouseButton.Left)
        {
            // Ensure mouse look resumes immediately after regaining focus/clicking viewport.
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        if (@event is InputEventMouseMotion motion && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            RotateY(-motion.Relative.X * MouseSensitivity);
            _cameraPitchRadians = Mathf.Clamp(
                _cameraPitchRadians - motion.Relative.Y * MouseSensitivity,
                Mathf.DegToRad(CameraMinPitch),
                Mathf.DegToRad(CameraMaxPitch)
            );

            _cameraPivot.Rotation = new Vector3(_cameraPitchRadians, 0.0f, 0.0f);
        }

        if (@event.IsActionPressed(InputActions.CameraZoomIn))
        {
            _springArm.SpringLength = Mathf.Max(CameraMinDistance, _springArm.SpringLength - CameraZoomStep);
        }

        if (@event.IsActionPressed(InputActions.CameraZoomOut))
        {
            _springArm.SpringLength = Mathf.Min(CameraMaxDistance, _springArm.SpringLength + CameraZoomStep);
        }

        if (@event.IsActionPressed(InputActions.Fire))
        {
            FireOrb();
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        var velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.Y -= _gravity * (float)delta;
        }

        if (Input.IsActionJustPressed(InputActions.Jump) && IsOnFloor())
        {
            velocity.Y = JumpVelocity;
        }

        var input = Input.GetVector(InputActions.MoveLeft, InputActions.MoveRight, InputActions.MoveForward, InputActions.MoveBack);
        var moveDirection = (Transform.Basis * new Vector3(input.X, 0.0f, input.Y)).Normalized();

        _isCrouching = Input.IsActionPressed(InputActions.Crouch);
        ApplyCrouchState(_isCrouching);

        var currentSpeed = WalkSpeed;
        if (_isCrouching)
        {
            currentSpeed = CrouchSpeed;
        }
        else if (Input.IsActionPressed(InputActions.Sprint))
        {
            currentSpeed = SprintSpeed;
        }

        if (moveDirection != Vector3.Zero)
        {
            velocity.X = moveDirection.X * currentSpeed;
            velocity.Z = moveDirection.Z * currentSpeed;
        }
        else
        {
            velocity.X = Mathf.MoveToward(velocity.X, 0.0f, currentSpeed);
            velocity.Z = Mathf.MoveToward(velocity.Z, 0.0f, currentSpeed);
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    private void ApplyCrouchState(bool crouching)
    {
        if (_collisionShape.Shape is not CapsuleShape3D capsule)
        {
            return;
        }

        capsule.Height = crouching ? _crouchingCapsuleHeight : _standingCapsuleHeight;
        _visualMesh.Scale = crouching ? new Vector3(1.0f, 0.7f, 1.0f) : Vector3.One;
    }

    private void FireOrb()
    {
        if (OrbProjectileScene == null)
        {
            return;
        }

        if (OrbProjectileScene.Instantiate() is not OrbProjectile projectile)
        {
            return;
        }

        var playerForward = -GlobalTransform.Basis.Z;
        var origin = GlobalPosition + (playerForward * OrbSpawnForwardOffset) + (Vector3.Up * OrbSpawnHeightOffset);
        var direction = -_camera.GlobalTransform.Basis.Z;

        projectile.GlobalPosition = origin;
        projectile.Initialize(direction, OrbSpeed);
        GetTree().CurrentScene.AddChild(projectile);
    }
}
