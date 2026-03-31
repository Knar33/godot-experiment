using Godot;
using GodotExperiment.GameLoop;

namespace GodotExperiment;

public partial class PauseMenu : Control
{
    private Button _resumeButton = null!;
    private Button _restartButton = null!;
    private Button _quitButton = null!;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        Visible = false;

        _resumeButton = GetNode<Button>("Panel/VBoxContainer/ResumeButton");
        _restartButton = GetNode<Button>("Panel/VBoxContainer/RestartButton");
        _quitButton = GetNode<Button>("Panel/VBoxContainer/QuitButton");

        _resumeButton.Pressed += OnResumePressed;
        _restartButton.Pressed += OnRestartPressed;
        _quitButton.Pressed += OnQuitPressed;

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
        Visible = state == GameState.Paused;
    }

    private void OnResumePressed()
    {
        GameManager.Instance?.Resume();
    }

    private void OnRestartPressed()
    {
        GameManager.Instance?.RestartFromPause();
    }

    private void OnQuitPressed()
    {
        GameManager.Instance?.QuitGame();
    }
}
