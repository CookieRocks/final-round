# Final Round - Prototype P34: First Playtest Feedback Pass

## Summary

P34 responds to the first external VS2 playtest feedback without expanding content or changing the Room scoring model.

The pass keeps the promoted VS2 loop intact:

```text
Main Menu
  -> The Desk
  -> Northbridge Jobs listing
  -> Application Strategy
  -> Maya Patel recruiter screen
  -> The Room
  -> Outcome email
  -> Return to Desk
  -> Northbridge Mail / Inbox
  -> Process Summary / Start New Run
```

## Playtest Feedback Summary

Positive feedback:

- Player understood the goal: review job ads and choose one to apply for.
- Job listing sections felt useful.
- Application choices felt meaningfully different.
- Maya felt like a real recruiter emailing them.
- The Room felt tense and evaluative.
- Return-to-Desk outcome made sense.
- Nothing felt slow or broadly confusing.

Issues addressed in this pass:

- Early Desk/recruiter choices did not obviously feel remembered in The Room.
- First-time Room entry needed clearer guidance.
- Room mouse look sensitivity felt too high.
- Bottom reaction/description text disappeared too quickly.

## What Changed

### Carry-Through Visibility

The Room now surfaces early Desk and recruiter choices more clearly through natural panel/context text rather than raw stat explanations.

Changes include:

- Desk recruiter completion now says Maya forwards the profile, attaches application notes, and schedules the final round.
- Desk-to-Room entry objective now says the panel has Maya's screening notes.
- Stage 1 intro variants now more explicitly reference Maya's screen or application notes.
- High overclaim/pressure paths now make the Architect testing line more directly reference earlier positioning.

Normal play still does not show CandidateState values, stat names, or numeric deltas.

### Room Guidance

Initial Room objective text is now more explicit:

```text
Find the highlighted chair and press E to sit.
```

Desk-launched Room runs add a short contextual lead-in:

```text
Maya has forwarded your profile. The panel has her screening notes. Find the highlighted chair and press E to sit.
```

Guidance still clears after sitting/interview start.

### Mouse Sensitivity

The Room first-person controller default mouse sensitivity was reduced from `1.4` to `0.85`.

This affects only pre-sit Room look. Desk UI mouse interaction and seated interview answer selection are unchanged.

The value is serialized on `SimpleFirstPersonWalkController` so it can be tuned in the Inspector.

### Reaction Text Timing

Reaction and stage-intro text now stays visible longer:

- Stage intro pause: `3.5s`.
- Positive reaction: `2.35s`.
- Neutral reaction: `2.5s`.
- Awkward reaction: `2.75s`.
- Concerned reaction: `2.9s`.
- Reaction duration clamp increased to allow up to `3.25s`.

This does not add click-to-continue after every answer and should keep overall pacing close to the playtest build.

## Files Changed

- `Assets/Scripts/CybersecurityPresalesInterviewFlow.cs`
- `Assets/Scripts/DeskPrototypeController.cs`
- `Assets/Scripts/InterviewGameManager.cs`
- `Assets/Scripts/SimpleFirstPersonWalkController.cs`
- `Docs/FinalRound_P34_FirstPlaytestFeedbackPass.md`
- `Docs/FinalRound_VS2_TheDesk.md`

## Intentionally Not Changed

- No new jobs.
- No new companies.
- No new recruiter content beyond small contextual lines.
- No Room scoring threshold changes.
- No Room question asset changes.
- No answer score delta changes.
- No change to the six-question stage structure.
- No Room or Desk art rework.
- No VS3 systems.
- No large CandidateState modifier changes.

## How To Test

1. Start from the main menu.
2. Choose `Start Job Search`.
3. Open the Desk laptop.
4. Review the Northbridge listing.
5. Choose and confirm an application strategy.
6. Complete Maya's recruiter screen.
7. Confirm the Desk transition copy says the profile and application notes are forwarded.
8. Continue to The Room.
9. Confirm the Room bypasses the menu and tells the player to find the highlighted chair and press `E`.
10. Confirm pre-sit mouse look feels calmer.
11. Sit and confirm the Stage 1 intro references Maya/application notes.
12. Answer questions and confirm reaction text remains readable without making the run feel slow.
13. Complete the interview and return to Desk.
14. Open `InterviewRoom` directly and confirm neutral direct-room fallback still works.

## Remaining Feedback For Future Milestones

- Add more jobs and roles later.
- Consider a proper Settings slider for Room mouse sensitivity if more players report tuning needs.
- Consider click-to-continue for major stage intros only if timed intros still feel too fast.
- Keep strengthening recruiter/personality carry-through in future recruiter-focused slices.

## Validation

Run:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

Validation result for this pass:

- `dotnet build "Assembly-CSharp.csproj"`: passed, 0 warnings, 0 errors.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: passed, 0 warnings, 0 errors.
