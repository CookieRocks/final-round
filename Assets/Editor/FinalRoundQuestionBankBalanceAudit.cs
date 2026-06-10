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
    private const string StagedRunReportPath = "Docs/FinalRound_P23_StagedRunAudit.md";
    private const int StartingTechnical = 4;
    private const int StartingCommercial = 4;
    private const int StartingRapport = 4;
    private const int StartingEnergy = 6;
    private const int ExpectedDeltaMin = -3;
    private const int ExpectedDeltaMax = 3;
    private const int StagedRunQuestionsPerCategory = 2;
    private const int StagedRunSampleCount = 100000;
    private const int StagedRunSampleSeed = 230023;

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

    [MenuItem("Final Round/Audit P23 Staged Run Balance")]
    public static void GenerateStagedRunReport()
    {
        InterviewQuestionData[] questions = Resources.LoadAll<InterviewQuestionData>(ResourcePath)
            .Where(question => question != null)
            .OrderBy(question => question.QuestionId, StringComparer.Ordinal)
            .ToArray();

        string report = BuildStagedRunReport(questions);
        string absolutePath = Path.Combine(Directory.GetCurrentDirectory(), StagedRunReportPath);
        Directory.CreateDirectory(Path.GetDirectoryName(absolutePath));
        File.WriteAllText(absolutePath, report);
        AssetDatabase.Refresh();
        Debug.Log($"Final Round P23 staged-run balance audit written to {StagedRunReportPath}");
    }

    [MenuItem("Final Round/Find RC15 Seed Presets")]
    public static void FindSeedPresets()
    {
        InterviewQuestionData[] questions = Resources.LoadAll<InterviewQuestionData>(ResourcePath)
            .Where(question => question != null)
            .OrderBy(question => question.QuestionId, StringComparer.Ordinal)
            .ToArray();

        List<QuestionRecord> validQuestions = new List<QuestionRecord>();
        foreach (InterviewQuestionData question in questions)
        {
            if (TryConvert(question, out QuestionRecord record, out _))
            {
                validQuestions.Add(record);
            }
        }

        Dictionary<QuestionCategory, List<QuestionRecord>> pools = new Dictionary<QuestionCategory, List<QuestionRecord>>
        {
            { QuestionCategory.ContextCustomerScenario, validQuestions.Where(question => question.Category == QuestionCategory.ContextCustomerScenario).ToList() },
            { QuestionCategory.TechnicalSecurityJudgement, validQuestions.Where(question => question.Category == QuestionCategory.TechnicalSecurityJudgement).ToList() },
            { QuestionCategory.CommercialExecutivePressure, validQuestions.Where(question => question.Category == QuestionCategory.CommercialExecutivePressure).ToList() }
        };

        Dictionary<InterviewOutcomeType, SeedPreset> presets = FindSeedPresets(pools, 50000);
        StringBuilder message = new StringBuilder("Final Round RC15 seed presets\n");
        foreach (InterviewOutcomeType outcomeType in Enum.GetValues(typeof(InterviewOutcomeType)))
        {
            if (presets.TryGetValue(outcomeType, out SeedPreset preset))
            {
                message.AppendLine($"{outcomeType}: seed {preset.Seed}, questions {preset.QuestionIds}, answer indexes {preset.AnswerIndexes}, scores T{preset.Technical}/C{preset.Commercial}/R{preset.Rapport}/E{preset.Energy}");
            }
            else
            {
                message.AppendLine($"{outcomeType}: no preset found in search range.");
            }
        }

        Debug.Log(message.ToString());
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

    private static string BuildStagedRunReport(IReadOnlyList<InterviewQuestionData> sourceQuestions)
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
        List<StagedSimulatedOutcome> outcomes = SimulateStagedRuns(contextQuestions, technicalQuestions, commercialQuestions, StagedRunSampleCount, StagedRunSampleSeed);

        StringBuilder report = new StringBuilder();
        report.AppendLine("# Final Round - Room Prototype P23 Staged Run Audit");
        report.AppendLine();
        report.AppendLine("Generated from `Assets/Resources/FinalRound/Questions/RC11`.");
        report.AppendLine("No question wording, score deltas, outcome thresholds, stage structure, UI, or room setup was changed by this audit.");
        report.AppendLine();
        report.AppendLine("## Runtime Flow Verification");
        report.AppendLine();
        report.AppendLine("The active room interview flow is `CybersecurityPresalesInterviewFlow`, which loads ScriptableObject questions from `FinalRound/Questions/RC11` when no explicit inspector question bank is assigned.");
        report.AppendLine("Runtime stages are configured as:");
        report.AppendLine("- Stage 1: Customer Context: 2 context/customer scenario questions.");
        report.AppendLine("- Stage 2: Technical Judgement: 2 technical/security judgement questions.");
        report.AppendLine("- Stage 3: Commercial Pressure: 2 commercial/executive pressure questions.");
        report.AppendLine("- Total runtime questions per run: 6.");
        report.AppendLine();
        report.AppendLine("P22's reported distribution was a short-run audit: 1 context + 1 technical + 1 commercial question, 3 total. It does not match the current 6-question staged runtime flow.");
        report.AppendLine();
        report.AppendLine("## Audit Method");
        report.AppendLine();
        report.AppendLine($"This P23 audit uses a deterministic sampled staged-run simulation with seed `{StagedRunSampleSeed}` and `{StagedRunSampleCount}` sampled runs.");
        report.AppendLine("For each sampled run it selects 2 questions without replacement from each category, then selects one of 4 answers for each selected question with uniform probability.");
        report.AppendLine("Scores start at Technical 4, Commercial 4, Rapport 4, Energy 6. Score clamping is applied after each answer, matching runtime `InterviewScore.Apply` behavior.");
        report.AppendLine();
        AppendValidationSummary(report, sourceQuestions.Count, validQuestions, contextQuestions, technicalQuestions, commercialQuestions, validationNotes);
        AppendStagedOutcomeSummary(report, outcomes);
        AppendStagedScoreSummary(report, outcomes);
        AppendClampSummary(report, outcomes);
        AppendQuestionNotes(report, validQuestions);
        AppendStagedRecommendations(report, outcomes, validQuestions);
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

    private static List<StagedSimulatedOutcome> SimulateStagedRuns(
        IReadOnlyList<QuestionRecord> contextQuestions,
        IReadOnlyList<QuestionRecord> technicalQuestions,
        IReadOnlyList<QuestionRecord> commercialQuestions,
        int sampleCount,
        int sampleSeed)
    {
        List<StagedSimulatedOutcome> outcomes = new List<StagedSimulatedOutcome>(sampleCount);
        if (contextQuestions.Count == 0 || technicalQuestions.Count == 0 || commercialQuestions.Count == 0)
        {
            return outcomes;
        }

        System.Random random = new System.Random(sampleSeed);
        for (int sampleIndex = 0; sampleIndex < sampleCount; sampleIndex++)
        {
            List<QuestionRecord> selectedQuestions = new List<QuestionRecord>();
            selectedQuestions.AddRange(PickQuestions(contextQuestions, random, StagedRunQuestionsPerCategory));
            selectedQuestions.AddRange(PickQuestions(technicalQuestions, random, StagedRunQuestionsPerCategory));
            selectedQuestions.AddRange(PickQuestions(commercialQuestions, random, StagedRunQuestionsPerCategory));

            int technicalScore = StartingTechnical;
            int commercialScore = StartingCommercial;
            int rapportScore = StartingRapport;
            int energyScore = StartingEnergy;
            bool clampOccurred = false;

            for (int questionIndex = 0; questionIndex < selectedQuestions.Count; questionIndex++)
            {
                QuestionRecord question = selectedQuestions[questionIndex];
                AnswerRecord answer = question.Answers[random.Next(question.Answers.Count)];
                technicalScore = ClampScoreWithFlag(technicalScore + answer.Technical, ref clampOccurred);
                commercialScore = ClampScoreWithFlag(commercialScore + answer.Commercial, ref clampOccurred);
                rapportScore = ClampScoreWithFlag(rapportScore + answer.Rapport, ref clampOccurred);
                energyScore = ClampScoreWithFlag(energyScore + answer.Energy, ref clampOccurred);
            }

            outcomes.Add(new StagedSimulatedOutcome(
                technicalScore,
                commercialScore,
                rapportScore,
                energyScore,
                DetermineOutcome(technicalScore, commercialScore, rapportScore, energyScore),
                clampOccurred));
        }

        return outcomes;
    }

    private static Dictionary<InterviewOutcomeType, SeedPreset> FindSeedPresets(Dictionary<QuestionCategory, List<QuestionRecord>> pools, int maxSeed)
    {
        Dictionary<InterviewOutcomeType, SeedPreset> presets = new Dictionary<InterviewOutcomeType, SeedPreset>();
        HashSet<int> usedSeeds = new HashSet<int>();
        for (int seed = 1; seed <= maxSeed && presets.Count < 4; seed++)
        {
            if (usedSeeds.Contains(seed))
            {
                continue;
            }

            System.Random random = new System.Random(seed);
            List<QuestionRecord> selectedQuestions = new List<QuestionRecord>();
            selectedQuestions.AddRange(PickQuestions(pools[QuestionCategory.ContextCustomerScenario], random, 2));
            selectedQuestions.AddRange(PickQuestions(pools[QuestionCategory.TechnicalSecurityJudgement], random, 2));
            selectedQuestions.AddRange(PickQuestions(pools[QuestionCategory.CommercialExecutivePressure], random, 2));
            if (selectedQuestions.Count == 0)
            {
                continue;
            }

            foreach (SeedPreset candidate in SimulateAnswerPathsForSeed(seed, selectedQuestions))
            {
                if (!presets.ContainsKey(candidate.Outcome))
                {
                    presets.Add(candidate.Outcome, candidate);
                    usedSeeds.Add(seed);
                    break;
                }
            }
        }

        return presets;
    }

    private static IEnumerable<QuestionRecord> PickQuestions(IReadOnlyList<QuestionRecord> pool, System.Random random, int requestedCount)
    {
        if (pool == null || pool.Count == 0)
        {
            yield break;
        }

        List<QuestionRecord> availableQuestions = new List<QuestionRecord>(pool);
        int count = Math.Min(requestedCount, availableQuestions.Count);
        for (int i = 0; i < count; i++)
        {
            int index = random.Next(availableQuestions.Count);
            QuestionRecord question = availableQuestions[index];
            availableQuestions.RemoveAt(index);
            yield return question;
        }
    }

    private static IEnumerable<SeedPreset> SimulateAnswerPathsForSeed(int seed, IReadOnlyList<QuestionRecord> questions)
    {
        List<AnswerRecord> selectedAnswers = new List<AnswerRecord>();
        foreach (SeedPreset preset in SimulateAnswerPathsForSeed(seed, questions, 0, selectedAnswers))
        {
            yield return preset;
        }
    }

    private static IEnumerable<SeedPreset> SimulateAnswerPathsForSeed(int seed, IReadOnlyList<QuestionRecord> questions, int questionIndex, List<AnswerRecord> selectedAnswers)
    {
        if (questionIndex >= questions.Count)
        {
            int technicalScore = ClampScore(StartingTechnical + selectedAnswers.Sum(answer => answer.Technical));
            int commercialScore = ClampScore(StartingCommercial + selectedAnswers.Sum(answer => answer.Commercial));
            int rapportScore = ClampScore(StartingRapport + selectedAnswers.Sum(answer => answer.Rapport));
            int energyScore = ClampScore(StartingEnergy + selectedAnswers.Sum(answer => answer.Energy));
            InterviewOutcomeType outcome = DetermineOutcome(technicalScore, commercialScore, rapportScore, energyScore);
            yield return new SeedPreset(
                seed,
                outcome,
                string.Join(", ", questions.Select(question => question.QuestionId)),
                string.Join(", ", selectedAnswers.Select(answer => answer.Index.ToString())),
                technicalScore,
                commercialScore,
                rapportScore,
                energyScore);
            yield break;
        }

        foreach (AnswerRecord answer in questions[questionIndex].Answers)
        {
            selectedAnswers.Add(answer);
            foreach (SeedPreset preset in SimulateAnswerPathsForSeed(seed, questions, questionIndex + 1, selectedAnswers))
            {
                yield return preset;
            }
            selectedAnswers.RemoveAt(selectedAnswers.Count - 1);
        }
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

    private static int ClampScoreWithFlag(int value, ref bool clampOccurred)
    {
        int clamped = ClampScore(value);
        if (clamped != value)
        {
            clampOccurred = true;
        }

        return clamped;
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

    private static void AppendStagedOutcomeSummary(StringBuilder report, IReadOnlyList<StagedSimulatedOutcome> outcomes)
    {
        report.AppendLine("## Outcome Distribution For Real Runtime Flow");
        report.AppendLine();
        report.AppendLine($"- Total sampled staged runs: {outcomes.Count}");
        foreach (InterviewOutcomeType outcomeType in Enum.GetValues(typeof(InterviewOutcomeType)))
        {
            int count = outcomes.Count(outcome => outcome.Outcome == outcomeType);
            float percent = outcomes.Count == 0 ? 0f : count * 100f / outcomes.Count;
            report.AppendLine($"- {outcomeType}: {count} ({percent:0.0}%)");
        }
        report.AppendLine();
    }

    private static void AppendStagedScoreSummary(StringBuilder report, IReadOnlyList<StagedSimulatedOutcome> outcomes)
    {
        report.AppendLine("## Score Distribution Summary");
        report.AppendLine();
        AppendDimension(report, "Technical", outcomes.Select(outcome => outcome.Technical));
        AppendDimension(report, "Commercial", outcomes.Select(outcome => outcome.Commercial));
        AppendDimension(report, "Rapport", outcomes.Select(outcome => outcome.Rapport));
        AppendDimension(report, "Energy", outcomes.Select(outcome => outcome.Energy));
        report.AppendLine();
    }

    private static void AppendClampSummary(StringBuilder report, IReadOnlyList<StagedSimulatedOutcome> outcomes)
    {
        report.AppendLine("## Clamp And Saturation Notes");
        report.AppendLine();
        if (outcomes.Count == 0)
        {
            report.AppendLine("- No staged runs were available to inspect for score clamping.");
            report.AppendLine();
            return;
        }

        int clampCount = outcomes.Count(outcome => outcome.ClampOccurred);
        report.AppendLine($"- Runs where at least one score delta was clamped during answer application: {clampCount} ({clampCount * 100f / outcomes.Count:0.0}%).");
        AppendSaturationDimension(report, "Technical", outcomes.Select(outcome => outcome.Technical));
        AppendSaturationDimension(report, "Commercial", outcomes.Select(outcome => outcome.Commercial));
        AppendSaturationDimension(report, "Rapport", outcomes.Select(outcome => outcome.Rapport));
        AppendSaturationDimension(report, "Energy", outcomes.Select(outcome => outcome.Energy));
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

    private static void AppendSaturationDimension(StringBuilder report, string label, IEnumerable<int> values)
    {
        int[] valueArray = values.ToArray();
        if (valueArray.Length == 0)
        {
            report.AppendLine($"- {label}: no data");
            return;
        }

        int minimumCount = valueArray.Count(value => value == 0);
        int maximumCount = valueArray.Count(value => value == 10);
        report.AppendLine($"- {label}: final score at 0 in {minimumCount} runs ({minimumCount * 100f / valueArray.Length:0.0}%), at 10 in {maximumCount} runs ({maximumCount * 100f / valueArray.Length:0.0}%).");
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

    private static void AppendStagedRecommendations(StringBuilder report, IReadOnlyList<StagedSimulatedOutcome> outcomes, IReadOnlyList<QuestionRecord> questions)
    {
        report.AppendLine("## Recommendation");
        report.AppendLine();
        if (outcomes.Count == 0)
        {
            report.AppendLine("- No staged answer paths were available to simulate.");
            return;
        }

        bool missingOutcome = false;
        bool strongPassTooCommon = false;
        foreach (InterviewOutcomeType outcomeType in Enum.GetValues(typeof(InterviewOutcomeType)))
        {
            int count = outcomes.Count(outcome => outcome.Outcome == outcomeType);
            if (count == 0)
            {
                missingOutcome = true;
                report.AppendLine($"- {outcomeType} never appears in the staged-run sample.");
            }
            else if (count > outcomes.Count * 0.7f)
            {
                report.AppendLine($"- {outcomeType} appears very often ({count}/{outcomes.Count}).");
            }

            if (outcomeType == InterviewOutcomeType.StrongPass && count > outcomes.Count * 0.2f)
            {
                strongPassTooCommon = true;
                report.AppendLine($"- StrongPass appears high for a final-round prototype sample ({count}/{outcomes.Count}).");
            }
        }

        int dominantWarnings = questions.Count(question => HasDominantAnswer(question, out _));
        int tradeoffWarnings = questions.Count(HasNoMeaningfulTradeoff);
        int clampCount = outcomes.Count(outcome => outcome.ClampOccurred);
        bool clampFrequent = clampCount > outcomes.Count * 0.5f;
        report.AppendLine($"- Dominant-answer warnings: {dominantWarnings}.");
        report.AppendLine($"- Limited-trade-off warnings: {tradeoffWarnings}.");
        report.AppendLine($"- Runs with at least one score clamp: {clampCount}.");

        if (!missingOutcome && dominantWarnings == 0 && tradeoffWarnings == 0 && !strongPassTooCommon && !clampFrequent)
        {
            report.AppendLine("- Recommendation: leave tuning as-is for now. The real staged flow keeps all outcomes reachable and preserves mixed-performance Holds.");
        }
        else if (strongPassTooCommon || clampFrequent)
        {
            report.AppendLine("- Recommendation: tune in a follow-up pass. Do not change P23 content here, but the 6-question runtime flow makes high-end scores saturate more often than the short-run audit suggested.");
        }
        else
        {
            report.AppendLine("- Recommendation: review the warnings above before making threshold or score-delta changes.");
        }

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

    private readonly struct StagedSimulatedOutcome
    {
        public int Technical { get; }
        public int Commercial { get; }
        public int Rapport { get; }
        public int Energy { get; }
        public InterviewOutcomeType Outcome { get; }
        public bool ClampOccurred { get; }

        public StagedSimulatedOutcome(int technical, int commercial, int rapport, int energy, InterviewOutcomeType outcome, bool clampOccurred)
        {
            Technical = technical;
            Commercial = commercial;
            Rapport = rapport;
            Energy = energy;
            Outcome = outcome;
            ClampOccurred = clampOccurred;
        }
    }

    private readonly struct SeedPreset
    {
        public int Seed { get; }
        public InterviewOutcomeType Outcome { get; }
        public string QuestionIds { get; }
        public string AnswerIndexes { get; }
        public int Technical { get; }
        public int Commercial { get; }
        public int Rapport { get; }
        public int Energy { get; }

        public SeedPreset(int seed, InterviewOutcomeType outcome, string questionIds, string answerIndexes, int technical, int commercial, int rapport, int energy)
        {
            Seed = seed;
            Outcome = outcome;
            QuestionIds = questionIds;
            AnswerIndexes = answerIndexes;
            Technical = technical;
            Commercial = commercial;
            Rapport = rapport;
            Energy = energy;
        }
    }
}
