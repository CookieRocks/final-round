# Final Round

Current milestone: `Final Round VS1: The Room`

Final Round is a compact Unity prototype about surviving a cybersecurity presales final-round interview. VS1 focuses on a single playable vertical slice: entering the interview room, sitting down in front of a three-person panel, answering a staged six-question interview, and receiving an outcome email plus scorecard.

## Current VS1 Features

- First-person room entry.
- Main menu and start flow.
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
- Debug tools for deterministic seeds and forced outcomes.

## Outcome Balance

The P24/P25 six-question staged audit distribution is:

- StrongPass: 10.1%
- Pass: 44.3%
- Hold: 35.1%
- Reject: 10.4%

## Controls

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
4. Start the interview process from the main menu.
5. Walk to the chair and press `E` when prompted.

## Windows Standalone Build

A prebuilt VS1 archive is checked in at the repository root:

- `FinalRound-VS1.zip`

To make a fresh Windows build:

1. Open `File > Build Profiles` or `File > Build Settings`, depending on your Unity version.
2. Select `Windows, Mac, Linux` / `Standalone`.
3. Set the target platform to `Windows`.
4. Confirm `Assets/Scenes/InterviewRoom.unity` is the enabled scene in `Scenes In Build`.
5. Click `Switch Platform` if Unity is not already on Windows Standalone.
6. Click `Build`.
7. Choose an output folder, for example `Builds/FinalRound_VS1_TheRoom`.
8. Run the generated `Final Round.exe`.

## Build Readiness Notes

- The active build scene is `Assets/Scenes/InterviewRoom.unity`.
- Runtime UI and room objects are generated or assigned from scene scripts.
- Unity player builds require a valid local Unity license.
- `dotnet build "Assembly-CSharp.csproj"` validates C# compilation but does not replace an interactive Unity smoke test.

## Known Limitations

- Interviewers are still standing prefabs hidden by table occlusion, not seated rigs.
- No voice, lip sync, facial animation, or animation pipeline.
- Room art remains prototype quality.
- Outcome email is overlay-based.
- Score saturation remains technical debt even though outcome distribution is tuned.
- Question resources still live under the legacy `RC11` folder path.
- No localization, controller support, save system, or broader campaign structure yet.
