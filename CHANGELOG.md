# Changelog

## v0.3.0 - Tactical Choices Pass

Released: 2026-06-06

### Overview

`v0.3.0` makes Final Round feel more game-like by adding limited-use Prep Cards during questions and recovery choices between rounds. It keeps the existing scoring thresholds, question pools, company profiles, seeded runs, non-repeating random events, final report card, and 3D viewport intact.

### Added

- Added Prep Cards during normal question state:
  - `Take a Breath`: +8 Energy, 2 uses
  - `Ask a Clarifying Question`: marks the strongest Commercial Alignment answer, costs -3 Energy, 2 uses
  - `Reframe to Business Value`: next selected answer gains +5 Commercial Alignment, 1 use
- Added Prep Card feedback integration so used cards are reflected in answer feedback.
- Added Prep Card usage summary to the final report.
- Added `Q`, `W`, and `E` shortcuts for Prep Cards.
- Added Between-Round Recovery Choices after each non-final stage:
  - `Review Notes`
  - `Reframe the Business Case`
  - `Take a Walk`
  - `Message a Friendly AE`
  - `Doom-scroll Glassdoor`
- Added recovery choice confirmation state showing applied effects and updated stats.
- Added recovery choice summary to the final report.
- Added `1` through `5` shortcuts on the recovery choice screen.

### Changed

- Updated in-game prototype label to `Prototype v0.3`.
- Updated README to describe Prep Cards, recovery choices, and the new shortcuts.
- Tightened Prep Card state handling so cards only work during normal question state.
- Improved Prep Card UI states for pending, active, and exhausted cards.
- Improved recovery confirmation layout so stat lines do not overlap the Continue button.

### Fixed

- Fixed a recovery screen startup error caused by creating button text after querying for a missing label.
- Fixed recovery confirmation text clipping under the Continue button.
- Fixed stale release documentation/version text that still referred to `Prototype v0.1`.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Confirmed `Assets/Scenes/InterviewRoom.unity` remains the enabled build scene.
- Confirmed v0.3 Windows standalone build exists under `Builds/v0.3`.
- Performed a standalone smoke launch of `Builds/v0.3/Final Round.exe`.

### Build

- Created Windows prototype build:
  - `Builds/v0.3/Final Round.exe`
- Tagged release:
  - `v0.3.0`

## v0.2.0 - Replayability Pass

Released: 2026-06-06

### Overview

`v0.2.0` builds on the first playable prototype with stronger run identity and replayability. The interview process now has an explicit process briefing, a visible Process ID/run seed, non-repeating random events, and stage question pools that draw a smaller subset each run.

### Added

- Added Process Briefing screen between main menu and first question.
- Added visible Process ID and run seed support.
- Added deterministic run seed option for replay/debug.
- Added seeded randomness for company selection, question selection, answer order, random events, and flavour message selection.
- Added stage question pools with per-run draw counts:
  - Recruiter Screen: draw 4 from 10
  - Hiring Manager: draw 5 from 12
  - Technical Panel: draw 5 from 12
  - VP Round: draw 3 from 8
- Added `Questions Faced` to the final summary.

### Changed

- Random between-stage events now select without replacement during a run.
- If all random events have already fired, the game skips the event instead of repeating one.
- Progress text now reflects selected question count, not total pool size.

### Build

- Created zipped Windows prototype build:
  - `FinalRound_Windows_Prototype_v0.2.zip`
- Tagged release:
  - `v0.2.0`

## v0.1.0 - Prototype Build

Released: 2026-06-06

### Overview

`v0.1.0` is the first playable Windows prototype of Final Round. It establishes the full interview game loop, runtime UI, cosmetic 3D interview-room viewport, randomized answer ordering, company/process modifiers, and a final report-card summary.

### Core Gameplay

- Added a complete multi-stage Sales Engineering interview loop:
  - Recruiter Screen
  - Hiring Manager
  - Technical Panel
  - VP Round
- Added answer-driven stat changes for:
  - Confidence
  - Energy
  - Technical Credibility
  - Commercial Alignment
- Added interview style tracking for dominant final styles.
- Added random between-stage events.
- Added final outcomes based on total score, weak stats, and interview style.
- Expanded question content with Sales Engineering-specific topics, including AE partnership, MEDDPICC, missing features, competitive deals, CISO framing, salary, hybrid work, burnout, and offer timing.

### Replayability

- Added randomized answer order per question.
- Added optional deterministic answer seed for debugging.
- Added fictional company/process profiles:
  - Big SaaS Vendor
  - Startup Rocketship
  - Security Vendor
  - AI Hype Company
  - Legacy Enterprise
- Added lightweight company modifiers for event frequency, energy pressure, recovery, and chaotic-style pressure.

### UI And Presentation

- Added presentation polish for selected answers, feedback panels, stat changes, stage intros, and stage transitions.
- Added short recruiter-style flavour text between stages.
- Added a cosmetic 3D interview-room viewport using Unity primitives and a RenderTexture camera.
- Cleaned up backdrop camera/render setup and removed old scene clutter.
- Hid the 3D viewport during answer feedback to keep the feedback state readable.
- Added UI juice:
  - button hover/click feedback
  - short screen fades
  - optional audio hooks for key UI moments
- Added keyboard shortcuts:
  - `1`, `2`, `3` select answers
  - `Enter` continues
  - `Esc` opens/resumes pause overlay
- Added pause overlay.
- Added How To Play and About menu content.
- Added visible prototype version label.

### Final Summary

- Added a richer final outcome report card showing:
  - selected company profile
  - final outcome
  - dominant style
  - final stats
  - total score
  - strongest and weakest stats
  - strongest and weakest stages
  - helpful and damaging random events
  - strong vs risky/chaotic answer counts
  - rule-based next-run advice

### Build And Project Setup

- Prepared Windows standalone build settings.
- Set `Assets/Scenes/InterviewRoom.unity` as the active build scene.
- Added README instructions for running in Unity and exporting a Windows build.
- Created zipped Windows prototype build:
  - `FinalRound_Windows_Prototype_v0.1.zip`
- Tagged release:
  - `v0.1.0`

### Known Limitations

- Prototype balance is playable but not deeply tuned across many full runs.
- The 3D room is cosmetic only.
- UI is tuned mainly for 1920x1080 and 1366x768 style layouts.
- No save system.
- No controller support.
- No localization or accessibility settings.
- Optional audio hooks exist, but no final sound assets are included.
- Company/process modifiers are intentionally lightweight and may need clearer player-facing explanation in a future version.

### Suggested v0.2 Follow-Ups

- Add a proper detailed breakdown screen or scrollable post-run report.
- Add more outcome/style variety after more playtesting.
- Add audio assets for UI hooks.
- Add basic settings for text size, fullscreen/windowed mode, and sound.
- Tune company profile modifiers after multiple full-run tests.
- Consider replacing placeholder/runtime primitive 3D props with a more authored scene once gameplay is stable.
