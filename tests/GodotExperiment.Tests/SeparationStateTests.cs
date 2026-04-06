using System.Numerics;
using GodotExperiment.Enemies;
using Xunit;

namespace GodotExperiment.Tests;

public class SeparationStateTests
{
    private const float DefaultRadius = 5f;
    private const float DefaultWeight = 4f;

    private static SeparationState Create(float radius = DefaultRadius, float weight = DefaultWeight, float tangent = 0.4f)
        => new(radius, weight, tangent);

    // --- Construction ---

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void Constructor_InvalidRadius_Throws(float radius)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SeparationState(radius, DefaultWeight));
    }

    [Theory]
    [InlineData(0f)]
    [InlineData(-1f)]
    public void Constructor_InvalidWeight_Throws(float weight)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SeparationState(DefaultRadius, weight));
    }

    [Fact]
    public void Constructor_NegativeTangent_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new SeparationState(DefaultRadius, DefaultWeight, -0.1f));
    }

    [Fact]
    public void Constructor_StoresParameters()
    {
        var state = new SeparationState(3.5f, 6f, 0.5f);

        Assert.Equal(3.5f, state.DetectionRadius);
        Assert.Equal(6f, state.SeparationWeight);
        Assert.Equal(0.5f, state.TangentialFactor);
    }

    // --- No neighbors ---

    [Fact]
    public void NoNeighbors_ReturnsZero()
    {
        var state = Create();
        var result = state.ComputeSeparationForce(Vector2.Zero, ReadOnlySpan<Vector2>.Empty);

        Assert.Equal(Vector2.Zero, result);
    }

    // --- Single neighbor ---

    [Fact]
    public void SingleNeighbor_ProducesForceAway()
    {
        var state = Create(radius: 10f, weight: 1f);
        var me = new Vector2(5f, 0f);
        var neighbors = new[] { new Vector2(3f, 0f) };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.X > 0, "Force should push away (positive X)");
    }

    [Fact]
    public void SingleNeighbor_ForceDirectionMatchesAwayVector()
    {
        var state = Create(radius: 10f, weight: 1f);
        var me = new Vector2(0f, 0f);
        var neighbors = new[] { new Vector2(0f, -2f) };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.Y > 0, "Force should push away (positive Y, opposite of neighbor at -Y)");
    }

    [Fact]
    public void SingleNeighbor_ZeroTangent_ProducesPurelyRadialForce()
    {
        var state = Create(radius: 10f, weight: 1f, tangent: 0f);
        var me = new Vector2(5f, 0f);
        var neighbors = new[] { new Vector2(3f, 0f) };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.X > 0, "Force should push away (positive X)");
        Assert.Equal(0f, result.Y, 3);
    }

    // --- Multiple neighbors ---

    [Fact]
    public void MultipleNeighbors_AverageCorrectly_SymmetricCancels()
    {
        var state = Create(radius: 10f, weight: 1f);
        var me = Vector2.Zero;
        var neighbors = new[]
        {
            new Vector2(2f, 0f),
            new Vector2(-2f, 0f)
        };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.Length() < 0.01f, "Symmetric neighbors should roughly cancel");
    }

    [Fact]
    public void MultipleNeighbors_AsymmetricProducesNetForce()
    {
        var state = Create(radius: 10f, weight: 4f);
        var me = Vector2.Zero;
        var neighbors = new[]
        {
            new Vector2(1f, 0f),
            new Vector2(1f, 1f)
        };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.X < 0, "Net force should push away from the cluster (negative X)");
    }

    // --- Out of range ---

    [Fact]
    public void OutOfRangeNeighbors_Ignored()
    {
        var state = Create(radius: 3f);
        var me = Vector2.Zero;
        var neighbors = new[]
        {
            new Vector2(10f, 0f),
            new Vector2(0f, 5f)
        };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.Equal(Vector2.Zero, result);
    }

    [Fact]
    public void MixedInAndOutOfRange_OnlyCountsInRange()
    {
        var state = Create(radius: 3f, weight: 1f);
        var me = Vector2.Zero;
        var neighbors = new[]
        {
            new Vector2(1f, 0f),   // in range
            new Vector2(10f, 0f)   // out of range
        };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.X < 0, "Should push away from the in-range neighbor at +X");
    }

    // --- Linear distance scaling ---

    [Fact]
    public void CloserNeighbor_ProducesStrongerForce()
    {
        var weight = 1f;
        var radius = 10f;
        var me = Vector2.Zero;

        var stateClose = Create(radius, weight);
        var closeNeighbors = new[] { new Vector2(1f, 0f) };
        var resultClose = stateClose.ComputeSeparationForce(me, closeNeighbors);

        var stateFar = Create(radius, weight);
        var farNeighbors = new[] { new Vector2(5f, 0f) };
        var resultFar = stateFar.ComputeSeparationForce(me, farNeighbors);

        Assert.True(resultClose.Length() > resultFar.Length(),
            "Closer neighbor should produce stronger separation force");
    }

    [Fact]
    public void ForceAtRadiusEdge_ApproachesZero()
    {
        var state = Create(radius: 5f, weight: 4f);
        var me = Vector2.Zero;
        var neighbors = new[] { new Vector2(4.99f, 0f) };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.Length() < 0.05f,
            "Force should approach zero near detection radius edge");
    }

    // --- Overlapping position (symmetry break) ---

    [Fact]
    public void OverlappingPosition_ProducesNonZeroForce()
    {
        var state = Create(radius: 5f, weight: 4f);
        var me = new Vector2(3f, 3f);
        var neighbors = new[] { new Vector2(3f, 3f) };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.Length() > 0, "Overlapping position should produce non-zero force via symmetry break");
    }

    // --- Weight scaling ---

    [Fact]
    public void WeightScalesOutputMagnitude()
    {
        var me = Vector2.Zero;
        var neighbors = new[] { new Vector2(2f, 0f) };

        var stateLight = Create(radius: 10f, weight: 2f);
        var resultLight = stateLight.ComputeSeparationForce(me, neighbors);

        var stateHeavy = Create(radius: 10f, weight: 6f);
        var resultHeavy = stateHeavy.ComputeSeparationForce(me, neighbors);

        Assert.True(resultHeavy.Length() > resultLight.Length(),
            "Higher weight should produce greater magnitude");
        Assert.Equal(6f / 2f, resultHeavy.Length() / resultLight.Length(), 2);
    }

    // --- Large neighbor count stability ---

    [Fact]
    public void LargeNeighborCount_ProducesNormalizedOutput()
    {
        var state = Create(radius: 20f, weight: 5f);
        var me = Vector2.Zero;

        var neighbors = new Vector2[50];
        for (int i = 0; i < neighbors.Length; i++)
        {
            float angle = i * MathF.Tau / neighbors.Length;
            neighbors[i] = new Vector2(MathF.Cos(angle) * 3f, MathF.Sin(angle) * 3f);
        }

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.Length() < 0.1f,
            "Evenly distributed neighbors should roughly cancel out");
    }

    [Fact]
    public void LargeAsymmetricNeighborCount_ProducesStableOutput()
    {
        var state = Create(radius: 20f, weight: 5f);
        var me = Vector2.Zero;

        var neighbors = new Vector2[30];
        for (int i = 0; i < neighbors.Length; i++)
            neighbors[i] = new Vector2(i * 0.3f + 0.5f, 0f);

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(float.IsFinite(result.X) && float.IsFinite(result.Y),
            "Output should be finite for large asymmetric neighbor counts");
    }

    // --- Tangential component ---

    [Fact]
    public void TangentialFactor_AddsPerpendicularComponent()
    {
        var state = Create(radius: 10f, weight: 1f, tangent: 0.5f);
        var me = Vector2.Zero;
        var neighbors = new[] { new Vector2(2f, 0f) };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.X < 0, "Radial component pushes away (negative X)");
        Assert.True(MathF.Abs(result.Y) > 0.01f,
            "Tangential component should produce a perpendicular force");
    }

    [Fact]
    public void TangentialFactor_Zero_ProducesNoPerpendicularComponent()
    {
        var state = Create(radius: 10f, weight: 1f, tangent: 0f);
        var me = Vector2.Zero;
        var neighbors = new[] { new Vector2(2f, 0f) };

        var result = state.ComputeSeparationForce(me, neighbors);

        Assert.True(result.X < 0, "Radial component pushes away");
        Assert.Equal(0f, result.Y, 3);
    }

    [Fact]
    public void HigherTangentialFactor_ProducesStrongerPerpendicularForce()
    {
        var me = Vector2.Zero;
        var neighbors = new[] { new Vector2(2f, 0f) };

        var stateLow = Create(radius: 10f, weight: 1f, tangent: 0.2f);
        var resultLow = stateLow.ComputeSeparationForce(me, neighbors);

        var stateHigh = Create(radius: 10f, weight: 1f, tangent: 0.8f);
        var resultHigh = stateHigh.ComputeSeparationForce(me, neighbors);

        Assert.True(MathF.Abs(resultHigh.Y) > MathF.Abs(resultLow.Y),
            "Higher tangential factor should produce stronger perpendicular force");
    }
}
