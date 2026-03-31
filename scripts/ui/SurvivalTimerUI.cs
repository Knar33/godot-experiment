using Godot;
using GodotExperiment.GameLoop;

namespace GodotExperiment;

public partial class SurvivalTimerUI : Control
{
    private Label _timerLabel = null!;

    public override void _Ready()
    {
        _timerLabel = GetNode<Label>("TimerLabel");
        _timerLabel.Text = "00:00.000";
        _timerLabel.Visible = false;

        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.SurvivalTimeUpdated += OnSurvivalTimeUpdated;
        gm.StateChanged += OnStateChanged;
    }

    public override void _ExitTree()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.SurvivalTimeUpdated -= OnSurvivalTimeUpdated;
        gm.StateChanged -= OnStateChanged;
    }

    private void OnSurvivalTimeUpdated(string formattedTime)
    {
        _timerLabel.Text = formattedTime;
    }

    private void OnStateChanged(int previous, int current)
    {
        var state = (GameState)current;
        _timerLabel.Visible = state == GameState.Playing || state == GameState.Dead || state == GameState.Paused;

        if (state == GameState.Countdown)
            _timerLabel.Text = "00:00.000";
    }
}
