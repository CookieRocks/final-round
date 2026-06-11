using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class FinalRoundPlaytestBuild
{
    private const string InterviewRoomScenePath = "Assets/Scenes/InterviewRoom.unity";
    private const string DeskScenePath = "Assets/Scenes/DeskScene.unity";
    private const string OutputRoot = "Builds/Playtest";
    private const string BuildFolder = OutputRoot + "/FinalRound_VS2_Windows";
    private const string ExecutablePath = BuildFolder + "/FinalRound.exe";
    private const string PackagePath = OutputRoot + "/FinalRound_VS2_Playtest.zip";
    private const string ReadmeSourcePath = "Docs/FinalRound_Playtest_README.md";
    private const string ReadmeBuildFileName = "README_Playtest.md";

    private static readonly string[] RequiredScenePaths =
    {
        InterviewRoomScenePath,
        DeskScenePath
    };

    [MenuItem("Final Round/Build Playtest Windows")]
    public static void BuildPlaytestWindows()
    {
        try
        {
            ValidateRequiredSceneAssets();
            EnsureRequiredScenesInBuildSettings();
            Directory.CreateDirectory(BuildFolder);
            CopyReadmeToBuildFolder();

            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = RequiredScenePaths,
                locationPathName = ExecutablePath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            };

            Debug.Log(
                "Final Round P34: starting Windows playtest build.\n" +
                $"- Output: {ExecutablePath}\n" +
                $"- Scenes: {string.Join(", ", RequiredScenePaths)}");

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    "Final Round P34: Windows playtest build failed.\n" +
                    $"- Result: {summary.result}\n" +
                    $"- Errors: {summary.totalErrors}\n" +
                    $"- Warnings: {summary.totalWarnings}\n" +
                    $"- Output: {summary.outputPath}");
            }

            Debug.Log(
                "Final Round P34: Windows playtest build complete.\n" +
                $"- Output folder: {BuildFolder}\n" +
                $"- Executable: {ExecutablePath}\n" +
                $"- Size: {FormatBytes(summary.totalSize)}\n" +
                $"- Warnings: {summary.totalWarnings}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Final Round P34: Windows playtest build failed.\n{exception}");
            throw;
        }
    }

    [MenuItem("Final Round/Package Latest Playtest Build")]
    public static void PackageLatestPlaytestBuild()
    {
        try
        {
            if (!Directory.Exists(BuildFolder) || !File.Exists(ExecutablePath))
            {
                throw new FileNotFoundException(
                    "Final Round P34: no completed playtest build was found. Run Final Round > Build Playtest Windows first.",
                    ExecutablePath);
            }

            Directory.CreateDirectory(OutputRoot);
            CopyReadmeToBuildFolder();

            if (File.Exists(PackagePath))
            {
                File.Delete(PackagePath);
            }

            ZipFile.CreateFromDirectory(BuildFolder, PackagePath, System.IO.Compression.CompressionLevel.Optimal, false);

            Debug.Log(
                "Final Round P34: playtest package complete.\n" +
                $"- Source folder: {BuildFolder}\n" +
                $"- Zip: {PackagePath}\n" +
                $"- README included: {BuildFolder}/{ReadmeBuildFileName}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Final Round P34: playtest packaging failed.\n{exception}");
            throw;
        }
    }

    [MenuItem("Final Round/Validate Playtest Build Settings")]
    public static void ValidatePlaytestBuildSettings()
    {
        ValidateRequiredSceneAssets();
        EnsureRequiredScenesInBuildSettings();

        Debug.Log(
            "Final Round P34: playtest Build Settings validated.\n" +
            $"- Required scenes: {string.Join(", ", RequiredScenePaths)}\n" +
            $"- Output folder: {BuildFolder}");
    }

    private static void ValidateRequiredSceneAssets()
    {
        List<string> missingScenes = RequiredScenePaths
            .Where(scenePath => AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath) == null)
            .ToList();

        if (missingScenes.Count > 0)
        {
            throw new BuildFailedException(
                "Final Round P34: required scene assets are missing.\n" +
                string.Join("\n", missingScenes.Select(scenePath => $"- {scenePath}")));
        }
    }

    private static void EnsureRequiredScenesInBuildSettings()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        bool changed = false;

        foreach (string scenePath in RequiredScenePaths)
        {
            int existingIndex = scenes.FindIndex(scene => scene.path == scenePath);
            EditorBuildSettingsScene verifiedScene = new EditorBuildSettingsScene(scenePath, true);

            if (existingIndex >= 0)
            {
                if (!scenes[existingIndex].enabled)
                {
                    scenes[existingIndex] = verifiedScene;
                    changed = true;
                }

                continue;
            }

            scenes.Add(verifiedScene);
            changed = true;
        }

        if (changed)
        {
            EditorBuildSettings.scenes = scenes.ToArray();
            Debug.Log("Final Round P34: Build Settings repaired with required playtest scenes.");
        }
    }

    private static void CopyReadmeToBuildFolder()
    {
        if (!File.Exists(ReadmeSourcePath))
        {
            throw new FileNotFoundException(
                "Final Round P34: playtest README is missing. Create it before building or packaging.",
                ReadmeSourcePath);
        }

        Directory.CreateDirectory(BuildFolder);
        File.Copy(ReadmeSourcePath, Path.Combine(BuildFolder, ReadmeBuildFileName), true);
    }

    private static string FormatBytes(ulong bytes)
    {
        const double OneMegabyte = 1024d * 1024d;
        return $"{bytes / OneMegabyte:0.0} MB";
    }
}
