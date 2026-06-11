# Final Round — Current High-Level Game Plan

_Last updated: 2026-06-10_  
_Current milestone context: VS2 complete; P34 external playtest packaging in progress_

## 1. Purpose of this document

This document captures the current high-level direction for **Final Round** before expanding beyond the first playable interview-room vertical slice.

It is not a fixed design bible. It is a working plan to help us keep the project coherent as we build future vertical slices and learn from playtests.

The current thinking is:

> **Final Round is a job-search pressure simulator where each stage of the hiring pipeline becomes its own focused psychological arena.**

The game should not become a giant life sim, an open-world office game, or a generic interview training app. It should stay focused on the emotional loop of modern job hunting: hope, uncertainty, performance, judgement, waiting, rejection, and occasional relief.

---

## 2. Current project state

### VS1 complete: The Room

The first major playable slice is effectively:

> **Final Round — Vertical Slice VS1: The Room**

The current build contains a complete room-interview loop:

1. Main menu.
2. Start interview process.
3. Walk into the interview room.
4. Find the highlighted chair.
5. Press `E` to sit.
6. Movement/look disable.
7. Seated camera faces the interview panel.
8. Six-question staged cybersecurity presales interview begins.
9. Answers update hidden scoring.
10. Interviewer reactions appear after each answer.
11. Final outcome email appears.
12. Player can open the scorecard.
13. Player can restart/debug.

Current runtime interview structure:

| Stage | Focus | Questions per run |
|---|---|---:|
| Stage 1 | Customer Context | 2 |
| Stage 2 | Technical Judgement | 2 |
| Stage 3 | Commercial Pressure | 2 |

Current question bank:

| Category | Question count |
|---|---:|
| Context/customer scenario | 8 |
| Technical/security judgement | 8 |
| Commercial/executive pressure | 8 |
| **Total** | **24** |

Current tuned six-question outcome distribution:

| Outcome | Approx. share |
|---|---:|
| Strong Pass | 10.1% |
| Pass | 44.3% |
| Hold | 35.1% |
| Reject | 10.4% |

This is a good distribution for a tense final-round prototype: Strong Pass is possible but earned, Pass is common for credible performance, Hold captures mixed results, and Reject remains meaningfully possible.

### Current known limitations

These are acceptable for VS1 unless playtesting proves otherwise:

- Interviewer characters are not truly seated; they are standing low-poly prefabs hidden by table occlusion.
- No voice, lip sync, facial animation, or rigged character pipeline.
- Room art is still prototype quality, though no longer pure greybox.
- Outcome email is overlay-based rather than fully diegetic/in-world.
- UI may still need future polish.
- Score saturation remains technical debt, especially around Technical and Commercial scoring in six-question runs.
- Question assets still live under a legacy `RC11` resources folder path.
- A distributable Unity player build depends on local Unity licensing/build setup.

---

## 3. High-level game thesis

The original seed of the game is not just “answer interview questions.”

The stronger full-game concept is:

> **Survive the hiring pipeline without losing your credibility, energy, or sense of self.**

The player is not only trying to “win” an interview. They are trying to manage the whole job-search process:

- choosing which jobs are worth applying for,
- interpreting vague job listings,
- deciding how much to tailor or exaggerate,
- talking to recruiters,
- preparing under limited energy,
- performing in different interview formats,
- waiting for feedback,
- dealing with rejection, ghosting, or offers,
- learning what kind of candidate they are becoming.

The current Room slice proves the end-stage interview pressure. The next slices should build the path that leads there.

---

## 4. Design pillars

### 4.1 Corporate pressure, not fantasy combat

The tension should come from social judgement, ambiguity, status, and uncertainty — not combat, puzzles, or horror tropes.

### 4.2 Serious, but slightly awkward

The tone should feel believable and uncomfortable. It can have dry corporate absurdity, but it should not become pure comedy.

### 4.3 Systems hidden under human-facing moments

The game should track stats and modifiers, but the player should mostly experience them through:

- tone of messages,
- reactions,
- room mood,
- recruiter warmth/coldness,
- interview panel behaviour,
- outcome emails,
- scorecard after the fact.

Do not turn the main experience into visible stat bars everywhere.

### 4.4 Every stage should feel different

A recruiter call, a job listing, a technical panel, and waiting for an email should not all play like the same multiple-choice UI.

Each stage should have a different interaction style and emotional focus.

### 4.5 Small, composable vertical slices

Build one focused vertical slice at a time. Each should be playable, testable, and useful on its own, but designed to connect into the larger hiring pipeline.

---

## 5. Overall game structure

The emerging full-game structure is a hiring pipeline made of connected stages:

```text
The Desk
  -> Job Listing
  -> Application Strategy
  -> Recruiter Contact
  -> Screening / Hiring Manager
  -> Technical / Presales Panel
  -> Final Round
  -> Waiting
  -> Outcome
  -> Next Opportunity / Run Summary
```

The player should not wander a giant world. Instead, the game should move between focused spaces or interfaces that represent the emotional state of each stage.

Possible physical/digital spaces:

| Stage | Possible space/style | Emotional focus |
|---|---|---|
| The Desk | Home desk / laptop hub | Hope, doubt, fatigue |
| Job Listing | Job board / listing analysis | Is this worth it? |
| Application | CV/profile/application builder | Authenticity vs overclaiming |
| Recruiter | Phone/Zoom/email interface | Confidence vs desperation |
| Hiring Manager | Video call or softer meeting room | Fit, motivation, communication |
| Technical Panel | Whiteboard / architecture room | Ambiguity, credibility, judgement |
| Final Round | Formal interview room | Pressure, performance, evaluation |
| Waiting | Inbox / desk / silent room | Uncertainty and loss of control |
| Offer/Reject | Email / call / negotiation | Relief, frustration, trade-off |

---

## 6. Core gameplay loop

At the highest level:

```text
Choose opportunity
  -> make preparation/application choices
  -> enter hiring stage
  -> respond under pressure
  -> receive hidden/visible consequences
  -> progress, stall, reject, or restart
```

The loop should produce trade-offs rather than obvious right answers.

Example choices:

- Apply for a stretch role.
- Tailor your CV honestly.
- Overclaim experience to pass the recruiter screen.
- Ask salary early.
- Preserve energy instead of preparing.
- Prepare technical content but neglect rapport.
- Be direct about product limitations.
- Be commercially polished but technically vague.

The game is strongest when success is not just “pick the best answer,” but “decide what kind of candidate you are becoming.”

---

## 7. Shared run-state concept

Future slices should connect through a shared run-state model.

Possible shared variables:

| Variable | Meaning |
|---|---|
| Role Fit | How naturally the player fits the opportunity. |
| Recruiter Trust | How much the recruiter believes the player is viable. |
| Company Interest | How much momentum the company has behind the candidate. |
| Candidate Confidence | How steady the player appears and feels. |
| Energy | Ability to stay engaged and present. |
| Stress | Pressure carried into later stages. |
| Overclaim Risk | Risk created by exaggerating experience or fit. |
| Technical Readiness | Preparation for technical/security judgement. |
| Commercial Readiness | Ability to connect technical work to business outcome. |
| Rapport Momentum | Warmth built with people across the process. |
| Compensation Alignment | Whether expectations match the role. |
| Red Flag Awareness | Whether the player has spotted risks in the company/role. |

These should not all be visible all the time. They are internal levers that future stages can use.

Example carry-over effects:

- High Overclaim Risk can trigger harder technical scrutiny later.
- High Recruiter Trust can make the hiring manager intro warmer.
- Low Energy can make interview reactions colder.
- High Role Fit can soften some outcome email language.
- Low Compensation Alignment can create a negotiation/waiting issue later.
- Good prep choices can add Technical Readiness but cost Energy.

---

## 8. Vertical slice roadmap

### VS1 — The Room

**Status:** complete vertical slice.

Core question:

> Can a single interview room make the player feel judged?

Current answer: yes, enough to show someone and gather feedback.

VS1 includes:

- spatial room entry,
- sit-down interaction,
- staged six-question interview,
- hidden scoring,
- judgement reactions,
- human interviewer presence,
- outcome email,
- scorecard,
- restart/debug tools.

Future work for VS1 after feedback:

- minor UI polish,
- better character assets,
- more room identity props,
- possible score normalisation if scorecard starts feeling samey,
- true build/distribution hardening.

### VS2 — The Desk

**Status:** complete vertical slice.

Core question:

> Can the player feel the pressure of starting the job hunt before they ever enter the interview room?

Minimum scope:

1. Player starts at a home/job-search desk.
2. Laptop/job board/inbox interface appears.
3. One job listing is available.
4. Player inspects role details and red flags.
5. Player chooses an application strategy.
6. Player handles a recruiter message or call.
7. Choices create modifiers.
8. Player is invited to The Room.
9. VS1 starts with those modifiers applied.
10. Outcome returns to The Desk.

VS2 should use one fake company, one role, one recruiter, and one controlled path into VS1.

Do not build a giant job board yet.

Recommended first implementation:

1. Start at a desk/laptop with one available job opportunity.
2. Inspect a job listing with role details, expectations, benefits, and red flags.
3. Choose one application angle: balanced/honest, technically assertive, commercially polished, cautious/low-energy, or overclaiming.
4. Respond to one recruiter thread or short screening exchange.
5. Choose one prep action before the final interview.
6. Convert these choices into a small shared run state.
7. Transition into VS1: The Room with light modifiers applied.
8. Return the outcome to the desk/inbox.

First shared run-state variables should be deliberately small:

| Variable | First-use meaning |
|---|---|
| Role Fit | Whether the role genuinely matches the candidate's experience and interests. |
| Recruiter Trust | Whether the recruiter believes the candidate is credible and manageable. |
| Candidate Confidence | How steady the candidate feels entering the final interview. |
| Energy | How much stamina remains after applying, messaging, and preparing. |
| Overclaim Risk | How much the candidate exaggerated or implied experience they may need to defend. |
| Technical Readiness | Whether the candidate prepared enough for security/presales scrutiny. |
| Rapport Momentum | Warmth carried from early communication into the panel interview. |

Keep VS2 emotionally focused on uncertainty, cautious hope, recruiter ambiguity, and the temptation to overstate experience without making the player pathetic.

### VS3 — Recruiter Screen

Core question:

> Can a friendly, transactional recruiter interaction feel like a meaningful game stage?

Possible style:

- phone call,
- video-call UI,
- chat/email thread,
- split-screen desk interface.

Gameplay themes:

- salary expectations,
- role fit,
- availability,
- confidence,
- asking useful questions,
- deciding whether to continue.

Carry-over:

- Recruiter Trust,
- Company Interest,
- Compensation Alignment,
- Overclaim Risk,
- Energy/Stress.

### VS4 — Hiring Manager Screen

Core question:

> Can the player communicate motivation, judgement, and fit without sounding generic?

Possible style:

- warmer interview room,
- Zoom call,
- office side-room,
- less formal than the final panel.

Gameplay themes:

- why this role,
- customer empathy,
- team fit,
- conflict handling,
- explaining career moves,
- “tell me about a time…” style questions.

Carry-over:

- Role Fit,
- Rapport Momentum,
- Candidate Confidence,
- Hiring Manager Confidence.

### VS5 — Technical / Presales Panel

Core question:

> Can the player handle ambiguity, architecture judgement, and customer pressure?

This could either become an expansion of The Room or a separate technical whiteboard-style slice.

Possible style:

- whiteboard room,
- architecture review,
- shared screen,
- customer workshop simulation.

Gameplay themes:

- discovery,
- false positives,
- risk framing,
- product limitations,
- data residency,
- breach context,
- security team vs executive priorities.

Carry-over:

- Technical Credibility,
- Commercial Credibility,
- Overclaim Risk,
- Energy.

### VS6 — Waiting / Inbox

Core question:

> Can the most passive part of the hiring process still feel tense and playable?

Possible style:

- back at The Desk,
- silent inbox,
- recruiter messages,
- calendar delays,
- “we are still aligning internally” emails.

Gameplay themes:

- waiting,
- ghosting,
- choosing whether to follow up,
- pursuing another job,
- managing confidence and energy,
- interpreting vague signals.

This could be emotionally strong and mechanically lightweight.

### Later slices

Potential future slices after the core path works:

- Offer negotiation.
- Rejection recovery.
- Multiple companies/opportunities.
- Competing timelines.
- Better company/job archetypes.
- Procedural-ish job listings.
- Candidate build/archetype system.
- Longer campaign mode.

These should wait until the first connected pipeline works.

---

## 9. Proposed near-term milestone path

Current recommended path:

```text
VS1 — The Room
  -> complete and tagged
  -> small VS1 fixes only if a blocker appears

P26 — VS2 Design / Architecture
  -> planning complete
  -> CandidateState / FinalRoundRunState skeleton added
  -> Desk-to-Room bridge defined without applying Room modifiers yet

P27 — Desk Scene Prototype
  -> runtime-generated desk/laptop shell added
  -> placeholder Northbridge Jobs UI added
  -> transition scaffolding still avoids real Room modifiers

P28 — Job Listing and Application Choices
  -> authored Northbridge listing added
  -> application strategy choices added
  -> first CandidateState deltas applied at the Desk

P29 — Recruiter Screen / Message Exchange
  -> one Maya Patel recruiter screen added
  -> recruiter trust / role fit / overclaim risk modifiers added
  -> still avoids applying Desk modifiers to The Room

P30 — Run-State Modifiers into The Room
  -> CandidateState now lightly affects VS1
  -> small starting score modifiers added
  -> stage intro, reaction warmth, Architect pressure, and email context variants added

P31 — Return-to-Desk Outcome / Inbox
  -> Room outcome can return to Desk
  -> Desk shows Northbridge Mail inbox summary after the final interview
  -> process summary, main menu, and start-new-run paths added

P32 — VS2 Readiness Review
  -> review complete Desk -> Room -> Desk loop
  -> manual smoke test passed
  -> The Desk promoted to VS2

P33 â€” Post-VS2 Playtest Hardening
  -> Desk debug hidden by default
  -> main menu labels clarified for external playtesting
  -> first-time guidance and readability tightened

P34 â€” External Playtest Packaging
  -> repeatable Unity Editor Windows build menu added
  -> playtest README and package workflow added
```

Avoid adding more room polish until friend/playtester feedback returns, unless something is clearly broken.

---

## 10. How The Desk should affect The Room

The Desk should not be a detached menu. It should make the interview feel like a consequence of earlier choices.

Possible modifier examples:

| Desk/recruiter choice | Later Room effect |
|---|---|
| Tailored application honestly | Slight Role Fit / Rapport bonus. |
| Overclaimed technical experience | More Technical pressure or harsher Architect reaction. |
| Asked good recruiter questions | Warmer stage intro or small Company Interest bonus. |
| Pushed salary too early | Lower Recruiter Trust but clearer Compensation Alignment. |
| Skipped prep to preserve energy | Higher Energy, lower Technical Readiness. |
| Over-prepared technically | Higher Technical Readiness, lower Energy. |
| Ignored red flags | Later outcome may feel more ambiguous or costly. |
| Spotted role mismatch | Player can choose to withdraw or continue knowingly. |

The Room should reflect these in small ways first:

- starting score modifiers,
- stage intro line changes,
- reaction tone changes,
- email copy variants,
- debug summary of carried modifiers.

Do not make this too complex initially.

---

## 11. Technical architecture direction

Future systems likely needed:

### Shared run state

A single object or data model that persists through one job opportunity run.

Possible structure:

```text
FinalRoundRunState
  RoleFit
  RecruiterTrust
  CompanyInterest
  CandidateConfidence
  Energy
  Stress
  OverclaimRisk
  TechnicalReadiness
  CommercialReadiness
  RapportMomentum
  CompensationAlignment
  selectedJobId
  selectedCompanyId
  chosenApplicationStrategy
  recruiterOutcome
  roomOutcome
```

### Data assets

Potential ScriptableObject types:

```text
JobListingData
ApplicationStrategyData
RecruiterMessageData
RecruiterChoiceData
InterviewQuestionData
StageModifierData
CompanyProfileData
OutcomeCopyData
```

### Scene/state structure

Possible choices:

1. **Separate scenes** for Desk and Room.
2. **Single scene with state transitions**.
3. **Hybrid**: Desk scene loads Room scene when needed.

Recommendation for next step:

- Use whichever approach causes the least disruption to VS1.
- Avoid refactoring The Room until we know The Desk flow works.
- A separate Desk scene may be cleaner, but passing run-state into The Room needs to be designed carefully.

---

## 12. What not to build yet

Avoid these until the connected pipeline has been proven:

- giant procedural job board,
- dozens of companies,
- full CV editor,
- open-world office,
- real-time calendar sim,
- complex salary negotiation,
- live AI dialogue,
- voice recognition,
- lip sync,
- rigged character animation pipeline,
- fully authored office environments,
- complex save/load campaign structure,
- multiplayer/networking,
- major scoring-system refactor unless scorecard/playtests demand it.

The project has stayed alive because we have kept slices focused. Keep doing that.

---

## 13. Open questions

Things to decide after VS1 feedback and before VS2 implementation:

1. Is the player always interviewing for cybersecurity / technical presales, or can roles vary later?
2. Is the game mostly realistic, or should it gradually become surreal?
3. Does the player have a visible character/profile/archetype?
4. Should The Desk be first-person spatial, UI-driven, or a hybrid?
5. Should the game have runs with multiple job opportunities at once?
6. How punishing should rejection be?
7. Should the player be able to walk away from bad opportunities?
8. Should “success” always mean getting the offer, or sometimes recognising the role is wrong?
9. How much should earlier overclaiming come back to haunt the player?
10. Should the final outcome be offer/pass/reject, or a more nuanced career-path result?

---

## 14. Current working conclusion

Final Round should evolve from a single interview-room prototype into a connected hiring-pipeline game.

The current Room slice should become the late-stage pressure moment, not the whole game.

The next major creative step should likely be:

> **VS2: The Desk — the beginning of the job hunt, feeding into The Room.**

Build one opportunity, one recruiter, one application path, and one transition into VS1. If that works, the game has a real spine.
