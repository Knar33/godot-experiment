using GodotExperiment.Enemies;
using Xunit;

namespace GodotExperiment.Tests;

public class ChargerAIStateTests
{
    private const float DefaultRange = 15f;
    private const float DefaultTelegraph = 1.5f;
    private const float DefaultCharge = 0.5f;
    private const float DefaultRecovery = 1.0f;
    private const float DefaultSpeedMult = 6f;
    private const float Dt = 0.016f;

    private ChargerAIState Create(
        float range = DefaultRange,
        float telegraph = DefaultTelegraph,
        float charge = DefaultCharge,
        float recovery = DefaultRecovery,
        float speedMult = DefaultSpeedMult)
        => new(range, telegraph, charge, recovery, speedMult);

    // --- Construction ---

    [Fact]
    public void Constructor_DefaultPhase_IsApproaching()
    {
        var ai = Create();

        Assert.Equal(ChargerAIState.Phase.Approaching, ai.CurrentPhase);
    }

    [Fact]
    public void Constructor_ShouldMove_WhenApproaching()
    {
        var ai = Create();

        Assert.True(ai.ShouldMove);
        Assert.False(ai.IsCharging);
        Assert.False(ai.IsTelegraphing);
        Assert.False(ai.IsRecovering);
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-5f)]
    public void Constructor_InvalidChargeRange_Throws(float range)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChargerAIState(chargeRange: range));
    }

    [Fact]
    public void Constructor_InvalidTelegraphDuration_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChargerAIState(telegraphDuration: 0));
    }

    [Fact]
    public void Constructor_InvalidChargeDuration_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChargerAIState(chargeDuration: 0));
    }

    [Fact]
    public void Constructor_InvalidRecoveryDuration_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChargerAIState(recoveryDuration: 0));
    }

    [Fact]
    public void Constructor_InvalidChargeSpeedMultiplier_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new ChargerAIState(chargeSpeedMultiplier: 0));
    }

    // --- Approaching phase ---

    [Fact]
    public void Approaching_StaysApproaching_WhenOutOfRange()
    {
        var ai = Create();

        var transition = ai.Update(Dt, 50f);

        Assert.Equal(ChargerAIState.Phase.Approaching, ai.CurrentPhase);
        Assert.Null(transition);
        Assert.True(ai.ShouldMove);
    }

    [Fact]
    public void Approaching_TransitionsToTelegraph_WhenInRange()
    {
        var ai = Create(range: 15f);

        var transition = ai.Update(Dt, 14f);

        Assert.Equal(ChargerAIState.Phase.Telegraph, ai.CurrentPhase);
        Assert.Equal(ChargerAIState.Phase.Telegraph, transition);
        Assert.True(ai.IsTelegraphing);
    }

    [Fact]
    public void Approaching_TransitionsToTelegraph_AtExactRange()
    {
        var ai = Create(range: 15f);

        var transition = ai.Update(Dt, 15f);

        Assert.Equal(ChargerAIState.Phase.Telegraph, ai.CurrentPhase);
        Assert.Equal(ChargerAIState.Phase.Telegraph, transition);
    }

    // --- Telegraph phase ---

    [Fact]
    public void Telegraph_DoesNotMove()
    {
        var ai = Create();
        ai.Update(Dt, 10f); // -> Telegraph

        Assert.False(ai.ShouldMove);
        Assert.True(ai.IsTelegraphing);
    }

    [Fact]
    public void Telegraph_SetsTimerToTelegraphDuration()
    {
        var ai = Create(telegraph: 1.5f);
        ai.Update(Dt, 10f); // -> Telegraph

        Assert.True(ai.PhaseTimer > 1.4f);
    }

    [Fact]
    public void Telegraph_StaysTelegraph_BeforeTimerExpires()
    {
        var ai = Create(telegraph: 1.5f);
        ai.Update(Dt, 10f); // -> Telegraph

        var transition = ai.Update(Dt, 10f);

        Assert.Equal(ChargerAIState.Phase.Telegraph, ai.CurrentPhase);
        Assert.Null(transition);
    }

    [Fact]
    public void Telegraph_TransitionsToCharging_WhenTimerExpires()
    {
        var ai = Create(telegraph: 0.1f);
        ai.Update(Dt, 10f); // -> Telegraph

        ChargerAIState.Phase? finalTransition = null;
        for (int i = 0; i < 20; i++)
        {
            var t = ai.Update(Dt, 10f);
            if (t != null) finalTransition = t;
            if (ai.CurrentPhase == ChargerAIState.Phase.Charging) break;
        }

        Assert.Equal(ChargerAIState.Phase.Charging, ai.CurrentPhase);
        Assert.Equal(ChargerAIState.Phase.Charging, finalTransition);
    }

    // --- Charging phase ---

    [Fact]
    public void Charging_ShouldMove_IsTrue()
    {
        var ai = Create(telegraph: 0.01f);
        ai.Update(Dt, 10f); // -> Telegraph
        ai.Update(Dt, 10f); // -> Charging

        Assert.True(ai.ShouldMove);
        Assert.True(ai.IsCharging);
    }

    [Fact]
    public void Charging_SetsTimerToChargeDuration()
    {
        var ai = Create(telegraph: 0.01f, charge: 0.5f);
        ai.Update(Dt, 10f); // -> Telegraph
        ai.Update(Dt, 10f); // -> Charging

        Assert.True(ai.PhaseTimer > 0.4f);
    }

    [Fact]
    public void Charging_TransitionsToRecovery_WhenTimerExpires()
    {
        var ai = Create(telegraph: 0.01f, charge: 0.05f);
        ai.Update(Dt, 10f); // -> Telegraph
        ai.Update(Dt, 10f); // -> Charging

        ChargerAIState.Phase? finalTransition = null;
        for (int i = 0; i < 20; i++)
        {
            var t = ai.Update(Dt, 10f);
            if (t != null) finalTransition = t;
            if (ai.CurrentPhase == ChargerAIState.Phase.Recovery) break;
        }

        Assert.Equal(ChargerAIState.Phase.Recovery, ai.CurrentPhase);
        Assert.Equal(ChargerAIState.Phase.Recovery, finalTransition);
    }

    [Fact]
    public void Charging_IgnoresDistanceToPlayer()
    {
        var ai = Create(telegraph: 0.01f, charge: 1.0f);
        ai.Update(Dt, 10f); // -> Telegraph
        ai.Update(Dt, 10f); // -> Charging

        ai.Update(Dt, 100f);

        Assert.Equal(ChargerAIState.Phase.Charging, ai.CurrentPhase);
    }

    // --- Recovery phase ---

    [Fact]
    public void Recovery_DoesNotMove()
    {
        var ai = Create(telegraph: 0.01f, charge: 0.01f);
        ai.Update(Dt, 10f); // -> Telegraph
        ai.Update(Dt, 10f); // -> Charging
        ai.Update(Dt, 10f); // -> Recovery

        Assert.False(ai.ShouldMove);
        Assert.True(ai.IsRecovering);
        Assert.False(ai.IsCharging);
    }

    [Fact]
    public void Recovery_SetsTimerToRecoveryDuration()
    {
        var ai = Create(telegraph: 0.01f, charge: 0.01f, recovery: 1.0f);
        ai.Update(Dt, 10f); // -> Telegraph
        ai.Update(Dt, 10f); // -> Charging
        ai.Update(Dt, 10f); // -> Recovery

        Assert.True(ai.PhaseTimer > 0.9f);
    }

    [Fact]
    public void Recovery_TransitionsToApproaching_WhenTimerExpires()
    {
        var ai = Create(telegraph: 0.01f, charge: 0.01f, recovery: 0.05f);
        ai.Update(Dt, 10f); // -> Telegraph
        ai.Update(Dt, 10f); // -> Charging
        ai.Update(Dt, 10f); // -> Recovery

        ChargerAIState.Phase? finalTransition = null;
        for (int i = 0; i < 20; i++)
        {
            var t = ai.Update(Dt, 10f);
            if (t != null) finalTransition = t;
            if (ai.CurrentPhase == ChargerAIState.Phase.Approaching) break;
        }

        Assert.Equal(ChargerAIState.Phase.Approaching, ai.CurrentPhase);
        Assert.Equal(ChargerAIState.Phase.Approaching, finalTransition);
    }

    // --- Full cycle ---

    [Fact]
    public void FullCycle_ApproachTelegraphChargeRecoveryApproach()
    {
        var ai = Create(range: 10f, telegraph: 0.5f, charge: 0.5f, recovery: 0.5f);

        Assert.Equal(ChargerAIState.Phase.Approaching, ai.CurrentPhase);

        ai.Update(Dt, 9f);
        Assert.Equal(ChargerAIState.Phase.Telegraph, ai.CurrentPhase);

        AdvanceUntilPhase(ai, ChargerAIState.Phase.Charging, 9f);
        Assert.Equal(ChargerAIState.Phase.Charging, ai.CurrentPhase);

        AdvanceUntilPhase(ai, ChargerAIState.Phase.Recovery, 9f);
        Assert.Equal(ChargerAIState.Phase.Recovery, ai.CurrentPhase);

        AdvanceUntilPhase(ai, ChargerAIState.Phase.Approaching, 9f);
        Assert.Equal(ChargerAIState.Phase.Approaching, ai.CurrentPhase);
    }

    private static void AdvanceUntilPhase(ChargerAIState ai, ChargerAIState.Phase target, float dist, int maxFrames = 200)
    {
        for (int i = 0; i < maxFrames; i++)
        {
            ai.Update(Dt, dist);
            if (ai.CurrentPhase == target) return;
        }
    }

    [Fact]
    public void FullCycle_CanChargeAgain_AfterRecovery()
    {
        var ai = Create(range: 10f, telegraph: 0.03f, charge: 0.03f, recovery: 0.03f);

        // First cycle
        ai.Update(Dt, 9f); // -> Telegraph
        for (int i = 0; i < 5; i++) ai.Update(Dt, 9f); // -> Charging
        for (int i = 0; i < 5; i++) ai.Update(Dt, 9f); // -> Recovery
        for (int i = 0; i < 5; i++) ai.Update(Dt, 50f); // -> Approaching (out of range)
        Assert.Equal(ChargerAIState.Phase.Approaching, ai.CurrentPhase);

        // Second cycle
        ai.Update(Dt, 9f); // -> Telegraph again
        Assert.Equal(ChargerAIState.Phase.Telegraph, ai.CurrentPhase);
    }

    // --- Properties reflect correct phase ---

    [Fact]
    public void ChargeSpeedMultiplier_ExposedFromConstructor()
    {
        var ai = Create(speedMult: 8f);

        Assert.Equal(8f, ai.ChargeSpeedMultiplier);
    }

    [Fact]
    public void Telegraph_DoesNotReturnTransition_OnSubsequentFrames()
    {
        var ai = Create(telegraph: 1.0f);
        var t1 = ai.Update(Dt, 10f); // -> Telegraph
        Assert.Equal(ChargerAIState.Phase.Telegraph, t1);

        var t2 = ai.Update(Dt, 10f);
        Assert.Null(t2);
    }

    [Fact]
    public void Approaching_DoesNotTransition_WhenFarAway()
    {
        var ai = Create(range: 5f);

        for (int i = 0; i < 100; i++)
        {
            var t = ai.Update(Dt, 30f);
            Assert.Null(t);
        }

        Assert.Equal(ChargerAIState.Phase.Approaching, ai.CurrentPhase);
    }
}
