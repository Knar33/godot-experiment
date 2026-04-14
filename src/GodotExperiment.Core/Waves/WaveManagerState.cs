namespace GodotExperiment.Waves;

/// <summary>
/// Tracks wave progression and provides a staggered spawn queue.
/// Waves advance automatically when all enemies for the current wave have been queued.
/// This creates continuous flow with no downtime between waves.
/// </summary>
public class WaveManagerState
{
    private readonly Queue<string> _spawnQueue = new();
    private readonly Random _rng;
    private float _spawnTimer;
    private float _currentSpawnInterval;
    private float _spawnIntervalOverrideSeconds;

    public int CurrentWave { get; private set; }
    public bool IsActive { get; private set; }
    public int RemainingSpawns => _spawnQueue.Count;
    public float SpawnIntervalOverrideSeconds
    {
        get => _spawnIntervalOverrideSeconds;
        set
        {
            _spawnIntervalOverrideSeconds = value;

            if (!IsActive || CurrentWave <= 0)
                return;

            var definition = WaveCompositions.GetWave(CurrentWave);
            _currentSpawnInterval = _spawnIntervalOverrideSeconds > 0f
                ? _spawnIntervalOverrideSeconds
                : definition.SpawnInterval;

            if (_spawnTimer > _currentSpawnInterval)
                _spawnTimer = _currentSpawnInterval;
        }
    }

    public event Action<int>? WaveStarted;

    public WaveManagerState(int? seed = null)
    {
        _rng = seed.HasValue ? new Random(seed.Value) : new Random();
    }

    public void Start()
    {
        IsActive = true;
        CurrentWave = 0;
        _spawnQueue.Clear();
        _spawnTimer = 0f;
        AdvanceWave();
    }

    /// <summary>
    /// Ticks the spawn timer. Returns the enemy type to spawn this frame, or null if waiting.
    /// </summary>
    public string? Update(float dt)
    {
        if (!IsActive) return null;

        _spawnTimer -= dt;
        if (_spawnTimer > 0f) return null;

        if (_spawnQueue.Count == 0)
            AdvanceWave();

        if (_spawnQueue.Count > 0)
        {
            _spawnTimer = _currentSpawnInterval;
            return _spawnQueue.Dequeue();
        }

        return null;
    }

    public void Reset()
    {
        IsActive = false;
        CurrentWave = 0;
        _spawnQueue.Clear();
        _spawnTimer = 0f;
    }

    private void AdvanceWave()
    {
        CurrentWave++;
        var definition = WaveCompositions.GetWave(CurrentWave);
        _currentSpawnInterval = SpawnIntervalOverrideSeconds > 0f
            ? SpawnIntervalOverrideSeconds
            : definition.SpawnInterval;

        var enemies = new List<string>();
        foreach (var group in definition.Groups)
        {
            for (int i = 0; i < group.Count; i++)
                enemies.Add(group.EnemyType);
        }

        Shuffle(enemies);

        foreach (var e in enemies)
            _spawnQueue.Enqueue(e);

        WaveStarted?.Invoke(CurrentWave);
    }

    private void Shuffle(List<string> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = _rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
