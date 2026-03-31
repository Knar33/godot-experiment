using Godot;
using GodotExperiment.GameLoop;

namespace GodotExperiment;

public partial class GameManager : Node
{
    public static GameManager? Instance { get; private set; }

    public GameStateMachine StateMachine { get; } = new();
    public SurvivalTimerState SurvivalTimer { get; } = new();
    public CountdownState Countdown { get; } = new();
    public UpgradeMeterState UpgradeMeter { get; } = new();

    [Signal]
    public delegate void StateChangedEventHandler(int previousState, int newState);

    [Signal]
    public delegate void CountdownTickEventHandler(int number);

    [Signal]
    public delegate void CountdownFinishedEventHandler();

    [Signal]
    public delegate void SurvivalTimeUpdatedEventHandler(string formattedTime);

    [Signal]
    public delegate void GemCountChangedEventHandler(int current, int threshold);

    public GameState CurrentState => StateMachine.Current;

    public override void _Ready()
    {
        Instance = this;
        ProcessMode = ProcessModeEnum.Always;
        StateMachine.StateChanged += OnStateMachineStateChanged;
        Countdown.NumberChanged += OnCountdownNumberChanged;
        Countdown.Finished += OnCountdownFinished;
        UpgradeMeter.GemsChanged += OnGemsChanged;

        StartCountdown();
    }

    public override void _ExitTree()
    {
        StateMachine.StateChanged -= OnStateMachineStateChanged;
        Countdown.NumberChanged -= OnCountdownNumberChanged;
        Countdown.Finished -= OnCountdownFinished;
        UpgradeMeter.GemsChanged -= OnGemsChanged;

        if (Instance == this)
            Instance = null;
    }

    public override void _Process(double delta)
    {
        if (StateMachine.Current == GameState.Countdown)
        {
            Countdown.Update(delta);
        }

        if (StateMachine.Current == GameState.Playing)
        {
            SurvivalTimer.Update(delta);
            EmitSignal(SignalName.SurvivalTimeUpdated, SurvivalTimer.Format());
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (@event.IsActionPressed("restart") && StateMachine.Current == GameState.Dead)
        {
            RestartRun();
        }

        if (@event.IsActionPressed("pause") && StateMachine.Current == GameState.Playing)
        {
            Pause();
        }
        else if (@event.IsActionPressed("pause") && StateMachine.Current == GameState.Paused)
        {
            Resume();
        }
    }

    public bool TransitionTo(GameState state) => StateMachine.TransitionTo(state);

    public void TriggerPlayerDeath()
    {
        SurvivalTimer.Freeze();
        EmitSignal(SignalName.SurvivalTimeUpdated, SurvivalTimer.Format());
    }

    public void AddGems(int count)
    {
        UpgradeMeter.AddGems(count);
    }

    public void RestartRun()
    {
        SurvivalTimer.Reset();
        Countdown.Reset();
        UpgradeMeter.Reset();

        ClearArena();
        ResetPlayer();

        StateMachine.Reset();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        StartCountdown();
    }

    public void Pause()
    {
        if (StateMachine.TransitionTo(GameState.Paused))
        {
            GetTree().Paused = true;
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
    }

    public void Resume()
    {
        if (StateMachine.TransitionTo(GameState.Playing))
        {
            GetTree().Paused = false;
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }

    public void RestartFromPause()
    {
        GetTree().Paused = false;
        StateMachine.TransitionTo(GameState.Countdown);

        SurvivalTimer.Reset();
        Countdown.Reset();
        UpgradeMeter.Reset();
        ClearArena();
        ResetPlayer();

        Input.MouseMode = Input.MouseModeEnum.Captured;
        StartCountdown();
    }

    public void QuitGame()
    {
        GetTree().Quit();
    }

    private void StartCountdown()
    {
        Countdown.Start();
    }

    private void OnCountdownNumberChanged(int number)
    {
        EmitSignal(SignalName.CountdownTick, number);
    }

    private void OnCountdownFinished()
    {
        StateMachine.TransitionTo(GameState.Playing);
        SurvivalTimer.Start();
        EmitSignal(SignalName.CountdownFinished);
    }

    private void OnStateMachineStateChanged(GameState previous, GameState current)
    {
        if (current == GameState.Dead)
            Input.MouseMode = Input.MouseModeEnum.Visible;

        EmitSignal(SignalName.StateChanged, (int)previous, (int)current);
    }

    private void OnGemsChanged(int current, int threshold)
    {
        EmitSignal(SignalName.GemCountChanged, current, threshold);
    }

    private void ClearArena()
    {
        ClearGroup("enemies");
        ClearGroup("projectiles");
        ClearGroup("gems");

        var projectilesContainer = GetTree().Root.FindChild("Projectiles", true, false) as Node;
        if (projectilesContainer != null)
        {
            foreach (var child in projectilesContainer.GetChildren())
                child.QueueFree();
        }
    }

    private void ClearGroup(string group)
    {
        foreach (var node in GetTree().GetNodesInGroup(group))
            node.QueueFree();
    }

    private void ResetPlayer()
    {
        var player = GetTree().GetFirstNodeInGroup("player") as CharacterBody3D;
        if (player == null) return;

        player.GlobalPosition = Vector3.Zero;
        player.Velocity = Vector3.Zero;

        if (player is Player p)
        {
            p.Health.Reset();
            p.Bhop.Reset();
            p.DodgeRoll.Reset();
            p.AutoFire.Reset();

            var mesh = player.GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
            if (mesh != null)
                mesh.Visible = true;
        }
    }
}
