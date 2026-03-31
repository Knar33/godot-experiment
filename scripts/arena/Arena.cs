using Godot;
using GodotExperiment.Combat;

namespace GodotExperiment;

public partial class Arena : Node3D
{
    [Export] public float Radius { get; set; } = 25f;
    [Export] public int SpawnPointCount { get; set; } = 12;
    [Export] public float SpawnPointInset { get; set; } = 1.5f;
    [Export] public float KillboxDepth { get; set; } = -20f;

    public Vector3[] SpawnPoints { get; private set; } = [];

    public override void _Ready()
    {
        CreateSpawnPoints();
        CreateKillbox();
    }

    private void CreateSpawnPoints()
    {
        float spawnRadius = Radius - SpawnPointInset;
        SpawnPoints = new Vector3[SpawnPointCount];
        float angleStep = Mathf.Tau / SpawnPointCount;

        for (int i = 0; i < SpawnPointCount; i++)
        {
            float angle = i * angleStep;
            SpawnPoints[i] = new Vector3(
                Mathf.Cos(angle) * spawnRadius,
                0f,
                Mathf.Sin(angle) * spawnRadius
            );

            var marker = new Marker3D
            {
                Position = SpawnPoints[i],
                Name = $"SpawnPoint_{i}"
            };
            AddChild(marker);
        }
    }

    private void CreateKillbox()
    {
        var killbox = new Area3D
        {
            Name = "Killbox",
            CollisionLayer = 0,
            CollisionMask = 0b1001, // layers 1 (player/geometry) and 4 (enemies)
            Monitoring = true,
            Monitorable = false
        };

        float boxSize = Radius * 4f;
        var shape = new BoxShape3D
        {
            Size = new Vector3(boxSize, 1f, boxSize)
        };
        var collision = new CollisionShape3D
        {
            Shape = shape,
            Position = new Vector3(0, KillboxDepth, 0)
        };

        killbox.AddChild(collision);
        AddChild(killbox);

        killbox.BodyEntered += OnKillboxBodyEntered;
    }

    private void OnKillboxBodyEntered(Node3D body)
    {
        if (body.IsInGroup("player") && body is Player player)
        {
            player.TakeDamage(DamageSource.GroundHazard);
        }
        else if (body.IsInGroup("enemy") && body is BaseEnemy enemy)
        {
            enemy.QueueFree();
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        return SpawnPoints[GD.RandRange(0, SpawnPoints.Length - 1)];
    }
}
