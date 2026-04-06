using System.Numerics;

namespace GodotExperiment.Enemies;

public class SeparationState
{
    private const float Epsilon = 0.001f;

    public float DetectionRadius { get; }
    public float SeparationWeight { get; }
    public float TangentialFactor { get; }

    private readonly Random _rng = new();

    public SeparationState(float detectionRadius, float separationWeight, float tangentialFactor = 0.4f)
    {
        if (detectionRadius <= 0)
            throw new ArgumentOutOfRangeException(nameof(detectionRadius), "Detection radius must be positive.");
        if (separationWeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(separationWeight), "Separation weight must be positive.");
        if (tangentialFactor < 0)
            throw new ArgumentOutOfRangeException(nameof(tangentialFactor), "Tangential factor must be non-negative.");

        DetectionRadius = detectionRadius;
        SeparationWeight = separationWeight;
        TangentialFactor = tangentialFactor;
    }

    public Vector2 ComputeSeparationForce(Vector2 myPosition, ReadOnlySpan<Vector2> neighborPositions)
    {
        var accumulated = Vector2.Zero;
        int count = 0;

        for (int i = 0; i < neighborPositions.Length; i++)
        {
            Vector2 away = myPosition - neighborPositions[i];
            float distance = away.Length();

            if (distance > DetectionRadius)
                continue;

            float strength;
            if (distance < Epsilon)
            {
                float angle = (float)(_rng.NextDouble() * Math.PI * 2.0);
                away = new Vector2(MathF.Cos(angle), MathF.Sin(angle));
                strength = 1f;
            }
            else
            {
                away = Vector2.Normalize(away);
                strength = 1f - distance / DetectionRadius;
            }

            Vector2 tangent = new(-away.Y, away.X);
            accumulated += (away + tangent * TangentialFactor) * strength;
            count++;
        }

        if (count == 0)
            return Vector2.Zero;

        return accumulated * (SeparationWeight / count);
    }
}
