using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
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
    private const string DeskSceneName = "DeskScene";

    private const string DefaultQuestionResourcePath = "FinalRound/Questions/RC11";

    private readonly InterviewScore score = new InterviewScore();
    private static readonly RoomUiTheme uiTheme = RoomUiTheme.Default;
    [SerializeField] private InterviewQuestionData[] questionBank;
    [SerializeField] private float stageIntroPauseDuration = 3.5f;
    [SerializeField] private float finalOutcomePauseDuration = 1.45f;
    [SerializeField] private float positiveReactionDuration = 2.35f;
    [SerializeField] private float neutralReactionDuration = 2.5f;
    [SerializeField] private float awkwardReactionDuration = 2.75f;
    [SerializeField] private float concernedReactionDuration = 2.9f;
    [SerializeField] private bool debugForceOutcome;
    [SerializeField] private InterviewOutcomeType debugForcedOutcome = InterviewOutcomeType.Pass;
    [SerializeField] private bool useDeterministicQuestionSeed;
    [SerializeField] private int deterministicQuestionSeed = 10603;
    [SerializeField] private float outcomeTransitionDelay = 1.15f;
    [SerializeField] private bool playtestModeEnabled = true;
    [Header("P21 Overlay Composition")]
    [SerializeField] private float stageIntroOverlayVerticalOffset = 120f;
    [SerializeField] private float stageIntroOverlayMaxWidth = 880f;
    [Range(0f, 1f)]
    [SerializeField] private float stageIntroOverlayOpacity = 0.62f;

    private RuntimeInterviewQuestion[] contextQuestionPool;
    private RuntimeInterviewQuestion[] technicalQuestionPool;
    private RuntimeInterviewQuestion[] commercialQuestionPool;
    private InterviewStageData[] interviewStages;
    private RuntimeInterviewQuestion[] questions;
    private InterviewStageData[] questionStages;
    private System.Random questionRandom;
    private TheRoomPrototypeController roomController;
    private InterviewGameManager gameManager;
    private int currentQuestionIndex;
    private int currentQuestionSeed;
    private bool answerLocked;
    private bool runSummaryLogged;
    private bool candidateStateAvailabilityLogged;
    private bool preserveForcedOutcomeOnRestart;
    private bool panelRootSuppressedByGameUi;
    private RoomModifierResult activeRoomModifiers;
    private bool hasActiveRoomModifiers;
    private readonly List<int> selectedAnswerIndexes = new List<int>();
    private readonly HashSet<int> shownStageIntroIndexes = new HashSet<int>();

    private Canvas canvas;
    private GameObject panelRoot;
    private GameObject questionPanel;
    private GameObject transitionPanel;
    private GameObject outcomePanel;
    private GameObject scorecardPanel;
    private GameObject debugPanel;
    private Button returnToDeskButton;
    private TMP_Text interviewerText;
    private TMP_Text stageMetaText;
    private TMP_Text interviewerNameText;
    private TMP_Text interviewerTitleText;
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
    private CanvasGroup[] answerButtonGroups;

    public bool IsDebugPanelVisible => debugPanel != null && debugPanel.activeSelf;

    private void Awake()
    {
        roomController = GetComponent<TheRoomPrototypeController>();
        gameManager = FindAnyObjectByType<InterviewGameManager>();
        if (playtestModeEnabled)
        {
            debugForceOutcome = false;
            preserveForcedOutcomeOnRestart = false;
        }

        BuildInterviewStages();
        BuildQuestions();
        BuildUi();
        LogCandidateStateAvailability();
        ResetFlow();
    }

    private void Update()
    {
        if (WasDebugTogglePressed())
        {
            ToggleDebugPanel();
        }

        SyncMainGameUiOverride();
    }

    public void BeginInterview()
    {
        EnsureUiReady();
        currentQuestionIndex = 0;
        answerLocked = false;
        runSummaryLogged = false;
        selectedAnswerIndexes.Clear();
        shownStageIntroIndexes.Clear();
        score.Reset();
        SelectQuestionsForRun();
        ResolveAndApplyRoomModifiers();
        if (roomController == null)
        {
            roomController = GetComponent<TheRoomPrototypeController>();
        }

        if (panelRoot == null || questionPanel == null || transitionPanel == null || outcomePanel == null || scorecardPanel == null)
        {
            Debug.LogWarning("Final Round RC15: interview UI could not be prepared. Check generated UI references.");
            return;
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
        runSummaryLogged = false;
        selectedAnswerIndexes.Clear();
        shownStageIntroIndexes.Clear();
        if (!preserveForcedOutcomeOnRestart)
        {
            debugForceOutcome = false;
        }

        score.Reset();
        activeRoomModifiers = default;
        hasActiveRoomModifiers = false;
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
            Debug.LogWarning("Final Round RC15: no selected questions were available. Re-selecting question bank.");
            SelectQuestionsForRun();
        }

        if (currentQuestionIndex >= questions.Length)
        {
            StartCoroutine(ShowOutcomeAfterTransition());
            return;
        }

        if (ShouldShowStageIntro(currentQuestionIndex))
        {
            StartCoroutine(ShowStageIntroThenQuestion(currentQuestionIndex));
            return;
        }

        RenderCurrentQuestion();
    }

    private void RenderCurrentQuestion()
    {
        RuntimeInterviewQuestion question = questions[currentQuestionIndex];
        if (question == null || question.Answers == null || question.Answers.Length != 4)
        {
            Debug.LogWarning($"Final Round RC15: question {currentQuestionIndex + 1} is missing or malformed. Skipping to outcome.");
            ShowOutcomeEmail();
            return;
        }

        InterviewStageData stage = GetStageForQuestion(currentQuestionIndex);
        string stageName = stage == null ? "Final Round" : stage.StageName;
        string interviewerTitle = GetInterviewerTitle(question.InterviewerName);
        SetText(interviewerText, $"{stageName} | Question {currentQuestionIndex + 1} of {questions.Length}\n{question.InterviewerName}");
        SetText(stageMetaText, $"{stageName.ToUpperInvariant()}  /  QUESTION {currentQuestionIndex + 1} OF {questions.Length}");
        SetText(interviewerNameText, question.InterviewerName);
        SetText(interviewerTitleText, interviewerTitle);
        questionText.text = question.QuestionText;
        reactionText.text = string.Empty;
        answerLocked = false;
        roomController?.ClearJudgementReaction();

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answerIndex = i;
            AnswerData answer = question.Answers[i];
            answerButtonTexts[i].text = FormatAnswerCardText(i, answer.Text);
            answerButtons[i].gameObject.SetActive(true);
            answerButtons[i].interactable = true;
            SetAnswerCardVisualState(i, true, false);
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => ChooseAnswer(answerIndex));
        }
    }

    private bool ShouldShowStageIntro(int questionIndex)
    {
        InterviewStageData stage = GetStageForQuestion(questionIndex);
        return stage != null && !shownStageIntroIndexes.Contains(stage.StageIndex);
    }

    private IEnumerator ShowStageIntroThenQuestion(int questionIndex)
    {
        InterviewStageData stage = GetStageForQuestion(questionIndex);
        if (stage == null)
        {
            RenderCurrentQuestion();
            yield break;
        }

        answerLocked = true;
        questionPanel.SetActive(false);
        ClearQuestionText();
        if (outcomePanel != null)
        {
            outcomePanel.SetActive(false);
        }

        if (transitionPanel != null && transitionText != null)
        {
            transitionPanel.SetActive(true);
            transitionText.text = GetStageIntroText(stage);
            yield return new WaitForSeconds(stageIntroPauseDuration);
            transitionPanel.SetActive(false);
        }

        shownStageIntroIndexes.Add(stage.StageIndex);
        questionPanel.SetActive(true);
        RenderCurrentQuestion();
    }

    private void ResolveAndApplyRoomModifiers()
    {
        if (!FinalRoundRunState.TryGetActiveState(out CandidateState state))
        {
            activeRoomModifiers = default;
            hasActiveRoomModifiers = false;
            return;
        }

        activeRoomModifiers = RoomModifierResolver.Resolve(state);
        hasActiveRoomModifiers = true;
        state.RoomModifierSummary = activeRoomModifiers.DebugSummary;
        score.ApplyStartingModifiers(
            activeRoomModifiers.TechnicalStartModifier,
            activeRoomModifiers.CommercialStartModifier,
            activeRoomModifiers.RapportStartModifier,
            activeRoomModifiers.EnergyStartModifier);
        Debug.Log("Final Round P30: Room modifiers resolved and applied.\n" + activeRoomModifiers.DebugSummary);
    }

    private string GetStageIntroText(InterviewStageData stage)
    {
        if (!hasActiveRoomModifiers || stage == null)
        {
            return stage == null ? string.Empty : stage.IntroText;
        }

        if (stage.StageIndex == 0)
        {
            return activeRoomModifiers.IntroTone switch
            {
                RoomIntroTone.Warm => "The Hiring Manager folds their hands. \"Maya's screen gave us a positive starting point. Let's build from there.\"",
                RoomIntroTone.LimitedSignal => "The Hiring Manager checks Maya's notes. \"We have limited signal from the screen, so we'll use this session to go deeper.\"",
                RoomIntroTone.DetailPressure => "The Hiring Manager looks up from the application notes. \"There were a few strong claims earlier. We'll test the detail through the scenarios.\"",
                RoomIntroTone.ClearMomentum => "The Hiring Manager folds their hands. \"Your screen and application notes came through clearly. Let's see how you handle the scenarios.\"",
                _ => "The Hiring Manager glances at Maya's screening notes. \"We'll use this session to understand how you work through customer scenarios.\""
            };
        }

        if (stage.StageIndex == 1 && activeRoomModifiers.ArchitectPressure == ArchitectPressureLevel.Sharper)
        {
            return "The Principal Security Architect leans forward. \"Your earlier positioning was strong. I want to test the technical detail behind it.\"";
        }

        return stage.IntroText;
    }

    private void ChooseAnswer(int answerIndex)
    {
        if (answerLocked || currentQuestionIndex >= questions.Length)
        {
            return;
        }

        answerLocked = true;
        RuntimeInterviewQuestion question = questions[currentQuestionIndex];
        if (question == null || question.Answers == null || answerIndex < 0 || answerIndex >= question.Answers.Length)
        {
            Debug.LogWarning("Final Round RC15: answer selection was invalid.");
            return;
        }

        AnswerData answer = question.Answers[answerIndex];
        selectedAnswerIndexes.Add(answerIndex + 1);
        score.Apply(answer);
        ReactionResult reaction = DetermineReaction(answer, currentQuestionIndex == questions.Length - 1);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;
            SetAnswerCardVisualState(i, false, i == answerIndex);
        }

        reactionText.text = $"<color=#{ColorUtility.ToHtmlStringRGB(uiTheme.MutedText)}>Observation</color>  <i>{reaction.Text}</i>";
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
        EnsureUiReady();
        if (questionPanel == null || outcomePanel == null || scorecardPanel == null)
        {
            Debug.LogWarning("Final Round RC15: outcome UI is missing required references.");
            return;
        }

        roomController?.SetRoomObjectiveText(string.Empty);
        if (transitionPanel != null)
        {
            transitionPanel.SetActive(false);
        }

        questionPanel.SetActive(false);
        outcomePanel.SetActive(true);
        scorecardPanel.SetActive(false);
        ClearQuestionText();

        InterviewOutcomeType outcome = GetOutcome();
        RecordRoomOutcome(outcome);
        OutcomeEmail email = OutcomeEmailGenerator.Generate(outcome, score.ToSnapshot());
        outcomeFromText.text =
            "<color=#647080>From</color>  recruitment@northbridge-cyber.example\n" +
            "<color=#647080>Time</color>  Today, 16:42";
        outcomeTitleText.text = email.SubjectLine;
        outcomeOpeningText.text = email.OpeningLine;
        outcomeBodyText.text = AddOutcomeContextLine(email.OutcomeParagraph);
        outcomeFeedbackText.text = $"<b>Feedback summary</b>\n{email.FeedbackParagraph}";
        RefreshReturnToDeskButton();
        LogRoomRunSummaryOnce(outcome);
        RefreshDebugStatus();
    }

    private string AddOutcomeContextLine(string outcomeParagraph)
    {
        if (!hasActiveRoomModifiers || string.IsNullOrWhiteSpace(activeRoomModifiers.OutcomeEmailContextLine))
        {
            return outcomeParagraph;
        }

        return $"{outcomeParagraph}\n\n{activeRoomModifiers.OutcomeEmailContextLine}";
    }

    private void ShowScorecard()
    {
        if (scorecardPanel == null || scorecardText == null)
        {
            Debug.LogWarning("Final Round RC15: scorecard UI is missing required references.");
            return;
        }

        scorecardPanel.SetActive(true);
        scorecardText.text =
            "<b>Scorecard</b>\n" +
            BuildScorecardCompactRow("Technical", score.Technical) + "    " +
            BuildScorecardCompactRow("Commercial", score.Commercial) + "\n" +
            BuildScorecardCompactRow("Rapport", score.Rapport) + "    " +
            BuildScorecardCompactRow("Energy", score.Energy) + "\n" +
            $"<color=#{ColorUtility.ToHtmlStringRGB(uiTheme.MutedText)}>{BuildScorecardReadout()}</color>";
    }

    private void RestartRun()
    {
        roomController?.ResetRun();
    }

    private void ReturnToDesk()
    {
        if (!FinalRoundRunState.TryGetActiveState(out CandidateState state))
        {
            Debug.LogWarning("Final Round P31: Return to Desk requested without active CandidateState.");
            RefreshReturnToDeskButton();
            return;
        }

        Debug.Log("Final Round P31: returning to Desk with completed CandidateState.\n" + state.BuildDebugSummary());
        SceneManager.LoadScene(DeskSceneName);
    }

    private void SkipToOutcome()
    {
        EnsureUiReady();
        StopAllCoroutines();
        if (panelRoot == null)
        {
            Debug.LogWarning("Final Round RC15: cannot skip to outcome because the interview UI root is missing.");
            return;
        }

        panelRoot.SetActive(true);
        roomController?.ClearJudgementReaction();
        ShowOutcomeEmail();
    }

    private IEnumerator ShowOutcomeAfterTransition()
    {
        roomController?.SetRoomObjectiveText(string.Empty);
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

        if (transitionPanel != null && transitionText != null)
        {
            yield return new WaitForSeconds(finalOutcomePauseDuration);
            transitionPanel.SetActive(true);
            transitionText.text = "Inbox: 1 new message";
            yield return new WaitForSeconds(outcomeTransitionDelay);
            transitionText.text = "Later that afternoon...";
            yield return new WaitForSeconds(0.55f);
        }

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

    private void UseSeedPreset(int seed)
    {
        deterministicQuestionSeed = Mathf.Abs(seed);
        useDeterministicQuestionSeed = true;
        SelectQuestionsForRun();
        RefreshDebugStatus();
    }

    private void ForceOutcome(InterviewOutcomeType outcome)
    {
        debugForceOutcome = true;
        debugForcedOutcome = outcome;
        preserveForcedOutcomeOnRestart = true;
        RefreshDebugStatus();
        if (outcomePanel != null && outcomePanel.activeSelf)
        {
            ShowOutcomeEmail();
        }
    }

    private void ClearForcedOutcome()
    {
        debugForceOutcome = false;
        preserveForcedOutcomeOnRestart = false;
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

        ApplyRoomModifierReactionBias(answer, ref tone, ref speaker, ref text);
        return new ReactionResult(tone, speaker, text, GetReactionDuration(tone));
    }

    private void ApplyRoomModifierReactionBias(AnswerData answer, ref ReactionTone tone, ref ReactionSpeaker speaker, ref string text)
    {
        if (!hasActiveRoomModifiers)
        {
            return;
        }

        if (activeRoomModifiers.ArchitectPressure == ArchitectPressureLevel.Sharper
            && answer.Technical <= 0
            && (tone == ReactionTone.Neutral || tone == ReactionTone.Awkward))
        {
            tone = ReactionTone.Concerned;
            speaker = ReactionSpeaker.SecurityArchitect;
            text = "The Architect writes a short note and waits for more detail.";
            return;
        }

        if (activeRoomModifiers.ReactionWarmthModifier > 0 && tone == ReactionTone.Neutral)
        {
            speaker = ReactionSpeaker.HiringManager;
            text = "The Hiring Manager nods once and lets the answer sit.";
            return;
        }

        if (activeRoomModifiers.ReactionWarmthModifier < 0 && tone == ReactionTone.Neutral)
        {
            speaker = ReactionSpeaker.Room;
            text = "The panel stay guarded, pens moving quietly.";
        }
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

        return Mathf.Clamp(duration, 1.5f, 3.25f);
    }

    private InterviewOutcomeType GetOutcome()
    {
        if (debugForceOutcome)
        {
            return debugForcedOutcome;
        }

        int total = score.Total;
        if (score.Technical >= 8
            && score.Commercial >= 8
            && score.Rapport >= 7
            && score.Energy >= 6
            && score.Technical >= 6
            && score.Commercial >= 6
            && score.Rapport >= 6
            && score.Energy >= 6
            && total >= 32)
        {
            return InterviewOutcomeType.StrongPass;
        }

        if (total >= 23 && score.Technical >= 5 && score.Commercial >= 5 && score.Energy >= 4)
        {
            return InterviewOutcomeType.Pass;
        }

        if (score.Rapport <= 1 && score.Energy <= 2)
        {
            return InterviewOutcomeType.Reject;
        }

        if (total >= 21)
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

    private static string BuildScorecardRow(string label, int value)
    {
        string bar = BuildScoreBar(value);
        string valueColor = value >= 7
            ? ColorUtility.ToHtmlStringRGB(uiTheme.Positive)
            : value <= 3
                ? ColorUtility.ToHtmlStringRGB(uiTheme.Warning)
                : ColorUtility.ToHtmlStringRGB(uiTheme.EmailMutedText);

        return $"<b>{label,-11}</b> <color=#{valueColor}>{value}/10</color>  <color=#{ColorUtility.ToHtmlStringRGB(uiTheme.Accent)}>{bar}</color>";
    }

    private static string BuildScoreBar(int value)
    {
        int filled = Mathf.Clamp(value, 0, 10);
        return new string('#', filled) + $"<color=#{ColorUtility.ToHtmlStringRGB(uiTheme.EmailLine)}>{new string('-', 10 - filled)}</color>";
    }

    private static string BuildScorecardCompactRow(string label, int value)
    {
        string valueColor = value >= 7
            ? ColorUtility.ToHtmlStringRGB(uiTheme.Positive)
            : value <= 3
                ? ColorUtility.ToHtmlStringRGB(uiTheme.Warning)
                : ColorUtility.ToHtmlStringRGB(uiTheme.EmailMutedText);

        return $"<b>{label}</b> <color=#{valueColor}>{value}/10</color>";
    }

    private static string FormatAnswerCardText(int index, string answerText)
    {
        char label = (char)('A' + Mathf.Clamp(index, 0, 25));
        return $"<color=#{ColorUtility.ToHtmlStringRGB(uiTheme.Accent)}><b>{label}</b></color>  {answerText}";
    }

    private void SetAnswerCardVisualState(int index, bool active, bool selected)
    {
        if (answerButtons == null || index < 0 || index >= answerButtons.Length || answerButtons[index] == null)
        {
            return;
        }

        Image image = answerButtons[index].GetComponent<Image>();
        if (image != null)
        {
            image.color = selected
                ? Color.Lerp(uiTheme.AnswerCard, uiTheme.Accent, 0.24f)
                : uiTheme.AnswerCard;
        }

        if (answerButtonGroups != null && index < answerButtonGroups.Length && answerButtonGroups[index] != null)
        {
            answerButtonGroups[index].alpha = active || selected ? 1f : 0.46f;
            answerButtonGroups[index].blocksRaycasts = active;
            answerButtonGroups[index].interactable = active;
        }

        if (answerButtonTexts != null && index < answerButtonTexts.Length && answerButtonTexts[index] != null)
        {
            answerButtonTexts[index].color = active || selected ? uiTheme.PrimaryText : uiTheme.MutedText;
        }
    }

    private static string GetInterviewerTitle(string interviewerName)
    {
        if (interviewerName == "Principal Security Architect")
        {
            return "Security architecture and technical validation";
        }

        if (interviewerName == "Sales Director")
        {
            return "Commercial alignment and deal discipline";
        }

        return "Hiring signal and customer empathy";
    }

    private static void SetText(TMP_Text text, string value)
    {
        if (text != null)
        {
            text.text = value;
        }
    }

    private void EnsureUiReady()
    {
        if (panelRoot == null)
        {
            BuildUi();
        }
    }

    private void SyncMainGameUiOverride()
    {
        if (panelRoot == null)
        {
            return;
        }

        if (gameManager == null)
        {
            gameManager = FindAnyObjectByType<InterviewGameManager>();
        }

        bool shouldSuppress = gameManager != null && gameManager.IsRoomUiFocusActive();
        if (shouldSuppress == panelRootSuppressedByGameUi)
        {
            return;
        }

        CanvasGroup canvasGroup = panelRoot.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = panelRoot.AddComponent<CanvasGroup>();
        }

        panelRootSuppressedByGameUi = shouldSuppress;
        canvasGroup.alpha = shouldSuppress ? 0f : 1f;
        canvasGroup.blocksRaycasts = !shouldSuppress;
        canvasGroup.interactable = !shouldSuppress;
    }

    private void BuildUi()
    {
        EnsureEventSystemExists();

        GameObject canvasObject = new GameObject("Cybersecurity Presales Interview Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 8;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;

        panelRoot = new GameObject("Cybersecurity Interview Flow", typeof(RectTransform));
        panelRoot.transform.SetParent(canvasObject.transform, false);
        Stretch(panelRoot.GetComponent<RectTransform>());

        questionPanel = CreatePanel("Question Panel", panelRoot.transform, uiTheme.Surface);
        RectTransform questionRect = questionPanel.GetComponent<RectTransform>();
        questionRect.anchorMin = new Vector2(0.09f, 0.05f);
        questionRect.anchorMax = new Vector2(0.91f, 0.43f);
        questionRect.offsetMin = Vector2.zero;
        questionRect.offsetMax = Vector2.zero;
        AddVerticalLayout(questionPanel, new RectOffset(28, 28, 18, 18), 7f);

        interviewerText = CreateText("Legacy Interviewer", questionPanel.transform, string.Empty, 1, FontStyles.Normal, TextAlignmentOptions.Left);
        interviewerText.gameObject.SetActive(false);

        stageMetaText = CreateText("Stage Progress", questionPanel.transform, string.Empty, 13, FontStyles.Bold, TextAlignmentOptions.Left);
        stageMetaText.color = uiTheme.MutedText;
        ConfigureLayout(stageMetaText.gameObject, -1f, 18f);

        interviewerNameText = CreateText("Interviewer Name", questionPanel.transform, string.Empty, 21, FontStyles.Bold, TextAlignmentOptions.Left);
        interviewerNameText.color = uiTheme.Accent;
        ConfigureLayout(interviewerNameText.gameObject, -1f, 24f);

        interviewerTitleText = CreateText("Interviewer Title", questionPanel.transform, string.Empty, 14, FontStyles.Normal, TextAlignmentOptions.Left);
        interviewerTitleText.color = uiTheme.MutedText;
        ConfigureLayout(interviewerTitleText.gameObject, -1f, 18f);

        questionText = CreateText("Question", questionPanel.transform, string.Empty, 22, FontStyles.Normal, TextAlignmentOptions.Left);
        questionText.color = uiTheme.PrimaryText;
        questionText.lineSpacing = 4f;
        ConfigureLayout(questionText.gameObject, -1f, 72f);

        answerButtons = new Button[4];
        answerButtonTexts = new TMP_Text[4];
        answerButtonGroups = new CanvasGroup[4];
        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i] = CreateAnswerButton(questionPanel.transform, i);
            answerButtonTexts[i] = answerButtons[i].GetComponentInChildren<TMP_Text>();
            answerButtonGroups[i] = answerButtons[i].GetComponent<CanvasGroup>();
        }

        reactionText = CreateText("Reaction", questionPanel.transform, string.Empty, 15, FontStyles.Italic, TextAlignmentOptions.Left);
        reactionText.color = uiTheme.SubtleText;
        ConfigureLayout(reactionText.gameObject, -1f, 24f);

        BuildTransitionPanel(panelRoot.transform);
        BuildOutcomePanel(panelRoot.transform);
        BuildDebugPanel(panelRoot.transform);
    }

    private void BuildTransitionPanel(Transform parent)
    {
        Color transitionColor = uiTheme.ModalOverlay;
        transitionColor.a = Mathf.Clamp01(stageIntroOverlayOpacity);
        transitionPanel = CreatePanel("Outcome Transition Panel", parent, transitionColor);
        RectTransform transitionRect = transitionPanel.GetComponent<RectTransform>();
        transitionRect.anchorMin = new Vector2(0.5f, 0f);
        transitionRect.anchorMax = new Vector2(0.5f, 0f);
        transitionRect.pivot = new Vector2(0.5f, 0f);
        transitionRect.anchoredPosition = new Vector2(0f, stageIntroOverlayVerticalOffset);
        transitionRect.sizeDelta = new Vector2(Mathf.Max(420f, stageIntroOverlayMaxWidth), 96f);
        AddVerticalLayout(transitionPanel, new RectOffset(24, 24, 16, 16), 6f);

        transitionText = CreateText("Transition Text", transitionPanel.transform, string.Empty, 24, FontStyles.Bold, TextAlignmentOptions.Center);
        transitionText.color = uiTheme.PrimaryText;
        ConfigureLayout(transitionText.gameObject, -1f, 64f);
        transitionPanel.SetActive(false);
    }

    private void BuildDebugPanel(Transform parent)
    {
        debugPanel = CreatePanel("RC17 Debug Panel", parent, new Color32(8, 11, 16, 238));
        RectTransform debugRect = debugPanel.GetComponent<RectTransform>();
        debugRect.anchorMin = new Vector2(0.72f, 0.24f);
        debugRect.anchorMax = new Vector2(0.98f, 0.94f);
        debugRect.offsetMin = Vector2.zero;
        debugRect.offsetMax = Vector2.zero;
        AddVerticalLayout(debugPanel, new RectOffset(18, 18, 16, 16), 8f);

        TMP_Text titleText = CreateText("Debug Title", debugPanel.transform, "RC17 Debug Tools", 22, FontStyles.Bold, TextAlignmentOptions.Left);
        titleText.color = uiTheme.Accent;
        ConfigureLayout(titleText.gameObject, -1f, 28f);

        debugStatusText = CreateText("Debug Status", debugPanel.transform, string.Empty, 17, FontStyles.Normal, TextAlignmentOptions.Left);
        debugStatusText.color = new Color32(218, 226, 236, 255);
        ConfigureLayout(debugStatusText.gameObject, -1f, 132f);

        CreateDebugButton("Toggle Deterministic Seed", debugPanel.transform, () => SetDeterministicSeedEnabled(!useDeterministicQuestionSeed));
        CreateDebugButton("Cycle Seed", debugPanel.transform, CycleSeed);
        CreateDebugButton("Preset Seed 1", debugPanel.transform, () => UseSeedPreset(1));
        CreateDebugButton("Preset Seed 2", debugPanel.transform, () => UseSeedPreset(2));
        CreateDebugButton("Preset Seed 3", debugPanel.transform, () => UseSeedPreset(3));
        CreateDebugButton("Preset Seed 4", debugPanel.transform, () => UseSeedPreset(4));
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
        outcomePanel = CreatePanel("Laptop Email Client Panel", parent, uiTheme.Surface);
        RectTransform outcomeRect = outcomePanel.GetComponent<RectTransform>();
        outcomeRect.anchorMin = new Vector2(0.2f, 0.14f);
        outcomeRect.anchorMax = new Vector2(0.8f, 0.84f);
        outcomeRect.offsetMin = Vector2.zero;
        outcomeRect.offsetMax = Vector2.zero;
        AddVerticalLayout(outcomePanel, new RectOffset(28, 28, 18, 18), 8f);

        TMP_Text clientHeaderText = CreateText("Email Client Header", outcomePanel.transform, "NORTHBRIDGE MAIL  /  INBOX", 15, FontStyles.Bold, TextAlignmentOptions.Left);
        clientHeaderText.color = uiTheme.MutedText;
        ConfigureLayout(clientHeaderText.gameObject, -1f, 22f);

        GameObject laptopScreen = CreatePanel("Email Message Body", outcomePanel.transform, uiTheme.EmailBody);
        AddVerticalLayout(laptopScreen, new RectOffset(32, 32, 22, 22), 7f);
        ConfigureLayout(laptopScreen, -1f, 492f);

        outcomeFromText = CreateText("Email From", laptopScreen.transform, string.Empty, 14, FontStyles.Normal, TextAlignmentOptions.Left);
        outcomeFromText.color = uiTheme.EmailMutedText;
        outcomeFromText.lineSpacing = 2f;
        ConfigureLayout(outcomeFromText.gameObject, -1f, 32f);

        outcomeTitleText = CreateText("Email Subject", laptopScreen.transform, string.Empty, 23, FontStyles.Bold, TextAlignmentOptions.Left);
        outcomeTitleText.color = uiTheme.EmailText;
        ConfigureLayout(outcomeTitleText.gameObject, -1f, 30f);

        outcomeOpeningText = CreateText("Email Opening", laptopScreen.transform, string.Empty, 16, FontStyles.Normal, TextAlignmentOptions.Left);
        outcomeOpeningText.color = uiTheme.EmailText;
        ConfigureLayout(outcomeOpeningText.gameObject, -1f, 40f);

        outcomeBodyText = CreateText("Email Outcome", laptopScreen.transform, string.Empty, 16, FontStyles.Normal, TextAlignmentOptions.Left);
        outcomeBodyText.color = uiTheme.EmailText;
        outcomeBodyText.lineSpacing = 2f;
        ConfigureLayout(outcomeBodyText.gameObject, -1f, 96f);

        outcomeFeedbackText = CreateText("Email Feedback", laptopScreen.transform, string.Empty, 16, FontStyles.Normal, TextAlignmentOptions.Left);
        outcomeFeedbackText.color = uiTheme.EmailText;
        outcomeFeedbackText.lineSpacing = 2f;
        ConfigureLayout(outcomeFeedbackText.gameObject, -1f, 82f);

        GameObject actionSpacer = new GameObject("Email Action Spacer", typeof(RectTransform), typeof(LayoutElement));
        actionSpacer.transform.SetParent(laptopScreen.transform, false);
        ConfigureLayout(actionSpacer, -1f, 12f);

        GameObject actionRow = new GameObject("Email Action Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        actionRow.transform.SetParent(laptopScreen.transform, false);
        ConfigureLayout(actionRow, -1f, 38f);
        HorizontalLayoutGroup actionLayout = actionRow.GetComponent<HorizontalLayoutGroup>();
        actionLayout.spacing = 10f;
        actionLayout.childControlWidth = true;
        actionLayout.childControlHeight = true;
        actionLayout.childForceExpandWidth = false;
        actionLayout.childForceExpandHeight = false;

        Button scorecardButton = CreateButton("Open Scorecard", actionRow.transform, uiTheme.ActionButton);
        TMP_Text buttonText = scorecardButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = "View Scorecard";
        buttonText.fontSize = 15;
        scorecardButton.onClick.AddListener(ShowScorecard);
        ConfigureLayout(scorecardButton.gameObject, 178f, 34f);

        Button restartButton = CreateButton("Restart Run", actionRow.transform, uiTheme.SecondaryButton);
        TMP_Text restartButtonText = restartButton.GetComponentInChildren<TMP_Text>();
        restartButtonText.text = "Restart";
        restartButtonText.fontSize = 15;
        restartButton.onClick.AddListener(RestartRun);
        ConfigureLayout(restartButton.gameObject, 124f, 34f);

        returnToDeskButton = CreateButton("Return To Desk", actionRow.transform, uiTheme.ActionButton);
        TMP_Text returnButtonText = returnToDeskButton.GetComponentInChildren<TMP_Text>();
        returnButtonText.text = "Return to Desk";
        returnButtonText.fontSize = 15;
        returnToDeskButton.onClick.AddListener(ReturnToDesk);
        ConfigureLayout(returnToDeskButton.gameObject, 158f, 34f);
        RefreshReturnToDeskButton();

        scorecardPanel = CreatePanel("Scorecard Panel", laptopScreen.transform, uiTheme.EmailInset);
        AddVerticalLayout(scorecardPanel, new RectOffset(18, 18, 12, 12), 4f);
        ConfigureLayout(scorecardPanel, -1f, 130f);
        scorecardText = CreateText("Scorecard Text", scorecardPanel.transform, string.Empty, 14, FontStyles.Normal, TextAlignmentOptions.TopLeft);
        scorecardText.color = uiTheme.EmailText;
        scorecardText.lineSpacing = 2f;
    }

    private static Button CreateAnswerButton(Transform parent, int index)
    {
        Button button = CreateButton($"Answer {index + 1}", parent, uiTheme.AnswerCard);
        button.gameObject.AddComponent<CanvasGroup>();
        ConfigureLayout(button.gameObject, -1f, 48f);
        TMP_Text text = button.GetComponentInChildren<TMP_Text>();
        text.fontSize = 16;
        text.lineSpacing = 1.5f;
        text.alignment = TextAlignmentOptions.MidlineLeft;
        RectTransform textRect = text.GetComponent<RectTransform>();
        textRect.offsetMin = new Vector2(18f, 5f);
        textRect.offsetMax = new Vector2(-18f, -5f);
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
        buttonObject.AddComponent<ButtonJuice>();

        Button button = buttonObject.GetComponent<Button>();
        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, uiTheme.Accent, 0.18f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.18f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = uiTheme.Disabled;
        colors.fadeDuration = 0.09f;
        button.colors = colors;

        TMP_Text text = CreateText("Label", buttonObject.transform, string.Empty, 21, FontStyles.Normal, TextAlignmentOptions.Left);
        text.color = uiTheme.PrimaryText;
        RectTransform textRect = text.GetComponent<RectTransform>();
        Stretch(textRect);
        textRect.offsetMin = new Vector2(uiTheme.CardPadding, 8f);
        textRect.offsetMax = new Vector2(-uiTheme.CardPadding, -8f);
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

        ClearText(stageMetaText);
        ClearText(interviewerNameText);
        ClearText(interviewerTitleText);

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

        string selectedQuestions = GetSelectedQuestionIdSummary();

        debugStatusText.text =
            "Final Round - Prototype P31\n" +
            "Branch: main\n" +
            $"Playtest mode: {(playtestModeEnabled ? "on" : "off")}\n" +
            $"Seed mode: {(useDeterministicQuestionSeed ? "deterministic" : "random")}\n" +
            $"Current seed: {currentQuestionSeed}\n" +
            $"Question IDs: {selectedQuestions}\n" +
            $"CandidateState: {GetCandidateStateDebugLine()}\n" +
            $"Room modifiers: {GetRoomModifierDebugLine()}\n" +
            $"Forced outcome: {(debugForceOutcome ? debugForcedOutcome.ToString() : "off")}\n" +
            "Toggle: F1";
    }

    private void LogCandidateStateAvailability()
    {
        if (candidateStateAvailabilityLogged)
        {
            return;
        }

        candidateStateAvailabilityLogged = true;
        if (FinalRoundRunState.TryGetActiveState(out CandidateState state))
        {
            Debug.Log("Final Round RunState: Room detected active CandidateState.\n" + state.BuildDebugSummary());
            return;
        }

        Debug.Log("Final Round RunState: Room started without active CandidateState; using neutral VS1 direct-start behavior.");
    }

    private void RecordRoomOutcome(InterviewOutcomeType outcome)
    {
        if (!FinalRoundRunState.TryGetActiveState(out CandidateState state))
        {
            return;
        }

        state.RoomOutcome = outcome.ToString();
    }

    private static string GetCandidateStateDebugLine()
    {
        return FinalRoundRunState.TryGetActiveState(out CandidateState state)
            ? $"active desk run, job {FormatDebugId(state.SelectedJobId)}"
            : "neutral direct-start fallback";
    }

    private void RefreshReturnToDeskButton()
    {
        if (returnToDeskButton == null)
        {
            return;
        }

        bool hasCompletedDeskRun = FinalRoundRunState.TryGetActiveState(out CandidateState state)
            && state.HasActiveDeskRun
            && !string.IsNullOrWhiteSpace(state.RoomOutcome);
        returnToDeskButton.gameObject.SetActive(hasCompletedDeskRun);
        returnToDeskButton.interactable = hasCompletedDeskRun;
    }

    private string GetRoomModifierDebugLine()
    {
        if (!hasActiveRoomModifiers)
        {
            return "neutral";
        }

        return activeRoomModifiers.DebugSummary.Replace("\n", " | ");
    }

    private static string FormatDebugId(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? "none" : value;
    }

    private string GetSelectedQuestionIdSummary()
    {
        if (questions == null || questions.Length == 0)
        {
            return "none";
        }

        List<string> questionIds = new List<string>();
        for (int i = 0; i < questions.Length; i++)
        {
            questionIds.Add(questions[i] == null ? $"Q{i + 1:00}-MISSING" : questions[i].QuestionId);
        }

        return string.Join(", ", questionIds);
    }

    private static int GetQuestionIndex(RuntimeInterviewQuestion[] pool, RuntimeInterviewQuestion question)
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

    private readonly struct RoomUiTheme
    {
        public Color Surface { get; }
        public Color ModalOverlay { get; }
        public Color AnswerCard { get; }
        public Color ActionButton { get; }
        public Color SecondaryButton { get; }
        public Color Disabled { get; }
        public Color Accent { get; }
        public Color PrimaryText { get; }
        public Color MutedText { get; }
        public Color SubtleText { get; }
        public Color Positive { get; }
        public Color Warning { get; }
        public Color EmailBody { get; }
        public Color EmailInset { get; }
        public Color EmailText { get; }
        public Color EmailMutedText { get; }
        public Color EmailLine { get; }
        public int PanelPadding { get; }
        public int CardPadding { get; }

        private RoomUiTheme(
            Color surface,
            Color modalOverlay,
            Color answerCard,
            Color actionButton,
            Color secondaryButton,
            Color disabled,
            Color accent,
            Color primaryText,
            Color mutedText,
            Color subtleText,
            Color positive,
            Color warning,
            Color emailBody,
            Color emailInset,
            Color emailText,
            Color emailMutedText,
            Color emailLine,
            int panelPadding,
            int cardPadding)
        {
            Surface = surface;
            ModalOverlay = modalOverlay;
            AnswerCard = answerCard;
            ActionButton = actionButton;
            SecondaryButton = secondaryButton;
            Disabled = disabled;
            Accent = accent;
            PrimaryText = primaryText;
            MutedText = mutedText;
            SubtleText = subtleText;
            Positive = positive;
            Warning = warning;
            EmailBody = emailBody;
            EmailInset = emailInset;
            EmailText = emailText;
            EmailMutedText = emailMutedText;
            EmailLine = emailLine;
            PanelPadding = panelPadding;
            CardPadding = cardPadding;
        }

        public static RoomUiTheme Default => new RoomUiTheme(
            new Color32(10, 14, 20, 214),
            new Color32(5, 7, 10, 214),
            new Color32(24, 32, 43, 226),
            new Color32(38, 76, 82, 255),
            new Color32(58, 65, 76, 255),
            new Color32(24, 29, 38, 172),
            new Color32(106, 206, 184, 255),
            new Color32(235, 241, 247, 255),
            new Color32(151, 164, 181, 255),
            new Color32(184, 195, 207, 255),
            new Color32(112, 201, 145, 255),
            new Color32(202, 147, 91, 255),
            new Color32(226, 231, 229, 246),
            new Color32(205, 214, 214, 246),
            new Color32(35, 42, 50, 255),
            new Color32(100, 112, 128, 255),
            new Color32(190, 198, 204, 255),
            34,
            22);
    }

    private void BuildQuestions()
    {
        if (TryBuildQuestionsFromAssets())
        {
            SelectQuestionsForRun();
            return;
        }

        Debug.LogWarning("Final Round RC11: no valid ScriptableObject question bank found. Using built-in sample questions.");
        contextQuestionPool = new[]
        {
            new RuntimeInterviewQuestion(
                "CTX-01",
                "Hiring Manager",
                "A customer says their board wants measurable cyber risk reduction this quarter, but the security team only wants to discuss tooling. How do you open discovery?",
                new[]
                {
                    new AnswerData("Start with the board metric, then ask what control failures or audit findings are driving urgency.", 2, 3, 2, 0),
                    new AnswerData("Ask for their current tooling list so you can map the fastest demo path.", 1, 0, 0, 1),
                    new AnswerData("Explain that cyber risk is hard to quantify and suggest a platform overview first.", 0, -1, -1, -1),
                    new AnswerData("Ask who owns the board narrative, then separate technical validation from executive proof.", 1, 2, 3, 0)
                }),
            new RuntimeInterviewQuestion(
                "CTX-02",
                "Hiring Manager",
                "The champion starts the meeting by saying, 'We have had three vendors tell us the same thing.' What do you do first?",
                new[]
                {
                    new AnswerData("Ask what felt repetitive or unhelpful, then use that to narrow the conversation.", 1, 2, 3, 0),
                    new AnswerData("Acknowledge the fatigue and give a concise overview anyway so everyone has baseline context.", 1, 0, 1, 0),
                    new AnswerData("Move directly into a differentiated feature demo.", 1, 0, -1, 1),
                    new AnswerData("Ask who is most skeptical in the room and what would make the meeting worth their time.", 0, 2, 3, 0)
                }),
            new RuntimeInterviewQuestion(
                "CTX-03",
                "Hiring Manager",
                "A CISO joins late, apologizes, and asks for the 'thirty-second version.' The technical team looks annoyed. How do you handle it?",
                new[]
                {
                    new AnswerData("Give the executive risk summary, then invite the technical team to validate the assumptions.", 1, 2, 3, 0),
                    new AnswerData("Restart from the architecture slide so the CISO has full context.", 1, -1, 0, -1),
                    new AnswerData("Ask the CISO which decision they are trying to make today before summarizing.", 0, 3, 2, 0),
                    new AnswerData("Keep going with the technical workshop and offer to brief the CISO later.", 1, 0, -1, 0)
                }),
            new RuntimeInterviewQuestion(
                "CTX-04",
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
            new RuntimeInterviewQuestion(
                "TECH-01",
                "Principal Security Architect",
                "During a technical workshop, the customer challenges your detection claims and asks how you reduce false positives without hiding real incidents. What do you do?",
                new[]
                {
                    new AnswerData("Describe the tuning model, then ask for sample alert categories so you can test the claim against their environment.", 3, 1, 1, 0),
                    new AnswerData("Say the product uses AI and shift quickly into the roadmap.", -1, 1, -1, 0),
                    new AnswerData("Acknowledge the risk, explain the validation path, and define what evidence would make them comfortable.", 3, 2, 2, 0),
                    new AnswerData("Offer to bring in engineering later and move back to the slide deck.", 0, 0, 0, -1)
                }),
            new RuntimeInterviewQuestion(
                "TECH-02",
                "Principal Security Architect",
                "The customer asks how your platform handles encrypted traffic visibility without creating privacy or compliance issues. What is your answer?",
                new[]
                {
                    new AnswerData("Explain metadata, policy controls, and inspection boundaries, then ask about their regulated data constraints.", 3, 2, 1, 0),
                    new AnswerData("Say decryption is always recommended if they want real security.", 2, 0, -2, -1),
                    new AnswerData("Focus on executive risk reporting and avoid the privacy detail.", -1, 2, 0, 0),
                    new AnswerData("Separate what the product observes by default from what requires explicit customer policy decisions.", 3, 1, 2, 0)
                }),
            new RuntimeInterviewQuestion(
                "TECH-03",
                "Principal Security Architect",
                "An architect says their SIEM already correlates identity, endpoint, and cloud telemetry. Where does your solution fit?",
                new[]
                {
                    new AnswerData("Ask where correlation still fails operationally, then position around coverage gaps and response workflow.", 3, 2, 2, 0),
                    new AnswerData("Argue that SIEMs are legacy and should be displaced.", 1, 1, -2, -1),
                    new AnswerData("Describe every integration available and let them decide what matters.", 2, -1, 0, -1),
                    new AnswerData("Position it as a board-level dashboard rather than a technical control.", -1, 2, 0, 0)
                }),
            new RuntimeInterviewQuestion(
                "TECH-04",
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
            new RuntimeInterviewQuestion(
                "COMM-01",
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

    private void BuildInterviewStages()
    {
        interviewStages = new[]
        {
            new InterviewStageData(
                0,
                "Stage 1: Customer Context",
                "Hiring Manager",
                "The Hiring Manager folds their hands. \"Let's start with the customer situation.\"",
                QuestionCategory.ContextCustomerScenario,
                2),
            new InterviewStageData(
                1,
                "Stage 2: Technical Judgement",
                "Principal Security Architect",
                "The Principal Security Architect leans forward. \"I want to go a level deeper technically.\"",
                QuestionCategory.TechnicalSecurityJudgement,
                2),
            new InterviewStageData(
                2,
                "Stage 3: Commercial Pressure",
                "Sales Director",
                "The Sales Director checks their notes. \"Let's talk about the commercial reality.\"",
                QuestionCategory.CommercialExecutivePressure,
                2)
        };
    }

    private RuntimeInterviewQuestion[] AppendCommercialQuestions(RuntimeInterviewQuestion[] existing)
    {
        RuntimeInterviewQuestion[] expanded = new RuntimeInterviewQuestion[4];
        existing.CopyTo(expanded, 0);
        expanded[1] = new RuntimeInterviewQuestion(
            "COMM-02",
            "Sales Director",
            "The CRO wants a close plan, but the security team says they need another month of testing. How do you avoid losing the deal or the trust?",
            new[]
            {
                new AnswerData("Split technical validation from commercial approval and agree what evidence must be produced by each date.", 2, 3, 2, 0),
                new AnswerData("Push for executive alignment and let the technical team continue testing in parallel.", 0, 3, 0, 1),
                new AnswerData("Tell the CRO the team is dragging their feet and needs pressure.", -1, 2, -2, -1),
                new AnswerData("Ask the technical team what unresolved risk blocks a recommendation, then convert that into the close plan.", 2, 2, 3, 0)
            });
        expanded[2] = new RuntimeInterviewQuestion(
            "COMM-03",
            "Sales Director",
            "The CFO asks why this should be funded now instead of next fiscal year. The champion looks at you. What do you say?",
            new[]
            {
                new AnswerData("Tie delay to quantified exposure, audit deadlines, and the operational cost of current gaps.", 2, 3, 1, 0),
                new AnswerData("Explain that threat actors are moving quickly and waiting is dangerous.", 1, 1, 0, 0),
                new AnswerData("Offer phased scope that protects the highest-risk use case first.", 1, 3, 2, 0),
                new AnswerData("Say budget timing is a business decision and return to technical value.", 1, -1, -1, -1)
            });
        expanded[3] = new RuntimeInterviewQuestion(
            "COMM-04",
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
        currentQuestionSeed = useDeterministicQuestionSeed
            ? deterministicQuestionSeed
            : System.Environment.TickCount & int.MaxValue;

        questionRandom = new System.Random(currentQuestionSeed);

        List<RuntimeInterviewQuestion> selectedQuestions = new List<RuntimeInterviewQuestion>();
        List<InterviewStageData> selectedStages = new List<InterviewStageData>();
        for (int i = 0; i < interviewStages.Length; i++)
        {
            InterviewStageData stage = interviewStages[i];
            RuntimeInterviewQuestion[] stageQuestions = PickQuestionsForStage(stage);
            for (int questionIndex = 0; questionIndex < stageQuestions.Length; questionIndex++)
            {
                selectedQuestions.Add(stageQuestions[questionIndex]);
                selectedStages.Add(stage);
            }
        }

        questions = selectedQuestions.ToArray();
        questionStages = selectedStages.ToArray();
        RefreshDebugStatus();
    }

    private RuntimeInterviewQuestion[] PickQuestionsForStage(InterviewStageData stage)
    {
        RuntimeInterviewQuestion[] pool = GetPoolForCategory(stage.Category);
        if (pool == null || pool.Length == 0)
        {
            Debug.LogWarning($"Final Round RC15: no valid questions available for {stage.StageName}.");
            return new[] { CreateFallbackQuestion() };
        }

        int count = Mathf.Min(stage.QuestionCount, pool.Length);
        if (count < stage.QuestionCount)
        {
            Debug.LogWarning($"Final Round RC15: {stage.StageName} requested {stage.QuestionCount} questions, but only {pool.Length} are available.");
        }

        List<RuntimeInterviewQuestion> availableQuestions = new List<RuntimeInterviewQuestion>(pool);
        RuntimeInterviewQuestion[] selected = new RuntimeInterviewQuestion[count];
        for (int i = 0; i < count; i++)
        {
            int selectedIndex = questionRandom.Next(availableQuestions.Count);
            selected[i] = availableQuestions[selectedIndex];
            availableQuestions.RemoveAt(selectedIndex);
        }

        return selected;
    }

    private RuntimeInterviewQuestion[] GetPoolForCategory(QuestionCategory category)
    {
        return category switch
        {
            QuestionCategory.ContextCustomerScenario => contextQuestionPool,
            QuestionCategory.TechnicalSecurityJudgement => technicalQuestionPool,
            QuestionCategory.CommercialExecutivePressure => commercialQuestionPool,
            _ => null
        };
    }

    private RuntimeInterviewQuestion CreateFallbackQuestion()
    {
        return new RuntimeInterviewQuestion(
            "FALLBACK-01",
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

    private RuntimeInterviewQuestion PickQuestion(RuntimeInterviewQuestion[] pool)
    {
        if (pool == null || pool.Length == 0)
        {
            Debug.LogError("Final Round RC11 question pool is empty.");
            return CreateFallbackQuestion();
        }

        return pool[questionRandom.Next(pool.Length)];
    }

    private InterviewStageData GetStageForQuestion(int questionIndex)
    {
        if (questionStages == null || questionIndex < 0 || questionIndex >= questionStages.Length)
        {
            return interviewStages != null && interviewStages.Length > 0 ? interviewStages[0] : null;
        }

        return questionStages[questionIndex];
    }

    private void LogRoomRunSummaryOnce(InterviewOutcomeType outcome)
    {
        if (runSummaryLogged)
        {
            return;
        }

        runSummaryLogged = true;
        Debug.Log(
            "Final Round RC15 Room Run Summary\n" +
            $"Seed: {currentQuestionSeed}\n" +
            $"Selected Question IDs: {GetSelectedQuestionIdSummary()}\n" +
            $"Selected Answer Indexes: {GetSelectedAnswerIndexSummary()}\n" +
            $"Final Scores: Technical {score.Technical}, Commercial {score.Commercial}, Rapport {score.Rapport}, Energy {score.Energy}\n" +
            $"Outcome: {outcome}\n" +
            $"Forced Outcome Used: {(debugForceOutcome ? debugForcedOutcome.ToString() : "off")}\n" +
            $"Deterministic Seed Active: {useDeterministicQuestionSeed}\n" +
            $"Playtest Mode: {(playtestModeEnabled ? "on" : "off")}");
    }

    private string GetSelectedAnswerIndexSummary()
    {
        if (selectedAnswerIndexes.Count == 0)
        {
            return "none";
        }

        return string.Join(", ", selectedAnswerIndexes);
    }

    private bool TryBuildQuestionsFromAssets()
    {
        InterviewQuestionData[] sourceQuestions = questionBank;
        if (sourceQuestions == null || sourceQuestions.Length == 0)
        {
            sourceQuestions = Resources.LoadAll<InterviewQuestionData>(DefaultQuestionResourcePath);
            System.Array.Sort(sourceQuestions, (left, right) => string.CompareOrdinal(left == null ? string.Empty : left.QuestionId, right == null ? string.Empty : right.QuestionId));
        }

        if (sourceQuestions == null || sourceQuestions.Length == 0)
        {
            return false;
        }

        contextQuestionPool = BuildCategoryPool(sourceQuestions, QuestionCategory.ContextCustomerScenario);
        technicalQuestionPool = BuildCategoryPool(sourceQuestions, QuestionCategory.TechnicalSecurityJudgement);
        commercialQuestionPool = BuildCategoryPool(sourceQuestions, QuestionCategory.CommercialExecutivePressure);

        bool valid = contextQuestionPool.Length > 0 && technicalQuestionPool.Length > 0 && commercialQuestionPool.Length > 0;
        if (!valid)
        {
            Debug.LogWarning("Final Round RC11: ScriptableObject question bank must include at least one valid question per category.");
        }

        return valid;
    }

    private RuntimeInterviewQuestion[] BuildCategoryPool(InterviewQuestionData[] sourceQuestions, QuestionCategory category)
    {
        List<RuntimeInterviewQuestion> pool = new List<RuntimeInterviewQuestion>();
        for (int i = 0; i < sourceQuestions.Length; i++)
        {
            InterviewQuestionData question = sourceQuestions[i];
            if (question == null || question.Category != category)
            {
                continue;
            }

            if (!question.IsValid(out string validationError))
            {
                Debug.LogWarning($"Final Round RC11: skipping question asset '{question.name}' because {validationError}");
                continue;
            }

            AnswerData[] answers = new AnswerData[question.AnswerOptions.Length];
            for (int answerIndex = 0; answerIndex < answers.Length; answerIndex++)
            {
                AnswerOptionData answer = question.AnswerOptions[answerIndex];
                answers[answerIndex] = new AnswerData(
                    answer.AnswerText,
                    answer.TechnicalDelta,
                    answer.CommercialDelta,
                    answer.RapportDelta,
                    answer.EnergyDelta);
            }

            pool.Add(new RuntimeInterviewQuestion(question.QuestionId, question.SpeakerName, question.QuestionText, answers));
        }

        return pool.ToArray();
    }

    private sealed class RuntimeInterviewQuestion
    {
        public string QuestionId { get; }
        public string InterviewerName { get; }
        public string QuestionText { get; }
        public AnswerData[] Answers { get; }

        public RuntimeInterviewQuestion(string questionId, string interviewerName, string questionText, AnswerData[] answers)
        {
            QuestionId = string.IsNullOrWhiteSpace(questionId) ? "QUESTION-UNSET" : questionId;
            InterviewerName = interviewerName;
            QuestionText = questionText;
            Answers = answers;
        }
    }

    private sealed class InterviewStageData
    {
        public int StageIndex { get; }
        public string StageName { get; }
        public string LeadInterviewer { get; }
        public string IntroText { get; }
        public QuestionCategory Category { get; }
        public int QuestionCount { get; }

        public InterviewStageData(int stageIndex, string stageName, string leadInterviewer, string introText, QuestionCategory category, int questionCount)
        {
            StageIndex = stageIndex;
            StageName = stageName;
            LeadInterviewer = leadInterviewer;
            IntroText = introText;
            Category = category;
            QuestionCount = Mathf.Max(1, questionCount);
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

        public void ApplyStartingModifiers(int technical, int commercial, int rapport, int energy)
        {
            Technical = Mathf.Clamp(Technical + technical, 0, 10);
            Commercial = Mathf.Clamp(Commercial + commercial, 0, 10);
            Rapport = Mathf.Clamp(Rapport + rapport, 0, 10);
            Energy = Mathf.Clamp(Energy + energy, 0, 10);
        }

        public OutcomeScoreSnapshot ToSnapshot()
        {
            return new OutcomeScoreSnapshot(Technical, Commercial, Rapport, Energy);
        }
    }
}
