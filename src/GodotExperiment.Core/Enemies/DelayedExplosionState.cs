using System;

namespace GodotExperiment.Enemies;

public sealed class DelayedExplosionState
{
    public float DelaySeconds { get; }
    public float RemainingSeconds { get; private set; }
    public bool IsArmed { get; private set; }
    public bool HasExploded { get; private set; }

    public DelayedExplosionState(float delaySeconds)
    {
        if (delaySeconds < 0f)
            throw new ArgumentOutOfRangeException(nameof(delaySeconds), "Delay must be non-negative.");

        DelaySeconds = delaySeconds;
    }

    public void Arm()
    {
        IsArmed = true;
        HasExploded = false;
        RemainingSeconds = DelaySeconds;
    }

    public bool Update(float dt)
    {
        if (!IsArmed || HasExploded) return false;

        RemainingSeconds -= dt;
        if (RemainingSeconds > 0f) return false;

        HasExploded = true;
        return true;
    }

    public void Reset()
    {
        IsArmed = false;
        HasExploded = false;
        RemainingSeconds = 0f;
    }
}

