using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[InitializeOnLoad]
public static class FinalRoundPlayModeStartScene
{
    private const string InterviewRoomScenePath = "Assets/Scenes/InterviewRoom.unity";

    static FinalRoundPlayModeStartScene()
    {
        EditorApplication.delayCall += EnsureInterviewRoomPlayModeStartScene;
    }

    [MenuItem("Final Round/Set Play Mode Start Scene")]
    public static void EnsureInterviewRoomPlayModeStartScene()
    {
        SceneAsset startScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(InterviewRoomScenePath);
        if (startScene == null)
        {
            Debug.LogWarning(
                "Final Round: could not set Play Mode start scene because InterviewRoom is missing.\n" +
                $"- Missing: {InterviewRoomScenePath}");
            return;
        }

        if (EditorSceneManager.playModeStartScene == startScene)
        {
            return;
        }

        EditorSceneManager.playModeStartScene = startScene;
        Debug.Log($"Final Round: Play Mode start scene set to {InterviewRoomScenePath}.");
    }
}
