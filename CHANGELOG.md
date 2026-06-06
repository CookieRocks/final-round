# Changelog

## v0.8 - Audio Feedback and Sound Hooks

Released: 2026-06-06

### Overview

`v0.8` adds a lightweight optional audio feedback layer. The game now has subtle runtime-generated SFX for common UI and interview moments, while keeping clip override hooks available for later asset replacement.

### Added

- Added `FinalRoundAudioManager` with procedural short tones for:
  - UI click
  - answer selected
  - Prep Card used
  - Recovery Choice selected
  - continue
  - random event appears
  - stage complete
  - pressure warning
  - final positive outcome
  - final negative outcome
  - badge reveal
- Added Inspector controls for `masterVolume`, `sfxVolume`, and `muteAudio`.
- Added optional AudioClip override fields for each major sound event.
- Added one-shot pressure warning audio when Interview Pressure enters the high-pressure band.
- Added badge reveal audio when the `BADGES EARNED` section appears.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Windows standalone batch build was attempted, but Unity aborted because this project is already open in another Unity instance.

## v0.7 - End-of-Run Badges

Released: 2026-06-06

### Overview

`v0.7` adds compact end-of-run badges to the final outcome screen. Badges summarize outcome strength, dominant interview style, performance signals, risky/funny choices, and pressure recovery without changing scoring, questions, company rules, Prep Cards, Recovery Choices, random events, or run seed behavior.

### Added

- Added `RunBadge` data with name, short description, type, and accent color.
- Added a `BADGES EARNED` section to the final outcome screen.
- Added badge priority logic:
  - outcome badge first when relevant
  - dominant style badge
  - one risk/funny badge
  - one performance badge
  - remaining qualifying badges up to the display cap
- Added final-run debug logging for earned badges.
- Added badge conditions for strong offers, dominant style reads, commercial alignment, high-pressure survival, no-damage event runs, recovery choices, stat shape, and VP/final confidence signal.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Windows standalone batch build was attempted, but Unity aborted because this project is already open in another Unity instance.

## v0.6.4 - 2D Call Panel Polish

Released: 2026-06-06

### Overview

`v0.6.4` lightly polishes the 2D call panel introduced in v0.6.3. It keeps the same UI structure while making participant tiles larger, avatar silhouettes cleaner, and tile surfaces a little less placeholder-like.

### Changed

- Updated in-game prototype label to `Prototype v0.6.4`.
- Reduced unused padding in the call participant area.
- Enlarged one-, two-, and three-participant tile layouts.
- Replaced the text-glyph avatar head with generated UI image shapes.
- Added a simple shoulder highlight layer for more readable silhouettes.
- Lightened participant tile backgrounds and preserved inactive tile dimming.
- Kept the existing single active-speaker border concept.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Windows standalone batch build was attempted, but Unity aborted because this project is already open in another Unity instance.

## v0.6.3 - 2D Call Panel Replacement

Released: 2026-06-06

### Overview

`v0.6.3` replaces the normal player-facing 3D RenderTexture interview viewport with a clean generated 2D Unity UI call panel in the existing right-side viewport area. The old 3D backdrop controller remains in the project as a disabled fallback path.

### Changed

- Updated in-game prototype label to `Prototype v0.6.3`.
- Replaced the RawImage/RenderTexture viewport content with a generated 2D call panel inside the runtime canvas.
- Disabled normal 3D viewport generation by default to avoid unnecessary backdrop camera and RenderTexture work.
- Added runtime cleanup for legacy 3D/sample scene objects and forced the display camera to render UI-only so old environments cannot appear behind the game.
- Added 2D call structure:
  - themed panel background
  - top bar with stage label and `LIVE CALL`/`STANDBY`
  - participant tiles
  - simple avatar silhouettes
  - role label bars
  - active speaker border
- Preserved stage-specific participant layouts:
  - Recruiter Screen: 1 tile
  - Hiring Manager: 1 tile
  - Technical Panel: 3 tiles
  - VP Round: 2 tiles
- Kept feedback state hiding the call panel for readability.
- Moved company flavor and pressure reaction into 2D UI colors, dimming, and active-border pulse.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Windows standalone batch build was attempted, but Unity aborted because this project is already open in another Unity instance.

## v0.6.2 - Viewport Polish Tune

Released: 2026-06-06

### Overview

`v0.6.2` lightly polishes the readable video-call viewport without changing its architecture. It tones down the frame, keeps call chrome simple, improves active/inactive tile contrast, and strengthens company flavor through color rather than extra geometry.

### Changed

- Updated in-game prototype label to `Prototype v0.6.2`.
- Lightened the outer call frame so participant tiles remain the focus.
- Kept the top call bar readable with a stage title and simple `LIVE CALL` status.
- Tuned inactive tile dimming and active speaker tile intensity.
- Made the active speaker effect a single clearer top edge with subtle pressure-sensitive pulse.
- Refined participant tile/body/head color variation to reduce clone-like panels.
- Kept company flavor in top bar, background tone, tile accents, and active border colors.
- Reduced high-pressure frame glow intensity to keep the viewport calm.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Windows standalone batch build was attempted, but Unity aborted because this project is already open in another Unity instance.

## v0.6.1 - Video-Call Viewport Polish

Released: 2026-06-06

### Overview

`v0.6.1` polishes the v0.6 video-call viewport after readability testing. It keeps the call-first generated RenderTexture scene but reduces visual clutter, improves role labels, and makes stage/company/pressure cues feel more like a lightweight interview-call UI.

### Changed

- Updated in-game prototype label to `Prototype v0.6.1`.
- Reduced outer viewport clutter to a single clean call frame and a readable top call bar.
- Kept participant tiles as the focus by removing tiny status text, side rails, signal lines, status dots, inner panels, and competing highlight effects.
- Enlarged single-participant, two-participant, and three-participant layouts so cards fill the viewport more confidently.
- Improved bottom role label bars and font sizing for readability at in-game viewport size.
- Added subtle per-participant avatar and tile color variation.
- Added clear stage chrome:
  - `ONE-TO-ONE SCREEN`
  - `HIRING MANAGER CALL`
  - `TECHNICAL PANEL`
  - `FINAL LEADERSHIP CALL`
- Kept company flavor in background, tile, top-bar, and accent colors rather than extra geometry.
- Kept pressure effects subtle by tightening active tile/frame color and pulse intensity without flicker.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Windows standalone batch build was attempted, but Unity aborted because this project is already open in another Unity instance.

## v0.6.0 - Animated Video-Call Viewport

Released: 2026-06-06

### Overview

`v0.6.0` evolves the decorative 3D interview room into a stylized animated video-call viewport. The new generated scene keeps the existing RenderTexture architecture and gameplay systems intact while making the viewport reflect the active stage, company process, and Interview Pressure.

### Added

- Added a fake remote-interview call screen inside the existing viewport.
- Added generated interviewer participant tiles with simple head-and-shoulders avatars.
- Added stage-specific panel layouts:
  - Recruiter Screen: 1 recruiter tile.
  - Hiring Manager: 1 hiring manager tile.
  - Technical Panel: 3 technical panel tiles.
  - VP Round: 2 executive panel tiles.
- Added company-specific call backgrounds and accent treatments.
- Added active-speaker pulse behavior during question screens.
- Added pressure-based viewport tension through glow, pulse speed, accent color, and high-pressure flicker.
- Added subtle avatar bob/nod animation.
- Added standby/muted visual state for non-question screens.

### Changed

- Updated in-game prototype label to `Prototype v0.6`.
- Updated README feature text from a generic 3D room viewport to the animated video-call viewport.
- Reframed the viewport as the video call itself, with a near orthographic camera on the call board instead of a room/laptop composition.
- Stopped generating the desk, laptop, wall display, and old interviewer silhouette for the viewport scene.
- Enlarged participant tile layouts so roles are readable at normal viewport size.
- Replaced the solid active-speaker overlay with thin border bars, high-contrast unlit participant cards, larger foreground avatars, clearer role labels, status dots, and a simple standby card.
- Rebuilt participant tile layout so cards, avatars, labels, dots, and borders are positioned explicitly instead of relying on parent scaling that could hide the contents behind the tile face.
- Added high-contrast label plates and oversized foreground role text so single-participant stages clearly read as Recruiter or Hiring Manager.
- Simplified the video-call viewport down to one call frame, larger participant cards, avatar silhouettes, bottom role bars, and a single active-speaker top edge; removed tiny headers, status text, side rails, signal lines, status dots, inner panels, and extra highlight clutter.
- Kept feedback screen viewport hiding behavior so answer feedback remains readable.

### Validation

- `dotnet build Assembly-CSharp.csproj` passes with 0 warnings and 0 errors.
- Windows standalone batch build was attempted, but Unity aborted because this project is already open in another Unity instance.

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
