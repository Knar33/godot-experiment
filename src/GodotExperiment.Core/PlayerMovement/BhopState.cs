namespace GodotExperiment.PlayerMovement;

public class BhopState
{
    public const float DefaultTimingWindow = 0.1f;
    public const float DefaultMaxSpeedMultiplier = 1.8f;
    public const float DefaultBoostPerBhop = 0.12f;
    public const float DefaultDecayRate = 3.0f;

    public float TimingWindow { get; set; } = DefaultTimingWindow;
    public float MaxSpeedMultiplier { get; set; } = DefaultMaxSpeedMultiplier;
    public float BoostPerBhop { get; set; } = DefaultBoostPerBhop;
    public float DecayRate { get; set; } = DefaultDecayRate;

    public float SpeedMultiplier { get; private set; } = 1.0f;
    public bool InChain { get; private set; }
    public int CurrentChainCount { get; private set; }

    public event Action? BhopLanded;
    public event Action? ChainBroken;

    /// <summary>
    /// Attempt a bunny hop. Call when the player jumps while grounded.
    /// </summary>
    /// <param name="timeSinceLanding">How long the player has been on the ground.</param>
    /// <returns>True if the jump counts as a successful bhop (within timing window).</returns>
    public bool TryBhop(float timeSinceLanding)
    {
        if (timeSinceLanding <= TimingWindow)
        {
            SpeedMultiplier = Math.Min(SpeedMultiplier + BoostPerBhop, MaxSpeedMultiplier);
            InChain = true;
            CurrentChainCount++;
            BhopLanded?.Invoke();
            return true;
        }

        BreakChain();
        return false;
    }

    /// <summary>
    /// Decay speed toward 1.0x while grounded and outside the bhop timing window.
    /// </summary>
    public void DecaySpeed(float deltaTime)
    {
        if (SpeedMultiplier > 1.0f)
        {
            SpeedMultiplier = Math.Max(1.0f, SpeedMultiplier - DecayRate * deltaTime);
            if (SpeedMultiplier <= 1.0f)
                BreakChain();
        }
    }

    public void Reset()
    {
        SpeedMultiplier = 1.0f;
        InChain = false;
        CurrentChainCount = 0;
    }

    private void BreakChain()
    {
        if (InChain || CurrentChainCount > 0)
        {
            InChain = false;
            CurrentChainCount = 0;
            ChainBroken?.Invoke();
        }
        InChain = false;
    }
}
