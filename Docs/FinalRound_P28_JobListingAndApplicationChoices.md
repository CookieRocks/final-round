# Final Round - Prototype P28: Job Listing and Application Choices

## Summary

P28 replaces the P27 placeholder Desk listing with an authored Northbridge Cyber Systems role and adds the first application strategy choices that modify `CandidateState`.

VS1 remains protected:

- no Room scoring changes;
- no Room question changes;
- no outcome threshold changes;
- no Room UI, art, character, or stage-structure changes;
- no CandidateState modifiers are applied to the Room yet.

## What Was Added

### Authored Listing Content

The Desk now presents a single opportunity:

- Company: `Northbridge Cyber Systems`
- Role: `Senior Solutions Engineer - Security Presales`
- Job ID: `NCS-SE-001`

The listing includes:

- Overview
- Responsibilities
- Requirements
- Nice-to-haves
- Salary / Process
- Red flags
- Green flags

The content is realistic, corporate, slightly ambiguous, and focused on cybersecurity technical presales.

### Application Strategy Choices

P28 adds four application strategies:

| Choice | Intent | Deltas |
| --- | --- | --- |
| Honest Fit | Grounded application that admits growth areas. | Role Fit +0, Recruiter Trust +1, Confidence -1, Energy +0, Overclaim Risk -2, Technical Readiness +1, Rapport Momentum +1 |
| Tailored Credible | Tightly maps real experience to the listing. | Role Fit +2, Recruiter Trust +2, Confidence +1, Energy -1, Overclaim Risk +1, Technical Readiness +1, Rapport Momentum +1 |
| Aggressive | Presents as a near-perfect fit. | Role Fit +2, Recruiter Trust -1, Confidence +2, Energy -1, Overclaim Risk +2, Technical Readiness +0, Rapport Momentum -1 |
| Quick Apply | Preserves energy but gives low signal. | Role Fit -2, Recruiter Trust -1, Confidence +0, Energy +2, Overclaim Risk -1, Technical Readiness -1, Rapport Momentum -1 |

All deltas are small and clamp through the existing `CandidateState` convention of `-3` to `+3`.

## CandidateState Behaviour

When the player confirms an application strategy:

1. `applicationChoiceId` is set.
2. The strategy deltas are applied to CandidateState.
3. CandidateState clamps values through its existing setters.
4. The Desk debug readout updates.
5. Feedback text confirms the application was submitted.
6. A CandidateState summary is logged.

Room modifiers are intentionally still disabled. P30 is the planned milestone for applying CandidateState to the Room.

## Desk UI Flow

The laptop panel now supports:

- `View Listing`
- `Choose Application Strategy`
- strategy selection buttons
- `Confirm Application`
- `Go To Interview Room (Prototype Shortcut)`
- `Reset Desk Run`

After confirmation, the panel shows:

`Application submitted.`

The interview-room shortcut remains available for testing the Desk-to-Room bridge.

## Main Menu Entry

P28 adds a low-risk main menu button:

`Start Job Search`

It loads `DeskScene`.

The existing direct Room entry remains available:

`Start Interview Process`

This preserves direct InterviewRoom testing.

## ScriptableObject Assets

P28 adds an editor menu command to create/repair the authored assets safely:

`Final Round > Create/Repair P28 Desk Content Assets`

The command creates:

- `Assets/Resources/FinalRound/VS2/P28/NCS-SE-001_JobListing.asset`
- four `ApplicationChoiceData` assets under the same folder

It also assigns those assets to `DeskPrototypeController` in `DeskScene` if the scene exists.

Runtime fallback content is built into `DeskPrototypeController`, so the Desk remains playable even before the assets are generated.

Note: the local batchmode attempt could not run while the Unity editor had the project open. Run the menu command in Unity to generate the actual assets.

## How To Test

### Desk flow

1. In Unity, run `Final Round > Create/Repair P28 Desk Content Assets`.
2. Open `Assets/Scenes/DeskScene.unity`.
3. Enter Play Mode.
4. Press `E`, press `Space`, or click the laptop.
5. Click `View Listing`.
6. Confirm all listing sections are readable.
7. Click `Choose Application Strategy`.
8. Select each strategy and inspect the feedback/delta text.
9. Confirm one strategy.
10. Confirm CandidateState debug values change.
11. Click `Go To Interview Room (Prototype Shortcut)`.
12. Confirm `InterviewRoom` loads and logs active CandidateState detection.

### Main menu flow

1. Open the normal starting scene.
2. Enter Play Mode.
3. Click `Start Job Search`.
4. Confirm `DeskScene` loads.
5. Return to main menu/direct Room testing separately with `Start Interview Process`.

### Direct Room fallback

1. Open `Assets/Scenes/InterviewRoom.unity` directly.
2. Enter Play Mode.
3. Confirm neutral fallback is logged.
4. Confirm VS1 Room flow still works.

## Intentionally Deferred

- Recruiter dialogue.
- Recruiter screen/message exchange.
- Prep/action choice.
- Return-to-desk inbox.
- Applying CandidateState modifiers to Room scoring.
- Stage intro or outcome email variants driven by CandidateState.
- Multiple jobs.
- Job board.
- CV editor.
- Full Desk art pass.

## Recommended P29 Prompt

`Final Round - Prototype P29: Recruiter Screen / Message Exchange`

Suggested scope:

- add one recruiter contact for Northbridge Cyber Systems;
- create one short recruiter message thread or screen;
- add response choices that modify Recruiter Trust, Role Fit, Energy, Overclaim Risk, and Rapport Momentum;
- keep Room modifiers disabled until P30;
- preserve direct InterviewRoom fallback;
- run runtime and editor builds.

## Validation

Run:

- `dotnet build "Assembly-CSharp.csproj"`
- `dotnet build "Assembly-CSharp-Editor.csproj"`

Both should pass with 0 warnings and 0 errors.
