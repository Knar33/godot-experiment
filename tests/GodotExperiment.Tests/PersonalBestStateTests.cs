using GodotExperiment.GameLoop;
using Xunit;

namespace GodotExperiment.Tests;

public class PersonalBestStateTests
{
    // --- Initial state ---

    [Fact]
    public void InitialState_HasNoBest()
    {
        var pb = new PersonalBestState();
        Assert.False(pb.HasBest);
        Assert.Equal(0.0, pb.BestTimeSeconds);
    }

    [Fact]
    public void InitialState_FormatBest_ReturnsDashes()
    {
        var pb = new PersonalBestState();
        Assert.Equal("--:--.---", pb.FormatBest());
    }

    // --- Setting new best ---

    [Fact]
    public void TrySetNewBest_FirstRun_ReturnsTrue()
    {
        var pb = new PersonalBestState();
        Assert.True(pb.TrySetNewBest(30.5));
    }

    [Fact]
    public void TrySetNewBest_FirstRun_StoresTime()
    {
        var pb = new PersonalBestState();
        pb.TrySetNewBest(30.5);
        Assert.Equal(30.5, pb.BestTimeSeconds);
    }

    [Fact]
    public void TrySetNewBest_BetterTime_ReturnsTrue()
    {
        var pb = new PersonalBestState();
        pb.TrySetNewBest(30.0);
        Assert.True(pb.TrySetNewBest(45.0));
        Assert.Equal(45.0, pb.BestTimeSeconds);
    }

    [Fact]
    public void TrySetNewBest_WorseTime_ReturnsFalse()
    {
        var pb = new PersonalBestState();
        pb.TrySetNewBest(45.0);
        Assert.False(pb.TrySetNewBest(30.0));
        Assert.Equal(45.0, pb.BestTimeSeconds);
    }

    [Fact]
    public void TrySetNewBest_EqualTime_ReturnsFalse()
    {
        var pb = new PersonalBestState();
        pb.TrySetNewBest(45.0);
        Assert.False(pb.TrySetNewBest(45.0));
    }

    [Fact]
    public void TrySetNewBest_ZeroTime_ReturnsFalse()
    {
        var pb = new PersonalBestState();
        Assert.False(pb.TrySetNewBest(0.0));
        Assert.False(pb.HasBest);
    }

    [Fact]
    public void TrySetNewBest_NegativeTime_ReturnsFalse()
    {
        var pb = new PersonalBestState();
        Assert.False(pb.TrySetNewBest(-5.0));
        Assert.False(pb.HasBest);
    }

    // --- Loading saved best ---

    [Fact]
    public void LoadBest_SetsTime()
    {
        var pb = new PersonalBestState();
        pb.LoadBest(120.456);
        Assert.True(pb.HasBest);
        Assert.Equal(120.456, pb.BestTimeSeconds);
    }

    [Fact]
    public void LoadBest_ZeroOrNegative_ClearsBest()
    {
        var pb = new PersonalBestState();
        pb.LoadBest(0.0);
        Assert.False(pb.HasBest);

        pb.LoadBest(-1.0);
        Assert.False(pb.HasBest);
    }

    [Fact]
    public void TrySetNewBest_AfterLoad_ComparesAgainstLoaded()
    {
        var pb = new PersonalBestState();
        pb.LoadBest(60.0);
        Assert.False(pb.TrySetNewBest(30.0));
        Assert.True(pb.TrySetNewBest(90.0));
    }

    // --- Formatting ---

    [Fact]
    public void FormatBest_WithTime_ReturnsCorrectFormat()
    {
        var pb = new PersonalBestState();
        pb.TrySetNewBest(62.123);
        Assert.Equal("01:02.123", pb.FormatBest());
    }

    [Fact]
    public void FormatBest_SubMinute_PadsWithZeros()
    {
        var pb = new PersonalBestState();
        pb.TrySetNewBest(5.007);
        Assert.Equal("00:05.007", pb.FormatBest());
    }

    [Fact]
    public void FormatBest_LargeTime_FormatsCorrectly()
    {
        var pb = new PersonalBestState();
        pb.TrySetNewBest(600.0);
        Assert.Equal("10:00.000", pb.FormatBest());
    }

    // --- HasBest ---

    [Fact]
    public void HasBest_TrueAfterFirstValidRun()
    {
        var pb = new PersonalBestState();
        pb.TrySetNewBest(1.0);
        Assert.True(pb.HasBest);
    }

    [Fact]
    public void HasBest_TrueAfterLoad()
    {
        var pb = new PersonalBestState();
        pb.LoadBest(50.0);
        Assert.True(pb.HasBest);
    }
}
