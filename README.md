# Final Round

Prototype version: `Prototype v0.1`

Final Round is a Unity prototype about surviving a multi-stage Sales Engineering interview loop. Each run includes randomized answer order, fictional company/process modifiers, between-stage events, a cosmetic 3D interview-room viewport, and a final report-card summary.

## Run In Unity

1. Open the project folder in Unity.
2. Open `Assets/Scenes/InterviewRoom.unity`.
3. Press Play.
4. Use the mouse or keyboard shortcuts:
   - `1`, `2`, `3`: choose visible answer options
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
- Runtime UI and the 3D backdrop are generated from scripts at play/build time.
- Optional UI audio hooks exist, but no sound files are required.

## Known Limitations

- Prototype balance is intentionally lightweight and may still need more full-run testing.
- The 3D interview room is cosmetic only.
- The game is built around 1920x1080 and 1366x768 style layouts; unusual aspect ratios may need more UI tuning.
- No save system, accessibility options, localization, or controller support yet.
- Company/process modifiers are small run-to-run flavor changes, not a full simulation system.
