using GodotExperiment.GameLoop;
using Xunit;

namespace GodotExperiment.Tests;

public class UpgradeMeterStateTests
{
    [Fact]
    public void InitialState_IsZero()
    {
        var meter = new UpgradeMeterState();
        Assert.Equal(0, meter.GemsCollected);
        Assert.Equal(0, meter.UpgradeLevel);
        Assert.Equal(10, meter.CurrentThreshold);
        Assert.Equal(0f, meter.Progress);
        Assert.False(meter.IsFull);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(1, 15)]
    [InlineData(2, 20)]
    [InlineData(3, 25)]
    [InlineData(5, 35)]
    [InlineData(10, 60)]
    public void CurrentThreshold_ScalesWithUpgradeLevel(int level, int expectedThreshold)
    {
        var meter = new UpgradeMeterState();
        for (int i = 0; i < level; i++)
        {
            meter.AddGems(meter.CurrentThreshold);
            meter.ConsumeUpgrade();
        }
        Assert.Equal(expectedThreshold, meter.CurrentThreshold);
    }

    [Fact]
    public void AddGems_IncreasesCount()
    {
        var meter = new UpgradeMeterState();
        meter.AddGems(5);
        Assert.Equal(5, meter.GemsCollected);
        Assert.Equal(0.5f, meter.Progress, 0.001f);
    }

    [Fact]
    public void AddGems_MultipleCalls_Accumulate()
    {
        var meter = new UpgradeMeterState();
        meter.AddGems(3);
        meter.AddGems(4);
        Assert.Equal(7, meter.GemsCollected);
    }

    [Fact]
    public void AddGems_ZeroOrNegative_Ignored()
    {
        var meter = new UpgradeMeterState();
        meter.AddGems(0);
        meter.AddGems(-5);
        Assert.Equal(0, meter.GemsCollected);
    }

    [Fact]
    public void IsFull_WhenGemsReachThreshold()
    {
        var meter = new UpgradeMeterState();
        meter.AddGems(10);
        Assert.True(meter.IsFull);
    }

    [Fact]
    public void IsFull_WhenGemsExceedThreshold()
    {
        var meter = new UpgradeMeterState();
        meter.AddGems(15);
        Assert.True(meter.IsFull);
    }

    [Fact]
    public void ThresholdReached_FiresWhenFull()
    {
        var meter = new UpgradeMeterState();
        bool fired = false;
        meter.ThresholdReached += () => fired = true;

        meter.AddGems(10);
        Assert.True(fired);
    }

    [Fact]
    public void ThresholdReached_DoesNotFireBelow()
    {
        var meter = new UpgradeMeterState();
        bool fired = false;
        meter.ThresholdReached += () => fired = true;

        meter.AddGems(9);
        Assert.False(fired);
    }

    [Fact]
    public void GemsChanged_FiresOnAdd()
    {
        var meter = new UpgradeMeterState();
        int reportedGems = -1;
        int reportedThreshold = -1;
        meter.GemsChanged += (g, t) => { reportedGems = g; reportedThreshold = t; };

        meter.AddGems(3);
        Assert.Equal(3, reportedGems);
        Assert.Equal(10, reportedThreshold);
    }

    [Fact]
    public void ConsumeUpgrade_ResetsGemsAndIncreasesLevel()
    {
        var meter = new UpgradeMeterState();
        meter.AddGems(10);
        meter.ConsumeUpgrade();

        Assert.Equal(0, meter.GemsCollected);
        Assert.Equal(1, meter.UpgradeLevel);
        Assert.Equal(15, meter.CurrentThreshold);
        Assert.False(meter.IsFull);
    }

    [Fact]
    public void ConsumeUpgrade_GemsChangedReportsNewThreshold()
    {
        var meter = new UpgradeMeterState();
        meter.AddGems(10);

        int reportedGems = -1;
        int reportedThreshold = -1;
        meter.GemsChanged += (g, t) => { reportedGems = g; reportedThreshold = t; };

        meter.ConsumeUpgrade();
        Assert.Equal(0, reportedGems);
        Assert.Equal(15, reportedThreshold);
    }

    [Fact]
    public void MultipleUpgrades_ThresholdIncreasesCorrectly()
    {
        var meter = new UpgradeMeterState();

        meter.AddGems(10);
        meter.ConsumeUpgrade();
        Assert.Equal(15, meter.CurrentThreshold);

        meter.AddGems(15);
        meter.ConsumeUpgrade();
        Assert.Equal(20, meter.CurrentThreshold);

        meter.AddGems(20);
        meter.ConsumeUpgrade();
        Assert.Equal(25, meter.CurrentThreshold);
    }

    [Fact]
    public void Reset_ClearsEverything()
    {
        var meter = new UpgradeMeterState();
        meter.AddGems(10);
        meter.ConsumeUpgrade();
        meter.AddGems(5);

        meter.Reset();

        Assert.Equal(0, meter.GemsCollected);
        Assert.Equal(0, meter.UpgradeLevel);
        Assert.Equal(10, meter.CurrentThreshold);
        Assert.False(meter.IsFull);
    }

    [Fact]
    public void Progress_CalculatesCorrectly()
    {
        var meter = new UpgradeMeterState();
        meter.AddGems(5);
        Assert.Equal(0.5f, meter.Progress, 0.001f);

        meter.AddGems(5);
        Assert.Equal(1.0f, meter.Progress, 0.001f);
    }

    [Fact]
    public void ThresholdFormula_MatchesDesign()
    {
        Assert.Equal(10, UpgradeMeterState.BaseThreshold);
        Assert.Equal(5, UpgradeMeterState.ThresholdIncrement);
    }
}
