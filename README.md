# Final Round

Current milestone: `Final Round VS2: The Desk`

Final Round is a compact Unity prototype about surviving a cybersecurity presales hiring process. VS2 connects the home Desk, job listing, application strategy, recruiter screen, final interview Room, outcome email, and post-run inbox into one playable Desk-to-Room loop.

## Current VS2 Features

- Main menu with the intended `Start Job Search` flow.
- Home Desk scene with laptop interaction.
- Northbridge Jobs listing with readable role sections.
- Application strategy choice that updates shared candidate state.
- Maya Patel recruiter screen before the interview.
- First-person room entry.
- Sit-down interaction with seated camera.
- Three human interviewer placeholders.
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
- Return-to-Desk flow with Northbridge Mail / inbox summary.
- Process summary and start-new-run path.
- Debug tools for deterministic seeds and forced outcomes.

## Outcome Balance

The VS1/P25 six-question staged Room audit distribution remains:

- StrongPass: 10.1%
- Pass: 44.3%
- Hold: 35.1%
- Reject: 10.4%

## Controls

### Desk

- `E`: open the laptop.
- `Space`: open the laptop.
- Mouse click on laptop: open the laptop.
- Mouse: choose listing sections, application strategies, recruiter replies, and buttons.
- `F1`: toggle the Desk debug readout.

### Room

- Move: standard first-person movement.
- Look: mouse/camera look before sitting.
- `E`: sit when the chair prompt is active.
- `Esc`: pause/resume or close menu overlays.
- `F1`: toggle room interview debug tools.
- Mouse: select answers and outcome/scorecard buttons.

## Debug Tools

The F1 panel supports:

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

## Run In Unity

1. Open the project folder in Unity.
2. Open `Assets/Scenes/InterviewRoom.unity`.
3. Press Play.
4. Choose `Start Job Search` from the main menu.
5. Complete the Desk listing, application strategy, and recruiter screen.
6. Continue to the Room, walk to the chair, and press `E` when prompted.

## Windows Standalone Build

Prebuilt archives are checked in at the repository root:

- `FinalRound_VS2_Playtest.zip`
- `FinalRound-VS1.zip`

For VS2 playtesting, unzip `FinalRound_VS2_Playtest.zip`, run `FinalRound.exe`, and start with `Start Job Search`.

To make a fresh Windows playtest build:

1. Open the project in Unity.
2. Run `Final Round > Validate Playtest Build Settings`.
3. Run `Final Round > Build Playtest Windows`.
4. Confirm `Builds/Playtest/FinalRound_VS2_Windows/FinalRound.exe` exists.
5. Run `Final Round > Package Latest Playtest Build`.
6. Use `Builds/Playtest/FinalRound_VS2_Playtest.zip` or copy it to the repository root for a checked-in playtest package.

## Build Readiness Notes

- The VS2 player build requires both `Assets/Scenes/InterviewRoom.unity` and `Assets/Scenes/DeskScene.unity`.
- Runtime UI and room objects are generated or assigned from scene scripts.
- Unity player builds require a valid local Unity license.
- `dotnet build "Assembly-CSharp.csproj"` validates C# compilation but does not replace an interactive Unity smoke test.
- `dotnet build "Assembly-CSharp-Editor.csproj"` validates the editor playtest packaging script.

## Known Limitations

- The Desk scene is prototype art and the laptop UI is functional rather than final.
- There is one company, one role, and one recruiter.
- The menu is still hosted by the Room scene.
- Interviewers are still standing prefabs hidden by table occlusion, not seated rigs.
- No voice, lip sync, facial animation, or animation pipeline.
- Room art remains prototype quality.
- Outcome email is overlay-based.
- Score saturation remains technical debt even though outcome distribution is tuned.
- Question resources still live under the legacy `RC11` folder path.
- No localization, controller support, save system, or broader campaign structure yet.
