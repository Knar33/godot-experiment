using Godot;
using GodotExperiment.GameLoop;

namespace GodotExperiment;

public partial class GemCounterUI : Control
{
    private ProgressBar _progressBar = null!;
    private Label _gemLabel = null!;

    public override void _Ready()
    {
        _progressBar = GetNode<ProgressBar>("GemProgressBar");
        _gemLabel = GetNode<Label>("GemLabel");

        Visible = false;
        UpdateDisplay(0, UpgradeMeterState.BaseThreshold);

        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.GemCountChanged += OnGemCountChanged;
        gm.StateChanged += OnStateChanged;
    }

    public override void _ExitTree()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        gm.GemCountChanged -= OnGemCountChanged;
        gm.StateChanged -= OnStateChanged;
    }

    private void OnGemCountChanged(int current, int threshold)
    {
        UpdateDisplay(current, threshold);
    }

    private void OnStateChanged(int previous, int current)
    {
        var state = (GameState)current;
        Visible = state == GameState.Playing || state == GameState.Paused;

        if (state == GameState.Countdown)
            UpdateDisplay(0, UpgradeMeterState.BaseThreshold);
    }

    private void UpdateDisplay(int gems, int threshold)
    {
        _progressBar.MaxValue = threshold;
        _progressBar.Value = Mathf.Min(gems, threshold);
        _gemLabel.Text = $"{gems} / {threshold}";
    }
}
