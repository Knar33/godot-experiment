using Godot;

namespace GodotExperiment;

public partial class GemPickup : Area3D
{
    [Signal]
    public delegate void CollectedEventHandler();

    private Vector3 _scatterVelocity;
    private float _scatterTime;
    private const float ScatterDuration = 0.3f;
    private const float ScatterDamping = 5f;

    private Node3D? _magnetTarget;
    private float _magnetTime;
    private const float MagnetDuration = 0.12f;
    private const float MagnetSpeed = 50f;

    public override void _Ready()
    {
        AddToGroup("gems");
    }

    public void SetScatterVelocity(Vector3 velocity)
    {
        _scatterVelocity = velocity;
        _scatterTime = ScatterDuration;
    }

    public void StartMagnetism(Node3D target)
    {
        _magnetTarget = target;
        _magnetTime = 0f;
        _scatterTime = 0f;
        _scatterVelocity = Vector3.Zero;
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (_magnetTarget != null)
        {
            ProcessMagnetism(dt);
            return;
        }

        if (_scatterTime <= 0f) return;

        _scatterTime -= dt;
        GlobalPosition += _scatterVelocity * dt;
        _scatterVelocity *= 1f - ScatterDamping * dt;

        if (_scatterTime <= 0f)
        {
            _scatterVelocity = Vector3.Zero;
            var pos = GlobalPosition;
            pos.Y = 0.15f;
            GlobalPosition = pos;
        }
    }

    private void ProcessMagnetism(float dt)
    {
        if (!IsInstanceValid(_magnetTarget))
        {
            _magnetTarget = null;
            return;
        }

        _magnetTime += dt;

        Vector3 targetPos = _magnetTarget.GlobalPosition + new Vector3(0, 0.6f, 0);
        Vector3 toTarget = targetPos - GlobalPosition;
        float dist = toTarget.Length();

        if (dist < 0.3f || _magnetTime >= MagnetDuration)
        {
            EmitSignal(SignalName.Collected);
            QueueFree();
            return;
        }

        float t = Mathf.Clamp(_magnetTime / MagnetDuration, 0f, 1f);
        float speed = Mathf.Lerp(MagnetSpeed * 0.5f, MagnetSpeed, t * t);
        Vector3 moveDir = toTarget.Normalized();
        moveDir.Y += 0.3f * (1f - t);
        moveDir = moveDir.Normalized();

        GlobalPosition += moveDir * speed * dt;
    }
}
