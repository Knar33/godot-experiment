using GodotExperiment.PlayerMovement;
using Xunit;

namespace GodotExperiment.Tests;

public class DodgeRollStateTests
{
    // --- Initial state ---

    [Fact]
    public void InitialState_CanRoll()
    {
        var roll = new DodgeRollState();
        Assert.True(roll.CanRoll);
    }

    [Fact]
    public void InitialState_NotRolling()
    {
        var roll = new DodgeRollState();
        Assert.False(roll.IsRolling);
    }

    [Fact]
    public void InitialState_NotInvulnerable()
    {
        var roll = new DodgeRollState();
        Assert.False(roll.IsInvulnerable);
    }

    // --- Starting a roll ---

    [Fact]
    public void TryStartRoll_WhenReady_ReturnsTrue()
    {
        var roll = new DodgeRollState();
        Assert.True(roll.TryStartRoll());
    }

    [Fact]
    public void TryStartRoll_SetsIsRolling()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        Assert.True(roll.IsRolling);
    }

    [Fact]
    public void TryStartRoll_SetsInvulnerable()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        Assert.True(roll.IsInvulnerable);
    }

    [Fact]
    public void TryStartRoll_WhileRolling_ReturnsFalse()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        Assert.False(roll.TryStartRoll());
    }

    // --- I-frame window ---

    [Fact]
    public void Invulnerable_DuringIFrameWindow()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(0.15f);
        Assert.True(roll.IsInvulnerable);
    }

    [Fact]
    public void NotInvulnerable_AfterIFrameWindow()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(DodgeRollState.DefaultIFrameDuration + 0.01f);
        Assert.False(roll.IsInvulnerable);
        Assert.True(roll.IsRolling);
    }

    [Fact]
    public void IFrameWindow_MatchesDesign()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();

        roll.Update(0.29f);
        Assert.True(roll.IsInvulnerable);

        roll.Update(0.02f);
        Assert.False(roll.IsInvulnerable);
    }

    // --- Roll duration ---

    [Fact]
    public void Roll_EndsAfterDuration()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(DodgeRollState.DefaultDuration + 0.01f);
        Assert.False(roll.IsRolling);
    }

    [Fact]
    public void Roll_StillActiveBeforeDuration()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(DodgeRollState.DefaultDuration - 0.1f);
        Assert.True(roll.IsRolling);
    }

    // --- Cooldown ---

    [Fact]
    public void Cooldown_StartsAfterRollEnds()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(DodgeRollState.DefaultDuration + 0.01f);
        Assert.False(roll.CanRoll);
        Assert.True(roll.CooldownTimer > 0f);
    }

    [Fact]
    public void Cooldown_PreventsNewRoll()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(DodgeRollState.DefaultDuration + 0.01f);
        Assert.False(roll.TryStartRoll());
    }

    [Fact]
    public void Cooldown_ExpiresAfterDuration()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(DodgeRollState.DefaultDuration + 0.01f);
        roll.Update(DodgeRollState.DefaultCooldown + 0.01f);
        Assert.True(roll.CanRoll);
    }

    [Fact]
    public void Cooldown_CanRollAgainAfterExpiry()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(DodgeRollState.DefaultDuration + 0.01f);
        roll.Update(DodgeRollState.DefaultCooldown + 0.01f);
        Assert.True(roll.TryStartRoll());
        Assert.True(roll.IsRolling);
    }

    [Fact]
    public void CooldownTimer_NeverGoesNegative()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(DodgeRollState.DefaultDuration + 0.01f);
        roll.Update(100f);
        Assert.Equal(0f, roll.CooldownTimer);
    }

    // --- Full sequence ---

    [Fact]
    public void FullSequence_Roll_Wait_RollAgain()
    {
        var roll = new DodgeRollState();

        Assert.True(roll.TryStartRoll());
        Assert.True(roll.IsRolling);
        Assert.True(roll.IsInvulnerable);

        roll.Update(0.31f);
        Assert.True(roll.IsRolling);
        Assert.False(roll.IsInvulnerable);

        roll.Update(0.2f);
        Assert.False(roll.IsRolling);
        Assert.False(roll.CanRoll);

        roll.Update(1.51f);
        Assert.True(roll.CanRoll);

        Assert.True(roll.TryStartRoll());
        Assert.True(roll.IsRolling);
    }

    // --- Reset ---

    [Fact]
    public void Reset_ClearsAllState()
    {
        var roll = new DodgeRollState();
        roll.TryStartRoll();
        roll.Update(0.1f);
        roll.Reset();

        Assert.False(roll.IsRolling);
        Assert.False(roll.IsInvulnerable);
        Assert.Equal(0f, roll.RollTimer);
        Assert.Equal(0f, roll.CooldownTimer);
        Assert.True(roll.CanRoll);
    }
}
