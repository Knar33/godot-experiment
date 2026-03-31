using GodotExperiment.Combat;
using Xunit;

namespace GodotExperiment.Tests;

public class PlayerHealthStateTests
{
    // --- Initial state ---

    [Fact]
    public void InitialState_IsAlive()
    {
        var health = new PlayerHealthState();
        Assert.True(health.IsAlive);
    }

    [Fact]
    public void InitialState_KilledByIsNull()
    {
        var health = new PlayerHealthState();
        Assert.Null(health.KilledBy);
    }

    // --- One-hit death from each damage source ---

    [Theory]
    [InlineData(DamageSource.Contact)]
    [InlineData(DamageSource.Projectile)]
    [InlineData(DamageSource.Explosion)]
    [InlineData(DamageSource.GroundHazard)]
    public void TakeDamage_WhenVulnerable_Dies(DamageSource source)
    {
        var health = new PlayerHealthState();

        bool died = health.TakeDamage(source, isInvulnerable: false);

        Assert.True(died);
        Assert.False(health.IsAlive);
        Assert.Equal(source, health.KilledBy);
    }

    // --- I-frame protection ---

    [Theory]
    [InlineData(DamageSource.Contact)]
    [InlineData(DamageSource.Projectile)]
    [InlineData(DamageSource.Explosion)]
    [InlineData(DamageSource.GroundHazard)]
    public void TakeDamage_WhenInvulnerable_Survives(DamageSource source)
    {
        var health = new PlayerHealthState();

        bool died = health.TakeDamage(source, isInvulnerable: true);

        Assert.False(died);
        Assert.True(health.IsAlive);
        Assert.Null(health.KilledBy);
    }

    // --- Died event ---

    [Fact]
    public void TakeDamage_WhenVulnerable_FiresDiedEvent()
    {
        var health = new PlayerHealthState();
        DamageSource? reportedSource = null;
        health.Died += source => reportedSource = source;

        health.TakeDamage(DamageSource.Contact, isInvulnerable: false);

        Assert.Equal(DamageSource.Contact, reportedSource);
    }

    [Fact]
    public void TakeDamage_WhenInvulnerable_DoesNotFireDiedEvent()
    {
        var health = new PlayerHealthState();
        bool eventFired = false;
        health.Died += _ => eventFired = true;

        health.TakeDamage(DamageSource.Explosion, isInvulnerable: true);

        Assert.False(eventFired);
    }

    // --- Already dead ---

    [Fact]
    public void TakeDamage_WhenAlreadyDead_ReturnsFalse()
    {
        var health = new PlayerHealthState();
        health.TakeDamage(DamageSource.Contact, isInvulnerable: false);

        bool diedAgain = health.TakeDamage(DamageSource.Projectile, isInvulnerable: false);

        Assert.False(diedAgain);
    }

    [Fact]
    public void TakeDamage_WhenAlreadyDead_PreservesOriginalKilledBy()
    {
        var health = new PlayerHealthState();
        health.TakeDamage(DamageSource.Contact, isInvulnerable: false);

        health.TakeDamage(DamageSource.Explosion, isInvulnerable: false);

        Assert.Equal(DamageSource.Contact, health.KilledBy);
    }

    [Fact]
    public void TakeDamage_WhenAlreadyDead_DoesNotFireDiedEventAgain()
    {
        var health = new PlayerHealthState();
        health.TakeDamage(DamageSource.Contact, isInvulnerable: false);

        int eventCount = 0;
        health.Died += _ => eventCount++;
        health.TakeDamage(DamageSource.Projectile, isInvulnerable: false);

        Assert.Equal(0, eventCount);
    }

    // --- Reset ---

    [Fact]
    public void Reset_AfterDeath_RestoresAlive()
    {
        var health = new PlayerHealthState();
        health.TakeDamage(DamageSource.Contact, isInvulnerable: false);

        health.Reset();

        Assert.True(health.IsAlive);
        Assert.Null(health.KilledBy);
    }

    [Fact]
    public void Reset_AllowsDeathAgain()
    {
        var health = new PlayerHealthState();
        health.TakeDamage(DamageSource.Contact, isInvulnerable: false);
        health.Reset();

        bool died = health.TakeDamage(DamageSource.Explosion, isInvulnerable: false);

        Assert.True(died);
        Assert.False(health.IsAlive);
        Assert.Equal(DamageSource.Explosion, health.KilledBy);
    }

    // --- I-frame integration with DodgeRollState ---

    [Fact]
    public void IFrameProtection_DuringDodgeRoll_BlocksDamage()
    {
        var health = new PlayerHealthState();
        var dodgeRoll = new GodotExperiment.PlayerMovement.DodgeRollState();

        dodgeRoll.TryStartRoll();

        Assert.True(dodgeRoll.IsInvulnerable);
        bool died = health.TakeDamage(DamageSource.Contact, dodgeRoll.IsInvulnerable);

        Assert.False(died);
        Assert.True(health.IsAlive);
    }

    [Fact]
    public void IFrameProtection_AfterIFrameWindow_AllowsDamage()
    {
        var health = new PlayerHealthState();
        var dodgeRoll = new GodotExperiment.PlayerMovement.DodgeRollState();

        dodgeRoll.TryStartRoll();
        // Advance past the i-frame window (0.3s) but still within roll duration (0.5s)
        dodgeRoll.Update(0.31f);

        Assert.False(dodgeRoll.IsInvulnerable);
        Assert.True(dodgeRoll.IsRolling);
        bool died = health.TakeDamage(DamageSource.Projectile, dodgeRoll.IsInvulnerable);

        Assert.True(died);
        Assert.False(health.IsAlive);
    }

    [Fact]
    public void IFrameProtection_AtExactIFrameBoundary_BlocksDamage()
    {
        var health = new PlayerHealthState();
        var dodgeRoll = new GodotExperiment.PlayerMovement.DodgeRollState();

        dodgeRoll.TryStartRoll();
        // Advance to just before the i-frame boundary
        dodgeRoll.Update(0.299f);

        Assert.True(dodgeRoll.IsInvulnerable);
        bool died = health.TakeDamage(DamageSource.Explosion, dodgeRoll.IsInvulnerable);

        Assert.False(died);
        Assert.True(health.IsAlive);
    }

    [Fact]
    public void IFrameProtection_NotRolling_AllowsDamage()
    {
        var health = new PlayerHealthState();
        var dodgeRoll = new GodotExperiment.PlayerMovement.DodgeRollState();

        Assert.False(dodgeRoll.IsInvulnerable);
        bool died = health.TakeDamage(DamageSource.GroundHazard, dodgeRoll.IsInvulnerable);

        Assert.True(died);
        Assert.False(health.IsAlive);
    }
}
