namespace GodotExperiment.Waves;

public class WaveDefinition
{
    public int WaveNumber { get; }
    public IReadOnlyList<WaveEnemyGroup> Groups { get; }
    public float SpawnInterval { get; }

    public int TotalEnemyCount
    {
        get
        {
            int total = 0;
            for (int i = 0; i < Groups.Count; i++)
                total += Groups[i].Count;
            return total;
        }
    }

    public WaveDefinition(int waveNumber, IReadOnlyList<WaveEnemyGroup> groups, float spawnInterval)
    {
        if (waveNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(waveNumber));
        if (spawnInterval <= 0f)
            throw new ArgumentOutOfRangeException(nameof(spawnInterval));
        ArgumentNullException.ThrowIfNull(groups);
        if (groups.Count == 0)
            throw new ArgumentException("Wave must have at least one enemy group.", nameof(groups));

        WaveNumber = waveNumber;
        Groups = groups;
        SpawnInterval = spawnInterval;
    }
}
