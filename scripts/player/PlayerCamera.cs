using Godot;

namespace GodotExperiment;

public partial class PlayerCamera : Node3D
{
    [Export] public float MouseSensitivity { get; set; } = 0.002f;
    [Export] public float Distance { get; set; } = 8f;
    [Export] public float MinPitch { get; set; } = -80f;
    [Export] public float MaxPitch { get; set; } = 60f;
    [Export] public float FollowSpeed { get; set; } = 20f;
    [Export] public float VerticalOffset { get; set; } = 1.5f;
    [Export] public float AimRayLength { get; set; } = 100f;
    [Export] public float ClipMargin { get; set; } = 0.3f;

    private Camera3D _camera = null!;
    private float _yaw;
    private float _pitch = Mathf.DegToRad(-15f);
    private Vector3 _orbitCenter;
    private Node3D? _player;

    public Vector3 AimPoint { get; private set; }
    public bool HasAimTarget { get; private set; }

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion
            && Input.MouseMode == Input.MouseModeEnum.Captured)
        {
            _yaw -= mouseMotion.Relative.X * MouseSensitivity;
            _pitch -= mouseMotion.Relative.Y * MouseSensitivity;
            _pitch = Mathf.Clamp(
                _pitch,
                Mathf.DegToRad(MinPitch),
                Mathf.DegToRad(MaxPitch));
        }

        if (@event is InputEventMouseButton mouseButton
            && mouseButton.Pressed
            && Input.MouseMode != Input.MouseModeEnum.Captured)
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }

        if (@event.IsActionPressed("pause"))
        {
            Input.MouseMode = Input.MouseMode == Input.MouseModeEnum.Captured
                ? Input.MouseModeEnum.Visible
                : Input.MouseModeEnum.Captured;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (_player == null)
        {
            _player = GetTree().GetFirstNodeInGroup("player") as Node3D;
            if (_player == null) return;
            _orbitCenter = _player.GlobalPosition + Vector3.Up * VerticalOffset;
        }

        Vector3 target = _player.GlobalPosition + Vector3.Up * VerticalOffset;
        float t = 1f - Mathf.Exp(-FollowSpeed * dt);
        _orbitCenter = _orbitCenter.Lerp(target, t);

        _camera.GlobalRotation = new Vector3(_pitch, _yaw, 0);

        Vector3 backward = _camera.GlobalTransform.Basis.Z;
        Vector3 desiredPos = _orbitCenter + backward * Distance;

        _camera.GlobalPosition = GetClippedPosition(_orbitCenter, desiredPos);

        UpdateAimPoint();
    }

    /// <summary>
    /// Raycasts from the orbit center toward the desired camera position.
    /// If geometry is hit, the camera is pulled forward to avoid clipping.
    /// </summary>
    private Vector3 GetClippedPosition(Vector3 from, Vector3 to)
    {
        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(from, to);

        if (_player is CollisionObject3D col)
            query.Exclude = new Godot.Collections.Array<Rid> { col.GetRid() };

        var result = spaceState.IntersectRay(query);
        if (result.Count > 0)
        {
            Vector3 hitPoint = (Vector3)result["position"];
            Vector3 hitNormal = (Vector3)result["normal"];
            return hitPoint + hitNormal * ClipMargin;
        }

        return to;
    }

    /// <summary>
    /// Casts a ray from the camera center into the world to determine
    /// the exact 3D point the player is aiming at.
    /// </summary>
    private void UpdateAimPoint()
    {
        Vector2 screenCenter = GetViewport().GetVisibleRect().Size / 2f;
        Vector3 rayOrigin = _camera.ProjectRayOrigin(screenCenter);
        Vector3 rayDir = _camera.ProjectRayNormal(screenCenter);
        Vector3 rayEnd = rayOrigin + rayDir * AimRayLength;

        var spaceState = GetWorld3D().DirectSpaceState;
        var query = PhysicsRayQueryParameters3D.Create(rayOrigin, rayEnd);

        if (_player is CollisionObject3D col)
            query.Exclude = new Godot.Collections.Array<Rid> { col.GetRid() };

        var result = spaceState.IntersectRay(query);
        if (result.Count > 0)
        {
            AimPoint = (Vector3)result["position"];
            HasAimTarget = true;
        }
        else
        {
            AimPoint = rayEnd;
            HasAimTarget = false;
        }
    }
}
