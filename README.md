# Final Round

Current milestone: `Final Round VS4: The Job Board`

Final Round is a compact Unity prototype about surviving a modern job-search pipeline: choosing an opportunity, positioning an application, handling recruiter ambiguity, entering a final interview, reading the outcome, and deciding what to do next.

The current build contains four promoted vertical slices:

- VS1: The Room
- VS2: The Desk
- VS3: The Aftermath
- VS4: The Job Board

## Current Flow

```text
Main Menu
  -> Start Job Search
  -> The Desk
  -> Opportunity Board
  -> choose one of three jobs
  -> Job Listing
  -> Application Strategy
  -> Recruiter Screen
  -> The Room
  -> Outcome Email
  -> Return to Desk
  -> Inbox / Process Summary
  -> optional Reject-only Aftermath
  -> Start New Run
```

## Current Features

- Main menu with the intended `Start Job Search` flow.
- Home Desk scene with laptop interaction.
- Three-card Opportunity Board.
- Three authored job listings:
  - Northbridge Cyber Systems: balanced security presales baseline.
  - Helios Cloud Platform: technical stretch / architecture-heavy role.
  - Redgate Financial Risk: commercial / compliance bureaucracy role.
- One selected job per run.
- Selected-job defaults applied once per run.
- Selected-job carry-through into listing, application feedback, recruiter identity/copy, Room context, outcome inbox, and process summary.
- Application strategy choices that update shared candidate state.
- Recruiter screen before the interview.
- First-person Room entry.
- Sit-down interaction with seated camera.
- Three interviewer placeholders.
- Staged cybersecurity presales interview.
- 24-question ScriptableObject question bank.
- Six-question runtime flow:
  - 2 customer/context questions.
  - 2 technical/security judgement questions.
  - 2 commercial/executive pressure questions.
- Hidden scoring across Technical, Commercial, Rapport, and Energy.
- Judgement reactions after answers.
- Four outcome types:
  - StrongPass
  - Pass
  - Hold
  - Reject
- Outcome email.
- Scorecard.
- Return-to-Desk inbox and process summary.
- Reject-only Aftermath room with safe symbolic object processing.
- Post-aftermath Desk choices.
- Debug tools for deterministic seeds and forced outcomes.

## Outcome Balance

The VS1/P25 six-question staged Room audit distribution remains:

- StrongPass: 10.1%
- Pass: 44.3%
- Hold: 35.1%
- Reject: 10.4%

VS4 adds job-selection context and small default state deltas, but it does not change Room questions, scoring rules, or outcome thresholds.

## Controls

### Desk

- `E`: open the laptop.
- `Space`: open the laptop.
- Mouse click on laptop: open the laptop.
- Mouse: choose job cards, listing sections, application strategies, recruiter replies, and buttons.
- `Esc`: Desk pause menu.
- `F1`: toggle the Desk debug readout.

### Room

- Move: standard first-person movement before sitting.
- Look: mouse/camera look before sitting.
- `E`: sit when the chair prompt is active.
- `Esc`: pause/resume or close menu overlays.
- `F1`: toggle Room interview debug tools.
- Mouse: select answers and outcome/scorecard buttons.

### Aftermath

After a Reject outcome:

- `Clear the Room`: enter the symbolic aftermath room from the Desk.
- `WASD`: move.
- Hold right mouse: look.
- Left click / `E` / `Space`: Feedback Hammer interaction.
- Mouse: select return/Desk buttons.

## Debug Tools

The F1 Room panel supports:

- Toggle deterministic seed.
- Cycle seed.
- Preset Seed 1-4.
- Force Strong Pass.
- Force Pass.
- Force Hold.
- Force Reject.
- Clear Forced Outcome.
- Skip To Outcome.
- Restart Current Run.

The Desk also has debug readout support for current run state, selected job, applied job defaults, and application/recruiter progress.

## Run In Unity

1. Open the project folder in Unity.
2. Open `Assets/Scenes/InterviewRoom.unity`.
3. Press Play.
4. Choose `Start Job Search` from the main menu.
5. Open the Desk laptop.
6. Choose a job from the Opportunity Board.
7. Complete the listing, application strategy, and recruiter screen.
8. Continue to the Room, walk to the chair, and press `E` when prompted.

## Windows Standalone Build

Prebuilt archives are checked in at the repository root:

- `FinalRound_VS4_Windows.zip`
- `FinalRound_VS2_Playtest.zip`
- `FinalRound-VS1.zip`

For current playtesting, unzip `FinalRound_VS4_Windows.zip`, run `FinalRound.exe`, and start with `Start Job Search`.

To make a fresh Windows playtest build:

1. Open the project in Unity.
2. Run `Final Round > Validate Playtest Build Settings`.
3. Run `Final Round > Build Playtest Windows`.
4. Confirm `Builds/Playtest/FinalRound_VS4_Windows/FinalRound.exe` exists.
5. Run `Final Round > Package Latest Playtest Build`.
6. Use `Builds/Playtest/FinalRound_VS4_Playtest.zip`, or copy the package to the repository root when preparing a checked-in playtest archive.

The current external playtest guide is:

- `Docs/FinalRound_P46_VS4PlaytestGuide.md`

## Build Readiness Notes

- The VS4 player build requires `InterviewRoom`, `DeskScene`, and `AftermathRoom` in Build Settings.
- Runtime UI and prototype objects are generated or assigned from scene scripts.
- Generated prototype materials are hardened for player builds to avoid magenta/black missing-shader failures.
- Unity player builds require a valid local Unity license.
- `dotnet build "Assembly-CSharp.csproj"` validates runtime C# compilation.
- `dotnet build "Assembly-CSharp-Editor.csproj"` validates editor tooling and playtest packaging scripts.
- C# builds do not replace an interactive Unity smoke test or a built-exe smoke test.

## Known Limitations

- Desk, Room, Job Board, and Aftermath art remain prototype quality.
- Job cards and email/process UI are functional rather than final.
- There are three authored jobs, not a procedural job marketplace.
- Application and recruiter mechanics are shared across jobs.
- Recruiter questions are not fully job-specific yet.
- Room questions are shared across all jobs.
- The menu is still hosted by the Room scene.
- Interviewers are still standing prefabs hidden by table occlusion, not seated rigs.
- No voice, lip sync, facial animation, or final animation pipeline.
- Outcome email is overlay-based.
- Aftermath uses symbolic prototype objects and procedural fragments.
- Score saturation remains technical debt even though outcome distribution is tuned.
- Question resources still live under the legacy `RC11` folder path.
- No localization, controller support, save system, or broader campaign structure yet.

