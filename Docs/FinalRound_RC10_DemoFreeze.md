# Final Round Prototype v1.0 RC10 Demo Freeze

## Current Build Flow

1. Player starts outside or near the generated meeting room.
2. HUD objective reads: `Find the interview chair.`
3. Player walks into the room and approaches the highlighted chair.
4. Looking at the chair shows: `Press E to sit`.
5. Pressing `E` seats the player, disables movement and mouse-look, and moves the camera to the seated viewpoint.
6. HUD objective changes to: `Interview in progress.`
7. After a 1 second delay, the 3-question cybersecurity presales interview begins.
8. Each answer updates hidden scores and shows a brief judgement reaction.
9. After the final reaction, the room transitions to:
   - `Inbox: 1 new message`
   - `Later that afternoon...`
10. The outcome email appears as a laptop/email-client overlay.
11. Player can open the scorecard or restart the run.

## Controls

- `WASD` - Move before sitting
- Mouse - Look before sitting
- `E` - Sit when looking at the interview chair
- `R` - Restart the current run
- `Esc` - Pause/menu flow from the existing UI
- `F1` - Toggle RC10 debug tools

## Debug Controls

The debug panel is hidden by default in demo mode. Press `F1` to open it.

- `Toggle Deterministic Seed` - Switch between random and deterministic question selection.
- `Cycle Seed` - Increment the deterministic seed for repeatable test runs.
- `Force Strong Pass` - Force the final email outcome to Strong Pass.
- `Force Pass` - Force the final email outcome to Pass.
- `Force Hold` - Force the final email outcome to Hold.
- `Force Reject` - Force the final email outcome to Reject.
- `Clear Forced Outcome` - Return to score-based outcome selection.
- `Skip To Outcome` - Jump directly to the outcome email.
- `Restart Current Run` - Reset through the room controller.

The debug panel shows the current seed, question IDs, seed mode, demo mode, and forced outcome state. Debug-only details are not shown while the panel is closed.

## Reproducing a Seeded Run

1. Press `F1`.
2. Enable deterministic seed mode.
3. Use `Cycle Seed` until the desired seed is shown.
4. Press `Restart Current Run`.
5. Sit down and answer the interview.
6. Repeating the same deterministic seed should select the same three question IDs.

At the end of each run, the Unity console logs:

- Seed mode
- Seed
- Selected question IDs
- Final Technical, Commercial, Rapport, and Energy scores
- Outcome
- Forced outcome state

## Forcing Each Outcome

1. Press `F1`.
2. Select one of:
   - `Force Strong Pass`
   - `Force Pass`
   - `Force Hold`
   - `Force Reject`
3. Complete the interview or press `Skip To Outcome`.
4. Confirm the email outcome matches the forced value.
5. Use `Clear Forced Outcome` to return to normal score-based results.

## Known Limitations

- Room geometry and props are runtime-generated placeholders.
- Interviewers are placeholder panels with small reaction hooks, not animated characters.
- Outcome email is an overlay styled as a laptop/email client, not true in-world UI.
- Question data remains in code for this prototype.
- Debug seed cycling is simple and intended for playtesting rather than production tooling.
- Pause/menu behavior is inherited from the existing UI and should be retested after any UI restructuring.

## Playtest Checklist

- Start the `InterviewRoom` scene.
- Confirm version label shows `Prototype v1.0 RC10`.
- Confirm objective reads `Find the interview chair.`
- Walk into the room and find the highlighted chair.
- Look at the chair and confirm `Press E to sit` appears.
- Press `E`.
- Confirm movement and mouse-look are disabled.
- Confirm camera moves to the seated viewpoint facing the three interviewers.
- Confirm objective changes to `Interview in progress.`
- Wait 1 second and confirm the first question appears.
- Select an answer and confirm answer buttons hide/disable.
- Confirm a reaction line appears before the next question.
- Complete all three questions.
- Confirm the inbox transition appears.
- Confirm the email outcome appears.
- Confirm `View Scorecard` opens the scorecard.
- Press `R` and confirm the run resets to standing gameplay.
- Open pause/menu and confirm the mouse can click buttons.
- Press `F1` and confirm the debug panel opens and releases the cursor.
- Enable deterministic seed, restart, and confirm the same question IDs are selected.
- Force Strong Pass, Pass, Hold, and Reject and confirm each outcome can be reached.
