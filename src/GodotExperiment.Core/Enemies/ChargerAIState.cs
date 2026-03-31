namespace GodotExperiment.Enemies;

public class ChargerAIState
{
    public enum Phase { Approaching, Telegraph, Charging, Recovery }

    public float ChargeRange { get; }
    public float TelegraphDuration { get; }
    public float ChargeDuration { get; }
    public float RecoveryDuration { get; }
    public float ChargeSpeedMultiplier { get; }

    public Phase CurrentPhase { get; private set; }
    public float PhaseTimer { get; private set; }

    public bool ShouldMove => CurrentPhase == Phase.Approaching || CurrentPhase == Phase.Charging;
    public bool IsCharging => CurrentPhase == Phase.Charging;
    public bool IsTelegraphing => CurrentPhase == Phase.Telegraph;
    public bool IsRecovering => CurrentPhase == Phase.Recovery;

    public ChargerAIState(
        float chargeRange = 20f,
        float telegraphDuration = 1.5f,
        float chargeDuration = 0.7f,
        float recoveryDuration = 0.75f,
        float chargeSpeedMultiplier = 8f)
    {
        if (chargeRange <= 0) throw new ArgumentOutOfRangeException(nameof(chargeRange));
        if (telegraphDuration <= 0) throw new ArgumentOutOfRangeException(nameof(telegraphDuration));
        if (chargeDuration <= 0) throw new ArgumentOutOfRangeException(nameof(chargeDuration));
        if (recoveryDuration <= 0) throw new ArgumentOutOfRangeException(nameof(recoveryDuration));
        if (chargeSpeedMultiplier <= 0) throw new ArgumentOutOfRangeException(nameof(chargeSpeedMultiplier));

        ChargeRange = chargeRange;
        TelegraphDuration = telegraphDuration;
        ChargeDuration = chargeDuration;
        RecoveryDuration = recoveryDuration;
        ChargeSpeedMultiplier = chargeSpeedMultiplier;
        CurrentPhase = Phase.Approaching;
    }

    /// <returns>Phase transition that occurred this frame, or null if none.</returns>
    public Phase? Update(float dt, float distanceToPlayer)
    {
        Phase? transition = null;

        switch (CurrentPhase)
        {
            case Phase.Approaching:
                if (distanceToPlayer <= ChargeRange)
                {
                    CurrentPhase = Phase.Telegraph;
                    PhaseTimer = TelegraphDuration;
                    transition = Phase.Telegraph;
                }
                break;

            case Phase.Telegraph:
                PhaseTimer -= dt;
                if (PhaseTimer <= 0f)
                {
                    CurrentPhase = Phase.Charging;
                    PhaseTimer = ChargeDuration;
                    transition = Phase.Charging;
                }
                break;

            case Phase.Charging:
                PhaseTimer -= dt;
                if (PhaseTimer <= 0f)
                {
                    CurrentPhase = Phase.Recovery;
                    PhaseTimer = RecoveryDuration;
                    transition = Phase.Recovery;
                }
                break;

            case Phase.Recovery:
                PhaseTimer -= dt;
                if (PhaseTimer <= 0f)
                {
                    CurrentPhase = Phase.Approaching;
                    transition = Phase.Approaching;
                }
                break;
        }

        return transition;
    }
}
