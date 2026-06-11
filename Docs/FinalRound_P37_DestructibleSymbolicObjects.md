# Final Round - Prototype P37: Destructible Symbolic Objects

## Summary

P37 adds the first playable destruction/catharsis loop for `Final Round - VS3: The Aftermath`.

The player can enter `AftermathRoom` after a Reject outcome and use the symbolic `Feedback Hammer` to process objects, phrases, and props tied to rejection and corporate ambiguity.

This is not combat. It is a safe, symbolic recovery interaction inside a distorted memory of The Room.

## Safety And Tone Boundary

Rules preserved in P37:

- no harming people;
- no recruiters, interviewers, civilians, mannequins, or human targets;
- no firearms;
- no blood or gore;
- no realistic workplace violence framing;
- no revenge fantasy;
- only symbolic objects, text, forms, and corporate phrases can be cleared.

The player is processing rejection. The targets are wording, props, and symbols.

## What Was Added

Runtime scripts:

- `Assets/Scripts/AftermathDestructible.cs`

Updated runtime scripts:

- `Assets/Scripts/AftermathRoomController.cs`

Updated build/project file:

- `Assembly-CSharp.csproj`

Docs:

- `Docs/FinalRound_P37_DestructibleSymbolicObjects.md`
- `FinalRound_GamePlan_Current.md`

## Controls

In `AftermathRoom`, during an active Reject aftermath:

- `WASD`: move around the altered room.
- Hold right mouse: look around.
- Left mouse click: use the `Feedback Hammer` at the mouse position.
- `E`: use the `Feedback Hammer` at screen center.
- `Space`: fallback use of the `Feedback Hammer` at screen center.

P37 uses a deliberately small Aftermath-only camera controller. It is not a full FPS controller and stays bounded inside the visible room.

## Debug Entry

For P37 testing, the main menu includes:

```text
Debug: Start Aftermath
```

This seeds a temporary active CandidateState with a Reject outcome, sets `AftermathAvailable = true`, and loads `AftermathRoom` directly. It is a testing shortcut only; normal play still exposes the aftermath path only after a Reject outcome at the Desk.

## Feedback Hammer

`Feedback Hammer` is a symbolic interaction label, not a realistic weapon.

The UI describes the action as:

```text
Feedback Hammer: Left Click / E / Space to process symbolic objects.
```

Player-facing feedback uses language such as:

- `process symbolic objects`;
- `one phrase processed`;
- `the wording loses some of its power`;
- `the room is quieter now`.

It avoids combat terms such as attack, kill, weapon damage, or enemies.

## Destructible Component

`AftermathDestructible` tracks a symbolic object:

- `objectId`
- `displayLabel`
- `hitPoints`
- `catharsisValue`
- `intactRoot`
- `brokenRoot`
- `onHitText`
- `onDestroyedText`
- `canBeDestroyed`
- `hasBeenDestroyed`

On hit:

- hit points reduce;
- a short hit line appears for about `1.25s`.

On destroyed:

- intact visuals hide;
- processed/broken visuals appear;
- Composure increases;
- a destroyed line appears for about `2.5s`;
- the object ignores future hits.

## Generated Destructible Objects

P37 generates these safe symbolic destructibles:

| Object | Catharsis value |
| --- | ---: |
| `Unfortunately...` plaque | 10 |
| `After careful consideration` sign | 10 |
| `We'll keep your details on file` placard | 10 |
| `No feedback available` placard | 10 |
| feedback form stack | 10 |
| empty nameplate | 8 |
| rejection laptop | 14 |
| job-ad panel | 12 |
| scorecard shard | 8 |
| empty chair | 12 |
| calendar invite block | 8 |

Total possible Composure is above 100, so the player does not need to clear every object exactly.

## Composure Meter

Composure starts at `0%`.

Destroying symbolic objects increases the meter by the object's catharsis value.

At `100%`:

```text
The room is quieter now.
```

The `Return To Desk` button remains available at all times, but it is visually highlighted after Composure reaches 100%.

`AftermathCompleted` is still set only when the player returns to the Desk, preserving P36 behaviour.

Returning to Desk from an active Reject aftermath still applies the tiny recovery:

- Energy `+1`;
- Candidate Confidence `+1`.

P37 does not change the Room outcome retroactively.

## Direct-Open Protection

If `AftermathRoom` opens without an active Reject aftermath:

- the no-active-run message appears;
- the destructible loop is not required;
- Return to Desk and Main Menu still work.

The symbolic objects may be visible, but the Feedback Hammer loop is disabled.

## Debug Logging

P37 logs:

- destroyed symbolic object ID and label;
- current Composure value;
- aftermath completion and return-to-Desk state summary.

Raw debug remains in Unity logs, not normal player UI.

## Intentionally Deferred

- first-person movement in Aftermath;
- animation-heavy hammer model;
- physics-heavy destruction;
- audio/particles;
- timed aftermath sessions;
- more expressive post-aftermath Desk choices;
- Hold/Waiting Room variant;
- new jobs;
- new recruiter content;
- Room scoring changes;
- Room question or threshold changes.

## How To Test

1. Start from the main menu.
2. Choose `Start Job Search`.
3. Complete the Desk job listing, application strategy, and recruiter screen.
4. Enter The Room.
5. Achieve or force a `Reject` outcome.
6. Return to Desk.
7. Confirm the rejection inbox appears.
8. Confirm `Clear the Room` appears.
9. Click `Clear the Room`.
10. Confirm `AftermathRoom` loads.
11. Confirm no people, recruiters, interviewers, civilians, mannequins, or human targets are present.
12. Use left click, `E`, or `Space` on symbolic objects.
13. Confirm objects swap from intact to processed/broken visuals.
14. Confirm short reaction text appears and remains readable.
15. Confirm Composure increases.
16. Fill Composure to 100%.
17. Confirm `The room is quieter now.` appears.
18. Return to Desk.
19. Confirm Process Summary shows aftermath completed.
20. Confirm non-Reject outcomes do not show `Clear the Room`.
21. Open `AftermathRoom` directly and confirm the safe no-active-run message and exit buttons still work.

Fast debug path:

1. Start from the main menu.
2. Choose `Debug: Start Aftermath`.
3. Confirm `AftermathRoom` loads with active Feedback Hammer controls.
4. Destroy symbolic objects and fill Composure.
5. Return to Desk and confirm aftermath completion.

## Recommended P38 Prompt

`Final Round - Prototype P38: Desk Integration After Reject`

Suggested scope:

- add post-aftermath Desk actions:
  - `Apply Again`;
  - `Take a Break`;
  - `Ask for Feedback`;
  - `Start New Run`;
- keep Reject-only routing;
- make aftermath completion feel acknowledged in the Desk inbox;
- keep P37 destructibles and Composure unchanged unless playtesting finds a blocker;
- do not change Room scoring, question data, thresholds, jobs, or recruiter content.

## Validation

Run:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

Validation result:

- `dotnet build "Assembly-CSharp.csproj"`: passed, 0 warnings, 0 errors.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: passed, 0 warnings, 0 errors.
