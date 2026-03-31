namespace GodotExperiment.Combat;

public class ProjectileState
{
    public float MaxRange { get; }
    public float DistanceTraveled { get; private set; }

    public bool IsExpired => DistanceTraveled >= MaxRange;

    public ProjectileState(float maxRange)
    {
        MaxRange = maxRange;
    }

    public void AddDistance(float distance)
    {
        DistanceTraveled += distance;
    }
}
