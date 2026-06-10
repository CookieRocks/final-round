# Final Round - Room Prototype P23 Staged Run Audit

## Scope

This checkpoint verifies whether the P22 balance audit matches the current runtime interview flow.

No question wording, score deltas, outcome thresholds, stage structure, UI, room setup, or question assets were changed.

## Runtime Flow Verification

The active room interview flow is `CybersecurityPresalesInterviewFlow`.

When no explicit inspector question bank is assigned, it loads ScriptableObject questions from:

`Assets/Resources/FinalRound/Questions/RC11`

The runtime interview stages are:

- Stage 1: Customer Context: 2 context/customer scenario questions.
- Stage 2: Technical Judgement: 2 technical/security judgement questions.
- Stage 3: Commercial Pressure: 2 commercial/executive pressure questions.
- Total runtime questions per run: 6.

Questions are selected without replacement within each category for the current run.

## Audit Alignment Finding

The P22 reported distribution was based on a short-run audit:

- 1 context/customer scenario question.
- 1 technical/security judgement question.
- 1 commercial/executive pressure question.
- 3 total questions.

That does not match the current runtime flow.

P23 adds a staged-run audit mode to `FinalRoundQuestionBankBalanceAudit` that simulates the real 6-question runtime pattern:

- 2 context/customer scenario questions.
- 2 technical/security judgement questions.
- 2 commercial/executive pressure questions.
- 6 total questions.

The existing short-run audit remains useful as a quick per-category content sanity check, but it should not be treated as the runtime outcome distribution.

## Audit Method

The staged-run audit uses a deterministic sampled simulation:

- Sample count: 100,000 staged runs.
- Fixed seed: 230023.
- Question selection: 2 questions without replacement from each category.
- Answer selection: one of 4 answers per selected question with uniform probability.
- Starting scores: Technical 4, Commercial 4, Rapport 4, Energy 6.
- Score clamping: applied after each answer, matching runtime `InterviewScore.Apply`.

Exhaustive simulation would cover 28 context pairs x 28 technical pairs x 28 commercial pairs x 4^6 answer paths, or 89,915,392 paths. The deterministic sample is used to keep the audit lightweight and reproducible.

## Validation Summary

- Source assets found: 24.
- Valid questions: 24.
- Context/customer scenario: 8.
- Technical/security judgement: 8.
- Commercial/executive pressure: 8.
- Structural validation warnings: 0.
- Dominant-answer warnings: 0.
- Limited-trade-off warnings: 0.

## Outcome Distribution For Real Runtime Flow

- Total sampled staged runs: 100,000.
- StrongPass: 33,616 (33.6%).
- Pass: 20,461 (20.5%).
- Hold: 40,960 (41.0%).
- Reject: 4,963 (5.0%).

## Score Distribution Summary

- Technical: average 8.25, min 0, max 10.
- Commercial: average 9.17, min 0, max 10.
- Rapport: average 5.84, min 0, max 10.
- Energy: average 4.26, min 0, max 10.

## Clamp And Saturation Notes

- Runs where at least one score delta was clamped during answer application: 78,390 (78.4%).
- Technical final score reached 10 in 42,961 runs (43.0%) and 0 in 133 runs (0.1%).
- Commercial final score reached 10 in 65,090 runs (65.1%) and 0 in 11 runs (0.0%).
- Rapport final score reached 10 in 13,462 runs (13.5%) and 0 in 4,820 runs (4.8%).
- Energy final score reached 10 in 567 runs (0.6%) and 0 in 3,201 runs (3.2%).

The main saturation issue is high-end Technical and especially Commercial scoring. The six-question runtime flow creates more chances to accumulate positive deltas than the three-question short-run audit.

## Recommendation

Tuning is recommended in a follow-up pass.

Do not change P23 content as part of this audit, but the real staged flow shows:

- StrongPass is much more common than P22 suggested.
- Reject is still reachable, but less common.
- Hold remains the most common mixed-performance result.
- Energy still matters because low Energy blocks Pass and contributes to Reject/Hold outcomes.
- Technical and Commercial scores frequently saturate at 10, especially Commercial.

Suggested follow-up direction:

- Review outcome thresholds against the six-question runtime length.
- Consider whether StrongPass should require stronger Energy or Rapport support.
- Consider whether score deltas should be normalized for a 6-question run.
- Keep the question wording and P22 content stable until the threshold/delta model is reviewed intentionally.

No automatic tuning changes were applied.
