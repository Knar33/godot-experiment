using Godot;
using GodotExperiment.Combat;

namespace GodotExperiment;

public partial class SpitterGroundHazard : Area3D
{
    [Export] public float Duration { get; set; } = 1.5f;

    private float _timer;
    private MeshInstance3D? _mesh;

    public override void _Ready()
    {
        BodyEntered += OnBodyEntered;
        _mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
    }

    public override void _PhysicsProcess(double delta)
    {
        _timer += (float)delta;

        if (_mesh != null)
        {
            float alpha = 1f - (_timer / Duration);
            var mat = _mesh.GetActiveMaterial(0) as StandardMaterial3D;
            if (mat != null)
                mat.AlbedoColor = new Color(mat.AlbedoColor.R, mat.AlbedoColor.G, mat.AlbedoColor.B, alpha);
        }

        if (_timer >= Duration)
            QueueFree();
    }

    private void OnBodyEntered(Node3D body)
    {
        if (!body.IsInGroup("player")) return;

        if (body is Player player)
            player.TakeDamage(DamageSource.GroundHazard);
    }
}
