namespace GodotExperiment.GameLoop;

public class CountdownState
{
    public const double DefaultDuration = 3.0;

    private double _elapsed;
    private double _duration;

    public bool IsActive { get; private set; }
    public int CurrentNumber { get; private set; }
    public bool IsFinished { get; private set; }

    public event Action<int>? NumberChanged;
    public event Action? Finished;

    public void Start(double duration = DefaultDuration)
    {
        _duration = duration;
        _elapsed = 0.0;
        IsActive = true;
        IsFinished = false;
        CurrentNumber = (int)Math.Ceiling(duration);
        NumberChanged?.Invoke(CurrentNumber);
    }

    public void Update(double deltaSeconds)
    {
        if (!IsActive || IsFinished)
            return;

        _elapsed += deltaSeconds;

        int remaining = (int)Math.Ceiling(_duration - _elapsed);
        if (remaining < 0) remaining = 0;

        if (remaining != CurrentNumber && remaining > 0)
        {
            CurrentNumber = remaining;
            NumberChanged?.Invoke(CurrentNumber);
        }

        if (_elapsed >= _duration)
        {
            CurrentNumber = 0;
            IsActive = false;
            IsFinished = true;
            Finished?.Invoke();
        }
    }

    public void Reset()
    {
        _elapsed = 0.0;
        _duration = 0.0;
        IsActive = false;
        IsFinished = false;
        CurrentNumber = 0;
    }
}
