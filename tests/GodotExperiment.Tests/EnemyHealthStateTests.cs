using GodotExperiment.Enemies;
using Xunit;

namespace GodotExperiment.Tests;

public class EnemyHealthStateTests
{
    // --- Construction ---

    [Fact]
    public void Constructor_SetsMaxAndCurrentHealth()
    {
        var health = new EnemyHealthState(10);

        Assert.Equal(10, health.MaxHealth);
        Assert.Equal(10, health.CurrentHealth);
    }

    [Fact]
    public void Constructor_WithZeroOrNegative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyHealthState(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyHealthState(-5));
    }

    [Fact]
    public void InitialState_IsAlive()
    {
        var health = new EnemyHealthState(5);

        Assert.True(health.IsAlive);
        Assert.False(health.IsDead);
    }

    // --- Damage application ---

    [Fact]
    public void TakeDamage_ReducesHealth()
    {
        var health = new EnemyHealthState(10);

        health.TakeDamage(3);

        Assert.Equal(7, health.CurrentHealth);
        Assert.True(health.IsAlive);
    }

    [Fact]
    public void TakeDamage_ToZero_Dies()
    {
        var health = new EnemyHealthState(3);

        bool died = health.TakeDamage(3);

        Assert.True(died);
        Assert.True(health.IsDead);
        Assert.False(health.IsAlive);
        Assert.Equal(0, health.CurrentHealth);
    }

    [Fact]
    public void TakeDamage_BeyondZero_ClampsToZero()
    {
        var health = new EnemyHealthState(3);

        health.TakeDamage(100);

        Assert.Equal(0, health.CurrentHealth);
        Assert.True(health.IsDead);
    }

    [Fact]
    public void TakeDamage_MultiplHits_AccumulatesDamage()
    {
        var health = new EnemyHealthState(5);

        bool died1 = health.TakeDamage(2);
        bool died2 = health.TakeDamage(2);
        bool died3 = health.TakeDamage(2);

        Assert.False(died1);
        Assert.False(died2);
        Assert.True(died3);
        Assert.Equal(0, health.CurrentHealth);
    }

    [Fact]
    public void TakeDamage_ZeroAmount_NoEffect()
    {
        var health = new EnemyHealthState(5);

        bool died = health.TakeDamage(0);

        Assert.False(died);
        Assert.Equal(5, health.CurrentHealth);
    }

    [Fact]
    public void TakeDamage_NegativeAmount_NoEffect()
    {
        var health = new EnemyHealthState(5);

        bool died = health.TakeDamage(-3);

        Assert.False(died);
        Assert.Equal(5, health.CurrentHealth);
    }

    [Fact]
    public void TakeDamage_WhenAlreadyDead_ReturnsFalse()
    {
        var health = new EnemyHealthState(1);
        health.TakeDamage(1);

        bool died = health.TakeDamage(1);

        Assert.False(died);
    }

    // --- Events ---

    [Fact]
    public void TakeDamage_FiresDamagedEvent()
    {
        var health = new EnemyHealthState(5);
        int damagedCount = 0;
        health.Damaged += () => damagedCount++;

        health.TakeDamage(1);

        Assert.Equal(1, damagedCount);
    }

    [Fact]
    public void TakeDamage_KillingBlow_FiresBothEvents()
    {
        var health = new EnemyHealthState(1);
        bool damagedFired = false;
        bool diedFired = false;
        health.Damaged += () => damagedFired = true;
        health.Died += () => diedFired = true;

        health.TakeDamage(1);

        Assert.True(damagedFired);
        Assert.True(diedFired);
    }

    [Fact]
    public void TakeDamage_NonKillingBlow_DoesNotFireDiedEvent()
    {
        var health = new EnemyHealthState(5);
        bool diedFired = false;
        health.Died += () => diedFired = true;

        health.TakeDamage(1);

        Assert.False(diedFired);
    }

    [Fact]
    public void TakeDamage_WhenDead_DoesNotFireEvents()
    {
        var health = new EnemyHealthState(1);
        health.TakeDamage(1);

        int eventCount = 0;
        health.Damaged += () => eventCount++;
        health.Died += () => eventCount++;

        health.TakeDamage(1);

        Assert.Equal(0, eventCount);
    }

    // --- Low health ---

    [Fact]
    public void IsLowHealth_AtFull_ReturnsFalse()
    {
        var health = new EnemyHealthState(10);

        Assert.False(health.IsLowHealth);
    }

    [Fact]
    public void IsLowHealth_Below25Percent_ReturnsTrue()
    {
        var health = new EnemyHealthState(10);
        health.TakeDamage(8); // 2 remaining = 20%

        Assert.True(health.IsLowHealth);
    }

    [Fact]
    public void IsLowHealth_AtExactly25Percent_ReturnsTrue()
    {
        var health = new EnemyHealthState(4);
        health.TakeDamage(3); // 1 remaining = 25%

        Assert.True(health.IsLowHealth);
    }

    [Fact]
    public void IsLowHealth_Above25Percent_ReturnsFalse()
    {
        var health = new EnemyHealthState(10);
        health.TakeDamage(7); // 3 remaining = 30%

        Assert.False(health.IsLowHealth);
    }

    [Fact]
    public void IsLowHealth_WhenDead_ReturnsFalse()
    {
        var health = new EnemyHealthState(4);
        health.TakeDamage(4);

        Assert.False(health.IsLowHealth);
    }

    // --- Health fraction ---

    [Fact]
    public void HealthFraction_AtFull_ReturnsOne()
    {
        var health = new EnemyHealthState(10);

        Assert.Equal(1f, health.HealthFraction);
    }

    [Fact]
    public void HealthFraction_AtHalf_ReturnsFifty()
    {
        var health = new EnemyHealthState(10);
        health.TakeDamage(5);

        Assert.Equal(0.5f, health.HealthFraction);
    }

    [Fact]
    public void HealthFraction_AtZero_ReturnsZero()
    {
        var health = new EnemyHealthState(10);
        health.TakeDamage(10);

        Assert.Equal(0f, health.HealthFraction);
    }

    // --- Reset ---

    [Fact]
    public void Reset_RestoresFullHealth()
    {
        var health = new EnemyHealthState(10);
        health.TakeDamage(10);

        health.Reset();

        Assert.Equal(10, health.CurrentHealth);
        Assert.True(health.IsAlive);
        Assert.False(health.IsDead);
    }

    [Fact]
    public void Reset_AllowsDamagingAgain()
    {
        var health = new EnemyHealthState(3);
        health.TakeDamage(3);
        health.Reset();

        bool died = health.TakeDamage(3);

        Assert.True(died);
        Assert.True(health.IsDead);
    }
}
