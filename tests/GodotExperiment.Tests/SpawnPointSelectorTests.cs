using GodotExperiment.Enemies;
using Xunit;

namespace GodotExperiment.Tests;

public class SpawnPointSelectorTests
{
    // --- ComputeWeights: basic behavior ---

    [Fact]
    public void ComputeWeights_PointDirectlyInFront_GetsHighWeight()
    {
        float[] sx = { 0f };
        float[] sz = { -10f };
        // Player at origin, facing -Z (forward)
        float[] weights = SpawnPointSelector.ComputeWeights(sx, sz, 0f, 0f, 0f, -1f);

        Assert.Single(weights);
        Assert.True(weights[0] > 0.9f);
    }

    [Fact]
    public void ComputeWeights_PointDirectlyBehind_GetsLowWeight()
    {
        float[] sx = { 0f };
        float[] sz = { 10f };
        // Player at origin, facing -Z
        float[] weights = SpawnPointSelector.ComputeWeights(sx, sz, 0f, 0f, 0f, -1f);

        Assert.Single(weights);
        Assert.True(weights[0] <= SpawnPointSelector.BehindPlayerMinWeight + 0.01f);
    }

    [Fact]
    public void ComputeWeights_PointToSide_GetsMidWeight()
    {
        float[] sx = { 10f };
        float[] sz = { 0f };
        // Player at origin, facing -Z
        float[] weights = SpawnPointSelector.ComputeWeights(sx, sz, 0f, 0f, 0f, -1f);

        Assert.Single(weights);
        Assert.True(weights[0] > 0.3f && weights[0] < 0.7f);
    }

    [Fact]
    public void ComputeWeights_FrontWeightHigherThanBehind()
    {
        float[] sx = { 0f, 0f };
        float[] sz = { -10f, 10f };
        // Player at origin, facing -Z
        float[] weights = SpawnPointSelector.ComputeWeights(sx, sz, 0f, 0f, 0f, -1f);

        Assert.True(weights[0] > weights[1],
            $"Front weight {weights[0]} should exceed behind weight {weights[1]}");
    }

    [Fact]
    public void ComputeWeights_ZeroForward_AllWeightsEqual()
    {
        float[] sx = { 10f, -10f, 0f };
        float[] sz = { 0f, 0f, 10f };

        float[] weights = SpawnPointSelector.ComputeWeights(sx, sz, 0f, 0f, 0f, 0f);

        Assert.All(weights, w => Assert.Equal(1f, w));
    }

    [Fact]
    public void ComputeWeights_MismatchedArrayLengths_Throws()
    {
        float[] sx = { 1f, 2f };
        float[] sz = { 1f };

        Assert.Throws<ArgumentException>(() =>
            SpawnPointSelector.ComputeWeights(sx, sz, 0f, 0f, 1f, 0f));
    }

    // --- ComputeWeights: distribution properties ---

    [Fact]
    public void ComputeWeights_AllWeightsPositive()
    {
        float[] sx = new float[12];
        float[] sz = new float[12];
        float angleStep = MathF.Tau / 12;
        for (int i = 0; i < 12; i++)
        {
            sx[i] = MathF.Cos(i * angleStep) * 28f;
            sz[i] = MathF.Sin(i * angleStep) * 28f;
        }

        float[] weights = SpawnPointSelector.ComputeWeights(sx, sz, 0f, 0f, 1f, 0f);

        Assert.All(weights, w => Assert.True(w > 0f, $"Weight {w} should be positive"));
    }

    [Fact]
    public void ComputeWeights_BehindWeightsNeverBelowMinimum()
    {
        float[] sx = new float[12];
        float[] sz = new float[12];
        float angleStep = MathF.Tau / 12;
        for (int i = 0; i < 12; i++)
        {
            sx[i] = MathF.Cos(i * angleStep) * 28f;
            sz[i] = MathF.Sin(i * angleStep) * 28f;
        }

        float[] weights = SpawnPointSelector.ComputeWeights(sx, sz, 0f, 0f, 1f, 0f);

        Assert.All(weights, w =>
            Assert.True(w >= SpawnPointSelector.BehindPlayerMinWeight - 0.001f,
                $"Weight {w} below minimum {SpawnPointSelector.BehindPlayerMinWeight}"));
    }

    // --- SelectWeighted ---

    [Fact]
    public void SelectWeighted_SingleOption_ReturnsThat()
    {
        float[] weights = { 1f };

        int index = SpawnPointSelector.SelectWeighted(weights, 0.5);

        Assert.Equal(0, index);
    }

    [Fact]
    public void SelectWeighted_EqualWeights_LowRandom_ReturnsFirst()
    {
        float[] weights = { 1f, 1f, 1f };

        int index = SpawnPointSelector.SelectWeighted(weights, 0.0);

        Assert.Equal(0, index);
    }

    [Fact]
    public void SelectWeighted_EqualWeights_HighRandom_ReturnsLast()
    {
        float[] weights = { 1f, 1f, 1f };

        int index = SpawnPointSelector.SelectWeighted(weights, 0.999);

        Assert.Equal(2, index);
    }

    [Fact]
    public void SelectWeighted_HeavilySkewed_MostlySelectsHeavy()
    {
        float[] weights = { 100f, 0.001f };
        int heavyCount = 0;

        for (int i = 0; i < 1000; i++)
        {
            double r = i / 1000.0;
            if (SpawnPointSelector.SelectWeighted(weights, r) == 0)
                heavyCount++;
        }

        Assert.True(heavyCount > 990, $"Heavy option selected {heavyCount}/1000 times");
    }

    [Fact]
    public void SelectWeighted_EmptyWeights_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            SpawnPointSelector.SelectWeighted(Array.Empty<float>(), 0.5));
    }

    // --- Distribution: behind-player bias ---

    [Fact]
    public void SpawnDistribution_FavorsFrontOverBehind()
    {
        float[] sx = new float[12];
        float[] sz = new float[12];
        float angleStep = MathF.Tau / 12;
        for (int i = 0; i < 12; i++)
        {
            sx[i] = MathF.Cos(i * angleStep) * 28f;
            sz[i] = MathF.Sin(i * angleStep) * 28f;
        }

        float[] weights = SpawnPointSelector.ComputeWeights(sx, sz, 0f, 0f, 0f, -1f);

        int frontCount = 0;
        int behindCount = 0;
        int trials = 10000;
        var rng = new Random(42);

        for (int t = 0; t < trials; t++)
        {
            int idx = SpawnPointSelector.SelectWeighted(weights, rng.NextDouble());
            // Points with sz < 0 are "in front" (player faces -Z)
            if (sz[idx] < -1f)
                frontCount++;
            else if (sz[idx] > 1f)
                behindCount++;
        }

        Assert.True(frontCount > behindCount,
            $"Front spawns ({frontCount}) should exceed behind spawns ({behindCount})");
    }
}
