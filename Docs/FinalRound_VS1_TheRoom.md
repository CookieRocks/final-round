# Final Round - Vertical Slice VS1: The Room

## What This Vertical Slice Demonstrates

VS1 proves the core end-to-end experience for `Final Round`: a player enters an interview room, sits opposite a human interview panel, answers a staged cybersecurity presales interview, receives judgement reactions, and gets an outcome email with a scorecard.

This slice demonstrates:

- A first-person room entry and seated interview transition.
- A tense interview-room composition.
- Three interviewer roles with human placeholders.
- ScriptableObject-driven interview content.
- Hidden scoring across technical, commercial, rapport, and energy dimensions.
- A complete six-question run from start to outcome.
- Outcome distribution tuned for the six-question runtime flow.
- Debug tooling for repeatable playtesting.

## Player Flow

1. Start from the main menu.
2. Choose `Start Interview Process`.
3. Enter the room and walk to the interview chair.
4. Look at the chair until `Press E to sit` appears.
5. Press `E` to sit.
6. Complete three interview stages:
   - Stage 1: Customer Context.
   - Stage 2: Technical Judgement.
   - Stage 3: Commercial Pressure.
7. Answer six questions total.
8. Watch panel reactions after each answer.
9. Receive the outcome email.
10. Open the scorecard or restart the run.

## Controls

- Move: standard first-person movement.
- Look: mouse/camera look before sitting.
- `E`: sit when the chair prompt is active.
- `Esc`: pause/resume or close menu overlays.
- `F1`: toggle room interview debug tools.
- Mouse: select answers and outcome/scorecard buttons.

## Debug Controls

The F1 debug panel includes:

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

Useful deterministic playtest paths:

| Seed | Expected reachable outcome path | Example answer indexes |
| ---: | --- | --- |
| 1 | StrongPass | 1, 1, 1, 1, 1, 1 |
| 2 | Pass | 1, 1, 1, 1, 1, 1 |
| 3 | Hold | 1, 1, 1, 2, 3, 3 |
| 4 | Reject | 1, 1, 1, 2, 1, 3 |

## Current Content

- 24-question ScriptableObject bank.
- 6-question staged runtime flow.
- 3 interview stages:
  - Customer Context.
  - Technical Judgement.
  - Commercial Pressure.
- 4 outcome types:
  - StrongPass.
  - Pass.
  - Hold.
  - Reject.

## Outcome Distribution

Current six-question staged audit distribution:

- StrongPass: 10.1%.
- Pass: 44.3%.
- Hold: 35.1%.
- Reject: 10.4%.

## Known Limitations

- Interviewers are standing prefabs hidden by table occlusion, not true seated characters.
- No rigging, sitting animation, facial animation, voice, or lip sync.
- Room art is still prototype-quality.
- Outcome email is overlay-based rather than fully diegetic.
- UI can still feel heavy in places.
- Score saturation remains a known scoring-model concern.
- Question assets still live under the legacy `RC11` resource folder.
- Unity player build depends on local Unity licensing.

## Intentionally Deferred

- Full character animation pipeline.
- Voice acting, audio dialogue, or lip sync.
- Larger question categories or branching interview structure.
- Broader company/campaign progression.
- Full score normalisation refactor.
- Localization.
- Controller support.
- Save/progression system.
- Major room art pass.

## What VS2 Might Explore

- Score normalisation or delayed score clamping for richer scorecard behaviour.
- More polished seated interviewer presentation.
- A second room or company scenario.
- Expanded question bank with more role-specific pressure.
- Better diegetic email/scorecard presentation.
- Audio and ambience pass.
- Stronger accessibility pass beyond current reduce-motion/audio settings.
- Build packaging and external playtest distribution.

## Validation

Manual Unity smoke test passed before promotion from P25 to VS1.

`dotnet build "Assembly-CSharp.csproj"` should pass before checkpointing this milestone.
