# Final Round - Room Prototype P24 Six-Question Outcome Tuning

## Scope

P24 tunes the outcome threshold model for the real six-question room interview flow.

No question assets, question wording, score deltas, UI, room setup, character setup, or stage structure were changed.

## Runtime Flow

The active runtime interview remains:

- Stage 1: Customer Context: 2 questions.
- Stage 2: Technical Judgement: 2 questions.
- Stage 3: Commercial Pressure: 2 questions.
- Total runtime questions per run: 6.

## Audit Method

Threshold candidates were tested against the same deterministic staged-run sample used for P23:

- Sample count: 100,000 staged runs.
- Fixed seed: 230023.
- Question selection: 2 questions without replacement from each category.
- Answer selection: uniform random selection from each question's 4 answers.
- Score starts: Technical 4, Commercial 4, Rapport 4, Energy 6.
- Score clamping: applied after each answer, matching runtime `InterviewScore.Apply`.

This was an audit-only threshold simulation first. Question assets were not edited.

## Before Distribution

P23/current-before tuning:

- StrongPass: 33,616 (33.6%).
- Pass: 20,461 (20.5%).
- Hold: 40,960 (41.0%).
- Reject: 4,963 (5.0%).

Issue: StrongPass was too common, Pass was too low, and Reject was too rare for the six-question flow.

## Candidate Models Tested

| Candidate | StrongPass | Pass | Hold | Reject | Notes |
| --- | ---: | ---: | ---: | ---: | --- |
| P23 current | 33.6% | 20.5% | 41.0% | 5.0% | Too generous for StrongPass. |
| A balanced SP | 10.1% | 45.5% | 36.4% | 8.0% | StrongPass fixed, Pass slightly above target. |
| B stricter SP total | 9.4% | 46.2% | 36.4% | 8.0% | StrongPass below target, Pass above target. |
| C pass needs Rapport | 9.4% | 41.3% | 41.3% | 8.0% | Hold above target, StrongPass low. |
| D hold 21 | 9.4% | 46.2% | 34.0% | 10.4% | Reject improved, but Pass high and StrongPass low. |
| J selected shape | 10.1% | 44.3% | 37.6% | 8.0% | All target bands met. |
| K selected final | 10.1% | 44.3% | 35.1% | 10.4% | All target bands met, stronger Reject presence. |

## Selected Threshold Model

P24 applies candidate K.

StrongPass now requires:

- Total score at least 32.
- Technical at least 8.
- Commercial at least 8.
- Rapport at least 7.
- Energy at least 6.
- No major weakness: all dimensions at least 6.

Pass now requires:

- Total score at least 23.
- Technical at least 5.
- Commercial at least 5.
- Energy at least 4.

Reject catches:

- Rapport at 1 or lower while Energy is 2 or lower.
- Any final total below 21.

Hold is the remaining mixed-performance band:

- Total at least 21, after StrongPass and Pass checks fail, unless the severe Rapport + Energy weakness rule applies.

## After Distribution

P24 selected model:

- StrongPass: 10,142 (10.1%).
- Pass: 44,327 (44.3%).
- Hold: 35,134 (35.1%).
- Reject: 10,397 (10.4%).

All four outcomes remain reachable and all target bands are met:

- StrongPass target: 10-18%.
- Pass target: 35-45%.
- Hold target: 28-38%.
- Reject target: 8-15%.

## Score Distribution

Threshold tuning does not change score generation, only outcome classification.

Before and after score distribution therefore remains:

- Technical: average 8.25, min 0, max 10.
- Commercial: average 9.17, min 0, max 10.
- Rapport: average 5.84, min 0, max 10.
- Energy: average 4.26, min 0, max 10.

Saturation remains:

- Runs with at least one score clamp: 78,390 (78.4%).
- Technical reached 10 in 42,961 runs (43.0%).
- Commercial reached 10 in 65,090 runs (65.1%).
- Rapport reached 10 in 13,462 runs (13.5%).
- Energy reached 10 in 567 runs (0.6%).

## Saturation Assessment

Threshold tuning is enough to hit the P24 outcome distribution targets.

Saturation remains a design concern, especially for Commercial and Technical. P24 intentionally avoids a scoring-system refactor. A future P25 score-normalisation pass is still recommended if the prototype needs more nuanced scorecard behaviour.

Possible P25 directions:

- Raw score accumulator separate from displayed 0-10 score.
- Delayed clamping until final outcome.
- Per-question average score model.
- Wider internal score range with scorecard normalisation.
- Delta scaling for six-question runs.

## Decision

Apply threshold changes only.

Question asset changes were avoided. The P22 question bank remains untouched.
