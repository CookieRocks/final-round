# Final Round - Prototype P40: Post-Aftermath Desk Choices

## Summary

P40 adds a small post-aftermath Desk choice moment after `Final Round - VS3: The Aftermath`.

After a Reject outcome and completed aftermath, the Desk now acknowledges that the player has cleared the room and offers a lightweight closure choice. This is not a new campaign system. It is a small emotional beat before the player starts again, reviews the run, or sits with the rejection.

## What Changed

Updated runtime script:

- `Assets/Scripts/DeskPrototypeController.cs`

Added documentation:

- `Docs/FinalRound_P40_PostAftermathDeskChoices.md`

Updated plan:

- `FinalRound_GamePlan_Current.md`

P40 also adds a small Desk Escape menu because the Desk scene had no pause/exit overlay.

## Post-Aftermath Desk Message

When `AftermathCompleted` is true, the Desk feedback area includes:

```text
The room is quieter now. The rejection is still there, but it no longer fills the screen.
```

The message appears as part of the Desk inbox/process area after returning from `AftermathRoom`.

## Choices Added

When aftermath is completed, a new post-aftermath row appears with:

- `Apply Again`
- `Take a Break`
- `Ask for Feedback`
- `Review Summary`

Existing `Start New Run` remains available through the existing reset/start-new-run button.

## Desk Escape Menu

Press `Esc` in the Desk scene to open a small pause menu.

Options:

- `Resume`
- `Main Menu`
- `Start New Run`

This does not replace existing Desk buttons. It gives the player a consistent way out of the Desk without hunting for the visible button row.

## Choice Behaviour

### Apply Again

Resets the current run and opens the Desk listing flow again.

Player-facing line:

```text
You set this process down and start again a little steadier.
```

### Take a Break

Applies a small clamped recovery:

- Energy `+1`

Player-facing line:

```text
You step away from the laptop. Nothing is solved, but your energy returns a little.
```

### Ask for Feedback

Shows a realistic lightweight response without starting a new recruiter branch.

Player-facing line:

```text
Maya says she'll ask the panel, but can't promise detailed feedback.
```

### Review Summary

Opens the existing Process Summary.

## CandidateState Effects

P40 keeps stat effects deliberately small.

- `Take a Break`: Energy `+1`, clamped by `CandidateState`.
- `Apply Again`: resets/prepares a fresh run through the existing Desk reset/start flow.
- `Ask for Feedback`: no stat change.
- `Review Summary`: no stat change.

P40 does not change the Room outcome retroactively.

## Safety And Scope

P40 does not add:

- new jobs;
- new recruiter content;
- calendar systems;
- save/load campaign systems;
- mental-health mechanics;
- new Room content;
- Room scoring changes;
- question data changes;
- Aftermath destruction changes;
- new effects or assets.

## How To Test

1. Start from the main menu.
2. Choose `Start Job Search`.
3. Complete Desk, recruiter, and Room flow.
4. Achieve or force a `Reject` outcome.
5. Return to Desk.
6. Choose `Clear the Room`.
7. Process symbolic objects in `AftermathRoom`.
8. Return to Desk.
9. Confirm the quieter-room message appears.
10. Confirm post-aftermath choices appear.
11. Click `Take a Break` and confirm Energy increases by `+1` in debug/state summary.
12. Click `Ask for Feedback` and confirm Maya's lightweight feedback line appears.
13. Click `Review Summary` and confirm the existing Process Summary opens.
14. Click `Apply Again` and confirm a fresh Desk listing run begins.
15. Confirm existing `Start New Run` remains available.
16. Confirm non-Reject outcomes do not show `Clear the Room` or post-aftermath choices.
17. Press `Esc` in the Desk and confirm Resume, Main Menu, and Start New Run are available.

## Intentionally Deferred

- Full feedback-request branch.
- Real recruiter follow-up content.
- Calendar/waiting consequences.
- Multiple new opportunities.
- Long-term recovery systems.
- Larger Desk UI redesign.
- Richer Apply Again run selection.

## Validation

Run:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

Validation result:

- `dotnet build "Assembly-CSharp.csproj"`: passed, 0 warnings, 0 errors.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: passed, 0 warnings, 0 errors.
