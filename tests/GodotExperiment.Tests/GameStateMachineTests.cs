using GodotExperiment;
using Xunit;

namespace GodotExperiment.Tests;

public class GameStateMachineTests
{
    [Fact]
    public void InitialState_IsCountdown()
    {
        var sm = new GameStateMachine();
        Assert.Equal(GameState.Countdown, sm.Current);
    }

    [Fact]
    public void InitialPreviousState_IsCountdown()
    {
        var sm = new GameStateMachine();
        Assert.Equal(GameState.Countdown, sm.Previous);
    }

    // --- Valid transitions ---

    [Fact]
    public void Countdown_CanTransitionTo_Playing()
    {
        var sm = new GameStateMachine();
        Assert.True(sm.TransitionTo(GameState.Playing));
        Assert.Equal(GameState.Playing, sm.Current);
        Assert.Equal(GameState.Countdown, sm.Previous);
    }

    [Fact]
    public void Playing_CanTransitionTo_Dead()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);

        Assert.True(sm.TransitionTo(GameState.Dead));
        Assert.Equal(GameState.Dead, sm.Current);
        Assert.Equal(GameState.Playing, sm.Previous);
    }

    [Fact]
    public void Playing_CanTransitionTo_Paused()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);

        Assert.True(sm.TransitionTo(GameState.Paused));
        Assert.Equal(GameState.Paused, sm.Current);
    }

    [Fact]
    public void Dead_CanTransitionTo_Countdown()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);
        sm.TransitionTo(GameState.Dead);

        Assert.True(sm.TransitionTo(GameState.Countdown));
        Assert.Equal(GameState.Countdown, sm.Current);
    }

    [Fact]
    public void Paused_CanTransitionTo_Playing()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);
        sm.TransitionTo(GameState.Paused);

        Assert.True(sm.TransitionTo(GameState.Playing));
        Assert.Equal(GameState.Playing, sm.Current);
    }

    [Fact]
    public void Paused_CanTransitionTo_Countdown()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);
        sm.TransitionTo(GameState.Paused);

        Assert.True(sm.TransitionTo(GameState.Countdown));
        Assert.Equal(GameState.Countdown, sm.Current);
    }

    // --- Invalid transitions ---

    [Theory]
    [InlineData(GameState.Dead)]
    [InlineData(GameState.Paused)]
    [InlineData(GameState.Countdown)]
    public void Countdown_CannotTransitionTo(GameState target)
    {
        var sm = new GameStateMachine();
        Assert.False(sm.TransitionTo(target));
        Assert.Equal(GameState.Countdown, sm.Current);
    }

    [Theory]
    [InlineData(GameState.Countdown)]
    [InlineData(GameState.Playing)]
    public void Playing_CannotTransitionTo(GameState target)
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);

        Assert.False(sm.TransitionTo(target));
        Assert.Equal(GameState.Playing, sm.Current);
    }

    [Theory]
    [InlineData(GameState.Playing)]
    [InlineData(GameState.Paused)]
    [InlineData(GameState.Dead)]
    public void Dead_CannotTransitionTo(GameState target)
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);
        sm.TransitionTo(GameState.Dead);

        Assert.False(sm.TransitionTo(target));
        Assert.Equal(GameState.Dead, sm.Current);
    }

    [Theory]
    [InlineData(GameState.Dead)]
    [InlineData(GameState.Paused)]
    public void Paused_CannotTransitionTo(GameState target)
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);
        sm.TransitionTo(GameState.Paused);

        Assert.False(sm.TransitionTo(target));
        Assert.Equal(GameState.Paused, sm.Current);
    }

    // --- Event behavior ---

    [Fact]
    public void StateChanged_FiresOnValidTransition()
    {
        var sm = new GameStateMachine();
        GameState? firedPrev = null;
        GameState? firedCurr = null;
        sm.StateChanged += (prev, curr) => { firedPrev = prev; firedCurr = curr; };

        sm.TransitionTo(GameState.Playing);

        Assert.Equal(GameState.Countdown, firedPrev);
        Assert.Equal(GameState.Playing, firedCurr);
    }

    [Fact]
    public void StateChanged_DoesNotFireOnInvalidTransition()
    {
        var sm = new GameStateMachine();
        bool fired = false;
        sm.StateChanged += (_, _) => fired = true;

        sm.TransitionTo(GameState.Dead);

        Assert.False(fired);
    }

    // --- Reset ---

    [Fact]
    public void Reset_SetsStateToCountdown()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);
        sm.TransitionTo(GameState.Dead);

        sm.Reset();

        Assert.Equal(GameState.Countdown, sm.Current);
        Assert.Equal(GameState.Dead, sm.Previous);
    }

    [Fact]
    public void Reset_FiresStateChangedEvent()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);

        GameState? firedCurr = null;
        sm.StateChanged += (_, curr) => firedCurr = curr;

        sm.Reset();

        Assert.Equal(GameState.Countdown, firedCurr);
    }

    // --- Full loop ---

    [Fact]
    public void FullGameLoop_Countdown_Playing_Dead_Countdown()
    {
        var sm = new GameStateMachine();

        Assert.True(sm.TransitionTo(GameState.Playing));
        Assert.True(sm.TransitionTo(GameState.Dead));
        Assert.True(sm.TransitionTo(GameState.Countdown));
        Assert.True(sm.TransitionTo(GameState.Playing));

        Assert.Equal(GameState.Playing, sm.Current);
    }

    [Fact]
    public void PauseResumeLoop_Playing_Paused_Playing()
    {
        var sm = new GameStateMachine();
        sm.TransitionTo(GameState.Playing);

        Assert.True(sm.TransitionTo(GameState.Paused));
        Assert.True(sm.TransitionTo(GameState.Playing));
        Assert.True(sm.TransitionTo(GameState.Paused));
        Assert.True(sm.TransitionTo(GameState.Playing));

        Assert.Equal(GameState.Playing, sm.Current);
    }
}
