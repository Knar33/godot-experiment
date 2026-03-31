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
    /// <summary>Lowers camera pivot while crouched so view matches shorter capsule.</summary>
    [Export] public float CrouchCameraPivotYOffset = -0.55f;
    /// <summary>Lowers projectile spawn while crouched to match shorter stance.</summary>
    [Export] public float CrouchOrbHeightOffset = -0.35f;
    /// <summary>Lowers the imported character mesh locally on Y when crouched (no squash scale; avoids full-height idle while capsule is short).</summary>
    [Export] public float CrouchMeshLocalYOffset = -0.22f;
    /// <summary>
    /// When true, locomotion uses AnimationTree blend parameters (additive shooting blend). When false (default),
    /// locomotion uses AnimationPlayer directly with crossfades — avoids tree quirks and clips that stop after one cycle
    /// if imported loop mode is wrong (often looks like "animation breaks after a few seconds").
    /// </summary>
    [Export] public bool UseAnimationTreeForLocomotion = false;
    [Export] public float LocomotionCrossFadeSeconds = 0.18f;

    private Node3D _cameraPivot = null!;
    private SpringArm3D _springArm = null!;
    private Camera3D _camera = null!;
    private CollisionShape3D _collisionShape = null!;
    private Node3D _character = null!;
    private AnimationTree _animationTree = null!;
    private AnimationPlayer? _animPlayer;
    private string _directLocoAnim = "";
    private bool _directLocoPrevCrouch;
    private float _characterStandingLocalY;
    private float _gravity = 9.8f;
    private float _shootBlend;
    private float _cameraPitchRadians;
    private bool _isCrouching;
    private float _standingCapsuleHeight = 1.8f;
    private float _crouchingCapsuleHeight = 1.1f;
    private float _capsuleBottomLocalY;
    private float _cameraPivotStandingY;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;

        _cameraPivot = GetNode<Node3D>("CameraPivot");
        _springArm = GetNode<SpringArm3D>("CameraPivot/SpringArm3D");
        _camera = GetNode<Camera3D>("CameraPivot/SpringArm3D/Camera3D");
        _collisionShape = GetNode<CollisionShape3D>("CollisionShape3D");
        _character = GetNode<Node3D>("Character");
        _characterStandingLocalY = _character.Position.Y;
        _animationTree = GetNode<AnimationTree>("AnimationTree");

        var animPlayer = _character.FindChild("AnimationPlayer", recursive: true, owned: false) as AnimationPlayer;
        if (animPlayer != null)
        {
            _animPlayer = animPlayer;
            _animationTree.AnimPlayer = GetPathTo(animPlayer);
        }
        else
        {
            GD.PushError("AnimationPlayer not found under Character; AnimationTree will not run.");
        }

        if (_animPlayer != null)
        {
            // GLTF clips often import without loop; locomotion freezes after one cycle (~few seconds).
            EnsureImportedAnimationsLoop(_animPlayer);
        }

        if (UseAnimationTreeForLocomotion)
        {
            _animationTree.Active = true;
        }
        else
        {
            // Tree must be off or it owns the AnimationPlayer and fights direct Play().
            _animationTree.Active = false;
        }

        _animationTree.Deterministic = false;
        _animationTree.CallbackModeProcess = AnimationMixer.AnimationCallbackModeProcess.Physics;

        _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");
        _cameraPivotStandingY = CameraPivotBaseHeight + ShoulderOffsetY;
        _cameraPivot.Position = new Vector3(ShoulderOffsetX, _cameraPivotStandingY, 0.0f);
        _camera.Position = Vector3.Zero;

        if (_collisionShape.Shape is CapsuleShape3D capsule)
        {
            _standingCapsuleHeight = capsule.Height;
            _crouchingCapsuleHeight = capsule.Height * 0.6f;
            _capsuleBottomLocalY = _collisionShape.Position.Y - _standingCapsuleHeight * 0.5f;
        }

        // Reduces IsOnFloor() flicker on edges; keeps ground locomotion / crouch blends stable.
        FloorSnapLength = 0.18f;
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

        _isCrouching = IsCrouchInput();
        ApplyCrouchState(_isCrouching);

        var currentSpeed = WalkSpeed;
        if (_isCrouching)
        {
            currentSpeed = CrouchSpeed;
        }
        else if (IsSprintInput())
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

        if (UseAnimationTreeForLocomotion)
        {
            UpdateAnimationTree((float)delta, input);
        }
        else
        {
            UpdateDirectLocomotion((float)delta, input);
        }
    }

    /// <summary>
    /// GLTF clips often import with loop disabled; one-shot clips freeze after the first play (~seconds of motion).
    /// </summary>
    private static void EnsureImportedAnimationsLoop(AnimationPlayer player)
    {
        foreach (StringName libName in player.GetAnimationLibraryList())
        {
            var lib = player.GetAnimationLibrary(libName);
            if (lib == null)
            {
                continue;
            }

            foreach (StringName animName in lib.GetAnimationList())
            {
                var anim = lib.GetAnimation(animName);
                if (anim == null)
                {
                    continue;
                }

                var n = animName.ToString();
                // One-shot: jump. Everything else we need to loop for locomotion.
                anim.LoopMode = n == "jump"
                    ? Animation.LoopModeEnum.None
                    : Animation.LoopModeEnum.Linear;
            }
        }
    }

    private void UpdateDirectLocomotion(float delta, Vector2 moveInput)
    {
        if (_animPlayer == null)
        {
            return;
        }

        var horizontal = new Vector3(Velocity.X, 0.0f, Velocity.Z);
        var horizontalSpeed = horizontal.Length();
        var maxSpeed = Mathf.Max(SprintSpeed, 0.001f);
        var onFloor = IsOnFloor();
        var moveIntent = moveInput.Length() > 0.08f;
        var sprintHeld = IsSprintInput();
        var sprintIntent = sprintHeld && !_isCrouching && (moveIntent || horizontalSpeed > 0.2f);

        string target;
        if (!onFloor)
        {
            target = Velocity.Y > 0.15f ? "jump" : "falling";
        }
        // Crouch movement uses slow CrouchSpeed — horizontal speed threshold can be below 0.12; keep moveIntent primary.
        else if (_isCrouching && (moveIntent || horizontalSpeed > 0.05f))
        {
            target = "walk";
        }
        else if (_isCrouching)
        {
            target = "idle";
        }
        else if (sprintIntent && (moveIntent || horizontalSpeed > 0.12f))
        {
            target = "run";
        }
        else if (moveIntent || horizontalSpeed > 0.12f)
        {
            target = "walk";
        }
        else
        {
            target = "idle";
        }

        // Same clip name when toggling crouch (e.g. walk→walk) must still Play() so SpeedScale / playback refresh.
        if (target != _directLocoAnim || _isCrouching != _directLocoPrevCrouch)
        {
            _directLocoAnim = target;
            _directLocoPrevCrouch = _isCrouching;
            _animPlayer.Play(new StringName(target), LocomotionCrossFadeSeconds, 1.0f, false);
        }

        var scale = 1.0f;
        if (!_isCrouching && sprintHeld && (moveIntent || horizontalSpeed > 0.12f) && target == "run")
        {
            scale = 1.5f;
        }
        else if (_isCrouching && target == "walk")
        {
            scale = 0.72f;
        }
        else if (_isCrouching && target == "idle")
        {
            scale = 0.82f;
        }

        _animPlayer.SpeedScale = scale;
    }

    private void UpdateAnimationTree(float delta, Vector2 moveInput)
    {
        var horizontal = new Vector3(Velocity.X, 0.0f, Velocity.Z);
        var horizontalSpeed = horizontal.Length();
        var maxSpeed = Mathf.Max(SprintSpeed, 0.001f);
        var onFloor = IsOnFloor();
        var moveIntent = moveInput.Length() > 0.08f;
        var sprintHeld = IsSprintInput();
        // Shift+W can briefly report lower axis length; keep sprint intent if shift held and we are clearly moving.
        var sprintIntent = sprintHeld && !_isCrouching && (moveIntent || horizontalSpeed > 0.2f);

        // Blend2 "run": 0 = idle, 1 = "speed" subtree (walk vs run clips).
        var runBlend = Mathf.Clamp(horizontalSpeed / maxSpeed, 0.0f, 1.0f);
        if (onFloor && !_isCrouching && moveIntent)
        {
            var inputBlend = Mathf.Clamp(moveInput.Length(), 0.0f, 1.0f);
            runBlend = Mathf.Max(runBlend, inputBlend * 0.92f);
        }

        // Blend2 "speed": 0 = walk, 1 = run. Do not derive run from (speed - walkThreshold): small velocity dips
        // drop speedBlend to 0 and the run clip vanishes after a few steps. Use sprint intent + floor instead.
        var speedBlend = 0.0f;
        if (onFloor && !_isCrouching && (moveIntent || horizontalSpeed > 0.12f))
        {
            if (sprintIntent)
            {
                speedBlend = Mathf.Clamp(Mathf.Max(horizontalSpeed / maxSpeed, 0.55f), 0.0f, 1.0f);
            }
        }
        else if (onFloor && _isCrouching && (moveIntent || horizontalSpeed > 0.12f))
        {
            // No crouch clip: walk-heavy locomotion + slower time scale. Require floor OR move intent so
            // IsOnFloor() flicker does not wipe the blend (previous else-if crouch branch did that).
            runBlend = Mathf.Max(runBlend, Mathf.Max(0.48f, Mathf.Clamp(horizontalSpeed / maxSpeed, 0.0f, 1.0f)));
            speedBlend = Mathf.Clamp(horizontalSpeed / Mathf.Max(CrouchSpeed * 1.15f, 0.001f), 0.0f, 0.5f);
        }

        if (_isCrouching && !moveIntent && horizontalSpeed <= 0.12f)
        {
            runBlend = 0.0f;
            speedBlend = 0.0f;
        }

        SetAnimParam("parameters/run/blend_amount", runBlend);
        SetAnimParam("parameters/speed/blend_amount", speedBlend);
        SetAnimParam("parameters/state/blend_amount", onFloor ? 0.0f : 1.0f);
        SetAnimParam("parameters/air_dir/blend_amount", Mathf.Clamp(-Velocity.Y / 4.0f + 0.5f, 0.0f, 1.0f));

        var aimTarget = Input.IsActionPressed(InputActions.Fire) ? 1.0f : 0.0f;
        _shootBlend = Mathf.MoveToward(_shootBlend, aimTarget, delta * 6.0f);
        SetAnimParam("parameters/gun/blend_amount", _shootBlend);

        var runScale = 1.0f;
        if (!_isCrouching && sprintHeld && (moveIntent || horizontalSpeed > 0.12f))
        {
            runScale = 1.5f;
        }
        else if (_isCrouching && (moveIntent || horizontalSpeed > 0.1f))
        {
            runScale = 0.72f;
        }

        SetAnimParam("parameters/scale/scale", runScale);
    }

    private void SetAnimParam(string path, float value)
    {
        // platformer_animation_tree.gd — GDScript set() matches inspector; C# GodotObject.Set on dynamic params is unreliable.
        _animationTree.Call("set_blend_param", path, value);
    }

    /// <summary>
    /// Sprint: InputMap can mis-report modifier keys (Shift) via IsActionPressed; poll physical keys too.
    /// E is a non-modifier alternate merged in <see cref="InputActions.EnsureDefaultBindings"/>.
    /// </summary>
    private static bool IsSprintInput()
    {
        return Input.IsActionPressed(InputActions.Sprint)
            || Input.IsPhysicalKeyPressed(Key.Shift)
            || Input.IsPhysicalKeyPressed(Key.E);
    }

    /// <summary>Crouch: same modifier/polling caveats; physical C / Ctrl cover layouts where actions fail.</summary>
    private static bool IsCrouchInput()
    {
        return Input.IsActionPressed(InputActions.Crouch)
            || Input.IsPhysicalKeyPressed(Key.C)
            || Input.IsPhysicalKeyPressed(Key.Ctrl);
    }

    private void ApplyCrouchState(bool crouching)
    {
        if (_collisionShape.Shape is not CapsuleShape3D capsule)
        {
            return;
        }

        var height = crouching ? _crouchingCapsuleHeight : _standingCapsuleHeight;
        capsule.Height = height;
        var centerY = _capsuleBottomLocalY + height * 0.5f;
        _collisionShape.Position = new Vector3(_collisionShape.Position.X, centerY, _collisionShape.Position.Z);

        _cameraPivot.Position = new Vector3(
            ShoulderOffsetX,
            _cameraPivotStandingY + (crouching ? CrouchCameraPivotYOffset : 0.0f),
            0.0f
        );

        var localY = crouching ? _characterStandingLocalY + CrouchMeshLocalYOffset : _characterStandingLocalY;
        _character.Position = new Vector3(_character.Position.X, localY, _character.Position.Z);
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
        var crouching = IsCrouchInput();
        var orbHeight = OrbSpawnHeightOffset + (crouching ? CrouchOrbHeightOffset : 0.0f);
        var origin = GlobalPosition + (playerForward * OrbSpawnForwardOffset) + (Vector3.Up * orbHeight);
        var direction = -_camera.GlobalTransform.Basis.Z;

        var root = GetTree().CurrentScene;
        if (root == null)
        {
            projectile.QueueFree();
            return;
        }

        root.AddChild(projectile);
        projectile.GlobalPosition = origin;
        projectile.Initialize(direction, OrbSpeed);
    }
}
