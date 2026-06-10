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
    private const string ContentFolderPath = "Assets/Resources/FinalRound/VS2/P28";
    private const string JobListingAssetPath = ContentFolderPath + "/NCS-SE-001_JobListing.asset";
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

    [MenuItem("Final Round/Create/Repair P28 Desk Content Assets")]
    public static void CreateOrRepairP28DeskContentAssets()
    {
        try
        {
            EnsureContentFolders();
            JobListingData jobListing = CreateOrUpdateJobListing();
            ApplicationChoiceData[] choices = CreateOrUpdateApplicationChoices();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            jobListing = AssetDatabase.LoadAssetAtPath<JobListingData>(JobListingAssetPath);
            choices = LoadApplicationChoiceAssets();
            AssignContentToDeskScene(jobListing, choices);
            Debug.Log(
                "Final Round P28: Desk content assets create/repair complete.\n" +
                $"- Job listing: {JobListingAssetPath}\n" +
                $"- Application choices: {choices.Length}\n" +
                "- DeskScene assignment repaired if DeskScene exists.");
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Final Round P28: Desk content asset create/repair failed.\n{exception}");
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
        AssignContentIfAvailable(serializedController);
        serializedController.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
    }

    private static void AssignContentToDeskScene(JobListingData jobListing, ApplicationChoiceData[] choices)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DeskScenePath) == null)
        {
            Debug.LogWarning("Final Round P28: DeskScene does not exist yet. Run Final Round > Create/Repair Desk Scene after creating content assets.");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(DeskScenePath, OpenSceneMode.Single);
        GameObject root = FindOrCreateRoot();
        DeskPrototypeController controller = FindOrCreateController(root.transform);
        SerializedObject serializedController = new SerializedObject(controller);
        SetObject(serializedController, "defaultJobListing", jobListing);
        SetObjectArray(serializedController, "authoredApplicationChoices", choices);
        SetBool(serializedController, "generatePrototypeSceneObjects", true);
        SetString(serializedController, "interviewRoomSceneName", "InterviewRoom");
        serializedController.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, DeskScenePath);
    }

    private static void AssignContentIfAvailable(SerializedObject serializedController)
    {
        JobListingData jobListing = AssetDatabase.LoadAssetAtPath<JobListingData>(JobListingAssetPath);
        ApplicationChoiceData[] choices = LoadApplicationChoiceAssets();
        if (jobListing != null)
        {
            SetObject(serializedController, "defaultJobListing", jobListing);
        }

        if (choices.Length > 0)
        {
            SetObjectArray(serializedController, "authoredApplicationChoices", choices);
        }
    }

    private static void EnsureContentFolders()
    {
        EnsureFolder("Assets", "Resources");
        EnsureFolder("Assets/Resources", "FinalRound");
        EnsureFolder("Assets/Resources/FinalRound", "VS2");
        EnsureFolder("Assets/Resources/FinalRound/VS2", "P28");
    }

    private static void EnsureFolder(string parent, string folder)
    {
        string path = $"{parent}/{folder}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static JobListingData CreateOrUpdateJobListing()
    {
        JobListingData jobListing = AssetDatabase.LoadAssetAtPath<JobListingData>(JobListingAssetPath);
        if (jobListing == null)
        {
            jobListing = ScriptableObject.CreateInstance<JobListingData>();
            AssetDatabase.CreateAsset(jobListing, JobListingAssetPath);
        }

        jobListing.jobId = "NCS-SE-001";
        jobListing.companyName = "Northbridge Cyber Systems";
        jobListing.roleTitle = "Senior Solutions Engineer - Security Presales";
        jobListing.summary = "Northbridge Cyber Systems is hiring a customer-facing security presales engineer to guide enterprise buyers through architecture, proof-of-value workshops, and executive risk conversations. The role sounds senior, visible, and useful, but the listing leaves some room for interpretation around workload, travel, and how mature the process really is.";
        jobListing.responsibilities = new[]
        {
            "Lead discovery and security architecture conversations with SOC, risk, and platform teams.",
            "Run proof-of-value workshops that connect detection and response outcomes to executive priorities.",
            "Translate product capabilities into credible customer stories without overpromising coverage.",
            "Partner with sales on account strategy, technical qualification, and late-stage stakeholder alignment."
        };
        jobListing.requirements = new[]
        {
            "Experience in cybersecurity, detection/response, cloud security, or adjacent technical presales.",
            "Able to explain architecture and risk trade-offs to both practitioners and executives.",
            "Comfortable handling ambiguous customer requirements and competitive vendor claims.",
            "Strong communication discipline under pressure."
        };
        jobListing.niceToHaves = new[]
        {
            "SOC tooling or incident response background.",
            "Experience building demo narratives for skeptical enterprise buyers.",
            "Familiarity with data residency, compliance, and executive security reporting."
        };
        jobListing.salaryRange = "Base salary listed as competitive, with variable compensation discussed later in process.";
        jobListing.processNotes = "Recruiter screen, technical/presales panel, final customer-scenario round. Timeline described as fast if the team aligns.";
        jobListing.redFlags = new[]
        {
            "The listing says fast-paced without clarifying travel, after-hours workshops, or escalation load.",
            "Compensation and success measures are described broadly rather than concretely.",
            "The role appears to sit between Sales, Product, and Security with unclear ownership boundaries."
        };
        jobListing.greenFlags = new[]
        {
            "The work is close to real customer problems rather than generic demo theatre.",
            "The role values judgement, communication, and technical credibility together.",
            "There is room to shape how security outcomes are explained to executives."
        };
        jobListing.defaultRoomProfileId = "InterviewRoom";
        EditorUtility.SetDirty(jobListing);
        return jobListing;
    }

    private static ApplicationChoiceData[] CreateOrUpdateApplicationChoices()
    {
        DeskPrototypeController.ApplicationStrategyChoice[] sourceChoices = DeskPrototypeController.BuildFallbackApplicationChoices();
        ApplicationChoiceData[] assets = new ApplicationChoiceData[sourceChoices.Length];
        for (int i = 0; i < sourceChoices.Length; i++)
        {
            DeskPrototypeController.ApplicationStrategyChoice source = sourceChoices[i];
            string path = $"{ContentFolderPath}/{source.choiceId}.asset";
            ApplicationChoiceData asset = AssetDatabase.LoadAssetAtPath<ApplicationChoiceData>(path);
            if (asset == null)
            {
                if (AssetDatabase.AssetPathExists(path))
                {
                    Debug.LogWarning($"Final Round P28: deleting invalid generated application choice asset before repair: {path}");
                    AssetDatabase.DeleteAsset(path);
                }

                asset = ScriptableObject.CreateInstance<ApplicationChoiceData>();
                AssetDatabase.CreateAsset(asset, path);
            }

            asset.choiceId = source.choiceId;
            asset.label = source.label;
            asset.bodyText = source.bodyText;
            asset.feedbackText = source.feedbackText;
            asset.roleFitDelta = source.roleFitDelta;
            asset.recruiterTrustDelta = source.recruiterTrustDelta;
            asset.candidateConfidenceDelta = source.candidateConfidenceDelta;
            asset.energyDelta = source.energyDelta;
            asset.overclaimRiskDelta = source.overclaimRiskDelta;
            asset.technicalReadinessDelta = source.technicalReadinessDelta;
            asset.rapportMomentumDelta = source.rapportMomentumDelta;
            EditorUtility.SetDirty(asset);
            assets[i] = asset;
        }

        return assets;
    }

    private static ApplicationChoiceData[] LoadApplicationChoiceAssets()
    {
        string[] guids = AssetDatabase.FindAssets("t:ApplicationChoiceData", new[] { ContentFolderPath });
        return guids
            .Select(AssetDatabase.GUIDToAssetPath)
            .Select(AssetDatabase.LoadAssetAtPath<ApplicationChoiceData>)
            .Where(asset => asset != null)
            .OrderBy(GetApplicationChoiceSortOrder)
            .ThenBy(asset => asset.choiceId)
            .ToArray();
    }

    private static int GetApplicationChoiceSortOrder(ApplicationChoiceData asset)
    {
        return asset.choiceId switch
        {
            "APP-HONEST-FIT" => 0,
            "APP-TAILORED-CREDIBLE" => 1,
            "APP-AGGRESSIVE-POSITIONING" => 2,
            "APP-QUICK-APPLY" => 3,
            _ => 100
        };
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

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"Final Round P28: could not find object property {propertyName} on DeskPrototypeController.");
            return;
        }

        property.objectReferenceValue = value;
    }

    private static void SetObjectArray(SerializedObject serializedObject, string propertyName, Object[] values)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || !property.isArray)
        {
            Debug.LogWarning($"Final Round P28: could not find array property {propertyName} on DeskPrototypeController.");
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
