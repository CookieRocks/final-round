# Final Round Prototype v1.0 RC14 Playtest Guide

## What This Prototype Is

Final Round is a first-person interview-room prototype about surviving a tense final-round cybersecurity presales interview.

The player walks into a meeting room, sits at the interview chair, answers three panel questions, sees subtle judgement reactions, then receives a post-interview outcome email and scorecard.

## Controls

- `WASD` - Walk before sitting.
- Mouse - Look around before sitting.
- `E` - Sit when looking at the interview chair.
- `R` - Restart the run.
- `Esc` - Pause/menu.
- `F1` - Open or close debug tools.

## Expected Player Flow

1. Start near the meeting room.
2. Read the HUD objective: `Find the interview chair.`
3. Walk into the room.
4. Find the highlighted chair.
5. Look at the chair until `Press E to sit` appears.
6. Press `E`.
7. Camera moves to the seated interview view.
8. Movement and mouse-look are disabled.
9. Answer three cybersecurity presales questions.
10. Watch the brief reaction after each answer.
11. See the `Inbox: 1 new message` transition.
12. Read the outcome email.
13. Open the scorecard.
14. Press `R` or use the restart button to reset.

## Playtest Mode

Playtest mode is enabled by default in the generated interview flow.

In playtest mode:

- Debug tools are hidden by default.
- `F1` still opens debug tools.
- Forced outcomes are off at the start.
- Deterministic seed mode can be enabled from debug tools.
- The version label remains visible and subtle.

## Debug Controls

Press `F1` to open debug tools.

- `Toggle Deterministic Seed` - Switch random question selection to deterministic seed mode.
- `Cycle Seed` - Increment the current deterministic seed.
- `Preset Strong Seed` - Set deterministic seed `1`.
- `Preset Pass Seed` - Set deterministic seed `2`.
- `Preset Hold Seed` - Set deterministic seed `3`.
- `Preset Reject Seed` - Set deterministic seed `4`.
- `Force Strong Pass` - Force the final outcome email to Strong Pass.
- `Force Pass` - Force the final outcome email to Pass.
- `Force Hold` - Force the final outcome email to Hold.
- `Force Reject` - Force the final outcome email to Reject.
- `Clear Forced Outcome` - Return to score-based outcomes.
- `Skip To Outcome` - Jump directly to the outcome email.
- `Restart Current Run` - Reset through the room controller.

Forced outcomes are for testing the email/result presentation only. Seed presets preserve normal scoring.

## Recommended Seed Presets

Use these to record or verify each outcome without forcing the result.

For each preset:

1. Press `F1`.
2. Click the matching preset seed button.
3. Click `Restart Current Run`.
4. Sit down and choose the listed answer indexes for questions 1, 2, and 3.

| Target outcome | Seed | Expected questions | Answer indexes | Expected scores |
| --- | ---: | --- | --- | --- |
| Strong Pass | 1 | `CTX-01`, `TECH-01`, `COMM-02` | `1, 1, 1` | `T10/C10/R6/E9` |
| Pass | 2 | `CTX-04`, `TECH-02`, `COMM-01` | `1, 1, 3` | `T10/C7/R5/E5` |
| Hold | 3 | `CTX-02`, `TECH-03`, `COMM-04` | `1, 2, 2` | `T5/C7/R3/E5` |
| Reject | 4 | `CTX-04`, `TECH-04`, `COMM-03` | `2, 3, 4` | `T7/C3/R3/E5` |

An editor-only helper is also available:

`Final Round > Find RC14 Seed Presets`

It searches deterministic seeds and logs example seed/question/answer combinations to the Unity console.

## End-of-Run Console Summary

At the end of each run, the Unity console logs:

- Seed
- Selected question IDs
- Selected answer indexes
- Final Technical, Commercial, Rapport, and Energy scores
- Outcome
- Whether forced outcome was active
- Whether deterministic seed mode was active

## Known Limitations

- Room art is still runtime-generated greybox geometry.
- Interviewers are placeholder panels, not characters.
- Reactions are text and tiny placeholder visual changes only.
- The outcome email is an overlay styled like a laptop/email client, not true in-world UI.
- Question bank has 12 questions total.
- Seed presets still require selecting the listed answer indexes.
- Debug tools are functional but not polished for production.

## What Feedback To Collect

Ask playtesters to focus on clarity, tension, answer quality, and believability rather than art polish.

Useful observations:

- Where did they look first?
- Did they understand when the interview started?
- Did they notice the interviewer reactions?
- Did they feel judged or just processed by UI?
- Did the final email feel connected to their answers?
- Did the scorecard feel fair?

## Tester Feedback Questions

- Did you know where to go at the start?
- Did the sit-down moment feel clear?
- Did the interview feel tense?
- Did the answer choices feel too obvious?
- Did the reactions feel natural or random?
- Did the outcome email feel believable?
- Did the scorecard explain the result?
- What was confusing?
- What was boring?
- What would you want more of?
