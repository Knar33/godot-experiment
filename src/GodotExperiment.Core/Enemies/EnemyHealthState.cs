namespace GodotExperiment.Enemies;

public class EnemyHealthState
{
    public int MaxHealth { get; }
    public int CurrentHealth { get; private set; }
    public bool IsAlive => CurrentHealth > 0;
    public bool IsDead => CurrentHealth <= 0;
    public bool IsLowHealth => IsAlive && CurrentHealth <= MaxHealth * 0.25f;
    public float HealthFraction => MaxHealth > 0 ? (float)CurrentHealth / MaxHealth : 0f;

    public event Action? Died;
    public event Action? Damaged;

    public EnemyHealthState(int maxHealth)
    {
        if (maxHealth <= 0)
            throw new ArgumentOutOfRangeException(nameof(maxHealth), "Max health must be positive.");

        MaxHealth = maxHealth;
        CurrentHealth = maxHealth;
    }

    /// <summary>
    /// Applies damage. Returns true if this damage killed the enemy.
    /// </summary>
    public bool TakeDamage(int amount)
    {
        if (IsDead) return false;
        if (amount <= 0) return false;

        CurrentHealth = Math.Max(0, CurrentHealth - amount);
        Damaged?.Invoke();

        if (IsDead)
        {
            Died?.Invoke();
            return true;
        }

        return false;
    }

    public void Reset()
    {
        CurrentHealth = MaxHealth;
    }
}
