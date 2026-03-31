namespace GodotExperiment.GameLoop;

public class SurvivalTimerState
{
    public double ElapsedSeconds { get; private set; }
    public bool IsRunning { get; private set; }

    public void Start()
    {
        IsRunning = true;
    }

    public void Freeze()
    {
        IsRunning = false;
    }

    public void Update(double deltaSeconds)
    {
        if (IsRunning)
            ElapsedSeconds += deltaSeconds;
    }

    public void Reset()
    {
        ElapsedSeconds = 0.0;
        IsRunning = false;
    }

    public string Format()
    {
        int totalMs = (int)(ElapsedSeconds * 1000.0);
        int minutes = totalMs / 60000;
        int seconds = (totalMs % 60000) / 1000;
        int millis = totalMs % 1000;
        return $"{minutes:D2}:{seconds:D2}.{millis:D3}";
    }
}
