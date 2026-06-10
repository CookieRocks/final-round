# Final Round - Prototype P31: Return-to-Desk Outcome / Inbox

## Summary

P31 closes the first connected VS2 loop:

```text
Desk -> application strategy -> recruiter screen -> Room -> outcome email -> Desk inbox
```

The goal is to make the Desk feel like a hub that receives the consequence of the final interview, without changing the VS1 Room scoring model or question content.

P31 does not change:

- Room question assets;
- answer score deltas;
- outcome thresholds;
- six-question stage structure;
- Room art;
- interviewer character setup;
- Room question UI layout.

Direct `InterviewRoom` play remains protected. If the Room starts without an active `FinalRoundRunState`, the return-to-Desk action is hidden and the Room behaves as neutral VS1.

## What Changed

### CandidateState

`CandidateState` now preserves:

- `roomOutcome`
- `roomModifierSummary`

`roomOutcome` is written when the Room outcome email is shown.

`roomModifierSummary` is written when P30 Room modifiers are resolved, so the Desk can show a lightweight process summary after the run.

### Room Outcome Screen

The outcome email keeps:

- `View Scorecard`
- `Restart`

When the Room was launched from an active Desk run, it now also shows:

- `Return to Desk`

Clicking `Return to Desk` loads `DeskScene` and preserves the active `CandidateState`.

If no active Desk run exists, `Return to Desk` is hidden.

### Desk Inbox

When `DeskScene` loads with a completed `CandidateState.roomOutcome`, the laptop panel opens directly to:

`Northbridge Mail / Inbox`

The inbox message is from Maya Patel / Northbridge Recruiting and varies by final Room outcome:

| Outcome | Desk inbox tone |
| --- | --- |
| StrongPass | Warm positive next step |
| Pass | Positive but measured follow-up |
| Hold | Ambiguous internal alignment |
| Reject | Polite but closed rejection |

The message can include one subtle CandidateState context line, such as:

- high overclaim risk: gaps between early positioning and scenario depth;
- high recruiter trust: early screen created a positive starting point;
- low energy: later stages lost momentum;
- high role fit: role alignment remained positive.

### Desk Post-Outcome Actions

After the outcome returns to Desk, the laptop panel offers:

- `View Listing`
- `Process Summary`
- `Start New Run`
- `Main Menu`

`Process Summary` shows the selected job, application choice, recruiter path, recruiter responses, Room outcome, CandidateState values, and Room modifier summary.

`Start New Run` clears `CandidateState` and resets the Desk flow.

`Main Menu` clears the active run and loads `InterviewRoom`, where the existing main menu remains available.

## How To Test

1. Start from the main menu.
2. Click `Start Job Search`.
3. Open the laptop with `E`, `Space`, or mouse click.
4. Review the listing.
5. Choose and confirm an application strategy.
6. Complete the Maya Patel recruiter screen.
7. Click `Continue to Interview`.
8. Complete or debug-skip the Room interview.
9. Confirm the outcome email appears.
10. Confirm `Return to Desk` appears alongside `View Scorecard` and `Restart`.
11. Click `Return to Desk`.
12. Confirm `DeskScene` opens directly to `Northbridge Mail / Inbox`.
13. Confirm the inbox copy matches the Room outcome.
14. Confirm `Process Summary` shows the Desk choices and Room outcome.
15. Confirm `Start New Run` resets the Desk.
16. Open `InterviewRoom` directly and confirm neutral VS1 fallback still works.

## Intentionally Deferred

- Multiple inbox messages.
- Waiting-time mechanics.
- Job-search calendar or pipeline board.
- Offer negotiation.
- A full post-outcome emotional recovery stage.
- Multiple jobs or companies.
- More detailed score normalization.
- Visual Desk art pass.

## Known Limitations

- The Desk inbox is still a prototype laptop panel, not a full email client.
- The outcome copy is deterministic by Room outcome and a small CandidateState context rule.
- `Main Menu` returns through the existing `InterviewRoom` scene because there is not a separate menu scene yet.
- Desk UI remains runtime-generated and functional rather than final-art.

## Recommended P32 Prompt

`Final Round - Prototype P32: VS2 Readiness Review`

Suggested scope:

- review the full Desk -> Room -> Desk loop;
- confirm direct VS1 Room remains protected;
- classify VS2 blockers and polish items;
- decide whether The Desk is ready to promote to VS2;
- avoid new content unless a blocker appears.

## Validation

Run:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```
