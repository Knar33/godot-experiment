namespace GodotExperiment.Waves;

/// <summary>
/// Provides wave definitions keyed by wave number.
/// Waves 1-5 are hand-authored per design doc.
/// Waves 6+ use a scaling formula (placeholder until mid/late compositions are implemented).
/// </summary>
public static class WaveCompositions
{
    public const string Crawler = "Crawler";
    public const string Spitter = "Spitter";
    public const string Charger = "Charger";
    public const string Drone = "Drone";
    public const string Bloater = "Bloater";
    public const string Shade = "Shade";
    public const string Burrower = "Burrower";
    public const string Sentinel = "Sentinel";
    public const string Howler = "Howler";
    public const string Titan = "Titan";

    private static readonly Dictionary<int, WaveDefinition> DefinedWaves = new();

    public static int DefinedWaveCount => DefinedWaves.Count;

    static WaveCompositions()
    {
        Define(1, 2.0f, (Crawler, 5));
        Define(2, 1.5f, (Crawler, 10));
        Define(3, 1.2f, (Crawler, 10), (Spitter, 2));
        Define(4, 1.0f, (Crawler, 12), (Spitter, 3), (Charger, 1));
        Define(5, 0.8f, (Crawler, 14), (Spitter, 3), (Charger, 1), (Drone, 6));
    }

    public static WaveDefinition GetWave(int waveNumber)
    {
        if (waveNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(waveNumber));

        if (DefinedWaves.TryGetValue(waveNumber, out var wave))
            return wave;

        return GenerateScalingWave(waveNumber);
    }

    /// <summary>
    /// Scaling formula for waves beyond hand-authored definitions.
    /// Placeholder until mid/late wave compositions are implemented (task 15).
    /// </summary>
    private static WaveDefinition GenerateScalingWave(int waveNumber)
    {
        int crawlerCount = 10 + (waveNumber - 5) * 3;
        float interval = Math.Max(0.3f, 1.0f - (waveNumber - 5) * 0.05f);

        int bloaterCount = waveNumber < 10 ? 1 : 2;

        var groups = new List<WaveEnemyGroup>
        {
            new(Crawler, crawlerCount),
            new(Bloater, bloaterCount)
        };
        return new WaveDefinition(waveNumber, groups, interval);
    }

    private static void Define(int number, float interval, params (string type, int count)[] groups)
    {
        var waveGroups = new List<WaveEnemyGroup>(groups.Length);
        foreach (var (type, count) in groups)
            waveGroups.Add(new WaveEnemyGroup(type, count));

        DefinedWaves[number] = new WaveDefinition(number, waveGroups, interval);
    }
}
