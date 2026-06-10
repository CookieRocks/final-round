using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FinalRoundDeskSceneCreator
{
    private const string DeskScenePath = "Assets/Scenes/DeskScene.unity";
    private const string InterviewRoomScenePath = "Assets/Scenes/InterviewRoom.unity";
    private const string RootObjectName = "DeskSceneRoot";
    private const string ControllerObjectName = "DeskPrototypeController";

    [MenuItem("Final Round/Create/Repair Desk Scene")]
    public static void CreateOrRepairDeskScene()
    {
        try
        {
            Scene scene = OpenOrCreateDeskScene();
            GameObject root = FindOrCreateRoot();
            DeskPrototypeController controller = FindOrCreateController(root.transform);

            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, DeskScenePath))
            {
                Debug.LogError($"Final Round P27: failed to save DeskScene at {DeskScenePath}.");
                return;
            }

            ConfigureController(controller);
            EditorSceneManager.MarkSceneDirty(scene);
            if (!EditorSceneManager.SaveScene(scene, DeskScenePath))
            {
                Debug.LogError($"Final Round P27: failed to save repaired DeskScene at {DeskScenePath}.");
                return;
            }

            AssetDatabase.Refresh();
            AssetDatabase.ImportAsset(DeskScenePath, ImportAssetOptions.ForceUpdate);
            EnsureBuildSettingsScenes();

            Debug.Log(
                "Final Round P27: DeskScene create/repair complete.\n" +
                $"- Scene: {DeskScenePath}\n" +
                $"- Root: {RootObjectName}\n" +
                $"- Controller: {ControllerObjectName}\n" +
                $"- Build Settings verified: {DeskScenePath}, {InterviewRoomScenePath}");
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Final Round P27: DeskScene create/repair failed.\n{exception}");
        }
    }

    private static Scene OpenOrCreateDeskScene()
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DeskScenePath) != null)
        {
            Debug.Log($"Final Round P27: opening existing DeskScene at {DeskScenePath}.");
            return EditorSceneManager.OpenScene(DeskScenePath, OpenSceneMode.Single);
        }

        Debug.Log($"Final Round P27: creating new DeskScene at {DeskScenePath}.");
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
        Undo.RegisterCreatedObjectUndo(root, "Create DeskSceneRoot");
        return root;
    }

    private static DeskPrototypeController FindOrCreateController(Transform root)
    {
        DeskPrototypeController[] controllers = Object.FindObjectsByType<DeskPrototypeController>(FindObjectsInactive.Include);
        DeskPrototypeController controller = controllers.FirstOrDefault(existing => existing != null);

        for (int i = 0; i < controllers.Length; i++)
        {
            if (controllers[i] != null && controllers[i] != controller)
            {
                Debug.LogWarning($"Final Round P27: removing duplicate DeskPrototypeController on {controllers[i].gameObject.name}.");
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
        Undo.RegisterCreatedObjectUndo(controllerObject, "Create DeskPrototypeController");
        controllerObject.transform.SetParent(root, false);
        return controllerObject.AddComponent<DeskPrototypeController>();
    }

    private static void ConfigureController(DeskPrototypeController controller)
    {
        SerializedObject serializedController = new SerializedObject(controller);
        SetBool(serializedController, "generatePrototypeSceneObjects", true);
        SetString(serializedController, "interviewRoomSceneName", "InterviewRoom");
        serializedController.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void EnsureBuildSettingsScenes()
    {
        List<EditorBuildSettingsScene> scenes = EditorBuildSettings.scenes.ToList();
        EnsureBuildScene(scenes, DeskScenePath);
        EnsureBuildScene(scenes, InterviewRoomScenePath);
        EditorBuildSettings.scenes = scenes.ToArray();
    }

    private static void EnsureBuildScene(List<EditorBuildSettingsScene> scenes, string scenePath)
    {
        int existingIndex = scenes.FindIndex(scene => scene.path == scenePath);
        EditorBuildSettingsScene verifiedScene = new EditorBuildSettingsScene(scenePath, true);
        if (existingIndex >= 0)
        {
            scenes[existingIndex] = verifiedScene;
            return;
        }

        scenes.Add(verifiedScene);
    }

    private static void SetBool(SerializedObject serializedObject, string propertyName, bool value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"Final Round P27: could not find bool property {propertyName} on DeskPrototypeController.");
            return;
        }

        property.boolValue = value;
    }

    private static void SetString(SerializedObject serializedObject, string propertyName, string value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"Final Round P27: could not find string property {propertyName} on DeskPrototypeController.");
            return;
        }

        property.stringValue = value;
    }
}
