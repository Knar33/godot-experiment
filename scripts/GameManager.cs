using Godot;

namespace GodotExperiment;

public partial class GameManager : Node
{
    public static GameManager? Instance { get; private set; }

    public GameStateMachine StateMachine { get; } = new();

    [Signal]
    public delegate void StateChangedEventHandler(int previousState, int newState);

    public GameState CurrentState => StateMachine.Current;

    public override void _Ready()
    {
        Instance = this;
        StateMachine.StateChanged += OnStateMachineStateChanged;
    }

    public override void _ExitTree()
    {
        StateMachine.StateChanged -= OnStateMachineStateChanged;
        if (Instance == this)
            Instance = null;
    }

    public bool TransitionTo(GameState state) => StateMachine.TransitionTo(state);

    public void Restart() => StateMachine.Reset();

    private void OnStateMachineStateChanged(GameState previous, GameState current)
    {
        EmitSignal(SignalName.StateChanged, (int)previous, (int)current);
    }
}
