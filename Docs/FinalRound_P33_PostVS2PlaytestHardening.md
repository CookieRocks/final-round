# Final Round - Prototype P33: Post-VS2 Playtest Hardening

## Summary

P33 prepares the promoted VS2 loop for external playtesting without adding new gameplay systems.

The scope is deliberately small:

- hide Desk debug by default;
- keep development debug access;
- clarify the main menu route into VS2 versus direct Room testing;
- add subtle first-time guidance;
- check build/distribution readiness;
- document remaining playtest limitations.

P33 does not change:

- jobs;
- recruiter content;
- Room questions;
- Room thresholds;
- Room scoring;
- CandidateState modifier rules;
- Room art;
- interviewer setup;
- VS3 functionality.

## Files Changed

- `Assets/Scripts/DeskPrototypeController.cs`
- `Assets/Scripts/InterviewGameManager.cs`
- `Docs/FinalRound_VS2_TheDesk.md`
- `Docs/FinalRound_P33_PostVS2PlaytestHardening.md`
- `FinalRound_GamePlan_Current.md`

## Debug Visibility Changes

Desk debug readout is now hidden by default.

Development access remains:

- `F1` in `DeskScene` toggles the Desk debug readout.
- `F1` in the Room continues to open the Room debug panel.
- Debug systems were not removed.

The Desk debug readout still shows:

- current Desk state;
- target interview scene;
- application/recruiter completion flags;
- CandidateState summary.

## Main Menu Clarity

The main menu now separates the intended VS2 path from the direct Room test path:

- `Start Job Search`
- `Debug: Start Room Directly`

The menu subtitle now explains that `Start Job Search` is the full Desk-to-Room loop and the direct Room button is for testing neutral fallback.

A separate main-menu scene was not implemented in P33. Keeping the menu inside `InterviewRoom` is acceptable for playtesting but remains technical debt.

## Desk UI Readability / Guidance Changes

Small player-facing text changes were made:

- Desk header prompt now says to open the laptop to review the role.
- Listing feedback now directs the player to choose how to position the application.
- Application strategy screen asks the player to select and confirm a strategy.
- Strategy selection feedback no longer exposes raw CandidateState deltas to normal playtesters.
- Application confirmation now directs the player to complete the recruiter screen.
- Recruiter completion now directs the player to continue to the interview.
- Reset copy now tells the player to open the laptop to review the role again.

The full state trail remains available in the Desk debug readout and post-run Process Summary.

## Return / Reset Behaviour

Intentional behaviours:

- `Return to Desk` appears only after a Desk-launched Room run has a completed outcome.
- `Return to Desk` preserves CandidateState and opens the Desk inbox.
- `Start New Run` clears CandidateState and resets the Desk flow.
- `Main Menu` clears CandidateState and loads the existing menu hosted in `InterviewRoom`.
- Room `Restart` preserves active Desk state so the player can replay the same opportunity/interview context.

## Playtest Controls

### Main Menu

- `Start Job Search`: full VS2 loop.
- `Debug: Start Room Directly`: direct neutral Room test path.
- `Settings`, `How To Play`, `About`, and `Quit` remain available where supported.

### Desk

- `E`: open laptop.
- `Space`: open laptop.
- Mouse click on laptop: open laptop.
- Mouse: choose listing sections, strategies, recruiter replies, and Desk actions.
- `F1`: toggle Desk debug readout.

### Room

- Move/look before sitting: existing first-person controls.
- `E`: sit at the interview chair when prompted.
- Mouse: select interview answers and email/scorecard buttons.
- `Esc`: pause/resume.
- `F1`: Room debug tools.

## Recommended External Playtest Checklist

1. Start from the main menu.
2. Choose `Start Job Search`.
3. Open the laptop with `E`.
4. Restart and confirm `Space` also opens the laptop.
5. Restart and confirm mouse click also opens the laptop.
6. Read the listing sections.
7. Choose and confirm one application strategy.
8. Complete all three recruiter prompts.
9. Continue to the Room.
10. Confirm the Room starts with the seated interaction flow.
11. Complete or debug-skip the six-question interview.
12. Confirm outcome email readability.
13. Confirm `Return to Desk` is available.
14. Return to Desk and read the inbox.
15. Open Process Summary.
16. Use `Start New Run` and confirm the Desk resets.
17. From main menu, use `Debug: Start Room Directly` and confirm no Desk inbox/return state leaks in.

## Build / Distribution Readiness

Build Settings include:

- `Assets/Scenes/InterviewRoom.unity`
- `Assets/Scenes/DeskScene.unity`

Hygiene checks:

- no untracked import debris was present before the player-build attempt;
- known dirty Unity drift still exists in:
  - `Assets/Scenes/DeskScene.unity`
  - `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset`
  - `ProjectSettings/GraphicsSettings.asset`
- text scan did not find missing script markers in scenes or prefabs;
- existing URP/profile null shader/script entries appear to be settings/profile data rather than a P33 blocker.

Player build result:

- Unity is installed at `C:\Program Files\Unity\Hub\Editor\6000.4.10f1`.
- There is no dedicated project build script yet.
- P33 attempted a basic command-line Windows player build with `-buildWindows64Player`.
- The Unity executable returned immediately with exit code `0`, but produced no player files and no log output under `Builds/P33Playtest`.
- No distributable player build was produced from this shell.

Recommended distribution path for now: build from the Unity Editor, or add a dedicated editor build script/menu command in a packaging milestone.

## Known Limitations

- Desk scene remains a generated prototype shell.
- Laptop UI is not a final job-board/email interface.
- There is still one company, one role, and one recruiter.
- No prep/action stage exists yet.
- Main menu is still hosted by `InterviewRoom`.
- Room score saturation remains technical debt.
- Question assets still live under a legacy `RC11` resource path.
- Unity player distribution depends on local Unity licensing/build setup.

## Remaining Technical Debt

- Add a dedicated main-menu scene if external playtesting expands.
- Split Desk UI rendering from `DeskPrototypeController` if the Desk grows.
- Add a proper build script/menu command for repeatable player builds.
- Reconcile or intentionally commit/revert URP/Graphics settings drift in a separate maintenance pass.
- Decide whether Room `Restart` should be relabelled `Retry Interview` in a later polish pass.

## Recommended Next Milestone

If external playtesting is next:

`Final Round - Prototype P34: External Playtest Packaging`

Suggested scope:

- create a repeatable Unity build command/menu item;
- produce a zipped Windows build;
- include a short playtest README;
- do not add gameplay or content.

If development continues directly:

`Final Round - Vertical Slice VS3: Recruiter Screen`

Suggested scope:

- expand recruiter interaction into a stronger stage;
- keep VS2 Desk -> Room -> Desk protected;
- avoid changing Room scoring or question content unless playtest feedback demands it.

## Validation

Run:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

P33 validation result:

- `dotnet build "Assembly-CSharp.csproj"`: passed, 0 warnings, 0 errors.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: passed, 0 warnings, 0 errors.
- Unity command-line player build: attempted, but no player/log output was produced.
