using System.Numerics;

namespace GodotExperiment.Enemies;

public class SeparationState
{
    private const float Epsilon = 0.001f;

    public float DetectionRadius { get; }
    public float SeparationWeight { get; }

    private readonly Random _rng = new();

    public SeparationState(float detectionRadius, float separationWeight)
    {
        if (detectionRadius <= 0)
            throw new ArgumentOutOfRangeException(nameof(detectionRadius), "Detection radius must be positive.");
        if (separationWeight <= 0)
            throw new ArgumentOutOfRangeException(nameof(separationWeight), "Separation weight must be positive.");

        DetectionRadius = detectionRadius;
        SeparationWeight = separationWeight;
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

            accumulated += away * strength;
            count++;
        }

        if (count == 0)
            return Vector2.Zero;

        return accumulated * (SeparationWeight / count);
    }
}
