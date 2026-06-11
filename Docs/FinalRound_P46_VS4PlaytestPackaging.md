# Final Round - P46 VS4 Playtest Packaging and Feedback Pass

_Date: 2026-06-11_  
_Status: Packaging docs prepared; Unity player build not run from this shell_

## Summary

P46 prepares the promoted VS4 loop for external playtesting by verifying packaging inputs, updating the playtest build metadata from VS2 to VS4, and creating a focused VS4 playtest guide.

No gameplay systems, jobs, recruiter branches, Room scoring, question data, outcome thresholds, Aftermath flow, art, or effects were changed.

## Files Changed

- `Assets/Editor/FinalRoundPlaytestBuild.cs`
- `Docs/FinalRound_P46_VS4PlaytestGuide.md`
- `Docs/FinalRound_P46_VS4PlaytestPackaging.md`
- `FinalRound_GamePlan_Current.md`

## Build And Package Readiness

Verified Build Settings include the required scenes:

- `Assets/Scenes/InterviewRoom.unity`
- `Assets/Scenes/DeskScene.unity`
- `Assets/Scenes/AftermathRoom.unity`

Verified `FinalRoundPlaytestBuild` uses the same required scene list.

Updated playtest package metadata:

- Build folder: `Builds/Playtest/FinalRound_VS4_Windows/`
- Executable: `Builds/Playtest/FinalRound_VS4_Windows/FinalRound.exe`
- Zip package: `Builds/Playtest/FinalRound_VS4_Playtest.zip`
- Packaged README source: `Docs/FinalRound_P46_VS4PlaytestGuide.md`
- Packaged README filename: `README_Playtest.md`

Build outputs remain ignored by git through `.gitignore`:

- `Builds/`
- `Build/`
- `Temp/`
- `Obj/`
- `Logs/`

## Build Regression Fix

The first VS4 player build exposed a Room presentation regression: the chair, panel figures, and assigned/imported props rendered, but the generated room shell was effectively lost in black space.

P46 now hardens `TheRoomPrototypeController` so generated room primitives are safer in player builds:

- generated primitives copy Unity's existing primitive material before tinting, instead of relying only on runtime shader lookup by name;
- existing generated Room shell/furniture primitives are refreshed at startup with build-safe materials;
- if the generated Room shell is missing while the room controller already has seat/player references, the floor and walls are rebuilt at runtime.

Manual verification still requires rebuilding the player package and checking the Desk -> Room path visually.

## Asset Verification

Verified the three VS4 job listing assets exist under:

```text
Assets/Resources/FinalRound/VS4/JobListings/
```

Assets present:

- `northbridge-security-presales.asset`
- `helios-cloud-security-consultant.asset`
- `redgate-risk-compliance-presales.asset`

The promoted VS4 docs and P45 readiness review confirm selected-job data is populated and unique.

## Copy Leak Check

Checked current scripts/docs for obvious VS4 packaging and selected-job copy leaks.

Findings:

- Runtime Room standby copy no longer hardcodes `Maya has forwarded your profile`.
- Remaining `enterprise security presales` strings are Northbridge-specific fallback/baseline copy.
- Older VS2 docs still intentionally mention Northbridge/Maya as historical milestone documentation.
- `FinalRoundPlaytestBuild` previously used VS2 output names and the old README path; P46 updates these to VS4.

## Unity Build Attempt

The Unity player build was not run from this shell because no reliable local Unity batch build command is configured here.

Use the existing Unity editor menu:

1. Open the project in Unity.
2. Run `Final Round > Validate Playtest Build Settings`.
3. Run `Final Round > Build Playtest Windows`.
4. Confirm `Builds/Playtest/FinalRound_VS4_Windows/FinalRound.exe` exists.
5. Run `Final Round > Package Latest Playtest Build`.
6. Confirm `Builds/Playtest/FinalRound_VS4_Playtest.zip` exists.
7. Send the zip to testers.

## Playtest Guide

Created:

```text
Docs/FinalRound_P46_VS4PlaytestGuide.md
```

The guide includes:

- prototype summary;
- current VS4 flow;
- controls;
- available jobs;
- what to test;
- known limitations;
- safety boundary for Aftermath;
- focused feedback questions for general flow, Job Board, carry-through, Room, Aftermath, and replay.

## Known Limitations For Distribution

- A fresh VS4 player build is required after the generated Room material/shell fix.
- Unity player build still needs to be run manually through the Unity editor menu.
- Job Board UI is functional prototype quality and may still be text-heavy.
- Room UI/layout issues from earlier slices remain outside P46 scope.
- Recruiter and application mechanics are shared across jobs.
- Room questions are shared across jobs.
- Aftermath art/audio/animation are still prototype quality.

## Recommended Next Milestone

Recommended after external playtest:

```text
Final Round - Prototype P47: VS4 Playtest Feedback Triage
```

Focus P47 on:

- collecting tester notes;
- classifying blockers versus polish;
- deciding whether the next slice should be a Waiting/InBox slice, recruiter deepening, or Room UI polish;
- avoiding new jobs until VS4 feedback proves the current three-job choice is understood.

## Validation

Required:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

Validation result:

- `dotnet build "Assembly-CSharp.csproj"`: passed, 0 warnings, 0 errors.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: passed, 0 warnings, 0 errors.
