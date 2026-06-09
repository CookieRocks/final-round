# Final Round Prototype v1.0 RC7 Playtest Checklist

## Core Room Flow

- [ ] Open `Assets/Scenes/InterviewRoom.unity`.
- [ ] Start Play Mode.
- [ ] Confirm HUD shows `Prototype v1.0 RC7`.
- [ ] Confirm the player starts in the room/waiting area.
- [ ] Find the highlighted interview chair.
- [ ] Look at the chair and confirm `Press E to sit` appears.
- [ ] Press `E`.
- [ ] Confirm the seated camera faces the three interviewer placeholders.
- [ ] Confirm player movement and mouse-look are disabled after sitting.

## Interview Flow

- [ ] Confirm the first interview question appears after the seated delay.
- [ ] Select an answer.
- [ ] Confirm answer buttons hide/disable.
- [ ] Confirm a reaction line appears.
- [ ] Confirm interviewer placeholders subtly react.
- [ ] Confirm question 2 appears after the reaction delay.
- [ ] Confirm question 3 appears after the second reaction delay.
- [ ] Complete all three questions.

## Outcome Flow

- [ ] Confirm the post-interview email outcome appears.
- [ ] Confirm the email includes from, subject, opening, outcome, and feedback copy.
- [ ] Confirm `Open Scorecard` is visible.
- [ ] Click `Open Scorecard`.
- [ ] Confirm Technical, Commercial, Rapport, and Energy scores appear only after opening the scorecard.
- [ ] Click `Restart`.
- [ ] Confirm the run resets to the starting position.
- [ ] Confirm movement is re-enabled after restart.
- [ ] Confirm active question UI, reaction text, outcome email, and scorecard are cleared after restart.

## Pause And Cursor

- [ ] Press `Esc` during the room flow.
- [ ] Confirm the pause menu appears.
- [ ] Confirm the mouse unlocks and pause menu buttons are clickable.
- [ ] Resume.
- [ ] Confirm room control returns correctly.

## Debug Panel

- [ ] Press `F1`.
- [ ] Confirm the RC7 debug panel appears.
- [ ] Confirm the panel shows the current seed, seed mode, question indexes, forced outcome state, version, and branch label.
- [ ] Toggle deterministic seed on.
- [ ] Cycle seed.
- [ ] Restart current run.
- [ ] Confirm the same deterministic seed reproduces the same three-question selection.
- [ ] Toggle deterministic seed off.
- [ ] Restart multiple times and confirm question selection can vary.
- [ ] Force `Strong Pass`, skip to outcome, and confirm the Strong Pass email appears.
- [ ] Force `Pass`, skip to outcome, and confirm the Pass email appears.
- [ ] Force `Hold`, skip to outcome, and confirm the Hold email appears.
- [ ] Force `Reject`, skip to outcome, and confirm the Reject email appears.
- [ ] Clear forced outcome and confirm normal scoring determines the result again.
- [ ] Press `F1` again and confirm the debug panel hides.
