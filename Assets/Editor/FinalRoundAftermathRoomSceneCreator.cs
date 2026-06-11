using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FinalRoundAftermathRoomSceneCreator
{
    private const string AftermathRoomScenePath = "Assets/Scenes/AftermathRoom.unity";
    private const string InterviewRoomScenePath = "Assets/Scenes/InterviewRoom.unity";
    private const string DeskScenePath = "Assets/Scenes/DeskScene.unity";
    private const string RootObjectName = "AftermathRoomRoot";
    private const string ControllerObjectName = "AftermathRoomController";
    private static readonly string[] RequiredScenePaths =
    {
        InterviewRoomScenePath,
        DeskScenePath,
        AftermathRoomScenePath
    };

    [MenuItem("Final Round/Create/Repair Aftermath Room Scene")]
    public static void CreateOrRepairAftermathRoomScene()
    {
        try
        {
            Scene scene = OpenOrCreateAftermathScene();
            GameObject root = FindOrCreateRoot();
            AftermathRoomController controller = FindOrCreateController(root.transform);
            ConfigureController(controller);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, AftermathRoomScenePath))
            {
                Debug.LogError($"Final Round P36: failed to save AftermathRoom at {AftermathRoomScenePath}.");
                return;
            }

            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(AftermathRoomScenePath, ImportAssetOptions.ForceUpdate);
            EnsureBuildSettingsScenes();
            OpenInterviewRoomForPlayMode();

            Debug.Log(
                "Final Round P36: AftermathRoom create/repair complete.\n" +
                $"- Scene: {AftermathRoomScenePath}\n" +
                $"- Root: {RootObjectName}\n" +
                $"- Controller: {ControllerObjectName}\n" +
                $"- Build Settings verified: {string.Join(", ", RequiredScenePaths)}\n" +
                $"- Active scene restored for Play mode: {InterviewRoomScenePath}");
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Final Round P36: AftermathRoom create/repair failed.\n{exception}");
        }
    }

    private static Scene OpenOrCreateAftermathScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(AftermathRoomScenePath) != null)
        {
            Debug.Log($"Final Round P36: opening existing AftermathRoom at {AftermathRoomScenePath}.");
            return EditorSceneManager.OpenScene(AftermathRoomScenePath, OpenSceneMode.Single);
        }

        Debug.Log($"Final Round P36: creating new AftermathRoom at {AftermathRoomScenePath}.");
        return EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
    }

    private static GameObject FindOrCreateRoot()
    {
        GameObject root = GameObject.Find(RootObjectName);
        if (root != null)
        {
            return root;
        }

        root = new GameObject(RootObjectName);
        Undo.RegisterCreatedObjectUndo(root, "Create AftermathRoomRoot");
        return root;
    }

    private static AftermathRoomController FindOrCreateController(Transform root)
    {
        AftermathRoomController[] controllers = Object.FindObjectsByType<AftermathRoomController>(FindObjectsInactive.Include);
        AftermathRoomController controller = controllers.FirstOrDefault(existing => existing != null);

        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] != null && controllers[i] != controller)
            {
                Debug.LogWarning($"Final Round P36: removing duplicate AftermathRoomController on {controllers[i].gameObject.name}.");
                Object.DestroyImmediate(controllers[i].gameObject);
            }
        }

        if (controller != null)
        {
            controller.gameObject.name = ControllerObjectName;
            controller.transform.SetParent(root, false);
            return controller;
        }

        GameObject controllerObject = new GameObject(ControllerObjectName);
        Undo.RegisterCreatedObjectUndo(controllerObject, "Create AftermathRoomController");
        controllerObject.transform.SetParent(root, false);
        return controllerObject.AddComponent<AftermathRoomController>();
    }

    private static void ConfigureController(AftermathRoomController controller)
    {
        SerializedObject serializedController = new SerializedObject(controller);
        SetBool(serializedController, "generateSceneShell", true);
        serializedController.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void EnsureBuildSettingsScenes()
    {
        List<EditorBuildSettingsScene> existingScenes = EditorBuildSettings.scenes.ToList();
        List<EditorBuildSettingsScene> orderedScenes = RequiredScenePaths
            .Select(scenePath => new EditorBuildSettingsScene(scenePath, true))
            .ToList();

        orderedScenes.AddRange(existingScenes.Where(scene => !RequiredScenePaths.Contains(scene.path)));
        EditorBuildSettings.scenes = orderedScenes.ToArray();
    }

    private static void OpenInterviewRoomForPlayMode()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(InterviewRoomScenePath) == null)
        {
            Debug.LogWarning(
                "Final Round P36: could not restore InterviewRoom after AftermathRoom repair because the scene asset is missing.\n" +
                $"- Missing: {InterviewRoomScenePath}");
            return;
        }

        EditorSceneManager.OpenScene(InterviewRoomScenePath, OpenSceneMode.Single);
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"Final Round P36: could not find bool property {propertyName} on AftermathRoomController.");
            return;
        }

        property.boolValue = value;
    }
}
