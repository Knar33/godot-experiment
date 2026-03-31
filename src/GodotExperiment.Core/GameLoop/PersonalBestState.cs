namespace GodotExperiment.GameLoop;

public class PersonalBestState
{
    public double BestTimeSeconds { get; private set; }
    public bool HasBest => BestTimeSeconds > 0.0;

    /// <summary>
    /// Check if the given time is a new personal best. If so, updates the stored best.
    /// </summary>
    /// <returns>True if this is a new personal best.</returns>
    public bool TrySetNewBest(double elapsedSeconds)
    {
        if (elapsedSeconds <= 0.0)
            return false;

        if (!HasBest || elapsedSeconds > BestTimeSeconds)
        {
            BestTimeSeconds = elapsedSeconds;
            return true;
        }

        return false;
    }

    public void LoadBest(double savedBestSeconds)
    {
        BestTimeSeconds = savedBestSeconds > 0.0 ? savedBestSeconds : 0.0;
    }

    public string FormatBest()
    {
        if (!HasBest) return "--:--.---";

        int totalMs = (int)(BestTimeSeconds * 1000.0);
        int minutes = totalMs / 60000;
        int seconds = (totalMs % 60000) / 1000;
        int millis = totalMs % 1000;
        return $"{minutes:D2}:{seconds:D2}.{millis:D3}";
    }
}
