# Final Round - Room Prototype P25 VS1 Readiness Review

## Summary

P25 reviews whether the current room prototype is ready to be promoted to:

`Final Round - Vertical Slice VS1: The Room`

Recommendation: ready for VS1 after one manual Unity smoke test pass.

No gameplay systems, new questions, score deltas, room art, UI layout, or stage structure were changed in P25.

P25 smoke-test fixes:

- Updated the visible build label to `Room Prototype P25`.
- Removed colon punctuation from two answer option texts that displayed as blank in Unity.
- Reduced sideways interviewer reaction tilt so the Sales Director no longer appears to lean sideways during agreement or awkward emphasis.

## Current Player Flow

1. The player starts at the main menu.
2. `Start Interview Process` enters the room flow.
3. The player walks to the interview chair.
4. Looking at the chair shows `Press E to sit`.
5. Pressing `E` seats the player, disables movement/look, and moves the camera to the seated viewpoint.
6. The staged interview begins:
   - Stage 1: Customer Context: 2 questions.
   - Stage 2: Technical Judgement: 2 questions.
   - Stage 3: Commercial Pressure: 2 questions.
7. Each answer updates hidden scoring and triggers a judgement reaction.
8. After 6 questions, the outcome email appears.
9. The player can open the scorecard.
10. The player can restart the run.

## Controls

- Move: standard first-person movement.
- Look: mouse/camera look before sitting.
- `E`: sit when the chair prompt is active.
- `Esc`: pause/resume or back out of menu overlays.
- `F1`: toggle room interview debug tools.
- Mouse: select answers and outcome/scorecard buttons.

## Debug Controls

The room interview debug panel includes:

- Toggle deterministic seed.
- Cycle seed.
- Preset Seed 1.
- Preset Seed 2.
- Preset Seed 3.
- Preset Seed 4.
- Force Strong Pass.
- Force Pass.
- Force Hold.
- Force Reject.
- Clear Forced Outcome.
- Skip To Outcome.
- Restart Current Run.

Recommended deterministic playtest seeds:

| Seed | Expected reachable outcome path | Example answer indexes |
| ---: | --- | --- |
| 1 | StrongPass | 1, 1, 1, 1, 1, 1 |
| 2 | Pass | 1, 1, 1, 1, 1, 1 |
| 3 | Hold | 1, 1, 1, 2, 3, 3 |
| 4 | Reject | 1, 1, 1, 2, 1, 3 |

These are not forced outcomes. They are deterministic question selections plus answer paths found against the P24/P25 threshold model.

## Checklist Result

Static/code review status:

- Main menu appears: code-verified.
- `Start Interview Process` enters the room flow: code-verified.
- Player can walk to the chair: code path present, requires manual Unity smoke test.
- `Press E to sit` appears correctly: code-verified, requires visual smoke test.
- Sitting disables movement/look: code-verified.
- Camera moves to seated viewpoint: code-verified.
- Stage intro appears: code-verified, requires visual overlap smoke test.
- Six questions are asked: code-verified.
- Reactions appear after answers: code-verified.
- Outcome email appears: code-verified.
- Scorecard opens: code-verified.
- Restart works: code-verified.
- Esc pause works: code-verified.
- F1 debug works: code-verified.
- Deterministic seed works: code-verified, seed paths listed above.
- Forced outcomes work: code-verified.

Manual Unity smoke test still recommended because this review did not run the interactive Unity player.

## Hygiene Review

Repository state before P25 edits was clean.

Observed hygiene:

- No untracked files before P25 work.
- No accidental SampleScene, URP/settings, tutorial, README, or changelog churn in git status.
- No missing-script markers found by text scan in scenes/prefabs/resources.
- No obvious magenta/error-shader markers found by text scan.
- Largest imported asset folder is `Assets/Prefabs/low-poly-ordinary-man-in-shirt-and-pants` at about 5.7 MB.
- `Assets/Prefabs` totals about 7.4 MB.
- No large unused asset folder appears to block VS1.

Limitations of this hygiene pass:

- Unity editor visual validation was not run here.
- Missing material/pink shader confirmation still needs one visual editor/player smoke test.
- Unity player build status depends on local Unity licensing.

## Known Limitation Triage

| Issue | Classification | Notes |
| --- | --- | --- |
| Score saturation remains high | Technical debt | P24 threshold tuning hit target outcome bands, but Commercial and Technical still saturate often. P25 does not refactor scoring. |
| Characters are not truly seated | Polish later | Current table occlusion reads well enough for VS1. No rigging or animation planned for this milestone. |
| No voice/lip sync | Intentionally deferred | Text-driven prototype is acceptable for VS1. |
| Room art is still partially prototype | Polish later | Room communicates the interview scenario and is good enough for a vertical slice label. |
| Outcome email is overlay-based | Polish later | Functional and readable; can be integrated diegetically later. |
| UI may still be heavy in places | Polish later | Acceptable for VS1 if manual smoke test confirms faces and text remain readable. |
| Question bank path uses legacy `RC11` folder | Technical debt | Non-blocking. Rename/migration can wait to avoid resource-path churn before VS1. |
| Unity player build may require local licensing | VS1 blocker for distributable build only | `dotnet build` passes, but a shareable player build requires Unity licensing to be healthy locally. |

## VS1 Blockers

No code or content blocker was found in static review.

Manual smoke-test blockers to check before declaring VS1:

- Main menu to seated interview path works in the Unity editor/player.
- No pink/missing-material assets are visible in the room.
- Stage intro and question UI do not severely block interviewer faces.
- Scorecard/restart/debug controls work in the actual runtime.
- Unity player build can be produced on a licensed Unity install if VS1 needs to be distributed.

## Non-Blocking Limitations

- Interviewers are standing prefabs hidden by table occlusion, not seated rigs.
- Character reactions are simple emphasis states, not animation.
- The room remains stylized/prototype quality.
- Score saturation remains a future scoring-model concern.
- The question Resources path still carries the legacy `RC11` name.
- Legacy RC docs remain in the repo as historical checkpoints.

## What Qualifies This As VS1

This is ready to become `Final Round - Vertical Slice VS1: The Room` if the manual smoke test passes because it contains a complete playable loop:

- entry/menu,
- spatial room interaction,
- seated interview composition,
- staged question flow,
- visible human panel,
- hidden scoring,
- reactions,
- final email outcome,
- scorecard,
- restart/debug tooling,
- balanced six-question outcome distribution.

## Build Validation

`dotnet build "Assembly-CSharp.csproj"` passed:

- 0 warnings.
- 0 errors.

## Final Recommendation

Ready for VS1 after manual Unity smoke test.

If the smoke test finds visual/material issues or a Unity player build cannot be produced because of licensing, handle that as a narrow P26 hardening pass before tagging VS1.
