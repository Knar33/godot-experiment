namespace GodotExperiment.Combat;

public class AutoFireState
{
    public const float DefaultFireInterval = 0.125f;

    public float FireInterval { get; set; } = DefaultFireInterval;
    public float TimeSinceLastShot { get; private set; }

    public bool CanFire => TimeSinceLastShot >= FireInterval;

    public void Update(float deltaTime)
    {
        TimeSinceLastShot += deltaTime;
    }

    public bool TryFire()
    {
        if (!CanFire) return false;
        TimeSinceLastShot = 0f;
        return true;
    }

    public void Reset()
    {
        TimeSinceLastShot = 0f;
    }

    public void ResetToReady()
    {
        TimeSinceLastShot = FireInterval;
    }
}
