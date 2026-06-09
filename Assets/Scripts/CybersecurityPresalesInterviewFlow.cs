using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class CybersecurityPresalesInterviewFlow : MonoBehaviour
{
    private const float ReactionDelay = 1.35f;

    private readonly InterviewScore score = new InterviewScore();
    private InterviewQuestionData[] questions;
    private int currentQuestionIndex;
    private bool answerLocked;

    private Canvas canvas;
    private GameObject panelRoot;
    private GameObject questionPanel;
    private GameObject outcomePanel;
    private GameObject scorecardPanel;
    private TMP_Text interviewerText;
    private TMP_Text questionText;
    private TMP_Text reactionText;
    private TMP_Text outcomeTitleText;
    private TMP_Text outcomeBodyText;
    private TMP_Text scorecardText;
    private Button[] answerButtons;
    private TMP_Text[] answerButtonTexts;

    private void Awake()
    {
        BuildQuestions();
        BuildUi();
        ResetFlow();
    }

    public void BeginInterview()
    {
        EnsureUiReady();
        currentQuestionIndex = 0;
        answerLocked = false;
        score.Reset();
        panelRoot.SetActive(true);
        questionPanel.SetActive(true);
        outcomePanel.SetActive(false);
        scorecardPanel.SetActive(false);
        ShowCurrentQuestion();
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
    }

    private void ShowCurrentQuestion()
    {
        if (currentQuestionIndex >= questions.Length)
        {
            ShowOutcomeEmail();
            return;
        }

        InterviewQuestionData question = questions[currentQuestionIndex];
        interviewerText.text = question.InterviewerName;
        questionText.text = question.QuestionText;
        reactionText.text = string.Empty;
        answerLocked = false;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            int answerIndex = i;
            AnswerData answer = question.Answers[i];
            answerButtonTexts[i].text = answer.Text;
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
        AnswerData answer = question.Answers[answerIndex];
        score.Apply(answer);

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].interactable = false;
        }

        reactionText.text = answer.ReactionText;
        StartCoroutine(ContinueAfterReaction());
    }

    private IEnumerator ContinueAfterReaction()
    {
        yield return new WaitForSeconds(ReactionDelay);
        currentQuestionIndex++;
        ShowCurrentQuestion();
    }

    private void ShowOutcomeEmail()
    {
        questionPanel.SetActive(false);
        outcomePanel.SetActive(true);
        scorecardPanel.SetActive(false);

        string outcome = GetOutcomeName();
        outcomeTitleText.text = $"Subject: Interview follow-up - {outcome}";
        outcomeBodyText.text = BuildOutcomeEmail(outcome);
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

    private string GetOutcomeName()
    {
        int total = score.Total;
        if (score.Technical >= 7 && score.Commercial >= 6 && score.Rapport >= 5 && total >= 27)
        {
            return "Strong Pass";
        }

        if (total >= 22 && score.Technical >= 5 && score.Commercial >= 5)
        {
            return "Pass";
        }

        if (total >= 17)
        {
            return "Hold";
        }

        return "Reject";
    }

    private string BuildOutcomeEmail(string outcome)
    {
        string body = outcome switch
        {
            "Strong Pass" =>
                "Hi,\n\nThank you for making the time today. The panel felt you handled the technical and commercial tension well, particularly where the buyer context was incomplete.\n\nWe are going to recommend moving forward. Recruiting will follow up with next steps.\n\nRegards,\nHiring Team",
            "Pass" =>
                "Hi,\n\nThanks again for the conversation. The panel saw enough signal to continue, with some notes around sharpening the discovery-to-demo thread.\n\nRecruiting will be in touch once the debrief is closed.\n\nRegards,\nHiring Team",
            "Hold" =>
                "Hi,\n\nThank you for speaking with us today. Feedback was mixed. There were credible moments, but the panel was not fully aligned on whether the presales judgment was consistent enough.\n\nWe need a little more time before confirming next steps.\n\nRegards,\nHiring Team",
            _ =>
                "Hi,\n\nThank you for taking the time to meet with the team. After debrief, we have decided not to move forward for this role.\n\nWe appreciate your interest and wish you the best with your search.\n\nRegards,\nHiring Team"
        };

        return body;
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

        BuildOutcomePanel(panelRoot.transform);
    }

    private void BuildOutcomePanel(Transform parent)
    {
        outcomePanel = CreatePanel("Post Interview Email Panel", parent, new Color32(238, 241, 236, 248));
        RectTransform outcomeRect = outcomePanel.GetComponent<RectTransform>();
        outcomeRect.anchorMin = new Vector2(0.18f, 0.12f);
        outcomeRect.anchorMax = new Vector2(0.82f, 0.82f);
        outcomeRect.offsetMin = Vector2.zero;
        outcomeRect.offsetMax = Vector2.zero;
        AddVerticalLayout(outcomePanel, new RectOffset(34, 34, 30, 30), 16f);

        outcomeTitleText = CreateText("Email Subject", outcomePanel.transform, string.Empty, 28, FontStyles.Bold, TextAlignmentOptions.Left);
        outcomeTitleText.color = new Color32(35, 40, 48, 255);
        outcomeBodyText = CreateText("Email Body", outcomePanel.transform, string.Empty, 24, FontStyles.Normal, TextAlignmentOptions.Left);
        outcomeBodyText.color = new Color32(45, 50, 58, 255);
        ConfigureLayout(outcomeBodyText.gameObject, -1f, 260f);

        Button scorecardButton = CreateButton("Open Scorecard", outcomePanel.transform, new Color32(44, 58, 72, 255));
        TMP_Text buttonText = scorecardButton.GetComponentInChildren<TMP_Text>();
        buttonText.text = "Open Scorecard";
        scorecardButton.onClick.AddListener(ShowScorecard);

        scorecardPanel = CreatePanel("Scorecard Panel", outcomePanel.transform, new Color32(220, 225, 222, 255));
        AddVerticalLayout(scorecardPanel, new RectOffset(22, 22, 18, 18), 8f);
        ConfigureLayout(scorecardPanel, -1f, 210f);
        scorecardText = CreateText("Scorecard Text", scorecardPanel.transform, string.Empty, 23, FontStyles.Normal, TextAlignmentOptions.Left);
        scorecardText.color = new Color32(35, 40, 48, 255);
    }

    private static Button CreateAnswerButton(Transform parent, int index)
    {
        Button button = CreateButton($"Answer {index + 1}", parent, new Color32(33, 43, 56, 252));
        ConfigureLayout(button.gameObject, -1f, 56f);
        return button;
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

    private void BuildQuestions()
    {
        questions = new[]
        {
            new InterviewQuestionData(
                "Hiring Manager",
                "A customer says their board wants measurable cyber risk reduction this quarter, but the security team only wants to discuss tooling. How do you open discovery?",
                new[]
                {
                    new AnswerData("Start with the board metric, then ask what control failures or audit findings are driving urgency.", 2, 3, 2, 0, "The Hiring Manager nods, but does not smile."),
                    new AnswerData("Ask for their current tooling list so you can map the fastest demo path.", 1, 0, 0, 1, "The Sales Director makes a note."),
                    new AnswerData("Explain that cyber risk is hard to quantify and suggest a platform overview first.", 0, -1, -1, -1, "The Hiring Manager looks briefly at the Architect."),
                    new AnswerData("Ask who owns the board narrative, then separate technical validation from executive proof.", 1, 2, 3, 0, "The Sales Director writes something down slowly.")
                }),
            new InterviewQuestionData(
                "Principal Security Architect",
                "During a technical workshop, the customer challenges your detection claims and asks how you reduce false positives without hiding real incidents. What do you do?",
                new[]
                {
                    new AnswerData("Describe the tuning model, then ask for sample alert categories so you can test the claim against their environment.", 3, 1, 1, 0, "The Architect leans back slightly."),
                    new AnswerData("Say the product uses AI and shift quickly into the roadmap.", -1, 1, -1, 0, "The Architect stops taking notes."),
                    new AnswerData("Acknowledge the risk, explain the validation path, and define what evidence would make them comfortable.", 3, 2, 2, 0, "The Architect gives a small, reluctant nod."),
                    new AnswerData("Offer to bring in engineering later and move back to the slide deck.", 0, 0, 0, -1, "The Hiring Manager glances at the clock.")
                }),
            new InterviewQuestionData(
                "Sales Director",
                "Procurement says the incumbent is cheaper and good enough. The champion is nervous. What is your next move?",
                new[]
                {
                    new AnswerData("Discount early to protect momentum, then ask legal to accelerate paper.", -1, 0, -1, -1, "The Sales Director's expression does not change."),
                    new AnswerData("Rebuild the cost of inaction with the champion and arm them with a concise internal business case.", 1, 3, 2, 0, "The Sales Director makes a note."),
                    new AnswerData("Challenge procurement directly and explain that cheaper security usually means hidden risk.", 1, 1, -2, -1, "The Hiring Manager nods, but not in a good way."),
                    new AnswerData("Ask what 'good enough' means operationally, then tie gaps to renewal risk, audit pressure, and incident response cost.", 2, 3, 2, 0, "The Architect looks down, then writes one line.")
                })
        };
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
        public string ReactionText { get; }

        public AnswerData(string text, int technical, int commercial, int rapport, int energy, string reactionText)
        {
            Text = text;
            Technical = technical;
            Commercial = commercial;
            Rapport = rapport;
            Energy = energy;
            ReactionText = reactionText;
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
    }
}
