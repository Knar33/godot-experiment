using GodotExperiment.PlayerMovement;
using Xunit;

namespace GodotExperiment.Tests;

public class BhopStateTests
{
    // --- Initial state ---

    [Fact]
    public void InitialSpeedMultiplier_Is1()
    {
        var bhop = new BhopState();
        Assert.Equal(1.0f, bhop.SpeedMultiplier);
    }

    [Fact]
    public void InitialInChain_IsFalse()
    {
        var bhop = new BhopState();
        Assert.False(bhop.InChain);
    }

    // --- Timing window ---

    [Fact]
    public void TryBhop_WithinWindow_ReturnsTrue()
    {
        var bhop = new BhopState();
        Assert.True(bhop.TryBhop(0.05f));
    }

    [Fact]
    public void TryBhop_ExactlyAtWindow_ReturnsTrue()
    {
        var bhop = new BhopState();
        Assert.True(bhop.TryBhop(BhopState.DefaultTimingWindow));
    }

    [Fact]
    public void TryBhop_BeyondWindow_ReturnsFalse()
    {
        var bhop = new BhopState();
        Assert.False(bhop.TryBhop(0.15f));
    }

    [Fact]
    public void TryBhop_AtZero_ReturnsTrue()
    {
        var bhop = new BhopState();
        Assert.True(bhop.TryBhop(0f));
    }

    // --- Speed stacking ---

    [Fact]
    public void SuccessfulBhop_IncreasesSpeedMultiplier()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        Assert.True(bhop.SpeedMultiplier > 1.0f);
    }

    [Fact]
    public void SuccessfulBhop_AddsCorrectBoost()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        Assert.Equal(1.0f + BhopState.DefaultBoostPerBhop, bhop.SpeedMultiplier, 4);
    }

    [Fact]
    public void MultipleBhops_StackSpeed()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        bhop.TryBhop(0f);
        bhop.TryBhop(0f);
        Assert.Equal(1.0f + BhopState.DefaultBoostPerBhop * 3, bhop.SpeedMultiplier, 4);
    }

    [Fact]
    public void SpeedMultiplier_CappedAtMax()
    {
        var bhop = new BhopState();
        for (int i = 0; i < 20; i++)
            bhop.TryBhop(0f);

        Assert.Equal(BhopState.DefaultMaxSpeedMultiplier, bhop.SpeedMultiplier, 4);
    }

    [Fact]
    public void SpeedMultiplier_NeverExceedsCap()
    {
        var bhop = new BhopState();
        for (int i = 0; i < 50; i++)
            bhop.TryBhop(0f);

        Assert.True(bhop.SpeedMultiplier <= BhopState.DefaultMaxSpeedMultiplier);
    }

    // --- Chain tracking ---

    [Fact]
    public void SuccessfulBhop_SetsInChainTrue()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        Assert.True(bhop.InChain);
    }

    [Fact]
    public void FailedBhop_SetsInChainFalse()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        bhop.TryBhop(0.2f);
        Assert.False(bhop.InChain);
    }

    // --- Speed decay ---

    [Fact]
    public void DecaySpeed_ReducesMultiplier()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        float before = bhop.SpeedMultiplier;
        bhop.DecaySpeed(0.05f);
        Assert.True(bhop.SpeedMultiplier < before);
    }

    [Fact]
    public void DecaySpeed_NeverGoesBelowOne()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        bhop.DecaySpeed(10f);
        Assert.Equal(1.0f, bhop.SpeedMultiplier);
    }

    [Fact]
    public void DecaySpeed_AtBaseSpeed_DoesNothing()
    {
        var bhop = new BhopState();
        bhop.DecaySpeed(1f);
        Assert.Equal(1.0f, bhop.SpeedMultiplier);
    }

    [Fact]
    public void DecaySpeed_FullDecay_ClearsChain()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        Assert.True(bhop.InChain);
        bhop.DecaySpeed(10f);
        Assert.False(bhop.InChain);
    }

    [Fact]
    public void DecayRate_MatchesExpectedDuration()
    {
        var bhop = new BhopState();
        for (int i = 0; i < 7; i++)
            bhop.TryBhop(0f);

        float excess = bhop.SpeedMultiplier - 1.0f;
        float expectedDecayTime = excess / BhopState.DefaultDecayRate;

        float elapsed = 0f;
        while (bhop.SpeedMultiplier > 1.0f && elapsed < 5f)
        {
            bhop.DecaySpeed(0.016f);
            elapsed += 0.016f;
        }

        Assert.InRange(elapsed, expectedDecayTime - 0.02f, expectedDecayTime + 0.02f);
    }

    // --- Chain count ---

    [Fact]
    public void InitialChainCount_IsZero()
    {
        var bhop = new BhopState();
        Assert.Equal(0, bhop.CurrentChainCount);
    }

    [Fact]
    public void SuccessfulBhop_IncrementsChainCount()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        Assert.Equal(1, bhop.CurrentChainCount);
        bhop.TryBhop(0f);
        Assert.Equal(2, bhop.CurrentChainCount);
    }

    [Fact]
    public void FailedBhop_ResetsChainCount()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        bhop.TryBhop(0f);
        bhop.TryBhop(0.2f);
        Assert.Equal(0, bhop.CurrentChainCount);
    }

    [Fact]
    public void DecayToBase_ResetsChainCount()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        bhop.DecaySpeed(10f);
        Assert.Equal(0, bhop.CurrentChainCount);
    }

    [Fact]
    public void BhopLanded_EventFires_OnSuccessfulBhop()
    {
        var bhop = new BhopState();
        int fired = 0;
        bhop.BhopLanded += () => fired++;

        bhop.TryBhop(0f);
        Assert.Equal(1, fired);

        bhop.TryBhop(0f);
        Assert.Equal(2, fired);
    }

    [Fact]
    public void ChainBroken_EventFires_OnFailedBhop()
    {
        var bhop = new BhopState();
        int fired = 0;
        bhop.ChainBroken += () => fired++;

        bhop.TryBhop(0f);
        bhop.TryBhop(0.2f);
        Assert.Equal(1, fired);
    }

    [Fact]
    public void ChainBroken_EventFires_OnFullDecay()
    {
        var bhop = new BhopState();
        int fired = 0;
        bhop.ChainBroken += () => fired++;

        bhop.TryBhop(0f);
        bhop.DecaySpeed(10f);
        Assert.Equal(1, fired);
    }

    [Fact]
    public void ChainBroken_DoesNotFireTwice_ForSameBreak()
    {
        var bhop = new BhopState();
        int fired = 0;
        bhop.ChainBroken += () => fired++;

        bhop.TryBhop(0.2f);
        Assert.Equal(0, fired);
    }

    // --- Reset ---

    [Fact]
    public void Reset_RestoresDefaults()
    {
        var bhop = new BhopState();
        bhop.TryBhop(0f);
        bhop.TryBhop(0f);
        bhop.Reset();

        Assert.Equal(1.0f, bhop.SpeedMultiplier);
        Assert.False(bhop.InChain);
        Assert.Equal(0, bhop.CurrentChainCount);
    }
}
