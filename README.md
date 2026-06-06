# Final Round

Prototype version: `Prototype v0.9`

Final Round is a Unity prototype about surviving a multi-stage Sales Engineering interview loop. Each run includes randomized answer order, company-specific process rules, an Interview Pressure meter, Prep Cards, between-round recovery choices, between-stage events, lightweight UI animation, an animated video-call interview viewport, subtle audio feedback, a player-facing Settings screen, end-of-run badges, and a final report-card summary.

## Current Features

- Multi-stage Sales Engineering interview process with randomized question pools and answer order.
- Company-specific rules and visual identity for each process profile.
- Interview Pressure meter that rises and falls based on answers, Prep Cards, recovery choices, and random events.
- Limited-use Prep Cards during questions.
- Between-round recovery choices.
- Non-repeating between-stage random events.
- Process ID/run seed support for repeatable testing.
- Final summary with outcome, stats, style, pressure, prep/recovery/event highlights, badges, and company rule context.
- Runtime-generated UI and animated video-call viewport with lightweight transitions, feedback animation, and procedural SFX hooks.
- Settings screen for audio, motion, run seed, and fullscreen options.

## Run In Unity

1. Open the project folder in Unity.
2. Open `Assets/Scenes/InterviewRoom.unity`.
3. Press Play.
4. Use the mouse or keyboard shortcuts:
   - `1`, `2`, `3`: choose visible answer options
   - `Q`, `W`, `E`: use Prep Cards before selecting an answer
   - `1` through `5`: choose between-round recovery options when that screen is active
   - `Enter`: continue feedback, stage transition, or random event screens
   - `Esc`: pause/resume or return to menu from the pause overlay

## Windows Standalone Build

1. Open `File > Build Profiles` or `File > Build Settings`, depending on your Unity version.
2. Select `Windows, Mac, Linux` / `Standalone`.
3. Set the target platform to `Windows`.
4. Confirm `Assets/Scenes/InterviewRoom.unity` is the enabled scene in `Scenes In Build`.
5. Click `Switch Platform` if Unity is not already on Windows Standalone.
6. Click `Build`.
7. Choose an empty output folder, for example `Builds/FinalRound_Windows`.
8. Run the generated `Final Round.exe`.

## Build Readiness Notes

- The active build scene is `Assets/Scenes/InterviewRoom.unity`.
- The project uses Unity's Input System backend; keyboard shortcuts use `Keyboard.current` in that mode.
- Runtime UI and the animated video-call panel are generated from scripts at play/build time.
- Audio feedback is optional and generated procedurally when no sound files are assigned.
- Settings use PlayerPrefs for current-machine persistence.
- Reduce Motion is available from Settings for reducing pulse/scale animation.

## Known Limitations

- Prototype balance is intentionally lightweight and may still need more full-run testing.
- The video-call viewport is cosmetic only.
- The game is built around 1920x1080 and 1366x768 style layouts; unusual aspect ratios may need more UI tuning.
- No save system, localization, or controller support yet.
- Accessibility is currently limited to the Inspector-only `reduceMotion` toggle.
- Company/process rules and pressure are intentionally lightweight, not a full interview simulation system.
