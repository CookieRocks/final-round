using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public sealed class DeskPrototypeController : MonoBehaviour
{
    [System.Serializable]
    public sealed class ApplicationStrategyChoice
    {
        public string choiceId;
        public string label;
        [TextArea(3, 8)] public string bodyText;
        [TextArea(2, 6)] public string feedbackText;
        [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int roleFitDelta;
        [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int recruiterTrustDelta;
        [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int candidateConfidenceDelta;
        [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int energyDelta;
        [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int overclaimRiskDelta;
        [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int technicalReadinessDelta;
        [Range(CandidateState.MinPrototypeModifier, CandidateState.MaxPrototypeModifier)] public int rapportMomentumDelta;

        public string BuildDeltaSummary()
        {
            return
                $"Role Fit {FormatDelta(roleFitDelta)}, Recruiter Trust {FormatDelta(recruiterTrustDelta)}, " +
                $"Confidence {FormatDelta(candidateConfidenceDelta)}, Energy {FormatDelta(energyDelta)}, " +
                $"Overclaim Risk {FormatDelta(overclaimRiskDelta)}, Technical Readiness {FormatDelta(technicalReadinessDelta)}, " +
                $"Rapport Momentum {FormatDelta(rapportMomentumDelta)}";
        }

        private static string FormatDelta(int delta)
        {
            return delta >= 0 ? $"+{delta}" : delta.ToString();
        }
    }

    public enum DeskPrototypeState
    {
        Standby,
        LaptopFocus,
        JobListing,
        ApplicationChoice,
        Recruiter,
        Prep,
        TransitioningToRoom
    }

    private const string PlaceholderJobId = "NCS-SE-001";
    private const string PlaceholderCompany = "Northbridge Cyber Systems";
    private const string PlaceholderRole = "Senior Solutions Engineer - Security Presales";
    private const string DefaultSalaryRange = "Base salary listed as competitive, with variable compensation discussed later in process.";
    private const string DefaultProcessNotes = "Recruiter screen, technical/presales panel, final customer-scenario round. Timeline described as fast if the team aligns.";

    [Header("Scene")]
    [SerializeField] private string interviewRoomSceneName = "InterviewRoom";
    [SerializeField] private bool generatePrototypeSceneObjects = true;
    [SerializeField] private Camera deskCamera;
    [SerializeField] private Transform laptopInteractable;

    [Header("Run State")]
    [SerializeField] private DeskPrototypeState currentState = DeskPrototypeState.Standby;
    [SerializeField] private JobListingData defaultJobListing;
    [SerializeField] private ApplicationChoiceData[] authoredApplicationChoices;

    [Header("Input")]
#if !ENABLE_INPUT_SYSTEM
    [SerializeField] private KeyCode openLaptopKey = KeyCode.E;
    [SerializeField] private KeyCode alternateOpenLaptopKey = KeyCode.Space;
#endif

    private Canvas canvas;
    private GameObject laptopPanel;
    private GameObject debugPanel;
    private TMP_Text debugText;
    private TMP_Text modeText;
    private TMP_Text listingSummaryText;
    private TMP_Text feedbackText;
    private Button confirmApplicationButton;
    private Button[] listingSectionButtons;
    private Button[] strategyButtons;
    private ApplicationStrategyChoice[] fallbackApplicationChoices;
    private ApplicationStrategyChoice selectedApplicationChoice;
    private int currentListingSectionIndex;
    private bool applicationConfirmed;

    public DeskPrototypeState CurrentState => currentState;

    private void Start()
    {
        if (generatePrototypeSceneObjects)
        {
            BuildPrototypeSceneShell();
        }

        BuildPrototypeUi();
        RefreshDebugDisplay();
        Debug.Log("Final Round P28: Desk prototype ready. Press E/Space or click the laptop to open the job listing UI.");
    }

    private void Update()
    {
        if (WasOpenLaptopPressed() || WasLaptopClicked())
        {
            OpenLaptopInterface();
        }
    }

    public void BeginDeskRun()
    {
        CandidateState state = FinalRoundRunState.CreateNeutralRun();
        state.SelectedJobId = GetActiveJobId();
        currentState = DeskPrototypeState.LaptopFocus;
        applicationConfirmed = false;
        selectedApplicationChoice = null;
        Debug.Log("Final Round P28: Desk run started.\n" + state.BuildDebugSummary());
        RefreshDebugDisplay();
    }

    public CandidateState CreateNeutralCandidateState()
    {
        CandidateState state = FinalRoundRunState.CreateNeutralRun();
        state.SelectedJobId = GetActiveJobId();
        currentState = DeskPrototypeState.Standby;
        RefreshDebugDisplay();
        return state;
    }

    public void OpenLaptopInterface()
    {
        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }

        currentState = DeskPrototypeState.JobListing;
        if (laptopPanel != null)
        {
            SetLaptopPanelVisible(true);
        }

        ShowListingView();
        RefreshDebugDisplay();
    }

    public void ShowListingView()
    {
        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }

        currentState = DeskPrototypeState.JobListing;
        selectedApplicationChoice = null;

        SetText(modeText, "View Listing");
        SetText(feedbackText, "Review the opportunity, then choose how to position the application.");
        ShowListingSection(currentListingSectionIndex);
        SetListingSectionButtonsVisible(true);
        SetStrategyButtonsVisible(false);
        SetConfirmInteractable(false);
        RefreshDebugDisplay();
    }

    public void ShowApplicationChoices()
    {
        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }

        currentState = DeskPrototypeState.ApplicationChoice;
        selectedApplicationChoice = null;
        if (listingSummaryText != null)
        {
            listingSummaryText.text =
                "Choose an application strategy.\n\n" +
                "This is the first VS2 step that changes CandidateState. The Room will receive the state, but P28 still does not apply Room modifiers.";
        }

        SetText(modeText, "Choose Application Strategy");
        SetText(feedbackText, "No strategy selected.");
        SetListingSectionButtonsVisible(false);
        SetStrategyButtonsVisible(true);
        SetConfirmInteractable(false);
        RefreshDebugDisplay();
    }

    public void ConfirmApplication()
    {
        if (selectedApplicationChoice == null)
        {
            SetText(feedbackText, "Select an application strategy first.");
            return;
        }

        CandidateState state = FinalRoundRunState.HasActiveRun()
            ? FinalRoundRunState.Instance.State
            : FinalRoundRunState.CreateNeutralRun();

        state.SelectedJobId = GetActiveJobId();
        state.ApplicationChoiceId = selectedApplicationChoice.choiceId;
        state.ApplyDeltas(
            selectedApplicationChoice.roleFitDelta,
            selectedApplicationChoice.recruiterTrustDelta,
            selectedApplicationChoice.candidateConfidenceDelta,
            selectedApplicationChoice.energyDelta,
            selectedApplicationChoice.overclaimRiskDelta,
            selectedApplicationChoice.technicalReadinessDelta,
            selectedApplicationChoice.rapportMomentumDelta);

        applicationConfirmed = true;
        currentState = DeskPrototypeState.Prep;
        SetText(modeText, "Application Submitted");
        SetText(
            listingSummaryText,
            "Application submitted.\n\n" +
            selectedApplicationChoice.feedbackText +
            "\n\nPrototype shortcut remains available for testing the Desk-to-Room bridge.");
        SetText(feedbackText, "CandidateState updated.\n" + selectedApplicationChoice.BuildDeltaSummary());
        SetListingSectionButtonsVisible(false);
        SetStrategyButtonsVisible(false);
        SetConfirmInteractable(false);

        Debug.Log("Final Round P28: application strategy confirmed.\n" + state.BuildDebugSummary());
        RefreshDebugDisplay();
    }

    private void SelectApplicationChoice(ApplicationStrategyChoice choice)
    {
        selectedApplicationChoice = choice;
        SetText(
            feedbackText,
            $"{choice.label}\n{choice.bodyText}\n\nCandidateState deltas: {choice.BuildDeltaSummary()}");
        SetConfirmInteractable(true);
        RefreshStrategyButtonLabels();
    }

    public void GoToInterviewRoom()
    {
        CandidateState state = FinalRoundRunState.HasActiveRun()
            ? FinalRoundRunState.Instance.State
            : FinalRoundRunState.CreateNeutralRun();

        state.SelectedJobId = string.IsNullOrWhiteSpace(state.SelectedJobId)
            ? GetActiveJobId()
            : state.SelectedJobId;

        currentState = DeskPrototypeState.TransitioningToRoom;
        Debug.Log(
            $"Final Round P28: Desk-to-Room transition requested. Scene: {interviewRoomSceneName}\n" +
            state.BuildDebugSummary());

        RefreshDebugDisplay();
        SceneManager.LoadScene(interviewRoomSceneName);
    }

    public void ResetDeskRun()
    {
        if (FinalRoundRunState.HasInstance)
        {
            FinalRoundRunState.Instance.ResetRun();
        }

        currentState = DeskPrototypeState.Standby;
        if (laptopPanel != null)
        {
            SetLaptopPanelVisible(false);
        }

        if (listingSummaryText != null)
        {
            listingSummaryText.text = BuildListingSectionText(0);
        }

        applicationConfirmed = false;
        selectedApplicationChoice = null;
        currentListingSectionIndex = 0;
        SetText(modeText, "View Listing");
        SetText(feedbackText, "Desk run reset. Open the laptop to begin again.");
        SetListingSectionButtonsVisible(true);
        SetStrategyButtonsVisible(false);
        SetConfirmInteractable(false);

        Debug.Log("Final Round P28: Desk run reset.");
        RefreshDebugDisplay();
    }

    private void BuildPrototypeSceneShell()
    {
        if (deskCamera == null)
        {
            deskCamera = Camera.main;
        }

        if (deskCamera == null)
        {
            GameObject cameraObject = new GameObject("Desk Camera");
            deskCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
        }

        deskCamera.transform.SetPositionAndRotation(new Vector3(0f, 2.2f, -4.8f), Quaternion.Euler(24f, 0f, 0f));
        deskCamera.clearFlags = CameraClearFlags.SolidColor;
        deskCamera.backgroundColor = new Color32(13, 16, 22, 255);

        if (FindAnyObjectByType<Light>() == null)
        {
            GameObject lightObject = new GameObject("Desk Soft Key Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.color = new Color32(218, 229, 236, 255);
            lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        }

        Material deskMaterial = CreateMaterial("P27 Desk Surface Material", new Color32(55, 50, 43, 255));
        Material laptopMaterial = CreateMaterial("P27 Laptop Material", new Color32(11, 15, 20, 255));
        Material screenMaterial = CreateMaterial("P27 Laptop Screen Material", new Color32(8, 59, 71, 255));
        Material roomMaterial = CreateMaterial("P27 Desk Room Material", new Color32(29, 33, 42, 255));
        Material chairMaterial = CreateMaterial("P27 Desk Chair Material", new Color32(34, 38, 49, 255));

        CreateCube("Desk Room Back Wall", new Vector3(0f, 1.4f, 1.75f), new Vector3(7f, 2.8f, 0.18f), roomMaterial);
        CreateCube("Desk Floor", new Vector3(0f, -0.08f, -1.2f), new Vector3(7f, 0.16f, 6f), roomMaterial);
        CreateCube("Desk Surface", new Vector3(0f, 0.78f, -0.15f), new Vector3(4.4f, 0.18f, 1.75f), deskMaterial);
        CreateCube("Desk Chair Back", new Vector3(0f, 1.03f, -2.05f), new Vector3(1.15f, 1.15f, 0.18f), chairMaterial);
        CreateCube("Desk Chair Seat", new Vector3(0f, 0.47f, -1.95f), new Vector3(1.2f, 0.18f, 1.0f), chairMaterial);
        CreateCube("Desk Notebook Placeholder", new Vector3(-1.42f, 0.91f, -0.38f), new Vector3(0.88f, 0.04f, 0.56f), CreateMaterial("P27 Notebook Material", new Color32(200, 190, 150, 255)));

        Transform laptopBase = CreateCube("Laptop Click Target", new Vector3(0f, 0.93f, -0.08f), new Vector3(1.45f, 0.08f, 0.9f), laptopMaterial);
        Transform laptopScreen = CreateCube("Laptop Screen", new Vector3(0f, 1.35f, 0.32f), new Vector3(1.45f, 0.72f, 0.08f), screenMaterial);
        laptopScreen.rotation = Quaternion.Euler(-10f, 0f, 0f);
        laptopInteractable = laptopBase;
    }

    private void BuildPrototypeUi()
    {
        EnsureEventSystem();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject("Desk Prototype Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        CreateHeader(canvas.transform);
        CreateDebugPanel(canvas.transform);
        CreateLaptopPanel(canvas.transform);
    }

    private void CreateHeader(Transform parent)
    {
        TMP_Text header = CreateText("Desk Header", parent, "FINAL ROUND  /  THE DESK", 28, FontStyles.Bold, TextAlignmentOptions.Left);
        RectTransform rect = header.rectTransform;
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(42f, -34f);
        rect.sizeDelta = new Vector2(700f, 48f);

        TMP_Text prompt = CreateText("Desk Prompt", parent, "Press E / Space or click the laptop", 20, FontStyles.Normal, TextAlignmentOptions.Left);
        RectTransform promptRect = prompt.rectTransform;
        promptRect.anchorMin = new Vector2(0f, 1f);
        promptRect.anchorMax = new Vector2(0f, 1f);
        promptRect.pivot = new Vector2(0f, 1f);
        promptRect.anchoredPosition = new Vector2(42f, -78f);
        promptRect.sizeDelta = new Vector2(700f, 34f);
        prompt.color = new Color32(151, 170, 188, 255);
    }

    private void CreateLaptopPanel(Transform parent)
    {
        laptopPanel = CreatePanel("Laptop Job UI", parent, new Color32(13, 18, 24, 238));
        RectTransform panelRect = laptopPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.anchoredPosition = new Vector2(0f, -16f);
        panelRect.sizeDelta = new Vector2(1240f, 700f);

        VerticalLayoutGroup layout = laptopPanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(34, 34, 28, 28);
        layout.spacing = 14f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        CreateText("Laptop Title", laptopPanel.transform, "Northbridge Jobs", 34, FontStyles.Bold, TextAlignmentOptions.Left);
        CreateText("Laptop Role", laptopPanel.transform, GetRoleLine(), 24, FontStyles.Bold, TextAlignmentOptions.Left);
        CreateText("Laptop Company", laptopPanel.transform, GetCompanyLine(), 20, FontStyles.Normal, TextAlignmentOptions.Left);

        modeText = CreateText("Laptop Mode", laptopPanel.transform, "View Listing", 20, FontStyles.Bold, TextAlignmentOptions.Left);
        modeText.color = new Color32(98, 218, 195, 255);

        GameObject sectionRow = new GameObject("Listing Section Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        sectionRow.transform.SetParent(laptopPanel.transform, false);
        HorizontalLayoutGroup sectionLayout = sectionRow.GetComponent<HorizontalLayoutGroup>();
        sectionLayout.spacing = 8f;
        sectionLayout.childForceExpandWidth = false;
        sectionLayout.childForceExpandHeight = false;
        sectionRow.GetComponent<LayoutElement>().preferredHeight = 42f;

        string[] sectionLabels = GetListingSectionLabels();
        listingSectionButtons = new Button[sectionLabels.Length];
        for (int i = 0; i < sectionLabels.Length; i++)
        {
            int sectionIndex = i;
            listingSectionButtons[i] = CreateButton(sectionLabels[i], sectionRow.transform, () => ShowListingSection(sectionIndex), 116f, 38f, 14);
        }

        listingSummaryText = CreateText("Laptop Summary", laptopPanel.transform, BuildListingSectionText(0), 20, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        listingSummaryText.color = new Color32(209, 217, 224, 255);
        listingSummaryText.rectTransform.sizeDelta = new Vector2(0f, 260f);
        listingSummaryText.GetComponent<LayoutElement>().preferredHeight = 260f;

        feedbackText = CreateText("Application Feedback", laptopPanel.transform, "Review the opportunity, then choose how to position the application.", 17, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        feedbackText.color = new Color32(172, 181, 196, 255);
        feedbackText.GetComponent<LayoutElement>().preferredHeight = 82f;

        GameObject strategyRow = new GameObject("Application Strategy Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        strategyRow.transform.SetParent(laptopPanel.transform, false);
        HorizontalLayoutGroup strategyLayout = strategyRow.GetComponent<HorizontalLayoutGroup>();
        strategyLayout.spacing = 10f;
        strategyLayout.childForceExpandWidth = false;
        strategyLayout.childForceExpandHeight = false;
        strategyRow.GetComponent<LayoutElement>().preferredHeight = 48f;

        ApplicationStrategyChoice[] choices = GetApplicationChoices();
        strategyButtons = new Button[choices.Length];
        for (int i = 0; i < choices.Length; i++)
        {
            ApplicationStrategyChoice choice = choices[i];
            strategyButtons[i] = CreateButton(choice.label, strategyRow.transform, () => SelectApplicationChoice(choice));
        }

        GameObject buttonRow = new GameObject("Laptop Button Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        buttonRow.transform.SetParent(laptopPanel.transform, false);
        HorizontalLayoutGroup rowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 14f;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;
        buttonRow.GetComponent<LayoutElement>().preferredHeight = 52f;

        CreateButton("View Listing", buttonRow.transform, ShowListingView, 150f);
        CreateButton("Choose Application Strategy", buttonRow.transform, ShowApplicationChoices, 230f);
        confirmApplicationButton = CreateButton("Confirm Application", buttonRow.transform, ConfirmApplication, 210f);
        CreateButton("Go To Interview Room (Prototype Shortcut)", buttonRow.transform, GoToInterviewRoom, 270f);
        CreateButton("Reset Desk Run", buttonRow.transform, ResetDeskRun, 185f);

        SetListingSectionButtonsVisible(true);
        SetStrategyButtonsVisible(false);
        SetConfirmInteractable(false);
        SetLaptopPanelVisible(false);
    }

    private void CreateDebugPanel(Transform parent)
    {
        debugPanel = CreatePanel("Desk Debug Readout", parent, new Color32(9, 13, 18, 190));
        RectTransform rect = debugPanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(1f, 0f);
        rect.anchorMax = new Vector2(1f, 0f);
        rect.pivot = new Vector2(1f, 0f);
        rect.anchoredPosition = new Vector2(-32f, 28f);
        rect.sizeDelta = new Vector2(440f, 138f);

        debugText = CreateText("Desk Debug Text", debugPanel.transform, string.Empty, 14, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        RectTransform textRect = debugText.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(18f, 14f);
        textRect.offsetMax = new Vector2(-18f, -14f);
    }

    private void RefreshDebugDisplay()
    {
        if (debugText == null)
        {
            return;
        }

        string summary = FinalRoundRunState.TryGetActiveState(out CandidateState state)
            ? state.BuildDebugSummary()
            : "No active CandidateState.\nDirect Room launch will use neutral VS1 fallback.";

        debugText.text =
            $"P28 Desk Prototype\nState: {currentState}\nTarget Scene: {interviewRoomSceneName}\nApplication Confirmed: {applicationConfirmed}\n\n{summary}";
    }

    private void SetLaptopPanelVisible(bool visible)
    {
        if (laptopPanel != null)
        {
            laptopPanel.SetActive(visible);
        }

        if (debugPanel != null)
        {
            debugPanel.SetActive(!visible);
        }
    }

    private string GetActiveJobId()
    {
        return defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.jobId)
            ? defaultJobListing.jobId
            : PlaceholderJobId;
    }

    private string GetRoleLine()
    {
        string role = defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.roleTitle)
            ? defaultJobListing.roleTitle
            : PlaceholderRole;
        return $"Role: {role}";
    }

    private string GetCompanyLine()
    {
        string company = defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.companyName)
            ? defaultJobListing.companyName
            : PlaceholderCompany;
        return $"Company: {company}";
    }

    private void ShowListingSection(int sectionIndex)
    {
        currentListingSectionIndex = Mathf.Clamp(sectionIndex, 0, GetListingSectionLabels().Length - 1);
        SetText(listingSummaryText, BuildListingSectionText(currentListingSectionIndex));
        RefreshListingSectionButtonLabels();
    }

    private static string[] GetListingSectionLabels()
    {
        return new[]
        {
            "Overview",
            "Duties",
            "Reqs",
            "Nice",
            "Pay",
            "Risks",
            "Signals"
        };
    }

    private string BuildListingSectionText(int sectionIndex)
    {
        return sectionIndex switch
        {
            1 => $"Responsibilities\n{FormatList(GetResponsibilities())}",
            2 => $"Requirements\n{FormatList(GetRequirements())}",
            3 => $"Nice-to-haves\n{FormatList(GetNiceToHaves())}",
            4 => $"Salary / Process\n{GetSalaryRange()}\n\n{GetProcessNotes()}",
            5 => $"Red flags\n{FormatList(GetRedFlags())}",
            6 => $"Green flags\n{FormatList(GetGreenFlags())}",
            _ => $"Overview\n{GetListingSummary()}"
        };
    }

    private string GetListingSummary()
    {
        return defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.summary)
            ? defaultJobListing.summary
            : "Northbridge Cyber Systems is hiring a customer-facing security presales engineer to guide enterprise buyers through architecture, proof-of-value workshops, and executive risk conversations. The role sounds senior, visible, and useful, but the listing leaves some room for interpretation around workload, travel, and how mature the process really is.";
    }

    private string[] GetResponsibilities()
    {
        return defaultJobListing != null && defaultJobListing.responsibilities != null && defaultJobListing.responsibilities.Length > 0
            ? defaultJobListing.responsibilities
            : new[]
            {
                "Lead discovery and security architecture conversations with SOC, risk, and platform teams.",
                "Run proof-of-value workshops that connect detection and response outcomes to executive priorities.",
                "Translate product capabilities into credible customer stories without overpromising coverage.",
                "Partner with sales on account strategy, technical qualification, and late-stage stakeholder alignment."
            };
    }

    private string[] GetRequirements()
    {
        return defaultJobListing != null && defaultJobListing.requirements != null && defaultJobListing.requirements.Length > 0
            ? defaultJobListing.requirements
            : new[]
            {
                "Experience in cybersecurity, detection/response, cloud security, or adjacent technical presales.",
                "Able to explain architecture and risk trade-offs to both practitioners and executives.",
                "Comfortable handling ambiguous customer requirements and competitive vendor claims.",
                "Strong communication discipline under pressure."
            };
    }

    private string[] GetNiceToHaves()
    {
        return defaultJobListing != null && defaultJobListing.niceToHaves != null && defaultJobListing.niceToHaves.Length > 0
            ? defaultJobListing.niceToHaves
            : new[]
            {
                "SOC tooling or incident response background.",
                "Experience building demo narratives for skeptical enterprise buyers.",
                "Familiarity with data residency, compliance, and executive security reporting."
            };
    }

    private string GetSalaryRange()
    {
        return defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.salaryRange)
            ? defaultJobListing.salaryRange
            : DefaultSalaryRange;
    }

    private string GetProcessNotes()
    {
        return defaultJobListing != null && !string.IsNullOrWhiteSpace(defaultJobListing.processNotes)
            ? defaultJobListing.processNotes
            : DefaultProcessNotes;
    }

    private string[] GetRedFlags()
    {
        return defaultJobListing != null && defaultJobListing.redFlags != null && defaultJobListing.redFlags.Length > 0
            ? defaultJobListing.redFlags
            : new[]
            {
                "The listing says fast-paced without clarifying travel, after-hours workshops, or escalation load.",
                "Compensation and success measures are described broadly rather than concretely.",
                "The role appears to sit between Sales, Product, and Security with unclear ownership boundaries."
            };
    }

    private string[] GetGreenFlags()
    {
        return defaultJobListing != null && defaultJobListing.greenFlags != null && defaultJobListing.greenFlags.Length > 0
            ? defaultJobListing.greenFlags
            : new[]
            {
                "The work is close to real customer problems rather than generic demo theatre.",
                "The role values judgement, communication, and technical credibility together.",
                "There is room to shape how security outcomes are explained to executives."
            };
    }

    private ApplicationStrategyChoice[] GetApplicationChoices()
    {
        if (authoredApplicationChoices != null && authoredApplicationChoices.Length > 0)
        {
            int validChoiceCount = 0;
            for (int i = 0; i < authoredApplicationChoices.Length; i++)
            {
                if (authoredApplicationChoices[i] != null)
                {
                    validChoiceCount++;
                }
            }

            if (validChoiceCount == authoredApplicationChoices.Length)
            {
                ApplicationStrategyChoice[] choices = new ApplicationStrategyChoice[authoredApplicationChoices.Length];
                for (int i = 0; i < authoredApplicationChoices.Length; i++)
                {
                    choices[i] = FromAsset(authoredApplicationChoices[i]);
                }

                return choices;
            }

            Debug.LogWarning("Final Round P28: authored application choices contain null entries. Falling back to built-in Desk prototype choices.");
            fallbackApplicationChoices = BuildFallbackApplicationChoices();
            return fallbackApplicationChoices;
        }

        if (fallbackApplicationChoices == null || fallbackApplicationChoices.Length == 0)
        {
            fallbackApplicationChoices = BuildFallbackApplicationChoices();
        }

        return fallbackApplicationChoices;
    }

    private static ApplicationStrategyChoice FromAsset(ApplicationChoiceData asset)
    {
        if (asset == null)
        {
            Debug.LogWarning("Final Round P28: attempted to read a null application choice asset. Using Honest Fit fallback.");
            return BuildFallbackApplicationChoices()[0];
        }

        if (string.IsNullOrWhiteSpace(asset.choiceId) || string.IsNullOrWhiteSpace(asset.label))
        {
            Debug.LogWarning($"Final Round P28: application choice asset {asset.name} is missing an id or label. Using Honest Fit fallback.");
            return BuildFallbackApplicationChoices()[0];
        }

        return new ApplicationStrategyChoice
        {
            choiceId = asset.choiceId,
            label = asset.label,
            bodyText = asset.bodyText,
            feedbackText = asset.feedbackText,
            roleFitDelta = asset.roleFitDelta,
            recruiterTrustDelta = asset.recruiterTrustDelta,
            candidateConfidenceDelta = asset.candidateConfidenceDelta,
            energyDelta = asset.energyDelta,
            overclaimRiskDelta = asset.overclaimRiskDelta,
            technicalReadinessDelta = asset.technicalReadinessDelta,
            rapportMomentumDelta = asset.rapportMomentumDelta
        };
    }

    private ApplicationStrategyChoice[] GetFallbackApplicationChoices()
    {
        if (fallbackApplicationChoices == null || fallbackApplicationChoices.Length == 0)
        {
            fallbackApplicationChoices = BuildFallbackApplicationChoices();
        }

        return fallbackApplicationChoices;
    }

    public static ApplicationStrategyChoice[] BuildFallbackApplicationChoices()
    {
        return new[]
        {
            new ApplicationStrategyChoice
            {
                choiceId = "APP-HONEST-FIT",
                label = "Honest Fit",
                bodyText = "Emphasise customer-facing security experience, then plainly name detection engineering and executive-demo areas as growth zones.",
                feedbackText = "The application reads grounded and low-risk. It may undersell some stretch fit, but it gives the recruiter fewer reasons to worry about overclaiming.",
                roleFitDelta = 0,
                recruiterTrustDelta = 1,
                candidateConfidenceDelta = -1,
                energyDelta = 0,
                overclaimRiskDelta = -2,
                technicalReadinessDelta = 1,
                rapportMomentumDelta = 1
            },
            new ApplicationStrategyChoice
            {
                choiceId = "APP-TAILORED-CREDIBLE",
                label = "Tailored Credible",
                bodyText = "Map previous workshops, stakeholder management, and security architecture work tightly to the listing without claiming perfect coverage.",
                feedbackText = "The application feels considered and relevant. It costs some energy, but it gives the recruiter a clean story to carry forward.",
                roleFitDelta = 2,
                recruiterTrustDelta = 2,
                candidateConfidenceDelta = 1,
                energyDelta = -1,
                overclaimRiskDelta = 1,
                technicalReadinessDelta = 1,
                rapportMomentumDelta = 1
            },
            new ApplicationStrategyChoice
            {
                choiceId = "APP-AGGRESSIVE-POSITIONING",
                label = "Aggressive",
                bodyText = "Position as a near-perfect match for enterprise security presales, implying direct ownership of outcomes the listing only hints at.",
                feedbackText = "The application has punch, but it creates a future proof problem. If the panel probes details, the story may need defending.",
                roleFitDelta = 2,
                recruiterTrustDelta = -1,
                candidateConfidenceDelta = 2,
                energyDelta = -1,
                overclaimRiskDelta = 2,
                technicalReadinessDelta = 0,
                rapportMomentumDelta = -1
            },
            new ApplicationStrategyChoice
            {
                choiceId = "APP-QUICK-APPLY",
                label = "Quick Apply",
                bodyText = "Send a lightly tailored version that mentions security presales, customer workshops, and architecture without doing much deeper mapping.",
                feedbackText = "You preserve energy, but the application feels generic. The recruiter has less signal and less story to work with.",
                roleFitDelta = -2,
                recruiterTrustDelta = -1,
                candidateConfidenceDelta = 0,
                energyDelta = 2,
                overclaimRiskDelta = -1,
                technicalReadinessDelta = -1,
                rapportMomentumDelta = -1
            }
        };
    }

    private static string FormatList(string[] items)
    {
        if (items == null || items.Length == 0)
        {
            return "- Not specified.";
        }

        return "- " + string.Join("\n- ", items);
    }

    private void SetStrategyButtonsVisible(bool visible)
    {
        if (strategyButtons == null)
        {
            return;
        }

        for (int i = 0; i < strategyButtons.Length; i++)
        {
            if (strategyButtons[i] != null)
            {
                strategyButtons[i].gameObject.SetActive(visible);
            }
        }

        if (visible)
        {
            RefreshStrategyButtonLabels();
        }
    }

    private void SetListingSectionButtonsVisible(bool visible)
    {
        if (listingSectionButtons == null)
        {
            return;
        }

        for (int i = 0; i < listingSectionButtons.Length; i++)
        {
            if (listingSectionButtons[i] != null)
            {
                listingSectionButtons[i].gameObject.SetActive(visible);
            }
        }

        if (visible)
        {
            RefreshListingSectionButtonLabels();
        }
    }

    private void RefreshListingSectionButtonLabels()
    {
        if (listingSectionButtons == null)
        {
            return;
        }

        string[] labels = GetListingSectionLabels();
        for (int i = 0; i < listingSectionButtons.Length && i < labels.Length; i++)
        {
            TMP_Text label = listingSectionButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = i == currentListingSectionIndex ? $"> {labels[i]}" : labels[i];
            }
        }
    }

    private void RefreshStrategyButtonLabels()
    {
        if (strategyButtons == null)
        {
            return;
        }

        ApplicationStrategyChoice[] choices = GetApplicationChoices();
        for (int i = 0; i < strategyButtons.Length && i < choices.Length; i++)
        {
            TMP_Text label = strategyButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = choices[i] == selectedApplicationChoice ? $"> {choices[i].label}" : choices[i].label;
            }
        }
    }

    private void SetConfirmInteractable(bool interactable)
    {
        if (confirmApplicationButton != null)
        {
            confirmApplicationButton.interactable = interactable;
        }
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private bool WasOpenLaptopPressed()
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
        {
            return false;
        }

        return Keyboard.current.eKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(openLaptopKey) || Input.GetKeyDown(alternateOpenLaptopKey);
#endif
    }

    private bool WasLaptopClicked()
    {
#if ENABLE_INPUT_SYSTEM
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
        {
            return false;
        }

        Vector2 pointerPosition = Mouse.current.position.ReadValue();
#else
        if (!Input.GetMouseButtonDown(0))
        {
            return false;
        }

        Vector2 pointerPosition = Input.mousePosition;
#endif
        if (deskCamera == null || laptopInteractable == null)
        {
            return false;
        }

        Ray ray = deskCamera.ScreenPointToRay(pointerPosition);
        return Physics.Raycast(ray, out RaycastHit hit, 100f) && hit.transform == laptopInteractable;
    }

    private static TMP_Text CreateText(string name, Transform parent, string text, int size, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI), typeof(LayoutElement));
        textObject.transform.SetParent(parent, false);
        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        tmpText.text = text;
        tmpText.fontSize = size;
        tmpText.fontStyle = style;
        tmpText.alignment = alignment;
        tmpText.color = new Color32(238, 242, 248, 255);
        tmpText.textWrappingMode = TextWrappingModes.Normal;
        return tmpText;
    }

    private static Button CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction action)
    {
        return CreateButton(label, parent, action, label.Length > 12 ? 230f : 150f, 46f, 18);
    }

    private static Button CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction action, float width)
    {
        return CreateButton(label, parent, action, width, 46f, 18);
    }

    private static Button CreateButton(string label, Transform parent, UnityEngine.Events.UnityAction action, float width, float height, int fontSize)
    {
        GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonObject.transform.SetParent(parent, false);
        buttonObject.GetComponent<Image>().color = new Color32(29, 82, 92, 255);
        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.onClick.AddListener(action);

        LayoutElement layoutElement = buttonObject.GetComponent<LayoutElement>();
        layoutElement.preferredWidth = width;
        layoutElement.preferredHeight = height;

        TMP_Text text = CreateText("Label", buttonObject.transform, label, fontSize, FontStyles.Bold, TextAlignmentOptions.Center);
        RectTransform textRect = text.rectTransform;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(10f, 6f);
        textRect.offsetMax = new Vector2(-10f, -6f);
        return button;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static Transform CreateCube(string name, Vector3 position, Vector3 scale, Material material)
    {
        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = name;
        cube.transform.position = position;
        cube.transform.localScale = scale;
        if (material != null && cube.TryGetComponent(out Renderer renderer))
        {
            renderer.sharedMaterial = material;
        }

        return cube.transform;
    }

    private static Material CreateMaterial(string name, Color color)
    {
        Material material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard"));
        material.name = name;
        material.color = color;
        return material;
    }

    private static void EnsureEventSystem()
    {
        EventSystem eventSystem = FindAnyObjectByType<EventSystem>();
        if (eventSystem != null)
        {
#if ENABLE_INPUT_SYSTEM
            if (eventSystem.GetComponent<InputSystemUIInputModule>() == null)
            {
                StandaloneInputModule legacyModule = eventSystem.GetComponent<StandaloneInputModule>();
                if (legacyModule != null)
                {
                    Destroy(legacyModule);
                }

                eventSystem.gameObject.AddComponent<InputSystemUIInputModule>();
            }
#else
            if (eventSystem.GetComponent<StandaloneInputModule>() == null)
            {
                eventSystem.gameObject.AddComponent<StandaloneInputModule>();
            }
#endif
            return;
        }

#if ENABLE_INPUT_SYSTEM
        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
        GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
        eventSystemObject.transform.SetAsLastSibling();
    }
}
