# Final Round - Vertical Slice VS2: The Desk

## Summary

`Final Round - Vertical Slice VS2: The Desk` is the first connected hiring-pipeline slice.

VS2 proves that the game can start before the interview room, let the player make early job-search choices, carry those choices into the final interview, and return the outcome back to the Desk.

The promoted loop is:

```text
Main Menu
  -> The Desk
  -> Northbridge Jobs listing
  -> Application Strategy
  -> Maya Patel recruiter screen
  -> The Room
  -> Outcome email
  -> Return to Desk
  -> Northbridge Mail / Inbox
  -> Process Summary / Start New Run
```

Manual smoke test for the connected loop passed before promotion.

## What VS2 Demonstrates

VS2 demonstrates:

- a separate Desk scene and spatial laptop setup;
- a job-search entry point from the main menu;
- one authored cybersecurity presales opportunity;
- application strategy choices with meaningful trade-offs;
- one recruiter screen/message exchange;
- persistent `CandidateState` / `FinalRoundRunState`;
- light CandidateState modifiers into the existing Room interview;
- direct Room fallback protection;
- outcome return from Room to Desk;
- Desk inbox/outcome summary;
- process summary and fresh-run reset.

## Player Flow

1. Start from the main menu.
2. Select `Start Job Search`.
3. Enter `DeskScene`.
4. Open the laptop with `E`, `Space`, or mouse click.
5. Review the Northbridge Cyber Systems role.
6. Choose an application strategy:
   - Honest Fit
   - Tailored Credible
   - Aggressive
   - Quick Apply
7. Confirm the application.
8. Complete Maya Patel's three-question recruiter screen.
9. Continue to the final interview.
10. Complete the six-question Room interview.
11. Read the outcome email.
12. Use `Return to Desk`.
13. Read the Northbridge Mail inbox outcome.
14. Review the Process Summary or start a new run.

## Controls

### Desk

- `E`: open laptop.
- `Space`: open laptop.
- Mouse click on laptop: open laptop.
- Mouse: select listing sections, strategy choices, recruiter replies, and Desk actions.
- `F1`: toggle Desk debug readout.

Desk actions include:

- `View Listing`
- `Application Strategy`
- `Confirm Application`
- `Recruiter Message`
- `Continue to Interview`
- `Return to Desk` after Room outcome
- `Process Summary`
- `Start New Run`
- `Main Menu`

### Room

- Move/look before sitting: existing first-person controls.
- Room mouse look sensitivity is reduced for playtest readability and is tunable on `SimpleFirstPersonWalkController`.
- `E`: sit when prompted at the interview chair.
- Mouse: select interview answers and outcome buttons.
- `Esc`: pause/resume menu.
- `F1`: Room debug panel.

## Debug Controls

The Room debug panel includes:

- deterministic seed toggle;
- seed cycling and seed presets;
- forced outcomes;
- clear forced outcome;
- skip to outcome;
- restart current Room run;
- CandidateState debug line;
- resolved Room modifier summary.

The Desk debug readout is hidden by default for playtesting. Press `F1` in `DeskScene` to toggle it.

When visible, the Desk debug readout shows:

- active run state;
- selected job ID;
- application choice;
- recruiter path and responses;
- Room outcome;
- CandidateState values;
- target interview scene.

Debug surfaces remain intentionally available while the run-state bridge is still being tested.

## External Playtest Packaging

P34 adds a repeatable Unity Editor packaging workflow for external Windows playtests.

Unity menu items:

- `Final Round > Build Playtest Windows`
- `Final Round > Package Latest Playtest Build`
- `Final Round > Validate Playtest Build Settings`

Build output:

```text
Builds/Playtest/FinalRound_VS2_Windows/
```

Packaged zip:

```text
Builds/Playtest/FinalRound_VS2_Playtest.zip
```

The build script copies `Docs/FinalRound_Playtest_README.md` into the player folder as `README_Playtest.md`.

Generated build folders and zips are intentionally ignored by git.

## Content

VS2 currently contains:

- Company: `Northbridge Cyber Systems`
- Role: `Senior Solutions Engineer - Security Presales`
- Job ID: `NCS-SE-001`
- Recruiter: `Maya Patel`, Senior Talent Partner
- Application strategies: 4
- Recruiter screen prompts: 3
- Room question bank: 24 ScriptableObject questions
- Room runtime structure: 6 questions total
- Room stages:
  - Customer Context
  - Technical Judgement
  - Commercial Pressure
- Outcome types:
  - StrongPass
  - Pass
  - Hold
  - Reject

Current tuned Room outcome distribution remains the P24 model:

| Outcome | Approx. share |
| --- | ---: |
| StrongPass | 10.1% |
| Pass | 44.3% |
| Hold | 35.1% |
| Reject | 10.4% |

## Run-State Behaviour

`FinalRoundRunState` persists a `CandidateState` across scenes.

CandidateState stores:

- selected job ID;
- application choice ID;
- recruiter path ID;
- recruiter response IDs;
- Room outcome;
- Room modifier summary;
- Role Fit;
- Recruiter Trust;
- Candidate Confidence;
- Energy;
- Overclaim Risk;
- Technical Readiness;
- Rapport Momentum.

The Room reads active CandidateState only when `HasActiveDeskRun` is true.

Direct `InterviewRoom` launch remains neutral and does not apply Desk modifiers.

## Room Modifiers

P30 modifiers remain intentionally small:

- starting modifiers clamp to `-2..+2`;
- most paths resolve to `-1`, `0`, or `+1`;
- high overclaim risk creates scrutiny and sharper Architect pressure rather than instant punishment;
- low-energy or low-trust paths are harder but recoverable;
- positive Desk paths help but do not guarantee StrongPass.

First playtest feedback showed that these effects needed to be more visible to players. Current Room copy now surfaces carry-through through natural lines about Maya's screening notes, application notes, and earlier positioning. Normal play still does not show raw CandidateState values or numeric deltas.

No Room thresholds, question content, score deltas, or six-question stage structure were changed for VS2 promotion.

## Known Limitations

- The Desk scene is a generated prototype shell, not final art.
- The laptop UI is functional but not a final email/job-board interface.
- VS2 has one company, one role, and one recruiter.
- There is no prep/action choice yet.
- Main Menu is still hosted by `InterviewRoom`; there is no separate menu scene.
- Desk debug readout remains available through `F1`, but is hidden by default.
- Main Menu is still hosted by `InterviewRoom`; there is no separate menu scene.
- Room score saturation remains technical debt from the Room scoring model.
- Question assets still live under a legacy `RC11` resource path.
- Unity player build/distribution depends on local Unity licensing and build setup.
- Windows playtest packaging now has a project-owned Editor menu path, but it still requires local Unity licensing and Windows build support.

## Intentionally Deferred

- Multiple jobs or companies.
- Procedural job board.
- CV/profile builder.
- Salary negotiation.
- Free-text recruiter dialogue.
- Prep/action stage.
- Waiting/calendar mechanics.
- Offer negotiation.
- Full Desk art pass.
- Room art/character overhaul.
- Room threshold retuning.
- Major scoring normalization.

## What VS3 Might Explore

Recommended next direction:

`Final Round - Vertical Slice VS3: Recruiter Screen`

Possible focus:

- make the recruiter interaction its own stronger stage;
- add clearer recruiter warmth/trust feedback through message tone;
- introduce salary/process ambiguity;
- add a prep/action choice before the Room;
- keep the existing VS2 Desk -> Room -> Desk loop protected.

If the next step should be hardening rather than expansion, use a small P33 pass for:

- main-menu scene separation;
- Desk UI layout cleanup;
- debug visibility toggle;
- Unity player build/distribution cleanup.

After P34, external playtest builds should use:

- `Final Round > Build Playtest Windows`
- `Final Round > Package Latest Playtest Build`

## Validation

Promotion validation:

- Manual Unity smoke test: passed.
- `dotnet build "Assembly-CSharp.csproj"`: required before commit.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: required before commit.
