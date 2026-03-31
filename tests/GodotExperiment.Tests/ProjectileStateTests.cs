using GodotExperiment.Combat;
using Xunit;

namespace GodotExperiment.Tests;

public class ProjectileStateTests
{
    [Fact]
    public void DistanceTraveled_StartsAtZero()
    {
        var state = new ProjectileState(25f);
        Assert.Equal(0f, state.DistanceTraveled);
    }

    [Fact]
    public void IsExpired_ReturnsFalse_Initially()
    {
        var state = new ProjectileState(25f);
        Assert.False(state.IsExpired);
    }

    [Fact]
    public void IsExpired_ReturnsFalse_BeforeMaxRange()
    {
        var state = new ProjectileState(25f);
        state.AddDistance(24.9f);
        Assert.False(state.IsExpired);
    }

    [Fact]
    public void IsExpired_ReturnsTrue_AtMaxRange()
    {
        var state = new ProjectileState(25f);
        state.AddDistance(25f);
        Assert.True(state.IsExpired);
    }

    [Fact]
    public void IsExpired_ReturnsTrue_BeyondMaxRange()
    {
        var state = new ProjectileState(25f);
        state.AddDistance(30f);
        Assert.True(state.IsExpired);
    }

    [Fact]
    public void AddDistance_AccumulatesCorrectly()
    {
        var state = new ProjectileState(100f);
        state.AddDistance(10f);
        state.AddDistance(15f);
        state.AddDistance(5f);
        Assert.Equal(30f, state.DistanceTraveled, 5);
    }

    [Fact]
    public void AddDistance_SmallIncrements_ReachMaxRange()
    {
        var state = new ProjectileState(25f);
        float speed = 50f;
        float dt = 1f / 60f;

        while (!state.IsExpired)
            state.AddDistance(speed * dt);

        Assert.True(state.DistanceTraveled >= 25f);
        Assert.True(state.DistanceTraveled < 26f);
    }

    [Theory]
    [InlineData(10f)]
    [InlineData(25f)]
    [InlineData(50f)]
    public void MaxRange_SetFromConstructor(float maxRange)
    {
        var state = new ProjectileState(maxRange);
        Assert.Equal(maxRange, state.MaxRange);
    }

    [Fact]
    public void TravelTime_MatchesExpected_At50UnitsPerSecond()
    {
        var state = new ProjectileState(25f);
        float speed = 50f;
        float dt = 1f / 60f;
        int frames = 0;

        while (!state.IsExpired)
        {
            state.AddDistance(speed * dt);
            frames++;
        }

        float expectedTime = 25f / 50f;
        float actualTime = frames * dt;
        Assert.InRange(actualTime, expectedTime - dt, expectedTime + dt);
    }
}
