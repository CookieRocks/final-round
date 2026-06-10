# Final Round - Prototype P27: Desk Scene Prototype

## Summary

P27 creates the first playable Desk prototype shell for `VS2: The Desk`.

The milestone proves that the player can start at a desk/laptop, open a placeholder job interface, create a neutral `CandidateState`, and transition into the existing `InterviewRoom` while keeping direct Room play protected.

P27 does not implement the full job listing, application strategy, recruiter exchange, prep choice, return-to-desk inbox, or Room modifiers.

## Source Files Read

- `FinalRound_GamePlan_Current.md`
- `Docs/FinalRound_VS2_TheDesk_Planning.md`
- `Docs/FinalRound_P26_VS2_ArchitectureSkeleton.md`
- `Docs/FinalRound_VS1_TheRoom.md`
- `Docs/FinalRound_P25_VS1ReadinessReview.md`
- `Assets/Scripts/DeskPrototypeController.cs`
- `Assets/Scripts/FinalRoundRunState.cs`
- `Assets/Scripts/CandidateState.cs`

## What Was Added

`DeskPrototypeController` now owns the P27 prototype shell:

- fixed camera setup;
- simple generated desk, laptop, chair, wall, floor, and notebook placeholders;
- soft directional light if no scene light exists;
- generated Canvas and EventSystem if missing;
- prompt text;
- placeholder laptop/job UI;
- simple debug readout;
- keyboard laptop opening;
- mouse-click laptop opening;
- neutral CandidateState creation;
- `InterviewRoom` transition button.

## Scene Creation

`Assets/Scenes/DeskScene.unity` is created through an editor-only menu command rather than by hand-editing scene YAML.

Menu command:

`Final Round > Create/Repair Desk Scene`

The command:

- creates or opens `Assets/Scenes/DeskScene.unity`;
- creates a root object named `DeskSceneRoot`;
- creates or repairs a GameObject named `DeskPrototypeController`;
- attaches `DeskPrototypeController`;
- enables `Generate Prototype Scene Objects`;
- sets the target interview scene to `InterviewRoom`;
- avoids duplicate `DeskPrototypeController` objects;
- saves the scene;
- verifies `Assets/Scenes/DeskScene.unity` and `Assets/Scenes/InterviewRoom.unity` in Editor Build Settings.

## Unity Setup

1. Open the project in Unity.
2. Run `Final Round > Create/Repair Desk Scene`.
3. Confirm `Assets/Scenes/DeskScene.unity` exists in the Project window.
4. Open `DeskScene`.
5. Confirm the Hierarchy contains:
   - `DeskSceneRoot`;
   - `DeskPrototypeController`.
6. Select `DeskPrototypeController`.
7. Confirm:
   - `Generate Prototype Scene Objects` is enabled;
   - `Interview Room Scene Name` is `InterviewRoom`.
8. Open Build Settings and confirm both scenes are listed and enabled:
   - `Assets/Scenes/DeskScene.unity`;
   - `Assets/Scenes/InterviewRoom.unity`.

No authored job listing assets are required for P27. If `Default Job Listing` is empty, the controller uses:

- job ID: `NCS-SE-001`;
- company: `Northbridge Cyber Systems`;
- role: `Senior Solutions Engineer - Security Presales`.

## Controls

- `E`: open laptop/job UI.
- `Space`: open laptop/job UI.
- Mouse click on laptop base: open laptop/job UI.
- `Continue`: advances the placeholder listing shell only.
- `Go To Interview Room`: creates/uses CandidateState and loads `InterviewRoom`.
- `Reset Desk Run`: resets CandidateState and closes the placeholder UI.

## CandidateState Creation

When the Desk run begins, `DeskPrototypeController.BeginDeskRun()`:

1. calls `FinalRoundRunState.CreateNeutralRun()`;
2. sets `CandidateState.hasActiveDeskRun` to true through the neutral run factory;
3. sets `CandidateState.selectedJobId` to `NCS-SE-001` or the assigned job listing asset ID;
4. logs a debug summary;
5. refreshes the on-screen debug readout.

All modifier values remain neutral at `0`.

## Transition To InterviewRoom

The `Go To Interview Room` button:

1. ensures a `FinalRoundRunState` exists;
2. ensures `selectedJobId` is populated;
3. logs the CandidateState summary;
4. loads the configured `InterviewRoom` scene.

No Room scoring, question selection, or outcome thresholds are changed by P27.

## Room Fallback Protection

Direct `InterviewRoom` play remains protected.

If `InterviewRoom` starts without an active `FinalRoundRunState`, `CybersecurityPresalesInterviewFlow` logs that it is using neutral VS1 direct-start behaviour and the Room proceeds as before.

If the Desk creates a run first, the Room logs the active CandidateState summary and later records the final room outcome back into `CandidateState.roomOutcome`.

## How To Test

### Desk flow

1. Run `Final Round > Create/Repair Desk Scene`.
2. Open `Assets/Scenes/DeskScene.unity`.
3. Enter Play Mode.
4. Confirm the fixed camera shows the generated desk/laptop shell.
5. Press `E`, press `Space`, or click the laptop base.
6. Confirm the `Northbridge Jobs` panel opens.
7. Confirm the debug readout shows:
   - active run state;
   - selected job ID `NCS-SE-001`;
   - target scene `InterviewRoom`.
8. Click `Continue`.
9. Confirm the placeholder text changes and no full application choices appear.
10. Click `Go To Interview Room`.
11. Confirm `InterviewRoom` loads.
12. Confirm the Room console logs active CandidateState detection.

### Direct Room fallback

1. Open `Assets/Scenes/InterviewRoom.unity` directly.
2. Enter Play Mode.
3. Start the interview as usual.
4. Confirm the console logs neutral direct-start fallback.
5. Confirm VS1 Room flow still works.

## Intentionally Deferred

- Full job listing sections.
- Application strategy choices.
- Recruiter message exchange.
- Prep/action choice.
- Return-to-desk inbox.
- Desk art pass.
- Main menu integration.
- Real Room score modifiers.
- Stage intro or outcome email variants driven by CandidateState.
- VS1 scoring, questions, UI, or room behaviour changes.

## Recommended P28 Prompt

`Final Round - Prototype P28: Job Listing and Application Choices`

Suggested scope:

- create one `JobListingData` asset for Northbridge Cyber Systems;
- add authored listing sections to the Desk UI;
- add 3-5 application strategy choices;
- apply first non-zero CandidateState deltas;
- keep Room modifiers disabled until P30;
- verify direct `InterviewRoom` fallback still works;
- run `dotnet build "Assembly-CSharp.csproj"`.

## Validation

Run:

`dotnet build "Assembly-CSharp.csproj"`

P27 should pass with 0 warnings and 0 errors.
