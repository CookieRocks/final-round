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
    private const float BetweenStageEventChance = 0.6f;
    private const float ScreenFadeDuration = 0.16f;
    private const string BuildVersion = "Prototype v0.1";

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

    private TMP_Text subtitleText;
    private TMP_Text progressText;
    private TMP_Text questionStageNameText;
    private TMP_Text questionStageIntroText;
    private TMP_Text questionText;
    private TMP_Text statsText;
    private TMP_Text feedbackText;
    private TMP_Text stageTransitionNameText;
    private TMP_Text stageTransitionBodyText;
    private TMP_Text stageTransitionStatsText;
    private TMP_Text randomEventTitleText;
    private TMP_Text randomEventBodyText;
    private TMP_Text randomEventChangesText;
    private TMP_Text menuBodyText;
    private TMP_Text processBriefingTitleText;
    private TMP_Text processBriefingBodyText;
    private TMP_Text outcomeTitleText;
    private TMP_Text outcomeBodyText;
    private TMP_Text outcomeStatsText;
    private TMP_Text outcomeHighlightsText;
    private TMP_Text outcomeAdviceText;
    private TMP_Text versionText;

    private Button[] answerButtons;
    private TMP_Text[] answerButtonTexts;
    private Button continueButton;
    private Button stageContinueButton;
    private Button randomEventContinueButton;
    private Button startInterviewButton;
    private Button howToPlayButton;
    private Button aboutButton;
    private Button quitButton;
    private Button beginProcessButton;
    private Button processBriefingReturnButton;
    private Button resumeButton;
    private Button pauseReturnToMenuButton;
    private GameObject feedbackPanel;
    private GameObject menuScreen;
    private GameObject processBriefingScreen;
    private GameObject questionScreen;
    private GameObject stageTransitionScreen;
    private GameObject randomEventScreen;
    private GameObject outcomeScreen;
    private GameObject pauseOverlay;
    private GameObject backdropViewportPanel;
    private InterviewRoomBackdropController roomBackdrop;
    private AudioSource uiAudioSource;
    private Coroutine activeFadeCoroutine;

    [Header("Optional UI Audio")]
    [SerializeField] private AudioClip answerSelectedClip;
    [SerializeField] private AudioClip continueClip;
    [SerializeField] private AudioClip stageCompleteClip;
    [SerializeField] private AudioClip finalOutcomeClip;

    [Header("Debug Options")]
    [SerializeField] private bool randomizeAnswerOrder = true;
    [SerializeField] private bool useCompanyProfileModifiers = true;
    [SerializeField] private bool debugAllowRepeatedRandomEvents;
    [SerializeField] private bool useDeterministicAnswerSeed;
    [SerializeField] private int debugAnswerSeed = 12345;

    private readonly PlayerStats playerStats = new PlayerStats();
    private readonly InterviewStyleTracker styleTracker = new InterviewStyleTracker();
    private int currentStageIndex;
    private int currentQuestionIndex;
    private PlayerStats stageStartStats;
    private bool currentStageWasStrong;
    private AnswerOption[] displayedAnswers;
    private System.Random answerOrderRandom;
    private readonly List<StageRunSummary> stageRunSummaries = new List<StageRunSummary>();
    private readonly List<RandomEventRunSummary> randomEventRunSummaries = new List<RandomEventRunSummary>();
    private readonly HashSet<int> usedRandomEventIndexes = new HashSet<int>();
    private int strongAnswerCount;
    private int riskyAnswerCount;

    private InterviewStage[] stages;
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

        if (!ValidateGameData())
        {
            enabled = false;
            return;
        }

        ResetGame(true);
        EnsureRoomBackdropExists();
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
            && feedbackPanel != null
            && feedbackText != null
            && continueButton != null
            && stageTransitionScreen != null
            && stageContinueButton != null
            && randomEventScreen != null
            && randomEventContinueButton != null
            && outcomeScreen != null
            && outcomeHighlightsText != null
            && outcomeAdviceText != null
            && backdropViewportPanel != null
            && pauseOverlay != null
            && resumeButton != null
            && pauseReturnToMenuButton != null
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

        GameObject background = CreatePanel("Dark Background", canvasObject.transform, backgroundColor);
        StretchToParent(background.GetComponent<RectTransform>());

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
        CreateRandomEventScreen(safeArea.transform);
        CreateOutcomeScreen(safeArea.transform);
        CreatePauseOverlay(canvasObject.transform);
        CreateVersionLabel(canvasObject.transform);
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
        ConfigurePreferredLayoutElement(header, -1f, 150f);

        VerticalLayoutGroup headerLayout = header.GetComponent<VerticalLayoutGroup>();
        headerLayout.spacing = 6f;
        headerLayout.childAlignment = TextAnchor.MiddleLeft;
        headerLayout.childControlWidth = true;
        headerLayout.childControlHeight = true;
        headerLayout.childForceExpandWidth = true;
        headerLayout.childForceExpandHeight = false;

        TMP_Text titleText = CreateText("Title", header.transform, "FINAL ROUND", 58, FontStyles.Bold, TextAlignmentOptions.Left);
        titleText.color = textColor;
        titleText.characterSpacing = 4f;

        subtitleText = CreateText(
            "Subtitle",
            header.transform,
            "A multi-stage interview gauntlet about confidence, stamina, credibility, and commercial judgment.",
            25,
            FontStyles.Normal,
            TextAlignmentOptions.Left);
        subtitleText.color = mutedTextColor;

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
        CreateFeedbackPanel(questionScreen.transform);
        CreateAnswerButtons(questionScreen.transform);
    }

    private void CreateBackdropViewportPanel(Transform parent)
    {
        backdropViewportPanel = CreatePanel("Backdrop Viewport Panel", parent, panelAccentColor);
        ConfigurePreferredLayoutElement(backdropViewportPanel, 600f, -1f);

        GameObject viewportObject = new GameObject("BackdropViewport", typeof(RectTransform), typeof(RawImage));
        viewportObject.transform.SetParent(backdropViewportPanel.transform, false);

        RawImage viewportImage = viewportObject.GetComponent<RawImage>();
        viewportImage.texture = roomBackdrop == null ? null : roomBackdrop.ViewportTexture;
        viewportImage.color = Color.white;
        viewportImage.raycastTarget = false;

        RectTransform viewportRect = viewportObject.GetComponent<RectTransform>();
        StretchToParent(viewportRect);
        viewportRect.offsetMin = new Vector2(8f, 8f);
        viewportRect.offsetMax = new Vector2(-8f, -8f);
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
        howToPlayButton = CreateMenuButton(menuScreen.transform, "How To Play", ShowHowToPlay);
        aboutButton = CreateMenuButton(menuScreen.transform, "About", ShowAbout);
        quitButton = CreateMenuButton(menuScreen.transform, "Quit", QuitGame);
        quitButton.gameObject.SetActive(!Application.isEditor);

        menuScreen.SetActive(false);
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

        beginProcessButton = CreateMenuButton(buttonRow.transform, "Begin Process", BeginProcess);
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
        pauseReturnToMenuButton = CreateMenuButton(pausePanel.transform, "Return to Menu", ReturnToMenuFromPause);
        pauseOverlay.SetActive(false);
    }

    private Button CreateMenuButton(Transform parent, string label, UnityEngine.Events.UnityAction onClick)
    {
        GameObject buttonObject = CreateButton(label + " Button", parent);
        ConfigurePreferredLayoutElement(buttonObject, -1f, 72f);

        TMP_Text buttonText = CreateText("Label", buttonObject.transform, label, 24, FontStyles.Bold, TextAlignmentOptions.Center);
        buttonText.color = textColor;
        StretchToParent(buttonText.GetComponent<RectTransform>());

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(onClick);
        return button;
    }

    private void CreateQuestionPanel(Transform parent)
    {
        GameObject panel = CreatePanel("Question Text Area", parent, panelColor);
        ConfigureFlexibleLayoutElement(panel, 1f);
        AddPaddingLayout(panel, new RectOffset(34, 34, 30, 32), 12f);

        questionStageNameText = CreateText("Question Stage Name", panel.transform, string.Empty, 30, FontStyles.Bold, TextAlignmentOptions.Left);
        questionStageNameText.color = accentColor;

        questionStageIntroText = CreateText("Question Stage Intro", panel.transform, string.Empty, 22, FontStyles.Normal, TextAlignmentOptions.Left);
        questionStageIntroText.color = mutedTextColor;
        questionStageIntroText.textWrappingMode = TextWrappingModes.Normal;

        questionText = CreateText("Question Text", panel.transform, string.Empty, 32, FontStyles.Bold, TextAlignmentOptions.TopLeft);
        questionText.textWrappingMode = TextWrappingModes.Normal;
        questionText.color = textColor;
        ConfigureFlexibleLayoutElement(questionText.gameObject, 1f);
    }

    private void CreateStatsPanel(Transform parent)
    {
        GameObject panel = CreatePanel("Stats Panel", parent, panelAccentColor);
        ConfigurePreferredLayoutElement(panel, -1f, 152f);
        SetMinimumLayoutHeight(panel, 146f);
        AddPaddingLayout(panel, new RectOffset(26, 26, 18, 20), 10f);

        TMP_Text statsTitle = CreateText("Stats Title", panel.transform, "CANDIDATE READ", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        statsTitle.color = accentColor;

        statsText = CreateText("Stats Text", panel.transform, string.Empty, 26, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        statsText.color = textColor;
        statsText.lineSpacing = 18f;
        ConfigurePreferredLayoutElement(statsText.gameObject, -1f, 88f);
        SetMinimumLayoutHeight(statsText.gameObject, 84f);
        ConfigureFlexibleLayoutElement(statsText.gameObject, 1f);
    }

    private void CreateFeedbackPanel(Transform parent)
    {
        feedbackPanel = CreatePanel("Answer Feedback Panel", parent, panelAccentColor);
        ConfigurePreferredLayoutElement(feedbackPanel, -1f, 324f);
        SetMinimumLayoutHeight(feedbackPanel, 310f);
        AddPaddingLayout(feedbackPanel, new RectOffset(32, 32, 24, 30), 20f);

        TMP_Text feedbackTitle = CreateText("Feedback Title", feedbackPanel.transform, "INTERVIEWER REACTION", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        feedbackTitle.color = accentColor;

        feedbackText = CreateText("Feedback Text", feedbackPanel.transform, string.Empty, 28, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        feedbackText.color = textColor;
        feedbackText.textWrappingMode = TextWrappingModes.Normal;
        feedbackText.lineSpacing = 9f;
        ConfigurePreferredLayoutElement(feedbackText.gameObject, -1f, 186f);
        SetMinimumLayoutHeight(feedbackText.gameObject, 178f);

        GameObject continueButtonObject = CreateButton("Continue Button", feedbackPanel.transform);
        ConfigurePreferredLayoutElement(continueButtonObject, -1f, 58f);
        SetMinimumLayoutHeight(continueButtonObject, 58f);

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

        outcomeTitleText = CreateText("Outcome Title", outcomeScreen.transform, string.Empty, 48, FontStyles.Bold, TextAlignmentOptions.Left);
        outcomeTitleText.color = accentColor;

        outcomeBodyText = CreateText("Outcome Body", outcomeScreen.transform, string.Empty, 27, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        outcomeBodyText.color = textColor;
        outcomeBodyText.textWrappingMode = TextWrappingModes.Normal;
        outcomeBodyText.lineSpacing = 8f;
        ConfigurePreferredLayoutElement(outcomeBodyText.gameObject, -1f, 150f);
        SetMinimumLayoutHeight(outcomeBodyText.gameObject, 122f);

        GameObject outcomeContentRow = new GameObject("Outcome Content Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        outcomeContentRow.transform.SetParent(outcomeScreen.transform, false);
        ConfigureFlexibleLayoutElement(outcomeContentRow, 1f);
        SetMinimumLayoutHeight(outcomeContentRow, 250f);

        HorizontalLayoutGroup contentRowLayout = outcomeContentRow.GetComponent<HorizontalLayoutGroup>();
        contentRowLayout.spacing = 22f;
        contentRowLayout.childControlWidth = true;
        contentRowLayout.childControlHeight = true;
        contentRowLayout.childForceExpandWidth = true;
        contentRowLayout.childForceExpandHeight = true;

        GameObject statsPanel = CreatePanel("Outcome Stats Panel", outcomeContentRow.transform, panelAccentColor);
        ConfigureFlexibleLayoutElement(statsPanel, 1f, 480f);
        AddPaddingLayout(statsPanel, new RectOffset(24, 24, 20, 20), 10f);

        TMP_Text statsHeading = CreateText("Outcome Stats Heading", statsPanel.transform, "FINAL READ", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        statsHeading.color = accentColor;

        outcomeStatsText = CreateText("Outcome Stats", statsPanel.transform, string.Empty, 25, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        outcomeStatsText.color = textColor;
        outcomeStatsText.lineSpacing = 13f;
        ConfigureFlexibleLayoutElement(outcomeStatsText.gameObject, 1f);

        GameObject highlightsPanel = CreatePanel("Outcome Highlights Panel", outcomeContentRow.transform, panelAccentColor);
        ConfigureFlexibleLayoutElement(highlightsPanel, 1f, 480f);
        AddPaddingLayout(highlightsPanel, new RectOffset(24, 24, 20, 20), 10f);

        TMP_Text highlightsHeading = CreateText("Outcome Highlights Heading", highlightsPanel.transform, "RUN HIGHLIGHTS", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        highlightsHeading.color = accentColor;

        outcomeHighlightsText = CreateText("Outcome Highlights", highlightsPanel.transform, string.Empty, 25, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        outcomeHighlightsText.color = textColor;
        outcomeHighlightsText.lineSpacing = 13f;
        outcomeHighlightsText.textWrappingMode = TextWrappingModes.Normal;
        ConfigureFlexibleLayoutElement(outcomeHighlightsText.gameObject, 1f);

        GameObject advicePanel = CreatePanel("Outcome Advice Panel", outcomeScreen.transform, transitionPanelColor);
        ConfigurePreferredLayoutElement(advicePanel, -1f, 104f);
        SetMinimumLayoutHeight(advicePanel, 96f);
        AddPaddingLayout(advicePanel, new RectOffset(24, 24, 16, 16), 8f);

        TMP_Text adviceHeading = CreateText("Outcome Advice Heading", advicePanel.transform, "NEXT RUN ADVICE", 21, FontStyles.Bold, TextAlignmentOptions.Left);
        adviceHeading.color = accentColor;

        outcomeAdviceText = CreateText("Outcome Advice", advicePanel.transform, string.Empty, 25, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        outcomeAdviceText.color = textColor;
        outcomeAdviceText.textWrappingMode = TextWrappingModes.Normal;
        ConfigureFlexibleLayoutElement(outcomeAdviceText.gameObject, 1f);

        GameObject buttonRow = new GameObject("Outcome Button Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        buttonRow.transform.SetParent(outcomeScreen.transform, false);
        ConfigurePreferredLayoutElement(buttonRow, -1f, 68f);

        HorizontalLayoutGroup buttonRowLayout = buttonRow.GetComponent<HorizontalLayoutGroup>();
        buttonRowLayout.spacing = 18f;
        buttonRowLayout.childControlWidth = true;
        buttonRowLayout.childControlHeight = true;
        buttonRowLayout.childForceExpandWidth = true;
        buttonRowLayout.childForceExpandHeight = true;

        CreateMenuButton(buttonRow.transform, "Restart Interview", RestartGame);
        CreateMenuButton(buttonRow.transform, "Return to Menu", ShowMenu);
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
        Color normalColor = selected ? selectedAnswerColor : buttonColor;
        Color disabledColor = selected ? selectedAnswerColor : disabledAnswerColor;

        button.colors = BuildButtonColors(normalColor, buttonHoverColor, disabledColor);
        image.color = selected || button.interactable ? normalColor : disabledAnswerColor;
        answerButtonTexts[answerIndex].fontStyle = selected ? FontStyles.Bold : FontStyles.Normal;
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

        activeFadeCoroutine = StartCoroutine(FadeCanvasGroup(canvasGroup));
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup)
    {
        canvasGroup.alpha = 0f;

        float elapsed = 0f;

        while (elapsed < ScreenFadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsed / ScreenFadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        activeFadeCoroutine = null;
    }

    private void PlayUiSound(AudioClip clip)
    {
        if (uiAudioSource == null || clip == null)
        {
            return;
        }

        uiAudioSource.PlayOneShot(clip);
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
                        })
                }),

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
                        })
                }),

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
                        })
                }),

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
                        })
                })
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
                "Default process pressure.",
                "Balanced, commercially aware answers land well."),
            new CompanyProfile(
                "Startup Rocketship",
                "Fast, Messy, Urgent",
                "They care about pace, ownership, and whether you can keep signal through moving parts.",
                "Random events are slightly more likely; event energy losses are one point sharper.",
                "Decisive answers help, but chaos has a cost.",
                randomEventChanceModifier: 0.12f,
                eventEnergyLossModifier: -1),
            new CompanyProfile(
                "Security Vendor",
                "Risk-Framing Process",
                "They care about risk framing, technical credibility, and clean customer communication.",
                "No direct stat changes; technical and commercial framing matter in the authored answers.",
                "Technical and commercial credibility are especially important signals."),
            new CompanyProfile(
                "AI Hype Company",
                "Narrative-Heavy Growth Motion",
                "They care about vision, speed, and whether you can stay grounded when the room starts saying agentic.",
                "Risky AI-flavoured answers and chaotic events increase chaotic style slightly faster.",
                "Grounded enthusiasm beats demo-theatre.",
                aiChaoticStyleBonus: 1),
            new CompanyProfile(
                "Legacy Enterprise",
                "Careful Procurement Maze",
                "They care about patience, stakeholder management, and whether you can keep energy through process drag.",
                "Random chaos is slightly lower; between-stage energy recovery is reduced.",
                "Steady diplomatic answers travel best.",
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

            if (stage.Questions == null || stage.Questions.Length == 0)
            {
                Debug.LogError($"Final Round setup error: '{stage.StageName}' has no questions.");
                return false;
            }

            for (int questionIndex = 0; questionIndex < stage.Questions.Length; questionIndex++)
            {
                InterviewQuestion question = stage.Questions[questionIndex];

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
        if (chooseNewCompanyProfile)
        {
            SelectRandomCompanyProfile();
        }

        InitializeAnswerOrderRandom();
        displayedAnswers = null;
        stageRunSummaries.Clear();
        randomEventRunSummaries.Clear();
        usedRandomEventIndexes.Clear();
        strongAnswerCount = 0;
        riskyAnswerCount = 0;
        playerStats.Reset(StartingConfidence, StartingEnergy, StartingTechnicalCredibility, StartingCommercialAlignment);
        styleTracker.Reset();
        currentStageIndex = 0;
        currentQuestionIndex = 0;
        CaptureStageStartStats();
    }

    private void SelectRandomCompanyProfile()
    {
        if (companyProfiles == null || companyProfiles.Length == 0)
        {
            activeCompanyProfile = null;
            return;
        }

        activeCompanyProfile = companyProfiles[Random.Range(0, companyProfiles.Length)];
    }

    private void InitializeAnswerOrderRandom()
    {
        if (useDeterministicAnswerSeed)
        {
            answerOrderRandom = new System.Random(debugAnswerSeed);
            return;
        }

        answerOrderRandom = new System.Random(System.Guid.NewGuid().GetHashCode());
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
        if (activeCompanyProfile == null)
        {
            SelectRandomCompanyProfile();
        }

        ResetGame(false);
        ShowProcessBriefing();
    }

    private void BeginProcess()
    {
        HidePauseOverlay();
        PlayUiSound(continueClip);
        ShowQuestionScreen();
    }

    private void ShowProcessBriefing()
    {
        HidePauseOverlay();
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(true);
        questionScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(false);

        progressText.text = "Process Briefing";
        subtitleText.text = "Read the room before the first call.";
        processBriefingTitleText.text = "TODAY'S PROCESS";
        processBriefingBodyText.text = BuildProcessBriefingBody();
        UpdateRoomBackdrop("Main Menu");
        FadeInScreen(processBriefingScreen);
    }

    private void ShowMenu()
    {
        SelectRandomCompanyProfile();
        HidePauseOverlay();
        menuScreen.SetActive(true);
        processBriefingScreen.SetActive(false);
        questionScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
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
            "Created as a playable Unity demo with runtime UI and a cosmetic 3D interview-room viewport.";
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
            $"{activeCompanyProfile.StatModifierNotes}";
    }

    private string GetCompanyProcessLine()
    {
        if (activeCompanyProfile == null)
        {
            return string.Empty;
        }

        return $"{activeCompanyProfile.CompanyName}: {activeCompanyProfile.PreferredStyle}";
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
            $"Today's process: <b>{activeCompanyProfile.CompanyName}</b>\n" +
            $"{activeCompanyProfile.ProfileName}\n\n" +
            $"{activeCompanyProfile.Description}\n\n" +
            $"Rewards: {activeCompanyProfile.PreferredStyle}\n" +
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
            _ => false
        };
#else
        return number switch
        {
            1 => Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Keypad1),
            2 => Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Keypad2),
            3 => Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Keypad3),
            _ => false
        };
#endif
    }

    private void HandleEscapeShortcut()
    {
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
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(false);
        questionScreen.SetActive(true);
        stageTransitionScreen.SetActive(false);
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

        if (currentQuestionIndex >= stage.Questions.Length)
        {
            ShowStageTransition();
            return;
        }

        InterviewQuestion question = stage.Questions[currentQuestionIndex];
        displayedAnswers = BuildDisplayedAnswerOrder(question);
        progressText.text = $"{stage.StageName} - Question {currentQuestionIndex + 1} of {stage.Questions.Length}";
        subtitleText.text = GetCompanyProcessLine();
        UpdateRoomBackdrop(stage.StageName);
        questionStageNameText.text = stage.StageName.ToUpperInvariant();
        questionStageIntroText.text = $"{stage.StageIntroText}\nToday's process: {GetCompanyProcessLine()}";
        questionText.text = question.QuestionText;
        feedbackPanel.SetActive(false);
        SetBackdropViewportVisible(true);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].interactable = true;
            answerButtonTexts[i].text = displayedAnswers[i].AnswerText;
            SetAnswerButtonVisual(i, false);
        }
    }

    private AnswerOption[] BuildDisplayedAnswerOrder(InterviewQuestion question)
    {
        AnswerOption[] answers = new AnswerOption[question.Answers.Length];
        question.Answers.CopyTo(answers, 0);

        if (!randomizeAnswerOrder || answers.Length <= 1)
        {
            return answers;
        }

        if (answerOrderRandom == null)
        {
            InitializeAnswerOrderRandom();
        }

        for (int i = answers.Length - 1; i > 0; i--)
        {
            int swapIndex = answerOrderRandom.Next(i + 1);
            (answers[i], answers[swapIndex]) = (answers[swapIndex], answers[i]);
        }

        return answers;
    }

    private void ChooseAnswer(int answerIndex)
    {
        InterviewQuestion question = stages[currentStageIndex].Questions[currentQuestionIndex];
        if (displayedAnswers == null || displayedAnswers.Length != question.Answers.Length)
        {
            Debug.LogError("Final Round runtime error: displayed answer order was not prepared for the current question.");
            return;
        }

        AnswerOption answer = displayedAnswers[answerIndex];

        PlayUiSound(answerSelectedClip);
        playerStats.Apply(answer);
        styleTracker.Apply(answer);
        ApplyCompanyStyleModifier(answer);
        TrackAnswerSummary(answer);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            bool selected = i == answerIndex;
            answerButtons[i].interactable = false;
            SetAnswerButtonVisual(i, selected);
            answerButtonTexts[i].text = displayedAnswers[i].AnswerText;
        }

        UpdateStatsText();
        ShowFeedback(answer);
    }

    private void TrackAnswerSummary(AnswerOption answer)
    {
        int totalStatChange = GetAnswerStatImpact(answer);

        if (totalStatChange >= 15 && answer.ChaoticStyleChange == 0 && answer.BurnedOutStyleChange == 0)
        {
            strongAnswerCount++;
        }

        if (totalStatChange < 0
            || answer.ChaoticStyleChange > 0
            || answer.BurnedOutStyleChange > 0
            || answer.BluntStyleChange > 1)
        {
            riskyAnswerCount++;
        }
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

    private void ShowFeedback(AnswerOption answer)
    {
        feedbackText.text =
            $"{answer.ConsequenceText}\n\n" +
            "<b>Stat changes</b>\n" +
            $"{FormatStatChange("Confidence", answer.ConfidenceChange)}\n" +
            $"{FormatStatChange("Energy", answer.EnergyChange)}\n" +
            $"{FormatStatChange("Technical Credibility", answer.TechnicalCredibilityChange)}\n" +
            $"{FormatStatChange("Commercial Alignment", answer.CommercialAlignmentChange)}";

        feedbackPanel.SetActive(true);
        SetBackdropViewportVisible(false);
    }

    private void SetBackdropViewportVisible(bool visible)
    {
        if (backdropViewportPanel != null)
        {
            backdropViewportPanel.SetActive(visible);
        }
    }

    private string FormatStatChange(string statName, int change)
    {
        string sign = change > 0 ? "+" : string.Empty;
        string color = GetStatChangeColor(change);
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
        int burnedOutStyleChange)
    {
        string summary = string.Empty;
        AppendChangeLine(ref summary, "Confidence", confidenceChange);
        AppendChangeLine(ref summary, "Energy", energyChange);
        AppendChangeLine(ref summary, "Technical Credibility", technicalCredibilityChange);
        AppendChangeLine(ref summary, "Commercial Alignment", commercialAlignmentChange);
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
        PlayUiSound(continueClip);
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
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(false);

        progressText.text = $"{stage.StageName} complete";
        subtitleText.text = "Quick reset before the next conversation.";
        UpdateRoomBackdrop(stage.StageName);
        stageTransitionNameText.text = stage.StageName;
        stageTransitionBodyText.text = BuildStageFeedback(stage);
        stageTransitionStatsText.text =
            $"{stage.StageCompleteText}\n\n" +
            GetStatsSummary() +
            BuildTransitionBonusText();
        PlayUiSound(stageCompleteClip);
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

        return notes[Random.Range(0, notes.Length)];
    }

    private void ContinueAfterStageTransition()
    {
        PlayUiSound(continueClip);
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
        }

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
            && Random.value < GetAdjustedRandomEventChance();
    }

    private float GetAdjustedRandomEventChance()
    {
        float modifier = !useCompanyProfileModifiers || activeCompanyProfile == null
            ? 0f
            : activeCompanyProfile.RandomEventChanceModifier;
        return Mathf.Clamp01(BetweenStageEventChance + modifier);
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
        ApplyRandomEvent(interviewEvent);

        HidePauseOverlay();
        questionScreen.SetActive(false);
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
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
                interviewEvent.BurnedOutStyleChange) +
            "\n\n" +
            GetStatsSummary();
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
            return Random.Range(0, randomEvents.Length);
        }

        int unusedCount = randomEvents.Length - usedRandomEventIndexes.Count;
        if (unusedCount <= 0)
        {
            return -1;
        }

        int targetUnusedIndex = Random.Range(0, unusedCount);
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

    private void ApplyRandomEvent(RandomInterviewEvent interviewEvent)
    {
        int totalScoreBefore = playerStats.TotalScore;
        playerStats.Apply(interviewEvent);
        styleTracker.Apply(interviewEvent);
        randomEventRunSummaries.Add(new RandomEventRunSummary(interviewEvent.EventTitle, playerStats.TotalScore - totalScoreBefore));
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
        }

        int chaoticStyleChange = interviewEvent.ChaoticStyleChange;
        if (activeCompanyProfile.AiChaoticStyleBonus > 0 && IsAiOrChaoticSignal(interviewEvent.EventTitle + " " + interviewEvent.EventDescription))
        {
            chaoticStyleChange += activeCompanyProfile.AiChaoticStyleBonus;
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
        PlayUiSound(continueClip);
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

        return "\n\n<b>Continue bonus</b>\n" +
            $"{FormatStatChange("Energy", 10)}\n" +
            $"{FormatStatChange("Confidence", currentStageWasStrong ? 5 : 0)}";
    }

    private void UpdateStatsText()
    {
        statsText.text = GetStatsSummary();
    }

    private string GetStatsSummary()
    {
        return
            $"Confidence: <b>{playerStats.Confidence}/100</b>\n" +
            $"Energy: <b>{playerStats.Energy}/100</b>\n" +
            $"Technical Credibility: <b>{playerStats.TechnicalCredibility}/100</b>\n" +
            $"Commercial Alignment: <b>{playerStats.CommercialAlignment}/100</b>";
    }

    private void ShowOutcome()
    {
        HidePauseOverlay();
        questionScreen.SetActive(false);
        menuScreen.SetActive(false);
        processBriefingScreen.SetActive(false);
        stageTransitionScreen.SetActive(false);
        randomEventScreen.SetActive(false);
        outcomeScreen.SetActive(true);
        progressText.text = "Interview process complete";
        subtitleText.text = "Final hiring feedback across every round.";
        UpdateRoomBackdrop("Final Outcome");

        int totalScore = playerStats.TotalScore;
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
        else if (totalScore >= 330)
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

        outcomeStatsText.text = BuildFinalReadSummary(outcomeName, styleResult);
        outcomeHighlightsText.text = BuildRunHighlightsSummary();
        outcomeAdviceText.text = BuildRunAdvice();

        PlayUiSound(finalOutcomeClip);
        FadeInScreen(outcomeScreen);
        LogRunSummary(outcomeName, styleResult);
    }

    private string BuildOutcomeBody(string mainFeedback, string finalComment, InterviewStyleResult styleResult)
    {
        return
            $"{mainFeedback}\n\n" +
            $"{BuildCompanyOutcomeLine()}\n\n" +
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

    private string BuildFinalReadSummary(string outcomeName, InterviewStyleResult styleResult)
    {
        StatSummary strongestStat = GetStrongestStat();
        StatSummary weakestStat = GetWeakestStat();

        return
            $"Company: <b>{GetCompanyNameForSummary()}</b>\n" +
            $"Outcome: <b>{outcomeName}</b>\n" +
            $"Style: <b>{styleResult.StyleName}</b>\n" +
            $"Total Score: <b>{playerStats.TotalScore}/400</b>\n\n" +
            BuildCompactStatsSummary() + "\n\n" +
            $"Strongest Stat: <b>{strongestStat.Name}</b> ({strongestStat.Value}/100)\n" +
            $"Weakest Stat: <b>{weakestStat.Name}</b> ({weakestStat.Value}/100)";
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
            $"Answer mix: <b>{strongAnswerCount}</b> strong | <b>{riskyAnswerCount}</b> risky";
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

    private string BuildRunAdvice()
    {
        StatSummary weakestStat = GetWeakestStat();

        if (styleTracker.ChaoticStyle >= 5 || riskyAnswerCount >= 7)
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

        return messages[Random.Range(0, messages.Length)];
    }

    private void UpdateRoomBackdrop(string stageName)
    {
        if (roomBackdrop == null)
        {
            EnsureRoomBackdropExists();
        }

        if (roomBackdrop != null)
        {
            roomBackdrop.SetStageAtmosphere(stageName);
        }
    }

    private void LogRunSummary(string outcomeName, InterviewStyleResult styleResult)
    {
        Debug.Log(
            "Final Round Run Summary\n" +
            $"Outcome: {outcomeName}\n" +
            $"Total Score: {playerStats.TotalScore}/400\n" +
            playerStats.GetSummary() + "\n" +
            styleTracker.GetDebugSummary() + "\n" +
            $"Dominant Style: {styleResult.StyleName}");
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
