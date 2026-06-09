using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public enum ReactionTone
{
    Positive,
    Neutral,
    Awkward,
    Concerned
}

public enum ReactionSpeaker
{
    HiringManager,
    SecurityArchitect,
    SalesDirector,
    Room
}

public sealed class CybersecurityPresalesInterviewFlow : MonoBehaviour
{
    private readonly InterviewScore score = new InterviewScore();
    [SerializeField] private float positiveReactionDuration = 1.1f;
    [SerializeField] private float neutralReactionDuration = 1.25f;
    [SerializeField] private float awkwardReactionDuration = 1.45f;
    [SerializeField] private float concernedReactionDuration = 1.65f;
    [SerializeField] private bool debugForceOutcome;
    [SerializeField] private InterviewOutcomeType debugForcedOutcome = InterviewOutcomeType.Pass;
    [SerializeField] private bool useDeterministicQuestionSeed;
    [SerializeField] private int deterministicQuestionSeed = 10603;
    [SerializeField] private float outcomeTransitionDelay = 1.15f;

    private InterviewQuestionData[] contextQuestionPool;
    private InterviewQuestionData[] technicalQuestionPool;
    private InterviewQuestionData[] commercialQuestionPool;
    private InterviewQuestionData[] questions;
    private System.Random questionRandom;
    private TheRoomPrototypeController roomController;
    private int currentQuestionIndex;
    private bool answerLocked;

    private Canvas canvas;
    private GameObject panelRoot;
    private GameObject questionPanel;
    private GameObject transitionPanel;
    private GameObject outcomePanel;
    private GameObject scorecardPanel;
    private GameObject debugPanel;
    private TMP_Text interviewerText;
    private TMP_Text questionText;
    private TMP_Text reactionText;
    private TMP_Text transitionText;
    private TMP_Text outcomeFromText;
    private TMP_Text outcomeTitleText;
    private TMP_Text outcomeOpeningText;
    private TMP_Text outcomeBodyText;
    private TMP_Text outcomeFeedbackText;
    private TMP_Text scorecardText;
    private TMP_Text debugStatusText;
    private Button[] answerButtons;
    private TMP_Text[] answerButtonTexts;

    public bool IsDebugPanelVisible => debugPanel != null && debugPanel.activeSelf;

    private void Awake()
    {
        roomController = GetComponent<TheRoomPrototypeController>();
        BuildQuestions();
        BuildUi();
        ResetFlow();
    }

    private void Update()
    {
        if (WasDebugTogglePressed())
        {
            ToggleDebugPanel();
        }
    }

    public void BeginInterview()
    {
        EnsureUiReady();
        currentQuestionIndex = 0;
        answerLocked = false;
        score.Reset();
        SelectQuestionsForRun();
        if (roomController == null)
        {
            roomController = GetComponent<TheRoomPrototypeController>();
        }

        roomController?.ClearJudgementReaction();
        panelRoot.SetActive(true);
        questionPanel.SetActive(true);
        transitionPanel.SetActive(false);
        outcomePanel.SetActive(false);
        scorecardPanel.SetActive(false);
        ClearOutcomeText();
        ShowCurrentQuestion();
        RefreshDebugStatus();
    }

    public void ResetFlow()
    {
        if (panelRoot != null)
        {
            panelRoot.SetActive(false);
        }

        StopAllCoroutines();
        currentQuestionIndex = 0;
        answerLocked = false;
        score.Reset();
        roomController?.ClearJudgementReaction();
        ClearQuestionText();
        ClearOutcomeText();
        if (debugPanel != null)
        {
            debugPanel.SetActive(false);
        }
    }

    private void ShowCurrentQuestion()
    {
        if (questions == null || questions.Length == 0)
        {
            Debug.LogWarning("Final Round RC7: no selected questions were available. Re-selecting question bank.");
            SelectQuestionsForRun();
        }

        if (currentQuestionIndex >= questions.Length)
        {
            StartCoroutine(ShowOutcomeAfterTransition());
            return;
        }

        InterviewQuestionData question = questions[currentQuestionIndex];
        if (question == null || question.Answers == null || question.Answers.Length != 4)
        {
            Debug.LogWarning($"Final Round RC7: question {currentQuestionIndex + 1} is missing or malformed. Skipping to outcome.");
            ShowOutcomeEmail();
            return;
        }

        interviewerText.text = question.InterviewerName;
        questionText.text = question.QuestionText;
        reactionText.text = string.Empty;
        answerLocked = false;
        roomController?.ClearJudgementReaction();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answerIndex = i;
            AnswerData answer = question.Answers[i];
            answerButtonTexts[i].text = answer.Text;
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].interactable = true;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => ChooseAnswer(answerIndex));
        }
    }

    private void ChooseAnswer(int answerIndex)
    {
        if (answerLocked || currentQuestionIndex >= questions.Length)
        {
            return;
        }

        answerLocked = true;
        InterviewQuestionData question = questions[currentQuestionIndex];
        if (question == null || question.Answers == null || answerIndex < 0 || answerIndex >= question.Answers.Length)
        {
            Debug.LogWarning("Final Round RC7: answer selection was invalid.");
            return;
        }

        AnswerData answer = question.Answers[answerIndex];
        score.Apply(answer);
        ReactionResult reaction = DetermineReaction(answer, currentQuestionIndex == questions.Length - 1);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;
            answerButtons[i].gameObject.SetActive(false);
        }

        reactionText.text = reaction.Text;
        roomController?.ApplyJudgementReaction(reaction.Speaker, reaction.Tone);
        StartCoroutine(ContinueAfterReaction(reaction.Duration));
    }

    private IEnumerator ContinueAfterReaction(float duration)
    {
        yield return new WaitForSeconds(duration);
        roomController?.ClearJudgementReaction();
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

    private void ShowOutcomeEmail()
    {
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(false);
        }

        questionPanel.SetActive(false);
        outcomePanel.SetActive(true);
        scorecardPanel.SetActive(false);
        ClearQuestionText();

        InterviewOutcomeType outcome = GetOutcome();
        OutcomeEmail email = OutcomeEmailGenerator.Generate(outcome, score.ToSnapshot());
        outcomeFromText.text = email.FromLine;
        outcomeTitleText.text = email.SubjectLine;
        outcomeOpeningText.text = email.OpeningLine;
        outcomeBodyText.text = email.OutcomeParagraph;
        outcomeFeedbackText.text = email.FeedbackParagraph;
        RefreshDebugStatus();
    }

    private void ShowScorecard()
    {
        scorecardPanel.SetActive(true);
        scorecardText.text =
            "<b>Scorecard</b>\n" +
            $"Technical: {score.Technical}\n" +
            $"Commercial: {score.Commercial}\n" +
            $"Rapport: {score.Rapport}\n" +
            $"Energy: {score.Energy}\n\n" +
            BuildScorecardReadout();
    }

    private void RestartRun()
    {
        roomController?.ResetRun();
    }

    private void SkipToOutcome()
    {
        EnsureUiReady();
        StopAllCoroutines();
        panelRoot.SetActive(true);
        roomController?.ClearJudgementReaction();
        ShowOutcomeEmail();
    }

    private IEnumerator ShowOutcomeAfterTransition()
    {
        questionPanel.SetActive(false);
        ClearQuestionText();
        if (outcomePanel != null)
        {
            outcomePanel.SetActive(false);
        }
        if (scorecardPanel != null)
        {
            scorecardPanel.SetActive(false);
        }

        transitionPanel.SetActive(true);
        transitionText.text = "Inbox: 1 new message";
        yield return new WaitForSeconds(outcomeTransitionDelay);
        transitionText.text = "Later that afternoon...";
        yield return new WaitForSeconds(0.55f);
        ShowOutcomeEmail();
    }

    private void ToggleDebugPanel()
    {
        EnsureUiReady();
        bool nextState = debugPanel == null || !debugPanel.activeSelf;
        debugPanel.SetActive(nextState);
        RefreshDebugStatus();
    }

    private void SetDeterministicSeedEnabled(bool enabled)
    {
        useDeterministicQuestionSeed = enabled;
        RefreshDebugStatus();
    }

    private void CycleSeed()
    {
        deterministicQuestionSeed = Mathf.Abs(deterministicQuestionSeed + 101);
        SelectQuestionsForRun();
        RefreshDebugStatus();
    }

    private void ForceOutcome(InterviewOutcomeType outcome)
    {
        debugForceOutcome = true;
        debugForcedOutcome = outcome;
        RefreshDebugStatus();
        if (outcomePanel != null && outcomePanel.activeSelf)
        {
            ShowOutcomeEmail();
        }
    }

    private void ClearForcedOutcome()
    {
        debugForceOutcome = false;
        RefreshDebugStatus();
    }

    private ReactionResult DetermineReaction(AnswerData answer, bool isFinalAnswer)
    {
        int totalDelta = answer.TotalDelta;
        ReactionTone tone;
        ReactionSpeaker speaker;
        string text;

        if (answer.Technical >= 2 && answer.Commercial <= 0)
        {
            tone = ReactionTone.Awkward;
            speaker = ReactionSpeaker.SalesDirector;
            text = "The Architect seems satisfied. The Sales Director does not write anything down.";
        }
        else if (answer.Commercial >= 2 && answer.Technical <= 0)
        {
            tone = ReactionTone.Awkward;
            speaker = ReactionSpeaker.SecurityArchitect;
            text = "The Sales Director makes a note. The Architect leans back slightly.";
        }
        else if (answer.Rapport >= 2)
        {
            tone = ReactionTone.Positive;
            speaker = ReactionSpeaker.HiringManager;
            text = "The Hiring Manager nods slowly.";
        }
        else if (answer.Rapport <= -1 || answer.Energy <= -1)
        {
            tone = ReactionTone.Concerned;
            speaker = ReactionSpeaker.Room;
            text = "Nobody speaks for a moment.";
        }
        else if (totalDelta >= 5)
        {
            tone = ReactionTone.Positive;
            speaker = ReactionSpeaker.Room;
            text = "The panel exchange a brief look.";
        }
        else if (totalDelta >= 2)
        {
            tone = ReactionTone.Neutral;
            speaker = ReactionSpeaker.Room;
            text = "The Sales Director makes a note.";
        }
        else
        {
            tone = ReactionTone.Concerned;
            speaker = ReactionSpeaker.Room;
            text = "The room goes quiet.";
        }

        if (isFinalAnswer)
        {
            text = tone switch
            {
                ReactionTone.Positive => "The panel sit with the answer for a moment.",
                ReactionTone.Neutral => "The room goes quiet while the panel finish their notes.",
                ReactionTone.Awkward => "The panel exchange a brief look before returning to their laptops.",
                _ => "The Architect writes something down without looking up."
            };
        }

        return new ReactionResult(tone, speaker, text, GetReactionDuration(tone));
    }

    private float GetReactionDuration(ReactionTone tone)
    {
        float duration = tone switch
        {
            ReactionTone.Positive => positiveReactionDuration,
            ReactionTone.Awkward => awkwardReactionDuration,
            ReactionTone.Concerned => concernedReactionDuration,
            _ => neutralReactionDuration
        };

        return Mathf.Clamp(duration, 1f, 1.75f);
    }

    private InterviewOutcomeType GetOutcome()
    {
        if (debugForceOutcome)
        {
            return debugForcedOutcome;
        }

        int total = score.Total;
        if (score.Technical >= 7 && score.Commercial >= 6 && score.Rapport >= 5 && total >= 27)
        {
            return InterviewOutcomeType.StrongPass;
        }

        if (total >= 22 && score.Technical >= 5 && score.Commercial >= 5)
        {
            return InterviewOutcomeType.Pass;
        }

        if (total >= 17)
        {
            return InterviewOutcomeType.Hold;
        }

        return InterviewOutcomeType.Reject;
    }

    private string BuildScorecardReadout()
    {
        if (score.Technical < 5)
        {
            return "Panel note: technical credibility did not survive follow-up pressure.";
        }

        if (score.Commercial < 5)
        {
            return "Panel note: the answers needed a clearer link to business risk and buying motion.";
        }

        if (score.Rapport < 4)
        {
            return "Panel note: credible, but a little difficult to put in front of an anxious customer.";
        }

        if (score.Energy < 4)
        {
            return "Panel note: the room read the delivery as controlled, but low-energy.";
        }

        return "Panel note: balanced signal. Nobody sounded delighted, which may be the closest thing to consensus.";
    }

    private void EnsureUiReady()
    {
        if (panelRoot == null)
        {
            BuildUi();
        }
    }

    private void BuildUi()
    {
        EnsureEventSystemExists();

        GameObject canvasObject = new GameObject("Cybersecurity Presales Interview Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 30;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        panelRoot = new GameObject("Cybersecurity Interview Flow", typeof(RectTransform));
        panelRoot.transform.SetParent(canvasObject.transform, false);
        Stretch(panelRoot.GetComponent<RectTransform>());

        questionPanel = CreatePanel("Question Panel", panelRoot.transform, new Color32(13, 16, 22, 236));
        RectTransform questionRect = questionPanel.GetComponent<RectTransform>();
        questionRect.anchorMin = new Vector2(0.08f, 0.07f);
        questionRect.anchorMax = new Vector2(0.92f, 0.46f);
        questionRect.offsetMin = Vector2.zero;
        questionRect.offsetMax = Vector2.zero;
        AddVerticalLayout(questionPanel, new RectOffset(30, 30, 24, 24), 12f);

        interviewerText = CreateText("Interviewer", questionPanel.transform, string.Empty, 22, FontStyles.Bold, TextAlignmentOptions.Left);
        interviewerText.color = new Color32(128, 218, 196, 255);
        questionText = CreateText("Question", questionPanel.transform, string.Empty, 28, FontStyles.Normal, TextAlignmentOptions.Left);
        questionText.color = new Color32(232, 238, 246, 255);
        ConfigureLayout(questionText.gameObject, -1f, 92f);

        answerButtons = new Button[4];
        answerButtonTexts = new TMP_Text[4];
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i] = CreateAnswerButton(questionPanel.transform, i);
            answerButtonTexts[i] = answerButtons[i].GetComponentInChildren<TMP_Text>();
        }

        reactionText = CreateText("Reaction", questionPanel.transform, string.Empty, 22, FontStyles.Italic, TextAlignmentOptions.Left);
        reactionText.color = new Color32(184, 194, 206, 255);
        ConfigureLayout(reactionText.gameObject, -1f, 36f);

        BuildTransitionPanel(panelRoot.transform);
        BuildOutcomePanel(panelRoot.transform);
        BuildDebugPanel(panelRoot.transform);
    }

    private void BuildTransitionPanel(Transform parent)
    {
        transitionPanel = CreatePanel("Outcome Transition Panel", parent, new Color32(5, 7, 10, 205));
        RectTransform transitionRect = transitionPanel.GetComponent<RectTransform>();
        transitionRect.anchorMin = new Vector2(0.25f, 0.38f);
        transitionRect.anchorMax = new Vector2(0.75f, 0.58f);
        transitionRect.offsetMin = Vector2.zero;
        transitionRect.offsetMax = Vector2.zero;
        AddVerticalLayout(transitionPanel, new RectOffset(28, 28, 24, 24), 8f);

        transitionText = CreateText("Transition Text", transitionPanel.transform, string.Empty, 30, FontStyles.Bold, TextAlignmentOptions.Center);
        transitionText.color = new Color32(222, 232, 238, 255);
        ConfigureLayout(transitionText.gameObject, -1f, 92f);
        transitionPanel.SetActive(false);
    }

    private void BuildDebugPanel(Transform parent)
    {
        debugPanel = CreatePanel("RC7 Debug Panel", parent, new Color32(8, 11, 16, 238));
        RectTransform debugRect = debugPanel.GetComponent<RectTransform>();
        debugRect.anchorMin = new Vector2(0.72f, 0.46f);
        debugRect.anchorMax = new Vector2(0.98f, 0.94f);
        debugRect.offsetMin = Vector2.zero;
        debugRect.offsetMax = Vector2.zero;
        AddVerticalLayout(debugPanel, new RectOffset(18, 18, 16, 16), 8f);

        TMP_Text titleText = CreateText("Debug Title", debugPanel.transform, "RC7 Debug Tools", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        titleText.color = new Color32(130, 220, 198, 255);
        ConfigureLayout(titleText.gameObject, -1f, 28f);

        debugStatusText = CreateText("Debug Status", debugPanel.transform, string.Empty, 17, FontStyles.Normal, TextAlignmentOptions.Left);
        debugStatusText.color = new Color32(218, 226, 236, 255);
        ConfigureLayout(debugStatusText.gameObject, -1f, 112f);

        CreateDebugButton("Toggle Deterministic Seed", debugPanel.transform, () => SetDeterministicSeedEnabled(!useDeterministicQuestionSeed));
        CreateDebugButton("Cycle Seed", debugPanel.transform, CycleSeed);
        CreateDebugButton("Force Strong Pass", debugPanel.transform, () => ForceOutcome(InterviewOutcomeType.StrongPass));
        CreateDebugButton("Force Pass", debugPanel.transform, () => ForceOutcome(InterviewOutcomeType.Pass));
        CreateDebugButton("Force Hold", debugPanel.transform, () => ForceOutcome(InterviewOutcomeType.Hold));
        CreateDebugButton("Force Reject", debugPanel.transform, () => ForceOutcome(InterviewOutcomeType.Reject));
        CreateDebugButton("Clear Forced Outcome", debugPanel.transform, ClearForcedOutcome);
        CreateDebugButton("Skip To Outcome", debugPanel.transform, SkipToOutcome);
        CreateDebugButton("Restart Current Run", debugPanel.transform, RestartRun);

        debugPanel.SetActive(false);
    }

    private void BuildOutcomePanel(Transform parent)
    {
        outcomePanel = CreatePanel("Laptop Email Client Panel", parent, new Color32(15, 20, 27, 248));
        RectTransform outcomeRect = outcomePanel.GetComponent<RectTransform>();
        outcomeRect.anchorMin = new Vector2(0.15f, 0.06f);
        outcomeRect.anchorMax = new Vector2(0.85f, 0.9f);
        outcomeRect.offsetMin = Vector2.zero;
        outcomeRect.offsetMax = Vector2.zero;
        AddVerticalLayout(outcomePanel, new RectOffset(30, 30, 24, 24), 10f);

        TMP_Text clientHeaderText = CreateText("Email Client Header", outcomePanel.transform, "Northbridge Mail - Inbox", 21, FontStyles.Bold, TextAlignmentOptions.Left);
        clientHeaderText.color = new Color32(128, 218, 196, 255);
        ConfigureLayout(clientHeaderText.gameObject, -1f, 30f);

        GameObject laptopScreen = CreatePanel("Laptop Screen Body", outcomePanel.transform, new Color32(235, 238, 233, 250));
        AddVerticalLayout(laptopScreen, new RectOffset(28, 28, 22, 22), 10f);
        ConfigureLayout(laptopScreen, -1f, 570f);

        outcomeFromText = CreateText("Email From", laptopScreen.transform, string.Empty, 20, FontStyles.Normal, TextAlignmentOptions.Left);
        outcomeFromText.color = new Color32(76, 84, 94, 255);
        ConfigureLayout(outcomeFromText.gameObject, -1f, 30f);

        outcomeTitleText = CreateText("Email Subject", laptopScreen.transform, string.Empty, 28, FontStyles.Bold, TextAlignmentOptions.Left);
        outcomeTitleText.color = new Color32(35, 40, 48, 255);
        ConfigureLayout(outcomeTitleText.gameObject, -1f, 38f);

        outcomeOpeningText = CreateText("Email Opening", laptopScreen.transform, string.Empty, 22, FontStyles.Normal, TextAlignmentOptions.Left);
        outcomeOpeningText.color = new Color32(45, 50, 58, 255);
        ConfigureLayout(outcomeOpeningText.gameObject, -1f, 66f);

        outcomeBodyText = CreateText("Email Outcome", laptopScreen.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.Left);
        outcomeBodyText.color = new Color32(45, 50, 58, 255);
        ConfigureLayout(outcomeBodyText.gameObject, -1f, 96f);

        outcomeFeedbackText = CreateText("Email Feedback", laptopScreen.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.Left);
        outcomeFeedbackText.color = new Color32(45, 50, 58, 255);
        ConfigureLayout(outcomeFeedbackText.gameObject, -1f, 92f);

        GameObject actionRow = new GameObject("Email Action Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        actionRow.transform.SetParent(laptopScreen.transform, false);
        ConfigureLayout(actionRow, -1f, 52f);
        HorizontalLayoutGroup actionLayout = actionRow.GetComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 12f;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = true;
        actionLayout.childForceExpandHeight = true;

        Button scorecardButton = CreateButton("Open Scorecard", actionRow.transform, new Color32(44, 58, 72, 255));
        TMP_Text buttonText = scorecardButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = "View Scorecard";
        scorecardButton.onClick.AddListener(ShowScorecard);
        ConfigureLayout(scorecardButton.gameObject, -1f, 48f);

        Button restartButton = CreateButton("Restart Run", actionRow.transform, new Color32(70, 76, 84, 255));
        TMP_Text restartButtonText = restartButton.GetComponentInChildren<TMP_Text>();
        restartButtonText.text = "Restart";
        restartButton.onClick.AddListener(RestartRun);
        ConfigureLayout(restartButton.gameObject, -1f, 48f);

        scorecardPanel = CreatePanel("Scorecard Panel", laptopScreen.transform, new Color32(220, 225, 222, 255));
        AddVerticalLayout(scorecardPanel, new RectOffset(22, 22, 18, 18), 8f);
        ConfigureLayout(scorecardPanel, -1f, 170f);
        scorecardText = CreateText("Scorecard Text", scorecardPanel.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.Left);
        scorecardText.color = new Color32(35, 40, 48, 255);
    }

    private static Button CreateAnswerButton(Transform parent, int index)
    {
        Button button = CreateButton($"Answer {index + 1}", parent, new Color32(33, 43, 56, 252));
        ConfigureLayout(button.gameObject, -1f, 56f);
        return button;
    }

    private static void CreateDebugButton(string label, Transform parent, UnityEngine.Events.UnityAction action)
    {
        Button button = CreateButton(label, parent, new Color32(35, 47, 62, 252));
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        text.text = label;
        text.fontSize = 16;
        button.onClick.AddListener(action);
        ConfigureLayout(button.gameObject, -1f, 34f);
    }

    private static Button CreateButton(string name, Transform parent, Color color)
    {
        GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);
        Image image = buttonObject.GetComponent<Image>();
        image.color = color;

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.08f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
        colors.selectedColor = colors.highlightedColor;
        button.colors = colors;

        TMP_Text text = CreateText("Label", buttonObject.transform, string.Empty, 21, FontStyles.Normal, TextAlignmentOptions.Left);
        text.color = new Color32(232, 238, 246, 255);
        RectTransform textRect = text.GetComponent<RectTransform>();
        Stretch(textRect);
        textRect.offsetMin = new Vector2(18f, 6f);
        textRect.offsetMax = new Vector2(-18f, -6f);
        return button;
    }

    private static GameObject CreatePanel(string name, Transform parent, Color color)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);
        panel.GetComponent<Image>().color = color;
        return panel;
    }

    private static TMP_Text CreateText(string name, Transform parent, string value, int size, FontStyles style, TextAlignmentOptions alignment)
    {
        GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(parent, false);
        TMP_Text text = textObject.GetComponent<TMP_Text>();
        text.text = value;
        text.fontSize = size;
        text.fontStyle = style;
        text.alignment = alignment;
        text.textWrappingMode = TextWrappingModes.Normal;
        return text;
    }

    private static void AddVerticalLayout(GameObject target, RectOffset padding, float spacing)
    {
        VerticalLayoutGroup layout = target.AddComponent<VerticalLayoutGroup>();
        layout.padding = padding;
        layout.spacing = spacing;
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
    }

    private static void ConfigureLayout(GameObject target, float preferredWidth, float preferredHeight)
    {
        LayoutElement layoutElement = target.GetComponent<LayoutElement>();
        if (layoutElement == null)
        {
            layoutElement = target.AddComponent<LayoutElement>();
        }

        if (preferredWidth >= 0f)
        {
            layoutElement.preferredWidth = preferredWidth;
        }

        if (preferredHeight >= 0f)
        {
            layoutElement.preferredHeight = preferredHeight;
        }
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }

    private static void EnsureEventSystemExists()
    {
        if (FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
    }

    private void ClearQuestionText()
    {
        if (interviewerText != null)
        {
            interviewerText.text = string.Empty;
        }

        if (questionText != null)
        {
            questionText.text = string.Empty;
        }

        if (reactionText != null)
        {
            reactionText.text = string.Empty;
        }

        if (answerButtons == null)
        {
            return;
        }

        for (int i = 0; i < answerButtons.Length; i++)
        {
            if (answerButtons[i] != null)
            {
                answerButtons[i].interactable = false;
                answerButtons[i].gameObject.SetActive(false);
            }
        }
    }

    private void ClearOutcomeText()
    {
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(false);
        }

        if (outcomePanel != null)
        {
            outcomePanel.SetActive(false);
        }

        if (scorecardPanel != null)
        {
            scorecardPanel.SetActive(false);
        }

        ClearText(outcomeFromText);
        ClearText(outcomeTitleText);
        ClearText(outcomeOpeningText);
        ClearText(outcomeBodyText);
        ClearText(outcomeFeedbackText);
        ClearText(scorecardText);
        ClearText(transitionText);
    }

    private static void ClearText(TMP_Text text)
    {
        if (text != null)
        {
            text.text = string.Empty;
        }
    }

    private void RefreshDebugStatus()
    {
        if (debugStatusText == null)
        {
            return;
        }

        string selectedQuestions = questions == null || questions.Length == 0
            ? "none"
            : $"{GetQuestionIndex(contextQuestionPool, questions[0]) + 1}/" +
              $"{GetQuestionIndex(technicalQuestionPool, questions.Length > 1 ? questions[1] : null) + 1}/" +
              $"{GetQuestionIndex(commercialQuestionPool, questions.Length > 2 ? questions[2] : null) + 1}";

        debugStatusText.text =
            "Prototype v1.0 RC7\n" +
            "Branch: prototype-v1.0-rc6-working\n" +
            $"Seed mode: {(useDeterministicQuestionSeed ? "deterministic" : "random")}\n" +
            $"Current seed: {deterministicQuestionSeed}\n" +
            $"Question indexes: {selectedQuestions}\n" +
            $"Forced outcome: {(debugForceOutcome ? debugForcedOutcome.ToString() : "off")}\n" +
            "Toggle: F1";
    }

    private static int GetQuestionIndex(InterviewQuestionData[] pool, InterviewQuestionData question)
    {
        if (pool == null || question == null)
        {
            return -1;
        }

        for (int i = 0; i < pool.Length; i++)
        {
            if (ReferenceEquals(pool[i], question))
            {
                return i;
            }
        }

        return -1;
    }

    private static bool WasDebugTogglePressed()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current != null && Keyboard.current.f1Key.wasPressedThisFrame;
#else
        return Input.GetKeyDown(KeyCode.F1);
#endif
    }

    private void BuildQuestions()
    {
        contextQuestionPool = new[]
        {
            new InterviewQuestionData(
                "Hiring Manager",
                "A customer says their board wants measurable cyber risk reduction this quarter, but the security team only wants to discuss tooling. How do you open discovery?",
                new[]
                {
                    new AnswerData("Start with the board metric, then ask what control failures or audit findings are driving urgency.", 2, 3, 2, 0),
                    new AnswerData("Ask for their current tooling list so you can map the fastest demo path.", 1, 0, 0, 1),
                    new AnswerData("Explain that cyber risk is hard to quantify and suggest a platform overview first.", 0, -1, -1, -1),
                    new AnswerData("Ask who owns the board narrative, then separate technical validation from executive proof.", 1, 2, 3, 0)
                }),
            new InterviewQuestionData(
                "Hiring Manager",
                "The champion starts the meeting by saying, 'We have had three vendors tell us the same thing.' What do you do first?",
                new[]
                {
                    new AnswerData("Ask what felt repetitive or unhelpful, then use that to narrow the conversation.", 1, 2, 3, 0),
                    new AnswerData("Acknowledge the fatigue and give a concise overview anyway so everyone has baseline context.", 1, 0, 1, 0),
                    new AnswerData("Move directly into a differentiated feature demo.", 1, 0, -1, 1),
                    new AnswerData("Ask who is most skeptical in the room and what would make the meeting worth their time.", 0, 2, 3, 0)
                }),
            new InterviewQuestionData(
                "Hiring Manager",
                "A CISO joins late, apologizes, and asks for the 'thirty-second version.' The technical team looks annoyed. How do you handle it?",
                new[]
                {
                    new AnswerData("Give the executive risk summary, then invite the technical team to validate the assumptions.", 1, 2, 3, 0),
                    new AnswerData("Restart from the architecture slide so the CISO has full context.", 1, -1, 0, -1),
                    new AnswerData("Ask the CISO which decision they are trying to make today before summarizing.", 0, 3, 2, 0),
                    new AnswerData("Keep going with the technical workshop and offer to brief the CISO later.", 1, 0, -1, 0)
                }),
            new InterviewQuestionData(
                "Hiring Manager",
                "The customer says their security team does not trust salespeople. The room goes quiet. What is your response?",
                new[]
                {
                    new AnswerData("Say that is fair, then define what evidence they should expect before trusting any claim.", 2, 1, 3, 0),
                    new AnswerData("Make a light comment and move to the customer logo slide.", 0, 1, 1, 1),
                    new AnswerData("Push back that your role is technical enough for the discussion.", 1, 0, -2, -1),
                    new AnswerData("Ask what previous vendors overclaimed, then set boundaries for what you can prove today.", 2, 2, 2, 0)
                })
        };

        technicalQuestionPool = new[]
        {
            new InterviewQuestionData(
                "Principal Security Architect",
                "During a technical workshop, the customer challenges your detection claims and asks how you reduce false positives without hiding real incidents. What do you do?",
                new[]
                {
                    new AnswerData("Describe the tuning model, then ask for sample alert categories so you can test the claim against their environment.", 3, 1, 1, 0),
                    new AnswerData("Say the product uses AI and shift quickly into the roadmap.", -1, 1, -1, 0),
                    new AnswerData("Acknowledge the risk, explain the validation path, and define what evidence would make them comfortable.", 3, 2, 2, 0),
                    new AnswerData("Offer to bring in engineering later and move back to the slide deck.", 0, 0, 0, -1)
                }),
            new InterviewQuestionData(
                "Principal Security Architect",
                "The customer asks how your platform handles encrypted traffic visibility without creating privacy or compliance issues. What is your answer?",
                new[]
                {
                    new AnswerData("Explain metadata, policy controls, and inspection boundaries, then ask about their regulated data constraints.", 3, 2, 1, 0),
                    new AnswerData("Say decryption is always recommended if they want real security.", 2, 0, -2, -1),
                    new AnswerData("Focus on executive risk reporting and avoid the privacy detail.", -1, 2, 0, 0),
                    new AnswerData("Separate what the product observes by default from what requires explicit customer policy decisions.", 3, 1, 2, 0)
                }),
            new InterviewQuestionData(
                "Principal Security Architect",
                "An architect says their SIEM already correlates identity, endpoint, and cloud telemetry. Where does your solution fit?",
                new[]
                {
                    new AnswerData("Ask where correlation still fails operationally, then position around coverage gaps and response workflow.", 3, 2, 2, 0),
                    new AnswerData("Argue that SIEMs are legacy and should be displaced.", 1, 1, -2, -1),
                    new AnswerData("Describe every integration available and let them decide what matters.", 2, -1, 0, -1),
                    new AnswerData("Position it as a board-level dashboard rather than a technical control.", -1, 2, 0, 0)
                }),
            new InterviewQuestionData(
                "Principal Security Architect",
                "A customer asks for proof that your attack path analysis is not just a prettier vulnerability scanner. What do you show?",
                new[]
                {
                    new AnswerData("Show how exploitability, identity privilege, exposure, and compensating controls change prioritization.", 3, 2, 1, 0),
                    new AnswerData("Show the UI and emphasize that executives understand it quickly.", 0, 2, 1, 1),
                    new AnswerData("Compare scanner feature matrices line by line.", 2, 0, 0, -1),
                    new AnswerData("Ask for a recent remediation debate and map how the model would have changed the decision.", 3, 2, 2, 0)
                })
        };

        commercialQuestionPool = new[]
        {
            new InterviewQuestionData(
                "Sales Director",
                "Procurement says the incumbent is cheaper and good enough. The champion is nervous. What is your next move?",
                new[]
                {
                    new AnswerData("Discount early to protect momentum, then ask legal to accelerate paper.", -1, 0, -1, -1),
                    new AnswerData("Rebuild the cost of inaction with the champion and arm them with a concise internal business case.", 1, 3, 2, 0),
                    new AnswerData("Challenge procurement directly and explain that cheaper security usually means hidden risk.", 1, 1, -2, -1),
                    new AnswerData("Ask what 'good enough' means operationally, then tie gaps to renewal risk, audit pressure, and incident response cost.", 2, 3, 2, 0)
                })
        };

        commercialQuestionPool = AppendCommercialQuestions(commercialQuestionPool);
        SelectQuestionsForRun();
    }

    private InterviewQuestionData[] AppendCommercialQuestions(InterviewQuestionData[] existing)
    {
        InterviewQuestionData[] expanded = new InterviewQuestionData[4];
        existing.CopyTo(expanded, 0);
        expanded[1] = new InterviewQuestionData(
            "Sales Director",
            "The CRO wants a close plan, but the security team says they need another month of testing. How do you avoid losing the deal or the trust?",
            new[]
            {
                new AnswerData("Split technical validation from commercial approval and agree what evidence must be produced by each date.", 2, 3, 2, 0),
                new AnswerData("Push for executive alignment and let the technical team continue testing in parallel.", 0, 3, 0, 1),
                new AnswerData("Tell the CRO the team is dragging their feet and needs pressure.", -1, 2, -2, -1),
                new AnswerData("Ask the technical team what unresolved risk blocks a recommendation, then convert that into the close plan.", 2, 2, 3, 0)
            });
        expanded[2] = new InterviewQuestionData(
            "Sales Director",
            "The CFO asks why this should be funded now instead of next fiscal year. The champion looks at you. What do you say?",
            new[]
            {
                new AnswerData("Tie delay to quantified exposure, audit deadlines, and the operational cost of current gaps.", 2, 3, 1, 0),
                new AnswerData("Explain that threat actors are moving quickly and waiting is dangerous.", 1, 1, 0, 0),
                new AnswerData("Offer phased scope that protects the highest-risk use case first.", 1, 3, 2, 0),
                new AnswerData("Say budget timing is a business decision and return to technical value.", 1, -1, -1, -1)
            });
        expanded[3] = new InterviewQuestionData(
            "Sales Director",
            "Legal flags data residency concerns late in the cycle. Sales wants you to say it is standard. What do you do?",
            new[]
            {
                new AnswerData("Clarify the actual residency requirement, state what is standard, and flag what needs formal review.", 2, 2, 3, 0),
                new AnswerData("Say legal reviews like this are common and should not block signature.", 0, 2, -1, 0),
                new AnswerData("Bring in security and legal owners, then protect the timeline with a specific decision path.", 1, 3, 2, 0),
                new AnswerData("Avoid answering until counsel joins the call.", 0, 0, 1, -1)
            });
        return expanded;
    }

    private void SelectQuestionsForRun()
    {
        questionRandom = useDeterministicQuestionSeed
            ? new System.Random(deterministicQuestionSeed)
            : new System.Random(System.Environment.TickCount);

        questions = new[]
        {
            PickQuestion(contextQuestionPool),
            PickQuestion(technicalQuestionPool),
            PickQuestion(commercialQuestionPool)
        };
        RefreshDebugStatus();
    }

    private InterviewQuestionData PickQuestion(InterviewQuestionData[] pool)
    {
        if (pool == null || pool.Length == 0)
        {
            Debug.LogError("Final Round RC6 question pool is empty.");
            return new InterviewQuestionData(
                "Hiring Manager",
                "The panel waits for a question that was not configured.",
                new[]
                {
                    new AnswerData("Acknowledge the setup issue and ask to proceed.", 0, 0, 1, 0),
                    new AnswerData("Try to improvise a product pitch.", 0, 0, -1, -1),
                    new AnswerData("Ask what signal they still need from the interview.", 0, 1, 1, 0),
                    new AnswerData("Say nothing for a moment.", -1, -1, -1, -1)
                });
        }

        return pool[questionRandom.Next(pool.Length)];
    }

    private sealed class InterviewQuestionData
    {
        public string InterviewerName { get; }
        public string QuestionText { get; }
        public AnswerData[] Answers { get; }

        public InterviewQuestionData(string interviewerName, string questionText, AnswerData[] answers)
        {
            InterviewerName = interviewerName;
            QuestionText = questionText;
            Answers = answers;
        }
    }

    private sealed class AnswerData
    {
        public string Text { get; }
        public int Technical { get; }
        public int Commercial { get; }
        public int Rapport { get; }
        public int Energy { get; }
        public int TotalDelta => Technical + Commercial + Rapport + Energy;

        public AnswerData(string text, int technical, int commercial, int rapport, int energy)
        {
            Text = text;
            Technical = technical;
            Commercial = commercial;
            Rapport = rapport;
            Energy = energy;
        }
    }

    private readonly struct ReactionResult
    {
        public ReactionTone Tone { get; }
        public ReactionSpeaker Speaker { get; }
        public string Text { get; }
        public float Duration { get; }

        public ReactionResult(ReactionTone tone, ReactionSpeaker speaker, string text, float duration)
        {
            Tone = tone;
            Speaker = speaker;
            Text = text;
            Duration = duration;
        }
    }

    private sealed class InterviewScore
    {
        public int Technical { get; private set; }
        public int Commercial { get; private set; }
        public int Rapport { get; private set; }
        public int Energy { get; private set; }
        public int Total => Technical + Commercial + Rapport + Energy;

        public void Reset()
        {
            Technical = 4;
            Commercial = 4;
            Rapport = 4;
            Energy = 6;
        }

        public void Apply(AnswerData answer)
        {
            Technical = Mathf.Clamp(Technical + answer.Technical, 0, 10);
            Commercial = Mathf.Clamp(Commercial + answer.Commercial, 0, 10);
            Rapport = Mathf.Clamp(Rapport + answer.Rapport, 0, 10);
            Energy = Mathf.Clamp(Energy + answer.Energy, 0, 10);
        }

        public OutcomeScoreSnapshot ToSnapshot()
        {
            return new OutcomeScoreSnapshot(Technical, Commercial, Rapport, Energy);
        }
    }
}
