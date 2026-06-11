# Final Round - Prototype P36: Aftermath Room Mode Skeleton

## Summary

P36 adds the first safe skeleton for `Final Round - VS3: The Aftermath`.

The purpose is to prove this flow without implementing destruction gameplay yet:

```text
Reject outcome
  -> Return to Desk
  -> rejection inbox
  -> Clear the Room
  -> AftermathRoom
  -> Return to Desk
```

P36 does not add melee, weapons, destructible objects, physics destruction, new jobs, new recruiter content, Room scoring changes, Room question changes, answer delta changes, or outcome threshold changes.

## Safety And Tone Boundary

The Aftermath Room is symbolic catharsis after rejection, not revenge.

Rules preserved in P36:

- no harming people;
- no recruiters, interviewers, civilians, or human targets;
- no firearms;
- no blood or gore;
- no workplace attack fantasy;
- no realistic revenge framing;
- symbolic objects, text, and corporate phrases only.

The P36 room is an empty altered memory of The Room. It contains chairs, table, laptop, nameplates, forms, panels, and rejection phrases, but no characters.

## What Was Added

Runtime scripts:

- `Assets/Scripts/AftermathRoomController.cs`

Editor tooling:

- `Assets/Editor/FinalRoundAftermathRoomSceneCreator.cs`

Updated existing scripts:

- `Assets/Scripts/CandidateState.cs`
- `Assets/Scripts/CybersecurityPresalesInterviewFlow.cs`
- `Assets/Scripts/DeskPrototypeController.cs`
- `Assets/Editor/FinalRoundPlaytestBuild.cs`

## CandidateState Fields

P36 adds:

- `AftermathAvailable`
- `AftermathCompleted`

Rules:

- `AftermathAvailable` is set to `true` only when an active Desk run records a `Reject` Room outcome.
- `AftermathAvailable` is set to `false` for `StrongPass`, `Pass`, and `Hold`.
- `AftermathCompleted` is reset when a new Room outcome is recorded.
- `AftermathCompleted` is set to `true` when the player exits the Aftermath Room back to the Desk.

## Desk Reject-Only Entry

When the Desk inbox shows a completed Reject run, it can now show:

```text
Clear the Room
```

The button appears only when:

- an active Desk run exists;
- `RoomOutcome` is `Reject`;
- `AftermathAvailable` is true;
- `AftermathCompleted` is false.

The action is hidden for `StrongPass`, `Pass`, `Hold`, direct Desk testing without a completed run, and completed aftermath runs.

The Desk feedback line also states whether the aftermath is available or completed.

## AftermathRoom Behaviour

`AftermathRoomController` generates a P36 skeleton scene at runtime:

- camera;
- light;
- dark/overlit empty room shell;
- empty chairs;
- interview table;
- laptop;
- whiteboard;
- job-ad panel;
- nameplates;
- feedback forms;
- safe rejection phrases;
- UI title/objective;
- placeholder Composure meter at 0%;
- `Return to Desk`;
- `Main Menu`.

Player-facing lines:

```text
The panel has left. The room has not.
Clear the Room.
```

Direct-open protection:

- If `AftermathRoom` opens without an active Reject aftermath state, it shows:

```text
No active aftermath run.
This room is safe to exit.
```

It still allows Return to Desk or Main Menu and does not crash.

## Return To Desk

Returning to Desk from an active Reject aftermath:

- sets `AftermathCompleted = true`;
- applies tiny clamped recovery:
  - Energy `+1`;
  - Candidate Confidence `+1`;
- preserves `RoomOutcome`;
- loads `DeskScene`.

P36 does not retroactively change the Room outcome.

## Scene And Build Settings

P36 adds an editor menu command:

```text
Final Round > Create/Repair Aftermath Room Scene
```

The command creates or repairs:

```text
Assets/Scenes/AftermathRoom.unity
```

It also ensures Build Settings include:

- `Assets/Scenes/InterviewRoom.unity`
- `Assets/Scenes/DeskScene.unity`
- `Assets/Scenes/AftermathRoom.unity`

The command keeps Build Settings ordered with `InterviewRoom` first and reopens `InterviewRoom` after repair so pressing Play starts from the normal main menu instead of directly inside `AftermathRoom`.

The playtest build helper now includes `AftermathRoom.unity` in its required scene list.

### Current Scene Creation Status

The scene asset was created from Unity Editor tooling. To repair or regenerate it later, run:

```text
Final Round > Create/Repair Aftermath Room Scene
```

After that, confirm `Assets/Scenes/AftermathRoom.unity` exists, appears in Build Settings after `InterviewRoom` and `DeskScene`, and Unity has returned to `InterviewRoom` as the open scene.

## Intentionally Deferred

- destructible object component;
- raycast hit/smash interaction;
- Feedback Hammer;
- physics fragments;
- catharsis scoring from object hits;
- timed aftermath session;
- post-aftermath branching beyond the current return path;
- Hold/Waiting Room variant;
- any new job or recruiter content;
- Room scoring/question/threshold changes.

## How To Test

1. In Unity, run `Final Round > Create/Repair Aftermath Room Scene`.
2. Start from the main menu.
3. Choose `Start Job Search`.
4. Complete Desk application and recruiter flow.
5. Enter The Room.
6. Reach or force a `Reject` outcome.
7. Return to Desk.
8. Confirm the rejection inbox appears.
9. Confirm `Clear the Room` appears.
10. Click `Clear the Room`.
11. Confirm `AftermathRoom` loads.
12. Confirm no people, recruiter, interviewers, civilians, or human targets are present.
13. Confirm symbolic objects and rejection phrases are present.
14. Confirm the Composure meter is visible at 0%.
15. Click `Return to Desk`.
16. Confirm the Desk inbox is still visible.
17. Confirm Process Summary shows aftermath completed.
18. Confirm non-Reject outcomes do not show `Clear the Room`.
19. Confirm direct `InterviewRoom` neutral fallback still works.
20. Open `AftermathRoom` directly and confirm the safe no-active-run message.

## Recommended P37 Prompt

`Final Round - Prototype P37: Destructible Objects Prototype`

Suggested scope:

- add `AftermathDestructible`;
- add symbolic raycast hit interaction;
- add intact/broken prefab or mesh swap;
- add catharsis values;
- update the Composure / Catharsis meter from object hits;
- keep all safety boundaries;
- do not add people, firearms, gore, or revenge framing;
- do not change Room scoring, questions, or thresholds.

## Validation

Run:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

Validation result:

- `dotnet build "Assembly-CSharp.csproj"`: passed, 0 warnings, 0 errors.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: passed, 0 warnings, 0 errors.
