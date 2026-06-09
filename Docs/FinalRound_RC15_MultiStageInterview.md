# Final Round Prototype v1.0 RC15 Multi-Stage Interview

## Structure

RC15 turns the seated interview from a 3-question flow into a 6-question staged panel interview.

Stages:

1. `Stage 1: Customer Context`
   - Lead interviewer: Hiring Manager
   - Category: `ContextCustomerScenario`
   - Questions selected: 2
2. `Stage 2: Technical Judgement`
   - Lead interviewer: Principal Security Architect
   - Category: `TechnicalSecurityJudgement`
   - Questions selected: 2
3. `Stage 3: Commercial Pressure`
   - Lead interviewer: Sales Director
   - Category: `CommercialExecutivePressure`
   - Questions selected: 2

Total questions per run: 6.

## Runtime Behavior

- Each stage begins with a short transition line.
- Stage intro pause is configurable with `stageIntroPauseDuration`.
- Final outcome pause is configurable with `finalOutcomePauseDuration`.
- The question panel shows the current stage and progress, such as `Question 2 of 6`.
- Existing answer scoring, judgement reactions, outcome email, scorecard, forced outcomes, and reset behavior are preserved.
- Scores remain hidden until the scorecard.

## Deterministic Seeds

Question selection uses one `System.Random` seeded by the current deterministic seed.

The selection order is:

1. Pick 2 unique context questions.
2. Pick 2 unique technical questions.
3. Pick 2 unique commercial questions.

Within each stage, selected questions are removed from that category's temporary pool, so duplicates are avoided within the run.

If a category has fewer valid questions than requested, the flow logs a warning and selects as many as possible.

## Debug Updates

- Debug panel label is updated to RC15.
- Seed preset buttons are now generic: `Preset Seed 1` through `Preset Seed 4`.
- The end-of-run console summary logs all 6 question IDs and all selected answer indexes.
- Editor helper updated to `Final Round > Find RC15 Seed Presets`.

## Test Checklist

- Start the room scene.
- Sit at the interview chair.
- Confirm Stage 1 transition appears.
- Answer 2 customer context questions.
- Confirm Stage 2 transition appears.
- Answer 2 technical judgement questions.
- Confirm Stage 3 transition appears.
- Answer 2 commercial pressure questions.
- Confirm each question shows progress from `Question 1 of 6` through `Question 6 of 6`.
- Confirm reactions still appear after each answer.
- Confirm the outcome email appears after the final reaction and final pause.
- Confirm scorecard opens.
- Press `R` and confirm the run resets.
- Open `F1` debug tools, enable deterministic seed, restart, and confirm the same 6 question IDs repeat.
- Force each outcome from debug tools and confirm the outcome email updates.

## Known Limitations

- The question bank still has only 4 questions per category, so repeated playthroughs will reveal the full bank quickly.
- Stage intros use text-only transitions, not animations or voice.
- RC14 outcome seed presets no longer map directly to outcomes because the run now has 6 questions.
