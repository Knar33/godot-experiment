using GodotExperiment.Combat;
using Xunit;

namespace GodotExperiment.Tests;

public class AutoFireStateTests
{
    [Fact]
    public void DefaultFireInterval_Is125ms()
    {
        var state = new AutoFireState();
        Assert.Equal(0.125f, state.FireInterval);
    }

    [Fact]
    public void CanFire_ReturnsFalse_Initially()
    {
        var state = new AutoFireState();
        Assert.False(state.CanFire);
    }

    [Fact]
    public void CanFire_ReturnsFalse_BeforeIntervalElapsed()
    {
        var state = new AutoFireState();
        state.Update(0.1f);
        Assert.False(state.CanFire);
    }

    [Fact]
    public void CanFire_ReturnsTrue_AfterIntervalElapsed()
    {
        var state = new AutoFireState();
        state.Update(0.125f);
        Assert.True(state.CanFire);
    }

    [Fact]
    public void CanFire_ReturnsTrue_WellAfterInterval()
    {
        var state = new AutoFireState();
        state.Update(1.0f);
        Assert.True(state.CanFire);
    }

    [Fact]
    public void TryFire_ReturnsTrue_WhenCanFire()
    {
        var state = new AutoFireState();
        state.Update(0.125f);
        Assert.True(state.TryFire());
    }

    [Fact]
    public void TryFire_ReturnsFalse_WhenCannotFire()
    {
        var state = new AutoFireState();
        state.Update(0.05f);
        Assert.False(state.TryFire());
    }

    [Fact]
    public void TryFire_ResetsTimeSinceLastShot()
    {
        var state = new AutoFireState();
        state.Update(0.2f);
        state.TryFire();
        Assert.Equal(0f, state.TimeSinceLastShot);
    }

    [Fact]
    public void TryFire_CannotFireAgain_Immediately()
    {
        var state = new AutoFireState();
        state.Update(0.125f);
        state.TryFire();
        Assert.False(state.CanFire);
        Assert.False(state.TryFire());
    }

    [Fact]
    public void Update_AccumulatesTime()
    {
        var state = new AutoFireState();
        state.Update(0.05f);
        state.Update(0.05f);
        state.Update(0.05f);
        Assert.Equal(0.15f, state.TimeSinceLastShot, 5);
        Assert.True(state.CanFire);
    }

    [Fact]
    public void Reset_ClearsTimeSinceLastShot()
    {
        var state = new AutoFireState();
        state.Update(1.0f);
        state.Reset();
        Assert.Equal(0f, state.TimeSinceLastShot);
        Assert.False(state.CanFire);
    }

    [Fact]
    public void FireRate_Approximately8PerSecond()
    {
        var state = new AutoFireState();
        int shotCount = 0;

        for (float time = 0; time < 1.0f; time += 1f / 60f)
        {
            state.Update(1f / 60f);
            if (state.TryFire())
                shotCount++;
        }

        Assert.InRange(shotCount, 7, 9);
    }

    [Theory]
    [InlineData(0.125f)]
    [InlineData(0.25f)]
    [InlineData(0.0625f)]
    public void CustomFireInterval_RespectedByCanFire(float interval)
    {
        var state = new AutoFireState { FireInterval = interval };
        state.Update(interval - 0.001f);
        Assert.False(state.CanFire);
        state.Update(0.002f);
        Assert.True(state.CanFire);
    }

    [Fact]
    public void TryFire_OnlyFiresOnce_PerInterval()
    {
        var state = new AutoFireState();
        state.Update(0.5f);
        Assert.True(state.TryFire());
        Assert.False(state.TryFire());
    }

    [Fact]
    public void MultipleFireCycles_MaintainConsistentTiming()
    {
        var state = new AutoFireState();
        float dt = 1f / 60f;
        int shotCount = 0;

        for (int frame = 0; frame < 480; frame++)
        {
            state.Update(dt);
            if (state.TryFire())
                shotCount++;
        }

        Assert.InRange(shotCount, 60, 68);
    }
}
