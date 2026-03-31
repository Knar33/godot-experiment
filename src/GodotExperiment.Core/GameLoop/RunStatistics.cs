namespace GodotExperiment.GameLoop;

public class RunStatistics
{
    public int EnemiesKilled { get; private set; }
    public int GemsCollected { get; private set; }
    public int WaveReached { get; private set; }
    public int LongestBhopChain { get; private set; }
    public List<string> UpgradesChosen { get; } = new();

    private int _currentBhopChain;

    public void RecordEnemyKill()
    {
        EnemiesKilled++;
    }

    public void RecordGemsCollected(int count)
    {
        if (count > 0)
            GemsCollected += count;
    }

    public void RecordWaveReached(int wave)
    {
        if (wave > WaveReached)
            WaveReached = wave;
    }

    public void RecordBhopLanded()
    {
        _currentBhopChain++;
        if (_currentBhopChain > LongestBhopChain)
            LongestBhopChain = _currentBhopChain;
    }

    public void RecordBhopChainBroken()
    {
        _currentBhopChain = 0;
    }

    public void RecordUpgradeChosen(string upgradeName)
    {
        UpgradesChosen.Add(upgradeName);
    }

    public void Reset()
    {
        EnemiesKilled = 0;
        GemsCollected = 0;
        WaveReached = 0;
        LongestBhopChain = 0;
        _currentBhopChain = 0;
        UpgradesChosen.Clear();
    }
}
