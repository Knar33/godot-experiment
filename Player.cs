using Godot;

namespace GodotExperiment;

public partial class Player : CharacterBody3D
{
	private const float WalkSpeed = 4.0f;
	private const float RunSpeed = 8.0f;
	private const float CrouchSpeedMult = 0.45f;
	private const float JumpVelocity = 4.8f;
	private const float Acceleration = 14.0f;
	private const float Friction = 18.0f;

	[Export] public float StandingHeight { get; set; } = 1.6f;
	[Export] public float CrouchHeight { get; set; } = 0.85f;
	[Export] public float CapsuleRadius { get; set; } = 0.35f;

	private float _gravity;
	private bool _crouching;

	private CollisionShape3D _collision = null!;
	private MeshInstance3D _bodyMesh = null!;

	public override void _Ready()
	{
		_gravity = (float)(double)ProjectSettings.GetSetting("physics/3d/default_gravity");
		_collision = GetNode<CollisionShape3D>("CollisionShape3D");
		_bodyMesh = GetNode<MeshInstance3D>("BodyMesh");
		ApplyCapsuleHeight(_crouching ? CrouchHeight : StandingHeight);
	}

	public override void _PhysicsProcess(double delta)
	{
		float dt = (float)delta;
		bool wantCrouch = Input.IsActionPressed("crouch");
		if (wantCrouch != _crouching)
		{
			_crouching = wantCrouch;
			ApplyCapsuleHeight(_crouching ? CrouchHeight : StandingHeight);
		}

		if (!IsOnFloor())
			Velocity = new Vector3(Velocity.X, Velocity.Y - _gravity * dt, Velocity.Z);

		if (Input.IsActionJustPressed("jump") && IsOnFloor() && !_crouching)
			Velocity = new Vector3(Velocity.X, JumpVelocity, Velocity.Z);

		Camera3D? cam = GetViewport().GetCamera3D();
		Vector2 inputVec = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
		Vector3 wishDir = Vector3.Zero;
		if (inputVec.LengthSquared() > 0.0001f)
		{
			if (cam != null)
			{
				Vector3 forward = -cam.GlobalTransform.Basis.Z;
				forward.Y = 0f;
				Vector3 right = cam.GlobalTransform.Basis.X;
				right.Y = 0f;
				if (forward.LengthSquared() > 0.0001f && right.LengthSquared() > 0.0001f)
				{
					forward = forward.Normalized();
					right = right.Normalized();
					wishDir = (right * inputVec.X + forward * (-inputVec.Y)).Normalized();
				}
			}
			else
				wishDir = new Vector3(inputVec.X, 0f, -inputVec.Y).Normalized();
		}

		float targetSpeed = Input.IsActionPressed("run") && !_crouching ? RunSpeed : WalkSpeed;
		if (_crouching)
			targetSpeed *= CrouchSpeedMult;

		Vector3 horiz = new(Velocity.X, 0f, Velocity.Z);
		if (wishDir.LengthSquared() > 0f)
		{
			Vector3 targetVel = wishDir * targetSpeed;
			horiz = horiz.MoveToward(targetVel, Acceleration * dt);
		}
		else
			horiz = horiz.MoveToward(Vector3.Zero, Friction * dt);

		Velocity = new Vector3(horiz.X, Velocity.Y, horiz.Z);
		MoveAndSlide();
	}

	private void ApplyCapsuleHeight(float h)
	{
		var capShape = (CapsuleShape3D)_collision.Shape;
		var capMesh = (CapsuleMesh)_bodyMesh.Mesh;
		capShape.Radius = CapsuleRadius;
		capShape.Height = h;
		capMesh.Radius = CapsuleRadius;
		capMesh.Height = h;
		float cy = h * 0.5f;
		_collision.Position = new Vector3(_collision.Position.X, cy, _collision.Position.Z);
		_bodyMesh.Position = new Vector3(_bodyMesh.Position.X, cy, _bodyMesh.Position.Z);
	}
}
