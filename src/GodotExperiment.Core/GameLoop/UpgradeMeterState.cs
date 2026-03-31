namespace GodotExperiment.GameLoop;

public class UpgradeMeterState
{
    public const int BaseThreshold = 10;
    public const int ThresholdIncrement = 5;

    public int GemsCollected { get; private set; }
    public int UpgradeLevel { get; private set; }
    public int CurrentThreshold => BaseThreshold + ThresholdIncrement * UpgradeLevel;
    public float Progress => CurrentThreshold > 0 ? (float)GemsCollected / CurrentThreshold : 0f;
    public bool IsFull => GemsCollected >= CurrentThreshold;

    public event Action<int, int>? GemsChanged;
    public event Action? ThresholdReached;

    public void AddGems(int count)
    {
        if (count <= 0) return;

        GemsCollected += count;
        GemsChanged?.Invoke(GemsCollected, CurrentThreshold);

        if (GemsCollected >= CurrentThreshold)
            ThresholdReached?.Invoke();
    }

    public void ConsumeUpgrade()
    {
        GemsCollected = 0;
        UpgradeLevel++;
        GemsChanged?.Invoke(GemsCollected, CurrentThreshold);
    }

    public void Reset()
    {
        GemsCollected = 0;
        UpgradeLevel = 0;
    }
}
