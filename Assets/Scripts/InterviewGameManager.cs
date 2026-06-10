using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
#endif

public class InterviewGameManager : MonoBehaviour
{
    private const int StartingConfidence = 60;
    private const int StartingEnergy = 70;
    private const int StartingTechnicalCredibility = 50;
    private const int StartingCommercialAlignment = 50;
    private const int StartingInterviewPressure = 35;
    private const float BetweenStageEventChance = 0.6f;
    private const float ScreenFadeDuration = 0.16f;
    private const string BuildVersion = "Room Prototype P21";
    private const int MaxDisplayedRunBadges = 4;
    private const string PrefMasterVolume = "FinalRound.MasterVolume";
    private const string PrefSfxVolume = "FinalRound.SfxVolume";
    private const string PrefMuteAudio = "FinalRound.MuteAudio";
    private const string PrefReduceMotion = "FinalRound.ReduceMotion";
    private const string PrefDeterministicRunSeed = "FinalRound.DeterministicRunSeed";
    private const string PrefRunSeed = "FinalRound.RunSeed";
    private const string PrefFullscreen = "FinalRound.Fullscreen";

    private static InterviewGameManager activeManager;

    private readonly Color backgroundColor = InterviewRoomBackdropController.debugBackdropVisibility
        ? new Color32(10, 12, 18, 92)
        : new Color32(10, 12, 18, 245);
    private readonly Color panelColor = InterviewRoomBackdropController.debugBackdropVisibility
        ? new Color32(24, 28, 39, 218)
        : new Color32(24, 28, 39, 248);
    private readonly Color panelAccentColor = InterviewRoomBackdropController.debugBackdropVisibility
        ? new Color32(34, 40, 55, 220)
        : new Color32(34, 40, 55, 248);
    private readonly Color buttonColor = InterviewRoomBackdropController.debugBackdropVisibility
        ? new Color32(48, 60, 82, 232)
        : new Color32(48, 60, 82, 250);
    private readonly Color buttonHoverColor = new Color32(67, 84, 116, 255);
    private readonly Color disabledAnswerColor = new Color32(30, 35, 46, 220);
    private readonly Color selectedAnswerColor = new Color32(78, 130, 118, 255);
    private readonly Color transitionPanelColor = InterviewRoomBackdropController.debugBackdropVisibility
        ? new Color32(18, 34, 46, 220)
        : new Color32(18, 34, 46, 248);
    private readonly Color textColor = new Color32(238, 242, 248, 255);
    private readonly Color mutedTextColor = new Color32(172, 181, 196, 255);
    private readonly Color accentColor = new Color32(92, 188, 164, 255);
    private readonly Color positiveStatColor = new Color32(122, 224, 159, 255);
    private readonly Color negativeStatColor = new Color32(255, 139, 139, 255);
    private readonly Color neutralStatColor = new Color32(194, 202, 214, 255);
    private readonly Color clarifiedAnswerColor = new Color32(50, 78, 74, 255);

    private TMP_Text subtitleText;
    private TMP_Text progressText;
    private TMP_Text questionStageNameText;
    private TMP_Text questionStageIntroText;
    private TMP_Text questionText;
    private TMP_Text statsText;
    private TMP_Text feedbackText;
    private TMP_Text feedbackPrepCardText;
    private TMP_Text stageTransitionNameText;
    private TMP_Text stageTransitionBodyText;
    private TMP_Text stageTransitionStatsText;
    private TMP_Text recoveryChoiceTitleText;
    private TMP_Text recoveryChoiceBodyText;
    private TMP_Text recoveryChoiceStatsText;
    private TMP_Text[] recoveryChoiceButtonTexts;
    private TMP_Text randomEventTitleText;
    private TMP_Text randomEventBodyText;
    private TMP_Text randomEventChangesText;
    private TMP_Text menuBodyText;
    private TMP_Text processBriefingTitleText;
    private TMP_Text processBriefingBodyText;
    private TMP_Text[] prepCardTexts;
    private TMP_Text outcomeTitleText;
    private TMP_Text outcomeBodyText;
    private TMP_Text outcomeStatsText;
    private TMP_Text outcomeHighlightsText;
    private TMP_Text outcomeBadgesText;
    private TMP_Text outcomeAdviceText;
    private TMP_Text versionText;

    private Button[] answerButtons;
    private TMP_Text[] answerButtonTexts;
    private Button continueButton;
    private Button stageContinueButton;
    private Button[] recoveryChoiceButtons;
    private Button recoveryChoiceContinueButton;
    private Button randomEventContinueButton;
    private Button startInterviewButton;
    private Button howToPlayButton;
    private Button aboutButton;
    private Button settingsButton;
    private Button quitButton;
    private Button beginProcessButton;
    private Button processBriefingReturnButton;
    private Button[] prepCardButtons;
    private Button resumeButton;
    private Button pauseSettingsButton;
    private Button pauseReturnToMenuButton;
    private Button settingsBackButton;
    private Button settingsResetDefaultsButton;
    private Button applySeedButton;
    private Button newRandomSeedButton;
    private GameObject feedbackPanel;
    private GameObject prepCardsPanel;
    private GameObject menuScreen;
    private GameObject processBriefingScreen;
    private GameObject questionScreen;
    private GameObject stageTransitionScreen;
    private GameObject recoveryChoiceScreen;
    private GameObject recoveryChoiceButtonColumn;
    private GameObject randomEventScreen;
    private GameObject outcomeScreen;
    private GameObject runtimeBackgroundPanel;
    private GameObject outcomeStatsPanel;
    private GameObject outcomeHighlightsPanel;
    private GameObject outcomeBadgesPanel;
    private GameObject outcomeAdvicePanel;
    private GameObject outcomeButtonRow;
    private GameObject pauseOverlay;
    private GameObject settingsOverlay;
    private GameObject backdropViewportPanel;
    private GameObject callPanelRoot;
    private GameObject callStandbyCard;
    private Image callPanelBackgroundImage;
    private Image callPanelFrameImage;
    private Image callTopBarImage;
    private TMP_Text callStageLabelText;
    private TMP_Text callStatusLabelText;
    private TMP_Text callStandbyText;
    private TMP_Text masterVolumeValueText;
    private TMP_Text sfxVolumeValueText;
    private TMP_Text seedNoteText;
    private CallParticipantTile[] callParticipantTiles;
    private InterviewRoomBackdropController roomBackdrop;
    private AudioSource uiAudioSource;
    private FinalRoundAudioManager audioManager;
    private Coroutine activeFadeCoroutine;
    private Coroutine answerEntranceCoroutine;
    private Coroutine answerSelectionCoroutine;
    private Coroutine feedbackRevealCoroutine;
    private Coroutine statsFlashCoroutine;
    private Coroutine finalRevealCoroutine;
    private Color statsBaseColor;

    [Header("Audio Feedback")]
    [Range(0f, 1f)]
    [SerializeField] private float masterVolume = 0.65f;
    [Range(0f, 1f)]
    [SerializeField] private float sfxVolume = 0.8f;
    [SerializeField] private bool muteAudio;
    [SerializeField] private AudioClip uiClickClip;
    [SerializeField] private AudioClip answerSelectedClip;
    [SerializeField] private AudioClip prepCardUsedClip;
    [SerializeField] private AudioClip recoveryChoiceSelectedClip;
    [SerializeField] private AudioClip continueClip;
    [SerializeField] private AudioClip randomEventClip;
    [SerializeField] private AudioClip stageCompleteClip;
    [SerializeField] private AudioClip pressureWarningClip;
    [SerializeField] private AudioClip finalPositiveOutcomeClip;
    [SerializeField] private AudioClip finalNegativeOutcomeClip;
    [SerializeField] private AudioClip badgeRevealClip;
    [SerializeField] private AudioClip finalOutcomeClip;

    [Header("Debug Options")]
    [SerializeField] private bool reduceMotion;
    [SerializeField] private bool randomizeAnswerOrder = true;
    [SerializeField] private bool useCompanyProfileModifiers = true;
    [SerializeField] private bool debugAllowRepeatedRandomEvents;
    [SerializeField] private bool useDebugCompanyProfile;
    [SerializeField] private int debugCompanyProfileIndex;
    [SerializeField] private bool useDeterministicRunSeed;
    [SerializeField] private int debugRunSeed = 48291;
    [SerializeField] private bool useDeterministicAnswerSeed;
    [SerializeField] private int debugAnswerSeed = 12345;

    private readonly PlayerStats playerStats = new PlayerStats();
    private readonly InterviewStyleTracker styleTracker = new InterviewStyleTracker();
    private int currentStageIndex;
    private int currentQuestionIndex;
    private PlayerStats stageStartStats;
    private bool currentStageWasStrong;
    private AnswerOption[] displayedAnswers;
    private System.Random runRandom;
    private int currentRunSeed;
    private string currentProcessId;
    private readonly List<StageRunSummary> stageRunSummaries = new List<StageRunSummary>();
    private readonly List<RandomEventRunSummary> randomEventRunSummaries = new List<RandomEventRunSummary>();
    private readonly HashSet<int> usedRandomEventIndexes = new HashSet<int>();
    private RecoveryChoice[] recoveryChoices;
    private int recoveryChoicesMadeCount;
    private int doomScrollChoiceCount;
    private PrepCard[] prepCards;
    private int prepCardsUsedCount;
    private int reframedCommercialAlignmentBonus;
    private int clarifiedAnswerIndex = -1;
    private string prepCardFeedbackNote;
    private int strongAnswerCount;
    private int riskyAnswerCount;
    private int interviewPressure;
    private string activeCallStageName = "Main Menu";
    private bool callInterviewerSpeaking;
    private bool firstChaoticAnswerBonusApplied;
    private bool highPressureWarningPlayed;
    private bool settingsOpenedFromPause;
    private bool fullscreenEnabled;
    private Slider masterVolumeSlider;
    private Slider sfxVolumeSlider;
    private Toggle muteAudioToggle;
    private Toggle reduceMotionToggle;
    private Toggle deterministicSeedToggle;
    private Toggle fullscreenToggle;
    private TMP_InputField seedInputField;
    private readonly List<string> activeRuleRunNotes = new List<string>();

    private InterviewStage[] stages;
    private InterviewQuestion[][] selectedStageQuestions;
    private RandomInterviewEvent[] randomEvents;
    private CompanyProfile[] companyProfiles;
    private CompanyProfile activeCompanyProfile;

    private void Awake()
    {
        if (activeManager != null && activeManager != this)
        {
            enabled = false;
            return;
        }

        activeManager = this;
    }

    private void Start()
    {
        if (!enabled)
        {
            return;
        }

        BuildStages();
        BuildRandomEvents();
        BuildCompanyProfiles();
        LoadPlayerSettings();

        if (!ValidateGameData())
        {
            enabled = false;
            return;
        }

        ResetGame(true);
        CleanupLegacySceneObjects();
        EnsureUiOnlyDisplayCamera();
        EnsureAudioSourceExists();
        LogDemoStart();

        if (!HasRequiredUi())
        {
            CreateRuntimeUi();
        }

        if (!HasRequiredUi())
        {
            Debug.LogError("Final Round setup error: runtime UI could not be created completely.");
            enabled = false;
            return;
        }

        ShowMenu();
    }

    private void Update()
    {
        HandleKeyboardShortcuts();
        AnimateCallPanel();
    }

    private void CleanupLegacySceneObjects()
    {
        string[] legacyObjectNames =
        {
            "Geometry",
            "BackgroundMesh",
            "Platform",
            "UnityMaterialBall_Gold",
            "SamplesSpotlight",
            "SamplesSpotlightModel",
            "SamplesFloorSpotlight",
            "SamplesFixture",
            "StaticLightingSky",
            "Adaptive Probe Volume",
            "ProbeVolumePerSceneData",
            "Reflection Probe",
            "Lighting",
            "Final Round 3D Backdrop",
            "Interview Room Backdrop",
            "Final Round Generated Interview Room",
            "Final Round Backdrop Camera",
            "Backdrop Camera",
            "Animated Video Call Viewport"
        };

        for (int i = 0; i < legacyObjectNames.Length; i++)
        {
            DestroyObjectsNamed(legacyObjectNames[i]);
        }
    }

    private void DestroyObjectsNamed(string objectName)
    {
        GameObject[] sceneObjects = FindObjectsByType<GameObject>(FindObjectsInactive.Include);

        for (int i = 0; i < sceneObjects.Length; i++)
        {
            GameObject target = sceneObjects[i];
            if (target.name == objectName && target != gameObject)
            {
                Destroy(target);
            }
        }
    }

    private void EnsureUiOnlyDisplayCamera()
    {
        Camera[] cameras = FindObjectsByType<Camera>(FindObjectsInactive.Include);
        Camera mainCamera = null;

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (candidate.targetTexture == null && candidate.gameObject.name == "Main Camera")
            {
                mainCamera = candidate;
                break;
            }
        }

        if (mainCamera == null)
        {
            GameObject cameraObject = new GameObject("Main Camera", typeof(Camera));
            mainCamera = cameraObject.GetComponent<Camera>();
        }

        mainCamera.gameObject.name = "Main Camera";
        mainCamera.gameObject.tag = "MainCamera";
        mainCamera.enabled = true;
        mainCamera.targetTexture = null;
        mainCamera.targetDisplay = 0;
        mainCamera.clearFlags = CameraClearFlags.SolidColor;
        mainCamera.backgroundColor = new Color32(8, 10, 15, 255);
        if (IsRoomPrototypeScene())
        {
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = new Color32(18, 20, 24, 255);
            mainCamera.cullingMask = ~0;
            mainCamera.depth = 0f;
        }
        else
        {
            mainCamera.cullingMask = 0;
            mainCamera.depth = -10f;
            mainCamera.transform.position = new Vector3(0f, 0f, -10f);
            mainCamera.transform.rotation = Quaternion.identity;
        }

        for (int i = 0; i < cameras.Length; i++)
        {
            Camera candidate = cameras[i];
            if (candidate == mainCamera)
            {
                continue;
            }

            candidate.enabled = false;
            candidate.gameObject.tag = "Untagged";
        }
    }

    private void EnsureRoomBackdropExists()
    {
        roomBackdrop = FindAnyObjectByType<InterviewRoomBackdropController>();

        if (roomBackdrop != null)
        {
            return;
        }

        GameObject backdropObject = new GameObject("Final Round Interview Room Backdrop Controller");
        roomBackdrop = backdropObject.AddComponent<InterviewRoomBackdropController>();
    }

    private void EnsureAudioSourceExists()
    {
        uiAudioSource = GetComponent<AudioSource>();

        if (uiAudioSource == null)
        {
            uiAudioSource = gameObject.AddComponent<AudioSource>();
        }

        uiAudioSource.playOnAwake = false;

        audioManager = GetComponent<FinalRoundAudioManager>();
        if (audioManager == null)
        {
            audioManager = gameObject.AddComponent<FinalRoundAudioManager>();
        }

        audioManager.Configure(uiAudioSource, masterVolume, sfxVolume, muteAudio);
        audioManager.SetClipOverride(FinalRoundSoundEvent.UiClick, uiClickClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.AnswerSelected, answerSelectedClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.PrepCardUsed, prepCardUsedClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.RecoveryChoiceSelected, recoveryChoiceSelectedClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.Continue, continueClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.RandomEventAppears, randomEventClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.StageComplete, stageCompleteClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.PressureWarning, pressureWarningClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.FinalPositiveOutcome, finalPositiveOutcomeClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.FinalNegativeOutcome, finalNegativeOutcomeClip);
        audioManager.SetClipOverride(FinalRoundSoundEvent.BadgeReveal, badgeRevealClip);
    }

    private void OnDestroy()
    {
        if (activeManager == this)
        {
            activeManager = null;
        }
    }

    private bool HasRequiredUi()
    {
        return menuScreen != null
            && startInterviewButton != null
            && howToPlayButton != null
            && aboutButton != null
            && settingsButton != null
            && processBriefingScreen != null
            && processBriefingTitleText != null
            && processBriefingBodyText != null
            && beginProcessButton != null
            && processBriefingReturnButton != null
            && questionStageNameText != null
            && questionStageIntroText != null
            && questionText != null
            && statsText != null
            && answerButtons != null
            && answerButtons.Length == 3
            && answerButtonTexts != null
            && answerButtonTexts.Length == 3
            && prepCardsPanel != null
            && prepCardButtons != null
            && prepCardButtons.Length == 3
            && prepCardTexts != null
            && prepCardTexts.Length == 3
            && feedbackPanel != null
            && feedbackText != null
            && feedbackPrepCardText != null
            && continueButton != null
            && stageTransitionScreen != null
            && stageContinueButton != null
            && recoveryChoiceScreen != null
            && recoveryChoiceTitleText != null
            && recoveryChoiceBodyText != null
            && recoveryChoiceStatsText != null
            && recoveryChoiceButtonColumn != null
            && recoveryChoiceButtons != null
            && recoveryChoiceButtons.Length == 5
            && recoveryChoiceButtonTexts != null
            && recoveryChoiceButtonTexts.Length == 5
            && recoveryChoiceContinueButton != null
            && randomEventScreen != null
            && randomEventContinueButton != null
            && outcomeScreen != null
            && outcomeHighlightsText != null
            && outcomeBadgesPanel != null
            && outcomeBadgesText != null
            && outcomeAdviceText != null
            && backdropViewportPanel != null
            && pauseOverlay != null
            && resumeButton != null
            && pauseSettingsButton != null
            && pauseReturnToMenuButton != null
            && settingsOverlay != null
            && settingsBackButton != null
            && settingsResetDefaultsButton != null
            && masterVolumeSlider != null
            && sfxVolumeSlider != null
            && muteAudioToggle != null
            && reduceMotionToggle != null
            && deterministicSeedToggle != null
            && seedInputField != null
            && applySeedButton != null
            && newRandomSeedButton != null
            && fullscreenToggle != null
            && versionText != null;
    }

    private void CreateRuntimeUi()
    {
        EnsureEventSystemExists();
        DisableLegacySceneCanvas();
        RemoveOldRuntimeCanvas();

        GameObject canvasObject = new GameObject("Final Round Runtime Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 10;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        runtimeBackgroundPanel = CreatePanel("Dark Background", canvasObject.transform, backgroundColor);
        StretchToParent(runtimeBackgroundPanel.GetComponent<RectTransform>());

        GameObject safeArea = new GameObject("Safe Area", typeof(RectTransform), typeof(VerticalLayoutGroup));
        safeArea.transform.SetParent(canvasObject.transform, false);
        RectTransform safeAreaRect = safeArea.GetComponent<RectTransform>();
        StretchToParent(safeAreaRect);
        safeAreaRect.offsetMin = new Vector2(80f, 54f);
        safeAreaRect.offsetMax = new Vector2(-80f, -54f);

        VerticalLayoutGroup safeLayout = safeArea.GetComponent<VerticalLayoutGroup>();
        safeLayout.spacing = 22f;
        safeLayout.childControlWidth = true;
        safeLayout.childControlHeight = true;
        safeLayout.childForceExpandWidth = true;
        safeLayout.childForceExpandHeight = false;

        CreateHeader(safeArea.transform);
        CreateMenuScreen(safeArea.transform);
        CreateProcessBriefingScreen(safeArea.transform);
        CreateQuestionScreen(safeArea.transform);
        CreateStageTransitionScreen(safeArea.transform);
        CreateRecoveryChoiceScreen(safeArea.transform);
        CreateRandomEventScreen(safeArea.transform);
        CreateOutcomeScreen(safeArea.transform);
        CreatePauseOverlay(canvasObject.transform);
        CreateSettingsOverlay(canvasObject.transform);
        CreateVersionLabel(canvasObject.transform);
        UpdateVersionLabel();
        RefreshSettingsControls();
    }

    private void DisableLegacySceneCanvas()
    {
        GameObject legacyCanvas = GameObject.Find("Canvas");

        if (legacyCanvas != null)
        {
            legacyCanvas.SetActive(false);
        }
    }

    private void RemoveOldRuntimeCanvas()
    {
        GameObject oldCanvas = GameObject.Find("Final Round Runtime Canvas");
        if (oldCanvas != null)
        {
            Destroy(oldCanvas);
        }
    }

    private void EnsureEventSystemExists()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

#if ENABLE_INPUT_SYSTEM
        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
#else
        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
#endif
    }

    private void CreateHeader(Transform parent)
    {
        GameObject header = new GameObject("Header", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        header.transform.SetParent(parent, false);
        ConfigurePreferredLayoutElement(header, -1f, 158f);

        VerticalLayoutGroup headerLayout = header.GetComponent<VerticalLayoutGroup>();
        headerLayout.spacing = 6f;
        headerLayout.childAlignment = TextAnchor.MiddleLeft;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childForceExpandHeight = false;

        TMP_Text titleText = CreateText("Title", header.transform, "FINAL ROUND", 54, FontStyles.Bold, TextAlignmentOptions.Left);
        titleText.color = textColor;
        titleText.characterSpacing = 4f;

        subtitleText = CreateText(
            "Subtitle",
            header.transform,
            "A multi-stage interview gauntlet about confidence, stamina, credibility, and commercial judgment.",
            24,
            FontStyles.Normal,
            TextAlignmentOptions.Left);
        subtitleText.color = mutedTextColor;
        subtitleText.textWrappingMode = TextWrappingModes.Normal;

        progressText = CreateText("Progress", header.transform, string.Empty, 22, FontStyles.Bold, TextAlignmentOptions.Left);
        progressText.color = accentColor;
    }

    private void CreateVersionLabel(Transform parent)
    {
        versionText = CreateText("Version Label", parent, BuildVersion, 18, FontStyles.Bold, TextAlignmentOptions.Right);
        versionText.color = new Color32(142, 152, 168, 210);

        RectTransform versionRect = versionText.GetComponent<RectTransform>();
        versionRect.anchorMin = new Vector2(1f, 0f);
        versionRect.anchorMax = new Vector2(1f, 0f);
        versionRect.pivot = new Vector2(1f, 0f);
        versionRect.sizeDelta = new Vector2(260f, 32f);
        versionRect.anchoredPosition = new Vector2(-18f, 14f);
    }

    private void UpdateVersionLabel()
    {
        if (versionText == null)
        {
            return;
        }

        versionText.text = string.IsNullOrEmpty(currentProcessId)
            ? BuildVersion
            : $"{BuildVersion} | {currentProcessId}";
    }

    private void CreateQuestionScreen(Transform parent)
    {
        questionScreen = new GameObject("Question Screen", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        questionScreen.transform.SetParent(parent, false);
        ConfigureFlexibleLayoutElement(questionScreen, 1f);

        VerticalLayoutGroup questionScreenLayout = questionScreen.GetComponent<VerticalLayoutGroup>();
        questionScreenLayout.spacing = 20f;
        questionScreenLayout.childControlWidth = true;
        questionScreenLayout.childControlHeight = true;
        questionScreenLayout.childForceExpandWidth = true;
        questionScreenLayout.childForceExpandHeight = false;

        GameObject mainRow = new GameObject("Question And Stats Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        mainRow.transform.SetParent(questionScreen.transform, false);
        ConfigurePreferredLayoutElement(mainRow, -1f, 360f);

        HorizontalLayoutGroup mainRowLayout = mainRow.GetComponent<HorizontalLayoutGroup>();
        mainRowLayout.spacing = 20f;
        mainRowLayout.childControlWidth = true;
        mainRowLayout.childControlHeight = true;
        mainRowLayout.childForceExpandWidth = false;
        mainRowLayout.childForceExpandHeight = true;

        GameObject leftColumn = new GameObject("Question And Stats Column", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        leftColumn.transform.SetParent(mainRow.transform, false);
        ConfigureFlexibleLayoutElement(leftColumn, 1f, 620f);

        VerticalLayoutGroup leftColumnLayout = leftColumn.GetComponent<VerticalLayoutGroup>();
        leftColumnLayout.spacing = 16f;
        leftColumnLayout.childControlWidth = true;
        leftColumnLayout.childControlHeight = true;
        leftColumnLayout.childForceExpandWidth = true;
        leftColumnLayout.childForceExpandHeight = false;

        CreateQuestionPanel(leftColumn.transform);
        CreateStatsPanel(leftColumn.transform);
        CreateBackdropViewportPanel(mainRow.transform);
        CreatePrepCardsPanel(questionScreen.transform);
        CreateFeedbackPanel(questionScreen.transform);
        CreateAnswerButtons(questionScreen.transform);
    }

    private void CreateBackdropViewportPanel(Transform parent)
    {
        backdropViewportPanel = CreatePanel("Backdrop Viewport Panel", parent, panelAccentColor);
        ConfigurePreferredLayoutElement(backdropViewportPanel, 600f, -1f);

        callPanelRoot = new GameObject("2D Interview Call Panel", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        callPanelRoot.transform.SetParent(backdropViewportPanel.transform, false);
        RectTransform callPanelRect = callPanelRoot.GetComponent<RectTransform>();
        StretchToParent(callPanelRect);
        callPanelRect.offsetMin = new Vector2(8f, 8f);
        callPanelRect.offsetMax = new Vector2(-8f, -8f);

        callPanelBackgroundImage = callPanelRoot.GetComponent<Image>();
        callPanelBackgroundImage.color = new Color32(20, 31, 47, 255);
        callPanelBackgroundImage.raycastTarget = false;

        callPanelFrameImage = CreateUiImage("Call Frame", callPanelRoot.transform, new Color32(68, 112, 128, 255));
        RectTransform frameRect = callPanelFrameImage.GetComponent<RectTransform>();
        StretchToParent(frameRect);

        GameObject innerPanel = new GameObject("Call Inner Surface", typeof(RectTransform), typeof(Image));
        innerPanel.transform.SetParent(callPanelRoot.transform, false);
        Image innerImage = innerPanel.GetComponent<Image>();
        innerImage.color = new Color32(26, 38, 55, 255);
        innerImage.raycastTarget = false;
        RectTransform innerRect = innerPanel.GetComponent<RectTransform>();
        StretchToParent(innerRect);
        innerRect.offsetMin = new Vector2(8f, 8f);
        innerRect.offsetMax = new Vector2(-8f, -8f);

        callTopBarImage = CreateUiImage("Call Top Bar", innerPanel.transform, new Color32(40, 54, 72, 255));
        RectTransform topBarRect = callTopBarImage.GetComponent<RectTransform>();
        topBarRect.anchorMin = new Vector2(0f, 1f);
        topBarRect.anchorMax = new Vector2(1f, 1f);
        topBarRect.pivot = new Vector2(0.5f, 1f);
        topBarRect.offsetMin = new Vector2(0f, -46f);
        topBarRect.offsetMax = Vector2.zero;

        callStageLabelText = CreateOverlayText("Call Stage Label", innerPanel.transform, "LIVE INTERVIEW", 23, TextAlignmentOptions.Left);
        RectTransform stageLabelRect = callStageLabelText.GetComponent<RectTransform>();
        stageLabelRect.anchorMin = new Vector2(0f, 1f);
        stageLabelRect.anchorMax = new Vector2(0.68f, 1f);
        stageLabelRect.pivot = new Vector2(0f, 1f);
        stageLabelRect.offsetMin = new Vector2(18f, -44f);
        stageLabelRect.offsetMax = new Vector2(-8f, -4f);

        callStatusLabelText = CreateOverlayText("Call Status Label", innerPanel.transform, "LIVE CALL", 19, TextAlignmentOptions.Right);
        RectTransform statusLabelRect = callStatusLabelText.GetComponent<RectTransform>();
        statusLabelRect.anchorMin = new Vector2(0.62f, 1f);
        statusLabelRect.anchorMax = new Vector2(1f, 1f);
        statusLabelRect.pivot = new Vector2(1f, 1f);
        statusLabelRect.offsetMin = new Vector2(8f, -43f);
        statusLabelRect.offsetMax = new Vector2(-18f, -5f);

        GameObject tileLayer = new GameObject("Call Participant Layer", typeof(RectTransform));
        tileLayer.transform.SetParent(innerPanel.transform, false);
        RectTransform tileLayerRect = tileLayer.GetComponent<RectTransform>();
        StretchToParent(tileLayerRect);
        tileLayerRect.offsetMin = new Vector2(12f, 12f);
        tileLayerRect.offsetMax = new Vector2(-12f, -54f);

        callParticipantTiles = new CallParticipantTile[3];
        for (int i = 0; i < callParticipantTiles.Length; i++)
        {
            callParticipantTiles[i] = CreateCallParticipantTile(tileLayer.transform, i);
        }

        callStandbyCard = new GameObject("Call Standby Card", typeof(RectTransform), typeof(Image));
        callStandbyCard.transform.SetParent(tileLayer.transform, false);
        Image standbyImage = callStandbyCard.GetComponent<Image>();
        standbyImage.color = new Color32(38, 51, 68, 255);
        standbyImage.raycastTarget = false;
        RectTransform standbyRect = callStandbyCard.GetComponent<RectTransform>();
        standbyRect.anchorMin = new Vector2(0.14f, 0.22f);
        standbyRect.anchorMax = new Vector2(0.86f, 0.78f);
        standbyRect.offsetMin = Vector2.zero;
        standbyRect.offsetMax = Vector2.zero;

        callStandbyText = CreateOverlayText("Call Standby Text", callStandbyCard.transform, "BETWEEN ROUNDS", 34, TextAlignmentOptions.Center);
        StretchToParent(callStandbyText.GetComponent<RectTransform>());

        UpdateCallPanel("Main Menu");
    }

    private void CreateMenuScreen(Transform parent)
    {
        menuScreen = CreatePanel("Main Menu Screen", parent, panelColor);
        ConfigureFlexibleLayoutElement(menuScreen, 1f);
        AddPaddingLayout(menuScreen, new RectOffset(42, 42, 44, 44), 18f);

        TMP_Text menuTitleText = CreateText("Menu Title", menuScreen.transform, "FINAL ROUND", 56, FontStyles.Bold, TextAlignmentOptions.Left);
        menuTitleText.color = textColor;
        menuTitleText.characterSpacing = 4f;

        menuBodyText = CreateText(
            "Menu Body",
            menuScreen.transform,
            "A short interview process about confidence, stamina, technical credibility, and commercial judgment.",
            28,
            FontStyles.Normal,
            TextAlignmentOptions.TopLeft);
        menuBodyText.color = mutedTextColor;
        menuBodyText.textWrappingMode = TextWrappingModes.Normal;
        ConfigureFlexibleLayoutElement(menuBodyText.gameObject, 1f);

        startInterviewButton = CreateMenuButton(menuScreen.transform, "Start Interview Process", StartInterviewProcess);
        settingsButton = CreateMenuButton(menuScreen.transform, "Settings", OpenSettingsFromMenu);
        howToPlayButton = CreateMenuButton(menuScreen.transform, "How To Play", ShowHowToPlay);
        aboutButton = CreateMenuButton(menuScreen.transform, "About", ShowAbout);
        quitButton = CreateMenuButton(menuScreen.transform, "Quit", QuitGame);
        quitButton.gameObject.SetActive(!Application.isEditor);

        menuScreen.SetActive(false);
    }

    private CallParticipantTile CreateCallParticipantTile(Transform parent, int tileIndex)
    {
        GameObject root = new GameObject($"Call Participant Tile {tileIndex + 1}", typeof(RectTransform), typeof(CanvasGroup));
        root.transform.SetParent(parent, false);

        Image borderImage = CreateUiImage("Active Border", root.transform, accentColor);
        RectTransform borderRect = borderImage.GetComponent<RectTransform>();
        StretchToParent(borderRect);

        Image tileImage = CreateUiImage("Tile Surface", root.transform, new Color32(58, 74, 96, 255));
        RectTransform tileRect = tileImage.GetComponent<RectTransform>();
        StretchToParent(tileRect);
        tileRect.offsetMin = new Vector2(5f, 5f);
        tileRect.offsetMax = new Vector2(-5f, -5f);

        TMP_Text headText = CreateOverlayText("Avatar Head", root.transform, "●", 74, TextAlignmentOptions.Center);
        headText.color = GetCallHeadColor(tileIndex);
        RectTransform headRect = headText.GetComponent<RectTransform>();
        headRect.anchorMin = new Vector2(0.31f, 0.47f);
        headRect.anchorMax = new Vector2(0.69f, 0.86f);
        headRect.offsetMin = Vector2.zero;
        headRect.offsetMax = Vector2.zero;

        Image bodyImage = CreateUiImage("Avatar Body", root.transform, GetCallBodyColor(tileIndex));
        RectTransform bodyRect = bodyImage.GetComponent<RectTransform>();
        bodyRect.anchorMin = new Vector2(0.29f, 0.28f);
        bodyRect.anchorMax = new Vector2(0.71f, 0.58f);
        bodyRect.offsetMin = Vector2.zero;
        bodyRect.offsetMax = Vector2.zero;

        Image shoulderImage = CreateUiImage("Avatar Shoulder Highlight", root.transform, Color.Lerp(GetCallBodyColor(tileIndex), Color.white, 0.08f));
        RectTransform shoulderRect = shoulderImage.GetComponent<RectTransform>();
        shoulderRect.anchorMin = new Vector2(0.34f, 0.47f);
        shoulderRect.anchorMax = new Vector2(0.66f, 0.56f);
        shoulderRect.offsetMin = Vector2.zero;
        shoulderRect.offsetMax = Vector2.zero;

        Image headImage = CreateUiImage("Avatar Head Shape", root.transform, GetCallHeadColor(tileIndex));
        RectTransform headShapeRect = headImage.GetComponent<RectTransform>();
        headShapeRect.anchorMin = new Vector2(0.34f, 0.55f);
        headShapeRect.anchorMax = new Vector2(0.66f, 0.82f);
        headShapeRect.offsetMin = Vector2.zero;
        headShapeRect.offsetMax = Vector2.zero;
        headText.gameObject.SetActive(false);

        Image labelBarImage = CreateUiImage("Role Label Bar", root.transform, new Color32(11, 18, 28, 245));
        RectTransform labelBarRect = labelBarImage.GetComponent<RectTransform>();
        labelBarRect.anchorMin = new Vector2(0f, 0f);
        labelBarRect.anchorMax = new Vector2(1f, 0.25f);
        labelBarRect.offsetMin = new Vector2(5f, 5f);
        labelBarRect.offsetMax = new Vector2(-5f, -5f);

        TMP_Text roleText = CreateOverlayText("Role Label", labelBarImage.transform, "Interviewer", 27, TextAlignmentOptions.Center);
        roleText.textWrappingMode = TextWrappingModes.Normal;
        StretchToParent(roleText.GetComponent<RectTransform>());

        return new CallParticipantTile
        {
            Root = root,
            CanvasGroup = root.GetComponent<CanvasGroup>(),
            BorderImage = borderImage,
            TileImage = tileImage,
            BodyImage = bodyImage,
            ShoulderImage = shoulderImage,
            HeadImage = headImage,
            HeadText = headText,
            LabelBarImage = labelBarImage,
            RoleText = roleText,
            TileIndex = tileIndex
        };
    }

    private Image CreateUiImage(string name, Transform parent, Color color)
    {
        GameObject imageObject = new GameObject(name, typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(parent, false);
        Image image = imageObject.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private TMP_Text CreateOverlayText(string name, Transform parent, string text, int fontSize, TextAlignmentOptions alignment)
    {
        TMP_Text label = CreateText(name, parent, text, fontSize, FontStyles.Bold, alignment);
        label.color = textColor;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.overflowMode = TextOverflowModes.Ellipsis;
        return label;
    }

    private void CreateProcessBriefingScreen(Transform parent)
    {
        processBriefingScreen = CreatePanel("Process Briefing Screen", parent, transitionPanelColor);
        ConfigureFlexibleLayoutElement(processBriefingScreen, 1f);
        AddPaddingLayout(processBriefingScreen, new RectOffset(42, 42, 40, 40), 20f);

        processBriefingTitleText = CreateText("Process Briefing Title", processBriefingScreen.transform, "TODAY'S PROCESS", 44, FontStyles.Bold, TextAlignmentOptions.Left);
        processBriefingTitleText.color = accentColor;

        processBriefingBodyText = CreateText("Process Briefing Body", processBriefingScreen.transform, string.Empty, 30, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        processBriefingBodyText.color = textColor;
        processBriefingBodyText.textWrappingMode = TextWrappingModes.Normal;
        processBriefingBodyText.lineSpacing = 10f;
        ConfigureFlexibleLayoutElement(processBriefingBodyText.gameObject, 1f);

        GameObject buttonRow = new GameObject("Process Briefing Button Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        buttonRow.transform.SetParent(processBriefingScreen.transform, false);
        ConfigurePreferredLayoutElement(buttonRow, -1f, 72f);

        HorizontalLayoutGroup buttonRowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
        buttonRowLayout.spacing = 18f;
        buttonRowLayout.childControlWidth = true;
        buttonRowLayout.childControlHeight = true;
        buttonRowLayout.childForceExpandWidth = true;
        buttonRowLayout.childForceExpandHeight = true;

        beginProcessButton = CreateMenuButton(buttonRow.transform, "Begin Process", BeginProcess, false);
        processBriefingReturnButton = CreateMenuButton(buttonRow.transform, "Return to Menu", ShowMenu);
        processBriefingScreen.SetActive(false);
    }

    private void CreatePauseOverlay(Transform parent)
    {
        pauseOverlay = CreatePanel("Pause Overlay", parent, new Color32(5, 7, 11, 186));
        StretchToParent(pauseOverlay.GetComponent<RectTransform>());

        GameObject pausePanel = CreatePanel("Pause Panel", pauseOverlay.transform, panelColor);
        AddPaddingLayout(pausePanel, new RectOffset(34, 34, 30, 30), 16f);

        RectTransform panelRect = pausePanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(560f, 348f);
        panelRect.anchoredPosition = Vector2.zero;

        TMP_Text pauseTitle = CreateText("Pause Title", pausePanel.transform, "PAUSED", 42, FontStyles.Bold, TextAlignmentOptions.Left);
        pauseTitle.color = accentColor;

        TMP_Text pauseBody = CreateText(
            "Pause Body",
            pausePanel.transform,
            "Take a breath, then resume the interview or return to the main menu.",
            24,
            FontStyles.Normal,
            TextAlignmentOptions.TopLeft);
        pauseBody.color = mutedTextColor;
        pauseBody.textWrappingMode = TextWrappingModes.Normal;
        ConfigureFlexibleLayoutElement(pauseBody.gameObject, 1f);

        resumeButton = CreateMenuButton(pausePanel.transform, "Resume", ResumeFromPause);
        pauseSettingsButton = CreateMenuButton(pausePanel.transform, "Settings", OpenSettingsFromPause);
        pauseReturnToMenuButton = CreateMenuButton(pausePanel.transform, "Return to Menu", ReturnToMenuFromPause);
        pauseOverlay.SetActive(false);
    }

    private void CreateSettingsOverlay(Transform parent)
    {
        settingsOverlay = CreatePanel("Settings Overlay", parent, new Color32(5, 7, 11, 210));
        StretchToParent(settingsOverlay.GetComponent<RectTransform>());

        GameObject settingsPanel = CreatePanel("Settings Panel", settingsOverlay.transform, panelColor);
        AddPaddingLayout(settingsPanel, new RectOffset(34, 34, 28, 28), 10f);

        RectTransform panelRect = settingsPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(980f, 900f);
        panelRect.anchoredPosition = Vector2.zero;

        TMP_Text title = CreateText("Settings Title", settingsPanel.transform, "SETTINGS", 42, FontStyles.Bold, TextAlignmentOptions.Left);
        title.color = accentColor;

        CreateSettingsSectionHeading(settingsPanel.transform, "AUDIO");
        masterVolumeSlider = CreateSettingsSlider(settingsPanel.transform, "Master Volume", masterVolume, OnMasterVolumeChanged, out masterVolumeValueText);
        sfxVolumeSlider = CreateSettingsSlider(settingsPanel.transform, "SFX Volume", sfxVolume, OnSfxVolumeChanged, out sfxVolumeValueText);
        muteAudioToggle = CreateSettingsToggle(settingsPanel.transform, "Mute Audio", muteAudio, OnMuteAudioChanged);

        CreateSettingsSectionHeading(settingsPanel.transform, "MOTION");
        reduceMotionToggle = CreateSettingsToggle(settingsPanel.transform, "Reduce Motion", reduceMotion, OnReduceMotionChanged);

        CreateSettingsSectionHeading(settingsPanel.transform, "RUN OPTIONS");
        deterministicSeedToggle = CreateSettingsToggle(settingsPanel.transform, "Deterministic Run Seed", useDeterministicRunSeed, OnDeterministicSeedChanged);
        seedInputField = CreateSettingsInputField(settingsPanel.transform, "Seed", debugRunSeed.ToString(), OnSeedInputChanged);

        GameObject seedButtonRow = CreateSettingsButtonRow(settingsPanel.transform);
        applySeedButton = CreateCompactSettingsButton(seedButtonRow.transform, "Apply Seed", ApplySeedSettings);
        newRandomSeedButton = CreateCompactSettingsButton(seedButtonRow.transform, "New Random Seed", GenerateNewSettingsSeed);

        seedNoteText = CreateText(
            "Seed Note",
            settingsPanel.transform,
            "Seed changes apply to the next new process.",
            20,
            FontStyles.Normal,
            TextAlignmentOptions.Left);
        seedNoteText.color = mutedTextColor;

        CreateSettingsSectionHeading(settingsPanel.transform, "DISPLAY");
        fullscreenToggle = CreateSettingsToggle(settingsPanel.transform, "Fullscreen", fullscreenEnabled, OnFullscreenChanged);

        GameObject buttonRow = CreateSettingsButtonRow(settingsPanel.transform);
        settingsBackButton = CreateCompactSettingsButton(buttonRow.transform, "Back", CloseSettings);
        settingsResetDefaultsButton = CreateCompactSettingsButton(buttonRow.transform, "Reset Defaults", ResetSettingsDefaults);

        settingsOverlay.SetActive(false);
    }

    private void CreateSettingsSectionHeading(Transform parent, string text)
    {
        TMP_Text heading = CreateText($"{text} Settings Heading", parent, text, 23, FontStyles.Bold, TextAlignmentOptions.Left);
        heading.color = accentColor;
        ConfigurePreferredLayoutElement(heading.gameObject, -1f, 30f);
    }

    private Slider CreateSettingsSlider(
        Transform parent,
        string label,
        float value,
        UnityEngine.Events.UnityAction<float> onValueChanged,
        out TMP_Text valueText)
    {
        GameObject row = CreateSettingsRow($"{label} Row", parent, 54f);

        TMP_Text labelText = CreateText($"{label} Label", row.transform, label, 22, FontStyles.Bold, TextAlignmentOptions.Left);
        labelText.color = textColor;
        ConfigurePreferredLayoutElement(labelText.gameObject, 260f, -1f);

        GameObject sliderObject = new GameObject($"{label} Slider", typeof(RectTransform), typeof(Slider), typeof(Image));
        sliderObject.transform.SetParent(row.transform, false);
        ConfigureFlexibleLayoutElement(sliderObject, 1f, 280f);

        Image background = sliderObject.GetComponent<Image>();
        background.color = new Color32(16, 22, 34, 255);

        Slider slider = sliderObject.GetComponent<Slider>();
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.targetGraphic = background;

        GameObject fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderObject.transform, false);
        RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
        StretchToParent(fillAreaRect);
        fillAreaRect.offsetMin = new Vector2(8f, 8f);
        fillAreaRect.offsetMax = new Vector2(-8f, -8f);

        Image fillImage = CreateUiImage("Fill", fillArea.transform, accentColor);
        slider.fillRect = fillImage.GetComponent<RectTransform>();
        StretchToParent(slider.fillRect);

        Image handleImage = CreateUiImage("Handle", sliderObject.transform, textColor);
        RectTransform handleRect = handleImage.GetComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0f, 0.5f);
        handleRect.anchorMax = new Vector2(0f, 0.5f);
        handleRect.pivot = new Vector2(0.5f, 0.5f);
        handleRect.sizeDelta = new Vector2(18f, 34f);
        slider.handleRect = handleRect;

        valueText = CreateText($"{label} Value", row.transform, FormatPercent(value), 20, FontStyles.Bold, TextAlignmentOptions.Right);
        valueText.color = mutedTextColor;
        ConfigurePreferredLayoutElement(valueText.gameObject, 86f, -1f);

        slider.SetValueWithoutNotify(value);
        slider.onValueChanged.AddListener(onValueChanged);
        return slider;
    }

    private Toggle CreateSettingsToggle(Transform parent, string label, bool value, UnityEngine.Events.UnityAction<bool> onValueChanged)
    {
        GameObject row = CreateSettingsRow($"{label} Row", parent, 48f);

        GameObject toggleObject = new GameObject($"{label} Toggle", typeof(RectTransform), typeof(Toggle), typeof(Image));
        toggleObject.transform.SetParent(row.transform, false);
        ConfigurePreferredLayoutElement(toggleObject, 44f, 34f);

        Image background = toggleObject.GetComponent<Image>();
        background.color = new Color32(16, 22, 34, 255);

        Image checkmark = CreateUiImage("Checkmark", toggleObject.transform, accentColor);
        RectTransform checkRect = checkmark.GetComponent<RectTransform>();
        StretchToParent(checkRect);
        checkRect.offsetMin = new Vector2(9f, 8f);
        checkRect.offsetMax = new Vector2(-9f, -8f);

        Toggle toggle = toggleObject.GetComponent<Toggle>();
        toggle.targetGraphic = background;
        toggle.graphic = checkmark;
        toggle.SetIsOnWithoutNotify(value);
        toggle.onValueChanged.AddListener(onValueChanged);

        TMP_Text labelText = CreateText($"{label} Label", row.transform, label, 22, FontStyles.Bold, TextAlignmentOptions.Left);
        labelText.color = textColor;
        ConfigureFlexibleLayoutElement(labelText.gameObject, 1f);

        return toggle;
    }

    private TMP_InputField CreateSettingsInputField(
        Transform parent,
        string label,
        string value,
        UnityEngine.Events.UnityAction<string> onValueChanged)
    {
        GameObject row = CreateSettingsRow($"{label} Row", parent, 54f);

        TMP_Text labelText = CreateText($"{label} Label", row.transform, label, 22, FontStyles.Bold, TextAlignmentOptions.Left);
        labelText.color = textColor;
        ConfigurePreferredLayoutElement(labelText.gameObject, 260f, -1f);

        GameObject inputObject = new GameObject($"{label} Input", typeof(RectTransform), typeof(Image), typeof(TMP_InputField));
        inputObject.transform.SetParent(row.transform, false);
        ConfigureFlexibleLayoutElement(inputObject, 1f, 280f);
        inputObject.GetComponent<Image>().color = new Color32(16, 22, 34, 255);

        TMP_Text inputText = CreateText("Text", inputObject.transform, value, 22, FontStyles.Bold, TextAlignmentOptions.Left);
        inputText.color = textColor;
        RectTransform inputTextRect = inputText.GetComponent<RectTransform>();
        StretchToParent(inputTextRect);
        inputTextRect.offsetMin = new Vector2(16f, 6f);
        inputTextRect.offsetMax = new Vector2(-16f, -6f);

        TMP_Text placeholderText = CreateText("Placeholder", inputObject.transform, "Run seed", 22, FontStyles.Normal, TextAlignmentOptions.Left);
        placeholderText.color = mutedTextColor;
        RectTransform placeholderRect = placeholderText.GetComponent<RectTransform>();
        StretchToParent(placeholderRect);
        placeholderRect.offsetMin = new Vector2(16f, 6f);
        placeholderRect.offsetMax = new Vector2(-16f, -6f);

        TMP_InputField inputField = inputObject.GetComponent<TMP_InputField>();
        inputField.textComponent = inputText;
        inputField.placeholder = placeholderText;
        inputField.contentType = TMP_InputField.ContentType.IntegerNumber;
        inputField.text = value;
        inputField.onEndEdit.AddListener(onValueChanged);
        return inputField;
    }

    private GameObject CreateSettingsRow(string name, Transform parent, float preferredHeight)
    {
        GameObject row = new GameObject(name, typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        ConfigurePreferredLayoutElement(row, -1f, preferredHeight);

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 14f;
        layout.childAlignment = TextAnchor.MiddleLeft;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = false;
        layout.childForceExpandHeight = false;
        return row;
    }

    private GameObject CreateSettingsButtonRow(Transform parent)
    {
        GameObject row = new GameObject("Settings Button Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        ConfigurePreferredLayoutElement(row, -1f, 58f);

        HorizontalLayoutGroup layout = row.GetComponent<HorizontalLayoutGroup>();
        layout.spacing = 14f;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = true;
        return row;
    }

    private Button CreateCompactSettingsButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateButton(label + " Button", parent);
        TMP_Text buttonText = CreateText("Label", buttonObject.transform, label, 22, FontStyles.Bold, TextAlignmentOptions.Center);
        buttonText.color = textColor;
        StretchToParent(buttonText.GetComponent<RectTransform>());

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            PlaySound(FinalRoundSoundEvent.UiClick);
            onClick();
        });
        return button;
    }

    private Button CreateMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick, bool playClickSound = true)
    {
        GameObject buttonObject = CreateButton(label + " Button", parent);
        ConfigurePreferredLayoutElement(buttonObject, -1f, 72f);

        TMP_Text buttonText = CreateText("Label", buttonObject.transform, label, 24, FontStyles.Bold, TextAlignmentOptions.Center);
        buttonText.color = textColor;
        StretchToParent(buttonText.GetComponent<RectTransform>());

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            if (playClickSound)
            {
                PlaySound(FinalRoundSoundEvent.UiClick);
            }

            onClick();
        });
        return button;
    }

    private void CreateQuestionPanel(Transform parent)
    {
        GameObject panel = CreatePanel("Question Text Area", parent, panelColor);
        ConfigureFlexibleLayoutElement(panel, 1f);
        AddPaddingLayout(panel, new RectOffset(34, 34, 28, 30), 10f);

        questionStageNameText = CreateText("Question Stage Name", panel.transform, string.Empty, 31, FontStyles.Bold, TextAlignmentOptions.Left);
        questionStageNameText.color = accentColor;

        questionStageIntroText = CreateText("Question Stage Intro", panel.transform, string.Empty, 24, FontStyles.Normal, TextAlignmentOptions.Left);
        questionStageIntroText.color = mutedTextColor;
        questionStageIntroText.textWrappingMode = TextWrappingModes.Normal;
        questionStageIntroText.lineSpacing = 4f;
        ConfigurePreferredLayoutElement(questionStageIntroText.gameObject, -1f, 54f);
        SetMinimumLayoutHeight(questionStageIntroText.gameObject, 44f);

        questionText = CreateText("Question Text", panel.transform, string.Empty, 34, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        questionText.textWrappingMode = TextWrappingModes.Normal;
        questionText.color = textColor;
        ConfigureFlexibleLayoutElement(questionText.gameObject, 1f);
    }

    private void CreateStatsPanel(Transform parent)
    {
        GameObject panel = CreatePanel("Stats Panel", parent, panelAccentColor);
        ConfigurePreferredLayoutElement(panel, -1f, 146f);
        SetMinimumLayoutHeight(panel, 136f);
        AddPaddingLayout(panel, new RectOffset(24, 24, 14, 16), 7f);

        TMP_Text statsTitle = CreateText("Stats Title", panel.transform, "CANDIDATE READ", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        statsTitle.color = accentColor;

        statsText = CreateText("Stats Text", panel.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        statsText.color = textColor;
        statsBaseColor = statsText.color;
        statsText.lineSpacing = 5f;
        ConfigurePreferredLayoutElement(statsText.gameObject, -1f, 84f);
        SetMinimumLayoutHeight(statsText.gameObject, 78f);
        ConfigureFlexibleLayoutElement(statsText.gameObject, 1f);
    }

    private void CreatePrepCardsPanel(Transform parent)
    {
        prepCardsPanel = CreatePanel("Prep Cards Panel", parent, panelAccentColor);
        ConfigurePreferredLayoutElement(prepCardsPanel, -1f, 138f);
        AddPaddingLayout(prepCardsPanel, new RectOffset(24, 24, 16, 18), 10f);

        TMP_Text prepTitle = CreateText("Prep Cards Title", prepCardsPanel.transform, "PREP CARDS", 20, FontStyles.Bold, TextAlignmentOptions.Left);
        prepTitle.color = accentColor;

        GameObject cardRow = new GameObject("Prep Card Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        cardRow.transform.SetParent(prepCardsPanel.transform, false);
        ConfigureFlexibleLayoutElement(cardRow, 1f);

        HorizontalLayoutGroup cardRowLayout = cardRow.GetComponent<HorizontalLayoutGroup>();
        cardRowLayout.spacing = 12f;
        cardRowLayout.childControlWidth = true;
        cardRowLayout.childControlHeight = true;
        cardRowLayout.childForceExpandWidth = true;
        cardRowLayout.childForceExpandHeight = true;

        prepCardButtons = new Button[3];
        prepCardTexts = new TMP_Text[3];

        for (int i = 0; i < prepCardButtons.Length; i++)
        {
            int cardIndex = i;
            GameObject buttonObject = CreateButton($"Prep Card {i + 1}", cardRow.transform);
            ConfigureFlexibleLayoutElement(buttonObject, 1f);

            TMP_Text label = CreateText("Label", buttonObject.transform, string.Empty, 19, FontStyles.Normal, TextAlignmentOptions.TopLeft);
            label.color = textColor;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.lineSpacing = 4f;

            RectTransform labelRect = label.GetComponent<RectTransform>();
            StretchToParent(labelRect);
            labelRect.offsetMin = new Vector2(18f, 10f);
            labelRect.offsetMax = new Vector2(-18f, -10f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(() => UsePrepCard(cardIndex));

            prepCardButtons[i] = button;
            prepCardTexts[i] = label;
        }
    }

    private void CreateFeedbackPanel(Transform parent)
    {
        feedbackPanel = CreatePanel("Answer Feedback Panel", parent, panelAccentColor);
        ConfigurePreferredLayoutElement(feedbackPanel, -1f, 304f);
        SetMinimumLayoutHeight(feedbackPanel, 286f);
        AddPaddingLayout(feedbackPanel, new RectOffset(28, 28, 20, 24), 12f);

        TMP_Text feedbackTitle = CreateText("Feedback Title", feedbackPanel.transform, "INTERVIEWER REACTION", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        feedbackTitle.color = accentColor;

        GameObject feedbackBodyRow = new GameObject("Feedback Body Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        feedbackBodyRow.transform.SetParent(feedbackPanel.transform, false);
        ConfigurePreferredLayoutElement(feedbackBodyRow, -1f, 168f);
        SetMinimumLayoutHeight(feedbackBodyRow, 154f);

        HorizontalLayoutGroup bodyRowLayout = feedbackBodyRow.GetComponent<HorizontalLayoutGroup>();
        bodyRowLayout.spacing = 22f;
        bodyRowLayout.childControlWidth = true;
        bodyRowLayout.childControlHeight = true;
        bodyRowLayout.childForceExpandWidth = true;
        bodyRowLayout.childForceExpandHeight = true;

        GameObject reactionColumn = new GameObject("Feedback Reaction Column", typeof(RectTransform), typeof(LayoutElement));
        reactionColumn.transform.SetParent(feedbackBodyRow.transform, false);
        ConfigureFlexibleLayoutElement(reactionColumn, 1.1f, 560f);

        GameObject prepColumn = new GameObject("Feedback Prep Card Column", typeof(RectTransform), typeof(LayoutElement));
        prepColumn.transform.SetParent(feedbackBodyRow.transform, false);
        ConfigureFlexibleLayoutElement(prepColumn, 0.9f, 400f);

        feedbackText = CreateText("Feedback Text", reactionColumn.transform, string.Empty, 24, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        feedbackText.color = textColor;
        feedbackText.textWrappingMode = TextWrappingModes.Normal;
        feedbackText.lineSpacing = 5f;
        StretchToParent(feedbackText.GetComponent<RectTransform>());

        feedbackPrepCardText = CreateText("Feedback Prep Card Text", prepColumn.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        feedbackPrepCardText.color = mutedTextColor;
        feedbackPrepCardText.textWrappingMode = TextWrappingModes.Normal;
        feedbackPrepCardText.lineSpacing = 5f;
        StretchToParent(feedbackPrepCardText.GetComponent<RectTransform>());

        GameObject continueButtonObject = CreateButton("Continue Button", feedbackPanel.transform);
        ConfigurePreferredLayoutElement(continueButtonObject, -1f, 54f);
        SetMinimumLayoutHeight(continueButtonObject, 54f);

        TMP_Text continueButtonText = CreateText("Label", continueButtonObject.transform, "Continue", 24, FontStyles.Bold, TextAlignmentOptions.Center);
        continueButtonText.color = textColor;
        StretchToParent(continueButtonText.GetComponent<RectTransform>());

        continueButton = continueButtonObject.GetComponent<Button>();
        continueButton.onClick.AddListener(ContinueAfterFeedback);

        feedbackPanel.SetActive(false);
    }

    private void CreateAnswerButtons(Transform parent)
    {
        GameObject buttonColumn = new GameObject("Answer Button Column", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        buttonColumn.transform.SetParent(parent, false);
        ConfigurePreferredLayoutElement(buttonColumn, -1f, 236f);
        SetMinimumLayoutHeight(buttonColumn, 210f);

        VerticalLayoutGroup buttonLayout = buttonColumn.GetComponent<VerticalLayoutGroup>();
        buttonLayout.spacing = 14f;
        buttonLayout.childControlWidth = true;
        buttonLayout.childControlHeight = true;
        buttonLayout.childForceExpandWidth = true;
        buttonLayout.childForceExpandHeight = true;

        answerButtons = new Button[3];
        answerButtonTexts = new TMP_Text[3];

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answerIndex = i;
            GameObject buttonObject = CreateButton($"Answer Button {i + 1}", buttonColumn.transform);
            ConfigureFlexibleLayoutElement(buttonObject, 1f);

            TMP_Text label = CreateText("Label", buttonObject.transform, string.Empty, 22, FontStyles.Normal, TextAlignmentOptions.MidlineLeft);
            label.color = textColor;
            label.textWrappingMode = TextWrappingModes.Normal;

            RectTransform labelRect = label.GetComponent<RectTransform>();
            StretchToParent(labelRect);
            labelRect.offsetMin = new Vector2(26f, 12f);
            labelRect.offsetMax = new Vector2(-26f, -12f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(() => ChooseAnswer(answerIndex));

            answerButtons[i] = button;
            answerButtonTexts[i] = label;
        }
    }

    private void CreateStageTransitionScreen(Transform parent)
    {
        stageTransitionScreen = CreatePanel("Stage Transition Screen", parent, transitionPanelColor);
        ConfigureFlexibleLayoutElement(stageTransitionScreen, 1f);
        AddPaddingLayout(stageTransitionScreen, new RectOffset(42, 42, 40, 40), 18f);

        TMP_Text stageCompleteText = CreateText("Stage Complete Title", stageTransitionScreen.transform, "ROUND DEBRIEF", 42, FontStyles.Bold, TextAlignmentOptions.Left);
        stageCompleteText.color = accentColor;

        stageTransitionNameText = CreateText("Stage Name", stageTransitionScreen.transform, string.Empty, 40, FontStyles.Bold, TextAlignmentOptions.Left);
        stageTransitionNameText.color = textColor;

        stageTransitionBodyText = CreateText("Stage Feedback", stageTransitionScreen.transform, string.Empty, 27, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        stageTransitionBodyText.color = textColor;
        stageTransitionBodyText.textWrappingMode = TextWrappingModes.Normal;
        ConfigureFlexibleLayoutElement(stageTransitionBodyText.gameObject, 1f);

        stageTransitionStatsText = CreateText("Stage Stats", stageTransitionScreen.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        stageTransitionStatsText.color = mutedTextColor;
        stageTransitionStatsText.lineSpacing = 10f;

        GameObject continueButtonObject = CreateButton("Continue To Next Stage Button", stageTransitionScreen.transform);
        ConfigurePreferredLayoutElement(continueButtonObject, -1f, 72f);

        TMP_Text continueText = CreateText("Label", continueButtonObject.transform, "Continue", 24, FontStyles.Bold, TextAlignmentOptions.Center);
        continueText.color = textColor;
        StretchToParent(continueText.GetComponent<RectTransform>());

        stageContinueButton = continueButtonObject.GetComponent<Button>();
        stageContinueButton.onClick.AddListener(ContinueAfterStageTransition);

        stageTransitionScreen.SetActive(false);
    }

    private void CreateRecoveryChoiceScreen(Transform parent)
    {
        recoveryChoiceScreen = CreatePanel("Between Round Choice Screen", parent, transitionPanelColor);
        ConfigureFlexibleLayoutElement(recoveryChoiceScreen, 1f);
        AddPaddingLayout(recoveryChoiceScreen, new RectOffset(42, 42, 38, 40), 16f);

        recoveryChoiceTitleText = CreateText("Recovery Choice Title", recoveryChoiceScreen.transform, "BETWEEN ROUNDS", 42, FontStyles.Bold, TextAlignmentOptions.Left);
        recoveryChoiceTitleText.color = accentColor;

        recoveryChoiceBodyText = CreateText("Recovery Choice Body", recoveryChoiceScreen.transform, string.Empty, 27, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        recoveryChoiceBodyText.color = textColor;
        recoveryChoiceBodyText.textWrappingMode = TextWrappingModes.Normal;
        ConfigurePreferredLayoutElement(recoveryChoiceBodyText.gameObject, -1f, 98f);
        SetMinimumLayoutHeight(recoveryChoiceBodyText.gameObject, 86f);

        recoveryChoiceStatsText = CreateText("Recovery Choice Stats", recoveryChoiceScreen.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        recoveryChoiceStatsText.color = mutedTextColor;
        recoveryChoiceStatsText.lineSpacing = 8f;
        SetRecoveryStatsLayout(false);

        recoveryChoiceButtonColumn = new GameObject("Recovery Choice Buttons", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        recoveryChoiceButtonColumn.transform.SetParent(recoveryChoiceScreen.transform, false);
        ConfigureFlexibleLayoutElement(recoveryChoiceButtonColumn, 1f);

        VerticalLayoutGroup buttonColumnLayout = recoveryChoiceButtonColumn.GetComponent<VerticalLayoutGroup>();
        buttonColumnLayout.spacing = 10f;
        buttonColumnLayout.childControlWidth = true;
        buttonColumnLayout.childControlHeight = true;
        buttonColumnLayout.childForceExpandWidth = true;
        buttonColumnLayout.childForceExpandHeight = true;

        recoveryChoiceButtons = new Button[5];
        recoveryChoiceButtonTexts = new TMP_Text[5];

        for (int i = 0; i < recoveryChoiceButtons.Length; i++)
        {
            int choiceIndex = i;
            GameObject buttonObject = CreateButton($"Recovery Choice {i + 1}", recoveryChoiceButtonColumn.transform);
            ConfigurePreferredLayoutElement(buttonObject, -1f, 74f);

            TMP_Text label = CreateText("Label", buttonObject.transform, string.Empty, 24, FontStyles.Normal, TextAlignmentOptions.Left);
            label.color = textColor;
            label.fontSize = 24;
            label.alignment = TextAlignmentOptions.Left;
            label.textWrappingMode = TextWrappingModes.Normal;
            label.lineSpacing = 4f;
            RectTransform labelRect = label.GetComponent<RectTransform>();
            StretchToParent(labelRect);
            labelRect.offsetMin = new Vector2(18f, 8f);
            labelRect.offsetMax = new Vector2(-18f, -8f);

            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(() => ChooseRecoveryChoice(choiceIndex));

            recoveryChoiceButtons[i] = button;
            recoveryChoiceButtonTexts[i] = label;
        }

        GameObject continueButtonObject = CreateButton("Continue After Recovery Choice Button", recoveryChoiceScreen.transform);
        ConfigurePreferredLayoutElement(continueButtonObject, -1f, 68f);

        TMP_Text continueText = CreateText("Label", continueButtonObject.transform, "Continue", 28, FontStyles.Bold, TextAlignmentOptions.Center);
        continueText.color = textColor;
        StretchToParent(continueText.GetComponent<RectTransform>());

        recoveryChoiceContinueButton = continueButtonObject.GetComponent<Button>();
        recoveryChoiceContinueButton.onClick.AddListener(ContinueAfterRecoveryChoice);
        recoveryChoiceContinueButton.gameObject.SetActive(false);

        recoveryChoiceScreen.SetActive(false);
    }

    private void CreateRandomEventScreen(Transform parent)
    {
        randomEventScreen = CreatePanel("Random Event Screen", parent, transitionPanelColor);
        ConfigureFlexibleLayoutElement(randomEventScreen, 1f);
        AddPaddingLayout(randomEventScreen, new RectOffset(42, 42, 40, 40), 20f);

        TMP_Text eventLabelText = CreateText("Event Label", randomEventScreen.transform, "BETWEEN ROUNDS", 32, FontStyles.Bold, TextAlignmentOptions.Left);
        eventLabelText.color = accentColor;

        randomEventTitleText = CreateText("Event Title", randomEventScreen.transform, string.Empty, 42, FontStyles.Bold, TextAlignmentOptions.Left);
        randomEventTitleText.color = textColor;

        randomEventBodyText = CreateText("Event Description", randomEventScreen.transform, string.Empty, 28, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        randomEventBodyText.color = textColor;
        randomEventBodyText.textWrappingMode = TextWrappingModes.Normal;
        ConfigureFlexibleLayoutElement(randomEventBodyText.gameObject, 1f);

        randomEventChangesText = CreateText("Event Changes", randomEventScreen.transform, string.Empty, 24, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        randomEventChangesText.color = mutedTextColor;
        randomEventChangesText.lineSpacing = 10f;

        GameObject continueButtonObject = CreateButton("Continue After Event Button", randomEventScreen.transform);
        ConfigurePreferredLayoutElement(continueButtonObject, -1f, 72f);

        TMP_Text continueText = CreateText("Label", continueButtonObject.transform, "Continue", 24, FontStyles.Bold, TextAlignmentOptions.Center);
        continueText.color = textColor;
        StretchToParent(continueText.GetComponent<RectTransform>());

        randomEventContinueButton = continueButtonObject.GetComponent<Button>();
        randomEventContinueButton.onClick.AddListener(ContinueAfterRandomEvent);

        randomEventScreen.SetActive(false);
    }

    private void CreateOutcomeScreen(Transform parent)
    {
        outcomeScreen = CreatePanel("Outcome Screen", parent, panelColor);
        ConfigureFlexibleLayoutElement(outcomeScreen, 1f);
        AddPaddingLayout(outcomeScreen, new RectOffset(34, 34, 26, 26), 14f);

        outcomeTitleText = CreateText("Outcome Title", outcomeScreen.transform, string.Empty, 45, FontStyles.Bold, TextAlignmentOptions.Left);
        outcomeTitleText.color = accentColor;

        outcomeBodyText = CreateText("Outcome Body", outcomeScreen.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        outcomeBodyText.color = textColor;
        outcomeBodyText.textWrappingMode = TextWrappingModes.Normal;
        outcomeBodyText.lineSpacing = 2f;
        ConfigurePreferredLayoutElement(outcomeBodyText.gameObject, -1f, 178f);
        SetMinimumLayoutHeight(outcomeBodyText.gameObject, 166f);

        GameObject outcomeContentRow = new GameObject("Outcome Content Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        outcomeContentRow.transform.SetParent(outcomeScreen.transform, false);
        ConfigureFlexibleLayoutElement(outcomeContentRow, 1f);
        SetMinimumLayoutHeight(outcomeContentRow, 224f);

        HorizontalLayoutGroup contentRowLayout = outcomeContentRow.GetComponent<HorizontalLayoutGroup>();
        contentRowLayout.spacing = 22f;
        contentRowLayout.childControlWidth = true;
        contentRowLayout.childControlHeight = true;
        contentRowLayout.childForceExpandWidth = true;
        contentRowLayout.childForceExpandHeight = true;

        outcomeStatsPanel = CreatePanel("Outcome Stats Panel", outcomeContentRow.transform, panelAccentColor);
        ConfigureFlexibleLayoutElement(outcomeStatsPanel, 1f, 480f);
        AddPaddingLayout(outcomeStatsPanel, new RectOffset(24, 24, 20, 20), 10f);

        TMP_Text statsHeading = CreateText("Outcome Stats Heading", outcomeStatsPanel.transform, "FINAL READ", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        statsHeading.color = accentColor;

        outcomeStatsText = CreateText("Outcome Stats", outcomeStatsPanel.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        outcomeStatsText.color = textColor;
        outcomeStatsText.lineSpacing = 8f;
        ConfigureFlexibleLayoutElement(outcomeStatsText.gameObject, 1f);

        outcomeHighlightsPanel = CreatePanel("Outcome Highlights Panel", outcomeContentRow.transform, panelAccentColor);
        ConfigureFlexibleLayoutElement(outcomeHighlightsPanel, 1f, 480f);
        AddPaddingLayout(outcomeHighlightsPanel, new RectOffset(24, 24, 20, 20), 10f);

        TMP_Text highlightsHeading = CreateText("Outcome Highlights Heading", outcomeHighlightsPanel.transform, "RUN HIGHLIGHTS", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        highlightsHeading.color = accentColor;

        outcomeHighlightsText = CreateText("Outcome Highlights", outcomeHighlightsPanel.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        outcomeHighlightsText.color = textColor;
        outcomeHighlightsText.lineSpacing = 8f;
        outcomeHighlightsText.textWrappingMode = TextWrappingModes.Normal;
        ConfigureFlexibleLayoutElement(outcomeHighlightsText.gameObject, 1f);

        outcomeBadgesPanel = CreatePanel("Outcome Badges Panel", outcomeScreen.transform, panelAccentColor);
        ConfigurePreferredLayoutElement(outcomeBadgesPanel, -1f, 118f);
        SetMinimumLayoutHeight(outcomeBadgesPanel, 104f);
        AddPaddingLayout(outcomeBadgesPanel, new RectOffset(24, 24, 14, 16), 8f);

        TMP_Text badgesHeading = CreateText("Outcome Badges Heading", outcomeBadgesPanel.transform, "BADGES EARNED", 21, FontStyles.Bold, TextAlignmentOptions.Left);
        badgesHeading.color = accentColor;

        outcomeBadgesText = CreateText("Outcome Badges", outcomeBadgesPanel.transform, string.Empty, 20, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        outcomeBadgesText.color = textColor;
        outcomeBadgesText.textWrappingMode = TextWrappingModes.Normal;
        outcomeBadgesText.lineSpacing = 7f;
        ConfigureFlexibleLayoutElement(outcomeBadgesText.gameObject, 1f);

        outcomeAdvicePanel = CreatePanel("Outcome Advice Panel", outcomeScreen.transform, transitionPanelColor);
        ConfigurePreferredLayoutElement(outcomeAdvicePanel, -1f, 94f);
        SetMinimumLayoutHeight(outcomeAdvicePanel, 86f);
        AddPaddingLayout(outcomeAdvicePanel, new RectOffset(24, 24, 16, 16), 8f);

        TMP_Text adviceHeading = CreateText("Outcome Advice Heading", outcomeAdvicePanel.transform, "NEXT RUN ADVICE", 21, FontStyles.Bold, TextAlignmentOptions.Left);
        adviceHeading.color = accentColor;

        outcomeAdviceText = CreateText("Outcome Advice", outcomeAdvicePanel.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        outcomeAdviceText.color = textColor;
        outcomeAdviceText.textWrappingMode = TextWrappingModes.Normal;
        ConfigureFlexibleLayoutElement(outcomeAdviceText.gameObject, 1f);

        outcomeButtonRow = new GameObject("Outcome Button Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        outcomeButtonRow.transform.SetParent(outcomeScreen.transform, false);
        ConfigurePreferredLayoutElement(outcomeButtonRow, -1f, 58f);

        HorizontalLayoutGroup buttonRowLayout = outcomeButtonRow.GetComponent<HorizontalLayoutGroup>();
        buttonRowLayout.spacing = 18f;
        buttonRowLayout.childControlWidth = true;
        buttonRowLayout.childControlHeight = true;
        buttonRowLayout.childForceExpandWidth = true;
        buttonRowLayout.childForceExpandHeight = true;

        CreateMenuButton(outcomeButtonRow.transform, "Restart Interview", RestartGame);
        CreateMenuButton(outcomeButtonRow.transform, "Return to Menu", ShowMenu);
        outcomeScreen.SetActive(false);
    }

    private GameObject CreatePanel(string objectName, Transform parent, Color color)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private GameObject CreateButton(string objectName, Transform parent)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = buttonColor;

        Button button = buttonObject.GetComponent<Button>();
        button.targetGraphic = image;
        button.colors = BuildButtonColors(buttonColor, buttonHoverColor, new Color32(32, 36, 47, 180));
        buttonObject.AddComponent<ButtonJuice>();

        return buttonObject;
    }

    private ColorBlock BuildButtonColors(Color normalColor, Color highlightedColor, Color disabledColor)
    {
        return new ColorBlock
        {
            normalColor = normalColor,
            highlightedColor = highlightedColor,
            pressedColor = accentColor,
            selectedColor = highlightedColor,
            disabledColor = disabledColor,
            colorMultiplier = 1f,
            fadeDuration = 0.08f
        };
    }

    private void SetAnswerButtonVisual(int answerIndex, bool selected)
    {
        Button button = answerButtons[answerIndex];
        Image image = button.GetComponent<Image>();
        bool clarified = answerIndex == clarifiedAnswerIndex && button.interactable && !selected;
        Color normalColor = selected ? selectedAnswerColor : clarified ? clarifiedAnswerColor : buttonColor;
        Color highlightedColor = clarified ? selectedAnswerColor : buttonHoverColor;
        Color disabledColor = selected ? selectedAnswerColor : disabledAnswerColor;

        button.colors = BuildButtonColors(normalColor, highlightedColor, disabledColor);
        image.color = selected || button.interactable ? normalColor : disabledAnswerColor;
        answerButtonTexts[answerIndex].fontStyle = selected || clarified ? FontStyles.Bold : FontStyles.Normal;
        answerButtonTexts[answerIndex].color = selected || button.interactable ? textColor : new Color32(170, 179, 194, 255);
    }

    private TMP_Text CreateText(string objectName, Transform parent, string text, int fontSize, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);

        TMP_Text tmpText = textObject.GetComponent<TMP_Text>();
        tmpText.text = text;
        tmpText.fontSize = fontSize;
        tmpText.fontStyle = style;
        tmpText.alignment = alignment;
        tmpText.enableAutoSizing = true;
        tmpText.fontSizeMin = Mathf.Max(14, fontSize - 10);
        tmpText.fontSizeMax = fontSize;
        tmpText.margin = Vector4.zero;

        return tmpText;
    }

    private void AddPaddingLayout(GameObject target, RectOffset padding, float spacing)
    {
        VerticalLayoutGroup layout = target.AddComponent<VerticalLayoutGroup>();
        layout.padding = padding;
        layout.spacing = spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private void ConfigurePreferredLayoutElement(GameObject target, float preferredWidth, float preferredHeight)
    {
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = target.AddComponent<LayoutElement>();
        }

        if (preferredWidth > 0f)
        {
            layoutElement.preferredWidth = preferredWidth;
        }

        if (preferredHeight > 0f)
        {
            layoutElement.preferredHeight = preferredHeight;
        }
    }

    private void SetMinimumLayoutHeight(GameObject target, float minimumHeight)
    {
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = target.AddComponent<LayoutElement>();
        }

        layoutElement.minHeight = minimumHeight;
    }

    private void ConfigureFlexibleLayoutElement(GameObject target, float flexibleWidth, float minWidth = -1f)
    {
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = target.AddComponent<LayoutElement>();
        }

        layoutElement.flexibleWidth = flexibleWidth;
        layoutElement.flexibleHeight = 1f;

        if (minWidth > 0f)
        {
            layoutElement.minWidth = minWidth;
        }
    }

    private void StretchToParent(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void FadeInScreen(GameObject screen)
    {
        if (screen == null || !screen.activeInHierarchy)
        {
            return;
        }

        CanvasGroup canvasGroup = screen.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = screen.AddComponent<CanvasGroup>();
        }

        if (activeFadeCoroutine != null)
        {
            StopCoroutine(activeFadeCoroutine);
        }

        activeFadeCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup, screen.GetComponent<RectTransform>()));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, RectTransform rectTransform)
    {
        canvasGroup.alpha = 0f;
        Vector3 originalScale = rectTransform == null ? Vector3.one : rectTransform.localScale;
        if (!reduceMotion && rectTransform != null)
        {
            rectTransform.localScale = originalScale * 0.992f;
        }

        float elapsed = 0f;

        while (elapsed < ScreenFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / ScreenFadeDuration));
            canvasGroup.alpha = t;
            if (!reduceMotion && rectTransform != null)
            {
                rectTransform.localScale = Vector3.Lerp(originalScale * 0.992f, originalScale, t);
            }
            yield return null;
        }

        canvasGroup.alpha = 1f;
        if (rectTransform != null)
        {
            rectTransform.localScale = originalScale;
        }
        activeFadeCoroutine = null;
    }

    private CanvasGroup EnsureCanvasGroup(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = target.AddComponent<CanvasGroup>();
        }

        return canvasGroup;
    }

    private void StopAnimation(ref Coroutine coroutine)
    {
        if (coroutine == null)
        {
            return;
        }

        StopCoroutine(coroutine);
        coroutine = null;
    }

    private void AnimateAnswerCardsIn()
    {
        StopAnimation(ref answerEntranceCoroutine);

        if (answerButtons == null)
        {
            return;
        }

        if (reduceMotion)
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                CanvasGroup group = EnsureCanvasGroup(answerButtons[i].gameObject);
                if (group != null)
                {
                    group.alpha = 1f;
                }
            }
            return;
        }

        answerEntranceCoroutine = StartCoroutine(AnimateAnswerCardsInRoutine());
    }

    private IEnumerator AnimateAnswerCardsInRoutine()
    {
        for (int i = 0; i < answerButtons.Length; i++)
        {
            CanvasGroup group = EnsureCanvasGroup(answerButtons[i].gameObject);
            if (group != null)
            {
                group.alpha = 0f;
            }
        }

        float elapsed = 0f;
        const float duration = 0.2f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));

            for (int i = 0; i < answerButtons.Length; i++)
            {
                CanvasGroup group = EnsureCanvasGroup(answerButtons[i].gameObject);
                float stagger = Mathf.Clamp01((elapsed - i * 0.035f) / duration);
                float itemT = Mathf.SmoothStep(0f, 1f, stagger);
                if (group != null)
                {
                    group.alpha = Mathf.Max(t * 0.35f, itemT);
                }
            }

            yield return null;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            CanvasGroup group = EnsureCanvasGroup(answerButtons[i].gameObject);
            if (group != null)
            {
                group.alpha = 1f;
            }
        }

        answerEntranceCoroutine = null;
    }

    private void AnimateAnswerSelection(int selectedAnswerIndex)
    {
        StopAnimation(ref answerEntranceCoroutine);
        StopAnimation(ref answerSelectionCoroutine);

        if (answerButtons == null || selectedAnswerIndex < 0 || selectedAnswerIndex >= answerButtons.Length)
        {
            return;
        }

        answerSelectionCoroutine = StartCoroutine(AnimateAnswerSelectionRoutine(selectedAnswerIndex));
    }

    private IEnumerator AnimateAnswerSelectionRoutine(int selectedAnswerIndex)
    {
        float elapsed = 0f;
        float duration = reduceMotion ? 0.08f : 0.18f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            for (int i = 0; i < answerButtons.Length; i++)
            {
                CanvasGroup group = EnsureCanvasGroup(answerButtons[i].gameObject);
                RectTransform rect = answerButtons[i].GetComponent<RectTransform>();
                bool selected = i == selectedAnswerIndex;

                if (group != null)
                {
                    group.alpha = Mathf.Lerp(1f, selected ? 1f : 0.48f, t);
                }

                if (!reduceMotion && selected)
                {
                    float pulse = Mathf.Sin(t * Mathf.PI) * 0.018f;
                    rect.localScale = Vector3.one * (1f + pulse);
                }
            }

            yield return null;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            CanvasGroup group = EnsureCanvasGroup(answerButtons[i].gameObject);
            if (group != null)
            {
                group.alpha = i == selectedAnswerIndex ? 1f : 0.48f;
            }

            answerButtons[i].GetComponent<RectTransform>().localScale = Vector3.one;
        }

        answerSelectionCoroutine = null;
    }

    private void RevealFeedbackPanel()
    {
        StopAnimation(ref feedbackRevealCoroutine);

        if (feedbackPanel == null)
        {
            return;
        }

        CanvasGroup group = EnsureCanvasGroup(feedbackPanel);
        if (group == null)
        {
            return;
        }

        feedbackRevealCoroutine = StartCoroutine(FadePanelRoutine(group, 0.14f));
    }

    private IEnumerator FadePanelRoutine(CanvasGroup canvasGroup, float duration)
    {
        canvasGroup.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration));
            canvasGroup.alpha = t;

            yield return null;
        }

        canvasGroup.alpha = 1f;
        feedbackRevealCoroutine = null;
    }

    private void FlashStatsText(int strongestChange, int pressureChange)
    {
        StopAnimation(ref statsFlashCoroutine);

        if (statsText == null)
        {
            return;
        }

        Color flashColor = strongestChange >= 0 ? positiveStatColor : negativeStatColor;
        if (pressureChange >= 8 || interviewPressure >= 75 && pressureChange > 0)
        {
            flashColor = negativeStatColor;
        }
        else if (pressureChange < 0 && Mathf.Abs(pressureChange) >= Mathf.Abs(strongestChange))
        {
            flashColor = positiveStatColor;
        }

        statsFlashCoroutine = StartCoroutine(FlashTextRoutine(statsText, flashColor));
    }

    private IEnumerator FlashTextRoutine(TMP_Text target, Color flashColor)
    {
        Color baseColor = statsBaseColor == default ? textColor : statsBaseColor;
        float duration = reduceMotion ? 0.12f : 0.24f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float wave = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
            target.color = Color.Lerp(baseColor, flashColor, wave * 0.55f);
            if (!reduceMotion)
            {
                target.rectTransform.localScale = Vector3.one * (1f + wave * 0.01f);
            }
            yield return null;
        }

        target.color = baseColor;
        target.rectTransform.localScale = Vector3.one;
        statsFlashCoroutine = null;
    }

    private void FlashTransientText(TMP_Text target, int strongestChange, int pressureChange)
    {
        if (target == null)
        {
            return;
        }

        Color flashColor = pressureChange > 0 ? negativeStatColor : pressureChange < 0 ? positiveStatColor : strongestChange >= 0 ? positiveStatColor : negativeStatColor;
        StartCoroutine(FlashTransientTextRoutine(target, target.color, flashColor));
    }

    private IEnumerator FlashTransientTextRoutine(TMP_Text target, Color baseColor, Color flashColor)
    {
        float duration = reduceMotion ? 0.1f : 0.22f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float wave = Mathf.Sin(Mathf.Clamp01(elapsed / duration) * Mathf.PI);
            target.color = Color.Lerp(baseColor, flashColor, wave * 0.45f);
            yield return null;
        }

        target.color = baseColor;
    }

    private int GetLargestAbsoluteStatChange(params int[] changes)
    {
        int strongest = 0;

        for (int i = 0; i < changes.Length; i++)
        {
            if (Mathf.Abs(changes[i]) > Mathf.Abs(strongest))
            {
                strongest = changes[i];
            }
        }

        return strongest;
    }

    private void RevealFinalOutcomeSections()
    {
        StopAnimation(ref finalRevealCoroutine);

        if (reduceMotion || outcomeScreen == null)
        {
            SetOutcomeSectionAlpha(1f);
            PlaySound(FinalRoundSoundEvent.BadgeReveal);
            return;
        }

        finalRevealCoroutine = StartCoroutine(RevealFinalOutcomeSectionsRoutine());
    }

    private IEnumerator RevealFinalOutcomeSectionsRoutine()
    {
        GameObject[] sections =
        {
            outcomeTitleText == null ? null : outcomeTitleText.gameObject,
            outcomeBodyText == null ? null : outcomeBodyText.gameObject,
            outcomeStatsPanel,
            outcomeHighlightsPanel,
            outcomeBadgesPanel,
            outcomeAdvicePanel,
            outcomeButtonRow
        };

        for (int i = 0; i < sections.Length; i++)
        {
            CanvasGroup group = EnsureCanvasGroup(sections[i]);
            if (group != null)
            {
                group.alpha = 0f;
            }
        }

        for (int i = 0; i < sections.Length; i++)
        {
            CanvasGroup group = EnsureCanvasGroup(sections[i]);
            if (group == null)
            {
                continue;
            }

            float elapsed = 0f;
            const float duration = 0.09f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                group.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }

            group.alpha = 1f;
            if (sections[i] == outcomeBadgesPanel)
            {
                PlaySound(FinalRoundSoundEvent.BadgeReveal);
            }

            yield return new WaitForSecondsRealtime(0.035f);
        }

        finalRevealCoroutine = null;
    }

    private void SetOutcomeSectionAlpha(float alpha)
    {
        GameObject[] sections =
        {
            outcomeTitleText == null ? null : outcomeTitleText.gameObject,
            outcomeBodyText == null ? null : outcomeBodyText.gameObject,
            outcomeStatsPanel,
            outcomeHighlightsPanel,
            outcomeBadgesPanel,
            outcomeAdvicePanel,
            outcomeButtonRow
        };

        for (int i = 0; i < sections.Length; i++)
        {
            CanvasGroup group = EnsureCanvasGroup(sections[i]);
            if (group != null)
            {
                group.alpha = alpha;
            }
        }
    }

    private void PlayUiSound(AudioClip clip, FinalRoundSoundEvent fallbackEvent = FinalRoundSoundEvent.UiClick)
    {
        if (audioManager == null)
        {
            return;
        }

        audioManager.Play(clip, fallbackEvent);
    }

    private void PlaySound(FinalRoundSoundEvent soundEvent)
    {
        if (audioManager == null)
        {
            return;
        }

        audioManager.Play(soundEvent);
    }

    private void OpenSettingsFromMenu()
    {
        settingsOpenedFromPause = false;
        if (settingsOverlay == null)
        {
            return;
        }

        RefreshSettingsControls();
        settingsOverlay.SetActive(true);
        FadeInScreen(settingsOverlay);
    }

    private void OpenSettingsFromPause()
    {
        settingsOpenedFromPause = true;
        HidePauseOverlay();
        if (settingsOverlay == null)
        {
            return;
        }

        RefreshSettingsControls();
        settingsOverlay.SetActive(true);
        FadeInScreen(settingsOverlay);
    }

    private void CloseSettings()
    {
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        if (settingsOpenedFromPause)
        {
            ShowPauseOverlay();
            return;
        }

        if (menuScreen != null)
        {
            menuScreen.SetActive(true);
        }
    }

    private void ResetSettingsDefaults()
    {
        masterVolume = 0.65f;
        sfxVolume = 0.8f;
        muteAudio = false;
        reduceMotion = false;
        useDeterministicRunSeed = false;
        debugRunSeed = 48291;
        fullscreenEnabled = true;

        SavePlayerSettings();
        ApplyAudioSettings();
        ApplyDisplaySettings();
        RefreshSettingsControls();
        LogSettingsApplied("defaults reset");
    }

    private void LoadPlayerSettings()
    {
        masterVolume = PlayerPrefs.GetFloat(PrefMasterVolume, masterVolume);
        sfxVolume = PlayerPrefs.GetFloat(PrefSfxVolume, sfxVolume);
        muteAudio = PlayerPrefs.GetInt(PrefMuteAudio, muteAudio ? 1 : 0) == 1;
        reduceMotion = PlayerPrefs.GetInt(PrefReduceMotion, reduceMotion ? 1 : 0) == 1;
        useDeterministicRunSeed = PlayerPrefs.GetInt(PrefDeterministicRunSeed, useDeterministicRunSeed ? 1 : 0) == 1;
        debugRunSeed = PlayerPrefs.GetInt(PrefRunSeed, debugRunSeed);
        fullscreenEnabled = PlayerPrefs.GetInt(PrefFullscreen, Screen.fullScreen ? 1 : 0) == 1;

        ApplyDisplaySettings();
        LogSettingsApplied("loaded");
    }

    private void SavePlayerSettings()
    {
        PlayerPrefs.SetFloat(PrefMasterVolume, masterVolume);
        PlayerPrefs.SetFloat(PrefSfxVolume, sfxVolume);
        PlayerPrefs.SetInt(PrefMuteAudio, muteAudio ? 1 : 0);
        PlayerPrefs.SetInt(PrefReduceMotion, reduceMotion ? 1 : 0);
        PlayerPrefs.SetInt(PrefDeterministicRunSeed, useDeterministicRunSeed ? 1 : 0);
        PlayerPrefs.SetInt(PrefRunSeed, debugRunSeed);
        PlayerPrefs.SetInt(PrefFullscreen, fullscreenEnabled ? 1 : 0);
        PlayerPrefs.Save();
    }

    private void RefreshSettingsControls()
    {
        if (masterVolumeSlider != null)
        {
            masterVolumeSlider.SetValueWithoutNotify(masterVolume);
        }

        if (sfxVolumeSlider != null)
        {
            sfxVolumeSlider.SetValueWithoutNotify(sfxVolume);
        }

        if (muteAudioToggle != null)
        {
            muteAudioToggle.SetIsOnWithoutNotify(muteAudio);
        }

        if (reduceMotionToggle != null)
        {
            reduceMotionToggle.SetIsOnWithoutNotify(reduceMotion);
        }

        if (deterministicSeedToggle != null)
        {
            deterministicSeedToggle.SetIsOnWithoutNotify(useDeterministicRunSeed);
        }

        if (fullscreenToggle != null)
        {
            fullscreenToggle.SetIsOnWithoutNotify(fullscreenEnabled);
        }

        if (seedInputField != null)
        {
            seedInputField.SetTextWithoutNotify(debugRunSeed.ToString());
        }

        UpdateSettingsValueLabels();
    }

    private void UpdateSettingsValueLabels()
    {
        if (masterVolumeValueText != null)
        {
            masterVolumeValueText.text = FormatPercent(masterVolume);
        }

        if (sfxVolumeValueText != null)
        {
            sfxVolumeValueText.text = FormatPercent(sfxVolume);
        }

        if (seedNoteText != null)
        {
            seedNoteText.text = useDeterministicRunSeed
                ? $"Seed changes apply to the next new process. Current seed: {debugRunSeed}."
                : "Seed changes apply to the next new process.";
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        UpdateSettingsValueLabels();
        SavePlayerSettings();
        ApplyAudioSettings();
    }

    private void OnSfxVolumeChanged(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        UpdateSettingsValueLabels();
        SavePlayerSettings();
        ApplyAudioSettings();
    }

    private void OnMuteAudioChanged(bool value)
    {
        muteAudio = value;
        SavePlayerSettings();
        ApplyAudioSettings();
    }

    private void OnReduceMotionChanged(bool value)
    {
        reduceMotion = value;
        SavePlayerSettings();
    }

    private void OnDeterministicSeedChanged(bool value)
    {
        useDeterministicRunSeed = value;
        SavePlayerSettings();
        UpdateSettingsValueLabels();
    }

    private void OnSeedInputChanged(string value)
    {
        if (!int.TryParse(value, out int parsedSeed))
        {
            RefreshSettingsControls();
            return;
        }

        debugRunSeed = Mathf.Max(1, parsedSeed);
        SavePlayerSettings();
        RefreshSettingsControls();
    }

    private void ApplySeedSettings()
    {
        OnSeedInputChanged(seedInputField == null ? debugRunSeed.ToString() : seedInputField.text);
        LogSettingsApplied("seed applied");
    }

    private void GenerateNewSettingsSeed()
    {
        debugRunSeed = Random.Range(10000, 999999);
        SavePlayerSettings();
        RefreshSettingsControls();
    }

    private void OnFullscreenChanged(bool value)
    {
        fullscreenEnabled = value;
        SavePlayerSettings();
        ApplyDisplaySettings();
    }

    private void ApplyAudioSettings()
    {
        if (audioManager != null)
        {
            audioManager.Configure(uiAudioSource, masterVolume, sfxVolume, muteAudio);
        }
    }

    private void ApplyDisplaySettings()
    {
        Screen.fullScreen = fullscreenEnabled;
    }

    private void LogSettingsApplied(string source)
    {
        Debug.Log(
            $"Settings {source}: audio master {FormatPercent(masterVolume)}, sfx {FormatPercent(sfxVolume)}, mute {muteAudio}; " +
            $"reduceMotion {reduceMotion}; deterministicSeed {useDeterministicRunSeed}, seed {debugRunSeed}; fullscreen {fullscreenEnabled}");
    }

    private string FormatPercent(float value)
    {
        return $"{Mathf.RoundToInt(Mathf.Clamp01(value) * 100f)}%";
    }

    private void BuildStages()
    {
        stages = new InterviewStage[]
        {
            new InterviewStage(
                "Recruiter Screen",
                "The recruiter is checking motivation, communication, and whether you understand the role.",
                "The recruiter has enough signal to decide whether you should meet the team.",
                new InterviewQuestion[]
                {
                    new InterviewQuestion(
                        "Why are you interested in this role?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I'm looking for a role where I can combine technical depth with commercial impact.", "The recruiter nods. You connected your motivation to the shape of the job.", 5, 0, 5, 10, diplomaticStyleChange: 1, commercialStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("The compensation looks better than my current role.", "Honest, but thin. The recruiter wanted a stronger reason to believe you will stay engaged.", -5, 0, 0, -10, bluntStyleChange: 1),
                            new AnswerOption("I'm not completely sure yet, but the recruiter message sounded interesting.", "The answer is casual. It keeps the call moving, but it does not create much momentum.", -5, 0, 0, -5, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "Tell me about your current role in two minutes.",
                        new AnswerOption[]
                        {
                            new AnswerOption("I support enterprise deals by mapping customer pain, shaping demos, and proving technical fit.", "Crisp and relevant. The recruiter can easily repeat that summary to the hiring manager.", 5, 0, 5, 10, commercialStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("It's complicated. I do a bit of everything depending on the week.", "The recruiter hears flexibility, but not a clear story.", -5, -5, 0, -5, burnedOutStyleChange: 1),
                            new AnswerOption("Mostly demos, some discovery, some internal calls, lots of context switching.", "Recognizable, but generic. You described activity more than impact.", 0, -5, 0, -5, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "What salary range are you targeting?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I'm flexible for the right opportunity, but based on scope I'm targeting the upper end of your posted range.", "Professional and grounded. You signal value without sounding slippery.", 5, 0, 0, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I need you to make the first offer.", "The recruiter notes the negotiation stance, but the tone feels a little rigid for an early screen.", -5, 0, 0, -5, bluntStyleChange: 1),
                            new AnswerOption("Honestly, as much as possible.", "The recruiter laughs, then moves on quickly. Not fatal, not especially strategic.", 0, 0, 0, -10, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "What are you hoping to avoid in your next role?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I want to avoid unclear ownership. I do my best work when discovery, technical validation, and customer outcomes are connected.", "The recruiter hears maturity rather than a complaint list. Sensible, which is rare enough to be memorable.", 5, 0, 0, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I'd like to avoid another place where every deal is on fire and nobody knows who promised what.", "Painfully relatable, but the recruiter files it under potentially carrying baggage.", -5, -5, 0, -5, bluntStyleChange: 1, burnedOutStyleChange: 1),
                            new AnswerOption("Mostly I want fewer internal meetings about meetings that failed to define the meeting.", "The recruiter smiles because it is true, then wonders if that joke was doing too much work.", 0, -5, 0, -5, chaoticStyleChange: 1, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How soon could you realistically start?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I would want to leave my current team cleanly, so four weeks is realistic and keeps the handover professional.", "Responsible answer. You sound like someone who does not disappear the moment paperwork appears.", 5, 0, 0, 5, diplomaticStyleChange: 1),
                            new AnswerOption("Technically two weeks, if everyone accepts that my handover will be a haunted spreadsheet.", "The recruiter appreciates the honesty, then writes down four weeks in their notes.", 0, -5, 0, -5, chaoticStyleChange: 1, burnedOutStyleChange: 1),
                            new AnswerOption("If the offer is right, I can be surprisingly available.", "Funny, but slightly mercenary. The recruiter has heard worse, usually from people wearing gilets.", 5, 0, 0, -5, bluntStyleChange: 1, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "What are your expectations around remote or hybrid work?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I work well remotely, and I am comfortable being in office when it improves collaboration, onboarding, or customer work.", "The recruiter hears flexibility with a reason behind it, which is more useful than a slogan about deep work.", 5, 0, 0, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("Hybrid is fine as long as the office days are not just Teams calls with worse coffee.", "Accurate enough to sting. The recruiter smiles, then gently checks whether you can say that with less seasoning.", 0, -5, 0, 0, chaoticStyleChange: 1),
                            new AnswerOption("I really prefer fully remote and would struggle if there were regular office expectations.", "Clear, but narrow. It may be true, but it gives the recruiter less room to position you.", -5, 0, 0, -10, bluntStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "What made you respond to this company specifically?",
                        new AnswerOption[]
                        {
                            new AnswerOption("The market problem is familiar, the buyer is technical, and the SE role looks close to revenue rather than just demo support.", "The recruiter gets a company-specific answer instead of a lightly reheated LinkedIn paragraph.", 5, 0, 5, 10, commercialStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("You seem less chaotic than some companies in this space, which I mean as a compliment.", "The recruiter laughs because the bar is real, but still wants evidence you did homework.", 0, 0, 0, -5, chaoticStyleChange: 1),
                            new AnswerOption("The job description looked solid, and honestly I am exploring a few things right now.", "Reasonable, but forgettable. The recruiter cannot do much with 'solid'.", -5, 0, 0, -5, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "Are you interviewing elsewhere right now?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I am having a few conversations, but this process is relevant because the SE role and buyer profile match what I want next.", "The recruiter hears market activity without feeling like they are being used as calendar filler.", 5, 0, 0, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("Yes, but nobody has successfully turned a job description into a real human conversation yet.", "The line has charm, but the recruiter still has to translate it into pipeline risk.", 0, 0, 0, -5, chaoticStyleChange: 1),
                            new AnswerOption("Yes, I am trying to create options because my current role is getting weird.", "Honest, but leaky. The recruiter now has more weather report than positioning.", -5, -5, 0, -5, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How do you feel about travel for customers or team events?",
                        new AnswerOption[]
                        {
                            new AnswerOption("Reasonable travel is fine when it improves customer trust, team ramp, or deal progress.", "Practical answer. You sound flexible without volunteering to live in airport seating.", 5, 0, 0, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I can travel, but I prefer when the meeting has a purpose beyond proving laptops are portable.", "The recruiter laughs and writes down 'reasonable travel' in the least dramatic way possible.", 0, -5, 0, 0, chaoticStyleChange: 1),
                            new AnswerOption("I would strongly prefer not to travel unless absolutely required.", "Clear boundary, but a little early. It may narrow the process before anyone has defined the ask.", -5, 0, 0, -10, bluntStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "If your current company countered, how would you think about it?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I would listen professionally, but I am looking at role fit, operating model, and growth, not just a last-minute adjustment.", "The recruiter hears that you have thought beyond the small theatre of retention budgets.", 5, 0, 0, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("A counteroffer would need to solve more than my salary, which is where most counteroffers quietly run out of road.", "Dry, accurate, and only slightly fatalistic. The recruiter seems to accept it.", 0, 0, 0, 5, bluntStyleChange: 1),
                            new AnswerOption("I suppose if they paid enough, I would have to think about it.", "True in the way gravity is true. The recruiter now has a retention risk-shaped note.", 0, 0, 0, -10, bluntStyleChange: 1)
                        })
                },
                4),

            new InterviewStage(
                "Hiring Manager",
                "The hiring manager is testing judgment, team fit, and how you operate inside a sales cycle.",
                "The hiring manager has a view on your operating style and whether they would trust you in front of customers.",
                new InterviewQuestion[]
                {
                    new InterviewQuestion(
                        "How do you partner with account executives?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I align early on qualification, discovery, success criteria, and the business outcome we're trying to prove.", "Strong answer. You sound like someone who can help a deal move with discipline.", 5, 0, 5, 15, diplomaticStyleChange: 1, commercialStyleChange: 2),
                            new AnswerOption("I prefer when they leave me alone until there's a demo.", "That got a laugh, but it also made you sound detached from the sales process.", 0, 5, 0, -15, bluntStyleChange: 1, chaoticStyleChange: 1),
                            new AnswerOption("It depends on the AE. Some need a lot of management.", "There may be truth there, but the manager hears friction before partnership.", -5, 0, 0, -10, bluntStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "The AE has promised a feature that does not exist. What do you do?",
                        new AnswerOption[]
                        {
                            new AnswerOption("Keep a united front, clarify the requirement, avoid overcommitting, and follow up internally after.", "The manager likes the balance. You protected trust without letting the deal drift into fiction.", 5, 0, 10, 15, diplomaticStyleChange: 1, commercialStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("Correct them immediately in front of the customer.", "Technically true, but the manager writes something down about executive presence.", -5, 0, 5, -10, bluntStyleChange: 2, technicalStyleChange: 1),
                            new AnswerOption("Say yes and hope Product builds it before the PoC.", "The room briefly goes colder. That answer sounds risky.", 0, -10, -15, -20, chaoticStyleChange: 2)
                        }),
                    new InterviewQuestion(
                        "How do you handle a deal where the customer has no clear success criteria?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I slow down, reframe the pain, and get agreement on what evidence would make the project worth buying.", "The manager hears process discipline and commercial maturity.", 5, 0, 5, 15, diplomaticStyleChange: 1, commercialStyleChange: 2),
                            new AnswerOption("I run the best demo I can and see what resonates.", "That can work, but it sounds reactive rather than controlled.", 0, -5, 0, -10, chaoticStyleChange: 1),
                            new AnswerOption("I ask the AE to sort that out before involving me.", "The manager sees boundary setting, but also a missed chance to lead discovery.", -5, 0, 0, -10, bluntStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "Your AE wants you on every discovery call. How do you handle that?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I agree where technical discovery will change qualification, and I help define when my involvement is useful.", "Good operating model. The manager hears partnership with boundaries, which is basically interview catnip.", 5, 0, 5, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I go, because if I say no I'll just get a Slack thread with twelve people in it.", "The manager has lived this, but it makes you sound governed by notification pressure.", -5, -10, 0, -5, burnedOutStyleChange: 2),
                            new AnswerOption("I tell the AE to bring me in after MEDDICC has survived first contact with reality.", "Sharp, funny, and maybe a little too inside-baseball. The manager enjoys it while noting the edge.", 0, 0, 0, -5, bluntStyleChange: 1, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How do you give feedback to an AE after a difficult customer call?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I separate what happened from blame, align on the customer risk, and agree what we change before the next call.", "The manager hears someone who can navigate sales politics without pretending politics do not exist.", 5, 0, 0, 15, diplomaticStyleChange: 1, commercialStyleChange: 2),
                            new AnswerOption("I am direct. If they talked over discovery for twenty minutes, I say that.", "Useful signal, abrasive delivery. The manager wonders how many 'quick syncs' follow you around.", -5, 0, 0, -5, bluntStyleChange: 2),
                            new AnswerOption("I write it in the shared notes so the truth has version control.", "Technically efficient, socially combustible. A beautiful way to start a tiny office war.", 0, 0, 5, -10, technicalStyleChange: 1, chaoticStyleChange: 2)
                        }),
                    new InterviewQuestion(
                        "How do you use MEDDPICC or qualification without turning it into theatre?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I treat it as a shared map: where technical proof supports pain, decision criteria, champion strength, and commercial urgency.", "The manager hears qualification as deal hygiene, not a spreadsheet everyone resents.", 5, 0, 5, 15, commercialStyleChange: 2, technicalStyleChange: 1),
                            new AnswerOption("I use it when the deal is real. Early stage MEDDPICC can become astrology with fields.", "The manager enjoys the line, then waits to see if you can still operate inside the process.", 0, 0, 0, 0, bluntStyleChange: 1, chaoticStyleChange: 1),
                            new AnswerOption("I mostly leave MEDDPICC to the AE because they own the forecast.", "Clean boundary, but too passive. The manager wanted technical qualification to show up in the forecast quality.", -5, 0, 0, -10, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How do you talk about burnout or load without sounding negative?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I focus on operating model: clear priorities, sustainable coverage, and using SE time where it changes deal quality.", "The manager hears self-awareness without a complaint cloud forming over the call.", 5, 5, 0, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I say I have learned the difference between urgency and every Slack message wearing a little hat.", "The joke lands, but the manager gently parks it under 'monitor for edge'.", 0, -5, 0, 0, chaoticStyleChange: 1, burnedOutStyleChange: 1),
                            new AnswerOption("I am trying to avoid another role where the calendar eats the actual job.", "Understandable, but raw. The manager hears a real concern before they hear a solution.", -5, -10, 0, -5, burnedOutStyleChange: 2)
                        }),
                    new InterviewQuestion(
                        "How do you decide where to spend SE effort when every AE says their deal is urgent?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I look at qualification, customer impact, technical risk, and whether my involvement changes the next decision.", "The manager hears discipline. You are not treating urgency as a calendar-shaped weather event.", 5, 0, 5, 15, commercialStyleChange: 2, technicalStyleChange: 1),
                            new AnswerOption("I ask which urgent deal has the clearest mutual action plan, because at least that kind of urgent has furniture.", "The manager enjoys the image and likes that you still found an operating principle.", 0, 0, 0, 5, chaoticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I usually help whoever is loudest, because that is how the Slack economy works.", "Recognizable, but not reassuring. The manager wants you above the noise, not rented by it.", -5, -10, 0, -10, burnedOutStyleChange: 2)
                        }),
                    new InterviewQuestion(
                        "A senior customer stakeholder dominates discovery and blocks the technical users. What do you do?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I acknowledge their priorities, then create space for the technical users by tying their input to risk and success criteria.", "The manager hears stakeholder control without making anyone feel publicly managed.", 5, 0, 5, 15, diplomaticStyleChange: 2, commercialStyleChange: 1),
                            new AnswerOption("I redirect politely and hope the word politely does a heroic amount of work.", "The manager laughs, then waits for a little more method than hope.", 0, 0, 0, 0, chaoticStyleChange: 1),
                            new AnswerOption("I let the AE handle it because that sounds like account politics.", "Boundary noted. Leadership opportunity missed.", -5, 0, 0, -10, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How would you ramp into a new market you have not sold into before?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I would learn buyer pains, common architecture patterns, objection language, and what proof actually changes decisions.", "The manager hears a useful ramp plan, not just 'I learn fast' wearing a blazer.", 5, 0, 5, 15, commercialStyleChange: 2, technicalStyleChange: 1),
                            new AnswerOption("I would listen to the best calls and steal only the parts that are legally culture.", "Possibly too honest, but the manager likes that you learn from real calls.", 0, 0, 0, 5, chaoticStyleChange: 1),
                            new AnswerOption("I would rely on the product demo until I understand the market better.", "Safe, but passive. The manager wanted buyer learning before demo muscle memory.", -5, 0, 0, -10, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "A competitor is spreading doubt in a deal. How do you respond with the AE?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I identify the specific concern, provide evidence, and help the AE re-anchor the decision criteria around the customer's outcomes.", "The manager hears calm competitive control rather than vendor gossip with slides.", 5, 0, 5, 15, commercialStyleChange: 2, diplomaticStyleChange: 1),
                            new AnswerOption("I ask what exactly they said, because sometimes competitor FUD arrives pre-weakened.", "The manager likes the instinct, even if the sentence could use less eyebrow.", 0, 0, 0, 5, bluntStyleChange: 1),
                            new AnswerOption("I explain why the competitor is wrong and move on.", "Tempting, but thin. The manager wanted you to manage doubt, not just swat at it.", 0, 0, 0, -10, bluntStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How do you define a good discovery call for an SE?",
                        new AnswerOption[]
                        {
                            new AnswerOption("It should reveal technical pain, business impact, stakeholders, decision criteria, and what proof would move the deal.", "The manager hears discovery that can actually steer a sales cycle.", 5, 0, 5, 15, commercialStyleChange: 2, technicalStyleChange: 1),
                            new AnswerOption("A good one is where I leave knowing what not to demo, which is half the battle and most of the therapy.", "Funny, and more strategic than it first sounds. The manager allows it.", 0, 0, 0, 5, chaoticStyleChange: 1),
                            new AnswerOption("If the customer gives us enough to build a demo, I count that as useful.", "Useful is not the same as good. The manager wanted sharper qualification.", -5, 0, 0, -10, burnedOutStyleChange: 1)
                        })
                },
                5),

            new InterviewStage(
                "Technical Panel",
                "The technical panel is looking for depth, clarity, and how you explain tradeoffs under pressure.",
                "The panel has tested whether your technical credibility holds up when the questions get sharper.",
                new InterviewQuestion[]
                {
                    new InterviewQuestion(
                        "How would you explain our platform architecture to a skeptical engineer?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I would start with the problem it solves, then map the data flow, trust boundaries, and failure modes.", "The panel likes the structure. You are technical without dumping a diagram on the table.", 5, 0, 15, 5, diplomaticStyleChange: 1, technicalStyleChange: 2),
                            new AnswerOption("I would walk through every component in detail from the docs.", "Accurate, but heavy. The panel worries you may lose the audience.", 0, -5, 10, -5, technicalStyleChange: 2, burnedOutStyleChange: 1),
                            new AnswerOption("I would keep it high level unless they ask questions.", "Safe, but a little vague. The panel wanted more confidence in your depth.", -5, 0, -5, 0, diplomaticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "A customer reports slow performance during a proof of concept. What do you check first?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I define the symptom, isolate where time is spent, and separate product behavior from customer environment issues.", "Good diagnostic shape. The panel hears calm troubleshooting, not guesswork.", 5, 0, 15, 5, technicalStyleChange: 2, diplomaticStyleChange: 1),
                            new AnswerOption("I ask engineering if there are known issues.", "Useful eventually, but the panel expected you to do more first-line analysis.", -5, 0, -5, 0, burnedOutStyleChange: 1),
                            new AnswerOption("I tell the customer performance can vary during PoCs.", "The panel does not love the deflection. It sounds like you are moving away from ownership.", -10, 0, -10, -10, burnedOutStyleChange: 1, bluntStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How do you explain a technical limitation without killing the deal?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I state the limitation clearly, tie it to the actual requirement, and offer a supported path or tradeoff.", "The panel likes the honesty and the commercial framing.", 5, 0, 10, 10, diplomaticStyleChange: 1, commercialStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("I avoid mentioning it unless the customer asks directly.", "The panel spots the risk. Avoidance can become a trust problem later.", -5, 0, -10, -10, chaoticStyleChange: 1),
                            new AnswerOption("I explain why the limitation probably does not matter.", "Maybe true, maybe not. The panel hears a shortcut instead of discovery.", 0, 0, 0, -10, bluntStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "The panel asks for a live troubleshooting walkthrough. What do you do?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I narrate my hypotheses, test the highest-signal checks first, and explain what would change my mind.", "Strong. You show technical method without turning the interview into a keyboard recital.", 5, 0, 15, 5, technicalStyleChange: 2, diplomaticStyleChange: 1),
                            new AnswerOption("I start with logs, metrics, traces, packet captures, and whatever else is available.", "Technically defensible, but the panel sees a warehouse of tools before they see a diagnosis.", 0, -5, 10, -5, technicalStyleChange: 2),
                            new AnswerOption("I say live troubleshooting is how dashboards become crime scenes, but yes, let's go.", "The joke lands with the engineers. The hiring manager quietly ages four months.", 5, -5, 5, -5, chaoticStyleChange: 2, technicalStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How do you know when a proof of concept is technically successful?",
                        new AnswerOption[]
                        {
                            new AnswerOption("When we have proven the agreed success criteria and can connect the evidence to the buying decision.", "The panel hears the rare sound of technical work tied to revenue reality.", 5, 0, 10, 15, commercialStyleChange: 2, technicalStyleChange: 1),
                            new AnswerOption("When the architecture works cleanly and the edge cases are understood.", "Correct, but incomplete. The panel wants the buyer's decision in the answer, not just the system state.", 0, 0, 10, -10, technicalStyleChange: 2),
                            new AnswerOption("When nobody adds 'just one more thing' to the success criteria document.", "The panel laughs in a tired way. Everyone has seen that document become a garden shed.", 0, -5, 0, -5, burnedOutStyleChange: 1, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "A customer asks for a feature that is not on the roadmap. How do you handle it?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I confirm the underlying requirement, explain the current gap plainly, and look for a supported workflow or product feedback path.", "The panel likes that you did not turn a missing feature into either panic or vapor.", 5, 0, 10, 10, diplomaticStyleChange: 1, technicalStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I say Product has heard similar requests, then immediately check whether that sentence is still legally alive.", "The panel laughs, but also notices you know the danger zone.", 0, 0, 0, -5, chaoticStyleChange: 1),
                            new AnswerOption("I tell them we can probably make it work with services.", "Maybe, maybe not. The panel hears a custom commitment trying to sneak out wearing a hoodie.", 0, -5, -10, -10, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How would you adapt a technical demo for a CISO who joins late?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I quickly reset the agenda around risk, controls, evidence, and governance, then use the product details only to support those points.", "The panel sees audience control without abandoning technical credibility.", 5, 0, 10, 10, diplomaticStyleChange: 1, commercialStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("I ask what they care about most and then cut whatever no longer serves that answer.", "Pragmatic and a little brisk. The panel likes the instinct, even if the phrasing has sharp elbows.", 0, 0, 5, 5, bluntStyleChange: 1),
                            new AnswerOption("I keep going but mention security more often.", "The panel does not love the find-and-replace approach to executive relevance.", -5, 0, -5, -10, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "The demo environment breaks five minutes before the customer call. What do you do?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I triage the failure, switch to a credible backup path, and tell the AE exactly what story still holds.", "The panel likes the calm recovery plan. Nobody needs heroics if the plan has bones.", 5, 0, 15, 5, technicalStyleChange: 2, diplomaticStyleChange: 1),
                            new AnswerOption("I keep a backup recording because live demos are just agreements with weather.", "The panel smiles. Preparedness counts, even with the ominous phrasing.", 0, 0, 10, 0, chaoticStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("I ask to reschedule rather than risk showing something broken.", "Sometimes right, but too soon. The panel wanted recovery options before retreat.", -5, -5, -5, -10, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How do you explain an API security tradeoff to a technical buyer?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I describe the threat model, auth boundary, operational tradeoff, and what control the customer retains.", "The panel hears security depth without turning the answer into a standards annex.", 5, 0, 15, 5, technicalStyleChange: 2, diplomaticStyleChange: 1),
                            new AnswerOption("I start with the architecture diagram and let the awkward arrows tell me where to go.", "Technically plausible, but the panel wants you driving before the diagram starts freelancing.", 0, 0, 5, 0, chaoticStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("I reassure them our defaults are secure.", "The panel wanted evidence, not a throw pillow with 'secure by default' stitched on it.", -5, 0, -10, -5, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "The customer's demo data makes the product look weaker than it is. How do you handle it?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I explain the data constraint, show what is still valid, and agree what better evidence would look like.", "The panel likes the honesty. You protected trust and kept the proof path alive.", 5, 0, 10, 10, diplomaticStyleChange: 1, commercialStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("I say the data is doing performance art and then move to a cleaner example.", "The line lands, but the panel watches whether you can still preserve customer dignity.", 0, 0, 5, 0, chaoticStyleChange: 1),
                            new AnswerOption("I avoid drawing attention to it and hope the customer does not notice.", "The panel notices. The customer would too, which is sort of the problem.", -5, 0, -10, -10, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "A technical buyer asks for a roadmap commitment in writing. What do you do?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I separate current capability from roadmap interest, avoid committing, and route the request through the right product process.", "The panel hears clean governance. Not glamorous, extremely useful.", 5, 0, 10, 10, diplomaticStyleChange: 1, technicalStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I say I can capture it as feedback, which is the safest sentence in enterprise software.", "Accurate, perhaps too spiritually tired, but safe.", 0, 0, 0, 0, burnedOutStyleChange: 1),
                            new AnswerOption("I say it is likely, but I would need to confirm internally.", "The panel hears the little door where future escalation walks in.", 0, -5, -10, -10, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How do you turn technical proof into business value after a PoC?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I map the evidence back to agreed pain, success criteria, risk reduction, and the decision the customer needs to make.", "The panel hears the bridge from working software to buying logic.", 5, 0, 10, 15, commercialStyleChange: 2, technicalStyleChange: 1),
                            new AnswerOption("I write the technical win clearly enough that Sales cannot turn it into interpretive dance.", "The panel laughs, and also appreciates the handoff discipline.", 0, 0, 5, 5, chaoticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I hand over the technical findings and let the AE build the business case.", "Clear boundary, but incomplete. The panel wanted you in the translation layer.", -5, 0, 0, -10, burnedOutStyleChange: 1)
                        })
                },
                5),

            new InterviewStage(
                "VP Round",
                "The VP is testing executive presence, business judgment, and whether you can represent the company at senior levels.",
                "The VP has enough signal to make the final call.",
                new InterviewQuestion[]
                {
                    new InterviewQuestion(
                        "A CISO joins your technical demo unexpectedly. What changes?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I lift the conversation up to risk, control, auditability, and cost of inaction, then go deep only where needed.", "The VP leans forward. You shifted from feature tour to executive relevance.", 5, 0, 10, 15, diplomaticStyleChange: 1, commercialStyleChange: 2, technicalStyleChange: 1),
                            new AnswerOption("Nothing. I prepared the demo and I'm sticking to it.", "The VP sees consistency, but not much audience awareness.", 0, 0, -5, -10, technicalStyleChange: 1, bluntStyleChange: 1),
                            new AnswerOption("I speed up the architecture section so they see we are serious.", "The VP hears effort, but not enough control of the room.", -10, -10, 0, -5, chaoticStyleChange: 2, technicalStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "What makes you different from other SEs?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I build rapport quickly, stay technically grounded, and translate complex systems into business value.", "Clean finish. The answer sounds specific, credible, and easy to repeat in feedback.", 10, 0, 10, 10, diplomaticStyleChange: 1, commercialStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("I'm just really hardworking.", "Nobody dislikes the answer, but nobody can really remember it either.", -5, -5, 0, 0, burnedOutStyleChange: 1),
                            new AnswerOption("I use AI for everything now, so technically I'm many SEs.", "The VP laughs, then gently wonders whether you have an answer without the punchline.", 5, 0, -5, -5, chaoticStyleChange: 2)
                        }),
                    new InterviewQuestion(
                        "Why should we hire you?",
                        new AnswerOption[]
                        {
                            new AnswerOption("Because I can earn technical trust, sharpen the commercial case, and help customers make confident decisions.", "The VP has the line they needed. It sounds like a final-round answer.", 10, 0, 10, 15, diplomaticStyleChange: 1, commercialStyleChange: 2, technicalStyleChange: 1),
                            new AnswerOption("Because I really want this and I know I can learn fast.", "Sincere, but more junior than the role needs at this stage.", 0, -5, -5, -5, burnedOutStyleChange: 1),
                            new AnswerOption("Because I think I would be better than most candidates.", "Confident, but unsupported. The VP waits for proof that never quite arrives.", 5, 0, 0, -10, bluntStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "Where do you think Sales Engineering creates the most leverage?",
                        new AnswerOption[]
                        {
                            new AnswerOption("At the point where customer pain, technical proof, and commercial urgency become the same conversation.", "The VP gets the headline they were hoping for. Annoyingly polished, but in a useful way.", 10, 0, 5, 15, commercialStyleChange: 2, diplomaticStyleChange: 1),
                            new AnswerOption("In making sure Sales does not sell vapor and Product does not build theatre.", "The VP enjoys the sentence, then wonders how often you say the quiet part in public.", 0, 0, 5, -5, bluntStyleChange: 2),
                            new AnswerOption("In being the adult in the room, provided the room has budget for an adult.", "Funny and not wrong. Also a tiny flare fired over your relationship with ambiguity.", 5, -5, 0, 0, chaoticStyleChange: 1, burnedOutStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "If you joined, what would your first 90 days look like?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I would learn the product, shadow strong calls, map the sales motion, and start contributing where I can create low-risk customer value.", "The VP hears momentum without bravado. A tidy answer, which is underrated in final rounds.", 10, 0, 5, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I would rebuild the demo story once I understand what buyers actually care about.", "Potentially valuable, but spicy. The VP wonders whether you have met the people who own the current demo.", 0, 0, 5, 0, bluntStyleChange: 1, technicalStyleChange: 1),
                            new AnswerOption("I would try not to become the person everyone forwards weird RFP questions to by week three.", "The room laughs because that person exists. The VP still wants an answer with a little more altitude.", 0, -5, 0, -5, burnedOutStyleChange: 1, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "How do you handle a competitive deal where the other vendor is technically strong?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I stay honest about parity, find the customer's decision criteria, and prove where our approach changes risk, speed, or business outcome.", "The VP hears competitive discipline instead of feature jousting in a nicer shirt.", 10, 0, 5, 15, commercialStyleChange: 2, diplomaticStyleChange: 1),
                            new AnswerOption("I avoid trashing the competitor unless they have really earned it, which unfortunately some do.", "The VP appreciates restraint right up until the sentence takes a scenic route.", 0, 0, 0, 0, bluntStyleChange: 1, chaoticStyleChange: 1),
                            new AnswerOption("I focus on our roadmap and try to make the gap feel temporary.", "That can sound hopeful, but the VP hears future tense doing too much sales work.", -5, 0, -5, -10, chaoticStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "If we moved to offer, how would you think about timing and notice?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I would want to move decisively, align on details quickly, and give professional notice so I can start cleanly.", "The VP hears momentum with adult supervision, a surprisingly marketable combination.", 10, 0, 0, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I can move fast if the numbers, scope, and paperwork all decide to be adults at the same time.", "Fair, dry, and only mildly haunted by procurement energy. The VP takes the point.", 5, 0, 0, 0, chaoticStyleChange: 1),
                            new AnswerOption("I would need to see the offer before I can say anything real.", "True, but closed down. The VP wanted practical readiness, not a locked filing cabinet.", -5, 0, 0, -5, bluntStyleChange: 1)
                        }),
                    new InterviewQuestion(
                        "What risk should we see in hiring you?",
                        new AnswerOption[]
                        {
                            new AnswerOption("I can move quickly toward the customer problem, so I manage that by aligning early on qualification, proof, and internal expectations.", "The VP hears self-awareness with a control attached. This is much better than pretending risk is for other candidates.", 10, 0, 5, 10, diplomaticStyleChange: 1, commercialStyleChange: 1),
                            new AnswerOption("I may challenge vague deal logic sooner than everyone finds comfortable.", "The VP appreciates the signal and mentally adds 'handle with manager context'.", 5, 0, 0, 0, bluntStyleChange: 1),
                            new AnswerOption("I sometimes take on too much because I want the deal to work.", "Human, but a little familiar. The VP has seen that become a calendar with teeth.", 0, -10, 0, -5, burnedOutStyleChange: 2)
                        })
                },
                3)
        };
    }

    private void BuildRandomEvents()
    {
        randomEvents = new RandomInterviewEvent[]
        {
            new RandomInterviewEvent(
                "Recruiter Delay",
                "Feedback is positive, but nobody replies for 9 days. You begin reading meaning into punctuation.",
                confidenceChange: -5,
                energyChange: -5,
                burnedOutStyleChange: 1),

            new RandomInterviewEvent(
                "Salary Range Revealed",
                "The compensation is actually in range. Suddenly the process has a little more oxygen in it.",
                confidenceChange: 5,
                energyChange: 5,
                commercialStyleChange: 1),

            new RandomInterviewEvent(
                "Hybrid Twist",
                "Remote means two days a week in London. The word remote is doing a lot of work there.",
                energyChange: -8,
                burnedOutStyleChange: 2),

            new RandomInterviewEvent(
                "Surprise CISO",
                "A senior security leader is joining the next round. The stakes go up, and so does the need to talk business impact.",
                energyChange: -5,
                commercialAlignmentChange: 5,
                commercialStyleChange: 1),

            new RandomInterviewEvent(
                "Take-Home Expands",
                "The task now includes a demo, written notes, and a roleplay. Somehow the assignment has developed side quests.",
                energyChange: -8,
                burnedOutStyleChange: 1),

            new RandomInterviewEvent(
                "Another Iron in the Fire",
                "Another company books a call. You remember that this process is not the only door in the building.",
                confidenceChange: 10,
                commercialStyleChange: 1),

            new RandomInterviewEvent(
                "Recruiter Doorstop Call",
                "The recruiter calls without warning while you're making lunch. You answer with one hand on the mute button and one eye on the pan.",
                energyChange: -5,
                chaoticStyleChange: 1)
        };
    }

    private void BuildCompanyProfiles()
    {
        companyProfiles = new CompanyProfile[]
        {
            new CompanyProfile(
                "Big SaaS Vendor",
                "Balanced Enterprise Motion",
                "They care about consistent discovery, credible demos, and a clean partnership with sales.",
                "Fewer random events; offer recommendation bar is higher.",
                "Balanced, commercially aware answers land well.",
                "Structured Process",
                "Fewer surprises, but the panel expects a cleaner overall signal before recommending offer.",
                "Fewer surprises, higher bar.",
                randomEventChanceModifier: -0.08f,
                offerRecommendedThresholdModifier: 10),
            new CompanyProfile(
                "Startup Rocketship",
                "Fast, Messy, Urgent",
                "They care about pace, ownership, and whether you can keep signal through moving parts.",
                "Random events are more likely; positive recovery effects are stronger; event energy losses are sharper.",
                "Decisive answers help, but chaos has a cost.",
                "High Chaos, High Recovery",
                "More process volatility, stronger recovery moves, and a slightly harsher energy hit when surprises land.",
                "More chaos, but recovery choices hit harder.",
                randomEventChanceModifier: 0.12f,
                eventEnergyLossModifier: -2,
                recoveryPositiveEffectBonus: 2),
            new CompanyProfile(
                "Security Vendor",
                "Risk-Framing Process",
                "They care about risk framing, technical credibility, and clean customer communication.",
                "Offer recommendation also requires Technical Credibility and Commercial Alignment to both clear 60.",
                "Technical and commercial credibility are especially important signals.",
                "Risk Framing Matters",
                "The panel will not recommend offer unless technical depth and business risk framing both survive scrutiny.",
                "Technical depth and business risk both matter.",
                requiresTechnicalAndCommercialOfferGate: true),
            new CompanyProfile(
                "AI Hype Company",
                "Narrative-Heavy Growth Motion",
                "They care about vision, speed, and whether you can stay grounded when the room starts saying agentic.",
                "A little chaos is tolerated; too much creates credibility risk.",
                "Grounded enthusiasm beats demo-theatre.",
                "Chaos Can Sell",
                "Early chaotic style is forgiven, and the first risky chaotic answer can create confidence, but excess chaos damages credibility.",
                "A little chaos helps. Too much becomes the product strategy.",
                chaoticStyleForgiveness: 3,
                firstChaoticAnswerConfidenceBonus: 2,
                aiChaoticStyleBonus: 1),
            new CompanyProfile(
                "Legacy Enterprise",
                "Careful Procurement Maze",
                "They care about patience, stakeholder management, and whether you can keep energy through process drag.",
                "Random chaos is slightly lower; between-stage energy recovery is reduced.",
                "Steady diplomatic answers travel best.",
                "Slow Process",
                "Fewer external surprises, but the process itself gives less energy back between rounds.",
                "Less chaos, more stamina tax.",
                randomEventChanceModifier: -0.12f,
                betweenStageEnergyRecoveryModifier: -3)
        };
    }

    private bool ValidateGameData()
    {
        if (stages == null || stages.Length == 0)
        {
            Debug.LogError("Final Round setup error: no interview stages were created.");
            return false;
        }

        for (int stageIndex = 0; stageIndex < stages.Length; stageIndex++)
        {
            InterviewStage stage = stages[stageIndex];

            if (stage == null)
            {
                Debug.LogError($"Final Round setup error: stage {stageIndex} is null.");
                return false;
            }

            if (stage.QuestionPool == null || stage.QuestionPool.Length == 0)
            {
                Debug.LogError($"Final Round setup error: '{stage.StageName}' has no questions.");
                return false;
            }

            if (stage.QuestionsToAskThisRun <= 0)
            {
                Debug.LogError($"Final Round setup error: '{stage.StageName}' must ask at least one question.");
                return false;
            }

            for (int questionIndex = 0; questionIndex < stage.QuestionPool.Length; questionIndex++)
            {
                InterviewQuestion question = stage.QuestionPool[questionIndex];

                if (question == null)
                {
                    Debug.LogError($"Final Round setup error: question {questionIndex} in '{stage.StageName}' is null.");
                    return false;
                }

                if (question.Answers == null || question.Answers.Length != 3)
                {
                    Debug.LogError($"Final Round setup error: '{stage.StageName}' question {questionIndex + 1} must have exactly 3 answers for the current UI.");
                    return false;
                }
            }
        }

        return true;
    }

    private void ResetGame(bool chooseNewCompanyProfile)
    {
        if (chooseNewCompanyProfile || runRandom == null)
        {
            InitializeRunIdentity();
            SelectRandomCompanyProfile();
        }

        SelectStageQuestionsForRun();
        displayedAnswers = null;
        stageRunSummaries.Clear();
        randomEventRunSummaries.Clear();
        usedRandomEventIndexes.Clear();
        InitializeRecoveryChoices();
        InitializePrepCards();
        strongAnswerCount = 0;
        riskyAnswerCount = 0;
        interviewPressure = StartingInterviewPressure;
        firstChaoticAnswerBonusApplied = false;
        highPressureWarningPlayed = false;
        activeRuleRunNotes.Clear();
        playerStats.Reset(StartingConfidence, StartingEnergy, StartingTechnicalCredibility, StartingCommercialAlignment);
        styleTracker.Reset();
        currentStageIndex = 0;
        currentQuestionIndex = 0;
        CaptureStageStartStats();
    }

    public void StartInterviewFromRoomSeat()
    {
        if (!HasRequiredUi())
        {
            Debug.LogWarning("Final Round room prototype could not start the interview because runtime UI is not ready.");
            return;
        }

        HidePauseOverlay();
        if (runRandom == null || activeCompanyProfile == null)
        {
            InitializeRunIdentity();
            SelectRandomCompanyProfile();
        }

        ResetGame(false);
        SetRuntimeBackgroundVisible(true);
        BeginProcess();
    }

    public void SetRoomObjectiveText(string objectiveText)
    {
        if (subtitleText != null)
        {
            subtitleText.text = objectiveText;
        }

        if (progressText != null)
        {
            progressText.text = "The Room";
        }
    }

    public void ResetRoomPrototypeRun()
    {
        if (!HasRequiredUi())
        {
            return;
        }

        HidePauseOverlay();
        ResetGame(true);
        ShowRoomStandby();
    }

    public bool IsRoomUiFocusActive()
    {
        return (pauseOverlay != null && pauseOverlay.activeSelf)
            || (settingsOverlay != null && settingsOverlay.activeSelf)
            || (menuScreen != null && menuScreen.activeSelf)
            || (processBriefingScreen != null && processBriefingScreen.activeSelf);
    }

    private bool IsRoomPrototypeScene()
    {
        return FindAnyObjectByType<TheRoomPrototypeController>() != null;
    }

    private void ShowRoomStandby()
    {
        PrepareMenuRunPreview();
        HidePauseOverlay();

        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(false);
        questionScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
        recoveryChoiceScreen.SetActive(false);
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(false);
        SetRuntimeBackgroundVisible(false);

        SetRoomObjectiveText("Find the interview chair.");
        UpdateRoomBackdrop("Main Menu");
    }

    private void SetRuntimeBackgroundVisible(bool visible)
    {
        if (runtimeBackgroundPanel != null)
        {
            runtimeBackgroundPanel.SetActive(visible);
        }
    }

    private void InitializeRecoveryChoices()
    {
        recoveryChoices = new RecoveryChoice[]
        {
            new RecoveryChoice(
                RecoveryChoiceType.ReviewNotes,
                "Review Notes",
                "+6 Technical Credibility. +1 Technical style.",
                "You review your notes. The next round feels a little more structured.",
                0, 0, 6, 0,
                0, 0, 0, 1, 0, 0,
                false),
            new RecoveryChoice(
                RecoveryChoiceType.ReframeBusinessCase,
                "Reframe the Business Case",
                "+6 Commercial Alignment. +1 Commercial style.",
                "You tighten the business case. The next answer has a clearer reason to exist.",
                0, 0, 0, 6,
                0, 0, 1, 0, 0, 0,
                false),
            new RecoveryChoice(
                RecoveryChoiceType.TakeAWalk,
                "Take a Walk",
                "+10 Energy. -1 Burned Out style.",
                "You step away from the screen long enough to reset your attention.",
                0, 10, 0, 0,
                0, 0, 0, 0, 0, -1,
                false),
            new RecoveryChoice(
                RecoveryChoiceType.MessageFriendlyAe,
                "Message a Friendly AE",
                "+4 Confidence, +4 Commercial Alignment. +1 Diplomatic and +1 Commercial style.",
                "The AE gives you just enough field context to sound less like you are interviewing in a vacuum.",
                4, 0, 0, 4,
                1, 0, 1, 0, 0, 0,
                false),
            new RecoveryChoice(
                RecoveryChoiceType.DoomScrollGlassdoor,
                "Doom-scroll Glassdoor",
                "-5 Confidence, -5 Energy. +2 Burned Out style.",
                "You learn several things you cannot verify and none of them help.",
                -5, -5, 0, 0,
                0, 0, 0, 0, 0, 2,
                true)
        };

        recoveryChoicesMadeCount = 0;
        doomScrollChoiceCount = 0;
    }

    private void InitializePrepCards()
    {
        prepCards = new PrepCard[]
        {
            new PrepCard(PrepCardType.TakeABreath, "Take a Breath", "+8 Energy immediately.", 2),
            new PrepCard(PrepCardType.ClarifyingQuestion, "Ask a Clarifying Question", "Marks the most commercially aligned answer. Costs -3 Energy.", 2),
            new PrepCard(PrepCardType.ReframeBusinessValue, "Reframe to Business Value", "Next selected answer gets +5 Commercial Alignment.", 1)
        };

        prepCardsUsedCount = 0;
        reframedCommercialAlignmentBonus = 0;
        clarifiedAnswerIndex = -1;
        prepCardFeedbackNote = string.Empty;
    }

    private void SelectStageQuestionsForRun()
    {
        if (stages == null)
        {
            selectedStageQuestions = null;
            return;
        }

        selectedStageQuestions = new InterviewQuestion[stages.Length][];

        for (int stageIndex = 0; stageIndex < stages.Length; stageIndex++)
        {
            InterviewStage stage = stages[stageIndex];
            InterviewQuestion[] pool = stage.QuestionPool ?? new InterviewQuestion[0];
            int drawCount = Mathf.Min(stage.QuestionsToAskThisRun, pool.Length);

            if (pool.Length < stage.QuestionsToAskThisRun)
            {
                Debug.LogWarning($"Final Round setup warning: '{stage.StageName}' has only {pool.Length} pooled questions, fewer than requested draw count {stage.QuestionsToAskThisRun}. Asking all available questions.");
            }

            selectedStageQuestions[stageIndex] = DrawQuestionsWithoutReplacement(pool, drawCount);
        }
    }

    private InterviewQuestion[] DrawQuestionsWithoutReplacement(InterviewQuestion[] pool, int drawCount)
    {
        InterviewQuestion[] shuffledPool = new InterviewQuestion[pool.Length];
        pool.CopyTo(shuffledPool, 0);

        for (int i = shuffledPool.Length - 1; i > 0; i--)
        {
            int swapIndex = GetRunRandomIndex(i + 1);
            (shuffledPool[i], shuffledPool[swapIndex]) = (shuffledPool[swapIndex], shuffledPool[i]);
        }

        InterviewQuestion[] selectedQuestions = new InterviewQuestion[drawCount];
        for (int i = 0; i < drawCount; i++)
        {
            selectedQuestions[i] = shuffledPool[i];
        }

        return selectedQuestions;
    }

    private void InitializeRunIdentity()
    {
        if (useDeterministicRunSeed)
        {
            currentRunSeed = debugRunSeed;
        }
        else if (useDeterministicAnswerSeed)
        {
            currentRunSeed = debugAnswerSeed;
        }
        else
        {
            currentRunSeed = Random.Range(10000, 99999);
        }

        runRandom = new System.Random(currentRunSeed);
        currentProcessId = FormatProcessId(currentRunSeed);
        UpdateVersionLabel();
    }

    private string FormatProcessId(int seed)
    {
        int displaySeed = Mathf.Abs(seed % 100000);
        return $"FR-{displaySeed:00000}";
    }

    private void SelectRandomCompanyProfile()
    {
        if (companyProfiles == null || companyProfiles.Length == 0)
        {
            activeCompanyProfile = null;
            return;
        }

        if (runRandom == null)
        {
            InitializeRunIdentity();
        }

        if (useDebugCompanyProfile)
        {
            int clampedIndex = Mathf.Clamp(debugCompanyProfileIndex, 0, companyProfiles.Length - 1);
            activeCompanyProfile = companyProfiles[clampedIndex];
            return;
        }

        activeCompanyProfile = companyProfiles[runRandom.Next(companyProfiles.Length)];
    }

    private void PrepareMenuRunPreview()
    {
        InitializeRunIdentity();
        SelectRandomCompanyProfile();
    }

    private void RestartGame()
    {
        HidePauseOverlay();
        ResetGame(true);
        ShowProcessBriefing();
    }

    private void StartInterviewProcess()
    {
        HidePauseOverlay();
        if (runRandom == null || activeCompanyProfile == null)
        {
            InitializeRunIdentity();
            SelectRandomCompanyProfile();
        }

        ResetGame(false);
        if (IsRoomPrototypeScene())
        {
            ShowRoomStandby();
            return;
        }

        ShowProcessBriefing();
    }

    private void BeginProcess()
    {
        HidePauseOverlay();
        PlayUiSound(continueClip, FinalRoundSoundEvent.Continue);
        ShowQuestionScreen();
    }

    private void ShowProcessBriefing()
    {
        HidePauseOverlay();
        SetRuntimeBackgroundVisible(true);
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(true);
        questionScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
        recoveryChoiceScreen.SetActive(false);
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(false);

        progressText.text = "Process Briefing";
        subtitleText.text = "Read the room before the first call.";
        processBriefingTitleText.text = "TODAY'S PROCESS";
        processBriefingBodyText.text = BuildProcessBriefingBody();
        UpdateRoomBackdrop("Main Menu");
        LogRunStart();
        FadeInScreen(processBriefingScreen);
    }

    private void ShowMenu()
    {
        PrepareMenuRunPreview();
        HidePauseOverlay();
        SetRuntimeBackgroundVisible(true);
        if (settingsOverlay != null)
        {
            settingsOverlay.SetActive(false);
        }

        menuScreen.SetActive(true);
        processBriefingScreen.SetActive(false);
        questionScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
        recoveryChoiceScreen.SetActive(false);
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(false);

        progressText.text = "Main Menu";
        subtitleText.text = "Choose when to begin the process.";
        menuBodyText.text =
            "A short interview process about confidence, stamina, technical credibility, and commercial judgment.\n\n" +
            GetCompanyProfileSummary();
        UpdateRoomBackdrop("Main Menu");
        FadeInScreen(menuScreen);
    }

    private void ShowHowToPlay()
    {
        menuBodyText.text =
            "How To Play\n\n" +
            "Choose answers with the mouse or number keys 1, 2, and 3.\n\n" +
            "Use Q, W, and E to play Prep Cards before answering.\n\n" +
            "Between rounds, use 1 through 5 to choose a recovery move.\n\n" +
            "Manage Confidence, Energy, Technical Credibility, and Commercial Alignment.\n\n" +
            "Random events may affect the process between stages.\n\n" +
            "The final outcome depends on total score, weak stats, interview style, and the active company process.\n\n" +
            GetCompanyProfileSummary();
    }

    private void ShowAbout()
    {
        menuBodyText.text =
            "About\n\n" +
            $"{BuildVersion}\n\n" +
            "Final Round is a compact interview prototype about reading the room, staying sharp, and balancing technical and commercial signals.\n\n" +
            "Created as a playable Unity demo with runtime UI, a cosmetic interview-call panel, and subtle audio feedback.";
    }

    private string GetCompanyProfileSummary()
    {
        if (activeCompanyProfile == null)
        {
            return "Today's process: Unknown";
        }

        return
            $"Today's process: {activeCompanyProfile.CompanyName}\n" +
            $"{activeCompanyProfile.Description}\n" +
            $"Rule: {activeCompanyProfile.RuleName} - {activeCompanyProfile.RuleHint}\n" +
            $"{activeCompanyProfile.StatModifierNotes}";
    }

    private string GetCompanyProcessLine()
    {
        if (activeCompanyProfile == null)
        {
            return string.Empty;
        }

        return $"{activeCompanyProfile.CompanyName} | {activeCompanyProfile.RuleName}: {activeCompanyProfile.RuleHint}";
    }

    private string BuildProcessBriefingBody()
    {
        if (activeCompanyProfile == null)
        {
            return
                "Today's process: Unknown\n\n" +
                "The panel seems to have lost the briefing pack. Somehow, this is still an interview signal.";
        }

        return
            $"Process ID: <b>{currentProcessId}</b>\n" +
            $"Run seed: {currentRunSeed}\n\n" +
            $"Today's process: <b>{activeCompanyProfile.CompanyName}</b>\n" +
            $"{activeCompanyProfile.ProfileName}\n\n" +
            $"{activeCompanyProfile.Description}\n\n" +
            $"Rewards: {activeCompanyProfile.PreferredStyle}\n" +
            $"Active rule: <b>{activeCompanyProfile.RuleName}</b>\n" +
            $"{activeCompanyProfile.RuleDescription}\n" +
            $"Hint: {activeCompanyProfile.RuleHint}\n\n" +
            $"Modifier note: {activeCompanyProfile.StatModifierNotes}\n\n" +
            GetProcessBriefingFlavourLine();
    }

    private string GetProcessBriefingFlavourLine()
    {
        if (activeCompanyProfile == null)
        {
            return string.Empty;
        }

        return activeCompanyProfile.CompanyName switch
        {
            "Startup Rocketship" => "Warning: the calendar may move faster than the facts.",
            "Security Vendor" => "Warning: vague risk framing will be politely disassembled.",
            "AI Hype Company" => "Warning: enthusiasm is useful; floating away is less useful.",
            "Legacy Enterprise" => "Warning: patience is part of the product evaluation.",
            _ => "Warning: balanced answers travel furthest in a calibrated process."
        };
    }

    private void HandleKeyboardShortcuts()
    {
        if (!HasRequiredUi())
        {
            return;
        }

        if (WasEscapePressed())
        {
            HandleEscapeShortcut();
            return;
        }

        if (pauseOverlay.activeSelf)
        {
            return;
        }

        if (WasEnterPressed())
        {
            HandleEnterShortcut();
            return;
        }

        if (recoveryChoiceScreen.activeSelf)
        {
            if (WasNumberShortcutPressed(1))
            {
                ChooseRecoveryChoice(0);
            }
            else if (WasNumberShortcutPressed(2))
            {
                ChooseRecoveryChoice(1);
            }
            else if (WasNumberShortcutPressed(3))
            {
                ChooseRecoveryChoice(2);
            }
            else if (WasNumberShortcutPressed(4))
            {
                ChooseRecoveryChoice(3);
            }
            else if (WasNumberShortcutPressed(5))
            {
                ChooseRecoveryChoice(4);
            }

            return;
        }

        if (WasPrepCardShortcutPressed(0))
        {
            UsePrepCard(0);
            return;
        }

        if (WasPrepCardShortcutPressed(1))
        {
            UsePrepCard(1);
            return;
        }

        if (WasPrepCardShortcutPressed(2))
        {
            UsePrepCard(2);
            return;
        }

        if (WasNumberShortcutPressed(1))
        {
            TrySelectAnswerByShortcut(0);
        }
        else if (WasNumberShortcutPressed(2))
        {
            TrySelectAnswerByShortcut(1);
        }
        else if (WasNumberShortcutPressed(3))
        {
            TrySelectAnswerByShortcut(2);
        }
    }

    private bool WasEscapePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    private bool WasEnterPressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null
            && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.numpadEnterKey.wasPressedThisFrame);
#else
        return Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
#endif
    }

    private bool WasNumberShortcutPressed(int number)
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
        {
            return false;
        }

        return number switch
        {
            1 => Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame,
            2 => Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame,
            3 => Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame,
            4 => Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame,
            5 => Keyboard.current.digit5Key.wasPressedThisFrame || Keyboard.current.numpad5Key.wasPressedThisFrame,
            _ => false
        };
#else
        return number switch
        {
            1 => Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1),
            2 => Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2),
            3 => Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3),
            4 => Input.GetKeyDown(KeyCode.Alpha4) || Input.GetKeyDown(KeyCode.Keypad4),
            5 => Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5),
            _ => false
        };
#endif
    }

    private bool WasPrepCardShortcutPressed(int cardIndex)
    {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current == null)
        {
            return false;
        }

        return cardIndex switch
        {
            0 => Keyboard.current.qKey.wasPressedThisFrame,
            1 => Keyboard.current.wKey.wasPressedThisFrame,
            2 => Keyboard.current.eKey.wasPressedThisFrame,
            _ => false
        };
#else
        return cardIndex switch
        {
            0 => Input.GetKeyDown(KeyCode.Q),
            1 => Input.GetKeyDown(KeyCode.W),
            2 => Input.GetKeyDown(KeyCode.E),
            _ => false
        };
#endif
    }

    private void HandleEscapeShortcut()
    {
        if (settingsOverlay != null && settingsOverlay.activeSelf)
        {
            CloseSettings();
            return;
        }

        if (pauseOverlay.activeSelf)
        {
            ResumeFromPause();
            return;
        }

        if (menuScreen.activeSelf || processBriefingScreen.activeSelf)
        {
            ShowMenu();
            return;
        }

        ShowPauseOverlay();
    }

    private void HandleEnterShortcut()
    {
        if (processBriefingScreen.activeSelf && beginProcessButton.interactable)
        {
            BeginProcess();
            return;
        }

        if (questionScreen.activeSelf && feedbackPanel.activeSelf && continueButton.interactable)
        {
            ContinueAfterFeedback();
            return;
        }

        if (stageTransitionScreen.activeSelf && stageContinueButton.interactable)
        {
            ContinueAfterStageTransition();
            return;
        }

        if (randomEventScreen.activeSelf && randomEventContinueButton.interactable)
        {
            ContinueAfterRandomEvent();
            return;
        }

        if (recoveryChoiceScreen.activeSelf
            && recoveryChoiceContinueButton.gameObject.activeSelf
            && recoveryChoiceContinueButton.interactable)
        {
            ContinueAfterRecoveryChoice();
        }
    }

    private void TrySelectAnswerByShortcut(int answerIndex)
    {
        if (!questionScreen.activeSelf
            || feedbackPanel.activeSelf
            || answerButtons == null
            || answerIndex < 0
            || answerIndex >= answerButtons.Length
            || !answerButtons[answerIndex].interactable)
        {
            return;
        }

        ChooseAnswer(answerIndex);
    }

    private void ShowPauseOverlay()
    {
        if (pauseOverlay == null)
        {
            return;
        }

        pauseOverlay.SetActive(true);
    }

    private void HidePauseOverlay()
    {
        if (pauseOverlay != null)
        {
            pauseOverlay.SetActive(false);
        }
    }

    private void ResumeFromPause()
    {
        HidePauseOverlay();
    }

    private void ReturnToMenuFromPause()
    {
        HidePauseOverlay();
        ShowMenu();
    }

    private void QuitGame()
    {
        Application.Quit();
    }

    private void CaptureStageStartStats()
    {
        stageStartStats = playerStats.Copy();
        currentStageWasStrong = false;
    }

    private void ShowQuestionScreen()
    {
        HidePauseOverlay();
        SetRuntimeBackgroundVisible(true);
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(false);
        questionScreen.SetActive(true);
        stageTransitionScreen.SetActive(false);
        recoveryChoiceScreen.SetActive(false);
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(false);
        ShowCurrentQuestion();
        UpdateStatsText();
        FadeInScreen(questionScreen);
    }

    private void ShowCurrentQuestion()
    {
        if (stages == null || currentStageIndex < 0 || currentStageIndex >= stages.Length)
        {
            Debug.LogError("Final Round runtime error: current stage index is invalid.");
            return;
        }

        InterviewStage stage = stages[currentStageIndex];
        InterviewQuestion[] stageQuestions = GetSelectedQuestionsForStage(currentStageIndex);

        if (currentQuestionIndex >= stageQuestions.Length)
        {
            ShowStageTransition();
            return;
        }

        InterviewQuestion question = stageQuestions[currentQuestionIndex];
        displayedAnswers = BuildDisplayedAnswerOrder(question);
        progressText.text = $"{stage.StageName} - Question {currentQuestionIndex + 1} of {stageQuestions.Length}";
        subtitleText.text = GetCompanyProcessLine();
        UpdateRoomBackdrop(stage.StageName);
        SetRoomBackdropSpeaking(true);
        questionStageNameText.text = stage.StageName.ToUpperInvariant();
        questionStageIntroText.text = stage.StageIntroText;
        questionText.text = question.QuestionText;
        feedbackPanel.SetActive(false);
        CanvasGroup feedbackGroup = EnsureCanvasGroup(feedbackPanel);
        if (feedbackGroup != null)
        {
            feedbackGroup.alpha = 1f;
        }
        prepCardsPanel.SetActive(true);
        SetBackdropViewportVisible(true);
        reframedCommercialAlignmentBonus = 0;
        clarifiedAnswerIndex = -1;
        prepCardFeedbackNote = string.Empty;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].interactable = true;
            CanvasGroup answerGroup = EnsureCanvasGroup(answerButtons[i].gameObject);
            if (answerGroup != null)
            {
                answerGroup.alpha = 1f;
            }
            answerButtons[i].GetComponent<RectTransform>().localScale = Vector3.one;
            SetAnswerButtonText(i);
            SetAnswerButtonVisual(i, false);
        }

        UpdatePrepCardButtons(true);
        AnimateAnswerCardsIn();
    }

    private AnswerOption[] BuildDisplayedAnswerOrder(InterviewQuestion question)
    {
        AnswerOption[] answers = new AnswerOption[question.Answers.Length];
        question.Answers.CopyTo(answers, 0);

        if (!randomizeAnswerOrder || answers.Length <= 1)
        {
            return answers;
        }

        if (runRandom == null)
        {
            InitializeRunIdentity();
        }

        for (int i = answers.Length - 1; i > 0; i--)
        {
            int swapIndex = runRandom.Next(i + 1);
            (answers[i], answers[swapIndex]) = (answers[swapIndex], answers[i]);
        }

        return answers;
    }

    private void ChooseAnswer(int answerIndex)
    {
        InterviewQuestion question = GetSelectedQuestionsForStage(currentStageIndex)[currentQuestionIndex];
        if (displayedAnswers == null || displayedAnswers.Length != question.Answers.Length)
        {
            Debug.LogError("Final Round runtime error: displayed answer order was not prepared for the current question.");
            return;
        }

        AnswerOption answer = displayedAnswers[answerIndex];
        int commercialAlignmentModifier = reframedCommercialAlignmentBonus;
        int confidenceModifier = GetFirstChaoticAnswerConfidenceBonus(answer);
        int pressureChange = CalculateAnswerPressureChange(answer, confidenceModifier, commercialAlignmentModifier);
        if (commercialAlignmentModifier > 0)
        {
            pressureChange -= 3;
        }

        PlayUiSound(answerSelectedClip, FinalRoundSoundEvent.AnswerSelected);
        playerStats.ApplyDirectChanges(
            answer.ConfidenceChange + confidenceModifier,
            answer.EnergyChange,
            answer.TechnicalCredibilityChange,
            answer.CommercialAlignmentChange + commercialAlignmentModifier);
        pressureChange = ApplyPressureChange(pressureChange);
        TrackAnswerSummary(answer);
        styleTracker.Apply(answer);
        ApplyCompanyStyleModifier(answer);
        prepCardsPanel.SetActive(false);
        UpdatePrepCardButtons(false);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool selected = i == answerIndex;
            answerButtons[i].interactable = false;
            SetAnswerButtonVisual(i, selected);
            SetAnswerButtonText(i);
        }

        UpdateStatsText();
        FlashStatsText(
            GetLargestAbsoluteStatChange(
                answer.ConfidenceChange + confidenceModifier,
                answer.EnergyChange,
                answer.TechnicalCredibilityChange,
                answer.CommercialAlignmentChange + commercialAlignmentModifier),
            pressureChange);
        AnimateAnswerSelection(answerIndex);
        ShowFeedback(answer, commercialAlignmentModifier, confidenceModifier, pressureChange);
        reframedCommercialAlignmentBonus = 0;
    }

    private void UsePrepCard(int cardIndex)
    {
        if (!CanUsePrepCards() || prepCards == null || cardIndex < 0 || cardIndex >= prepCards.Length)
        {
            return;
        }

        PrepCard card = prepCards[cardIndex];
        if (card.RemainingUses <= 0)
        {
            return;
        }

        card.RemainingUses--;
        card.UseCount++;
        prepCardsUsedCount++;
        PlayUiSound(prepCardUsedClip, FinalRoundSoundEvent.PrepCardUsed);

        switch (card.CardType)
        {
            case PrepCardType.TakeABreath:
                playerStats.ApplyDirectChanges(0, 8, 0, 0);
                int breathPressureChange = ApplyPressureChange(-8);
                AppendPrepCardFeedbackNote($"You take a breath. Energy +8. Interview Pressure {FormatSignedNumber(breathPressureChange)}.");
                break;
            case PrepCardType.ClarifyingQuestion:
                playerStats.ApplyDirectChanges(0, -3, 0, 0);
                int clarifyPressureChange = ApplyPressureChange(-2);
                clarifiedAnswerIndex = GetHighestCommercialAlignmentAnswerIndex();
                AppendPrepCardFeedbackNote($"Prep Card used: Ask a Clarifying Question (-3 Energy, Interview Pressure {FormatSignedNumber(clarifyPressureChange)}).");
                break;
            case PrepCardType.ReframeBusinessValue:
                reframedCommercialAlignmentBonus += 5;
                AppendPrepCardFeedbackNote("Prep Card used: Reframe to Business Value (+5 Commercial Alignment and Interview Pressure -3 on the selected answer).");
                break;
        }

        UpdateStatsText();
        FlashStatsText(0, GetPrepCardFlashPressureChange(card.CardType));
        UpdateAnswerButtonLabels();
        UpdateAnswerButtonVisuals();
        UpdatePrepCardButtons(true);
    }

    private int GetPrepCardFlashPressureChange(PrepCardType cardType)
    {
        switch (cardType)
        {
            case PrepCardType.TakeABreath:
                return -8;
            case PrepCardType.ClarifyingQuestion:
                return -2;
            default:
                return 0;
        }
    }

    private bool CanUsePrepCards()
    {
        return questionScreen.activeSelf
            && prepCardsPanel.activeSelf
            && feedbackPanel != null
            && !feedbackPanel.activeSelf
            && pauseOverlay != null
            && !pauseOverlay.activeSelf
            && displayedAnswers != null;
    }

    private void AppendPrepCardFeedbackNote(string note)
    {
        if (string.IsNullOrEmpty(prepCardFeedbackNote))
        {
            prepCardFeedbackNote = note;
            return;
        }

        prepCardFeedbackNote += "\n" + note;
    }

    private int GetHighestCommercialAlignmentAnswerIndex()
    {
        int bestIndex = 0;
        int bestCommercialAlignment = int.MinValue;

        for (int i = 0; i < displayedAnswers.Length; i++)
        {
            if (displayedAnswers[i].CommercialAlignmentChange > bestCommercialAlignment)
            {
                bestCommercialAlignment = displayedAnswers[i].CommercialAlignmentChange;
                bestIndex = i;
            }
        }

        return bestIndex;
    }

    private void UpdateAnswerButtonLabels()
    {
        if (answerButtonTexts == null)
        {
            return;
        }

        for (int i = 0; i < answerButtonTexts.Length; i++)
        {
            SetAnswerButtonText(i);
        }
    }

    private void UpdateAnswerButtonVisuals()
    {
        if (answerButtons == null)
        {
            return;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            SetAnswerButtonVisual(i, false);
        }
    }

    private void SetAnswerButtonText(int answerIndex)
    {
        string prefix = answerIndex == clarifiedAnswerIndex ? "<color=#5CBCA4><b>Clarified lead:</b></color> " : string.Empty;
        answerButtonTexts[answerIndex].text = prefix + displayedAnswers[answerIndex].AnswerText;
    }

    private void UpdatePrepCardButtons(bool canUseCards)
    {
        if (prepCardButtons == null || prepCards == null)
        {
            return;
        }

        for (int i = 0; i < prepCardButtons.Length; i++)
        {
            PrepCard card = prepCards[i];
            bool hasUses = card.RemainingUses > 0;
            prepCardButtons[i].interactable = canUseCards && hasUses;
            prepCardTexts[i].text =
                $"<b>{card.Name}</b>\n" +
                $"{card.Description}\n" +
                GetPrepCardStatusText(card);
            prepCardTexts[i].color = hasUses ? textColor : mutedTextColor;
        }
    }

    private string GetPrepCardStatusText(PrepCard card)
    {
        if (card.RemainingUses <= 0)
        {
            return "<color=#ACB5C4><b>Exhausted</b></color>";
        }

        if (card.CardType == PrepCardType.ReframeBusinessValue && reframedCommercialAlignmentBonus > 0)
        {
            return "<color=#5CBCA4><b>Pending: next answer gains +5 Commercial Alignment</b></color>";
        }

        if (card.CardType == PrepCardType.ClarifyingQuestion && clarifiedAnswerIndex >= 0)
        {
            return "<color=#5CBCA4><b>Active: commercial lead marked</b></color>";
        }

        return $"Uses remaining: <b>{card.RemainingUses}/{card.MaxUses}</b>";
    }

    private InterviewQuestion[] GetSelectedQuestionsForStage(int stageIndex)
    {
        if (selectedStageQuestions != null
            && stageIndex >= 0
            && stageIndex < selectedStageQuestions.Length
            && selectedStageQuestions[stageIndex] != null)
        {
            return selectedStageQuestions[stageIndex];
        }

        return stages[stageIndex].QuestionPool;
    }

    private void TrackAnswerSummary(AnswerOption answer)
    {
        int totalStatChange = GetAnswerStatImpact(answer);

        if (totalStatChange >= 15 && answer.ChaoticStyleChange == 0 && answer.BurnedOutStyleChange == 0)
        {
            strongAnswerCount++;
        }

        if (ShouldTreatAnswerAsRisky(answer, totalStatChange))
        {
            riskyAnswerCount++;
        }
    }

    private bool ShouldTreatAnswerAsRisky(AnswerOption answer, int totalStatChange)
    {
        if (totalStatChange < 0 || answer.BurnedOutStyleChange > 0 || answer.BluntStyleChange > 1)
        {
            return true;
        }

        if (answer.ChaoticStyleChange <= 0)
        {
            return false;
        }

        int forgiveness = !useCompanyProfileModifiers || activeCompanyProfile == null
            ? 0
            : activeCompanyProfile.ChaoticStyleForgiveness;
        bool forgiven = styleTracker.ChaoticStyle < forgiveness;

        if (forgiven)
        {
            AddRuleRunNoteOnce("Early chaotic answers were treated as sellable energy.");
        }

        return !forgiven;
    }

    private int GetAnswerStatImpact(AnswerOption answer)
    {
        return answer.ConfidenceChange
            + answer.EnergyChange
            + answer.TechnicalCredibilityChange
            + answer.CommercialAlignmentChange;
    }

    private void ApplyCompanyStyleModifier(AnswerOption answer)
    {
        if (activeCompanyProfile == null || activeCompanyProfile.AiChaoticStyleBonus <= 0)
        {
            return;
        }

        if (!useCompanyProfileModifiers)
        {
            return;
        }

        if (answer.ChaoticStyleChange > 0 && IsAiOrChaoticSignal(answer.AnswerText + " " + answer.ConsequenceText))
        {
            styleTracker.AddChaoticStyle(activeCompanyProfile.AiChaoticStyleBonus);
        }
    }

    private int GetFirstChaoticAnswerConfidenceBonus(AnswerOption answer)
    {
        if (!useCompanyProfileModifiers
            || activeCompanyProfile == null
            || activeCompanyProfile.FirstChaoticAnswerConfidenceBonus <= 0
            || firstChaoticAnswerBonusApplied
            || answer.ChaoticStyleChange <= 0)
        {
            return 0;
        }

        firstChaoticAnswerBonusApplied = true;
        AddRuleRunNoteOnce($"{activeCompanyProfile.RuleName}: first risky chaotic answer landed for Confidence {FormatSignedNumber(activeCompanyProfile.FirstChaoticAnswerConfidenceBonus)}.");
        return activeCompanyProfile.FirstChaoticAnswerConfidenceBonus;
    }

    private bool IsAiOrChaoticSignal(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return false;
        }

        string lowerText = text.ToLowerInvariant();
        return lowerText.Contains("ai")
            || lowerText.Contains("agentic")
            || lowerText.Contains("chaotic")
            || lowerText.Contains("side quests")
            || lowerText.Contains("without warning");
    }

    private void ShowFeedback(AnswerOption answer, int commercialAlignmentModifier, int confidenceModifier, int pressureChange)
    {
        feedbackText.text =
            $"{answer.ConsequenceText}\n\n" +
            "<b>Stat changes</b>\n" +
            $"{FormatStatChange("Confidence", answer.ConfidenceChange + confidenceModifier)}\n" +
            $"{FormatStatChange("Energy", answer.EnergyChange)}\n" +
            $"{FormatStatChange("Technical Credibility", answer.TechnicalCredibilityChange)}\n" +
            $"{FormatStatChange("Commercial Alignment", answer.CommercialAlignmentChange + commercialAlignmentModifier)}\n" +
            $"{FormatStatChange("Interview Pressure", pressureChange)}" +
            BuildAnswerRuleFeedback(confidenceModifier);

        feedbackPrepCardText.text = string.IsNullOrEmpty(prepCardFeedbackNote)
            ? "<b>Prep cards</b>\nNo prep card effects on this answer."
            : $"<b>Prep cards</b>\n{prepCardFeedbackNote}";

        feedbackPanel.SetActive(true);
        prepCardsPanel.SetActive(false);
        SetRoomBackdropSpeaking(false);
        SetBackdropViewportVisible(false);
        RevealFeedbackPanel();
    }

    private string BuildAnswerRuleFeedback(int confidenceModifier)
    {
        if (confidenceModifier == 0 || activeCompanyProfile == null)
        {
            return string.Empty;
        }

        return $"\n\n<b>{activeCompanyProfile.RuleName}</b>\nThat risky answer created useful room energy. Confidence {FormatSignedNumber(confidenceModifier)}.";
    }

    private void SetBackdropViewportVisible(bool visible)
    {
        if (backdropViewportPanel != null)
        {
            backdropViewportPanel.SetActive(visible);
        }
    }

    private void UpdateCallPanel(string stageName)
    {
        activeCallStageName = string.IsNullOrEmpty(stageName) ? "Main Menu" : stageName;

        if (callPanelRoot == null || callParticipantTiles == null)
        {
            return;
        }

        CallTheme theme = GetCallTheme();
        int participantCount = GetCallParticipantCount(activeCallStageName);
        bool showParticipants = participantCount > 0;

        callPanelBackgroundImage.color = theme.BackgroundColor;
        callPanelFrameImage.color = ApplyPressureToFrameColor(theme.FrameColor);
        callTopBarImage.color = theme.TopBarColor;
        callStageLabelText.text = GetCallStageTitle(activeCallStageName);
        callStageLabelText.color = Color.Lerp(textColor, theme.AccentColor, 0.12f);
        callStatusLabelText.text = showParticipants ? "LIVE CALL" : "STANDBY";
        callStatusLabelText.color = Color.Lerp(mutedTextColor, theme.AccentColor, 0.35f);

        string[] labels = GetCallParticipantLabels(activeCallStageName);
        Rect[] layouts = GetCallParticipantLayout(participantCount);

        for (int i = 0; i < callParticipantTiles.Length; i++)
        {
            CallParticipantTile tile = callParticipantTiles[i];
            bool isVisible = showParticipants && i < participantCount;
            tile.Root.SetActive(isVisible);

            if (!isVisible)
            {
                continue;
            }

            ApplyTileRect(tile.Root.GetComponent<RectTransform>(), layouts[i]);
            tile.RoleText.text = labels[i];
            tile.TileImage.color = Color.Lerp(GetCallTileColor(i), theme.AccentColor, 0.08f);
            tile.BodyImage.color = Color.Lerp(GetCallBodyColor(i), theme.AccentColor, 0.18f);
            tile.ShoulderImage.color = Color.Lerp(tile.BodyImage.color, Color.white, 0.08f);
            tile.HeadImage.color = GetCallHeadColor(i);
            tile.LabelBarImage.color = Color.Lerp(new Color32(9, 15, 24, 245), theme.AccentColor, 0.08f);
            tile.RoleText.color = textColor;
            tile.BorderImage.color = theme.AccentColor;
            tile.BorderImage.gameObject.SetActive(callInterviewerSpeaking && i == GetCallActiveSpeakerIndex());
            tile.CanvasGroup.alpha = callInterviewerSpeaking && i != GetCallActiveSpeakerIndex() ? 0.76f : 1f;
            ApplyAvatarLayout(tile, participantCount);
        }

        callStandbyCard.SetActive(!showParticipants);
        if (callStandbyCard.activeSelf)
        {
            callStandbyCard.GetComponent<Image>().color = Color.Lerp(new Color32(34, 46, 62, 255), theme.AccentColor, 0.12f);
            callStandbyText.text = activeCallStageName == "Final Outcome" ? "FINAL DECISION" : "BETWEEN ROUNDS";
            callStandbyText.color = textColor;
        }
    }

    private void AnimateCallPanel()
    {
        if (callPanelRoot == null || callParticipantTiles == null || !callPanelRoot.activeInHierarchy)
        {
            return;
        }

        int activeIndex = GetCallActiveSpeakerIndex();
        float pressureTension = Mathf.InverseLerp(45f, 100f, interviewPressure);
        float speed = Mathf.Lerp(1.6f, 3.4f, pressureTension);
        float pulse = callInterviewerSpeaking && !reduceMotion ? 0.5f + Mathf.Sin(Time.unscaledTime * speed) * 0.5f : 0f;
        CallTheme theme = GetCallTheme();

        callPanelFrameImage.color = Color.Lerp(
            ApplyPressureToFrameColor(theme.FrameColor),
            theme.AccentColor,
            pressureTension * (0.08f + pulse * 0.1f));

        for (int i = 0; i < callParticipantTiles.Length; i++)
        {
            CallParticipantTile tile = callParticipantTiles[i];
            if (tile == null || !tile.Root.activeSelf)
            {
                continue;
            }

            bool active = callInterviewerSpeaking && i == activeIndex;
            tile.BorderImage.gameObject.SetActive(active);
            tile.BorderImage.color = Color.Lerp(theme.AccentColor, Color.white, active ? pulse * 0.18f : 0f);
            tile.CanvasGroup.alpha = active || !callInterviewerSpeaking ? 1f : 0.74f;
            tile.TileImage.color = active
                ? Color.Lerp(GetCallTileColor(i), theme.AccentColor, 0.14f + pressureTension * 0.08f + pulse * 0.04f)
                : Color.Lerp(GetCallTileColor(i), Color.black, 0.16f);
        }
    }

    private void ApplyTileRect(RectTransform rectTransform, Rect normalizedRect)
    {
        rectTransform.anchorMin = new Vector2(normalizedRect.xMin, normalizedRect.yMin);
        rectTransform.anchorMax = new Vector2(normalizedRect.xMax, normalizedRect.yMax);
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private void ApplyAvatarLayout(CallParticipantTile tile, int participantCount)
    {
        bool largeTile = participantCount == 1;
        RectTransform bodyRect = tile.BodyImage.GetComponent<RectTransform>();
        bodyRect.anchorMin = largeTile ? new Vector2(0.34f, 0.32f) : new Vector2(0.27f, 0.3f);
        bodyRect.anchorMax = largeTile ? new Vector2(0.66f, 0.66f) : new Vector2(0.73f, 0.65f);
        bodyRect.offsetMin = Vector2.zero;
        bodyRect.offsetMax = Vector2.zero;

        RectTransform shoulderRect = tile.ShoulderImage.GetComponent<RectTransform>();
        shoulderRect.anchorMin = largeTile ? new Vector2(0.39f, 0.58f) : new Vector2(0.35f, 0.56f);
        shoulderRect.anchorMax = largeTile ? new Vector2(0.61f, 0.66f) : new Vector2(0.65f, 0.65f);
        shoulderRect.offsetMin = Vector2.zero;
        shoulderRect.offsetMax = Vector2.zero;

        RectTransform headRect = tile.HeadImage.GetComponent<RectTransform>();
        headRect.anchorMin = largeTile ? new Vector2(0.39f, 0.58f) : new Vector2(0.36f, 0.56f);
        headRect.anchorMax = largeTile ? new Vector2(0.61f, 0.82f) : new Vector2(0.64f, 0.82f);
        headRect.offsetMin = Vector2.zero;
        headRect.offsetMax = Vector2.zero;
        tile.RoleText.fontSize = largeTile ? 30 : 22;
    }

    private int GetCallActiveSpeakerIndex()
    {
        int participantCount = GetCallParticipantCount(activeCallStageName);
        if (participantCount <= 1)
        {
            return 0;
        }

        float interval = Mathf.Lerp(4.6f, 2.8f, Mathf.InverseLerp(55f, 100f, interviewPressure));
        return Mathf.FloorToInt(Time.unscaledTime / interval) % participantCount;
    }

    private int GetCallParticipantCount(string stageName)
    {
        switch (stageName)
        {
            case "Recruiter Screen":
            case "Hiring Manager":
                return 1;
            case "Technical Panel":
                return 3;
            case "VP Round":
                return 2;
            default:
                return 0;
        }
    }

    private string[] GetCallParticipantLabels(string stageName)
    {
        switch (stageName)
        {
            case "Recruiter Screen":
                return new[] { "Recruiter", string.Empty, string.Empty };
            case "Hiring Manager":
                return new[] { "Hiring Manager", string.Empty, string.Empty };
            case "Technical Panel":
                return new[] { "Solutions Lead", "Security Architect", "SE Manager" };
            case "VP Round":
                return new[] { "VP Sales Engineering", "Regional Director", string.Empty };
            default:
                return new[] { string.Empty, string.Empty, string.Empty };
        }
    }

    private Rect[] GetCallParticipantLayout(int participantCount)
    {
        if (participantCount == 1)
        {
            return new[] { new Rect(0.04f, 0.05f, 0.92f, 0.90f), Rect.zero, Rect.zero };
        }

        if (participantCount == 2)
        {
            return new[] { new Rect(0.025f, 0.055f, 0.465f, 0.89f), new Rect(0.51f, 0.055f, 0.465f, 0.89f), Rect.zero };
        }

        return new[]
        {
            new Rect(0.01f, 0.055f, 0.315f, 0.89f),
            new Rect(0.3425f, 0.055f, 0.315f, 0.89f),
            new Rect(0.675f, 0.055f, 0.315f, 0.89f)
        };
    }

    private string GetCallStageTitle(string stageName)
    {
        switch (stageName)
        {
            case "Recruiter Screen":
                return "RECRUITER SCREEN";
            case "Hiring Manager":
                return "HIRING MANAGER CALL";
            case "Technical Panel":
                return "TECHNICAL PANEL";
            case "VP Round":
                return "VP ROUND";
            case "Final Outcome":
                return "FINAL DECISION";
            case "Between Rounds":
                return "BETWEEN ROUNDS";
            default:
                return "LIVE INTERVIEW";
        }
    }

    private Color GetCallTileColor(int participantIndex)
    {
        switch (participantIndex)
        {
            case 1:
                return new Color32(64, 78, 98, 255);
            case 2:
                return new Color32(62, 82, 92, 255);
            default:
                return new Color32(72, 90, 112, 255);
        }
    }

    private Color GetCallBodyColor(int participantIndex)
    {
        switch (participantIndex)
        {
            case 1:
                return new Color32(88, 118, 152, 255);
            case 2:
                return new Color32(92, 128, 120, 255);
            default:
                return new Color32(104, 138, 176, 255);
        }
    }

    private Color GetCallHeadColor(int participantIndex)
    {
        switch (participantIndex)
        {
            case 1:
                return new Color32(226, 232, 236, 255);
            case 2:
                return new Color32(218, 228, 220, 255);
            default:
                return new Color32(232, 238, 246, 255);
        }
    }

    private CallTheme GetCallTheme()
    {
        string companyName = activeCompanyProfile == null ? string.Empty : activeCompanyProfile.CompanyName;
        switch (companyName)
        {
            case "Startup Rocketship":
                return new CallTheme(new Color32(42, 34, 38, 255), new Color32(74, 48, 42, 255), new Color32(235, 112, 72, 255), new Color32(92, 58, 48, 255));
            case "Security Vendor":
                return new CallTheme(new Color32(16, 28, 36, 255), new Color32(20, 48, 54, 255), new Color32(64, 220, 166, 255), new Color32(42, 90, 92, 255));
            case "AI Hype Company":
                return new CallTheme(new Color32(28, 24, 46, 255), new Color32(44, 32, 76, 255), new Color32(116, 236, 255, 255), new Color32(72, 48, 112, 255));
            case "Legacy Enterprise":
                return new CallTheme(new Color32(56, 54, 50, 255), new Color32(68, 66, 60, 255), new Color32(166, 158, 138, 255), new Color32(92, 88, 78, 255));
            case "Big SaaS Vendor":
                return new CallTheme(new Color32(26, 38, 52, 255), new Color32(42, 56, 74, 255), new Color32(92, 154, 218, 255), new Color32(58, 94, 126, 255));
            default:
                return new CallTheme(new Color32(20, 31, 47, 255), new Color32(40, 54, 72, 255), accentColor, new Color32(68, 112, 128, 255));
        }
    }

    private Color ApplyPressureToFrameColor(Color baseColor)
    {
        float pressureTension = Mathf.InverseLerp(55f, 100f, interviewPressure);
        return Color.Lerp(baseColor, new Color32(210, 80, 112, 255), pressureTension * 0.28f);
    }

    private string FormatStatChange(string statName, int change)
    {
        string sign = change > 0 ? "+" : string.Empty;
        string color = statName == "Interview Pressure"
            ? GetPressureChangeColor(change)
            : GetStatChangeColor(change);
        return $"{statName}: <color=#{color}><b>{sign}{change}</b></color>";
    }

    private string GetStatChangeColor(int change)
    {
        Color color = neutralStatColor;

        if (change > 0)
        {
            color = positiveStatColor;
        }
        else if (change < 0)
        {
            color = negativeStatColor;
        }

        return ColorUtility.ToHtmlStringRGB(color);
    }

    private string GetPressureChangeColor(int change)
    {
        if (change > 0)
        {
            return ColorUtility.ToHtmlStringRGB(negativeStatColor);
        }

        if (change < 0)
        {
            return ColorUtility.ToHtmlStringRGB(positiveStatColor);
        }

        return ColorUtility.ToHtmlStringRGB(neutralStatColor);
    }

    private string BuildChangeSummary(
        int confidenceChange,
        int energyChange,
        int technicalCredibilityChange,
        int commercialAlignmentChange,
        int diplomaticStyleChange,
        int bluntStyleChange,
        int commercialStyleChange,
        int technicalStyleChange,
        int chaoticStyleChange,
        int burnedOutStyleChange,
        int pressureChange = 0)
    {
        string summary = string.Empty;
        AppendChangeLine(ref summary, "Confidence", confidenceChange);
        AppendChangeLine(ref summary, "Energy", energyChange);
        AppendChangeLine(ref summary, "Technical Credibility", technicalCredibilityChange);
        AppendChangeLine(ref summary, "Commercial Alignment", commercialAlignmentChange);
        AppendChangeLine(ref summary, "Interview Pressure", pressureChange);
        AppendChangeLine(ref summary, "Diplomatic Style", diplomaticStyleChange);
        AppendChangeLine(ref summary, "Blunt Style", bluntStyleChange);
        AppendChangeLine(ref summary, "Commercial Style", commercialStyleChange);
        AppendChangeLine(ref summary, "Technical Style", technicalStyleChange);
        AppendChangeLine(ref summary, "Chaotic Style", chaoticStyleChange);
        AppendChangeLine(ref summary, "Burned Out Style", burnedOutStyleChange);

        if (string.IsNullOrEmpty(summary))
        {
            return "No changes";
        }

        return summary.TrimEnd();
    }

    private void AppendChangeLine(ref string summary, string label, int change)
    {
        if (change == 0)
        {
            return;
        }

        summary += FormatStatChange(label, change) + "\n";
    }

    private void ContinueAfterFeedback()
    {
        PlayUiSound(continueClip, FinalRoundSoundEvent.Continue);
        currentQuestionIndex++;
        ShowCurrentQuestion();
        FadeInScreen(questionScreen);
    }

    private void ShowStageTransition()
    {
        InterviewStage stage = stages[currentStageIndex];
        currentStageWasStrong = CalculateCurrentStageWasStrong();
        TrackStagePerformance(stage);

        HidePauseOverlay();
        questionScreen.SetActive(false);
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(false);
        stageTransitionScreen.SetActive(true);
        recoveryChoiceScreen.SetActive(false);
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(false);

        progressText.text = $"{stage.StageName} complete";
        subtitleText.text = "Quick reset before the next conversation.";
        UpdateRoomBackdrop(stage.StageName);
        SetRoomBackdropSpeaking(false);
        stageTransitionNameText.text = stage.StageName;
        stageTransitionBodyText.text = BuildStageFeedback(stage);
        stageTransitionStatsText.text =
            $"{stage.StageCompleteText}\n\n" +
            GetStatsSummary() +
            BuildTransitionBonusText();
        PlayUiSound(stageCompleteClip, FinalRoundSoundEvent.StageComplete);
        FadeInScreen(stageTransitionScreen);
    }

    private void TrackStagePerformance(InterviewStage stage)
    {
        if (stageStartStats == null || HasStageSummary(stage.StageName))
        {
            return;
        }

        stageRunSummaries.Add(new StageRunSummary(stage.StageName, playerStats.TotalScore - stageStartStats.TotalScore));
    }

    private bool HasStageSummary(string stageName)
    {
        for (int i = 0; i < stageRunSummaries.Count; i++)
        {
            if (stageRunSummaries[i].StageName == stageName)
            {
                return true;
            }
        }

        return false;
    }

    private bool CalculateCurrentStageWasStrong()
    {
        int startTotal = stageStartStats.TotalScore;
        int currentTotal = playerStats.TotalScore;
        return currentTotal - startTotal >= 15;
    }

    private string BuildStageFeedback(InterviewStage stage)
    {
        string recruiterNote = PickRecruiterStageNote();

        if (currentStageWasStrong)
        {
            return $"{recruiterNote}\n\nStrong signal from {stage.StageName}. The conversation had enough clarity and momentum to make the next round feel earned.";
        }

        return $"{recruiterNote}\n\nMixed signal from {stage.StageName}. There were useful moments, but the next round will need sharper answers and steadier positioning.";
    }

    private string PickRecruiterStageNote()
    {
        string[] notes =
        {
            "Recruiter note: The team wants one more conversation before they compare notes.",
            "Recruiter note: Feedback is moving, but the wording is doing that careful recruiter thing.",
            "Recruiter note: The next round is being framed as a calibration chat, which is never just a calibration chat.",
            "Recruiter note: They liked the direction and want to pressure-test the signal."
        };

        return notes[GetRunRandomIndex(notes.Length)];
    }

    private void ContinueAfterStageTransition()
    {
        PlayUiSound(continueClip, FinalRoundSoundEvent.Continue);
        bool hasNextStage = currentStageIndex < stages.Length - 1;

        currentStageIndex++;
        currentQuestionIndex = 0;

        if (currentStageIndex >= stages.Length)
        {
            ShowOutcome();
            return;
        }

        if (hasNextStage)
        {
            int energyRecoveryModifier = !useCompanyProfileModifiers || activeCompanyProfile == null
                ? 0
                : activeCompanyProfile.BetweenStageEnergyRecoveryModifier;
            playerStats.RecoverBetweenStages(currentStageWasStrong, energyRecoveryModifier);
            ShowRecoveryChoiceScreen();
            return;
        }

        ContinueAfterRecoveryChoice();
    }

    private void ShowRecoveryChoiceScreen()
    {
        HidePauseOverlay();
        questionScreen.SetActive(false);
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
        recoveryChoiceScreen.SetActive(true);
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(false);

        progressText.text = "Between rounds";
        subtitleText.text = "A small choice before the next interview.";
        UpdateRoomBackdrop("Between Rounds");
        recoveryChoiceTitleText.text = "BETWEEN ROUNDS";
        recoveryChoiceBodyText.text = "You have a little time before the next interview. Pick one useful move, or one obviously unhelpful one.";
        recoveryChoiceStatsText.text =
            "<b>Current stats</b>\n" +
            GetStatsSummary();
        SetRecoveryStatsLayout(false);
        recoveryChoiceButtonColumn.SetActive(true);
        recoveryChoiceContinueButton.gameObject.SetActive(false);
        UpdateRecoveryChoiceButtons();
        FadeInScreen(recoveryChoiceScreen);
    }

    private void UpdateRecoveryChoiceButtons()
    {
        if (recoveryChoices == null || recoveryChoiceButtons == null)
        {
            return;
        }

        for (int i = 0; i < recoveryChoiceButtons.Length; i++)
        {
            RecoveryChoice choice = recoveryChoices[i];
            recoveryChoiceButtons[i].interactable = true;
            recoveryChoiceButtonTexts[i].text =
                $"<b>{i + 1}. {choice.Name}</b>\n" +
                choice.Description;
        }
    }

    private void ChooseRecoveryChoice(int choiceIndex)
    {
        if (!CanChooseRecoveryChoice(choiceIndex))
        {
            return;
        }

        RecoveryChoice choice = recoveryChoices[choiceIndex];
        choice.UseCount++;
        recoveryChoicesMadeCount++;
        if (choice.IsBadIdea)
        {
            doomScrollChoiceCount++;
        }

        int confidenceChange = ApplyRecoveryPositiveBonus(choice.ConfidenceChange);
        int energyChange = ApplyRecoveryPositiveBonus(choice.EnergyChange);
        int technicalCredibilityChange = ApplyRecoveryPositiveBonus(choice.TechnicalCredibilityChange);
        int commercialAlignmentChange = ApplyRecoveryPositiveBonus(choice.CommercialAlignmentChange);
        int pressureChange = ApplyPressureChange(GetRecoveryChoicePressureChange(choice.ChoiceType));

        playerStats.ApplyDirectChanges(
            confidenceChange,
            energyChange,
            technicalCredibilityChange,
            commercialAlignmentChange);
        styleTracker.ApplyStyleChanges(
            choice.DiplomaticStyleChange,
            choice.BluntStyleChange,
            choice.CommercialStyleChange,
            choice.TechnicalStyleChange,
            choice.ChaoticStyleChange,
            choice.BurnedOutStyleChange);
        PlayUiSound(recoveryChoiceSelectedClip, FinalRoundSoundEvent.RecoveryChoiceSelected);

        recoveryChoiceBodyText.text = choice.ConfirmationText;
        SetRecoveryStatsLayout(true);
        recoveryChoiceStatsText.text =
            "<b>Recovery choice effects</b>\n" +
            BuildChangeSummary(
                confidenceChange,
                energyChange,
                technicalCredibilityChange,
                commercialAlignmentChange,
                choice.DiplomaticStyleChange,
                choice.BluntStyleChange,
                choice.CommercialStyleChange,
                choice.TechnicalStyleChange,
                choice.ChaoticStyleChange,
                choice.BurnedOutStyleChange,
                pressureChange) +
            "\n\n<b>Current stats</b>\n" +
            GetStatsSummary();
        FlashTransientText(
            recoveryChoiceStatsText,
            GetLargestAbsoluteStatChange(confidenceChange, energyChange, technicalCredibilityChange, commercialAlignmentChange),
            pressureChange);
        recoveryChoiceButtonColumn.SetActive(false);
        recoveryChoiceContinueButton.gameObject.SetActive(true);
        recoveryChoiceContinueButton.interactable = true;
    }

    private int ApplyRecoveryPositiveBonus(int change)
    {
        if (!useCompanyProfileModifiers
            || activeCompanyProfile == null
            || activeCompanyProfile.RecoveryPositiveEffectBonus <= 0
            || change <= 0)
        {
            return change;
        }

        AddRuleRunNoteOnce($"{activeCompanyProfile.RuleName}: positive recovery effects gained +{activeCompanyProfile.RecoveryPositiveEffectBonus}.");
        return change + activeCompanyProfile.RecoveryPositiveEffectBonus;
    }

    private int GetRecoveryChoicePressureChange(RecoveryChoiceType choiceType)
    {
        switch (choiceType)
        {
            case RecoveryChoiceType.ReviewNotes:
                return -3;
            case RecoveryChoiceType.ReframeBusinessCase:
                return -3;
            case RecoveryChoiceType.TakeAWalk:
                return -8;
            case RecoveryChoiceType.MessageFriendlyAe:
                return -4;
            case RecoveryChoiceType.DoomScrollGlassdoor:
                return 12;
            default:
                return 0;
        }
    }

    private void SetRecoveryStatsLayout(bool confirmationState)
    {
        if (recoveryChoiceStatsText == null)
        {
            return;
        }

        float preferredHeight = confirmationState ? 308f : 174f;
        float minimumHeight = confirmationState ? 292f : 160f;
        ConfigurePreferredLayoutElement(recoveryChoiceStatsText.gameObject, -1f, preferredHeight);
        SetMinimumLayoutHeight(recoveryChoiceStatsText.gameObject, minimumHeight);
    }

    private bool CanChooseRecoveryChoice(int choiceIndex)
    {
        return recoveryChoiceScreen.activeSelf
            && recoveryChoiceButtonColumn.activeSelf
            && recoveryChoices != null
            && choiceIndex >= 0
            && choiceIndex < recoveryChoices.Length
            && recoveryChoiceButtons != null
            && choiceIndex < recoveryChoiceButtons.Length
            && recoveryChoiceButtons[choiceIndex].interactable;
    }

    private void ContinueAfterRecoveryChoice()
    {
        PlayUiSound(continueClip, FinalRoundSoundEvent.Continue);
        if (ShouldShowRandomEvent())
        {
            if (ShowRandomEvent())
            {
                return;
            }
        }

        StartNextStage();
    }

    private bool ShouldShowRandomEvent()
    {
        return randomEvents != null
            && randomEvents.Length > 0
            && GetRunRandomValue() < GetAdjustedRandomEventChance();
    }

    private float GetAdjustedRandomEventChance()
    {
        float modifier = !useCompanyProfileModifiers || activeCompanyProfile == null
            ? 0f
            : activeCompanyProfile.RandomEventChanceModifier;
        return Mathf.Clamp01(BetweenStageEventChance + modifier);
    }

    private int GetRunRandomIndex(int exclusiveMax)
    {
        if (exclusiveMax <= 0)
        {
            return 0;
        }

        if (runRandom == null)
        {
            InitializeRunIdentity();
        }

        return runRandom.Next(exclusiveMax);
    }

    private float GetRunRandomValue()
    {
        if (runRandom == null)
        {
            InitializeRunIdentity();
        }

        return (float)runRandom.NextDouble();
    }

    private bool ShowRandomEvent()
    {
        int eventIndex = PickUnusedRandomEventIndex();
        if (eventIndex < 0)
        {
            Debug.Log("No unused random events remaining; skipping between-stage event.");
            return false;
        }

        if (!debugAllowRepeatedRandomEvents)
        {
            usedRandomEventIndexes.Add(eventIndex);
        }

        RandomInterviewEvent interviewEvent = BuildCompanyAdjustedEvent(randomEvents[eventIndex]);
        int pressureChange = ApplyRandomEvent(interviewEvent);

        HidePauseOverlay();
        questionScreen.SetActive(false);
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
        recoveryChoiceScreen.SetActive(false);
        randomEventScreen.SetActive(true);
        outcomeScreen.SetActive(false);

        progressText.text = "Between rounds";
        subtitleText.text = "The process shifts slightly before the next conversation.";
        UpdateRoomBackdrop("Between Rounds");
        randomEventTitleText.text = interviewEvent.EventTitle;
        randomEventBodyText.text =
            $"{PickBetweenStageMessage()}\n\n" +
            interviewEvent.EventDescription;
        randomEventChangesText.text =
            "Event changes:\n" +
            BuildChangeSummary(
                interviewEvent.ConfidenceChange,
                interviewEvent.EnergyChange,
                interviewEvent.TechnicalCredibilityChange,
                interviewEvent.CommercialAlignmentChange,
                interviewEvent.DiplomaticStyleChange,
                interviewEvent.BluntStyleChange,
                interviewEvent.CommercialStyleChange,
                interviewEvent.TechnicalStyleChange,
                interviewEvent.ChaoticStyleChange,
                interviewEvent.BurnedOutStyleChange,
                pressureChange) +
            "\n\n" +
            GetStatsSummary();
        FlashTransientText(
            randomEventChangesText,
            GetLargestAbsoluteStatChange(
                interviewEvent.ConfidenceChange,
                interviewEvent.EnergyChange,
                interviewEvent.TechnicalCredibilityChange,
                interviewEvent.CommercialAlignmentChange),
            pressureChange);
        PlayUiSound(randomEventClip, FinalRoundSoundEvent.RandomEventAppears);
        FadeInScreen(randomEventScreen);
        return true;
    }

    private int PickUnusedRandomEventIndex()
    {
        if (randomEvents == null || randomEvents.Length == 0)
        {
            return -1;
        }

        if (debugAllowRepeatedRandomEvents)
        {
            return GetRunRandomIndex(randomEvents.Length);
        }

        int unusedCount = randomEvents.Length - usedRandomEventIndexes.Count;
        if (unusedCount <= 0)
        {
            return -1;
        }

        int targetUnusedIndex = GetRunRandomIndex(unusedCount);
        for (int i = 0; i < randomEvents.Length; i++)
        {
            if (usedRandomEventIndexes.Contains(i))
            {
                continue;
            }

            if (targetUnusedIndex == 0)
            {
                return i;
            }

            targetUnusedIndex--;
        }

        return -1;
    }

    private int ApplyRandomEvent(RandomInterviewEvent interviewEvent)
    {
        int totalScoreBefore = playerStats.TotalScore;
        playerStats.Apply(interviewEvent);
        styleTracker.Apply(interviewEvent);
        int pressureChange = ApplyPressureChange(CalculateRandomEventPressureChange(interviewEvent));
        randomEventRunSummaries.Add(new RandomEventRunSummary(interviewEvent.EventTitle, playerStats.TotalScore - totalScoreBefore));
        return pressureChange;
    }

    private RandomInterviewEvent BuildCompanyAdjustedEvent(RandomInterviewEvent interviewEvent)
    {
        if (!useCompanyProfileModifiers || activeCompanyProfile == null)
        {
            return interviewEvent;
        }

        int energyChange = interviewEvent.EnergyChange;
        if (energyChange < 0)
        {
            energyChange += activeCompanyProfile.EventEnergyLossModifier;
            if (activeCompanyProfile.EventEnergyLossModifier != 0)
            {
                AddRuleRunNoteOnce($"{activeCompanyProfile.RuleName}: random event energy losses modified by {activeCompanyProfile.EventEnergyLossModifier}.");
            }
        }

        int chaoticStyleChange = interviewEvent.ChaoticStyleChange;
        if (activeCompanyProfile.AiChaoticStyleBonus > 0 && IsAiOrChaoticSignal(interviewEvent.EventTitle + " " + interviewEvent.EventDescription))
        {
            chaoticStyleChange += activeCompanyProfile.AiChaoticStyleBonus;
            AddRuleRunNoteOnce($"{activeCompanyProfile.RuleName}: AI/chaos event added chaotic style +{activeCompanyProfile.AiChaoticStyleBonus}.");
        }

        return new RandomInterviewEvent(
            interviewEvent.EventTitle,
            interviewEvent.EventDescription,
            interviewEvent.ConfidenceChange,
            energyChange,
            interviewEvent.TechnicalCredibilityChange,
            interviewEvent.CommercialAlignmentChange,
            interviewEvent.DiplomaticStyleChange,
            interviewEvent.BluntStyleChange,
            interviewEvent.CommercialStyleChange,
            interviewEvent.TechnicalStyleChange,
            chaoticStyleChange,
            interviewEvent.BurnedOutStyleChange);
    }

    private void ContinueAfterRandomEvent()
    {
        PlayUiSound(continueClip, FinalRoundSoundEvent.Continue);
        StartNextStage();
    }

    private void StartNextStage()
    {
        CaptureStageStartStats();
        ShowQuestionScreen();
    }

    private string BuildTransitionBonusText()
    {
        if (currentStageIndex >= stages.Length - 1)
        {
            return "\n\n<b>Next step</b>\nContinue to final decision.";
        }

        int energyRecoveryModifier = !useCompanyProfileModifiers || activeCompanyProfile == null
            ? 0
            : activeCompanyProfile.BetweenStageEnergyRecoveryModifier;

        return "\n\n<b>Continue bonus</b>\n" +
            $"{FormatStatChange("Energy", 10 + energyRecoveryModifier)}\n" +
            $"{FormatStatChange("Confidence", currentStageWasStrong ? 5 : 0)}";
    }

    private void UpdateStatsText()
    {
        statsText.text = GetStatsSummary();
    }

    private string GetStatsSummary()
    {
        return
            $"Confidence <b>{playerStats.Confidence}/100</b> | Energy <b>{playerStats.Energy}/100</b>\n" +
            $"Technical <b>{playerStats.TechnicalCredibility}/100</b> | Commercial <b>{playerStats.CommercialAlignment}/100</b>\n\n" +
            BuildPressureSummary();
    }

    private string BuildPressureSummary()
    {
        return
            $"INTERVIEW PRESSURE: {BuildPressureBar()} <b>{interviewPressure}/100</b> | {GetPressureStateLabel()}" +
            BuildPressureWarningLine();
    }

    private string BuildPressureBar()
    {
        const int segmentCount = 8;
        int filledSegments = Mathf.Clamp(Mathf.CeilToInt(interviewPressure / 12.5f), 0, segmentCount);
        string bar = "[";

        for (int i = 0; i < segmentCount; i++)
        {
            bar += i < filledSegments ? "#" : "-";
        }

        return bar + "]";
    }

    private string BuildPressureWarningLine()
    {
        if (interviewPressure < 80)
        {
            return string.Empty;
        }

        return "\n<color=#FF8B8B>The room feels unstable. Another bad answer could spiral.</color>";
    }

    private string GetPressureStateLabel()
    {
        if (interviewPressure <= 24)
        {
            return "Calm";
        }

        if (interviewPressure <= 49)
        {
            return "Focused";
        }

        if (interviewPressure <= 74)
        {
            return "Tense";
        }

        return "Spiralling";
    }

    private int ApplyPressureChange(int pressureChange)
    {
        int pressureBefore = interviewPressure;
        interviewPressure = Mathf.Clamp(interviewPressure + pressureChange, 0, 100);
        int actualChange = interviewPressure - pressureBefore;

        if (actualChange != 0)
        {
            UpdateRoomBackdrop(GetCurrentBackdropStageName());
        }

        if (!highPressureWarningPlayed && pressureBefore < 75 && interviewPressure >= 75)
        {
            highPressureWarningPlayed = true;
            PlaySound(FinalRoundSoundEvent.PressureWarning);
        }
        else if (interviewPressure < 65)
        {
            highPressureWarningPlayed = false;
        }

        return actualChange;
    }

    private int CalculateAnswerPressureChange(AnswerOption answer, int confidenceModifier, int commercialAlignmentModifier)
    {
        int totalDelta = answer.ConfidenceChange
            + confidenceModifier
            + answer.EnergyChange
            + answer.TechnicalCredibilityChange
            + answer.CommercialAlignmentChange
            + commercialAlignmentModifier;

        if (totalDelta >= 18)
        {
            return -6;
        }

        if (totalDelta >= 10)
        {
            return -4;
        }

        if (totalDelta >= 0)
        {
            return 3;
        }

        if (totalDelta >= -9)
        {
            return 7;
        }

        return 11;
    }

    private int CalculateRandomEventPressureChange(RandomInterviewEvent interviewEvent)
    {
        int totalDelta = interviewEvent.ConfidenceChange
            + interviewEvent.EnergyChange
            + interviewEvent.TechnicalCredibilityChange
            + interviewEvent.CommercialAlignmentChange;

        if (totalDelta >= 8)
        {
            return -3;
        }

        if (totalDelta >= 0)
        {
            return 3;
        }

        if (totalDelta >= -9)
        {
            return 6;
        }

        return 10;
    }

    private void ShowOutcome()
    {
        HidePauseOverlay();
        SetRuntimeBackgroundVisible(true);
        questionScreen.SetActive(false);
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
        recoveryChoiceScreen.SetActive(false);
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(true);
        progressText.text = "Interview process complete";
        subtitleText.text = "Final hiring feedback across every round.";
        UpdateRoomBackdrop("Final Outcome");

        int totalScore = playerStats.TotalScore;
        int offerRecommendedThreshold = GetOfferRecommendedThreshold();
        bool hasCriticalWeakness = playerStats.HasCriticalWeakness;
        InterviewStyleResult styleResult = styleTracker.DetermineDominantStyle(playerStats);

        string outcomeName;

        if (hasCriticalWeakness)
        {
            outcomeName = "Process Ended";
            outcomeTitleText.text = "Outcome: Process Ended";
            outcomeBodyText.text = BuildOutcomeBody(
                "Across the process, one core signal dropped below the hiring bar. The team saw potential, but the risk felt too high to progress to offer.",
                "One weak signal became the thing everyone kept circling back to.",
                styleResult);
        }
        else if (totalScore >= offerRecommendedThreshold && CanRecommendOffer())
        {
            outcomeName = "Offer Recommended";
            outcomeTitleText.text = "Outcome: Offer Recommended";
            outcomeBodyText.text = BuildOutcomeBody(
                "You carried a strong signal from recruiter screen through VP round. The feedback points to a credible, commercially sharp hire.",
                "This is the version of you the panel can easily defend in debrief.",
                styleResult);
        }
        else if (totalScore >= 285)
        {
            outcomeName = "Final Debrief Pass";
            outcomeTitleText.text = "Outcome: Final Debrief Pass";
            outcomeBodyText.text = BuildOutcomeBody(
                "The process landed well overall. There are a few calibration notes, but the team has enough confidence to keep the offer conversation alive.",
                "Not flawless, but the room has enough to keep moving.",
                styleResult);
        }
        else if (totalScore >= 230)
        {
            outcomeName = "Hold";
            outcomeTitleText.text = "Outcome: Hold";
            outcomeBodyText.text = BuildOutcomeBody(
                "The rounds produced mixed feedback. Some interviewers saw the fit, while others wanted stronger evidence before making the final call.",
                "The debrief has enough positives to argue about, which is both good and exhausting.",
                styleResult);
        }
        else
        {
            outcomeName = "Rejected";
            outcomeTitleText.text = "Outcome: Rejected";
            outcomeBodyText.text = BuildOutcomeBody(
                "The process never quite built enough momentum. The team decided the role needs a clearer blend of technical depth and commercial judgment.",
                "The process ended before your strongest version really arrived.",
                styleResult);
        }

        List<RunBadge> earnedBadges = BuildEarnedBadges(outcomeName, styleResult);

        outcomeStatsText.text = BuildFinalReadSummary(outcomeName, styleResult);
        outcomeHighlightsText.text = BuildRunHighlightsSummary();
        outcomeBadgesText.text = BuildBadgesDisplayText(earnedBadges);
        outcomeAdviceText.text = BuildRunAdvice();

        PlayOutcomeSound(outcomeName);
        FadeInScreen(outcomeScreen);
        RevealFinalOutcomeSections();
        LogRunSummary(outcomeName, styleResult, earnedBadges);
    }

    private void PlayOutcomeSound(string outcomeName)
    {
        bool positiveOutcome = IsPassOrBetterOutcome(outcomeName);
        AudioClip clip = positiveOutcome ? finalPositiveOutcomeClip : finalNegativeOutcomeClip;
        if (clip == null)
        {
            clip = finalOutcomeClip;
        }

        PlayUiSound(
            clip,
            positiveOutcome
                ? FinalRoundSoundEvent.FinalPositiveOutcome
                : FinalRoundSoundEvent.FinalNegativeOutcome);
    }

    private int GetOfferRecommendedThreshold()
    {
        int modifier = !useCompanyProfileModifiers || activeCompanyProfile == null
            ? 0
            : activeCompanyProfile.OfferRecommendedThresholdModifier;

        if (modifier != 0)
        {
            AddRuleRunNoteOnce($"{activeCompanyProfile.RuleName}: offer recommendation threshold adjusted by {FormatSignedNumber(modifier)}.");
        }

        return 330 + modifier;
    }

    private bool CanRecommendOffer()
    {
        if (!useCompanyProfileModifiers
            || activeCompanyProfile == null
            || !activeCompanyProfile.RequiresTechnicalAndCommercialOfferGate)
        {
            return true;
        }

        bool gatePassed = playerStats.TechnicalCredibility >= 60 && playerStats.CommercialAlignment >= 60;
        AddRuleRunNoteOnce(gatePassed
            ? $"{activeCompanyProfile.RuleName}: technical and commercial offer gate passed."
            : $"{activeCompanyProfile.RuleName}: offer gate blocked by technical/commercial minimums.");
        return gatePassed;
    }

    private string BuildOutcomeBody(string mainFeedback, string finalComment, InterviewStyleResult styleResult)
    {
        return
            $"{mainFeedback}\n\n" +
            $"{BuildCompanyOutcomeLine()}\n\n" +
            $"{BuildRuleOutcomeLine()}\n\n" +
            $"{BuildPressureOutcomeLine()}\n\n" +
            $"Final note: {finalComment}\n\n" +
            $"Dominant style: {styleResult.StyleName}.";
    }

    private string BuildCompanyOutcomeLine()
    {
        if (activeCompanyProfile == null)
        {
            return "Company process: The panel weighed the run against a fairly standard interview process.";
        }

        return $"Against a {activeCompanyProfile.CompanyName} process, the panel weighted the run through {activeCompanyProfile.ProfileName.ToLowerInvariant()}: {activeCompanyProfile.PreferredStyle}";
    }

    private string BuildPressureOutcomeLine()
    {
        if (interviewPressure >= 85)
        {
            return "Pressure read: The room felt unstable by the end, which added credibility risk to the debrief.";
        }

        if (interviewPressure < 25)
        {
            return "Pressure read: You kept the room calm enough that the panel could focus on the signal.";
        }

        return $"Pressure read: Final pressure landed at {interviewPressure}/100 ({GetPressureStateLabel()}).";
    }

    private string BuildRuleOutcomeLine()
    {
        if (activeCompanyProfile == null)
        {
            return "Active rule: Standard process.";
        }

        return $"Active rule: {activeCompanyProfile.RuleName}. {activeCompanyProfile.RuleHint}{BuildSpecialRuleOutcomeLine()}";
    }

    private string BuildSpecialRuleOutcomeLine()
    {
        if (activeCompanyProfile == null)
        {
            return string.Empty;
        }

        switch (activeCompanyProfile.CompanyName)
        {
            case "Security Vendor":
                if (playerStats.TechnicalCredibility >= 75 && playerStats.CommercialAlignment >= 75)
                {
                    AddRuleRunNoteOnce("Risk Framing Matters: strong technical and commercial risk framing earned a positive debrief line.");
                    return " The panel specifically liked that technical depth and business risk stayed connected.";
                }
                break;
            case "AI Hype Company":
                if (styleTracker.ChaoticStyle > activeCompanyProfile.ChaoticStyleForgiveness + 3)
                {
                    AddRuleRunNoteOnce("Chaos Can Sell: excessive chaotic style created credibility risk.");
                    return " The debrief flagged that the chaos started to look like credibility risk.";
                }
                break;
            case "Legacy Enterprise":
                if (playerStats.Energy >= 75)
                {
                    AddRuleRunNoteOnce("Slow Process: high final energy earned a stamina note.");
                    return " Keeping that much energy through a slow process read as real stamina.";
                }
                break;
        }

        return string.Empty;
    }

    private string BuildFinalReadSummary(string outcomeName, InterviewStyleResult styleResult)
    {
        StatSummary strongestStat = GetStrongestStat();
        StatSummary weakestStat = GetWeakestStat();

        return
            $"Process ID: <b>{currentProcessId}</b>\n" +
            $"Company: <b>{GetCompanyNameForSummary()}</b>\n" +
            $"Rule: <b>{(activeCompanyProfile == null ? "Standard Process" : activeCompanyProfile.RuleName)}</b>\n" +
            $"Outcome: <b>{outcomeName}</b>\n" +
            $"Style: <b>{styleResult.StyleName}</b>\n" +
            $"Total Score: <b>{playerStats.TotalScore}/400</b>\n\n" +
            $"Final Pressure: <b>{interviewPressure}/100</b> | {GetPressureStateLabel()}\n\n" +
            $"Questions Faced: <b>{GetSelectedQuestionCount()}</b>\n\n" +
            BuildCompactStatsSummary() + "\n\n" +
            $"Strongest Stat: <b>{strongestStat.Name}</b> ({strongestStat.Value}/100)\n" +
            $"Weakest Stat: <b>{weakestStat.Name}</b> ({weakestStat.Value}/100)";
    }

    private int GetSelectedQuestionCount()
    {
        if (selectedStageQuestions == null)
        {
            return 0;
        }

        int count = 0;
        for (int i = 0; i < selectedStageQuestions.Length; i++)
        {
            if (selectedStageQuestions[i] != null)
            {
                count += selectedStageQuestions[i].Length;
            }
        }

        return count;
    }

    private string BuildRunHighlightsSummary()
    {
        StatSummary strongestStat = GetStrongestStat();
        StatSummary weakestStat = GetWeakestStat();
        StageRunSummary strongestStage = GetStrongestStage();
        StageRunSummary weakestStage = GetWeakestStage();
        RandomEventRunSummary mostHelpfulEvent = GetMostHelpfulEvent();
        RandomEventRunSummary mostDamagingEvent = GetMostDamagingEvent();

        return
            $"Best stat: <b>{strongestStat.Name}</b> ({strongestStat.Value}/100)\n" +
            $"Watch stat: <b>{weakestStat.Name}</b> ({weakestStat.Value}/100)\n\n" +
            $"Best stage: <b>{FormatStageSummary(strongestStage)}</b>\n" +
            $"Hardest stage: <b>{FormatStageSummary(weakestStage)}</b>\n\n" +
            $"Helpful event: {FormatEventSummary(mostHelpfulEvent, true)}\n" +
            $"Damaging event: {FormatEventSummary(mostDamagingEvent, false)}\n\n" +
            $"Answer mix: <b>{strongAnswerCount}</b> strong | <b>{riskyAnswerCount}</b> risky\n" +
            $"Prep cards used: <b>{prepCardsUsedCount}</b>{GetMostUsedPrepCardSummary()}\n" +
            $"Recovery: {BuildRecoveryChoiceSummary()}";
    }

    private List<RunBadge> BuildEarnedBadges(string outcomeName, InterviewStyleResult styleResult)
    {
        List<RunBadge> selectedBadges = new List<RunBadge>();
        List<RunBadge> riskBadges = new List<RunBadge>();
        List<RunBadge> performanceBadges = new List<RunBadge>();
        List<RunBadge> fillerBadges = new List<RunBadge>();

        bool passOrBetter = IsPassOrBetterOutcome(outcomeName);
        if (outcomeName == "Offer Recommended")
        {
            AddBadgeIfUnique(
                selectedBadges,
                CreateRunBadge(
                    "Strong Signal",
                    "Offer recommendation earned from the full process.",
                    RunBadgeType.Positive,
                    new Color32(92, 229, 194, 255)));
        }

        RunBadge dominantStyleBadge = CreateDominantStyleBadge(styleResult);
        AddBadgeIfUnique(selectedBadges, dominantStyleBadge);

        if ((styleResult.StyleName == "Burnout Goblin" || styleTracker.BurnedOutStyle >= 4)
            && dominantStyleBadge?.BadgeName != "Burnout Goblin")
        {
            riskBadges.Add(CreateRunBadge(
                "Burnout Goblin",
                "The panel saw talent, stamina debt, and a few tired jokes.",
                RunBadgeType.Warning,
                new Color32(245, 167, 94, 255)));
        }

        if (styleTracker.ChaoticStyle >= 5 && passOrBetter)
        {
            riskBadges.Add(CreateRunBadge(
                "Chaos Merchant",
                "Enough sparkle to worry them, enough signal to pass.",
                RunBadgeType.Funny,
                new Color32(221, 126, 255, 255)));
        }

        if (doomScrollChoiceCount > 0)
        {
            riskBadges.Add(CreateRunBadge(
                "Glassdoor Casualty",
                "Doom-scrolled the process and somehow kept going.",
                RunBadgeType.Funny,
                new Color32(255, 181, 99, 255)));
        }

        AddBadgeIfUnique(selectedBadges, GetFirstUniqueBadge(riskBadges, selectedBadges));

        if (playerStats.CommercialAlignment >= 75 && interviewPressure <= 65)
        {
            performanceBadges.Add(CreateRunBadge(
                "AE Whisperer",
                "High commercial signal without letting pressure take over.",
                RunBadgeType.Performance,
                new Color32(91, 195, 255, 255)));
        }

        if (interviewPressure >= 75 && passOrBetter)
        {
            performanceBadges.Add(CreateRunBadge(
                "Held It Together",
                "Pressure was high, but the final read still held.",
                RunBadgeType.Performance,
                new Color32(255, 216, 117, 255)));
        }

        if (GetMostDamagingEvent() == null)
        {
            performanceBadges.Add(CreateRunBadge(
                "No Drama Run",
                "No random event materially damaged the process.",
                RunBadgeType.Positive,
                new Color32(132, 214, 172, 255)));
        }

        if (playerStats.TotalScore >= 320 && GetWeakestStat().Name == "Energy")
        {
            performanceBadges.Add(CreateRunBadge(
                "Overqualified, Under-Rested",
                "Strong total signal, with energy as the watch item.",
                RunBadgeType.Warning,
                new Color32(240, 196, 124, 255)));
        }

        if (playerStats.TechnicalCredibility >= 75
            && playerStats.CommercialAlignment <= playerStats.TechnicalCredibility - 20)
        {
            performanceBadges.Add(CreateRunBadge(
                "Technically Correct",
                "Deep technical proof needed a stronger buyer bridge.",
                RunBadgeType.Performance,
                new Color32(120, 182, 255, 255)));
        }

        if (playerStats.IsBalanced() && styleTracker.ChaoticStyle <= 2)
        {
            performanceBadges.Add(CreateRunBadge(
                "Safe Pair of Hands",
                "Balanced signals and very little process turbulence.",
                RunBadgeType.Style,
                new Color32(175, 203, 195, 255)));
        }

        StageRunSummary strongestStage = GetStrongestStage();
        if ((strongestStage != null && strongestStage.StageName == "VP Round")
            || playerStats.Confidence >= 80)
        {
            performanceBadges.Add(CreateRunBadge(
                "Final Panel Energy",
                "Finished with senior-room confidence.",
                RunBadgeType.Performance,
                new Color32(169, 156, 255, 255)));
        }

        AddBadgeIfUnique(selectedBadges, GetFirstUniqueBadge(performanceBadges, selectedBadges));

        fillerBadges.AddRange(riskBadges);
        fillerBadges.AddRange(performanceBadges);
        for (int i = 0; i < fillerBadges.Count && selectedBadges.Count < MaxDisplayedRunBadges; i++)
        {
            AddBadgeIfUnique(selectedBadges, fillerBadges[i]);
        }

        return selectedBadges;
    }

    private RunBadge CreateDominantStyleBadge(InterviewStyleResult styleResult)
    {
        switch (styleResult.StyleName)
        {
            case "Boardroom Translator":
                return CreateRunBadge(
                    "Boardroom Translator",
                    "Dominant style translated technical detail into business decisions.",
                    RunBadgeType.Style,
                    new Color32(83, 215, 190, 255));
            case "AE Whisperer":
                return CreateRunBadge(
                    "AE Whisperer",
                    "Dominant style kept the deal story calm and useful.",
                    RunBadgeType.Style,
                    new Color32(91, 195, 255, 255));
            case "Burnout Goblin":
                return CreateRunBadge(
                    "Burnout Goblin",
                    "Dominant style showed signal through obvious stamina debt.",
                    RunBadgeType.Warning,
                    new Color32(245, 167, 94, 255));
            case "Technical Purist":
                return CreateRunBadge(
                    "Technically Correct",
                    "Dominant style led with technical precision.",
                    RunBadgeType.Style,
                    new Color32(120, 182, 255, 255));
            case "Safe Pair of Hands":
                return CreateRunBadge(
                    "Safe Pair of Hands",
                    "Dominant style was steady, balanced, and low-drama.",
                    RunBadgeType.Style,
                    new Color32(175, 203, 195, 255));
        }

        return null;
    }

    private RunBadge CreateRunBadge(string badgeName, string shortDescription, RunBadgeType badgeType, Color accentColor)
    {
        return new RunBadge(badgeName, shortDescription, badgeType, accentColor);
    }

    private bool IsPassOrBetterOutcome(string outcomeName)
    {
        return outcomeName == "Offer Recommended" || outcomeName == "Final Debrief Pass";
    }

    private RunBadge GetFirstUniqueBadge(List<RunBadge> candidateBadges, List<RunBadge> selectedBadges)
    {
        for (int i = 0; i < candidateBadges.Count; i++)
        {
            if (!HasBadge(selectedBadges, candidateBadges[i].BadgeName))
            {
                return candidateBadges[i];
            }
        }

        return null;
    }

    private void AddBadgeIfUnique(List<RunBadge> badges, RunBadge badge)
    {
        if (badge == null || badges.Count >= MaxDisplayedRunBadges || HasBadge(badges, badge.BadgeName))
        {
            return;
        }

        badges.Add(badge);
    }

    private bool HasBadge(List<RunBadge> badges, string badgeName)
    {
        for (int i = 0; i < badges.Count; i++)
        {
            if (badges[i].BadgeName == badgeName)
            {
                return true;
            }
        }

        return false;
    }

    private string BuildBadgesDisplayText(List<RunBadge> badges)
    {
        if (badges == null || badges.Count == 0)
        {
            return "No badge pattern emerged this run.";
        }

        string summary = string.Empty;
        for (int i = 0; i < badges.Count; i++)
        {
            RunBadge badge = badges[i];
            summary += $"<color=#{ColorUtility.ToHtmlStringRGB(badge.AccentColor)}><b>{badge.BadgeName}</b></color> - {badge.ShortDescription}";
            if (i < badges.Count - 1)
            {
                summary += "\n";
            }
        }

        return summary;
    }

    private string GetMostUsedPrepCardSummary()
    {
        if (prepCards == null || prepCardsUsedCount <= 0)
        {
            return string.Empty;
        }

        PrepCard mostUsedCard = null;
        for (int i = 0; i < prepCards.Length; i++)
        {
            if (prepCards[i].UseCount <= 0)
            {
                continue;
            }

            if (mostUsedCard == null || prepCards[i].UseCount > mostUsedCard.UseCount)
            {
                mostUsedCard = prepCards[i];
            }
        }

        return mostUsedCard == null ? string.Empty : $" | Most used: <b>{mostUsedCard.Name}</b>";
    }

    private string BuildRecoveryChoiceSummary()
    {
        if (recoveryChoices == null || recoveryChoicesMadeCount <= 0)
        {
            return "none";
        }

        RecoveryChoice mostUsedChoice = GetMostUsedRecoveryChoice();
        string summary = mostUsedChoice == null
            ? $"{recoveryChoicesMadeCount} choices"
            : $"Most used: <b>{mostUsedChoice.Name}</b>";

        if (doomScrollChoiceCount > 0)
        {
            summary += $" | Bad idea count: <b>{doomScrollChoiceCount}</b>";
        }

        return summary;
    }

    private RecoveryChoice GetMostUsedRecoveryChoice()
    {
        RecoveryChoice mostUsedChoice = null;

        for (int i = 0; i < recoveryChoices.Length; i++)
        {
            if (recoveryChoices[i].UseCount <= 0)
            {
                continue;
            }

            if (mostUsedChoice == null || recoveryChoices[i].UseCount > mostUsedChoice.UseCount)
            {
                mostUsedChoice = recoveryChoices[i];
            }
        }

        return mostUsedChoice;
    }

    private string BuildCompactStatsSummary()
    {
        return
            $"Confidence {playerStats.Confidence}/100 | Energy {playerStats.Energy}/100\n" +
            $"Technical {playerStats.TechnicalCredibility}/100 | Commercial {playerStats.CommercialAlignment}/100";
    }

    private string GetCompanyNameForSummary()
    {
        return activeCompanyProfile == null ? "Standard Process" : activeCompanyProfile.CompanyName;
    }

    private StatSummary GetStrongestStat()
    {
        StatSummary strongest = new StatSummary("Confidence", playerStats.Confidence);
        strongest = PickHigherStat(strongest, new StatSummary("Energy", playerStats.Energy));
        strongest = PickHigherStat(strongest, new StatSummary("Technical Credibility", playerStats.TechnicalCredibility));
        strongest = PickHigherStat(strongest, new StatSummary("Commercial Alignment", playerStats.CommercialAlignment));
        return strongest;
    }

    private StatSummary GetWeakestStat()
    {
        StatSummary weakest = new StatSummary("Confidence", playerStats.Confidence);
        weakest = PickLowerStat(weakest, new StatSummary("Energy", playerStats.Energy));
        weakest = PickLowerStat(weakest, new StatSummary("Technical Credibility", playerStats.TechnicalCredibility));
        weakest = PickLowerStat(weakest, new StatSummary("Commercial Alignment", playerStats.CommercialAlignment));
        return weakest;
    }

    private StatSummary PickHigherStat(StatSummary current, StatSummary candidate)
    {
        return candidate.Value > current.Value ? candidate : current;
    }

    private StatSummary PickLowerStat(StatSummary current, StatSummary candidate)
    {
        return candidate.Value < current.Value ? candidate : current;
    }

    private StageRunSummary GetStrongestStage()
    {
        if (stageRunSummaries.Count == 0)
        {
            return null;
        }

        StageRunSummary strongest = stageRunSummaries[0];
        for (int i = 1; i < stageRunSummaries.Count; i++)
        {
            if (stageRunSummaries[i].NetScoreChange > strongest.NetScoreChange)
            {
                strongest = stageRunSummaries[i];
            }
        }

        return strongest;
    }

    private StageRunSummary GetWeakestStage()
    {
        if (stageRunSummaries.Count == 0)
        {
            return null;
        }

        StageRunSummary weakest = stageRunSummaries[0];
        for (int i = 1; i < stageRunSummaries.Count; i++)
        {
            if (stageRunSummaries[i].NetScoreChange < weakest.NetScoreChange)
            {
                weakest = stageRunSummaries[i];
            }
        }

        return weakest;
    }

    private string FormatStageSummary(StageRunSummary stageSummary)
    {
        if (stageSummary == null)
        {
            return "No stage data";
        }

        return $"{stageSummary.StageName} ({FormatSignedNumber(stageSummary.NetScoreChange)} net)";
    }

    private RandomEventRunSummary GetMostHelpfulEvent()
    {
        RandomEventRunSummary bestEvent = null;

        for (int i = 0; i < randomEventRunSummaries.Count; i++)
        {
            RandomEventRunSummary eventSummary = randomEventRunSummaries[i];
            if (eventSummary.NetScoreImpact <= 0)
            {
                continue;
            }

            if (bestEvent == null || eventSummary.NetScoreImpact > bestEvent.NetScoreImpact)
            {
                bestEvent = eventSummary;
            }
        }

        return bestEvent;
    }

    private RandomEventRunSummary GetMostDamagingEvent()
    {
        RandomEventRunSummary worstEvent = null;

        for (int i = 0; i < randomEventRunSummaries.Count; i++)
        {
            RandomEventRunSummary eventSummary = randomEventRunSummaries[i];
            if (eventSummary.NetScoreImpact >= 0)
            {
                continue;
            }

            if (worstEvent == null || eventSummary.NetScoreImpact < worstEvent.NetScoreImpact)
            {
                worstEvent = eventSummary;
            }
        }

        return worstEvent;
    }

    private string FormatEventSummary(RandomEventRunSummary eventSummary, bool helpful)
    {
        if (randomEventRunSummaries.Count == 0)
        {
            return "No major external drama this time.";
        }

        if (eventSummary == null)
        {
            return helpful ? "No event materially helped." : "No event materially damaged the run.";
        }

        return $"{eventSummary.EventTitle} ({FormatSignedNumber(eventSummary.NetScoreImpact)} net)";
    }

    private string FormatSignedNumber(int value)
    {
        return value > 0 ? $"+{value}" : value.ToString();
    }

    private void AddRuleRunNoteOnce(string note)
    {
        if (string.IsNullOrEmpty(note) || activeRuleRunNotes.Contains(note))
        {
            return;
        }

        activeRuleRunNotes.Add(note);
    }

    private string BuildRuleRunNotesForLog()
    {
        if (activeRuleRunNotes.Count == 0)
        {
            return "Rule effects applied: none beyond baseline modifiers.";
        }

        string summary = "Rule effects applied:";
        for (int i = 0; i < activeRuleRunNotes.Count; i++)
        {
            summary += $"\n- {activeRuleRunNotes[i]}";
        }

        return summary;
    }

    private string BuildRunAdvice()
    {
        StatSummary weakestStat = GetWeakestStat();

        int chaoticWarningThreshold = activeCompanyProfile == null
            ? 5
            : 5 + activeCompanyProfile.ChaoticStyleForgiveness;

        if (styleTracker.ChaoticStyle >= chaoticWarningThreshold || riskyAnswerCount >= 7)
        {
            return "Funny answers can land, but too many make the panel nervous.";
        }

        if (weakestStat.Name == "Commercial Alignment")
        {
            return "Tie more answers back to business outcomes and deal progress.";
        }

        if (weakestStat.Name == "Technical Credibility")
        {
            return "Give clearer technical examples and defend your reasoning.";
        }

        if (weakestStat.Name == "Energy")
        {
            return "Protect stamina; not every answer needs to be a war story.";
        }

        if (weakestStat.Name == "Confidence")
        {
            return "Be more decisive and avoid sounding like the process happened to you.";
        }

        return "Keep balancing technical proof with commercial clarity.";
    }

    private string PickBetweenStageMessage()
    {
        string[] messages =
        {
            "Recruiter message: Quick update before the next round.",
            "Recruiter message: Small process wrinkle, nothing to panic about.",
            "Recruiter message: The schedule shifted, but the conversation is still warm.",
            "Recruiter message: Sharing context so you are not reading tea leaves in your inbox."
        };

        return messages[GetRunRandomIndex(messages.Length)];
    }

    private void UpdateRoomBackdrop(string stageName)
    {
        UpdateCallPanel(stageName);
    }

    private void SetRoomBackdropSpeaking(bool interviewerIsSpeaking)
    {
        callInterviewerSpeaking = interviewerIsSpeaking;
        UpdateCallPanel(activeCallStageName);

        if (roomBackdrop != null)
        {
            roomBackdrop.SetInterviewerSpeaking(interviewerIsSpeaking);
        }
    }

    private string GetCurrentBackdropStageName()
    {
        if (outcomeScreen != null && outcomeScreen.activeSelf)
        {
            return "Final Outcome";
        }

        if ((recoveryChoiceScreen != null && recoveryChoiceScreen.activeSelf)
            || (randomEventScreen != null && randomEventScreen.activeSelf))
        {
            return "Between Rounds";
        }

        if (questionScreen != null && questionScreen.activeSelf
            && stages != null
            && currentStageIndex >= 0
            && currentStageIndex < stages.Length)
        {
            return stages[currentStageIndex].StageName;
        }

        return "Main Menu";
    }

    private void LogRunSummary(string outcomeName, InterviewStyleResult styleResult, List<RunBadge> earnedBadges)
    {
        Debug.Log(
            "Final Round Run Summary\n" +
            $"Process ID: {currentProcessId}\n" +
            $"Run Seed: {currentRunSeed}\n" +
            $"Company Profile: {GetCompanyNameForSummary()}\n" +
            $"Rule: {(activeCompanyProfile == null ? "none" : activeCompanyProfile.RuleName)}\n" +
            $"Outcome: {outcomeName}\n" +
            $"Total Score: {playerStats.TotalScore}/400\n" +
            $"Final Pressure: {interviewPressure}/100 ({GetPressureStateLabel()})\n" +
            playerStats.GetSummary() + "\n" +
            styleTracker.GetDebugSummary() + "\n" +
            $"Dominant Style: {styleResult.StyleName}\n" +
            $"Badges Earned: {BuildBadgeLogSummary(earnedBadges)}\n" +
            BuildRuleRunNotesForLog());
    }

    private string BuildBadgeLogSummary(List<RunBadge> earnedBadges)
    {
        if (earnedBadges == null || earnedBadges.Count == 0)
        {
            return "none";
        }

        string summary = string.Empty;
        for (int i = 0; i < earnedBadges.Count; i++)
        {
            summary += $"{earnedBadges[i].BadgeName} ({earnedBadges[i].BadgeType})";
            if (i < earnedBadges.Count - 1)
            {
                summary += ", ";
            }
        }

        return summary;
    }

    private void LogRunStart()
    {
        Debug.Log(
            "Final Round Run Start\n" +
            $"Run Seed: {currentRunSeed}\n" +
            $"Process ID: {currentProcessId}\n" +
            $"Company Profile: {GetCompanyNameForSummary()}\n" +
            $"Rule: {(activeCompanyProfile == null ? "none" : activeCompanyProfile.RuleName)}\n" +
            $"Rule Modifiers: {(activeCompanyProfile == null ? "none" : activeCompanyProfile.GetRuleModifierSummary())}\n" +
            $"Stages: {(stages == null ? 0 : stages.Length)}\n" +
            $"Adjusted Random Event Chance: {GetAdjustedRandomEventChance():0.00}");
    }

    private void LogDemoStart()
    {
        Debug.Log(
            $"Final Round {BuildVersion} started\n" +
            $"Loaded stages: {GetLoadedStageSummary()}\n" +
            $"Random events loaded: {(randomEvents == null ? 0 : randomEvents.Length)}");
    }

    private string GetLoadedStageSummary()
    {
        if (stages == null || stages.Length == 0)
        {
            return "none";
        }

        string summary = string.Empty;

        for (int i = 0; i < stages.Length; i++)
        {
            InterviewStage stage = stages[i];
            int questionCount = stage.Questions == null ? 0 : stage.Questions.Length;
            summary += $"{stage.StageName} ({questionCount})";

            if (i < stages.Length - 1)
            {
                summary += ", ";
            }
        }

        return summary;
    }

    private sealed class StageRunSummary
    {
        public string StageName { get; }
        public int NetScoreChange { get; }

        public StageRunSummary(string stageName, int netScoreChange)
        {
            StageName = stageName;
            NetScoreChange = netScoreChange;
        }
    }

    private sealed class CallParticipantTile
    {
        public int TileIndex;
        public GameObject Root;
        public CanvasGroup CanvasGroup;
        public Image BorderImage;
        public Image TileImage;
        public Image BodyImage;
        public Image ShoulderImage;
        public Image HeadImage;
        public TMP_Text HeadText;
        public Image LabelBarImage;
        public TMP_Text RoleText;
    }

    private readonly struct CallTheme
    {
        public Color BackgroundColor { get; }
        public Color TopBarColor { get; }
        public Color AccentColor { get; }
        public Color FrameColor { get; }

        public CallTheme(Color backgroundColor, Color topBarColor, Color accentColor, Color frameColor)
        {
            BackgroundColor = backgroundColor;
            TopBarColor = topBarColor;
            AccentColor = accentColor;
            FrameColor = frameColor;
        }
    }

    private sealed class RandomEventRunSummary
    {
        public string EventTitle { get; }
        public int NetScoreImpact { get; }

        public RandomEventRunSummary(string eventTitle, int netScoreImpact)
        {
            EventTitle = eventTitle;
            NetScoreImpact = netScoreImpact;
        }
    }

    private enum RecoveryChoiceType
    {
        ReviewNotes,
        ReframeBusinessCase,
        TakeAWalk,
        MessageFriendlyAe,
        DoomScrollGlassdoor
    }

    private sealed class RecoveryChoice
    {
        public RecoveryChoiceType ChoiceType { get; }
        public string Name { get; }
        public string Description { get; }
        public string ConfirmationText { get; }
        public int ConfidenceChange { get; }
        public int EnergyChange { get; }
        public int TechnicalCredibilityChange { get; }
        public int CommercialAlignmentChange { get; }
        public int DiplomaticStyleChange { get; }
        public int BluntStyleChange { get; }
        public int CommercialStyleChange { get; }
        public int TechnicalStyleChange { get; }
        public int ChaoticStyleChange { get; }
        public int BurnedOutStyleChange { get; }
        public bool IsBadIdea { get; }
        public int UseCount { get; set; }

        public RecoveryChoice(
            RecoveryChoiceType choiceType,
            string name,
            string description,
            string confirmationText,
            int confidenceChange,
            int energyChange,
            int technicalCredibilityChange,
            int commercialAlignmentChange,
            int diplomaticStyleChange,
            int bluntStyleChange,
            int commercialStyleChange,
            int technicalStyleChange,
            int chaoticStyleChange,
            int burnedOutStyleChange,
            bool isBadIdea)
        {
            ChoiceType = choiceType;
            Name = name;
            Description = description;
            ConfirmationText = confirmationText;
            ConfidenceChange = confidenceChange;
            EnergyChange = energyChange;
            TechnicalCredibilityChange = technicalCredibilityChange;
            CommercialAlignmentChange = commercialAlignmentChange;
            DiplomaticStyleChange = diplomaticStyleChange;
            BluntStyleChange = bluntStyleChange;
            CommercialStyleChange = commercialStyleChange;
            TechnicalStyleChange = technicalStyleChange;
            ChaoticStyleChange = chaoticStyleChange;
            BurnedOutStyleChange = burnedOutStyleChange;
            IsBadIdea = isBadIdea;
        }
    }

    private readonly struct StatSummary
    {
        public string Name { get; }
        public int Value { get; }

        public StatSummary(string name, int value)
        {
            Name = name;
            Value = value;
        }
    }

    private enum PrepCardType
    {
        TakeABreath,
        ClarifyingQuestion,
        ReframeBusinessValue
    }

    private sealed class PrepCard
    {
        public PrepCardType CardType { get; }
        public string Name { get; }
        public string Description { get; }
        public int MaxUses { get; }
        public int RemainingUses { get; set; }
        public int UseCount { get; set; }

        public PrepCard(PrepCardType cardType, string name, string description, int maxUses)
        {
            CardType = cardType;
            Name = name;
            Description = description;
            MaxUses = maxUses;
            RemainingUses = maxUses;
        }
    }

}

public class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private const float HoverScale = 1.018f;
    private const float PressedScale = 0.985f;
    private const float LerpSpeed = 16f;

    private RectTransform rectTransform;
    private Vector3 baseScale;
    private Vector3 targetScale;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        baseScale = rectTransform.localScale;
        targetScale = baseScale;
    }

    private void OnDisable()
    {
        if (rectTransform != null)
        {
            rectTransform.localScale = baseScale;
            targetScale = baseScale;
        }
    }

    private void Update()
    {
        if (rectTransform == null)
        {
            return;
        }

        rectTransform.localScale = Vector3.Lerp(rectTransform.localScale, targetScale, Time.unscaledDeltaTime * LerpSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = baseScale * HoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = baseScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetScale = baseScale * PressedScale;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetScale = baseScale * HoverScale;
    }
}
