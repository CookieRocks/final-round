# Final Round

Prototype version: `Prototype v1.0 RC1`

Final Round is a compact Unity game about surviving a multi-stage Sales Engineering interview loop. Each run asks you to manage confidence, energy, technical credibility, commercial alignment, and interview pressure while moving through a recruiter screen, hiring manager round, technical panel, and VP round.

The game is built as a replayable prototype: question pools, answer order, company profile, random events, and process identity can vary by run, while deterministic seed settings support repeatable testing.

## Current Features

- Multi-stage Sales Engineering interview process.
- Randomized question pools and answer order.
- Process ID and run seed support.
- Company-specific process profiles, rules, and call-panel themes.
- Process Briefing before the first round.
- Interview Pressure meter.
- Limited-use Prep Cards during questions.
- Between-round Recovery Choices.
- Non-repeating between-stage random events.
- 2D video-call interview panel.
- Procedural audio feedback and optional clip hooks.
- Settings screen for audio, motion, run seed, and fullscreen.
- End-of-run badges.
- Final outcome summary with stats, highlights, style, badges, advice, and company rule context.

## Controls

- Mouse: click answers, cards, settings, and continue buttons.
- `1`, `2`, `3`: choose visible answer options.
- `Q`, `W`, `E`: use Prep Cards before selecting an answer.
- `1` through `5`: choose between-round Recovery Choices when that screen is active.
- `Enter`: continue feedback, stage transition, random event, or recovery screens.
- `Esc`: pause/resume, return to menu from pause, or close Settings.

## Settings

Settings are available from the Main Menu and Pause Overlay.

- Audio: Master Volume, SFX Volume, Mute Audio.
- Motion: Reduce Motion.
- Run Options: deterministic run seed, seed input, apply seed, new random seed.
- Display: fullscreen toggle.

Settings are stored with PlayerPrefs on the current machine. Seed changes apply to the next new process and do not restart an active run.

## Run In Unity

1. Open the project folder in Unity.
2. Open `Assets/Scenes/InterviewRoom.unity`.
3. Press Play.

## Windows Standalone Build

1. Open `File > Build Profiles` or `File > Build Settings`, depending on your Unity version.
2. Select `Windows, Mac, Linux` / `Standalone`.
3. Set the target platform to `Windows`.
4. Confirm `Assets/Scenes/InterviewRoom.unity` is the enabled scene in `Scenes In Build`.
5. Click `Switch Platform` if Unity is not already on Windows Standalone.
6. Click `Build`.
7. Choose an output folder, for example `Builds/FinalRound_Windows`.
8. Run the generated `Final Round.exe`.

## Build Readiness Notes

- The active build scene is `Assets/Scenes/InterviewRoom.unity`.
- The project uses Unity's Input System backend; keyboard shortcuts use `Keyboard.current` in that mode.
- Runtime UI and the 2D interview call panel are generated from scripts at play/build time.
- Audio feedback is optional and generated procedurally when no sound files are assigned.
- Reduce Motion is available from Settings for reducing pulse/scale animation.
- The intended RC build target is `Builds/v1.0-rc1/Final Round.exe`.

## Known Limitations

- Prototype balance is intentionally lightweight and may still need more full-run testing.
- The video-call viewport is cosmetic only.
- The game is tuned around 1920x1080 and 1366x768 style layouts; unusual aspect ratios may need more UI tuning.
- No save system, unlock collection, localization, or controller support yet.
- Accessibility is limited to Reduce Motion and optional/mutable audio.
- Company/process rules and pressure are intentionally lightweight, not a full interview simulation system.
