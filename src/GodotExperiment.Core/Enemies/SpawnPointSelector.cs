namespace GodotExperiment.Enemies;

/// <summary>
/// Selects spawn points with a bias against spawning directly behind the player.
/// </summary>
public static class SpawnPointSelector
{
    public const float BehindPlayerMinWeight = 0.2f;

    /// <summary>
    /// Computes per-point weights based on direction relative to the player's facing.
    /// Points in front of the player get higher weight; points behind get lower (but non-zero) weight.
    /// All positions are treated as 2D (X, Z plane).
    /// </summary>
    public static float[] ComputeWeights(
        float[] spawnX, float[] spawnZ,
        float playerX, float playerZ,
        float forwardX, float forwardZ)
    {
        if (spawnX.Length != spawnZ.Length)
            throw new ArgumentException("Spawn coordinate arrays must have equal length.");

        int count = spawnX.Length;
        var weights = new float[count];

        float fwdLen = MathF.Sqrt(forwardX * forwardX + forwardZ * forwardZ);
        if (fwdLen < 0.001f)
        {
            Array.Fill(weights, 1f);
            return weights;
        }

        float nfx = forwardX / fwdLen;
        float nfz = forwardZ / fwdLen;

        for (int i = 0; i < count; i++)
        {
            float dx = spawnX[i] - playerX;
            float dz = spawnZ[i] - playerZ;
            float len = MathF.Sqrt(dx * dx + dz * dz);

            if (len < 0.001f)
            {
                weights[i] = 1f;
                continue;
            }

            float dot = (dx / len) * nfx + (dz / len) * nfz;
            weights[i] = MathF.Max(BehindPlayerMinWeight, dot * 0.5f + 0.5f);
        }

        return weights;
    }

    /// <summary>
    /// Selects an index using weighted random selection.
    /// </summary>
    /// <param name="weights">Non-negative weights for each option.</param>
    /// <param name="randomValue">A uniform random value in [0, 1).</param>
    public static int SelectWeighted(float[] weights, double randomValue)
    {
        if (weights.Length == 0)
            throw new ArgumentException("Weights array must not be empty.");

        float totalWeight = 0f;
        for (int i = 0; i < weights.Length; i++)
            totalWeight += weights[i];

        if (totalWeight <= 0f)
            return 0;

        float threshold = (float)(randomValue * totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < weights.Length; i++)
        {
            cumulative += weights[i];
            if (cumulative > threshold)
                return i;
        }

        return weights.Length - 1;
    }
}
