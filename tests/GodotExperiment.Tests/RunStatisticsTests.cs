using GodotExperiment.GameLoop;
using Xunit;

namespace GodotExperiment.Tests;

public class RunStatisticsTests
{
    // --- Initial state ---

    [Fact]
    public void InitialState_AllZeros()
    {
        var stats = new RunStatistics();
        Assert.Equal(0, stats.EnemiesKilled);
        Assert.Equal(0, stats.GemsCollected);
        Assert.Equal(0, stats.WaveReached);
        Assert.Equal(0, stats.LongestBhopChain);
        Assert.Empty(stats.UpgradesChosen);
    }

    // --- Enemy kills ---

    [Fact]
    public void RecordEnemyKill_IncrementsCount()
    {
        var stats = new RunStatistics();
        stats.RecordEnemyKill();
        Assert.Equal(1, stats.EnemiesKilled);
    }

    [Fact]
    public void RecordEnemyKill_MultipleTimes_Accumulates()
    {
        var stats = new RunStatistics();
        for (int i = 0; i < 25; i++)
            stats.RecordEnemyKill();
        Assert.Equal(25, stats.EnemiesKilled);
    }

    // --- Gems collected ---

    [Fact]
    public void RecordGemsCollected_AddsToTotal()
    {
        var stats = new RunStatistics();
        stats.RecordGemsCollected(5);
        Assert.Equal(5, stats.GemsCollected);
    }

    [Fact]
    public void RecordGemsCollected_MultipleBatches_Accumulates()
    {
        var stats = new RunStatistics();
        stats.RecordGemsCollected(3);
        stats.RecordGemsCollected(7);
        Assert.Equal(10, stats.GemsCollected);
    }

    [Fact]
    public void RecordGemsCollected_ZeroOrNegative_Ignored()
    {
        var stats = new RunStatistics();
        stats.RecordGemsCollected(5);
        stats.RecordGemsCollected(0);
        stats.RecordGemsCollected(-3);
        Assert.Equal(5, stats.GemsCollected);
    }

    // --- Wave reached ---

    [Fact]
    public void RecordWaveReached_TracksHighest()
    {
        var stats = new RunStatistics();
        stats.RecordWaveReached(1);
        stats.RecordWaveReached(3);
        stats.RecordWaveReached(2);
        Assert.Equal(3, stats.WaveReached);
    }

    [Fact]
    public void RecordWaveReached_SameWave_NoChange()
    {
        var stats = new RunStatistics();
        stats.RecordWaveReached(5);
        stats.RecordWaveReached(5);
        Assert.Equal(5, stats.WaveReached);
    }

    // --- Bhop chain ---

    [Fact]
    public void RecordBhopLanded_IncreasesChain()
    {
        var stats = new RunStatistics();
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        Assert.Equal(3, stats.LongestBhopChain);
    }

    [Fact]
    public void RecordBhopChainBroken_ResetsCurrentChain()
    {
        var stats = new RunStatistics();
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopChainBroken();
        stats.RecordBhopLanded();
        Assert.Equal(2, stats.LongestBhopChain);
    }

    [Fact]
    public void LongestBhopChain_TracksMaxAcrossMultipleChains()
    {
        var stats = new RunStatistics();

        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopChainBroken();

        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopChainBroken();

        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopChainBroken();

        Assert.Equal(5, stats.LongestBhopChain);
    }

    // --- Upgrades ---

    [Fact]
    public void RecordUpgradeChosen_AddsToList()
    {
        var stats = new RunStatistics();
        stats.RecordUpgradeChosen("Rapid Fire");
        stats.RecordUpgradeChosen("Gem Magnet");
        Assert.Equal(2, stats.UpgradesChosen.Count);
        Assert.Equal("Rapid Fire", stats.UpgradesChosen[0]);
        Assert.Equal("Gem Magnet", stats.UpgradesChosen[1]);
    }

    [Fact]
    public void RecordUpgradeChosen_PreservesAcquisitionOrder()
    {
        var stats = new RunStatistics();
        stats.RecordUpgradeChosen("A");
        stats.RecordUpgradeChosen("B");
        stats.RecordUpgradeChosen("C");
        Assert.Equal(new[] { "A", "B", "C" }, stats.UpgradesChosen);
    }

    // --- Reset ---

    [Fact]
    public void Reset_ClearsAllStats()
    {
        var stats = new RunStatistics();
        stats.RecordEnemyKill();
        stats.RecordEnemyKill();
        stats.RecordGemsCollected(10);
        stats.RecordWaveReached(5);
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordUpgradeChosen("Test");

        stats.Reset();

        Assert.Equal(0, stats.EnemiesKilled);
        Assert.Equal(0, stats.GemsCollected);
        Assert.Equal(0, stats.WaveReached);
        Assert.Equal(0, stats.LongestBhopChain);
        Assert.Empty(stats.UpgradesChosen);
    }

    [Fact]
    public void Reset_NewChainAfterReset_TracksCorrectly()
    {
        var stats = new RunStatistics();
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();
        stats.RecordBhopLanded();

        stats.Reset();

        stats.RecordBhopLanded();
        Assert.Equal(1, stats.LongestBhopChain);
    }
}
