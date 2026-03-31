using Godot;
using GodotExperiment.Enemies;
using GodotExperiment.GameLoop;

namespace GodotExperiment;

/// <summary>
/// Spawns enemy instances at arena-edge spawn points with staggered timing,
/// randomized selection, and a bias against spawning behind the player.
/// </summary>
public partial class EnemySpawner : Node3D
{
	[Export] public float SpawnInterval { get; set; } = 2.0f;
	[Export] public PackedScene? EnemyScene { get; set; }
	[Export] public bool WaveManaged { get; set; }

	private Arena? _arena;
	private Node3D? _enemiesContainer;
	private float _spawnTimer;
	private RandomNumberGenerator _rng = new();

	public override void _Ready()
	{
		_arena = GetTree().CurrentScene.GetNodeOrNull<Arena>("Arena");
		_enemiesContainer = GetTree().CurrentScene.GetNodeOrNull<Node3D>("Enemies");
	}

	public override void _PhysicsProcess(double delta)
	{
		if (WaveManaged) return;
		if (GameManager.Instance?.CurrentState != GameState.Playing) return;
		if (_arena == null || _enemiesContainer == null || EnemyScene == null) return;

		_spawnTimer -= (float)delta;
		if (_spawnTimer > 0f) return;

		_spawnTimer = SpawnInterval;
		SpawnEnemy();
	}

	public void SpawnEnemy()
	{
		if (_arena == null || _enemiesContainer == null || EnemyScene == null) return;

		Vector3 spawnPos = SelectSpawnPoint();
		var enemy = EnemyScene.Instantiate<Node3D>();
		_enemiesContainer.AddChild(enemy);
		enemy.GlobalPosition = spawnPos;
	}

	public void SpawnEnemyOfType(PackedScene scene)
	{
		if (_arena == null || _enemiesContainer == null) return;

		Vector3 spawnPos = SelectSpawnPoint();
		var enemy = scene.Instantiate<Node3D>();
		_enemiesContainer.AddChild(enemy);
		enemy.GlobalPosition = spawnPos;
	}

	public void SpawnEnemyAt(PackedScene scene, Vector3 position)
	{
		if (_enemiesContainer == null) return;

		var enemy = scene.Instantiate<Node3D>();
		_enemiesContainer.AddChild(enemy);
		enemy.GlobalPosition = position;
	}

	public Vector3 SelectSpawnPoint()
	{
		if (_arena!.SpawnPoints.Length == 0)
			return Vector3.Zero;

		var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
		if (player == null)
			return _arena.SpawnPoints[(int)_rng.Randi() % _arena.SpawnPoints.Length];

		int count = _arena.SpawnPoints.Length;
		float[] sx = new float[count];
		float[] sz = new float[count];

		for (int i = 0; i < count; i++)
		{
			sx[i] = _arena.SpawnPoints[i].X;
			sz[i] = _arena.SpawnPoints[i].Z;
		}

		Vector3 pPos = player.GlobalPosition;
		Vector3 pFwd = GetPlayerForward(player);

		float[] weights = SpawnPointSelector.ComputeWeights(
			sx, sz,
			pPos.X, pPos.Z,
			pFwd.X, pFwd.Z);

		int index = SpawnPointSelector.SelectWeighted(weights, _rng.Randf());
		return _arena.SpawnPoints[index];
	}

	private static Vector3 GetPlayerForward(Node3D player)
	{
		Camera3D? camera = player.GetViewport()?.GetCamera3D();
		if (camera == null)
			return Vector3.Forward;

		Vector3 forward = -camera.GlobalTransform.Basis.Z;
		forward.Y = 0;
		return forward.LengthSquared() > 0.001f ? forward.Normalized() : Vector3.Forward;
	}

	public void Reset()
	{
		_spawnTimer = 0f;
	}
}
