using GodotExperiment.Waves;
using Xunit;

namespace GodotExperiment.Tests;

public class WaveManagerStateTests
{
    // --- Initial state ---

    [Fact]
    public void InitialState_IsInactive()
    {
        var state = new WaveManagerState(seed: 42);

        Assert.False(state.IsActive);
        Assert.Equal(0, state.CurrentWave);
    }

    // --- Start ---

    [Fact]
    public void Start_ActivatesAndSetsWave1()
    {
        var state = new WaveManagerState(seed: 42);

        state.Start();

        Assert.True(state.IsActive);
        Assert.Equal(1, state.CurrentWave);
    }

    [Fact]
    public void Start_PopulatesSpawnQueue()
    {
        var state = new WaveManagerState(seed: 42);

        state.Start();

        Assert.True(state.RemainingSpawns > 0);
    }

    [Fact]
    public void Start_FiresWaveStartedEvent()
    {
        var state = new WaveManagerState(seed: 42);
        int receivedWave = 0;
        state.WaveStarted += w => receivedWave = w;

        state.Start();

        Assert.Equal(1, receivedWave);
    }

    // --- Update / spawn timing ---

    [Fact]
    public void FirstUpdate_ReturnsEnemyImmediately()
    {
        var state = new WaveManagerState(seed: 42);
        state.Start();

        string? result = state.Update(0.016f);

        Assert.NotNull(result);
    }

    [Fact]
    public void Update_RespectsSpawnInterval()
    {
        var state = new WaveManagerState(seed: 42);
        state.Start();

        state.Update(0.016f);

        string? result = state.Update(0.016f);
        Assert.Null(result);
    }

    [Fact]
    public void Update_SpawnsAfterInterval()
    {
        var state = new WaveManagerState(seed: 42);
        state.Start();

        state.Update(0.016f);

        var wave1 = WaveCompositions.GetWave(1);
        string? result = state.Update(wave1.SpawnInterval + 0.001f);
        Assert.NotNull(result);
    }

    [Fact]
    public void Update_SpawnsCorrectTotalForWave1()
    {
        var state = new WaveManagerState(seed: 42);
        state.Start();

        var wave1 = WaveCompositions.GetWave(1);
        int spawnCount = 0;

        for (int i = 0; i < 1000; i++)
        {
            string? result = state.Update(wave1.SpawnInterval + 0.001f);
            if (result != null) spawnCount++;
            if (state.CurrentWave > 1) break;
        }

        Assert.True(spawnCount >= wave1.TotalEnemyCount);
    }

    [Fact]
    public void Update_Wave1_OnlySpawnsCrawlers()
    {
        var state = new WaveManagerState(seed: 42);
        state.Start();

        var wave1 = WaveCompositions.GetWave(1);
        var spawnedTypes = new HashSet<string>();

        for (int i = 0; i < wave1.TotalEnemyCount; i++)
        {
            string? result = state.Update(wave1.SpawnInterval + 0.001f);
            if (result != null) spawnedTypes.Add(result);
        }

        Assert.Single(spawnedTypes);
        Assert.Contains(WaveCompositions.Crawler, spawnedTypes);
    }

    // --- Inactive state ---

    [Fact]
    public void Update_WhenInactive_ReturnsNull()
    {
        var state = new WaveManagerState(seed: 42);

        string? result = state.Update(1.0f);

        Assert.Null(result);
    }

    // --- Wave advancement ---

    [Fact]
    public void AdvancesToWave2_AfterWave1QueueDepleted()
    {
        var state = new WaveManagerState(seed: 42);
        state.Start();

        var wave1 = WaveCompositions.GetWave(1);

        for (int i = 0; i < wave1.TotalEnemyCount + 1; i++)
            state.Update(wave1.SpawnInterval + 0.001f);

        Assert.True(state.CurrentWave >= 2);
    }

    [Fact]
    public void WaveStartedEvent_FiresOnEachAdvance()
    {
        var state = new WaveManagerState(seed: 42);
        var waves = new List<int>();
        state.WaveStarted += w => waves.Add(w);

        state.Start();

        var wave1 = WaveCompositions.GetWave(1);
        for (int i = 0; i < wave1.TotalEnemyCount + 5; i++)
            state.Update(wave1.SpawnInterval + 0.001f);

        Assert.Contains(1, waves);
        Assert.Contains(2, waves);
    }

    // --- Reset ---

    [Fact]
    public void Reset_DeactivatesAndClearsState()
    {
        var state = new WaveManagerState(seed: 42);
        state.Start();
        state.Update(0.016f);

        state.Reset();

        Assert.False(state.IsActive);
        Assert.Equal(0, state.CurrentWave);
        Assert.Equal(0, state.RemainingSpawns);
    }

    [Fact]
    public void Reset_ThenStart_BeginsFromWave1()
    {
        var state = new WaveManagerState(seed: 42);
        state.Start();

        var wave1 = WaveCompositions.GetWave(1);
        for (int i = 0; i < wave1.TotalEnemyCount + 5; i++)
            state.Update(wave1.SpawnInterval + 0.001f);

        Assert.True(state.CurrentWave >= 2);

        state.Reset();
        state.Start();

        Assert.Equal(1, state.CurrentWave);
    }

    // --- Deterministic seeding ---

    [Fact]
    public void SameSeeed_ProducesSameSpawnOrder()
    {
        var state1 = new WaveManagerState(seed: 123);
        var state2 = new WaveManagerState(seed: 123);
        state1.Start();
        state2.Start();

        var spawns1 = new List<string>();
        var spawns2 = new List<string>();

        for (int i = 0; i < 20; i++)
        {
            string? r1 = state1.Update(5.0f);
            string? r2 = state2.Update(5.0f);
            if (r1 != null) spawns1.Add(r1);
            if (r2 != null) spawns2.Add(r2);
        }

        Assert.Equal(spawns1, spawns2);
    }

    // --- Continuous flow (no downtime) ---

    [Fact]
    public void ContinuousFlow_NoGapBetweenWaves()
    {
        var state = new WaveManagerState(seed: 42);
        state.Start();

        var wave1 = WaveCompositions.GetWave(1);
        int nullsBetweenLastWave1AndFirstWave2 = 0;
        bool finishedWave1 = false;
        int spawned = 0;

        for (int i = 0; i < 200; i++)
        {
            string? result = state.Update(wave1.SpawnInterval + 0.001f);
            if (result != null)
            {
                spawned++;
                if (spawned == wave1.TotalEnemyCount)
                    finishedWave1 = true;
                if (finishedWave1 && spawned > wave1.TotalEnemyCount)
                    break;
            }
            else if (finishedWave1)
            {
                nullsBetweenLastWave1AndFirstWave2++;
            }
        }

        Assert.Equal(0, nullsBetweenLastWave1AndFirstWave2);
    }
}
