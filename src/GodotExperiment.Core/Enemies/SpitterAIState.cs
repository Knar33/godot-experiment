namespace GodotExperiment.Enemies;

public class SpitterAIState
{
    public enum Phase { Approaching, Planted, Repositioning }

    public float PreferredRange { get; }
    public float RangeTolerance { get; }
    public float AttackInterval { get; }
    public float RepositionCheckInterval { get; }

    public Phase CurrentPhase { get; private set; }
    public float AttackTimer { get; private set; }
    public bool ShouldMove => CurrentPhase != Phase.Planted;

    private float _plantedTime;

    public SpitterAIState(
        float preferredRange = 14f,
        float rangeTolerance = 4f,
        float attackInterval = 3f,
        float repositionCheckInterval = 5f)
    {
        if (preferredRange <= 0) throw new ArgumentOutOfRangeException(nameof(preferredRange));
        if (attackInterval <= 0) throw new ArgumentOutOfRangeException(nameof(attackInterval));

        PreferredRange = preferredRange;
        RangeTolerance = rangeTolerance;
        AttackInterval = attackInterval;
        RepositionCheckInterval = repositionCheckInterval;
        CurrentPhase = Phase.Approaching;
        AttackTimer = attackInterval;
    }

    /// <returns>True if the Spitter should fire this frame.</returns>
    public bool Update(float dt, float distanceToPlayer)
    {
        bool shouldFire = false;

        switch (CurrentPhase)
        {
            case Phase.Approaching:
                if (IsInRange(distanceToPlayer))
                {
                    CurrentPhase = Phase.Planted;
                    AttackTimer = AttackInterval * 0.5f;
                    _plantedTime = 0f;
                }
                break;

            case Phase.Planted:
                _plantedTime += dt;
                AttackTimer -= dt;

                if (AttackTimer <= 0f)
                {
                    shouldFire = true;
                    AttackTimer = AttackInterval;
                }

                if (_plantedTime >= RepositionCheckInterval && !IsInRange(distanceToPlayer))
                {
                    CurrentPhase = Phase.Repositioning;
                }
                break;

            case Phase.Repositioning:
                if (IsInRange(distanceToPlayer))
                {
                    CurrentPhase = Phase.Planted;
                    AttackTimer = AttackInterval * 0.5f;
                    _plantedTime = 0f;
                }
                break;
        }

        return shouldFire;
    }

    public bool IsInRange(float distance)
    {
        return distance >= PreferredRange - RangeTolerance
            && distance <= PreferredRange + RangeTolerance;
    }
}
