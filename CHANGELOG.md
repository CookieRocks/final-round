# Changelog

## v0.5.0 - Pressure And UI Juice Pass

Released: 2026-06-06

### Overview

`v0.5.0` adds an Interview Pressure meter and lightweight UI animation so Final Round feels more tense and responsive moment to moment. It keeps the existing scoring thresholds, company rules, question pools, Prep Cards, Recovery Choices, random events, final summary, and 3D viewport structure intact.

### Added

- Added Interview Pressure as a run-level value from 0 to 100.
- Added pressure states:
  - `Calm`
  - `Focused`
  - `Tense`
  - `Spiralling`
- Added compact Interview Pressure display to the Candidate Read panel.
- Added pressure changes from answers, Prep Cards, Recovery Choices, and random events.
- Added pressure delta feedback in answer, recovery, and random event panels.
- Added pressure-based final summary lines for very low or very high final pressure.
- Added subtle pressure reactions to the 3D viewport lighting.
- Added lightweight UI animation:
  - answer card fade-in
  - selected answer pulse/dim
  - feedback panel fade-in
  - stat/pressure flash feedback
  - screen fade/scale transitions
  - quick final outcome section reveal
- Added Inspector-only `reduceMotion` toggle to reduce pulse/scale motion.

### Changed

- Updated in-game prototype label to `Prototype v0.5`.
- Compactened Candidate Read stat text to make room for Interview Pressure.
- Tuned question and feedback panel layout after adding pressure and animation.
- Kept feedback panel animation layout-safe by using fade only.

### Fixed

- Fixed cramped question header/stage intro text after v0.5 pressure layout changes.
- Fixed feedback panel animation overlap caused by moving a layout-controlled panel.
- Fixed pressure meter presentation so it does not dominate the question screen.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Confirmed v0.5 Windows standalone build exists under `Builds/v0.5`.
- Completed v0.5 smoke testing.

### Build

- Created Windows prototype build:
  - `Builds/v0.5/Final Round.exe`
- Tagged release:
  - `v0.5.0`

## v0.4.0 - Company Identity Pass

Released: 2026-06-06

### Overview

`v0.4.0` makes each company profile feel mechanically and visually distinct. Company-specific process rules now affect the run, appear in the Process Briefing and final summary, and drive the 3D viewport's atmosphere so it reinforces the selected process instead of feeling generic.

### Added

- Added explicit company process rules:
  - Big SaaS Vendor: `Structured Process`
  - Startup Rocketship: `High Chaos, High Recovery`
  - Security Vendor: `Risk Framing Matters`
  - AI Hype Company: `Chaos Can Sell`
  - Legacy Enterprise: `Slow Process`
- Added player-facing rule names, rule descriptions, and compact hints to company profiles.
- Added active rule display to the Process Briefing.
- Added compact in-run rule context to the question screen.
- Added active rule details to the final summary.
- Added debug logging for run start and final outcome rule modifiers/effects.
- Added debug company profile forcing:
  - `useDebugCompanyProfile`
  - `debugCompanyProfileIndex`

### Changed

- Big SaaS Vendor now has fewer random events but a higher `Offer Recommended` bar.
- Startup Rocketship now has more random events, stronger positive recovery choices, and harsher random event Energy losses.
- Security Vendor now requires both Technical Credibility and Commercial Alignment to clear the offer gate.
- AI Hype Company now tolerates a little chaotic style, can lightly reward the first chaotic answer, and flags excessive chaos.
- Legacy Enterprise now has fewer random events, lower between-stage Energy recovery, and can reward high final stamina.
- Updated the 3D viewport to use company-specific primitive props, colors, lighting, accent moods, and wall display labels.
- Preserved the existing RenderTexture viewport architecture and no-movement interview-room presentation.
- Tuned final outcome layout to avoid overlap with the expanded rule summary.
- Shortened the compact in-run rule line to reduce wrapping.

### Fixed

- Fixed the final outcome screen's top summary/body overlap after adding v0.4 rule text.
- Fixed AI chaos forgiveness so early chaotic answers are evaluated against the pre-answer chaos count.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Unity editor script compile/domain reload completed successfully during validation.
- Confirmed v0.4 Windows standalone build was compiled.
- Completed v0.4 smoke test.

### Build

- Created Windows prototype build for v0.4.
- Tagged release:
  - `v0.4.0`

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
