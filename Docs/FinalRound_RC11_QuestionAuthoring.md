# Final Round Prototype v1.0 RC11 Question Authoring

## Data Structure

RC11 moves the active cybersecurity presales question bank into Unity assets.

- `QuestionCategory` defines the three run slots:
  - `ContextCustomerScenario`
  - `TechnicalSecurityJudgement`
  - `CommercialExecutivePressure`
- `InterviewQuestionData` is a ScriptableObject question asset.
- `AnswerOptionData` is a ScriptableObject answer asset, stored as subassets inside each migrated question asset.

Each `InterviewQuestionData` includes:

- Question ID
- Category
- Speaker name/title
- Question text
- 4 answer options

Each `AnswerOptionData` includes:

- Answer text
- Technical delta
- Commercial delta
- Rapport delta
- Energy delta

## Asset Folder Structure

Runtime-loaded question assets live here:

`Assets/Resources/FinalRound/Questions/RC11`

The current migrated bank contains 12 questions:

- `CTX-01` through `CTX-04`
- `TECH-01` through `TECH-04`
- `COMM-01` through `COMM-04`

## Loading Behaviour

`CybersecurityPresalesInterviewFlow` first tries to use its assigned `questionBank` array.

If that array is empty, it loads:

`Resources.LoadAll<InterviewQuestionData>("FinalRound/Questions/RC11")`

If no valid ScriptableObject questions are available, the flow falls back to the built-in sample questions so the demo remains playable.

## How Random Selection Works

Each run selects one question from each category:

1. Context/customer scenario
2. Technical/security judgement
3. Commercial/executive pressure

Deterministic seed mode is preserved. With the same deterministic seed and the same question bank, the same three question IDs should be selected.

## How To Add A New Question

1. In Unity, create a new `InterviewQuestionData` asset using `Create > Final Round > Interview Question`.
2. Set a unique question ID, such as `TECH-05`.
3. Choose one `QuestionCategory`.
4. Fill in the speaker name/title and question text.
5. Create four `AnswerOptionData` assets using `Create > Final Round > Answer Option`.
6. Fill in the answer text and four score deltas.
7. Assign the four answer assets to the question asset's answer options array.
8. Put the question asset under `Assets/Resources/FinalRound/Questions/RC11`, or assign it directly to the `questionBank` array on `CybersecurityPresalesInterviewFlow`.

For hand-authored assets, keeping answer options as subassets under the question asset is tidier, but standalone answer assets also work.

## How To Validate The Question Bank

Validation happens when the interview flow builds its runtime pools.

A question is skipped if:

- Question ID is empty.
- Speaker name is empty.
- Question text is empty.
- It does not have exactly 4 answer options.
- Any answer option is missing.
- Any answer text is empty.

The Unity console logs warnings for skipped invalid assets. The bank is considered playable when at least one valid question exists in each category.

## Playtest

1. Start the `InterviewRoom` scene.
2. Press `F1`.
3. Enable deterministic seed mode.
4. Note the current seed and question IDs.
5. Complete or restart the run.
6. Confirm the selected question IDs are stable for the same seed.
7. Temporarily move or rename the `RC11` Resources folder and confirm the built-in fallback still starts the interview.

## Known Limitations

- The migrated `.asset` files are intentionally lightweight and keep the current prototype wording/scoring.
- No custom editor exists yet for bulk validation or CSV import.
- The runtime still selects exactly one question from each category.
- Question order remains fixed by category, not by an authored sequence.
