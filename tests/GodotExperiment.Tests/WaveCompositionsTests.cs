using GodotExperiment.Waves;
using Xunit;

namespace GodotExperiment.Tests;

public class WaveCompositionsTests
{
    // --- Wave 1: Crawlers only ---

    [Fact]
    public void Wave1_ContainsOnlyCrawlers()
    {
        var wave = WaveCompositions.GetWave(1);

        Assert.Single(wave.Groups);
        Assert.Equal(WaveCompositions.Crawler, wave.Groups[0].EnemyType);
    }

    [Fact]
    public void Wave1_Has5Crawlers()
    {
        var wave = WaveCompositions.GetWave(1);

        Assert.Equal(5, wave.TotalEnemyCount);
        Assert.Equal(5, wave.Groups[0].Count);
    }

    // --- Wave 2: More Crawlers ---

    [Fact]
    public void Wave2_ContainsOnlyCrawlers()
    {
        var wave = WaveCompositions.GetWave(2);

        Assert.Single(wave.Groups);
        Assert.Equal(WaveCompositions.Crawler, wave.Groups[0].EnemyType);
    }

    [Fact]
    public void Wave2_Has10Crawlers()
    {
        var wave = WaveCompositions.GetWave(2);

        Assert.Equal(10, wave.TotalEnemyCount);
    }

    // --- Wave 3: Introduces Spitters ---

    [Fact]
    public void Wave3_IntroducesSpitters()
    {
        var wave = WaveCompositions.GetWave(3);

        var types = wave.Groups.Select(g => g.EnemyType).ToHashSet();
        Assert.Contains(WaveCompositions.Crawler, types);
        Assert.Contains(WaveCompositions.Spitter, types);
    }

    [Fact]
    public void Wave3_Has2Spitters()
    {
        var wave = WaveCompositions.GetWave(3);

        var spitterGroup = wave.Groups.First(g => g.EnemyType == WaveCompositions.Spitter);
        Assert.Equal(2, spitterGroup.Count);
    }

    // --- Wave 4: Introduces Charger ---

    [Fact]
    public void Wave4_IntroducesCharger()
    {
        var wave = WaveCompositions.GetWave(4);

        var types = wave.Groups.Select(g => g.EnemyType).ToHashSet();
        Assert.Contains(WaveCompositions.Charger, types);
    }

    [Fact]
    public void Wave4_Has1Charger()
    {
        var wave = WaveCompositions.GetWave(4);

        var chargerGroup = wave.Groups.First(g => g.EnemyType == WaveCompositions.Charger);
        Assert.Equal(1, chargerGroup.Count);
    }

    // --- Wave 5: Introduces Drones ---

    [Fact]
    public void Wave5_IntroducesDrones()
    {
        var wave = WaveCompositions.GetWave(5);

        var types = wave.Groups.Select(g => g.EnemyType).ToHashSet();
        Assert.Contains(WaveCompositions.Drone, types);
    }

    [Fact]
    public void Wave5_ContainsAllEarlyTypes()
    {
        var wave = WaveCompositions.GetWave(5);

        var types = wave.Groups.Select(g => g.EnemyType).ToHashSet();
        Assert.Contains(WaveCompositions.Crawler, types);
        Assert.Contains(WaveCompositions.Spitter, types);
        Assert.Contains(WaveCompositions.Charger, types);
        Assert.Contains(WaveCompositions.Drone, types);
    }

    // --- Spawn intervals ---

    [Fact]
    public void AllDefinedWaves_HavePositiveSpawnIntervals()
    {
        for (int i = 1; i <= WaveCompositions.DefinedWaveCount; i++)
        {
            var wave = WaveCompositions.GetWave(i);
            Assert.True(wave.SpawnInterval > 0f, $"Wave {i} has non-positive spawn interval.");
        }
    }

    [Fact]
    public void SpawnIntervals_DecreaseOverEarlyWaves()
    {
        float previousInterval = float.MaxValue;
        for (int i = 1; i <= 5; i++)
        {
            var wave = WaveCompositions.GetWave(i);
            Assert.True(wave.SpawnInterval < previousInterval,
                $"Wave {i} interval ({wave.SpawnInterval}) should be less than wave {i - 1} interval ({previousInterval}).");
            previousInterval = wave.SpawnInterval;
        }
    }

    // --- Enemy counts increase ---

    [Fact]
    public void TotalEnemyCount_IncreasesOverEarlyWaves()
    {
        int previousCount = 0;
        for (int i = 1; i <= 5; i++)
        {
            var wave = WaveCompositions.GetWave(i);
            Assert.True(wave.TotalEnemyCount > previousCount,
                $"Wave {i} count ({wave.TotalEnemyCount}) should be greater than wave {i - 1} count ({previousCount}).");
            previousCount = wave.TotalEnemyCount;
        }
    }

    // --- Scaling waves (beyond defined) ---

    [Fact]
    public void ScalingWave_ReturnsValidDefinition()
    {
        var wave = WaveCompositions.GetWave(10);

        Assert.Equal(10, wave.WaveNumber);
        Assert.True(wave.TotalEnemyCount > 0);
        Assert.True(wave.SpawnInterval > 0f);
        Assert.NotEmpty(wave.Groups);
    }

    [Fact]
    public void ScalingWave_EnemyCountIncreases()
    {
        var wave6 = WaveCompositions.GetWave(6);
        var wave10 = WaveCompositions.GetWave(10);

        Assert.True(wave10.TotalEnemyCount > wave6.TotalEnemyCount);
    }

    [Fact]
    public void ScalingWave_SpawnIntervalHasFloor()
    {
        var wave100 = WaveCompositions.GetWave(100);

        Assert.True(wave100.SpawnInterval >= 0.3f);
    }

    // --- Invalid input ---

    [Fact]
    public void GetWave_WithZero_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WaveCompositions.GetWave(0));
    }

    [Fact]
    public void GetWave_WithNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => WaveCompositions.GetWave(-1));
    }

    // --- Difficulty scaling rule: stats never modified ---

    [Fact]
    public void WaveDefinitions_ContainOnlyEnemyTypesAndCounts_NotStats()
    {
        for (int i = 1; i <= 10; i++)
        {
            var wave = WaveCompositions.GetWave(i);
            foreach (var group in wave.Groups)
            {
                Assert.False(string.IsNullOrWhiteSpace(group.EnemyType));
                Assert.True(group.Count > 0);
            }
        }
    }
}
