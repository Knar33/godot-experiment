using Godot;

namespace GodotExperiment;

public partial class CameraFollow : Camera3D
{
	[Export] public NodePath TargetPath { get; set; } = new();
	[Export] public float Height { get; set; } = 10f;
	[Export] public float Distance { get; set; } = 12f;
	[Export] public float SmoothSpeed { get; set; } = 8f;

	private Node3D? _target;

	public override void _Ready()
	{
		if (!TargetPath.IsEmpty)
			_target = GetNodeOrNull<Node3D>(TargetPath);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_target == null)
			return;

		float dt = (float)delta;
		Vector3 tpos = _target.GlobalPosition;
		Vector3 desired = tpos + new Vector3(0f, Height, Distance);
		float t = 1f - Mathf.Exp(-SmoothSpeed * dt);
		GlobalPosition = GlobalPosition.Lerp(desired, t);
		LookAt(tpos, Vector3.Up);
	}
}
