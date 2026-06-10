# Final Round - Prototype P29: Recruiter Screen / Message Exchange

## Summary

P29 adds one short recruiter interaction to the Desk flow.

The Desk sequence is now:

1. Review the Northbridge Cyber Systems listing.
2. Choose and confirm an application strategy.
3. Complete Maya Patel's short recruiter screen.
4. Continue to the existing InterviewRoom.

VS1 remains protected. P29 does not apply Room modifiers, change Room scoring, change Room questions, alter outcome thresholds, or touch Room UI/art/character setup.

## Recruiter

- Name: Maya Patel
- Role: Senior Talent Partner
- Company: Northbridge Cyber Systems
- Tone: friendly, efficient, slightly vague, corporate

Maya thanks the player for applying, says the profile looks relevant, notes that the role is moving quickly, and asks three short screen questions before forwarding the profile to the panel.

## Recruiter Sequence

### Prompt 1: Availability / Energy

Question:

`Are you available for a short screen this week?`

| Choice | Deltas |
| --- | --- |
| Clear Availability | Recruiter Trust +2, Confidence +1, Rapport Momentum +1 |
| Immediately Free | Confidence +1, Energy -1 |
| Cautious Window | Recruiter Trust -1, Energy +1 |
| Slow Response | Recruiter Trust -2, Confidence -1, Energy -1, Rapport Momentum -1 |

### Prompt 2: Experience Positioning

Question:

`How would you summarise your fit for a security presales role like this?`

| Choice | Deltas |
| --- | --- |
| Balanced Fit | Role Fit +2, Recruiter Trust +2, Confidence +1, Technical Readiness +1, Rapport Momentum +1 |
| Technical Depth | Role Fit +1, Recruiter Trust +1, Confidence +1, Technical Readiness +2, Rapport Momentum -1 |
| Perfect Match | Role Fit +2, Recruiter Trust -1, Confidence +2, Overclaim Risk +2, Rapport Momentum -1 |
| Modest Fit | Role Fit -1, Recruiter Trust +1, Confidence -2, Overclaim Risk -1, Rapport Momentum +1 |

### Prompt 3: Process / Expectations

Question:

`The team may move quickly if there is alignment. Are you comfortable with a technical panel and final-round customer scenario?`

| Choice | Deltas |
| --- | --- |
| Realistic Yes | Role Fit +1, Recruiter Trust +2, Confidence +1, Technical Readiness +1, Rapport Momentum +1 |
| No Problem | Recruiter Trust -1, Confidence +2, Overclaim Risk +2, Rapport Momentum -1 |
| Clarify Format | Recruiter Trust +2, Confidence +1, Energy -1, Technical Readiness +2, Rapport Momentum +1 |
| Hesitant | Role Fit -1, Recruiter Trust -2, Confidence -2, Energy -1, Rapport Momentum -1 |

## CandidateState Behaviour

Recruiter responses apply small clamped deltas to:

- Role Fit
- Recruiter Trust
- Candidate Confidence
- Energy
- Overclaim Risk
- Technical Readiness
- Rapport Momentum

P29 also records:

- `recruiterResponseIds`: comma-separated selected recruiter reply IDs
- `recruiterPathId`: `MAYA-PATEL-SCREEN` after the final recruiter prompt

After the final prompt, the Desk shows:

`Maya has forwarded your profile to the panel.`

The interview button then changes from a debug/prototype shortcut to:

`Continue to Interview`

## UI Flow

The laptop panel now supports:

- `View Listing`
- `Application Strategy`
- `Confirm Application`
- `Recruiter Message`
- `Debug: Go To Interview` before recruiter completion
- `Continue to Interview` after recruiter completion
- `Reset Desk Run`

The recruiter screen reuses the Desk laptop panel rather than adding a separate scene or dialogue system.

## Content Assets

Runtime fallback recruiter content is built into `DeskPrototypeController`, so P29 works before asset generation.

Editor menu command:

`Final Round > Create/Repair P29 Recruiter Content Assets`

The command creates/repairs:

- `Assets/Resources/FinalRound/VS2/P29/MAYA-PATEL-INTRO.asset`
- `Assets/Resources/FinalRound/VS2/P29/REC-AVAILABILITY.asset`
- `Assets/Resources/FinalRound/VS2/P29/REC-FIT.asset`
- `Assets/Resources/FinalRound/VS2/P29/REC-PROCESS.asset`

It also assigns the recruiter content to `DeskPrototypeController` in `DeskScene` if the scene exists.

To avoid the P28 ScriptableObject binding issue, recruiter asset classes now live in their own files:

- `RecruiterMessageData.cs`
- `RecruiterScreenQuestionData.cs`

## How To Test

1. In Unity, run `Final Round > Create/Repair P29 Recruiter Content Assets`.
2. Open `Assets/Scenes/DeskScene.unity`.
3. Enter Play Mode.
4. Press `E`, press `Space`, or click the laptop.
5. Inspect the listing.
6. Choose an application strategy.
7. Confirm the application.
8. Click `Recruiter Message`.
9. Answer all three recruiter screen prompts.
10. Confirm the debug/readout or console shows CandidateState changes.
11. Confirm the interview button changes to `Continue to Interview`.
12. Continue to `InterviewRoom`.
13. Confirm the Room logs active CandidateState detection.
14. Open `InterviewRoom` directly and confirm neutral VS1 fallback still works.

## Intentionally Deferred

- Full branching recruiter dialogue tree.
- Free text replies.
- AI dialogue.
- Salary negotiation.
- Recruiter call/video presentation.
- Prep/action choice.
- Applying CandidateState modifiers to Room scoring or copy.
- Return-to-desk inbox outcome.

## Recommended P30 Prompt

`Final Round - Prototype P30: Apply Run-State Modifiers to The Room`

Suggested scope:

- read CandidateState in InterviewRoom;
- apply small starting score modifiers;
- add warm/cold stage intro variants;
- add debug display of carried Desk state;
- keep P24/P25 outcome thresholds unchanged;
- audit whether modifiers distort outcome distribution.

## Validation

Run:

- `dotnet build "Assembly-CSharp.csproj"`
- `dotnet build "Assembly-CSharp-Editor.csproj"`

Both should pass with 0 warnings and 0 errors.
