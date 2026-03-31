namespace GodotExperiment.Waves;

public class WaveEnemyGroup
{
    public string EnemyType { get; }
    public int Count { get; }

    public WaveEnemyGroup(string enemyType, int count)
    {
        ArgumentNullException.ThrowIfNull(enemyType);
        if (count <= 0)
            throw new ArgumentOutOfRangeException(nameof(count), "Count must be positive.");

        EnemyType = enemyType;
        Count = count;
    }
}
