# Final Round - Prototype P30: Run-State Modifiers into The Room

## Summary

P30 makes the existing VS2 `CandidateState` lightly affect VS1: The Room.

The goal is not to retune the interview. The goal is to make earlier Desk choices follow the player into the final interview in small, readable ways.

P30 does not change:

- question wording;
- question assets;
- answer score deltas;
- outcome thresholds;
- six-question stage structure;
- room art;
- character setup.

Direct `InterviewRoom` play remains neutral if there is no active `FinalRoundRunState`.

## New Resolver

P30 adds:

`Assets/Scripts/RoomModifierResolver.cs`

It maps `CandidateState` into `RoomModifierResult`.

`RoomModifierResult` contains:

- `technicalStartModifier`
- `commercialStartModifier`
- `rapportStartModifier`
- `energyStartModifier`
- `recruiterWarmth`
- `architectPressure`
- `introTone`
- `reactionWarmthModifier`
- `outcomeEmailContextLine`
- `debugSummary`

## CandidateState Fields Used

P30 reads:

- Role Fit
- Recruiter Trust
- Candidate Confidence
- Energy
- Overclaim Risk
- Technical Readiness
- Rapport Momentum
- selected job/application/recruiter IDs for debug context only

## Mapping Rules

Numeric starting modifiers are intentionally small:

- normal range: `-1`, `0`, `+1`
- extreme combined state can reach `-2` or `+2`
- final starting modifiers are clamped to `-2..+2`
- score application still clamps Room scores to `0..10`

Mappings:

| CandidateState source | Room effect |
| --- | --- |
| Technical Readiness | Technical start modifier |
| Energy | Energy start modifier |
| Candidate Confidence | Rapport/Energy influence |
| Role Fit | Commercial start modifier, plus possible Rapport support |
| Rapport Momentum | Rapport start modifier and reaction warmth |
| Recruiter Trust | intro warmth and reaction warmth |
| Overclaim Risk | architect pressure and tone; only affects Technical start when combined with low Technical Readiness |

Overclaim Risk does not automatically punish the player. It mostly changes tone unless paired with weak technical readiness.

## Intro Tone Variants

Stage 1 can vary based on resolved tone:

- warm screen signal;
- limited signal from the screen;
- detail-pressure after strong earlier claims;
- clear early-screen momentum.

Stage 2 can become sharper when `architectPressure` is active:

`I want to test the technical detail behind the earlier positioning.`

The variants are intentionally written as panel dialogue, not stat explanations.

## Reaction Warmth

Recruiter Trust and Rapport Momentum can slightly bias neutral reactions:

- high warmth: a mild Hiring Manager nod;
- low warmth: more guarded quiet note-taking.

High Overclaim Risk plus weak technical answer can make the Architect response more concerned.

No scores are shown.

## Outcome Email Context Line

The outcome email can gain one optional context line:

- high recruiter trust:
  `The early screen gave the panel a useful starting point.`
- high overclaim risk with weak readiness:
  `The panel noted some gaps between early positioning and scenario depth.`
- high role fit:
  `The role alignment remained a positive signal throughout the process.`
- low energy:
  `The panel felt the later stages lost some momentum.`

The rest of the email system remains unchanged.

## Debug Support

F1 debug now includes:

- active CandidateState status;
- resolved RoomModifierResult summary;
- starting score modifiers;
- intro tone;
- architect pressure;
- reaction warmth;
- outcome context line.

The Room also logs the resolved modifier result when an active CandidateState exists.

## Fallback Behaviour

If `InterviewRoom` starts directly:

- no active CandidateState is found;
- `RoomModifierResolver` is not applied;
- starting scores remain VS1 neutral;
- stage intros, reactions, and email context remain neutral.

## How To Test

1. Start from the main menu.
2. Click `Start Job Search`.
3. Open the laptop.
4. Choose different application strategies.
5. Complete Maya's recruiter screen with different reply styles.
6. Continue to `InterviewRoom`.
7. Open F1 debug and confirm:
   - active CandidateState is detected;
   - Room modifiers are listed;
   - starting modifiers are small.
8. Confirm intro tone changes subtly.
9. Answer through the interview and confirm the outcome email still appears.
10. Open `InterviewRoom` directly and confirm neutral fallback behaviour.

## Intentionally Deferred

- changing Room outcome thresholds;
- changing question selection based on Desk state;
- harder question banks for overclaiming;
- larger score normalisation;
- return-to-desk inbox loop;
- visible stat explanation to the player;
- full recruiter/hiring-manager branching.

## Recommended P31 Prompt

`Final Round - Prototype P31: Return-to-Desk Outcome / Inbox`

Suggested scope:

- return from Room outcome to Desk;
- show inbox outcome summary;
- preserve CandidateState and roomOutcome;
- support restart/new run path;
- keep scoring and question data unchanged.

## Validation

Run:

- `dotnet build "Assembly-CSharp.csproj"`
- `dotnet build "Assembly-CSharp-Editor.csproj"`

Both should pass with 0 warnings and 0 errors.
