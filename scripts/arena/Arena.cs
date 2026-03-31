using Godot;

namespace GodotExperiment;

public partial class Arena : Node3D
{
    [Export] public float Radius { get; set; } = 25f;
    [Export] public int BoundarySegments { get; set; } = 32;
    [Export] public float WallHeight { get; set; } = 4f;
    [Export] public float WallThickness { get; set; } = 0.5f;
    [Export] public int SpawnPointCount { get; set; } = 12;
    [Export] public float SpawnPointRadius { get; set; } = 28f;

    public Vector3[] SpawnPoints { get; private set; } = [];

    public override void _Ready()
    {
        CreateBoundaryWalls();
        CreateSpawnPoints();
    }

    private void CreateBoundaryWalls()
    {
        float angleStep = Mathf.Tau / BoundarySegments;
        float arcLength = Mathf.Tau * Radius / BoundarySegments;
        float segmentLength = arcLength * 1.1f;
        float wallCenterRadius = Radius + WallThickness / 2f;

        for (int i = 0; i < BoundarySegments; i++)
        {
            float angle = i * angleStep;

            var wall = new StaticBody3D
            {
                Position = new Vector3(
                    Mathf.Cos(angle) * wallCenterRadius,
                    WallHeight / 2f,
                    Mathf.Sin(angle) * wallCenterRadius
                ),
                Name = $"BoundaryWall_{i}"
            };
            wall.RotateY(-angle + Mathf.Pi / 2f);

            var shape = new BoxShape3D
            {
                Size = new Vector3(segmentLength, WallHeight, WallThickness)
            };
            var collision = new CollisionShape3D { Shape = shape };

            wall.AddChild(collision);
            AddChild(wall);
        }
    }

    private void CreateSpawnPoints()
    {
        SpawnPoints = new Vector3[SpawnPointCount];
        float angleStep = Mathf.Tau / SpawnPointCount;

        for (int i = 0; i < SpawnPointCount; i++)
        {
            float angle = i * angleStep;
            SpawnPoints[i] = new Vector3(
                Mathf.Cos(angle) * SpawnPointRadius,
                0f,
                Mathf.Sin(angle) * SpawnPointRadius
            );

            var marker = new Marker3D
            {
                Position = SpawnPoints[i],
                Name = $"SpawnPoint_{i}"
            };
            AddChild(marker);
        }
    }

    public Vector3 GetRandomSpawnPoint()
    {
        return SpawnPoints[GD.RandRange(0, SpawnPoints.Length - 1)];
    }
}
