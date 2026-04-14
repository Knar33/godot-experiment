using System;
using GodotExperiment.Enemies;
using Xunit;

namespace GodotExperiment.Tests;

public class DelayedExplosionStateTests
{
    [Fact]
    public void Ctor_WithNegativeDelay_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DelayedExplosionState(-0.01f));
    }

    [Fact]
    public void Update_WhenNotArmed_DoesNotExplode()
    {
        var state = new DelayedExplosionState(0.5f);

        bool exploded = state.Update(0.6f);

        Assert.False(exploded);
        Assert.False(state.IsArmed);
        Assert.False(state.HasExploded);
    }

    [Fact]
    public void Arm_SetsRemainingSecondsToDelay()
    {
        var state = new DelayedExplosionState(0.5f);

        state.Arm();

        Assert.True(state.IsArmed);
        Assert.False(state.HasExploded);
        Assert.Equal(0.5f, state.RemainingSeconds, precision: 4);
    }

    [Fact]
    public void Update_BeforeDelay_DoesNotExplode()
    {
        var state = new DelayedExplosionState(0.5f);
        state.Arm();

        bool exploded = state.Update(0.49f);

        Assert.False(exploded);
        Assert.True(state.IsArmed);
        Assert.False(state.HasExploded);
    }

    [Fact]
    public void Update_AtOrAfterDelay_ExplodesOnce()
    {
        var state = new DelayedExplosionState(0.5f);
        state.Arm();

        bool exploded = state.Update(0.5f);
        bool explodedAgain = state.Update(0.01f);

        Assert.True(exploded);
        Assert.False(explodedAgain);
        Assert.True(state.HasExploded);
    }

    [Fact]
    public void Reset_ClearsArmedAndExplodedState()
    {
        var state = new DelayedExplosionState(0.5f);
        state.Arm();
        state.Update(1.0f);

        state.Reset();

        Assert.False(state.IsArmed);
        Assert.False(state.HasExploded);
        Assert.Equal(0f, state.RemainingSeconds, precision: 4);
    }
}

