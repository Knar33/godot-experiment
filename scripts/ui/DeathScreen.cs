using Godot;
using GodotExperiment.GameLoop;

namespace GodotExperiment;

public partial class DeathScreen : Control
{
    private Label _timeLabel = null!;
    private Label _newBestLabel = null!;
    private Label _enemiesKilledLabel = null!;
    private Label _gemsCollectedLabel = null!;
    private Label _waveReachedLabel = null!;
    private Label _bhopChainLabel = null!;
    private HBoxContainer _upgradesContainer = null!;
    private Label _restartHint = null!;
    private Label _leaderboardPlaceholder = null!;
    private AudioStreamPlayer? _fanfareAudio;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        _timeLabel = GetNode<Label>("Panel/Content/LeftPanel/TimeLabel");
        _newBestLabel = GetNode<Label>("Panel/Content/LeftPanel/NewBestLabel");
        _enemiesKilledLabel = GetNode<Label>("Panel/Content/LeftPanel/StatsContainer/EnemiesKilledLabel");
        _gemsCollectedLabel = GetNode<Label>("Panel/Content/LeftPanel/StatsContainer/GemsCollectedLabel");
        _waveReachedLabel = GetNode<Label>("Panel/Content/LeftPanel/StatsContainer/WaveReachedLabel");
        _bhopChainLabel = GetNode<Label>("Panel/Content/LeftPanel/StatsContainer/BhopChainLabel");
        _upgradesContainer = GetNode<HBoxContainer>("Panel/Content/LeftPanel/StatsContainer/UpgradesContainer");
        _restartHint = GetNode<Label>("Panel/Content/LeftPanel/RestartHint");
        _leaderboardPlaceholder = GetNode<Label>("Panel/Content/RightPanel/LeaderboardPlaceholder");
        _fanfareAudio = GetNodeOrNull<AudioStreamPlayer>("FanfareAudio");

        _newBestLabel.Visible = false;

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
            PopulateDeathScreen();
    }

    private void PopulateDeathScreen()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        _timeLabel.Text = gm.SurvivalTimer.Format();

        bool isNewBest = gm.LastRunWasPersonalBest;
        _newBestLabel.Visible = isNewBest;

        if (isNewBest)
            _fanfareAudio?.Play();

        var stats = gm.RunStats;
        _enemiesKilledLabel.Text = $"Enemies Killed: {stats.EnemiesKilled}";
        _gemsCollectedLabel.Text = $"Gems Collected: {stats.GemsCollected}";
        _waveReachedLabel.Text = $"Wave Reached: {stats.WaveReached}";
        _bhopChainLabel.Text = $"Longest Bhop Chain: {stats.LongestBhopChain}";

        PopulateUpgradeIcons(stats);

        _restartHint.Text = "Press R to restart";
    }

    private void PopulateUpgradeIcons(RunStatistics stats)
    {
        foreach (var child in _upgradesContainer.GetChildren())
            child.QueueFree();

        if (stats.UpgradesChosen.Count == 0)
        {
            var noneLabel = new Label { Text = "None" };
            noneLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            noneLabel.AddThemeFontSizeOverride("font_size", 14);
            _upgradesContainer.AddChild(noneLabel);
            return;
        }

        foreach (var upgrade in stats.UpgradesChosen)
        {
            var label = new Label { Text = $"[{upgrade}]" };
            label.AddThemeFontSizeOverride("font_size", 14);
            _upgradesContainer.AddChild(label);
        }
    }
}
