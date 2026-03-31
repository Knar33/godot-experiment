using Godot;
using GodotExperiment.GameLoop;

namespace GodotExperiment;

public partial class CountdownUI : Control
{
    private Label _countdownLabel = null!;

    public override void _Ready()
    {
        _countdownLabel = GetNode<Label>("CountdownLabel");
        _countdownLabel.Visible = false;

        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.CountdownTick += OnCountdownTick;
        gm.CountdownFinished += OnCountdownFinished;
        gm.StateChanged += OnStateChanged;

        if (gm.CurrentState == GameState.Countdown)
        {
            _countdownLabel.Visible = true;
            _countdownLabel.Text = "3";
        }
    }

    public override void _ExitTree()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.CountdownTick -= OnCountdownTick;
        gm.CountdownFinished -= OnCountdownFinished;
        gm.StateChanged -= OnStateChanged;
    }

    private void OnCountdownTick(int number)
    {
        _countdownLabel.Visible = true;
        _countdownLabel.Text = number.ToString();
    }

    private void OnCountdownFinished()
    {
        _countdownLabel.Text = "GO";

        var tween = CreateTween();
        tween.TweenInterval(0.4);
        tween.TweenCallback(Callable.From(() => _countdownLabel.Visible = false));
    }

    private void OnStateChanged(int previous, int current)
    {
        var state = (GameState)current;
        if (state == GameState.Countdown)
        {
            _countdownLabel.Visible = true;
        }
    }
}
