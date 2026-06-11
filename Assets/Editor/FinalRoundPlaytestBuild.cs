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
    private const string AftermathRoomScenePath = "Assets/Scenes/AftermathRoom.unity";
    private const string OutputRoot = "Builds/Playtest";
    private const string BuildFolder = OutputRoot + "/FinalRound_VS4_Windows";
    private const string ExecutablePath = BuildFolder + "/FinalRound.exe";
    private const string PackagePath = OutputRoot + "/FinalRound_VS4_Playtest.zip";
    private const string ReadmeSourcePath = "Docs/FinalRound_P46_VS4PlaytestGuide.md";
    private const string ReadmeBuildFileName = "README_Playtest.md";

    private static readonly string[] RequiredScenePaths =
    {
        InterviewRoomScenePath,
        DeskScenePath,
        AftermathRoomScenePath
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
                "Final Round P46: starting Windows VS4 playtest build.\n" +
                $"- Output: {ExecutablePath}\n" +
                $"- Scenes: {string.Join(", ", RequiredScenePaths)}");

            BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
            BuildSummary summary = report.summary;

            if (summary.result != BuildResult.Succeeded)
            {
                throw new BuildFailedException(
                    "Final Round P46: Windows VS4 playtest build failed.\n" +
                    $"- Result: {summary.result}\n" +
                    $"- Errors: {summary.totalErrors}\n" +
                    $"- Warnings: {summary.totalWarnings}\n" +
                    $"- Output: {summary.outputPath}");
            }

            Debug.Log(
                "Final Round P46: Windows VS4 playtest build complete.\n" +
                $"- Output folder: {BuildFolder}\n" +
                $"- Executable: {ExecutablePath}\n" +
                $"- Size: {FormatBytes(summary.totalSize)}\n" +
                $"- Warnings: {summary.totalWarnings}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Final Round P46: Windows VS4 playtest build failed.\n{exception}");
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
                    "Final Round P46: no completed VS4 playtest build was found. Run Final Round > Build Playtest Windows first.",
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
                "Final Round P46: VS4 playtest package complete.\n" +
                $"- Source folder: {BuildFolder}\n" +
                $"- Zip: {PackagePath}\n" +
                $"- README included: {BuildFolder}/{ReadmeBuildFileName}");
        }
        catch (Exception exception)
        {
            Debug.LogError($"Final Round P46: VS4 playtest packaging failed.\n{exception}");
            throw;
        }
    }

    [MenuItem("Final Round/Validate Playtest Build Settings")]
    public static void ValidatePlaytestBuildSettings()
    {
        ValidateRequiredSceneAssets();
        EnsureRequiredScenesInBuildSettings();

        Debug.Log(
            "Final Round P46: VS4 playtest Build Settings validated.\n" +
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
                "Final Round P46: required scene assets are missing.\n" +
                string.Join("\n", missingScenes.Select(scenePath => $"- {scenePath}")));
        }
    }

    private static void EnsureRequiredScenesInBuildSettings()
    {
        EditorBuildSettingsScene[] existingScenes = EditorBuildSettings.scenes;
        List<EditorBuildSettingsScene> orderedScenes = RequiredScenePaths
            .Select(scenePath => new EditorBuildSettingsScene(scenePath, true))
            .ToList();

        orderedScenes.AddRange(existingScenes.Where(scene => !RequiredScenePaths.Contains(scene.path)));

        if (!BuildSettingsMatch(existingScenes, orderedScenes))
        {
            EditorBuildSettings.scenes = orderedScenes.ToArray();
            Debug.Log("Final Round P46: Build Settings repaired with required playtest scenes.");
        }
    }

    private static bool BuildSettingsMatch(IReadOnlyList<EditorBuildSettingsScene> existingScenes, IReadOnlyList<EditorBuildSettingsScene> orderedScenes)
    {
        if (existingScenes.Count != orderedScenes.Count)
        {
            return false;
        }

        for (int i = 0; i < existingScenes.Count; i++)
        {
            if (existingScenes[i].path != orderedScenes[i].path || existingScenes[i].enabled != orderedScenes[i].enabled)
            {
                return false;
            }
        }

        return true;
    }

    private static void CopyReadmeToBuildFolder()
    {
        if (!File.Exists(ReadmeSourcePath))
        {
            throw new FileNotFoundException(
                "Final Round P46: VS4 playtest guide is missing. Create it before building or packaging.",
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
