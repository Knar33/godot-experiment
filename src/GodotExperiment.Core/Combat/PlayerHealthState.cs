namespace GodotExperiment.Combat;

public class PlayerHealthState
{
    public bool IsAlive { get; private set; } = true;
    public DamageSource? KilledBy { get; private set; }

    public event Action<DamageSource>? Died;

    /// <summary>
    /// Applies damage from the given source. If the player is invulnerable
    /// (i-frames) or already dead, the damage is ignored. Returns true if
    /// the player died from this damage.
    /// </summary>
    public bool TakeDamage(DamageSource source, bool isInvulnerable)
    {
        if (!IsAlive) return false;
        if (isInvulnerable) return false;

        IsAlive = false;
        KilledBy = source;
        Died?.Invoke(source);
        return true;
    }

    public void Reset()
    {
        IsAlive = true;
        KilledBy = null;
    }
}
