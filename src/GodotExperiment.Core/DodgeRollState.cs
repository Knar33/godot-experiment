namespace GodotExperiment;

public class DodgeRollState
{
    public const float DefaultCooldown = 1.5f;
    public const float DefaultDuration = 0.5f;
    public const float DefaultIFrameDuration = 0.3f;

    public float Cooldown { get; set; } = DefaultCooldown;
    public float Duration { get; set; } = DefaultDuration;
    public float IFrameDuration { get; set; } = DefaultIFrameDuration;

    public bool IsRolling { get; private set; }
    public bool IsInvulnerable { get; private set; }
    public float RollTimer { get; private set; }
    public float CooldownTimer { get; private set; }

    public bool CanRoll => !IsRolling && CooldownTimer <= 0f;

    /// <summary>
    /// Start a dodge roll if not on cooldown and not already rolling.
    /// </summary>
    public bool TryStartRoll()
    {
        if (!CanRoll) return false;
        IsRolling = true;
        RollTimer = 0f;
        IsInvulnerable = true;
        return true;
    }

    /// <summary>
    /// Advance the roll and cooldown timers by deltaTime.
    /// </summary>
    public void Update(float deltaTime)
    {
        if (CooldownTimer > 0f)
            CooldownTimer = Math.Max(0f, CooldownTimer - deltaTime);

        if (IsRolling)
        {
            RollTimer += deltaTime;
            IsInvulnerable = RollTimer < IFrameDuration;

            if (RollTimer >= Duration)
            {
                IsRolling = false;
                IsInvulnerable = false;
                CooldownTimer = Cooldown;
            }
        }
    }

    public void Reset()
    {
        IsRolling = false;
        IsInvulnerable = false;
        RollTimer = 0f;
        CooldownTimer = 0f;
    }
}
