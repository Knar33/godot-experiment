using GodotExperiment.GameLoop;
using Xunit;

namespace GodotExperiment.Tests;

public class SurvivalTimerStateTests
{
    [Fact]
    public void InitialState_IsZeroAndNotRunning()
    {
        var timer = new SurvivalTimerState();
        Assert.Equal(0.0, timer.ElapsedSeconds);
        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void Start_SetsIsRunningTrue()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        Assert.True(timer.IsRunning);
    }

    [Fact]
    public void Update_AccumulatesTime_WhenRunning()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        timer.Update(1.5);
        Assert.Equal(1.5, timer.ElapsedSeconds, precision: 6);
    }

    [Fact]
    public void Update_DoesNotAccumulateTime_WhenNotRunning()
    {
        var timer = new SurvivalTimerState();
        timer.Update(1.0);
        Assert.Equal(0.0, timer.ElapsedSeconds);
    }

    [Fact]
    public void Freeze_StopsAccumulation()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        timer.Update(2.0);
        timer.Freeze();
        timer.Update(5.0);
        Assert.Equal(2.0, timer.ElapsedSeconds, precision: 6);
        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void Reset_ClearsTimeAndStops()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        timer.Update(10.0);
        timer.Reset();
        Assert.Equal(0.0, timer.ElapsedSeconds);
        Assert.False(timer.IsRunning);
    }

    [Fact]
    public void Format_ZeroTime()
    {
        var timer = new SurvivalTimerState();
        Assert.Equal("00:00.000", timer.Format());
    }

    [Fact]
    public void Format_SubSecond()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        timer.Update(0.123);
        Assert.Equal("00:00.123", timer.Format());
    }

    [Fact]
    public void Format_OneMinute()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        timer.Update(60.0);
        Assert.Equal("01:00.000", timer.Format());
    }

    [Fact]
    public void Format_ComplexTime()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        timer.Update(125.456);
        Assert.Equal("02:05.456", timer.Format());
    }

    [Fact]
    public void Format_PreservesTimeAfterFreeze()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        timer.Update(3.141);
        timer.Freeze();
        Assert.Equal("00:03.141", timer.Format());
    }

    [Fact]
    public void MultipleStartStopCycles_AccumulateCorrectly()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        timer.Update(1.0);
        timer.Freeze();
        timer.Start();
        timer.Update(1.0);
        Assert.Equal(2.0, timer.ElapsedSeconds, precision: 6);
    }

    [Fact]
    public void Update_AccumulatesMultipleCalls()
    {
        var timer = new SurvivalTimerState();
        timer.Start();
        for (int i = 0; i < 60; i++)
            timer.Update(1.0 / 60.0);

        Assert.InRange(timer.ElapsedSeconds, 0.99, 1.01);
    }
}
