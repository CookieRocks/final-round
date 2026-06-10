# Final Round - Prototype P26: VS2 Desk Architecture Skeleton

## Summary

P26 creates the safe architecture skeleton for `VS2: The Desk` without implementing the full job listing, recruiter exchange, application flow, or room modifiers.

The intent is to prove that a future Desk scene can create a small persistent run state, transition into `InterviewRoom`, and let the existing Room detect that state while still working exactly as a standalone VS1 scene.

## Source Files Read

- `FinalRound_GamePlan_Current.md`
- `Docs/FinalRound_VS2_TheDesk_Planning.md`
- `Docs/FinalRound_VS1_TheRoom.md`
- `Docs/FinalRound_P25_VS1ReadinessReview.md`
- `Assets/Scripts/CybersecurityPresalesInterviewFlow.cs`
- `Assets/Scripts/InterviewGameManager.cs`
- `Assets/Scripts/AnswerOptionData.cs`

## What Was Added

### CandidateState

`CandidateState` is a serializable runtime state object for one job-opportunity run.

Fields:

| Field | Purpose |
| --- | --- |
| `selectedJobId` | The job listing selected at the Desk. |
| `applicationChoiceId` | The application strategy selected later. |
| `recruiterPathId` | The recruiter branch/path selected later. |
| `roomOutcome` | The final Room outcome recorded after the interview. |
| `hasActiveDeskRun` | Whether this state came from an active Desk run. |
| `roleFit` | Early fit signal for the opportunity. |
| `recruiterTrust` | Recruiter confidence in the candidate. |
| `candidateConfidence` | How steady the candidate is entering later stages. |
| `energy` | Stamina carried into the process. |
| `overclaimRisk` | Risk created by overstating experience or fit. |
| `technicalReadiness` | Preparation for technical/security judgement. |
| `rapportMomentum` | Warmth carried from earlier communication. |

Numeric convention:

- `0` is neutral.
- Prototype modifiers clamp from `-3` to `+3`.
- No score or outcome thresholds are changed in P26.

`CandidateState.BuildDebugSummary()` provides a compact log-friendly state readout.

### FinalRoundRunState

`FinalRoundRunState` is a lightweight persistent singleton MonoBehaviour.

It:

- survives scene loads with `DontDestroyOnLoad`;
- avoids duplicate singleton instances;
- supports `CreateNeutralRun()`;
- supports `ResetRun()`;
- supports `HasActiveRun()`;
- exposes the current `CandidateState`;
- is safe when `InterviewRoom` is launched directly, because the Room does not create or require it.

### VS2 Data Model Stubs

P26 adds Unity-friendly ScriptableObject/data classes for future authoring:

- `JobListingData`
- `ApplicationChoiceData`
- `RecruiterMessageData`
- `RecruiterScreenQuestionData`
- `RoomModifierData`

These are stubs only. No content assets were created in P26.

### DeskPrototypeController

`DeskPrototypeController` is a placeholder controller for the future Desk scene.

Current responsibilities:

- track a simple Desk state enum;
- create a neutral CandidateState;
- optionally attach a default job ID;
- transition to `InterviewRoom` by configurable scene name;
- reset the Desk run;
- log debug summaries.

It does not build the full Desk UI, job listing flow, recruiter screen, prep choice, or return-to-desk loop yet.

## Scene Architecture Decision

`DeskScene.unity` was deferred.

Reason:

- creating Unity scene YAML by hand is risky;
- P26 can validate the architecture through scripts and docs without scene churn;
- the next Unity editor pass can create the scene through normal editor mechanisms.

Recommended scene shape for P27:

1. Create `Assets/Scenes/DeskScene.unity` in Unity.
2. Add an empty `Desk Prototype Controller` GameObject.
3. Attach `DeskPrototypeController`.
4. Set `interviewRoomSceneName` to `InterviewRoom`.
5. Add both `DeskScene` and `InterviewRoom` to Build Settings.
6. Add a temporary button or debug key that calls `BeginDeskRun()`.
7. Add a temporary button or debug key that calls `GoToInterviewRoom()`.

## Room Fallback Behaviour

`CybersecurityPresalesInterviewFlow` now checks for an active `FinalRoundRunState` on startup.

If an active Desk run exists:

- the Room logs the CandidateState debug summary;
- the Room records the final outcome back to `CandidateState.roomOutcome`;
- no gameplay modifiers are applied yet.

If no active Desk run exists:

- the Room logs that it is using neutral VS1 direct-start behaviour;
- the interview proceeds as before.

This keeps `InterviewRoom` safe to launch directly from the editor.

## Debug Logging

P26 adds logging for:

- CandidateState creation;
- CandidateState reset;
- duplicate run-state cleanup;
- Desk run start;
- Desk-to-Room transition;
- Room detecting active CandidateState;
- Room fallback to neutral direct-start mode.

The F1 debug panel now includes a compact CandidateState line:

- active Desk run with job ID, or
- neutral direct-start fallback.

## Manual Unity Setup

No manual setup is required for existing `InterviewRoom` direct play.

Manual setup is needed before testing the Desk path in Unity:

1. Create `Assets/Scenes/DeskScene.unity`.
2. Add `DeskPrototypeController` to an empty GameObject.
3. Confirm `InterviewRoom` is the configured target scene.
4. Add `DeskScene` to Build Settings.
5. Wire temporary debug UI or editor buttons to:
   - `BeginDeskRun()`;
   - `GoToInterviewRoom()`;
   - `ResetDeskRun()`.

## How To Test

### Direct InterviewRoom fallback

1. Open `Assets/Scenes/InterviewRoom.unity`.
2. Enter Play Mode.
3. Start/sit as usual.
4. Confirm the console logs:
   - `Room started without active CandidateState; using neutral VS1 direct-start behavior.`
5. Confirm the Room flow is unchanged.

### CandidateState bridge after DeskScene exists

1. Open `DeskScene`.
2. Call `BeginDeskRun()`.
3. Call `GoToInterviewRoom()`.
4. Confirm the console logs:
   - CandidateState creation;
   - Desk-to-Room transition;
   - Room detected active CandidateState.
5. Complete or skip the Room interview.
6. Confirm `CandidateState.roomOutcome` is populated in debug output.

## Intentionally Deferred

- Full job listing UI.
- Recruiter message exchange.
- Application strategy selection.
- Prep/action choice.
- Return-to-desk inbox.
- Room score modifiers from CandidateState.
- Stage intro variants from CandidateState.
- Outcome email variants from CandidateState.
- Any scoring threshold changes.
- Any question wording, delta, or asset changes.
- Any room art, UI layout, or character changes.

## Recommended P27 Prompt

`Final Round - Prototype P27: Desk Scene Prototype`

Suggested scope:

- create `Assets/Scenes/DeskScene.unity` through Unity;
- add a simple desk/laptop composition;
- attach `DeskPrototypeController`;
- add temporary debug controls for `BeginDeskRun`, `GoToInterviewRoom`, and `ResetDeskRun`;
- keep job listing/recruiter content placeholder-only;
- verify direct `InterviewRoom` fallback still works;
- run `dotnet build "Assembly-CSharp.csproj"`.

## Validation

Run:

`dotnet build "Assembly-CSharp.csproj"`

P26 should pass with 0 warnings and 0 errors.
