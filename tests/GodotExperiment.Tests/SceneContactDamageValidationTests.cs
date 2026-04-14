using System;
using System.Globalization;
using System.IO;
using Xunit;

namespace GodotExperiment.Tests;

public class SceneContactDamageValidationTests
{
    [Fact]
    public void CrawlerContactDamageSetup_IsDetectable()
    {
        string root = FindRepoRoot();
        string crawlerScenePath = Path.Combine(root, "scenes", "enemies", "Crawler.tscn");

        Assert.True(File.Exists(crawlerScenePath), $"Expected scene file at '{crawlerScenePath}'.");

        string[] lines = File.ReadAllLines(crawlerScenePath);

        float bodyRadius = ReadSubResourceFloat(lines, subResourceId: "SphereShape3D_6qrdm", propertyName: "radius");
        float contactRadius = ReadSubResourceFloat(lines, subResourceId: "SphereShape3D_1", propertyName: "radius");

        Assert.True(contactRadius > bodyRadius,
            $"Crawler contact radius ({contactRadius}) should be greater than body collision radius ({bodyRadius}) to ensure overlap-based contact damage can trigger.");

        AssertBlockContains(lines,
            blockHeaderStartsWith: "[node name=\"ContactArea\" type=\"Area3D\"",
            requiredLine: "collision_mask = 1");

        AssertBlockContains(lines,
            blockHeaderStartsWith: "[node name=\"ContactArea\" type=\"Area3D\"",
            requiredLine: "monitoring = true");
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

    private static float ReadSubResourceFloat(string[] lines, string subResourceId, string propertyName)
    {
        int start = FindLineIndex(lines, $"[sub_resource type=\"SphereShape3D\" id=\"{subResourceId}\"]");
        for (int i = start + 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("[", StringComparison.Ordinal))
                break;

            if (line.StartsWith(propertyName + " = ", StringComparison.Ordinal))
            {
                string value = line[(propertyName.Length + 3)..].Trim();
                return float.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        throw new InvalidOperationException($"Failed to find '{propertyName}' for sub_resource id '{subResourceId}'.");
    }

    private static void AssertBlockContains(string[] lines, string blockHeaderStartsWith, string requiredLine)
    {
        int start = FindLineIndexStartsWith(lines, blockHeaderStartsWith);
        for (int i = start + 1; i < lines.Length; i++)
        {
            string line = lines[i].Trim();
            if (line.StartsWith("[", StringComparison.Ordinal))
                break;

            if (string.Equals(line, requiredLine, StringComparison.Ordinal))
                return;
        }

        throw new Xunit.Sdk.XunitException($"Expected block '{blockHeaderStartsWith}...' to contain line '{requiredLine}'.");
    }

    private static int FindLineIndex(string[] lines, string exact)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (string.Equals(lines[i].Trim(), exact, StringComparison.Ordinal))
                return i;
        }

        throw new InvalidOperationException($"Failed to find line '{exact}'.");
    }

    private static int FindLineIndexStartsWith(string[] lines, string prefix)
    {
        for (int i = 0; i < lines.Length; i++)
        {
            if (lines[i].Trim().StartsWith(prefix, StringComparison.Ordinal))
                return i;
        }

        throw new InvalidOperationException($"Failed to find block header starting with '{prefix}'.");
    }
}

