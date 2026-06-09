using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class FinalRoundQuestionBankBalanceAudit
{
    private const string ResourcePath = "FinalRound/Questions/RC11";
    private const string ReportPath = "Docs/FinalRound_RC12_BalanceAudit.md";
    private const int StartingTechnical = 4;
    private const int StartingCommercial = 4;
    private const int StartingRapport = 4;
    private const int StartingEnergy = 6;
    private const int ExpectedDeltaMin = -3;
    private const int ExpectedDeltaMax = 3;

    [MenuItem("Final Round/Audit Question Bank Balance")]
    public static void GenerateReport()
    {
        InterviewQuestionData[] questions = Resources.LoadAll<InterviewQuestionData>(ResourcePath)
            .Where(question => question != null)
            .OrderBy(question => question.QuestionId, StringComparer.Ordinal)
            .ToArray();

        string report = BuildReport(questions);
        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), ReportPath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
        File.WriteAllText(absolutePath, report);
        AssetDatabase.Refresh();
        Debug.Log($"Final Round RC12 question bank balance audit written to {ReportPath}");
    }

    private static string BuildReport(IReadOnlyList<InterviewQuestionData> sourceQuestions)
    {
        List<QuestionRecord> validQuestions = new List<QuestionRecord>();
        List<string> validationNotes = new List<string>();

        foreach (InterviewQuestionData question in sourceQuestions)
        {
            if (question == null)
            {
                continue;
            }

            if (!TryConvert(question, out QuestionRecord record, out string validationNote))
            {
                validationNotes.Add(validationNote);
                continue;
            }

            validQuestions.Add(record);
        }

        List<QuestionRecord> contextQuestions = validQuestions.Where(question => question.Category == QuestionCategory.ContextCustomerScenario).ToList();
        List<QuestionRecord> technicalQuestions = validQuestions.Where(question => question.Category == QuestionCategory.TechnicalSecurityJudgement).ToList();
        List<QuestionRecord> commercialQuestions = validQuestions.Where(question => question.Category == QuestionCategory.CommercialExecutivePressure).ToList();
        List<SimulatedOutcome> outcomes = Simulate(contextQuestions, technicalQuestions, commercialQuestions);

        StringBuilder report = new StringBuilder();
        report.AppendLine("# Final Round Prototype v1.0 RC12 Balance Audit");
        report.AppendLine();
        report.AppendLine("Generated from `Assets/Resources/FinalRound/Questions/RC11`.");
        report.AppendLine("No question wording, score deltas, or gameplay behavior was changed by this audit.");
        report.AppendLine();
        AppendValidationSummary(report, sourceQuestions.Count, validQuestions, contextQuestions, technicalQuestions, commercialQuestions, validationNotes);
        AppendOutcomeSummary(report, outcomes);
        AppendScoreSummary(report, outcomes);
        AppendQuestionNotes(report, validQuestions);
        AppendRecommendations(report, outcomes, validQuestions);
        return report.ToString();
    }

    private static bool TryConvert(InterviewQuestionData question, out QuestionRecord record, out string validationNote)
    {
        record = null;
        if (!question.IsValid(out string validationError))
        {
            validationNote = $"- `{question.name}` invalid: {validationError}.";
            return false;
        }

        List<AnswerRecord> answers = new List<AnswerRecord>();
        for (int i = 0; i < question.AnswerOptions.Length; i++)
        {
            AnswerOptionData option = question.AnswerOptions[i];
            AnswerRecord answer = new AnswerRecord(
                i + 1,
                option.AnswerText,
                option.TechnicalDelta,
                option.CommercialDelta,
                option.RapportDelta,
                option.EnergyDelta);
            if (!answer.HasExpectedDeltaRange(ExpectedDeltaMin, ExpectedDeltaMax, out string rangeError))
            {
                validationNote = $"`{question.name}` invalid: question '{question.QuestionId}' answer {i + 1} {rangeError}.";
                return false;
            }

            answers.Add(answer);
        }

        record = new QuestionRecord(question.QuestionId, question.Category, question.SpeakerName, question.QuestionText, answers);
        validationNote = string.Empty;
        return true;
    }

    private static List<SimulatedOutcome> Simulate(
        IReadOnlyList<QuestionRecord> contextQuestions,
        IReadOnlyList<QuestionRecord> technicalQuestions,
        IReadOnlyList<QuestionRecord> commercialQuestions)
    {
        List<SimulatedOutcome> outcomes = new List<SimulatedOutcome>();
        foreach (QuestionRecord context in contextQuestions)
        {
            foreach (QuestionRecord technical in technicalQuestions)
            {
                foreach (QuestionRecord commercial in commercialQuestions)
                {
                    foreach (AnswerRecord contextAnswer in context.Answers)
                    {
                        foreach (AnswerRecord technicalAnswer in technical.Answers)
                        {
                            foreach (AnswerRecord commercialAnswer in commercial.Answers)
                            {
                                int technicalScore = ClampScore(StartingTechnical + contextAnswer.Technical + technicalAnswer.Technical + commercialAnswer.Technical);
                                int commercialScore = ClampScore(StartingCommercial + contextAnswer.Commercial + technicalAnswer.Commercial + commercialAnswer.Commercial);
                                int rapportScore = ClampScore(StartingRapport + contextAnswer.Rapport + technicalAnswer.Rapport + commercialAnswer.Rapport);
                                int energyScore = ClampScore(StartingEnergy + contextAnswer.Energy + technicalAnswer.Energy + commercialAnswer.Energy);
                                outcomes.Add(new SimulatedOutcome(technicalScore, commercialScore, rapportScore, energyScore, DetermineOutcome(technicalScore, commercialScore, rapportScore, energyScore)));
                            }
                        }
                    }
                }
            }
        }

        return outcomes;
    }

    private static InterviewOutcomeType DetermineOutcome(int technical, int commercial, int rapport, int energy)
    {
        int total = technical + commercial + rapport + energy;
        if (technical >= 8 && commercial >= 7 && rapport >= 6 && total >= 29)
        {
            return InterviewOutcomeType.StrongPass;
        }

        if (total >= 22 && technical >= 5 && commercial >= 5 && energy >= 5)
        {
            return InterviewOutcomeType.Pass;
        }

        return total >= 19 ? InterviewOutcomeType.Hold : InterviewOutcomeType.Reject;
    }

    private static int ClampScore(int value)
    {
        return Mathf.Clamp(value, 0, 10);
    }

    private static void AppendValidationSummary(
        StringBuilder report,
        int sourceCount,
        IReadOnlyList<QuestionRecord> validQuestions,
        IReadOnlyList<QuestionRecord> contextQuestions,
        IReadOnlyList<QuestionRecord> technicalQuestions,
        IReadOnlyList<QuestionRecord> commercialQuestions,
        IReadOnlyList<string> validationNotes)
    {
        report.AppendLine("## Validation Summary");
        report.AppendLine();
        report.AppendLine($"- Source assets found: {sourceCount}");
        report.AppendLine($"- Valid questions: {validQuestions.Count}");
        report.AppendLine($"- Context/customer scenario: {contextQuestions.Count}");
        report.AppendLine($"- Technical/security judgement: {technicalQuestions.Count}");
        report.AppendLine($"- Commercial/executive pressure: {commercialQuestions.Count}");
        report.AppendLine($"- Validation warnings: {validationNotes.Count}");
        report.AppendLine();
        if (validationNotes.Count == 0)
        {
            report.AppendLine("All loaded question assets passed structural validation.");
        }
        else
        {
            foreach (string note in validationNotes)
            {
                report.AppendLine(note);
            }
        }
        report.AppendLine();
    }

    private static void AppendOutcomeSummary(StringBuilder report, IReadOnlyList<SimulatedOutcome> outcomes)
    {
        report.AppendLine("## Outcome Distribution");
        report.AppendLine();
        report.AppendLine($"- Total simulated answer paths: {outcomes.Count}");
        foreach (InterviewOutcomeType outcomeType in Enum.GetValues(typeof(InterviewOutcomeType)))
        {
            int count = outcomes.Count(outcome => outcome.Outcome == outcomeType);
            float percent = outcomes.Count == 0 ? 0f : count * 100f / outcomes.Count;
            report.AppendLine($"- {outcomeType}: {count} ({percent:0.0}%)");
        }
        report.AppendLine();
    }

    private static void AppendScoreSummary(StringBuilder report, IReadOnlyList<SimulatedOutcome> outcomes)
    {
        report.AppendLine("## Score Distribution Summary");
        report.AppendLine();
        AppendDimension(report, "Technical", outcomes.Select(outcome => outcome.Technical));
        AppendDimension(report, "Commercial", outcomes.Select(outcome => outcome.Commercial));
        AppendDimension(report, "Rapport", outcomes.Select(outcome => outcome.Rapport));
        AppendDimension(report, "Energy", outcomes.Select(outcome => outcome.Energy));
        report.AppendLine();
    }

    private static void AppendDimension(StringBuilder report, string label, IEnumerable<int> values)
    {
        int[] valueArray = values.ToArray();
        if (valueArray.Length == 0)
        {
            report.AppendLine($"- {label}: no data");
            return;
        }

        report.AppendLine($"- {label}: average {valueArray.Average():0.00}, min {valueArray.Min()}, max {valueArray.Max()}");
    }

    private static void AppendQuestionNotes(StringBuilder report, IReadOnlyList<QuestionRecord> questions)
    {
        report.AppendLine("## Question-Level Notes");
        report.AppendLine();
        foreach (QuestionRecord question in questions)
        {
            string dominantWarning = HasDominantAnswer(question, out int dominantAnswerIndex)
                ? $" Dominant answer warning: answer {dominantAnswerIndex} is never lower than any other answer across all score deltas."
                : string.Empty;
            string tradeoffWarning = HasNoMeaningfulTradeoff(question)
                ? " Trade-off warning: all answers trend in the same direction with little downside."
                : string.Empty;
            report.AppendLine($"- `{question.QuestionId}` ({question.Category}, {question.SpeakerName}):{dominantWarning}{tradeoffWarning}");
        }
        report.AppendLine();
    }

    private static void AppendRecommendations(StringBuilder report, IReadOnlyList<SimulatedOutcome> outcomes, IReadOnlyList<QuestionRecord> questions)
    {
        report.AppendLine("## Recommended Tuning Areas");
        report.AppendLine();
        if (outcomes.Count == 0)
        {
            report.AppendLine("- No answer paths were available to simulate.");
            return;
        }

        foreach (InterviewOutcomeType outcomeType in Enum.GetValues(typeof(InterviewOutcomeType)))
        {
            int count = outcomes.Count(outcome => outcome.Outcome == outcomeType);
            if (count == 0)
            {
                report.AppendLine($"- {outcomeType} never appears.");
            }
            else if (count > outcomes.Count * 0.7f)
            {
                report.AppendLine($"- {outcomeType} appears very often ({count}/{outcomes.Count}).");
            }
        }

        Dictionary<string, int> deltaTouches = CountDimensionTouches(questions);
        foreach (KeyValuePair<string, int> entry in deltaTouches)
        {
            if (entry.Value <= questions.Count)
            {
                report.AppendLine($"- {entry.Key} is rarely affected by answer deltas ({entry.Value} non-zero answer deltas).");
            }
        }

        report.AppendLine("- Review any dominant-answer warnings before expanding the bank.");
        report.AppendLine("- No automatic tuning changes were applied.");
    }

    private static bool HasDominantAnswer(QuestionRecord question, out int answerIndex)
    {
        answerIndex = -1;
        foreach (AnswerRecord candidate in question.Answers)
        {
            bool dominates = question.Answers
                .Where(other => !ReferenceEquals(candidate, other))
                .All(other => candidate.Technical >= other.Technical
                    && candidate.Commercial >= other.Commercial
                    && candidate.Rapport >= other.Rapport
                    && candidate.Energy >= other.Energy);
            if (dominates)
            {
                answerIndex = candidate.Index;
                return true;
            }
        }

        return false;
    }

    private static bool HasNoMeaningfulTradeoff(QuestionRecord question)
    {
        return question.Answers.All(answer => answer.Total >= 0)
            && question.Answers.Count(answer => answer.Technical < 0 || answer.Commercial < 0 || answer.Rapport < 0 || answer.Energy < 0) <= 1;
    }

    private static Dictionary<string, int> CountDimensionTouches(IReadOnlyList<QuestionRecord> questions)
    {
        return new Dictionary<string, int>
        {
            { "Technical", questions.Sum(question => question.Answers.Count(answer => answer.Technical != 0)) },
            { "Commercial", questions.Sum(question => question.Answers.Count(answer => answer.Commercial != 0)) },
            { "Rapport", questions.Sum(question => question.Answers.Count(answer => answer.Rapport != 0)) },
            { "Energy", questions.Sum(question => question.Answers.Count(answer => answer.Energy != 0)) }
        };
    }

    private sealed class QuestionRecord
    {
        public string QuestionId { get; }
        public QuestionCategory Category { get; }
        public string SpeakerName { get; }
        public string QuestionText { get; }
        public IReadOnlyList<AnswerRecord> Answers { get; }

        public QuestionRecord(string questionId, QuestionCategory category, string speakerName, string questionText, IReadOnlyList<AnswerRecord> answers)
        {
            QuestionId = questionId;
            Category = category;
            SpeakerName = speakerName;
            QuestionText = questionText;
            Answers = answers;
        }
    }

    private sealed class AnswerRecord
    {
        public int Index { get; }
        public string Text { get; }
        public int Technical { get; }
        public int Commercial { get; }
        public int Rapport { get; }
        public int Energy { get; }
        public int Total => Technical + Commercial + Rapport + Energy;

        public AnswerRecord(int index, string text, int technical, int commercial, int rapport, int energy)
        {
            Index = index;
            Text = text;
            Technical = technical;
            Commercial = commercial;
            Rapport = rapport;
            Energy = energy;
        }

        public bool HasExpectedDeltaRange(int minimum, int maximum, out string rangeError)
        {
            if (Technical < minimum || Technical > maximum)
            {
                rangeError = $"Technical delta {Technical} is outside {minimum}..{maximum}";
                return false;
            }

            if (Commercial < minimum || Commercial > maximum)
            {
                rangeError = $"Commercial delta {Commercial} is outside {minimum}..{maximum}";
                return false;
            }

            if (Rapport < minimum || Rapport > maximum)
            {
                rangeError = $"Rapport delta {Rapport} is outside {minimum}..{maximum}";
                return false;
            }

            if (Energy < minimum || Energy > maximum)
            {
                rangeError = $"Energy delta {Energy} is outside {minimum}..{maximum}";
                return false;
            }

            rangeError = string.Empty;
            return true;
        }
    }

    private readonly struct SimulatedOutcome
    {
        public int Technical { get; }
        public int Commercial { get; }
        public int Rapport { get; }
        public int Energy { get; }
        public InterviewOutcomeType Outcome { get; }

        public SimulatedOutcome(int technical, int commercial, int rapport, int energy, InterviewOutcomeType outcome)
        {
            Technical = technical;
            Commercial = commercial;
            Rapport = rapport;
            Energy = energy;
            Outcome = outcome;
        }
    }
}
