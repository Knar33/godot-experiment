using Godot;
using GodotExperiment.GameLoop;
using GodotExperiment.Waves;
using System.Collections.Generic;

namespace GodotExperiment;

/// <summary>
/// Drives wave-based enemy spawning. Controls timing and composition via
/// WaveManagerState (Core) and delegates actual spawning to EnemySpawner.
/// Gracefully skips enemy types whose scenes don't exist yet.
/// </summary>
public partial class WaveManager : Node
{
	[Export] public NodePath EnemySpawnerPath { get; set; } = "";

	private WaveManagerState _state = new();
	private EnemySpawner? _spawner;
	private readonly Dictionary<string, PackedScene> _enemyScenes = new();

	[Signal]
	public delegate void WaveStartedEventHandler(int waveNumber);

	public int CurrentWave => _state.CurrentWave;

	public override void _Ready()
	{
		_spawner = GetNodeOrNull<EnemySpawner>(EnemySpawnerPath);
		if (_spawner == null)
		{
			var scene = GetTree().CurrentScene;
			_spawner = scene?.GetNodeOrNull<EnemySpawner>("EnemySpawner");
		}

		RegisterEnemyScenes();
		_state.WaveStarted += OnWaveStarted;

		if (GameManager.Instance != null)
		{
			GameManager.Instance.CountdownFinished += OnCountdownFinished;
			GameManager.Instance.StateChanged += OnGameStateChanged;
		}
	}

	public override void _ExitTree()
	{
		_state.WaveStarted -= OnWaveStarted;
		if (GameManager.Instance != null)
		{
			GameManager.Instance.CountdownFinished -= OnCountdownFinished;
			GameManager.Instance.StateChanged -= OnGameStateChanged;
		}
	}

	public override void _PhysicsProcess(double delta)
	{
		if (GameManager.Instance?.CurrentState != GameState.Playing) return;

		string? enemyType = _state.Update((float)delta);
		if (enemyType != null)
			SpawnEnemyByType(enemyType);
	}

	public void Reset()
	{
		_state.Reset();
	}

	private void OnCountdownFinished()
	{
		_state.Start();
	}

	private void OnGameStateChanged(int previous, int current)
	{
		var state = (GameState)current;
		if (state == GameState.Dead || state == GameState.Countdown)
			_state.Reset();
	}

	private void OnWaveStarted(int waveNumber)
	{
		GameManager.Instance?.RecordWaveReached(waveNumber);
		EmitSignal(SignalName.WaveStarted, waveNumber);
	}

	private void SpawnEnemyByType(string enemyType)
	{
		if (_spawner == null) return;

		if (!_enemyScenes.TryGetValue(enemyType, out var scene))
			return;

		_spawner.SpawnEnemyOfType(scene);
	}

	private void RegisterEnemyScenes()
	{
		TryRegister(WaveCompositions.Crawler, "res://scenes/enemies/Crawler.tscn");
		TryRegister(WaveCompositions.Spitter, "res://scenes/enemies/Spitter.tscn");
		TryRegister(WaveCompositions.Charger, "res://scenes/enemies/Charger.tscn");
		TryRegister(WaveCompositions.Drone, "res://scenes/enemies/Drone.tscn");
		TryRegister(WaveCompositions.Bloater, "res://scenes/enemies/Bloater.tscn");
		TryRegister(WaveCompositions.Shade, "res://scenes/enemies/Shade.tscn");
		TryRegister(WaveCompositions.Burrower, "res://scenes/enemies/Burrower.tscn");
		TryRegister(WaveCompositions.Sentinel, "res://scenes/enemies/Sentinel.tscn");
		TryRegister(WaveCompositions.Howler, "res://scenes/enemies/Howler.tscn");
		TryRegister(WaveCompositions.Titan, "res://scenes/enemies/Titan.tscn");
	}

	private void TryRegister(string type, string path)
	{
		if (ResourceLoader.Exists(path))
			_enemyScenes[type] = GD.Load<PackedScene>(path);
	}
}
