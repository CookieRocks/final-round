# Final Round - Vertical Slice VS3: The Aftermath

## Summary

`Final Round - Vertical Slice VS3: The Aftermath` promotes the Reject-only aftermath loop from prototype milestone work into a playable vertical slice.

VS3 makes rejection part of the playable hiring-pipeline loop instead of only a fail state. After a Reject outcome, the player returns to the Desk, sees the rejection inbox, and can choose `Clear the Room` to enter a distorted empty version of The Room. Inside, the player processes symbolic objects and corporate phrases with the `Feedback Hammer`, fills the Composure meter, and returns to the Desk with the aftermath recorded.

VS3 remains symbolic catharsis, not revenge.

## Player Flow

Promoted VS3 flow:

```text
Main Menu
  -> Start Job Search
  -> Desk
  -> Job Listing
  -> Application Strategy
  -> Recruiter Screen
  -> Room
  -> Reject outcome
  -> Return to Desk
  -> rejection inbox
  -> Clear the Room
  -> AftermathRoom
  -> process symbolic objects
  -> Composure reaches 100%
  -> Return to Desk
  -> Process Summary shows aftermath completed
```

Fast debug path for testing:

```text
Main Menu
  -> Debug: Start Aftermath
  -> AftermathRoom with seeded Reject aftermath state
```

## Reject-Only Trigger

`Clear the Room` appears only when:

- there is an active Desk run;
- the Room outcome is `Reject`;
- `AftermathAvailable` is true;
- `AftermathCompleted` is false.

`StrongPass`, `Pass`, and `Hold` do not expose the aftermath option.

Once the aftermath has been completed and the player returns to Desk, `Clear the Room` does not appear again for that run.

## Controls

### Desk

- Mouse: choose Desk actions and inbox options.
- `Clear the Room`: appears in the rejection inbox after a Reject outcome.

### AftermathRoom

- `WASD`: move around the altered room.
- Hold right mouse: look around.
- Left mouse click: use `Feedback Hammer` at the pointer.
- `E`: use `Feedback Hammer` at screen center.
- `Space`: fallback `Feedback Hammer` use at screen center.
- `Return To Desk`: exit the aftermath and complete the aftermath state.
- `Main Menu`: leave to the main menu scene.

## Composure Meter Behaviour

Composure starts at `0%`.

Destroying symbolic objects increases Composure by each object's catharsis value.

Milestone lines:

- 25%: `The wording starts to lose its grip.`
- 50%: `The room feels less loud.`
- 75%: `You can hear yourself think again.`
- 100%: `The room is quieter now.`

At 100%:

- completion text appears;
- the room becomes slightly quieter visually;
- `Return To Desk` is highlighted more strongly.

Return to Desk remains available before 100%, but completion is acknowledged when the meter fills.

## Safety And Tone Boundary

VS3 hard rules:

- no people;
- no recruiters;
- no interviewers;
- no civilians;
- no mannequins or human targets;
- no firearms;
- no blood or gore;
- no revenge framing;
- no realistic workplace attack fantasy.

Targets are symbolic only:

- corporate phrases;
- rejection wording;
- forms;
- nameplates;
- laptop/inbox props;
- job-ad panels;
- scorecard fragments;
- empty chairs and room objects.

The player is not attacking people or a real workplace. They are processing rejection through a distorted memory of the interview room.

## What VS3 Demonstrates

VS3 demonstrates:

- rejection can route into a playable aftermath mode;
- the Desk can present post-outcome actions conditionally;
- shared `CandidateState` can track aftermath availability and completion;
- the aftermath scene can be opened safely from a Reject run;
- direct-open protection keeps the scene harmless without an active Reject state;
- symbolic object processing can fill a Composure meter;
- Return to Desk records aftermath completion;
- VS1 and VS2 direct paths remain protected;
- the safety boundary can hold even with a rage-room-inspired mechanic.

## Implementation Notes

Primary runtime classes:

- `CandidateState`
- `FinalRoundRunState`
- `DeskPrototypeController`
- `CybersecurityPresalesInterviewFlow`
- `AftermathRoomController`
- `AftermathDestructible`
- `InterviewGameManager`

Primary scene:

- `Assets/Scenes/AftermathRoom.unity`

Relevant editor tooling:

- `Final Round > Create/Repair Aftermath Room Scene`
- `Final Round > Set Play Mode Start Scene`

## Known Limitations

Accepted for VS3:

- Aftermath room art is generated/prototype quality.
- Destruction visuals are procedural fragments, not authored assets.
- There is no authored `Feedback Hammer` model.
- Audio hooks exist, but no real sound pass is assigned by default.
- There are no authored particle systems.
- Break animations are simple scale/visibility/fragment changes.
- The symbolic object list is small.
- Direct-open protection still shows the symbolic room shell, though the destruction loop is disabled.
- Full packaged-build manual QA is still recommended before external playtesting.

## Intentionally Deferred

- Post-aftermath Desk choices:
  - `Apply Again`;
  - `Take a Break`;
  - `Ask for Feedback`;
  - richer `Start New Run` handling.
- Hold/Waiting Room variant.
- Final aftermath room art pass.
- Authored breakable prefabs.
- Authored hammer viewmodel/icon.
- Sound design pass.
- More object variety.
- Campaign-level recovery systems.
- VS4 recruiter expansion.

## Future Expansion Ideas

Possible next work:

- P40: post-aftermath Desk choices.
- A deeper aftermath inbox state after `AftermathCompleted`.
- Optional `Ask for Feedback` action that may or may not receive a useful response.
- `Take a Break` action that restores Energy but advances time.
- `Apply Again` route into a fresh Desk run.
- More symbolic object sets tied to different rejection styles.
- Waiting Room variant for `Hold`.
- Final art/audio pass once the mode survives more playtesting.

## Validation

Promotion validation:

- `Docs/FinalRound_P39_VS3ReadinessReview.md`: recommends promotion.
- `dotnet build "Assembly-CSharp.csproj"`: passed, 0 warnings, 0 errors.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: passed, 0 warnings, 0 errors.
