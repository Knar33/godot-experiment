namespace GodotExperiment.GameLoop;

public class GameStateMachine
{
    private static readonly Dictionary<GameState, HashSet<GameState>> ValidTransitions = new()
    {
        [GameState.Countdown] = [GameState.Playing],
        [GameState.Playing] = [GameState.Dead, GameState.Paused],
        [GameState.Dead] = [GameState.Countdown],
        [GameState.Paused] = [GameState.Playing, GameState.Countdown],
    };

    public GameState Current { get; private set; } = GameState.Countdown;
    public GameState Previous { get; private set; } = GameState.Countdown;

    public event Action<GameState, GameState>? StateChanged;

    public bool TransitionTo(GameState target)
    {
        if (!ValidTransitions.TryGetValue(Current, out var valid) || !valid.Contains(target))
            return false;

        Previous = Current;
        Current = target;
        StateChanged?.Invoke(Previous, Current);
        return true;
    }

    public void Reset()
    {
        Previous = Current;
        Current = GameState.Countdown;
        StateChanged?.Invoke(Previous, Current);
    }
}
