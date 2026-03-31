using Godot;
using GodotExperiment.GameLoop;

namespace GodotExperiment;

public partial class DeathScreen : Control
{
    private Label _timeLabel = null!;
    private Label _restartHint = null!;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        _timeLabel = GetNode<Label>("Panel/VBoxContainer/TimeLabel");
        _restartHint = GetNode<Label>("Panel/VBoxContainer/RestartHint");

        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.StateChanged += OnStateChanged;
    }

    public override void _ExitTree()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.StateChanged -= OnStateChanged;
    }

    private void OnStateChanged(int previous, int current)
    {
        var state = (GameState)current;
        Visible = state == GameState.Dead;

        if (state == GameState.Dead)
        {
            var gm = GameManager.Instance;
            if (gm != null)
                _timeLabel.Text = gm.SurvivalTimer.Format();
            _restartHint.Text = "Press R to restart";
        }
    }
}
