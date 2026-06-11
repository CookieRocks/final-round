# Final Round - Prototype P34: External Playtest Packaging

## Summary

P34 adds a repeatable Unity Editor workflow for producing a Windows external playtest build of the promoted VS2 loop.

The milestone does not add gameplay, content, Room scoring changes, question changes, threshold changes, CandidateState rule changes, recruiter content, Room art, or VS3 functionality.

## What Changed

Added editor-only build tooling:

- `Assets/Editor/FinalRoundPlaytestBuild.cs`

Added menu items:

- `Final Round > Build Playtest Windows`
- `Final Round > Package Latest Playtest Build`
- `Final Round > Validate Playtest Build Settings`

Added playtest documentation:

- `Docs/FinalRound_Playtest_README.md`
- `Docs/FinalRound_P34_ExternalPlaytestPackaging.md`

Updated active docs:

- `Docs/FinalRound_VS2_TheDesk.md`
- `FinalRound_GamePlan_Current.md`

## Build Menu Behaviour

`Final Round > Build Playtest Windows`:

- verifies required scene assets exist;
- ensures required scenes are enabled in Build Settings;
- builds a Windows x64 player;
- outputs to `Builds/Playtest/FinalRound_VS2_Windows/`;
- creates `FinalRound.exe`;
- copies `Docs/FinalRound_Playtest_README.md` into the build folder as `README_Playtest.md`;
- logs build result, output path, build size, and warning count;
- throws a build failure if the required scenes are missing or the build fails.

Required scenes:

- `Assets/Scenes/InterviewRoom.unity`
- `Assets/Scenes/DeskScene.unity`

`Final Round > Package Latest Playtest Build`:

- requires a completed build at `Builds/Playtest/FinalRound_VS2_Windows/`;
- refreshes the copied playtest README;
- creates `Builds/Playtest/FinalRound_VS2_Playtest.zip`;
- logs the package path.

`Final Round > Validate Playtest Build Settings`:

- verifies the required scenes exist;
- repairs Build Settings if either required scene is missing or disabled;
- logs the expected output folder.

## Output Paths

Build folder:

```text
Builds/Playtest/FinalRound_VS2_Windows/
```

Executable:

```text
Builds/Playtest/FinalRound_VS2_Windows/FinalRound.exe
```

Package:

```text
Builds/Playtest/FinalRound_VS2_Playtest.zip
```

Generated build folders and zips are protected by the existing `.gitignore` rule for `Builds/`.

The external playtest package checked into the repository root is:

```text
FinalRound_VS2_Playtest.zip
```

## How To Build For Playtesters

1. Open the project in Unity.
2. Run `Final Round > Validate Playtest Build Settings`.
3. Run `Final Round > Build Playtest Windows`.
4. Confirm `FinalRound.exe` exists under `Builds/Playtest/FinalRound_VS2_Windows/`.
5. Run `Final Round > Package Latest Playtest Build`.
6. Send `Builds/Playtest/FinalRound_VS2_Playtest.zip` to playtesters, or copy it to the repository root as `FinalRound_VS2_Playtest.zip` when preparing a checked-in package checkpoint.

## What Failed In P33

P33 attempted a basic command-line Windows player build using `-buildWindows64Player`.

Observed result:

- Unity returned exit code `0`;
- no player files were produced;
- no log output was produced under the requested build folder.

P34 avoids relying on that bare command by adding a project-owned editor build method that explicitly validates scenes, writes to a known output folder, copies the playtest README, and reports the Unity `BuildReport` result.

## Command-Line Option

The editor menu item can also be called from a command line on a licensed Unity install:

```text
Unity.exe -quit -batchmode -projectPath "<project path>" -executeMethod FinalRoundPlaytestBuild.BuildPlaytestWindows -logFile "Builds/Playtest/unity-p34-build.log"
```

Package after a successful build:

```text
Unity.exe -quit -batchmode -projectPath "<project path>" -executeMethod FinalRoundPlaytestBuild.PackageLatestPlaytestBuild -logFile "Builds/Playtest/unity-p34-package.log"
```

If Unity returns without producing files or logs, run the menu item from the Unity Editor and check the Console for build pipeline/licensing messages.

## Troubleshooting

If the build fails because a scene is missing:

- run `Final Round > Create/Repair Desk Scene`;
- run `Final Round > Validate Playtest Build Settings`;
- confirm both `InterviewRoom` and `DeskScene` appear in Build Settings.

If packaging fails:

- run `Final Round > Build Playtest Windows` first;
- confirm `Builds/Playtest/FinalRound_VS2_Windows/FinalRound.exe` exists;
- confirm the README exists at `Docs/FinalRound_Playtest_README.md`.

If the command-line build produces no files:

- use the Unity Editor menu item directly;
- confirm the Unity install has Windows build support;
- confirm local Unity licensing is active;
- check whether antivirus or permissions block writes under `Builds/Playtest/`.

## Known Limitations

- The build script prepares a Windows x64 player only.
- The build still depends on local Unity licensing and installed Windows build support.
- There is no installer; distribution is a zipped folder.
- The menu remains hosted by `InterviewRoom`.
- P34 does not add telemetry, crash reporting, save files, or a feedback form.

## Validation

Run:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

Optional validation:

- run `Final Round > Build Playtest Windows`;
- run `Final Round > Package Latest Playtest Build`;
- confirm the ignored build folder and zip exist under `Builds/Playtest/`.

P34 local command-line attempt:

- Unity executable checked at `C:\Program Files\Unity\Hub\Editor\6000.4.10f1\Editor\Unity.exe`;
- attempted `-executeMethod FinalRoundPlaytestBuild.BuildPlaytestWindows`;
- PowerShell returned immediately without a useful exit code;
- no `FinalRound.exe` was produced;
- no `unity-p34-build.log` was produced.

Conclusion: the code path is prepared and compiles, but this machine still needs a manual Unity Editor build attempt from the menu item to confirm player-build output.

## Recommended Next Milestone

If the playtest package builds cleanly:

`Final Round - Prototype P35: External Playtest Feedback Triage`

Suggested scope:

- collect first external playtest notes;
- classify blockers, confusion points, and polish;
- fix only issues that block the VS2 playtest loop;
- avoid VS3 expansion until feedback has been reviewed.
