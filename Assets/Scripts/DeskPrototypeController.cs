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

    [System.Serializable]
    public sealed class RecruiterScreenChoice
    {
        public string choiceId;
        public string label;
        [TextArea(2, 6)] public string responseText;
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

    [System.Serializable]
    public sealed class RecruiterScreenPrompt
    {
        public string questionId;
        public string prompt;
        public RecruiterScreenChoice[] responseChoices;
    }

    public enum DeskPrototypeState
    {
        Standby,
        LaptopFocus,
        JobBoard,
        JobListing,
        ApplicationChoice,
        Recruiter,
        Prep,
        OutcomeInbox,
        ProcessSummary,
        TransitioningToRoom
    }

    private const string PlaceholderJobId = "NCS-SE-001";
    private const string PlaceholderCompany = "Northbridge Cyber Systems";
    private const string PlaceholderRole = "Senior Solutions Engineer - Security Presales";
    private const string DefaultSalaryRange = "Base salary listed as competitive, with variable compensation discussed later in process.";
    private const string DefaultProcessNotes = "Recruiter screen, technical/presales panel, final customer-scenario round. Timeline described as fast if the team aligns.";
    private const string RecruiterPathId = "MAYA-PATEL-SCREEN";
    private const string RecruiterSender = "Maya Patel";
    private const string RecruiterSubject = "Northbridge Cyber Systems - quick screen";
    private const string RecruiterIntroMessage = "Hi,\n\nThanks for applying. Your profile looks relevant for the Senior Solutions Engineer - Security Presales role. The team is moving fairly quickly, so I would like to run through a short screen before I forward your profile to the panel.\n\nA few quick questions below.";
    private const string AftermathRoomSceneName = "AftermathRoom";
    private const string Vs4JobListingsResourcePath = "FinalRound/VS4/JobListings";
    private const string NorthbridgeJobId = "northbridge-security-presales";
    private const string HeliosJobId = "helios-cloud-security-consultant";
    private const string RedgateJobId = "redgate-risk-compliance-presales";

    [Header("Scene")]
    [SerializeField] private string interviewRoomSceneName = "InterviewRoom";
    [SerializeField] private bool generatePrototypeSceneObjects = true;
    [SerializeField] private Camera deskCamera;
    [SerializeField] private Transform laptopInteractable;

    [Header("Run State")]
    [SerializeField] private DeskPrototypeState currentState = DeskPrototypeState.Standby;
    [SerializeField] private JobListingData defaultJobListing;
    [SerializeField] private JobListingData[] availableJobListings;
    [SerializeField] private ApplicationChoiceData[] authoredApplicationChoices;
    [SerializeField] private RecruiterMessageData recruiterIntroMessage;
    [SerializeField] private RecruiterScreenQuestionData[] authoredRecruiterQuestions;

    [Header("Debug")]
    [SerializeField] private bool showDebugReadoutByDefault;
    [SerializeField] private bool allowDeskDebugToggle = true;

    [Header("Input")]
#if !ENABLE_INPUT_SYSTEM
    [SerializeField] private KeyCode openLaptopKey = KeyCode.E;
    [SerializeField] private KeyCode alternateOpenLaptopKey = KeyCode.Space;
#endif

    private Canvas canvas;
    private GameObject laptopPanel;
    private GameObject pausePanel;
    private GameObject debugPanel;
    private GameObject jobBoardRow;
    private GameObject listingSectionRow;
    private GameObject strategyRow;
    private GameObject recruiterRow;
    private GameObject postAftermathChoiceRow;
    private TMP_Text debugText;
    private TMP_Text laptopTitleText;
    private TMP_Text modeText;
    private TMP_Text roleLineText;
    private TMP_Text companyLineText;
    private TMP_Text listingSummaryText;
    private TMP_Text feedbackText;
    private Button backToJobBoardButton;
    private Button viewListingButton;
    private Button applicationStrategyButton;
    private Button confirmApplicationButton;
    private Button recruiterButton;
    private Button interviewButton;
    private Button clearRoomButton;
    private Button processSummaryButton;
    private Button applyAgainButton;
    private Button takeBreakButton;
    private Button askFeedbackButton;
    private Button reviewProcessSummaryButton;
    private Button mainMenuButton;
    private Button resetButton;
    private Button pauseResumeButton;
    private Button pauseMainMenuButton;
    private Button pauseStartNewRunButton;
    private GameObject confirmApplicationButtonObject;
    private Button[] jobCardButtons;
    private Button[] listingSectionButtons;
    private Button[] strategyButtons;
    private Button[] recruiterChoiceButtons;
    private ApplicationStrategyChoice[] fallbackApplicationChoices;
    private RecruiterScreenPrompt[] fallbackRecruiterPrompts;
    private JobListingData[] cachedResourceJobListings;
    private JobListingData selectedJobListing;
    private ApplicationStrategyChoice selectedApplicationChoice;
    private int currentListingSectionIndex;
    private int currentRecruiterPromptIndex;
    private bool applicationConfirmed;
    private bool recruiterCompleted;
    private bool deskDebugVisible;
    private bool pauseVisible;
    private string recruiterResponseIds;

    public DeskPrototypeState CurrentState => currentState;

    private void Start()
    {
        if (generatePrototypeSceneObjects)
        {
            BuildPrototypeSceneShell();
        }

        BuildPrototypeUi();
        deskDebugVisible = showDebugReadoutByDefault;
        ShowCompletedRunInboxIfAvailable();
        RefreshDebugDisplay();
        RefreshDebugVisibility();
        Debug.Log("Final Round P33: Desk ready. Press E/Space or click the laptop to open the job listing UI. Press F1 to toggle the Desk debug readout.");
    }

    private void Update()
    {
        if (WasDebugTogglePressed())
        {
            ToggleDeskDebug();
        }

        if (WasPauseTogglePressed())
        {
            TogglePauseMenu();
        }

        if (pauseVisible)
        {
            return;
        }

        if (WasOpenLaptopPressed() || WasLaptopClicked())
        {
            OpenLaptopInterface();
        }
    }

    public void BeginDeskRun()
    {
        CandidateState state = FinalRoundRunState.CreateNeutralRun();
        selectedJobListing = null;
        if (!ShouldUseJobBoard())
        {
            SelectJobForRun(GetFallbackJobListing(), state);
        }

        currentState = DeskPrototypeState.LaptopFocus;
        applicationConfirmed = false;
        recruiterCompleted = false;
        recruiterResponseIds = string.Empty;
        currentRecruiterPromptIndex = 0;
        selectedApplicationChoice = null;
        Debug.Log("Final Round P31: Desk run started.\n" + state.BuildDebugSummary());
        RefreshDebugDisplay();
    }

    public CandidateState CreateNeutralCandidateState()
    {
        CandidateState state = FinalRoundRunState.CreateNeutralRun();
        selectedJobListing = null;
        if (!ShouldUseJobBoard())
        {
            SelectJobForRun(GetFallbackJobListing(), state);
        }

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
        else if (FinalRoundRunState.TryGetActiveState(out CandidateState state))
        {
            if (!ShouldUseJobBoard() || !string.IsNullOrWhiteSpace(state.SelectedJobId))
            {
                EnsureSelectedJobForState(state);
            }
        }

        if (ShowCompletedRunInboxIfAvailable())
        {
            return;
        }

        if (laptopPanel != null)
        {
            SetLaptopPanelVisible(true);
        }

        if (ShouldShowJobBoardFirst())
        {
            ShowJobBoardView();
        }
        else
        {
            ShowListingView();
        }

        RefreshDebugDisplay();
    }

    public bool SelectJobById(string jobId)
    {
        JobListingData job = FindJobListingById(jobId);
        if (job == null)
        {
            Debug.LogWarning($"Final Round P41: no job listing found for id '{jobId}'. Keeping current/default listing.");
            return false;
        }

        SelectJobFromBoard(job);
        return true;
    }

    public void ShowJobBoardView()
    {
        if (!ShouldUseJobBoard())
        {
            ShowListingView();
            return;
        }

        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }

        currentState = DeskPrototypeState.JobBoard;
        SetLaptopPanelVisible(true);
        SetLaptopTextAreaLayout(40f, 62f, 16, 15);
        RefreshJobHeaderText();
        SetText(modeText, "Job Board");
        SetText(
            listingSummaryText,
            "Choose one opportunity. You can switch before applying.");
        SetText(feedbackText, HasSelectedJob()
            ? $"Selected: {GetCompanyName(GetActiveJobListing())} - {GetRoleTitle(GetActiveJobListing())}"
            : "Select a card to unlock the listing.");
        SetJobBoardVisible(true);
        SetListingSectionButtonsVisible(false);
        SetStrategyButtonsVisible(false);
        SetRecruiterChoiceButtonsVisible(false);
        SetConfirmInteractable(false);
        SetConfirmVisible(false);
        SetRecruiterInteractable(false);
        SetPostOutcomeButtonsVisible(false);
        SetPostAftermathChoicesVisible(false);
        SetBoardNavigationState();
        RefreshJobCardButtonLabels();
        RefreshDebugDisplay();
    }

    private void SelectJobFromBoard(JobListingData job)
    {
        if (job == null)
        {
            SetText(feedbackText, "That opportunity is unavailable.");
            return;
        }

        if (applicationConfirmed)
        {
            SetText(feedbackText, "Application already submitted. Start a new run to choose another role.");
            return;
        }

        CandidateState state = FinalRoundRunState.CreateNeutralRun();
        applicationConfirmed = false;
        recruiterCompleted = false;
        recruiterResponseIds = string.Empty;
        currentRecruiterPromptIndex = 0;
        selectedApplicationChoice = null;
        currentListingSectionIndex = 0;

        SelectJobForRun(job, state);
        currentState = DeskPrototypeState.JobBoard;
        RefreshJobHeaderText();
        SetText(feedbackText, $"Selected: {GetCompanyName(job)} - {GetRoleTitle(job)}");
        SetBoardNavigationState();
        RefreshJobCardButtonLabels();
        RefreshDebugDisplay();
    }

    public void ViewSelectedListing()
    {
        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }

        if (FinalRoundRunState.TryGetActiveState(out CandidateState state))
        {
            if (ShouldUseJobBoard() && string.IsNullOrWhiteSpace(state.SelectedJobId))
            {
                ShowJobBoardView();
                return;
            }

            EnsureSelectedJobForState(state);
        }

        ShowListingView();
    }

    private void ReturnToJobBoard()
    {
        if (applicationConfirmed)
        {
            SetText(feedbackText, "Application already submitted. Start a new run to choose another role.");
            return;
        }

        ShowJobBoardView();
    }

    public void ShowListingView()
    {
        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }
        else if (ShouldUseJobBoard()
            && FinalRoundRunState.TryGetActiveState(out CandidateState boardState)
            && string.IsNullOrWhiteSpace(boardState.SelectedJobId))
        {
            ShowJobBoardView();
            return;
        }
        else if (FinalRoundRunState.TryGetActiveState(out CandidateState activeState))
        {
            EnsureSelectedJobForState(activeState);
        }

        if (ShouldUseJobBoard() && !HasSelectedJob())
        {
            ShowJobBoardView();
            return;
        }

        currentState = DeskPrototypeState.JobListing;
        selectedApplicationChoice = null;
        SetLaptopTextAreaLayout(250f, 96f, 18, 16);
        RefreshJobHeaderText();

        SetText(modeText, "View Listing");
        SetText(feedbackText, "Review the role, then choose how to position your application.");
        SetJobBoardVisible(false);
        ShowListingSection(currentListingSectionIndex);
        SetListingSectionButtonsVisible(true);
        SetStrategyButtonsVisible(false);
        SetRecruiterChoiceButtonsVisible(false);
        SetConfirmInteractable(false);
        SetConfirmVisible(true);
        SetRecruiterInteractable(applicationConfirmed);
        SetPostOutcomeButtonsVisible(IsCompletedRoomRunActive());
        SetBoardNavigationState();
        RefreshDebugDisplay();
    }

    public void ShowApplicationChoices()
    {
        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }

        if (ShouldUseJobBoard() && !HasSelectedJob())
        {
            ShowJobBoardView();
            SetText(feedbackText, "Select an opportunity before choosing an application strategy.");
            return;
        }

        currentState = DeskPrototypeState.ApplicationChoice;
        selectedApplicationChoice = null;
        SetLaptopTextAreaLayout(250f, 96f, 18, 16);
        if (listingSummaryText != null)
        {
            listingSummaryText.text =
                "Choose an application strategy.\n\n" +
                "Choose how to position your application before Maya decides whether to move you forward.";
        }

        SetText(modeText, "Choose Application Strategy");
        SetText(feedbackText, "Select a strategy, then confirm the application.");
        SetJobBoardVisible(false);
        SetListingSectionButtonsVisible(false);
        SetStrategyButtonsVisible(true);
        SetRecruiterChoiceButtonsVisible(false);
        SetConfirmInteractable(false);
        SetConfirmVisible(true);
        SetPostOutcomeButtonsVisible(false);
        SetBoardNavigationState();
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

        EnsureSelectedJobForState(state);
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
        recruiterCompleted = false;
        currentState = DeskPrototypeState.Prep;
        SetLaptopTextAreaLayout(250f, 96f, 18, 16);
        SetText(modeText, "Application Submitted");
        SetText(
            listingSummaryText,
            "Application submitted.\n\n" +
            selectedApplicationChoice.feedbackText +
            $"\n\n{GetRecruiterName()} has replied with a short recruiter screen.");
        SetText(feedbackText, "Application confirmed. Complete the recruiter screen to continue.");
        SetListingSectionButtonsVisible(false);
        SetStrategyButtonsVisible(false);
        SetRecruiterChoiceButtonsVisible(false);
        SetConfirmInteractable(false);
        SetConfirmVisible(false);
        SetRecruiterInteractable(true);
        SetPostOutcomeButtonsVisible(false);

        Debug.Log("Final Round P31: application strategy confirmed.\n" + state.BuildDebugSummary());
        RefreshDebugDisplay();
    }

    public void ShowRecruiterScreen()
    {
        if (!FinalRoundRunState.HasActiveRun())
        {
            BeginDeskRun();
        }

        if (!applicationConfirmed && FinalRoundRunState.TryGetActiveState(out CandidateState state) && !string.IsNullOrWhiteSpace(state.ApplicationChoiceId))
        {
            applicationConfirmed = true;
        }

        if (!applicationConfirmed)
        {
            SetText(feedbackText, "Choose and confirm an application strategy before the recruiter screen.");
            return;
        }

        currentState = DeskPrototypeState.Recruiter;
        SetLaptopTextAreaLayout(250f, 96f, 18, 16);
        SetText(modeText, recruiterCompleted ? "Recruiter Screen Complete" : $"Recruiter Screen {currentRecruiterPromptIndex + 1} of {GetRecruiterPrompts().Length}");
        SetJobBoardVisible(false);
        SetListingSectionButtonsVisible(false);
        SetStrategyButtonsVisible(false);
        SetConfirmInteractable(false);
        SetConfirmVisible(false);
        SetRecruiterChoiceButtonsVisible(!recruiterCompleted);
        SetText(feedbackText, recruiterCompleted ? "Recruiter screen complete. Continue to the interview when ready." : "Choose one reply below.");
        SetPostOutcomeButtonsVisible(false);
        RenderRecruiterPrompt();
        RefreshDebugDisplay();
    }

    private void SelectRecruiterResponse(RecruiterScreenChoice choice)
    {
        if (choice == null || recruiterCompleted)
        {
            return;
        }

        CandidateState state = FinalRoundRunState.HasActiveRun()
            ? FinalRoundRunState.Instance.State
            : FinalRoundRunState.CreateNeutralRun();

        EnsureSelectedJobForState(state);
        state.ApplyDeltas(
            choice.roleFitDelta,
            choice.recruiterTrustDelta,
            choice.candidateConfidenceDelta,
            choice.energyDelta,
            choice.overclaimRiskDelta,
            choice.technicalReadinessDelta,
            choice.rapportMomentumDelta);

        recruiterResponseIds = string.IsNullOrWhiteSpace(recruiterResponseIds)
            ? choice.choiceId
            : $"{recruiterResponseIds},{choice.choiceId}";
        state.RecruiterResponseIds = recruiterResponseIds;

        SetText(feedbackText, $"{choice.label}: {choice.feedbackText}");

        currentRecruiterPromptIndex++;
        RecruiterScreenPrompt[] prompts = GetRecruiterPrompts();
        if (currentRecruiterPromptIndex >= prompts.Length)
        {
            recruiterCompleted = true;
            currentRecruiterPromptIndex = prompts.Length - 1;
            state.RecruiterPathId = RecruiterPathId;
            SetText(modeText, "Recruiter Screen Complete");
            SetText(
                listingSummaryText,
                $"{GetRecruiterName()} forwards your profile to the interview panel.\n\n" +
                "Your application notes are attached to the invite.\n\n" +
                "Final round scheduled.");
            SetText(feedbackText, "Recruiter screen complete. Continue to the interview when ready.");
            SetRecruiterChoiceButtonsVisible(false);
            SetInterviewButtonLabel("Continue to Interview");
            SetRecruiterInteractable(false);
            Debug.Log("Final Round P31: recruiter screen complete.\n" + state.BuildDebugSummary());
        }
        else
        {
            SetText(modeText, $"Recruiter Screen {currentRecruiterPromptIndex + 1} of {prompts.Length}");
            SetText(feedbackText, "Choose one reply below.");
            RenderRecruiterPrompt();
            Debug.Log("Final Round P31: recruiter response recorded.\n" + state.BuildDebugSummary());
        }

        RefreshDebugDisplay();
    }

    private void SelectApplicationChoice(ApplicationStrategyChoice choice)
    {
        selectedApplicationChoice = choice;
        SetText(
            feedbackText,
            $"{choice.label}\n{choice.bodyText}\n\nConfirm this application strategy to continue.");
        SetConfirmInteractable(true);
        RefreshStrategyButtonLabels();
    }

    public void GoToInterviewRoom()
    {
        CandidateState state = FinalRoundRunState.HasActiveRun()
            ? FinalRoundRunState.Instance.State
            : FinalRoundRunState.CreateNeutralRun();

        EnsureSelectedJobForState(state);

        currentState = DeskPrototypeState.TransitioningToRoom;
        Debug.Log(
            $"Final Round P31: Desk-to-Room transition requested. Scene: {interviewRoomSceneName}\n" +
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
        SetLaptopTextAreaLayout(250f, 96f, 18, 16);
        if (laptopPanel != null)
        {
            SetLaptopPanelVisible(false);
        }

        if (listingSummaryText != null)
        {
            listingSummaryText.text = BuildListingSectionText(0);
        }

        SetJobBoardVisible(false);
        applicationConfirmed = false;
        recruiterCompleted = false;
        recruiterResponseIds = string.Empty;
        selectedJobListing = null;
        selectedApplicationChoice = null;
        currentListingSectionIndex = 0;
        currentRecruiterPromptIndex = 0;
        SetText(modeText, "View Listing");
        SetText(feedbackText, "Desk run reset. Open the laptop to review the role.");
        SetListingSectionButtonsVisible(true);
        SetStrategyButtonsVisible(false);
        SetRecruiterChoiceButtonsVisible(false);
        SetConfirmInteractable(false);
        SetConfirmVisible(true);
        SetRecruiterInteractable(false);
        SetInterviewButtonLabel("Debug: Go To Interview");
        SetResetButtonLabel("Reset Desk Run");
        SetPostOutcomeButtonsVisible(false);

        Debug.Log("Final Round P31: Desk run reset.");
        RefreshDebugDisplay();
    }

    public bool ShowCompletedRunInboxIfAvailable()
    {
        if (!FinalRoundRunState.TryGetActiveState(out CandidateState state) || string.IsNullOrWhiteSpace(state.RoomOutcome))
        {
            return false;
        }

        ShowOutcomeInbox(state);
        return true;
    }

    public void ShowOutcomeInbox()
    {
        if (!FinalRoundRunState.TryGetActiveState(out CandidateState state) || string.IsNullOrWhiteSpace(state.RoomOutcome))
        {
            SetText(feedbackText, "No completed interview result is available yet.");
            return;
        }

        ShowOutcomeInbox(state);
    }

    private void ShowOutcomeInbox(CandidateState state)
    {
        currentState = DeskPrototypeState.OutcomeInbox;
        if (laptopPanel != null)
        {
            SetLaptopPanelVisible(true);
        }

        SetText(modeText, $"{GetRecruiterCompany()} Mail / Inbox");
        SetLaptopTextAreaLayout(state.AftermathCompleted ? 285f : 330f, state.AftermathCompleted ? 105f : 54f, 17, 15);
        SetText(listingSummaryText, BuildOutcomeInboxMessage(state));
        SetText(feedbackText, BuildOutcomeFeedbackLine(state));
        SetJobBoardVisible(false);
        SetListingSectionButtonsVisible(false);
        SetStrategyButtonsVisible(false);
        SetRecruiterChoiceButtonsVisible(false);
        SetConfirmVisible(false);
        SetRecruiterInteractable(false);
        SetResetButtonLabel("Start New Run");
        SetPostOutcomeButtonsVisible(true);
        SetPostAftermathChoicesVisible(state.AftermathCompleted);

        Debug.Log("Final Round P31: Desk inbox opened for completed Room run.\n" + state.BuildDebugSummary());
        RefreshDebugDisplay();
    }

    public void ShowProcessSummary()
    {
        if (!FinalRoundRunState.TryGetActiveState(out CandidateState state))
        {
            SetText(feedbackText, "No active Desk run is available.");
            return;
        }

        currentState = DeskPrototypeState.ProcessSummary;
        SetText(modeText, "Process Summary");
        SetLaptopTextAreaLayout(370f, 20f, 15, 14);
        SetText(listingSummaryText, BuildProcessSummary(state));
        SetText(feedbackText, string.Empty);
        SetJobBoardVisible(false);
        SetListingSectionButtonsVisible(false);
        SetStrategyButtonsVisible(false);
        SetRecruiterChoiceButtonsVisible(false);
        SetConfirmVisible(false);
        SetRecruiterInteractable(false);
        SetPostOutcomeButtonsVisible(IsCompletedRoomRunActive());
        SetPostAftermathChoicesVisible(state.AftermathCompleted);
        RefreshDebugDisplay();
    }

    private void ApplyAgainAfterAftermath()
    {
        ResetDeskRun();
        OpenLaptopInterface();
        SetText(feedbackText, "You set this process down and start again a little steadier.");
    }

    private void TakeBreakAfterAftermath()
    {
        if (!FinalRoundRunState.TryGetActiveState(out CandidateState state))
        {
            SetText(feedbackText, "No active process is available.");
            return;
        }

        state.Energy += 1;
        SetText(feedbackText, "You step away from the laptop. Nothing is solved, but your energy returns a little.");
        RefreshDebugDisplay();
    }

    private void AskForFeedbackAfterAftermath()
    {
        SetText(feedbackText, $"{GetRecruiterName()} says they'll ask the panel, but can't promise detailed feedback.");
    }

    private void ReviewProcessSummaryAfterAftermath()
    {
        ShowProcessSummary();
    }

    public void ReturnToMainMenu()
    {
        if (FinalRoundRunState.HasInstance)
        {
            FinalRoundRunState.Instance.ResetRun();
        }

        Debug.Log("Final Round P31: returning from Desk to main menu.");
        SceneManager.LoadScene(interviewRoomSceneName);
    }

    private string BuildOutcomeInboxMessage(CandidateState state)
    {
        string outcome = string.IsNullOrWhiteSpace(state.RoomOutcome) ? "Hold" : state.RoomOutcome;
        JobListingData job = FindJobListingById(state.SelectedJobId) ?? GetActiveJobListing();
        string subject;
        string body;
        switch (outcome)
        {
            case nameof(InterviewOutcomeType.StrongPass):
                subject = "Subject: Strong next step";
                body =
                    "Thanks again for the final conversation. The panel came away with a strong signal and would like to continue quickly.\n\n" +
                    "We are aligning on the next practical step and will follow up with details shortly.";
                break;
            case nameof(InterviewOutcomeType.Pass):
                subject = "Subject: Interview follow-up";
                body =
                    "Thank you for speaking with the panel. There are a few areas the team would want to calibrate, but the signal from the final round was strong enough to continue the process.\n\n" +
                    "We will come back once the hiring team has aligned on timing and next steps.";
                break;
            case nameof(InterviewOutcomeType.Reject):
                subject = "Subject: Final round update";
                body =
                    "Thank you for the time and preparation throughout the process.\n\n" +
                    "After review, the team has decided not to move forward. The feedback was not about one single answer, but about overall fit for this specific panel and role at this stage.";
                break;
            default:
                subject = "Subject: Final round update";
                body =
                    "Thank you for the conversation today. We appreciate the time and preparation.\n\n" +
                    "We are still aligning internally and will come back to you once we have completed the process. At this stage, feedback is not negative, but it is not fully settled.";
                break;
        }

        return
            $"From  {GetRecruiterName(job)}, {GetRecruiterCompany(job)} Recruiting\n" +
            "Time  Today, 17:18\n" +
            $"{subject}\n\n" +
            "Hi,\n\n" +
            body + "\n\n" +
            FormatOptionalLine(GetOutcomeContextLine(job)) +
            BuildCandidateContextLine(state);
    }

    private static string BuildOutcomeFeedbackLine(CandidateState state)
    {
        string aftermathLine = state.AftermathCompleted
            ? "\nAftermath: Clear the Room completed.\nThe room is quieter now. The rejection is still there, but it no longer fills the screen."
            : IsAftermathEntryAvailable(state)
                ? "\nAftermath: Clear the Room is available."
                : string.Empty;

        return
            $"Outcome: {FormatId(state.RoomOutcome)}\n" +
            $"Process signal: job {FormatId(state.SelectedJobId)}, application {FormatId(state.ApplicationChoiceId)}, recruiter path {FormatId(state.RecruiterPathId)}." +
            aftermathLine;
    }

    private string BuildProcessSummary(CandidateState state)
    {
        JobListingData job = FindJobListingById(state.SelectedJobId) ?? GetActiveJobListing();
        return
            "Desk-to-Room process summary\n\n" +
            $"Job: {FormatId(state.SelectedJobId)}\n" +
            $"Company: {GetCompanyName(job)}\n" +
            $"Role: {GetRoleTitle(job)}\n" +
            $"Profile: {FormatId(GetDifficultyProfile(job))}\n" +
            FormatOptionalLine(GetProcessSummaryNote(job)) +
            $"Application: {FormatId(state.ApplicationChoiceId)}\n" +
            $"Recruiter: {FormatId(state.RecruiterPathId)}\n" +
            $"Replies: {FormatId(state.RecruiterResponseIds)}\n" +
            $"Room outcome: {FormatId(state.RoomOutcome)}\n\n" +
            $"Aftermath: {(state.AftermathCompleted ? "completed" : state.AftermathAvailable ? "available" : "not available")}\n\n" +
            "CandidateState\n" +
            $"Role {FormatSigned(state.RoleFit)} | Trust {FormatSigned(state.RecruiterTrust)} | Confidence {FormatSigned(state.CandidateConfidence)} | Energy {FormatSigned(state.Energy)}\n" +
            $"Overclaim {FormatSigned(state.OverclaimRisk)} | Tech Ready {FormatSigned(state.TechnicalReadiness)} | Rapport {FormatSigned(state.RapportMomentum)}\n\n" +
            "Room modifiers\n" +
            BuildRoomModifierBrief(state.RoomModifierSummary);
    }

    public void EnterAftermathRoom()
    {
        if (!FinalRoundRunState.TryGetActiveState(out CandidateState state) || !IsAftermathEntryAvailable(state))
        {
            SetText(feedbackText, "No active Reject aftermath is available.");
            return;
        }

        Debug.Log("Final Round P36: Clear the Room selected from Desk.\n" + state.BuildDebugSummary());
        SceneManager.LoadScene(AftermathRoomSceneName);
    }

    private static string BuildRoomModifierBrief(string modifierSummary)
    {
        if (string.IsNullOrWhiteSpace(modifierSummary))
        {
            return "Neutral / no Desk modifiers applied.";
        }

        string[] lines = modifierSummary.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0)
        {
            return "Neutral / no Desk modifiers applied.";
        }

        string firstLine = lines[0];
        string secondLine = lines.Length > 1 ? lines[1] : string.Empty;
        return string.IsNullOrWhiteSpace(secondLine)
            ? firstLine
            : $"{firstLine}\n{secondLine}";
    }

    private static string BuildCandidateContextLine(CandidateState state)
    {
        if (state.OverclaimRisk >= 2)
        {
            return "The team noted some gaps between early positioning and scenario depth.";
        }

        if (state.RecruiterTrust >= 2)
        {
            return "The early screen helped create a positive starting point.";
        }

        if (state.Energy <= -2)
        {
            return "The panel felt the process lost some momentum in later stages.";
        }

        if (state.RoleFit >= 2)
        {
            return "The role alignment remained a positive signal.";
        }

        return "The final decision reflects both the Desk process and the Room panel signal.";
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
        CreatePausePanel(canvas.transform);
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

        TMP_Text prompt = CreateText("Desk Prompt", parent, "Open the laptop to review the role. Press E / Space or click.", 20, FontStyles.Normal, TextAlignmentOptions.Left);
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

        laptopTitleText = CreateText("Laptop Title", laptopPanel.transform, "Opportunity Board", 34, FontStyles.Bold, TextAlignmentOptions.Left);
        roleLineText = CreateText("Laptop Role", laptopPanel.transform, GetRoleLine(), 24, FontStyles.Bold, TextAlignmentOptions.Left);
        companyLineText = CreateText("Laptop Company", laptopPanel.transform, GetCompanyLine(), 20, FontStyles.Normal, TextAlignmentOptions.Left);

        modeText = CreateText("Laptop Mode", laptopPanel.transform, "View Listing", 20, FontStyles.Bold, TextAlignmentOptions.Left);
        modeText.color = new Color32(98, 218, 195, 255);

        jobBoardRow = new GameObject("Job Board Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        jobBoardRow.transform.SetParent(laptopPanel.transform, false);
        HorizontalLayoutGroup jobBoardLayout = jobBoardRow.GetComponent<HorizontalLayoutGroup>();
        jobBoardLayout.spacing = 12f;
        jobBoardLayout.childForceExpandWidth = false;
        jobBoardLayout.childForceExpandHeight = false;
        jobBoardRow.GetComponent<LayoutElement>().preferredHeight = 238f;

        jobCardButtons = new Button[3];
        for (int i = 0; i < jobCardButtons.Length; i++)
        {
            int jobIndex = i;
            jobCardButtons[i] = CreateButton($"Job {i + 1}", jobBoardRow.transform, () => SelectJobCard(jobIndex), 374f, 226f, 12);
            ConfigureJobCardButton(jobCardButtons[i]);
        }

        listingSectionRow = new GameObject("Listing Section Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        listingSectionRow.transform.SetParent(laptopPanel.transform, false);
        HorizontalLayoutGroup sectionLayout = listingSectionRow.GetComponent<HorizontalLayoutGroup>();
        sectionLayout.spacing = 8f;
        sectionLayout.childForceExpandWidth = false;
        sectionLayout.childForceExpandHeight = false;
        listingSectionRow.GetComponent<LayoutElement>().preferredHeight = 42f;

        string[] sectionLabels = GetListingSectionLabels();
        listingSectionButtons = new Button[sectionLabels.Length];
        for (int i = 0; i < sectionLabels.Length; i++)
        {
            int sectionIndex = i;
            listingSectionButtons[i] = CreateButton(sectionLabels[i], listingSectionRow.transform, () => ShowListingSection(sectionIndex), 116f, 38f, 14);
        }

        listingSummaryText = CreateText("Laptop Summary", laptopPanel.transform, BuildListingSectionText(0), 18, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        listingSummaryText.color = new Color32(209, 217, 224, 255);
        listingSummaryText.rectTransform.sizeDelta = new Vector2(0f, 260f);
        listingSummaryText.GetComponent<LayoutElement>().preferredHeight = 250f;

        feedbackText = CreateText("Application Feedback", laptopPanel.transform, "Review the opportunity, then choose how to position the application.", 16, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        feedbackText.color = new Color32(172, 181, 196, 255);
        feedbackText.GetComponent<LayoutElement>().preferredHeight = 96f;

        strategyRow = new GameObject("Application Strategy Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
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

        recruiterRow = new GameObject("Recruiter Response Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        recruiterRow.transform.SetParent(laptopPanel.transform, false);
        HorizontalLayoutGroup recruiterLayout = recruiterRow.GetComponent<HorizontalLayoutGroup>();
        recruiterLayout.spacing = 10f;
        recruiterLayout.childForceExpandWidth = false;
        recruiterLayout.childForceExpandHeight = false;
        recruiterRow.GetComponent<LayoutElement>().preferredHeight = 48f;

        recruiterChoiceButtons = new Button[4];
        for (int i = 0; i < recruiterChoiceButtons.Length; i++)
        {
            int choiceIndex = i;
            recruiterChoiceButtons[i] = CreateButton($"Reply {i + 1}", recruiterRow.transform, () => SelectRecruiterResponse(GetCurrentRecruiterChoice(choiceIndex)), 210f, 46f, 16);
        }

        postAftermathChoiceRow = new GameObject("Post Aftermath Choice Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        postAftermathChoiceRow.transform.SetParent(laptopPanel.transform, false);
        HorizontalLayoutGroup aftermathLayout = postAftermathChoiceRow.GetComponent<HorizontalLayoutGroup>();
        aftermathLayout.spacing = 12f;
        aftermathLayout.childForceExpandWidth = false;
        aftermathLayout.childForceExpandHeight = false;
        postAftermathChoiceRow.GetComponent<LayoutElement>().preferredHeight = 46f;

        applyAgainButton = CreateButton("Apply Again", postAftermathChoiceRow.transform, ApplyAgainAfterAftermath, 135f, 42f, 15);
        takeBreakButton = CreateButton("Take a Break", postAftermathChoiceRow.transform, TakeBreakAfterAftermath, 145f, 42f, 15);
        askFeedbackButton = CreateButton("Ask for Feedback", postAftermathChoiceRow.transform, AskForFeedbackAfterAftermath, 170f, 42f, 15);
        reviewProcessSummaryButton = CreateButton("Review Summary", postAftermathChoiceRow.transform, ReviewProcessSummaryAfterAftermath, 170f, 42f, 15);

        GameObject buttonRow = new GameObject("Laptop Button Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        buttonRow.transform.SetParent(laptopPanel.transform, false);
        HorizontalLayoutGroup rowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 14f;
        rowLayout.childForceExpandWidth = false;
        rowLayout.childForceExpandHeight = false;
        buttonRow.GetComponent<LayoutElement>().preferredHeight = 52f;

        backToJobBoardButton = CreateButton("Back to Job Board", buttonRow.transform, ReturnToJobBoard, 170f, 46f, 16);
        viewListingButton = CreateButton("View Listing", buttonRow.transform, ViewSelectedListing, 130f, 46f, 16);
        applicationStrategyButton = CreateButton("Application Strategy", buttonRow.transform, ShowApplicationChoices, 185f, 46f, 16);
        confirmApplicationButton = CreateButton("Confirm Application", buttonRow.transform, ConfirmApplication, 175f, 46f, 16);
        confirmApplicationButtonObject = confirmApplicationButton.gameObject;
        recruiterButton = CreateButton("Recruiter Message", buttonRow.transform, ShowRecruiterScreen, 170f, 46f, 16);
        interviewButton = CreateButton("Debug: Go To Interview", buttonRow.transform, GoToInterviewRoom, 220f, 46f, 16);
        clearRoomButton = CreateButton("Clear the Room", buttonRow.transform, EnterAftermathRoom, 165f, 46f, 16);
        processSummaryButton = CreateButton("Process Summary", buttonRow.transform, ShowProcessSummary, 170f, 46f, 16);
        mainMenuButton = CreateButton("Main Menu", buttonRow.transform, ReturnToMainMenu, 130f, 46f, 16);
        resetButton = CreateButton("Reset Desk Run", buttonRow.transform, ResetDeskRun, 150f, 46f, 16);

        SetJobBoardVisible(false);
        SetListingSectionButtonsVisible(true);
        SetStrategyButtonsVisible(false);
        SetRecruiterChoiceButtonsVisible(false);
        SetConfirmInteractable(false);
        SetConfirmVisible(true);
        SetRecruiterInteractable(false);
        SetInterviewButtonLabel("Debug: Go To Interview");
        SetPostOutcomeButtonsVisible(false);
        SetPostAftermathChoicesVisible(false);
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

    private void CreatePausePanel(Transform parent)
    {
        pausePanel = CreatePanel("Desk Pause Menu", parent, new Color32(8, 12, 17, 242));
        RectTransform rect = pausePanel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = new Vector2(420f, 300f);

        VerticalLayoutGroup layout = pausePanel.AddComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(28, 28, 26, 26);
        layout.spacing = 14f;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        TMP_Text title = CreateText("Desk Pause Title", pausePanel.transform, "THE DESK", 28, FontStyles.Bold, TextAlignmentOptions.Center);
        title.GetComponent<LayoutElement>().preferredHeight = 42f;

        TMP_Text body = CreateText("Desk Pause Body", pausePanel.transform, "Pause", 18, FontStyles.Normal, TextAlignmentOptions.Center);
        body.color = new Color32(172, 181, 196, 255);
        body.GetComponent<LayoutElement>().preferredHeight = 34f;

        pauseResumeButton = CreateButton("Resume", pausePanel.transform, HidePauseMenu, 220f, 42f, 16);
        pauseMainMenuButton = CreateButton("Main Menu", pausePanel.transform, ReturnToMainMenu, 220f, 42f, 16);
        pauseStartNewRunButton = CreateButton("Start New Run", pausePanel.transform, ResetDeskRunFromPause, 220f, 42f, 16);
        SetPauseMenuVisible(false);
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
        string jobDefaults = FinalRoundRunState.TryGetActiveState(out CandidateState activeState)
            ? FormatId(activeState.JobDefaultDeltasAppliedJobId)
            : "none";

        debugText.text =
            $"VS2 Desk Debug\nState: {currentState}\nTarget Scene: {interviewRoomSceneName}\nApplication Confirmed: {applicationConfirmed}\nRecruiter Complete: {recruiterCompleted}\n" +
            $"Available Jobs: {GetAvailableJobListings().Length}\nActive Job: {GetActiveJobId()} / {GetCompanyName(GetActiveJobListing())}\nJob Defaults Applied: {jobDefaults}\n\n{summary}";
    }

    private void SetLaptopPanelVisible(bool visible)
    {
        if (laptopPanel != null)
        {
            laptopPanel.SetActive(visible);
        }

        if (debugPanel != null)
        {
            RefreshDebugVisibility();
        }
    }

    private void ToggleDeskDebug()
    {
        if (!allowDeskDebugToggle)
        {
            return;
        }

        deskDebugVisible = !deskDebugVisible;
        RefreshDebugDisplay();
        RefreshDebugVisibility();
    }

    private void RefreshDebugVisibility()
    {
        if (debugPanel == null)
        {
            return;
        }

        bool laptopIsOpen = laptopPanel != null && laptopPanel.activeSelf;
        debugPanel.SetActive(deskDebugVisible && !laptopIsOpen);
    }

    private void TogglePauseMenu()
    {
        SetPauseMenuVisible(!pauseVisible);
    }

    private void HidePauseMenu()
    {
        SetPauseMenuVisible(false);
    }

    private void ResetDeskRunFromPause()
    {
        SetPauseMenuVisible(false);
        ResetDeskRun();
    }

    private void SetPauseMenuVisible(bool visible)
    {
        pauseVisible = visible;
        if (pausePanel != null)
        {
            pausePanel.SetActive(visible);
        }
    }

    private void RenderRecruiterPrompt()
    {
        RecruiterScreenPrompt[] prompts = GetRecruiterPrompts();
        if (prompts.Length == 0)
        {
            SetText(modeText, "Recruiter Screen Unavailable");
            SetText(listingSummaryText, "Recruiter content is unavailable. Use the prototype shortcut to continue to the interview room.");
            SetText(feedbackText, "Run Final Round > Create/Repair P29 Recruiter Content Assets, or use runtime fallback content.");
            SetRecruiterChoiceButtonsVisible(false);
            return;
        }

        RecruiterScreenPrompt prompt = prompts[Mathf.Clamp(currentRecruiterPromptIndex, 0, prompts.Length - 1)];
        string intro = currentRecruiterPromptIndex == 0
            ? $"{GetRecruiterName()} - {GetRecruiterTitle()}\nSubject: {GetRecruiterSubject()}\n\n{GetRecruiterName()} says your profile looks relevant and the team is moving quickly. They want a short screen before forwarding you to the panel.\n\n"
            : string.Empty;

        SetText(
            listingSummaryText,
            $"{intro}Question {currentRecruiterPromptIndex + 1} of {prompts.Length}\n\n{prompt.prompt}");
        RefreshRecruiterChoiceButtonLabels();
    }

    private RecruiterScreenChoice GetCurrentRecruiterChoice(int choiceIndex)
    {
        RecruiterScreenPrompt[] prompts = GetRecruiterPrompts();
        if (prompts.Length == 0)
        {
            return null;
        }

        RecruiterScreenPrompt prompt = prompts[Mathf.Clamp(currentRecruiterPromptIndex, 0, prompts.Length - 1)];
        if (prompt.responseChoices == null || choiceIndex < 0 || choiceIndex >= prompt.responseChoices.Length)
        {
            return null;
        }

        return prompt.responseChoices[choiceIndex];
    }

    private void SetRecruiterInteractable(bool interactable)
    {
        if (recruiterButton != null)
        {
            recruiterButton.interactable = interactable;
        }
    }

    private void SetInterviewButtonLabel(string label)
    {
        if (interviewButton == null)
        {
            return;
        }

        TMP_Text text = interviewButton.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = label;
        }
    }

    private void SetLaptopTextAreaLayout(float summaryHeight, float feedbackHeight, int summaryFontSize, int feedbackFontSize)
    {
        if (listingSummaryText != null)
        {
            listingSummaryText.fontSize = summaryFontSize;
            LayoutElement summaryLayout = listingSummaryText.GetComponent<LayoutElement>();
            if (summaryLayout != null)
            {
                summaryLayout.preferredHeight = summaryHeight;
            }
        }

        if (feedbackText != null)
        {
            feedbackText.fontSize = feedbackFontSize;
            LayoutElement feedbackLayout = feedbackText.GetComponent<LayoutElement>();
            if (feedbackLayout != null)
            {
                feedbackLayout.preferredHeight = feedbackHeight;
            }
        }
    }

    private void SetResetButtonLabel(string label)
    {
        if (resetButton == null)
        {
            return;
        }

        TMP_Text text = resetButton.GetComponentInChildren<TMP_Text>();
        if (text != null)
        {
            text.text = label;
        }
    }

    private void SetBoardNavigationState()
    {
        bool onJobBoard = currentState == DeskPrototypeState.JobBoard;
        bool canUseJobBoard = ShouldUseJobBoard();
        bool hasSelectedJob = HasSelectedJob();
        bool completedRun = IsCompletedRoomRunActive();

        SetButtonVisible(backToJobBoardButton, canUseJobBoard && !onJobBoard && !completedRun);
        SetButtonVisible(viewListingButton, !completedRun && (!onJobBoard || hasSelectedJob));
        SetButtonVisible(applicationStrategyButton, !completedRun && !onJobBoard);

        if (viewListingButton != null)
        {
            viewListingButton.interactable = !canUseJobBoard || hasSelectedJob;
        }

        if (applicationStrategyButton != null)
        {
            applicationStrategyButton.interactable = (!canUseJobBoard || hasSelectedJob) && currentState != DeskPrototypeState.JobBoard;
        }

        if (onJobBoard)
        {
            SetConfirmVisible(false);
        }
    }

    private void SetPostOutcomeButtonsVisible(bool visible)
    {
        SetButtonVisible(backToJobBoardButton, !visible && ShouldUseJobBoard() && currentState != DeskPrototypeState.JobBoard);
        SetButtonVisible(viewListingButton, !visible);
        SetButtonVisible(applicationStrategyButton, !visible);
        SetButtonVisible(recruiterButton, !visible);
        SetButtonVisible(interviewButton, !visible);
        SetButtonVisible(clearRoomButton, visible && IsAftermathEntryAvailable());
        SetButtonVisible(processSummaryButton, visible);
        SetButtonVisible(mainMenuButton, visible);
        SetPostAftermathChoicesVisible(visible && IsPostAftermathChoicesAvailable());
        SetResetButtonLabel(visible ? "Start New Run" : "Reset Desk Run");

        if (visible)
        {
            SetConfirmVisible(false);
        }
        else
        {
            SetBoardNavigationState();
        }
    }

    private void SetPostAftermathChoicesVisible(bool visible)
    {
        if (postAftermathChoiceRow != null)
        {
            postAftermathChoiceRow.SetActive(visible);
        }

        SetButtonVisible(applyAgainButton, visible);
        SetButtonVisible(takeBreakButton, visible);
        SetButtonVisible(askFeedbackButton, visible);
        SetButtonVisible(reviewProcessSummaryButton, visible);
    }

    private static void SetButtonVisible(Button button, bool visible)
    {
        if (button != null)
        {
            button.gameObject.SetActive(visible);
        }
    }

    private void SetJobBoardVisible(bool visible)
    {
        if (jobBoardRow != null)
        {
            jobBoardRow.SetActive(visible);
        }

        if (jobCardButtons == null)
        {
            return;
        }

        for (int i = 0; i < jobCardButtons.Length; i++)
        {
            if (jobCardButtons[i] != null)
            {
                jobCardButtons[i].gameObject.SetActive(visible);
            }
        }

        if (visible)
        {
            RefreshJobCardButtonLabels();
        }
    }

    private void RefreshJobCardButtonLabels()
    {
        if (jobCardButtons == null)
        {
            return;
        }

        JobListingData[] jobs = GetAvailableJobListings();
        for (int i = 0; i < jobCardButtons.Length; i++)
        {
            bool hasJob = i < jobs.Length && jobs[i] != null;
            jobCardButtons[i].gameObject.SetActive(currentState == DeskPrototypeState.JobBoard && hasJob);
            if (!hasJob)
            {
                continue;
            }

            TMP_Text label = jobCardButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = BuildJobCardText(jobs[i], IsSelectedJob(jobs[i]));
            }
        }
    }

    private void SelectJobCard(int jobIndex)
    {
        JobListingData[] jobs = GetAvailableJobListings();
        if (jobIndex < 0 || jobIndex >= jobs.Length)
        {
            SetText(feedbackText, "That opportunity is unavailable.");
            return;
        }

        SelectJobFromBoard(jobs[jobIndex]);
    }

    private static bool IsCompletedRoomRunActive()
    {
        return FinalRoundRunState.TryGetActiveState(out CandidateState state)
            && !string.IsNullOrWhiteSpace(state.RoomOutcome);
    }

    private static bool IsAftermathEntryAvailable()
    {
        return FinalRoundRunState.TryGetActiveState(out CandidateState state)
            && IsAftermathEntryAvailable(state);
    }

    private static bool IsPostAftermathChoicesAvailable()
    {
        return FinalRoundRunState.TryGetActiveState(out CandidateState state)
            && state.AftermathCompleted;
    }

    private static bool IsAftermathEntryAvailable(CandidateState state)
    {
        return state != null
            && state.HasActiveDeskRun
            && state.AftermathAvailable
            && !state.AftermathCompleted
            && state.RoomOutcome == nameof(InterviewOutcomeType.Reject);
    }

    private static string FormatId(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "none" : value;
    }

    private static string FormatSigned(int value)
    {
        return value >= 0 ? $"+{value}" : value.ToString();
    }

    private bool ShouldShowJobBoardFirst()
    {
        return ShouldUseJobBoard()
            && !applicationConfirmed
            && !HasSelectedJob();
    }

    private bool ShouldUseJobBoard()
    {
        JobListingData[] jobs = GetAvailableJobListings();
        if (jobs.Length < 3)
        {
            return false;
        }

        for (int i = 0; i < 3; i++)
        {
            if (jobs[i] == null
                || string.IsNullOrWhiteSpace(jobs[i].jobId)
                || string.IsNullOrWhiteSpace(jobs[i].companyName)
                || string.IsNullOrWhiteSpace(jobs[i].roleTitle))
            {
                return false;
            }

            for (int j = i + 1; j < 3; j++)
            {
                if (jobs[j] != null && jobs[i].jobId == jobs[j].jobId)
                {
                    return false;
                }
            }
        }

        return true;
    }

    private bool HasSelectedJob()
    {
        return FinalRoundRunState.TryGetActiveState(out CandidateState state)
            && !string.IsNullOrWhiteSpace(state.SelectedJobId)
            && FindJobListingById(state.SelectedJobId) != null;
    }

    private bool IsSelectedJob(JobListingData job)
    {
        return job != null
            && FinalRoundRunState.TryGetActiveState(out CandidateState state)
            && state.SelectedJobId == job.jobId;
    }

    private static string BuildJobCardText(JobListingData job, bool selected)
    {
        string marker = selected ? "Selected\n" : string.Empty;
        string greenFlag = FormatSingleFlag(job.greenFlags);
        string redFlag = FormatSingleFlag(job.redFlags);
        string recruiter = string.IsNullOrWhiteSpace(job.recruiterName) ? "Recruiter pending" : job.recruiterName;

        return
            $"{marker}{job.companyName}\n" +
            $"{job.roleTitle}\n" +
            $"{FormatId(job.difficultyProfile)}\n" +
            $"Recruiter: {recruiter}\n\n" +
            $"Signal: {greenFlag}\n\n" +
            $"Risk: {redFlag}\n\n" +
            (selected ? "Selected" : "Select Opportunity");
    }

    private static string FormatSingleFlag(string[] flags)
    {
        if (flags == null || flags.Length == 0)
        {
            return "Not specified";
        }

        for (int i = 0; i < flags.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(flags[i]))
            {
                return TruncateForCard(flags[i], 94);
            }
        }

        return "Not specified";
    }

    private static string TruncateForCard(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "Not specified";
        }

        string compact = value.Trim();
        if (compact.Length <= maxLength)
        {
            return compact;
        }

        return compact.Substring(0, Mathf.Max(0, maxLength - 3)).TrimEnd() + "...";
    }

    private string GetActiveJobId()
    {
        JobListingData job = GetActiveJobListing();
        return job != null && !string.IsNullOrWhiteSpace(job.jobId)
            ? job.jobId
            : PlaceholderJobId;
    }

    private string GetRoleLine()
    {
        return $"Role: {GetRoleTitle(GetActiveJobListing())}";
    }

    private string GetCompanyLine()
    {
        return $"Company: {GetCompanyName(GetActiveJobListing())}";
    }

    private void RefreshJobHeaderText()
    {
        if (currentState == DeskPrototypeState.JobBoard)
        {
            SetText(laptopTitleText, "Opportunity Board");
            if (!HasSelectedJob())
            {
                SetText(roleLineText, "Role: Choose an opportunity");
                SetText(companyLineText, "Company: Three active listings");
            }
            else
            {
                SetText(roleLineText, GetRoleLine());
                SetText(companyLineText, GetCompanyLine());
            }

            return;
        }

        SetText(laptopTitleText, $"{GetCompanyName(GetActiveJobListing())} Jobs");
        SetText(roleLineText, GetRoleLine());
        SetText(companyLineText, GetCompanyLine());
    }

    private JobListingData GetActiveJobListing()
    {
        if (selectedJobListing != null)
        {
            return selectedJobListing;
        }

        if (FinalRoundRunState.TryGetActiveState(out CandidateState state) && !string.IsNullOrWhiteSpace(state.SelectedJobId))
        {
            selectedJobListing = FindJobListingById(state.SelectedJobId);
            if (selectedJobListing != null)
            {
                return selectedJobListing;
            }
        }

        selectedJobListing = GetFallbackJobListing();
        return selectedJobListing;
    }

    private JobListingData GetFallbackJobListing()
    {
        if (defaultJobListing != null)
        {
            return defaultJobListing;
        }

        JobListingData[] jobs = GetAvailableJobListings();
        return jobs.Length > 0 ? jobs[0] : null;
    }

    private JobListingData[] GetAvailableJobListings()
    {
        JobListingData[] assignedJobs = GetAssignedJobListings();
        if (assignedJobs.Length >= 3)
        {
            return assignedJobs;
        }

        JobListingData[] resourceJobs = GetResourceJobListings();
        if (resourceJobs.Length > 0)
        {
            return resourceJobs;
        }

        if (assignedJobs.Length > 0)
        {
            return assignedJobs;
        }

        return defaultJobListing != null ? new[] { defaultJobListing } : System.Array.Empty<JobListingData>();
    }

    private JobListingData[] GetAssignedJobListings()
    {
        if (availableJobListings == null || availableJobListings.Length == 0)
        {
            return System.Array.Empty<JobListingData>();
        }

        int count = 0;
        for (int i = 0; i < availableJobListings.Length; i++)
        {
            if (availableJobListings[i] != null)
            {
                count++;
            }
        }

        if (count == 0)
        {
            return System.Array.Empty<JobListingData>();
        }

        JobListingData[] jobs = new JobListingData[count];
        int index = 0;
        for (int i = 0; i < availableJobListings.Length; i++)
        {
            if (availableJobListings[i] != null)
            {
                jobs[index++] = availableJobListings[i];
            }
        }

        return jobs;
    }

    private JobListingData[] GetResourceJobListings()
    {
        if (cachedResourceJobListings != null)
        {
            return cachedResourceJobListings;
        }

        JobListingData[] loadedJobs = Resources.LoadAll<JobListingData>(Vs4JobListingsResourcePath);
        if (loadedJobs == null || loadedJobs.Length == 0)
        {
            cachedResourceJobListings = System.Array.Empty<JobListingData>();
            return cachedResourceJobListings;
        }

        JobListingData northbridge = FindJobInArray(loadedJobs, NorthbridgeJobId);
        JobListingData helios = FindJobInArray(loadedJobs, HeliosJobId);
        JobListingData redgate = FindJobInArray(loadedJobs, RedgateJobId);

        if (northbridge != null && helios != null && redgate != null)
        {
            cachedResourceJobListings = new[] { northbridge, helios, redgate };
            return cachedResourceJobListings;
        }

        int count = 0;
        for (int i = 0; i < loadedJobs.Length; i++)
        {
            if (loadedJobs[i] != null)
            {
                count++;
            }
        }

        if (count == 0)
        {
            cachedResourceJobListings = System.Array.Empty<JobListingData>();
            return cachedResourceJobListings;
        }

        cachedResourceJobListings = new JobListingData[count];
        int index = 0;
        for (int i = 0; i < loadedJobs.Length; i++)
        {
            if (loadedJobs[i] != null)
            {
                cachedResourceJobListings[index++] = loadedJobs[i];
            }
        }

        return cachedResourceJobListings;
    }

    private static JobListingData FindJobInArray(JobListingData[] jobs, string jobId)
    {
        if (jobs == null || string.IsNullOrWhiteSpace(jobId))
        {
            return null;
        }

        for (int i = 0; i < jobs.Length; i++)
        {
            if (jobs[i] != null && jobs[i].jobId == jobId)
            {
                return jobs[i];
            }
        }

        return null;
    }

    private JobListingData FindJobListingById(string jobId)
    {
        if (string.IsNullOrWhiteSpace(jobId))
        {
            return null;
        }

        JobListingData[] jobs = GetAvailableJobListings();
        for (int i = 0; i < jobs.Length; i++)
        {
            if (jobs[i] != null && jobs[i].jobId == jobId)
            {
                return jobs[i];
            }
        }

        if (defaultJobListing != null && defaultJobListing.jobId == jobId)
        {
            return defaultJobListing;
        }

        return null;
    }

    private void EnsureSelectedJobForState(CandidateState state)
    {
        if (state == null)
        {
            return;
        }

        JobListingData job = !string.IsNullOrWhiteSpace(state.SelectedJobId)
            ? FindJobListingById(state.SelectedJobId)
            : null;

        SelectJobForRun(job ?? GetActiveJobListing(), state);
    }

    private void SelectJobForRun(JobListingData job, CandidateState state)
    {
        if (state == null)
        {
            return;
        }

        selectedJobListing = job ?? GetFallbackJobListing();
        string jobId = selectedJobListing != null && !string.IsNullOrWhiteSpace(selectedJobListing.jobId)
            ? selectedJobListing.jobId
            : PlaceholderJobId;

        state.SelectedJobId = jobId;
        ApplyJobDefaultDeltasOnce(state, selectedJobListing, jobId);
        RefreshJobHeaderText();
    }

    private static void ApplyJobDefaultDeltasOnce(CandidateState state, JobListingData job, string jobId)
    {
        if (state == null || job == null || string.IsNullOrWhiteSpace(jobId))
        {
            return;
        }

        if (state.JobDefaultDeltasAppliedJobId == jobId)
        {
            return;
        }

        state.ApplyDeltas(
            job.defaultRoleFitDelta,
            job.defaultRecruiterTrustDelta,
            job.defaultCandidateConfidenceDelta,
            job.defaultEnergyDelta,
            job.defaultOverclaimRiskDelta,
            job.defaultTechnicalReadinessDelta,
            job.defaultRapportMomentumDelta);
        state.JobDefaultDeltasAppliedJobId = jobId;

        Debug.Log(
            $"Final Round P41: selected job '{jobId}' and applied default job deltas once.\n" +
            BuildJobDefaultDeltaSummary(job) +
            "\n" +
            state.BuildDebugSummary());
    }

    private static string BuildJobDefaultDeltaSummary(JobListingData job)
    {
        if (job == null)
        {
            return "No JobListingData defaults available.";
        }

        return
            $"Job defaults: Role Fit {FormatSigned(job.defaultRoleFitDelta)}, " +
            $"Recruiter Trust {FormatSigned(job.defaultRecruiterTrustDelta)}, " +
            $"Confidence {FormatSigned(job.defaultCandidateConfidenceDelta)}, " +
            $"Energy {FormatSigned(job.defaultEnergyDelta)}, " +
            $"Overclaim {FormatSigned(job.defaultOverclaimRiskDelta)}, " +
            $"Tech Ready {FormatSigned(job.defaultTechnicalReadinessDelta)}, " +
            $"Rapport {FormatSigned(job.defaultRapportMomentumDelta)}";
    }

    private static string GetCompanyName(JobListingData job)
    {
        return job != null && !string.IsNullOrWhiteSpace(job.companyName)
            ? job.companyName
            : PlaceholderCompany;
    }

    private static string GetRoleTitle(JobListingData job)
    {
        return job != null && !string.IsNullOrWhiteSpace(job.roleTitle)
            ? job.roleTitle
            : PlaceholderRole;
    }

    private static string GetDifficultyProfile(JobListingData job)
    {
        return job != null && !string.IsNullOrWhiteSpace(job.difficultyProfile)
            ? job.difficultyProfile
            : "Balanced baseline";
    }

    private string GetRecruiterName()
    {
        return GetRecruiterName(GetActiveJobListing());
    }

    private static string GetRecruiterName(JobListingData job)
    {
        return job != null && !string.IsNullOrWhiteSpace(job.recruiterName)
            ? job.recruiterName
            : RecruiterSender;
    }

    private string GetRecruiterTitle()
    {
        JobListingData job = GetActiveJobListing();
        return job != null && !string.IsNullOrWhiteSpace(job.recruiterTitle)
            ? job.recruiterTitle
            : "Senior Talent Partner";
    }

    private string GetRecruiterCompany()
    {
        return GetRecruiterCompany(GetActiveJobListing());
    }

    private static string GetRecruiterCompany(JobListingData job)
    {
        return job != null && !string.IsNullOrWhiteSpace(job.recruiterCompany)
            ? job.recruiterCompany
            : GetCompanyName(job);
    }

    private string GetRecruiterSubject()
    {
        JobListingData job = GetActiveJobListing();
        return $"{GetRecruiterCompany(job)} - quick screen";
    }

    private static string GetOutcomeContextLine(JobListingData job)
    {
        return job != null ? job.outcomeContextLine : string.Empty;
    }

    private static string GetProcessSummaryNote(JobListingData job)
    {
        return job != null ? job.processSummaryNote : string.Empty;
    }

    private static string FormatOptionalLine(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim() + "\n\n";
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
        JobListingData job = GetActiveJobListing();
        return job != null && !string.IsNullOrWhiteSpace(job.summary)
            ? job.summary
            : "Northbridge Cyber Systems is hiring a customer-facing security presales engineer to guide enterprise buyers through architecture, proof-of-value workshops, and executive risk conversations. The role sounds senior, visible, and useful, but the listing leaves some room for interpretation around workload, travel, and how mature the process really is.";
    }

    private string[] GetResponsibilities()
    {
        JobListingData job = GetActiveJobListing();
        return job != null && job.responsibilities != null && job.responsibilities.Length > 0
            ? job.responsibilities
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
        JobListingData job = GetActiveJobListing();
        return job != null && job.requirements != null && job.requirements.Length > 0
            ? job.requirements
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
        JobListingData job = GetActiveJobListing();
        return job != null && job.niceToHaves != null && job.niceToHaves.Length > 0
            ? job.niceToHaves
            : new[]
            {
                "SOC tooling or incident response background.",
                "Experience building demo narratives for skeptical enterprise buyers.",
                "Familiarity with data residency, compliance, and executive security reporting."
            };
    }

    private string GetSalaryRange()
    {
        JobListingData job = GetActiveJobListing();
        return job != null && !string.IsNullOrWhiteSpace(job.salaryRange)
            ? job.salaryRange
            : DefaultSalaryRange;
    }

    private string GetProcessNotes()
    {
        JobListingData job = GetActiveJobListing();
        return job != null && !string.IsNullOrWhiteSpace(job.processNotes)
            ? job.processNotes
            : DefaultProcessNotes;
    }

    private string[] GetRedFlags()
    {
        JobListingData job = GetActiveJobListing();
        return job != null && job.redFlags != null && job.redFlags.Length > 0
            ? job.redFlags
            : new[]
            {
                "The listing says fast-paced without clarifying travel, after-hours workshops, or escalation load.",
                "Compensation and success measures are described broadly rather than concretely.",
                "The role appears to sit between Sales, Product, and Security with unclear ownership boundaries."
            };
    }

    private string[] GetGreenFlags()
    {
        JobListingData job = GetActiveJobListing();
        return job != null && job.greenFlags != null && job.greenFlags.Length > 0
            ? job.greenFlags
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

    private string GetRecruiterIntroMessage()
    {
        return recruiterIntroMessage != null && !string.IsNullOrWhiteSpace(recruiterIntroMessage.messageText)
            ? recruiterIntroMessage.messageText
            : RecruiterIntroMessage;
    }

    private RecruiterScreenPrompt[] GetRecruiterPrompts()
    {
        if (authoredRecruiterQuestions != null && authoredRecruiterQuestions.Length > 0)
        {
            int validPromptCount = 0;
            for (int i = 0; i < authoredRecruiterQuestions.Length; i++)
            {
                if (authoredRecruiterQuestions[i] != null)
                {
                    validPromptCount++;
                }
            }

            if (validPromptCount == authoredRecruiterQuestions.Length)
            {
                RecruiterScreenPrompt[] prompts = new RecruiterScreenPrompt[authoredRecruiterQuestions.Length];
                for (int i = 0; i < authoredRecruiterQuestions.Length; i++)
                {
                    prompts[i] = FromAsset(authoredRecruiterQuestions[i]);
                }

                return prompts;
            }

            Debug.LogWarning("Final Round P29: authored recruiter questions contain null entries. Falling back to built-in recruiter screen content.");
        }

        if (fallbackRecruiterPrompts == null || fallbackRecruiterPrompts.Length == 0)
        {
            fallbackRecruiterPrompts = BuildFallbackRecruiterPrompts();
        }

        return fallbackRecruiterPrompts;
    }

    private static RecruiterScreenPrompt FromAsset(RecruiterScreenQuestionData asset)
    {
        if (asset == null || string.IsNullOrWhiteSpace(asset.questionId) || string.IsNullOrWhiteSpace(asset.prompt))
        {
            Debug.LogWarning("Final Round P29: recruiter question asset is missing required fields. Using first fallback prompt.");
            return BuildFallbackRecruiterPrompts()[0];
        }

        RecruiterScreenChoice[] choices = new RecruiterScreenChoice[asset.responseChoices != null ? asset.responseChoices.Length : 0];
        for (int i = 0; i < choices.Length; i++)
        {
            choices[i] = FromAsset(asset.responseChoices[i]);
        }

        return new RecruiterScreenPrompt
        {
            questionId = asset.questionId,
            prompt = asset.prompt,
            responseChoices = choices
        };
    }

    private static RecruiterScreenChoice FromAsset(RecruiterScreenResponseChoice asset)
    {
        if (asset == null || string.IsNullOrWhiteSpace(asset.choiceId) || string.IsNullOrWhiteSpace(asset.label))
        {
            return BuildFallbackRecruiterPrompts()[0].responseChoices[0];
        }

        return new RecruiterScreenChoice
        {
            choiceId = asset.choiceId,
            label = asset.label,
            responseText = asset.responseText,
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

    public static RecruiterScreenPrompt[] BuildFallbackRecruiterPrompts()
    {
        return new[]
        {
            new RecruiterScreenPrompt
            {
                questionId = "REC-AVAILABILITY",
                prompt = "Are you available for a short screen this week?",
                responseChoices = new[]
                {
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-AVAIL-CLEAR",
                        label = "Clear Availability",
                        responseText = "Yes. I can do Tuesday or Thursday afternoon, and I can send a few other options if helpful.",
                        feedbackText = "Maya has enough signal to schedule without chasing you.",
                        recruiterTrustDelta = 2,
                        candidateConfidenceDelta = 1,
                        energyDelta = 0,
                        rapportMomentumDelta = 1
                    },
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-AVAIL-EAGER",
                        label = "Immediately Free",
                        responseText = "I can talk any time today, tomorrow, or whenever the team wants. I am very keen.",
                        feedbackText = "The enthusiasm helps, but it reads slightly uncalibrated.",
                        recruiterTrustDelta = 0,
                        candidateConfidenceDelta = 1,
                        energyDelta = -1,
                        rapportMomentumDelta = 0
                    },
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-AVAIL-CAUTIOUS",
                        label = "Cautious Window",
                        responseText = "Possibly. I would need to check my week and understand roughly what the screen covers first.",
                        feedbackText = "The caution is reasonable, but Maya has to do a little more work to move things forward.",
                        recruiterTrustDelta = -1,
                        candidateConfidenceDelta = 0,
                        energyDelta = 1,
                        rapportMomentumDelta = 0
                    },
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-AVAIL-SLOW",
                        label = "Slow Response",
                        responseText = "Maybe later in the week. I am not sure yet.",
                        feedbackText = "Maya can keep you in process, but the momentum cools.",
                        recruiterTrustDelta = -2,
                        candidateConfidenceDelta = -1,
                        energyDelta = -1,
                        rapportMomentumDelta = -1
                    }
                }
            },
            new RecruiterScreenPrompt
            {
                questionId = "REC-FIT",
                prompt = "How would you summarise your fit for a security presales role like this?",
                responseChoices = new[]
                {
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-FIT-BALANCED",
                        label = "Balanced Fit",
                        responseText = "I am strongest where customer conversations, security architecture, and practical trade-offs meet.",
                        feedbackText = "Maya gets a clean, credible positioning line to carry forward.",
                        roleFitDelta = 2,
                        recruiterTrustDelta = 2,
                        candidateConfidenceDelta = 1,
                        technicalReadinessDelta = 1,
                        rapportMomentumDelta = 1
                    },
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-FIT-TECHNICAL",
                        label = "Technical Depth",
                        responseText = "The technical side is the strongest match. I can go deep on detection, architecture, and risk.",
                        feedbackText = "The technical signal is useful, though Maya may still need evidence that you can sell the story.",
                        roleFitDelta = 1,
                        recruiterTrustDelta = 1,
                        candidateConfidenceDelta = 1,
                        technicalReadinessDelta = 2,
                        rapportMomentumDelta = -1
                    },
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-FIT-PERFECT",
                        label = "Perfect Match",
                        responseText = "Honestly, it sounds like exactly what I have already been doing end to end.",
                        feedbackText = "It is punchy, but it creates proof pressure for later.",
                        roleFitDelta = 2,
                        recruiterTrustDelta = -1,
                        candidateConfidenceDelta = 2,
                        overclaimRiskDelta = 2,
                        rapportMomentumDelta = -1
                    },
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-FIT-MODEST",
                        label = "Modest Fit",
                        responseText = "Some parts fit well, though I would probably need to grow into the full presales side.",
                        feedbackText = "The honesty helps, but the answer may undersell your readiness.",
                        roleFitDelta = -1,
                        recruiterTrustDelta = 1,
                        candidateConfidenceDelta = -2,
                        overclaimRiskDelta = -1,
                        rapportMomentumDelta = 1
                    }
                }
            },
            new RecruiterScreenPrompt
            {
                questionId = "REC-PROCESS",
                prompt = "The team may move quickly if there is alignment. Are you comfortable with a technical panel and final-round customer scenario?",
                responseChoices = new[]
                {
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-PROCESS-REALISTIC",
                        label = "Realistic Yes",
                        responseText = "Yes. I am comfortable with that, and I would want to understand the customer scenario format before the final round.",
                        feedbackText = "Maya reads this as confident without pretending the process is trivial.",
                        roleFitDelta = 1,
                        recruiterTrustDelta = 2,
                        candidateConfidenceDelta = 1,
                        technicalReadinessDelta = 1,
                        rapportMomentumDelta = 1
                    },
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-PROCESS-OVERCONFIDENT",
                        label = "No Problem",
                        responseText = "Absolutely. I have handled much tougher panels, so I am not worried about it.",
                        feedbackText = "The confidence lands, but it also raises the bar for the panel.",
                        recruiterTrustDelta = -1,
                        candidateConfidenceDelta = 2,
                        overclaimRiskDelta = 2,
                        rapportMomentumDelta = -1
                    },
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-PROCESS-CLARIFY",
                        label = "Clarify Format",
                        responseText = "Yes, and it would help to know whether they are testing discovery, architecture, or executive communication.",
                        feedbackText = "The clarification is useful and makes you sound prepared rather than difficult.",
                        recruiterTrustDelta = 2,
                        candidateConfidenceDelta = 1,
                        energyDelta = -1,
                        technicalReadinessDelta = 2,
                        rapportMomentumDelta = 1
                    },
                    new RecruiterScreenChoice
                    {
                        choiceId = "REC-PROCESS-HESITANT",
                        label = "Hesitant",
                        responseText = "I can probably do it, but I would need to know exactly what they are looking for before agreeing.",
                        feedbackText = "Maya may still move you forward, but the answer carries uncertainty.",
                        roleFitDelta = -1,
                        recruiterTrustDelta = -2,
                        candidateConfidenceDelta = -2,
                        energyDelta = -1,
                        rapportMomentumDelta = -1
                    }
                }
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
        if (strategyRow != null)
        {
            strategyRow.SetActive(visible);
        }

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

    private void SetRecruiterChoiceButtonsVisible(bool visible)
    {
        if (recruiterRow != null)
        {
            recruiterRow.SetActive(visible);
        }

        if (recruiterChoiceButtons == null)
        {
            return;
        }

        for (int i = 0; i < recruiterChoiceButtons.Length; i++)
        {
            if (recruiterChoiceButtons[i] != null)
            {
                recruiterChoiceButtons[i].gameObject.SetActive(visible);
            }
        }

        if (visible)
        {
            RefreshRecruiterChoiceButtonLabels();
        }
    }

    private void SetListingSectionButtonsVisible(bool visible)
    {
        if (listingSectionRow != null)
        {
            listingSectionRow.SetActive(visible);
        }

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

    private void RefreshRecruiterChoiceButtonLabels()
    {
        if (recruiterChoiceButtons == null)
        {
            return;
        }

        RecruiterScreenPrompt[] prompts = GetRecruiterPrompts();
        RecruiterScreenChoice[] choices = prompts.Length > 0
            ? prompts[Mathf.Clamp(currentRecruiterPromptIndex, 0, prompts.Length - 1)].responseChoices
            : null;

        for (int i = 0; i < recruiterChoiceButtons.Length; i++)
        {
            bool hasChoice = choices != null && i < choices.Length && choices[i] != null;
            recruiterChoiceButtons[i].gameObject.SetActive(hasChoice && currentState == DeskPrototypeState.Recruiter && !recruiterCompleted);
            if (!hasChoice)
            {
                continue;
            }

            TMP_Text label = recruiterChoiceButtons[i].GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                label.text = choices[i].label;
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

    private void SetConfirmVisible(bool visible)
    {
        if (confirmApplicationButtonObject != null)
        {
            confirmApplicationButtonObject.SetActive(visible);
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

    private bool WasDebugTogglePressed()
    {
        if (!allowDeskDebugToggle)
        {
            return false;
        }

#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F1);
#endif
    }

    private static bool WasPauseTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
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

    private static void ConfigureJobCardButton(Button button)
    {
        if (button == null)
        {
            return;
        }

        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        if (text == null)
        {
            return;
        }

        text.alignment = TextAlignmentOptions.TopLeft;
        text.fontSize = 12;
        text.fontStyle = FontStyles.Normal;
        text.lineSpacing = -8f;
        text.overflowMode = TextOverflowModes.Truncate;

        RectTransform textRect = text.rectTransform;
        textRect.offsetMin = new Vector2(14f, 10f);
        textRect.offsetMax = new Vector2(-14f, -10f);
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
