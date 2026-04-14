using System;
using System.IO;
using Xunit;

namespace GodotExperiment.Tests;

public class RespawnHazardValidationTests
{
    [Fact]
    public void Restart_ClearsHazardsGroup()
    {
        string root = FindRepoRoot();
        string gameManagerPath = Path.Combine(root, "scripts", "managers", "GameManager.cs");

        Assert.True(File.Exists(gameManagerPath), $"Expected script file at '{gameManagerPath}'.");

        string content = File.ReadAllText(gameManagerPath);
        Assert.Contains("ClearGroup(\"hazards\")", content);
    }

    [Fact]
    public void SpitterGroundHazard_IsGroupedAndGatedToPlayingState()
    {
        string root = FindRepoRoot();
        string hazardPath = Path.Combine(root, "scripts", "enemies", "SpitterGroundHazard.cs");

        Assert.True(File.Exists(hazardPath), $"Expected script file at '{hazardPath}'.");

        string content = File.ReadAllText(hazardPath);
        Assert.Contains("AddToGroup(\"hazards\")", content);
        Assert.Contains("GameState.Playing", content);
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "project.godot")))
            dir = dir.Parent;

        if (dir == null)
            throw new InvalidOperationException("Failed to locate repo root (could not find project.godot in any parent directory).");

        return dir.FullName;
    }
}

