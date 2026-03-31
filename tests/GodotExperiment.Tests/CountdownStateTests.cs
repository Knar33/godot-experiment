using GodotExperiment.GameLoop;
using Xunit;

namespace GodotExperiment.Tests;

public class CountdownStateTests
{
    [Fact]
    public void InitialState_IsInactive()
    {
        var countdown = new CountdownState();
        Assert.False(countdown.IsActive);
        Assert.False(countdown.IsFinished);
        Assert.Equal(0, countdown.CurrentNumber);
    }

    [Fact]
    public void Start_ActivatesAndSetsInitialNumber()
    {
        var countdown = new CountdownState();
        countdown.Start();
        Assert.True(countdown.IsActive);
        Assert.False(countdown.IsFinished);
        Assert.Equal(3, countdown.CurrentNumber);
    }

    [Fact]
    public void Start_CustomDuration_SetsCorrectInitialNumber()
    {
        var countdown = new CountdownState();
        countdown.Start(5.0);
        Assert.Equal(5, countdown.CurrentNumber);
    }

    [Fact]
    public void Start_FiresNumberChangedWithInitialNumber()
    {
        var countdown = new CountdownState();
        int? received = null;
        countdown.NumberChanged += n => received = n;
        countdown.Start();
        Assert.Equal(3, received);
    }

    [Fact]
    public void Update_DecreasesNumberOverTime()
    {
        var countdown = new CountdownState();
        var numbers = new List<int>();
        countdown.NumberChanged += n => numbers.Add(n);
        countdown.Start();

        countdown.Update(1.01);
        Assert.Contains(2, numbers);
    }

    [Fact]
    public void Update_FiresFinishedAtEnd()
    {
        var countdown = new CountdownState();
        bool finished = false;
        countdown.Finished += () => finished = true;
        countdown.Start();

        countdown.Update(3.01);
        Assert.True(finished);
        Assert.True(countdown.IsFinished);
        Assert.False(countdown.IsActive);
    }

    [Fact]
    public void Update_FullCountdown_3_2_1_Finish()
    {
        var countdown = new CountdownState();
        var ticks = new List<int>();
        bool finished = false;
        countdown.NumberChanged += n => ticks.Add(n);
        countdown.Finished += () => finished = true;
        countdown.Start();

        Assert.Contains(3, ticks);

        countdown.Update(1.01);
        Assert.Contains(2, ticks);

        countdown.Update(1.0);
        Assert.Contains(1, ticks);

        countdown.Update(1.0);
        Assert.True(finished);
        Assert.Equal(0, countdown.CurrentNumber);
    }

    [Fact]
    public void Update_DoesNothingWhenInactive()
    {
        var countdown = new CountdownState();
        bool any = false;
        countdown.NumberChanged += _ => any = true;
        countdown.Finished += () => any = true;
        countdown.Update(10.0);
        Assert.False(any);
    }

    [Fact]
    public void Update_DoesNothingAfterFinished()
    {
        var countdown = new CountdownState();
        countdown.Start();
        countdown.Update(4.0);

        int callCount = 0;
        countdown.NumberChanged += _ => callCount++;
        countdown.Finished += () => callCount++;
        countdown.Update(1.0);
        Assert.Equal(0, callCount);
    }

    [Fact]
    public void Reset_ClearsState()
    {
        var countdown = new CountdownState();
        countdown.Start();
        countdown.Update(1.5);
        countdown.Reset();

        Assert.False(countdown.IsActive);
        Assert.False(countdown.IsFinished);
        Assert.Equal(0, countdown.CurrentNumber);
    }

    [Fact]
    public void CanRestartAfterFinish()
    {
        var countdown = new CountdownState();
        countdown.Start();
        countdown.Update(4.0);
        Assert.True(countdown.IsFinished);

        countdown.Reset();
        countdown.Start();
        Assert.True(countdown.IsActive);
        Assert.Equal(3, countdown.CurrentNumber);
    }

    [Fact]
    public void CanRestartAfterReset()
    {
        var countdown = new CountdownState();
        countdown.Start();
        countdown.Update(1.0);
        countdown.Reset();

        var ticks = new List<int>();
        countdown.NumberChanged += n => ticks.Add(n);
        countdown.Start();
        Assert.Contains(3, ticks);
    }

    [Fact]
    public void FullLoop_Countdown_Playing_Dead_Countdown()
    {
        var sm = new GameStateMachine();
        var countdown = new CountdownState();
        var timer = new SurvivalTimerState();

        countdown.Finished += () =>
        {
            sm.TransitionTo(GameState.Playing);
            timer.Start();
        };

        countdown.Start();
        countdown.Update(3.1);

        Assert.Equal(GameState.Playing, sm.Current);
        Assert.True(timer.IsRunning);

        timer.Update(5.0);
        timer.Freeze();
        sm.TransitionTo(GameState.Dead);

        Assert.Equal(GameState.Dead, sm.Current);
        Assert.False(timer.IsRunning);
        Assert.Equal(5.0, timer.ElapsedSeconds, precision: 6);

        timer.Reset();
        countdown.Reset();
        sm.Reset();
        countdown.Start();

        Assert.Equal(GameState.Countdown, sm.Current);
        Assert.Equal(0.0, timer.ElapsedSeconds);
        Assert.True(countdown.IsActive);
    }
}
