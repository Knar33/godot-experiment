using GodotExperiment.Enemies;
using Xunit;

namespace GodotExperiment.Tests;

public class DroneAIStateTests
{
    private const float DefaultTriggerRange = 14f;
    private const float DefaultInterval = 3.5f;
    private const float DefaultTelegraph = 0.25f;
    private const float DefaultDive = 0.35f;
    private const float DefaultRecovery = 0.6f;
    private const float DefaultSpeedMult = 7f;
    private const float Dt = 0.016f;

    private static DroneAIState Create(
        float triggerRange = DefaultTriggerRange,
        float interval = DefaultInterval,
        float telegraph = DefaultTelegraph,
        float dive = DefaultDive,
        float recovery = DefaultRecovery,
        float speedMult = DefaultSpeedMult,
        float initialAttackTimer = -1f)
        => new(triggerRange, interval, telegraph, dive, recovery, speedMult, initialAttackTimer);

    [Fact]
    public void Constructor_DefaultPhase_IsOrbiting()
    {
        var ai = Create();

        Assert.Equal(DroneAIState.Phase.Orbiting, ai.CurrentPhase);
        Assert.True(ai.ShouldMove);
        Assert.False(ai.IsTelegraphing);
        Assert.False(ai.IsDiving);
        Assert.False(ai.IsRecovering);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void Constructor_InvalidTriggerRange_Throws(float range)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DroneAIState(diveTriggerRange: range));
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void Constructor_InvalidDiveInterval_Throws(float interval)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DroneAIState(diveInterval: interval));
    }

    [Fact]
    public void Constructor_InvalidTelegraphDuration_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DroneAIState(telegraphDuration: 0f));
    }

    [Fact]
    public void Constructor_InvalidDiveDuration_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DroneAIState(diveDuration: 0f));
    }

    [Fact]
    public void Constructor_InvalidRecoveryDuration_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DroneAIState(recoveryDuration: 0f));
    }

    [Fact]
    public void Constructor_InvalidDiveSpeedMultiplier_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new DroneAIState(diveSpeedMultiplier: 0f));
    }

    [Fact]
    public void Constructor_SetsAttackTimer_ToInterval_ByDefault()
    {
        var ai = Create(interval: 4f);

        Assert.Equal(4f, ai.AttackTimer);
    }

    [Fact]
    public void Constructor_UsesInitialAttackTimer_WhenProvided()
    {
        var ai = Create(interval: 4f, initialAttackTimer: 1.25f);

        Assert.Equal(1.25f, ai.AttackTimer);
    }

    [Fact]
    public void Orbiting_DoesNotTransition_WhenAttackTimerNotElapsed()
    {
        var ai = Create(initialAttackTimer: 1.0f);

        var transition = ai.Update(Dt, horizontalDistanceToPlayer: DefaultTriggerRange - 1f);

        Assert.Equal(DroneAIState.Phase.Orbiting, ai.CurrentPhase);
        Assert.Null(transition);
    }

    [Fact]
    public void Orbiting_DoesNotTransition_WhenOutOfRange_EvenIfTimerElapsed()
    {
        var ai = Create(initialAttackTimer: 0f);

        var transition = ai.Update(Dt, horizontalDistanceToPlayer: DefaultTriggerRange + 10f);

        Assert.Equal(DroneAIState.Phase.Orbiting, ai.CurrentPhase);
        Assert.Null(transition);
        Assert.Equal(0f, ai.AttackTimer);
    }

    [Fact]
    public void Orbiting_TransitionsToTelegraph_WhenInRange_AndTimerElapsed()
    {
        var ai = Create(telegraph: 0.2f, initialAttackTimer: 0f);

        var transition = ai.Update(Dt, horizontalDistanceToPlayer: DefaultTriggerRange);

        Assert.Equal(DroneAIState.Phase.Telegraph, ai.CurrentPhase);
        Assert.Equal(DroneAIState.Phase.Telegraph, transition);
        Assert.True(ai.IsTelegraphing);
        Assert.False(ai.ShouldMove);
        Assert.True(ai.PhaseTimer > 0.15f);
    }

    [Fact]
    public void Telegraph_DoesNotReturnTransition_OnSubsequentFrames()
    {
        var ai = Create(initialAttackTimer: 0f);
        var t1 = ai.Update(Dt, DefaultTriggerRange - 1f); // -> Telegraph
        Assert.Equal(DroneAIState.Phase.Telegraph, t1);

        var t2 = ai.Update(Dt, DefaultTriggerRange - 1f);
        Assert.Null(t2);
    }

    [Fact]
    public void Telegraph_TransitionsToDiving_WhenTimerExpires()
    {
        var ai = Create(telegraph: 0.03f, initialAttackTimer: 0f);
        ai.Update(Dt, DefaultTriggerRange - 1f); // -> Telegraph

        DroneAIState.Phase? finalTransition = null;
        for (int i = 0; i < 20; i++)
        {
            var t = ai.Update(Dt, DefaultTriggerRange - 1f);
            if (t != null) finalTransition = t;
            if (ai.CurrentPhase == DroneAIState.Phase.Diving) break;
        }

        Assert.Equal(DroneAIState.Phase.Diving, ai.CurrentPhase);
        Assert.Equal(DroneAIState.Phase.Diving, finalTransition);
        Assert.True(ai.ShouldMove);
        Assert.True(ai.IsDiving);
    }

    [Fact]
    public void Diving_TransitionsToRecovery_WhenTimerExpires()
    {
        var ai = Create(telegraph: 0.01f, dive: 0.03f, initialAttackTimer: 0f);
        ai.Update(Dt, DefaultTriggerRange - 1f); // -> Telegraph
        ai.Update(Dt, DefaultTriggerRange - 1f); // -> Diving

        DroneAIState.Phase? finalTransition = null;
        for (int i = 0; i < 20; i++)
        {
            var t = ai.Update(Dt, DefaultTriggerRange - 1f);
            if (t != null) finalTransition = t;
            if (ai.CurrentPhase == DroneAIState.Phase.Recovery) break;
        }

        Assert.Equal(DroneAIState.Phase.Recovery, ai.CurrentPhase);
        Assert.Equal(DroneAIState.Phase.Recovery, finalTransition);
        Assert.True(ai.IsRecovering);
    }

    [Fact]
    public void Recovery_TransitionsToOrbiting_AndResetsAttackTimer()
    {
        var ai = Create(interval: 4f, telegraph: 0.01f, dive: 0.01f, recovery: 0.03f, initialAttackTimer: 0f);
        ai.Update(Dt, DefaultTriggerRange - 1f); // -> Telegraph
        ai.Update(Dt, DefaultTriggerRange - 1f); // -> Diving
        ai.Update(Dt, DefaultTriggerRange - 1f); // -> Recovery

        DroneAIState.Phase? finalTransition = null;
        for (int i = 0; i < 20; i++)
        {
            var t = ai.Update(Dt, DefaultTriggerRange - 1f);
            if (t != null) finalTransition = t;
            if (ai.CurrentPhase == DroneAIState.Phase.Orbiting) break;
        }

        Assert.Equal(DroneAIState.Phase.Orbiting, ai.CurrentPhase);
        Assert.Equal(DroneAIState.Phase.Orbiting, finalTransition);
        Assert.Equal(4f, ai.AttackTimer);
    }

    [Fact]
    public void Orbiting_AttackTimer_ClampsAtZero()
    {
        var ai = Create(initialAttackTimer: 0.01f);

        for (int i = 0; i < 10; i++)
            ai.Update(Dt, horizontalDistanceToPlayer: DefaultTriggerRange + 50f);

        Assert.Equal(0f, ai.AttackTimer);
    }
}

