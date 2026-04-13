namespace GodotExperiment.Enemies;

public class DroneAIState
{
    public enum Phase { Orbiting, Telegraph, Diving, Recovery }

    public float DiveTriggerRange { get; }
    public float DiveInterval { get; }
    public float TelegraphDuration { get; }
    public float DiveDuration { get; }
    public float RecoveryDuration { get; }
    public float DiveSpeedMultiplier { get; }

    public Phase CurrentPhase { get; private set; }
    public float PhaseTimer { get; private set; }
    public float AttackTimer { get; private set; }

    public bool ShouldMove => CurrentPhase != Phase.Telegraph;
    public bool IsTelegraphing => CurrentPhase == Phase.Telegraph;
    public bool IsDiving => CurrentPhase == Phase.Diving;
    public bool IsRecovering => CurrentPhase == Phase.Recovery;

    public DroneAIState(
        float diveTriggerRange = 14f,
        float diveInterval = 3.5f,
        float telegraphDuration = 0.25f,
        float diveDuration = 0.35f,
        float recoveryDuration = 0.6f,
        float diveSpeedMultiplier = 7f,
        float initialAttackTimer = -1f)
    {
        if (diveTriggerRange <= 0) throw new ArgumentOutOfRangeException(nameof(diveTriggerRange));
        if (diveInterval <= 0) throw new ArgumentOutOfRangeException(nameof(diveInterval));
        if (telegraphDuration <= 0) throw new ArgumentOutOfRangeException(nameof(telegraphDuration));
        if (diveDuration <= 0) throw new ArgumentOutOfRangeException(nameof(diveDuration));
        if (recoveryDuration <= 0) throw new ArgumentOutOfRangeException(nameof(recoveryDuration));
        if (diveSpeedMultiplier <= 0) throw new ArgumentOutOfRangeException(nameof(diveSpeedMultiplier));

        DiveTriggerRange = diveTriggerRange;
        DiveInterval = diveInterval;
        TelegraphDuration = telegraphDuration;
        DiveDuration = diveDuration;
        RecoveryDuration = recoveryDuration;
        DiveSpeedMultiplier = diveSpeedMultiplier;

        CurrentPhase = Phase.Orbiting;
        AttackTimer = initialAttackTimer >= 0f ? initialAttackTimer : DiveInterval;
    }

    /// <returns>Phase transition that occurred this frame, or null if none.</returns>
    public Phase? Update(float dt, float horizontalDistanceToPlayer)
    {
        Phase? transition = null;

        switch (CurrentPhase)
        {
            case Phase.Orbiting:
                AttackTimer = Math.Max(0f, AttackTimer - dt);
                if (horizontalDistanceToPlayer <= DiveTriggerRange && AttackTimer <= 0f)
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
                    CurrentPhase = Phase.Diving;
                    PhaseTimer = DiveDuration;
                    transition = Phase.Diving;
                }
                break;

            case Phase.Diving:
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
                    CurrentPhase = Phase.Orbiting;
                    AttackTimer = DiveInterval;
                    transition = Phase.Orbiting;
                }
                break;
        }

        return transition;
    }
}

