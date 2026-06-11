using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FinalRoundVs4JobListingCreator
{
    private const string DeskScenePath = "Assets/Scenes/DeskScene.unity";
    private const string ContentFolderPath = "Assets/Resources/FinalRound/VS4/JobListings";
    private const string NorthbridgeAssetPath = ContentFolderPath + "/northbridge-security-presales.asset";
    private const string HeliosAssetPath = ContentFolderPath + "/helios-cloud-security-consultant.asset";
    private const string RedgateAssetPath = ContentFolderPath + "/redgate-risk-compliance-presales.asset";
    private const string RootObjectName = "DeskSceneRoot";
    private const string ControllerObjectName = "DeskPrototypeController";

    [MenuItem("Final Round/Create/Repair VS4 Job Listings")]
    public static void CreateOrRepairVs4JobListings()
    {
        try
        {
            EnsureContentFolders();
            JobListingData northbridge = CreateOrUpdateNorthbridge();
            JobListingData helios = CreateOrUpdateHelios();
            JobListingData redgate = CreateOrUpdateRedgate();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            JobListingData[] jobs =
            {
                AssetDatabase.LoadAssetAtPath<JobListingData>(NorthbridgeAssetPath),
                AssetDatabase.LoadAssetAtPath<JobListingData>(HeliosAssetPath),
                AssetDatabase.LoadAssetAtPath<JobListingData>(RedgateAssetPath)
            };

            ValidateJobs(jobs);
            AssignJobsToDeskScene(jobs[0], jobs);

            Debug.Log(
                "Final Round P42: VS4 job listings create/repair complete.\n" +
                $"- Northbridge: {NorthbridgeAssetPath}\n" +
                $"- Helios: {HeliosAssetPath}\n" +
                $"- Redgate: {RedgateAssetPath}\n" +
                "- DeskScene assignment repaired if DeskScene exists.");
        }
        catch (System.Exception exception)
        {
            Debug.LogError($"Final Round P42: VS4 job listing create/repair failed.\n{exception}");
        }
    }

    private static void EnsureContentFolders()
    {
        EnsureFolder("Assets", "Resources");
        EnsureFolder("Assets/Resources", "FinalRound");
        EnsureFolder("Assets/Resources/FinalRound", "VS4");
        EnsureFolder("Assets/Resources/FinalRound/VS4", "JobListings");
    }

    private static void EnsureFolder(string parent, string folder)
    {
        string path = $"{parent}/{folder}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, folder);
        }
    }

    private static JobListingData LoadOrCreate(string assetPath)
    {
        JobListingData job = AssetDatabase.LoadAssetAtPath<JobListingData>(assetPath);
        if (job != null)
        {
            return job;
        }

        if (AssetDatabase.AssetPathExists(assetPath))
        {
            Debug.LogWarning($"Final Round P42: deleting invalid generated job listing asset before repair: {assetPath}");
            AssetDatabase.DeleteAsset(assetPath);
        }

        job = ScriptableObject.CreateInstance<JobListingData>();
        AssetDatabase.CreateAsset(job, assetPath);
        return job;
    }

    private static JobListingData CreateOrUpdateNorthbridge()
    {
        JobListingData job = LoadOrCreate(NorthbridgeAssetPath);
        job.jobId = "northbridge-security-presales";
        job.companyName = "Northbridge Cyber Systems";
        job.roleTitle = "Senior Solutions Engineer - Security Presales";
        job.jobCardSummary = "Balanced enterprise security presales role with familiar customer, technical, and commercial pressure.";
        job.difficultyProfile = "Balanced baseline";
        job.recruiterName = "Maya Patel";
        job.recruiterTitle = "Senior Talent Partner";
        job.recruiterCompany = "Northbridge Cyber Systems";
        job.summary = "Northbridge Cyber Systems is hiring a customer-facing security presales engineer to guide enterprise buyers through architecture, proof-of-value workshops, and executive risk conversations. The role is senior and visible, with enough ambiguity around workload and process maturity to require judgement.";
        job.responsibilities = new[]
        {
            "Lead discovery and security architecture conversations with SOC, risk, and platform teams.",
            "Run proof-of-value workshops that connect detection and response outcomes to executive priorities.",
            "Translate product capabilities into credible customer stories without overpromising coverage.",
            "Partner with sales on account strategy, technical qualification, and late-stage stakeholder alignment."
        };
        job.requirements = new[]
        {
            "Experience in cybersecurity, detection and response, cloud security, or adjacent technical presales.",
            "Ability to explain architecture and risk trade-offs to practitioners and executives.",
            "Comfort handling ambiguous customer requirements and competitive vendor claims.",
            "Strong communication discipline under pressure."
        };
        job.niceToHaves = new[]
        {
            "SOC tooling or incident response background.",
            "Experience building demo narratives for skeptical enterprise buyers.",
            "Familiarity with data residency, compliance, and executive security reporting."
        };
        job.salaryRange = "Competitive base with variable compensation discussed later in process.";
        job.processNotes = "Recruiter screen, technical/presales panel, final customer-scenario round. Timeline is described as fast if the team aligns.";
        job.redFlags = new[]
        {
            "Fast-paced is not clearly defined around travel, after-hours workshops, or escalation load.",
            "Compensation and success measures are described broadly rather than concretely.",
            "The role sits between Sales, Product, and Security with some ownership ambiguity."
        };
        job.greenFlags = new[]
        {
            "The work is close to real customer problems rather than generic demo theatre.",
            "The role values judgement, communication, and technical credibility together.",
            "The current Room content maps cleanly to the role's expected pressure."
        };
        SetDefaultDeltas(job, 0, 0, 0, 0, 0, 0, 0);
        job.defaultRoomProfileId = "InterviewRoom-Balanced";
        job.roomContextLine = "Northbridge frames the panel as a balanced enterprise security presales final round.";
        job.outcomeContextLine = "The final decision reflects both the Desk process and the Room panel signal for the Northbridge role.";
        job.processSummaryNote = "Northbridge was the balanced baseline opportunity.";
        EditorUtility.SetDirty(job);
        return job;
    }

    private static JobListingData CreateOrUpdateHelios()
    {
        JobListingData job = LoadOrCreate(HeliosAssetPath);
        job.jobId = "helios-cloud-security-consultant";
        job.companyName = "Helios Cloud Platform";
        job.roleTitle = "Cloud Security Solutions Consultant";
        job.jobCardSummary = "Architecture-heavy cloud security stretch role with stronger upside and sharper proof pressure.";
        job.difficultyProfile = "Technical stretch / architecture-heavy";
        job.recruiterName = "Iris Chen";
        job.recruiterTitle = "Cloud Security Talent Partner";
        job.recruiterCompany = "Helios Cloud Platform";
        job.summary = "Helios Cloud Platform is hiring a cloud security consultant to help enterprise customers reason about identity, exposure, platform controls, and executive risk. The opportunity looks attractive and ambitious, but the listing expects enough architecture depth that over-positioning would be easy to regret later.";
        job.responsibilities = new[]
        {
            "Lead cloud security discovery across identity, network exposure, data protection, and platform governance.",
            "Shape architecture workshops for security, platform, and executive stakeholders.",
            "Explain trade-offs between customer risk, implementation effort, and platform capability.",
            "Support late-stage technical validation without promising custom outcomes the product cannot support."
        };
        job.requirements = new[]
        {
            "Strong understanding of cloud security architecture and enterprise platform constraints.",
            "Experience translating technical risk into buying and implementation decisions.",
            "Comfort defending technical depth under customer and internal scrutiny.",
            "Ability to keep claims precise when the role sounds like a stretch."
        };
        job.niceToHaves = new[]
        {
            "Multi-cloud security or CNAPP experience.",
            "Identity, access, or data exposure assessment background.",
            "Experience with platform engineering or security architecture review boards."
        };
        job.salaryRange = "Upper-band compensation range advertised, with meaningful variable upside if targets are met.";
        job.processNotes = "Recruiter screen, technical architecture panel, final customer-scenario round. The process sounds efficient, but the technical panel is likely to probe depth.";
        job.redFlags = new[]
        {
            "The listing blends consulting, architecture, and presales ownership into one broad role.",
            "Strong compensation may be paired with aggressive expectations.",
            "The phrase hands-on enough is not defined and may invite overclaiming."
        };
        job.greenFlags = new[]
        {
            "The platform scope could build stronger cloud security credibility.",
            "The process is presented clearly and appears well funded.",
            "The role offers a meaningful stretch if the candidate prepares honestly."
        };
        SetDefaultDeltas(job, 0, 0, 1, -1, 1, -1, 0);
        job.defaultRoomProfileId = "InterviewRoom-TechnicalStretch";
        job.roomContextLine = "Helios gives the panel more reason to test the architecture detail behind earlier claims.";
        job.outcomeContextLine = "The panel weighed the strength of the cloud security positioning against scenario depth.";
        job.processSummaryNote = "Helios was the technical stretch opportunity with higher overclaim risk.";
        EditorUtility.SetDirty(job);
        return job;
    }

    private static JobListingData CreateOrUpdateRedgate()
    {
        JobListingData job = LoadOrCreate(RedgateAssetPath);
        job.jobId = "redgate-risk-compliance-presales";
        job.companyName = "Redgate Financial Risk";
        job.roleTitle = "Risk & Compliance Presales Consultant";
        job.jobCardSummary = "Compliance-heavy presales role with less deep technical demand, more process and stakeholder pressure.";
        job.difficultyProfile = "Commercial / compliance bureaucracy";
        job.recruiterName = "Eleanor Shaw";
        job.recruiterTitle = "Risk Solutions Recruiting Lead";
        job.recruiterCompany = "Redgate Financial Risk";
        job.summary = "Redgate Financial Risk is hiring a presales consultant to support regulated financial customers through risk, compliance, procurement, and legal review. The technical bar is less architecture-heavy than Helios, but the process language suggests careful stakeholder management and slow-moving committees.";
        job.responsibilities = new[]
        {
            "Support discovery with risk, compliance, procurement, and security stakeholders.",
            "Translate product capability into careful regulatory and business-value language.",
            "Prepare customer-facing material for committees that need defensible claims.",
            "Coordinate with sales and legal teams when customer expectations become ambiguous."
        };
        job.requirements = new[]
        {
            "Experience with compliance, risk, governance, or regulated customer environments.",
            "Commercial judgement when technical claims may have legal or audit implications.",
            "Patience with multi-stakeholder evaluation and formal buying processes.",
            "Ability to communicate clearly without overstating control coverage."
        };
        job.niceToHaves = new[]
        {
            "Financial services customer experience.",
            "Familiarity with audit evidence, policy mapping, or third-party risk workflows.",
            "Comfort presenting to mixed legal, risk, security, and executive audiences."
        };
        job.salaryRange = "Stable compensation range with bonus potential described as tied to regional performance.";
        job.processNotes = "Recruiter screen, stakeholder-fit discussion, final customer-scenario round. The process may move slowly because legal and risk teams are involved.";
        job.redFlags = new[]
        {
            "Bureaucracy and legal review may slow decisions after strong conversations.",
            "The listing uses careful language around customer commitments and control claims.",
            "Success may depend on navigating committees more than solving technical problems."
        };
        job.greenFlags = new[]
        {
            "The role rewards precise communication and commercial patience.",
            "Deep cloud architecture is less central than credibility with regulated stakeholders.",
            "The company appears to understand compliance buyer complexity."
        };
        SetDefaultDeltas(job, 1, 0, 0, -1, 0, -1, 1);
        job.defaultRoomProfileId = "InterviewRoom-ComplianceCommercial";
        job.roomContextLine = "Redgate shifts the pressure toward careful claims, stakeholder patience, and compliance ambiguity.";
        job.outcomeContextLine = "The panel weighed commercial care, compliance judgement, and patience with process drag.";
        job.processSummaryNote = "Redgate was the commercial/compliance opportunity with heavier process pressure.";
        EditorUtility.SetDirty(job);
        return job;
    }

    private static void SetDefaultDeltas(
        JobListingData job,
        int roleFit,
        int recruiterTrust,
        int candidateConfidence,
        int energy,
        int overclaimRisk,
        int technicalReadiness,
        int rapportMomentum)
    {
        job.defaultRoleFitDelta = roleFit;
        job.defaultRecruiterTrustDelta = recruiterTrust;
        job.defaultCandidateConfidenceDelta = candidateConfidence;
        job.defaultEnergyDelta = energy;
        job.defaultOverclaimRiskDelta = overclaimRisk;
        job.defaultTechnicalReadinessDelta = technicalReadiness;
        job.defaultRapportMomentumDelta = rapportMomentum;
    }

    private static void ValidateJobs(JobListingData[] jobs)
    {
        string[] ids = jobs.Where(job => job != null).Select(job => job.jobId).ToArray();
        if (jobs.Any(job => job == null) || ids.Length != 3 || ids.Distinct().Count() != ids.Length)
        {
            Debug.LogError("Final Round P42: VS4 job listing validation failed. Expected three non-null jobs with unique job IDs.");
            return;
        }

        for (int i = 0; i < jobs.Length; i++)
        {
            JobListingData job = jobs[i];
            if (string.IsNullOrWhiteSpace(job.companyName)
                || string.IsNullOrWhiteSpace(job.roleTitle)
                || string.IsNullOrWhiteSpace(job.jobCardSummary)
                || string.IsNullOrWhiteSpace(job.difficultyProfile)
                || string.IsNullOrWhiteSpace(job.summary)
                || job.greenFlags == null
                || job.greenFlags.Length == 0
                || job.redFlags == null
                || job.redFlags.Length == 0)
            {
                Debug.LogWarning($"Final Round P42: job listing '{job.jobId}' has missing display fields.");
            }
        }
    }

    private static void AssignJobsToDeskScene(JobListingData defaultJob, JobListingData[] jobs)
    {
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(DeskScenePath) == null)
        {
            Debug.LogWarning("Final Round P42: DeskScene does not exist yet. Run Final Round > Create/Repair Desk Scene before assignment.");
            return;
        }

        Scene scene = EditorSceneManager.OpenScene(DeskScenePath, OpenSceneMode.Single);
        GameObject root = FindOrCreateRoot();
        DeskPrototypeController controller = FindOrCreateController(root.transform);
        SerializedObject serializedController = new SerializedObject(controller);
        SetObject(serializedController, "defaultJobListing", defaultJob);
        SetObjectArray(serializedController, "availableJobListings", jobs);
        serializedController.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(controller);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene, DeskScenePath);
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
                Debug.LogWarning($"Final Round P42: removing duplicate DeskPrototypeController on {controllers[i].gameObject.name}.");
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

    private static void SetObject(SerializedObject serializedObject, string propertyName, Object value)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null)
        {
            Debug.LogWarning($"Final Round P42: could not find object property {propertyName} on DeskPrototypeController.");
            return;
        }

        property.objectReferenceValue = value;
    }

    private static void SetObjectArray(SerializedObject serializedObject, string propertyName, Object[] values)
    {
        SerializedProperty property = serializedObject.FindProperty(propertyName);
        if (property == null || !property.isArray)
        {
            Debug.LogWarning($"Final Round P42: could not find array property {propertyName} on DeskPrototypeController.");
            return;
        }

        property.arraySize = values.Length;
        for (int i = 0; i < values.Length; i++)
        {
            property.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
