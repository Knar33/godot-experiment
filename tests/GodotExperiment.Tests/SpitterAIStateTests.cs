using GodotExperiment.Enemies;
using Xunit;

namespace GodotExperiment.Tests;

public class SpitterAIStateTests
{
    private const float DefaultRange = 14f;
    private const float DefaultTolerance = 4f;
    private const float DefaultAttackInterval = 3f;
    private const float DefaultRepositionInterval = 5f;

    private SpitterAIState Create(
        float range = DefaultRange,
        float tolerance = DefaultTolerance,
        float attackInterval = DefaultAttackInterval,
        float repositionInterval = DefaultRepositionInterval)
        => new(range, tolerance, attackInterval, repositionInterval);

    // --- Construction ---

    [Fact]
    public void Constructor_DefaultPhase_IsApproaching()
    {
        var ai = Create();

        Assert.Equal(SpitterAIState.Phase.Approaching, ai.CurrentPhase);
    }

    [Fact]
    public void Constructor_ShouldMoveWhileApproaching()
    {
        var ai = Create();

        Assert.True(ai.ShouldMove);
    }

    [Fact]
    public void Constructor_InvalidRange_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpitterAIState(preferredRange: 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpitterAIState(preferredRange: -5));
    }

    [Fact]
    public void Constructor_InvalidAttackInterval_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SpitterAIState(attackInterval: 0));
    }

    // --- Range detection ---

    [Fact]
    public void IsInRange_AtPreferredRange_ReturnsTrue()
    {
        var ai = Create();

        Assert.True(ai.IsInRange(DefaultRange));
    }

    [Fact]
    public void IsInRange_WithinTolerance_ReturnsTrue()
    {
        var ai = Create();

        Assert.True(ai.IsInRange(DefaultRange - DefaultTolerance));
        Assert.True(ai.IsInRange(DefaultRange + DefaultTolerance));
        Assert.True(ai.IsInRange(DefaultRange - 1f));
        Assert.True(ai.IsInRange(DefaultRange + 2f));
    }

    [Fact]
    public void IsInRange_OutsideTolerance_ReturnsFalse()
    {
        var ai = Create();

        Assert.False(ai.IsInRange(DefaultRange - DefaultTolerance - 1f));
        Assert.False(ai.IsInRange(DefaultRange + DefaultTolerance + 1f));
        Assert.False(ai.IsInRange(0f));
        Assert.False(ai.IsInRange(50f));
    }

    // --- Phase transitions ---

    [Fact]
    public void Approaching_TransitionsToPlanted_WhenInRange()
    {
        var ai = Create();

        ai.Update(0.016f, DefaultRange);

        Assert.Equal(SpitterAIState.Phase.Planted, ai.CurrentPhase);
        Assert.False(ai.ShouldMove);
    }

    [Fact]
    public void Approaching_StaysApproaching_WhenTooFar()
    {
        var ai = Create();

        ai.Update(0.016f, 50f);

        Assert.Equal(SpitterAIState.Phase.Approaching, ai.CurrentPhase);
        Assert.True(ai.ShouldMove);
    }

    [Fact]
    public void Approaching_StaysApproaching_WhenTooClose()
    {
        var ai = Create();

        ai.Update(0.016f, 2f);

        Assert.Equal(SpitterAIState.Phase.Approaching, ai.CurrentPhase);
    }

    // --- Attack timing ---

    [Fact]
    public void Planted_FirstAttack_FiresAtHalfInterval()
    {
        var ai = Create(attackInterval: 2f);
        ai.Update(0.016f, DefaultRange); // transition to planted

        bool fired = false;
        for (int i = 0; i < 100; i++)
        {
            if (ai.Update(0.016f, DefaultRange))
            {
                fired = true;
                break;
            }
        }

        Assert.True(fired);
    }

    [Fact]
    public void Planted_DoesNotFire_BeforeTimerExpires()
    {
        var ai = Create(attackInterval: 10f);
        ai.Update(0.016f, DefaultRange); // transition to planted

        bool fired = ai.Update(0.016f, DefaultRange);

        Assert.False(fired);
    }

    [Fact]
    public void Planted_FiresRepeatedly_AtInterval()
    {
        var ai = Create(attackInterval: 1f);
        ai.Update(0.016f, DefaultRange); // transition to planted

        int fireCount = 0;
        for (int i = 0; i < 300; i++)
        {
            if (ai.Update(0.016f, DefaultRange))
                fireCount++;
        }

        // ~4.8s total, first fire at 0.5s, then at 1.5s, 2.5s, 3.5s, 4.5s = 5 fires
        Assert.True(fireCount >= 4);
    }

    // --- Repositioning ---

    [Fact]
    public void Planted_TransitionsToRepositioning_WhenOutOfRangeAfterInterval()
    {
        var ai = Create(repositionInterval: 1f);
        ai.Update(0.016f, DefaultRange); // transition to planted

        // Simulate being planted for > repositionInterval while player moves out of range
        for (int i = 0; i < 100; i++)
            ai.Update(0.016f, DefaultRange); // in range, stays planted

        // Now player moves far away
        ai.Update(0.016f, 50f);

        Assert.Equal(SpitterAIState.Phase.Repositioning, ai.CurrentPhase);
        Assert.True(ai.ShouldMove);
    }

    [Fact]
    public void Planted_StaysPlanted_WhenOutOfRangeBeforeInterval()
    {
        var ai = Create(repositionInterval: 100f);
        ai.Update(0.016f, DefaultRange); // transition to planted

        // Player moves away immediately — not enough time elapsed
        ai.Update(0.016f, 50f);

        Assert.Equal(SpitterAIState.Phase.Planted, ai.CurrentPhase);
    }

    [Fact]
    public void Repositioning_TransitionsToPlanted_WhenBackInRange()
    {
        var ai = Create(repositionInterval: 0.1f);
        ai.Update(0.016f, DefaultRange); // → planted

        // Wait past reposition interval
        for (int i = 0; i < 20; i++)
            ai.Update(0.016f, DefaultRange);

        // Force out of range → repositioning
        ai.Update(0.016f, 50f);
        Assert.Equal(SpitterAIState.Phase.Repositioning, ai.CurrentPhase);

        // Move back in range
        ai.Update(0.016f, DefaultRange);
        Assert.Equal(SpitterAIState.Phase.Planted, ai.CurrentPhase);
    }

    [Fact]
    public void Repositioning_StaysRepositioning_WhenStillOutOfRange()
    {
        var ai = Create(repositionInterval: 0.1f);
        ai.Update(0.016f, DefaultRange); // → planted

        for (int i = 0; i < 20; i++)
            ai.Update(0.016f, DefaultRange);

        ai.Update(0.016f, 50f); // → repositioning

        ai.Update(0.016f, 50f);
        Assert.Equal(SpitterAIState.Phase.Repositioning, ai.CurrentPhase);
    }

    // --- Attack timer resets on replant ---

    [Fact]
    public void Repositioning_ResetsAttackTimer_WhenReplanted()
    {
        var ai = Create(attackInterval: 2f, repositionInterval: 0.1f);
        ai.Update(0.016f, DefaultRange); // → planted

        for (int i = 0; i < 20; i++)
            ai.Update(0.016f, DefaultRange);

        ai.Update(0.016f, 50f); // → repositioning
        ai.Update(0.016f, DefaultRange); // → replanted, timer = 1.0s

        bool firedEarly = false;
        // Should fire around half-interval (1.0s), not immediately
        for (int i = 0; i < 5; i++)
        {
            if (ai.Update(0.016f, DefaultRange))
                firedEarly = true;
        }

        Assert.False(firedEarly);
    }
}
