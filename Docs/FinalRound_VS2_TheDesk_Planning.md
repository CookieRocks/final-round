# Final Round - VS2 Planning: The Desk and Job Pipeline

## Source Documents Read

- `FinalRound_GamePlan_Current.md`
- `Docs/FinalRound_VS1_TheRoom.md`
- `Docs/FinalRound_P25_VS1ReadinessReview.md`

These are treated as the source of truth for the current direction.

## Recommendation

VS2 should be:

`Final Round - Vertical Slice VS2: The Desk`

The Desk should be a focused beginning-of-job-search slice that feeds into the existing VS1 Room. It should not become a career sim, open world, procedural job board, or full CV editor.

Recommended first implementation:

- One company.
- One role.
- One job listing.
- One recruiter.
- One short recruiter interaction.
- One preparation/application path into the existing Room.
- One return-to-desk inbox outcome after the Room.

## Core Emotional Target

VS2 should feel like the start of a job hunt:

- uncertain,
- cautiously hopeful,
- slightly desperate without humiliating the player,
- full of recruiter ambiguity,
- shaped by trade-offs between confidence, honesty, energy, and overclaiming.

The player should feel the pressure of trying to become hireable without becoming dishonest or exhausted.

## Minimal VS2 Player Flow

1. Player starts at a desk/laptop.
2. Laptop opens to one job opportunity.
3. Player inspects job listing sections:
   - role overview,
   - responsibilities,
   - requirements,
   - nice-to-haves,
   - salary/process notes,
   - red flags.
4. Player chooses an application strategy.
5. Player receives a recruiter message.
6. Player responds to the recruiter.
7. Player handles a short recruiter screen/message exchange.
8. Recruiter invites the player to the final interview.
9. CandidateState / RunState is carried into VS1: The Room.
10. VS1 plays with light modifiers.
11. Outcome returns to the desk/inbox.

## Desk Interaction Model

The player should interact with a small desk/laptop surface rather than a full simulation.

Initial interactions:

- Click laptop/job board.
- Inspect job listing sections.
- Highlight or acknowledge red flags.
- Choose application angle.
- Reply to recruiter.
- Choose one prep action.
- Continue to final interview or withdraw.

Keep interactions low in count but high in trade-off value. The player should not be asked to fill out a form field-by-field.

## First Shared Run-State Variables

Use 7 variables for the first build:

| Variable | Meaning | Desk source | Room effect |
| --- | --- | --- | --- |
| Role Fit | How naturally the candidate fits the role. | Listing interpretation, application angle. | Small starting score modifier and outcome copy tone. |
| Recruiter Trust | How credible/manageable the recruiter thinks the candidate is. | Recruiter replies, salary/process tone. | Warmer or colder opening line. |
| Candidate Confidence | How steady the candidate feels entering the interview. | Application strategy, recruiter screen. | Starting Rapport/Energy modifier or intro tone. |
| Energy | Remaining stamina. | Prep choice, cautious vs aggressive actions. | Starting Energy modifier. |
| Overclaim Risk | Risk created by exaggerating fit or experience. | Application/recruiter overstatements. | Harder technical pressure or harsher Architect reactions. |
| Technical Readiness | Preparation for security/presales scrutiny. | Prep action, honest self-assessment. | Starting Technical modifier. |
| Rapport Momentum | Warmth carried from early communication. | Recruiter trust-building choices. | Stage intro/reaction warmth modifier. |

Do not implement every candidate variable from the high-level plan yet. Company Interest, Stress, Commercial Readiness, Compensation Alignment, and Red Flag Awareness can wait unless a specific P26/P27 design need appears.

## How VS2 Should Affect VS1

Use small, explainable modifiers first.

Potential effects:

- Starting score modifiers:
  - Technical Readiness -> Technical.
  - Role Fit -> Commercial or Rapport.
  - Energy -> Energy.
  - Candidate Confidence -> Rapport or Energy.
- Intro variations:
  - High Recruiter Trust creates a warmer panel intro.
  - Low Recruiter Trust creates a colder "we only have limited signal" intro.
- Technical pressure:
  - High Overclaim Risk can make the Architect's stage intro sharper.
  - It can also bias reactions toward awkward/concerned when technical answers are weak.
- Outcome email copy:
  - High Role Fit can soften Hold/Reject wording.
  - High Overclaim Risk can add a line about follow-up concerns.
- Debug summary:
  - F1 panel should show carried VS2 state in development builds.

Do not change VS1 scoring thresholds in VS2. Apply small starting modifiers or copy/reaction variants only.

## Data Model Proposal

### JobListingData

ScriptableObject representing the single role/opportunity.

Fields:

- `jobId`
- `companyName`
- `roleTitle`
- `summary`
- `responsibilities[]`
- `requirements[]`
- `niceToHaves[]`
- `salaryRange`
- `processNotes`
- `redFlags[]`
- `greenFlags[]`
- `defaultRoomProfileId`

### ApplicationChoiceData

ScriptableObject or serializable data for application strategy choices.

Fields:

- `choiceId`
- `label`
- `bodyText`
- `feedbackText`
- `roleFitDelta`
- `recruiterTrustDelta`
- `candidateConfidenceDelta`
- `energyDelta`
- `overclaimRiskDelta`
- `technicalReadinessDelta`
- `rapportMomentumDelta`

### RecruiterMessageData

ScriptableObject or serializable message node for recruiter communication.

Fields:

- `messageId`
- `senderName`
- `subject`
- `messageText`
- `responseChoices[]`
- `nextMessageId`

### RecruiterScreenQuestionData

ScriptableObject for one short screen question.

Fields:

- `questionId`
- `prompt`
- `responseChoices[]`
- `successText`
- `concernText`

### CandidateState / RunState

Runtime object persisted through the opportunity.

Fields:

- `selectedJobId`
- `applicationChoiceId`
- `recruiterPathId`
- `roleFit`
- `recruiterTrust`
- `candidateConfidence`
- `energy`
- `overclaimRisk`
- `technicalReadiness`
- `rapportMomentum`
- `roomOutcome`

Start values can be neutral, for example 0 or 5 depending on the chosen stat convention. Use one convention consistently.

### RoomModifierData

Generated or ScriptableObject-based mapping from CandidateState into The Room.

Fields:

- `technicalStartModifier`
- `commercialStartModifier`
- `rapportStartModifier`
- `energyStartModifier`
- `stageIntroVariant`
- `architectPressureVariant`
- `outcomeEmailVariant`
- `reactionWarmthModifier`
- `debugSummary`

## Scene Architecture Recommendation

Use a separate Desk scene that transitions into `InterviewRoom`.

Why:

- It protects the working VS1 room from unnecessary refactor churn.
- It lets The Desk develop its own camera, laptop, lighting, and interaction model.
- It keeps the emotional space distinct: private job-search anxiety at the desk, public judgement in the room.
- It makes future hub/inbox work easier.

Implementation shape:

- `DeskScene` owns job listing, application, recruiter, and prep states.
- A lightweight `FinalRoundRunState` object persists between scenes.
- `InterviewRoom` reads the run state if present and falls back to neutral VS1 behavior if started directly.
- After outcome, Room can return to Desk with outcome summary.

Alternative considered:

- Single scene/state inside `InterviewRoom`.
  - Lower scene-management overhead.
  - But it risks turning the room scene into a hub and makes VS1 harder to preserve.
- Lightweight UI-only hub.
  - Fast to build.
  - But weaker spatial identity and less useful for the "desk/laptop" fantasy.

Chosen approach: separate Desk scene with a small persistent run-state bridge.

## Minimal Content Scope

First VS2 content should include:

- Company: one fictional cybersecurity vendor.
- Role: one Senior Solutions Engineer / Security Presales role.
- Listing: one authored job listing with a few ambiguous requirements.
- Recruiter: one named recruiter with a short message thread.
- Recruiter interaction: 2-3 prompts or message choices.
- Prep action: choose one of a few trade-offs.
- Transition: move into existing VS1 Room.
- Return: outcome email/inbox summary appears back at Desk.

## Milestone Breakdown

### P26: VS2 Design / Architecture

- Finalize scene architecture.
- Define `CandidateState`.
- Define data assets.
- Define Desk-to-Room modifier rules.
- No gameplay implementation beyond stubs if needed.

### P27: Desk Scene Prototype

- Create Desk scene.
- Add laptop/desk interaction shell.
- Add basic navigation or fixed camera.
- Add placeholder job listing view.
- Add transition placeholder.

### P28: Job Listing and Application Choices

- Implement one `JobListingData`.
- Implement listing inspection.
- Implement application strategy choices.
- Apply choices to CandidateState.

### P29: Recruiter Screen / Message Exchange

- Add one recruiter message thread or short call.
- Add recruiter response choices.
- Modify Recruiter Trust, Role Fit, Energy, Overclaim Risk, and Rapport Momentum.

### P30: Run-State Modifiers into The Room

- Carry CandidateState into VS1.
- Apply starting score modifiers.
- Add warm/cold stage intro variants.
- Add debug readout for carried state.

### P31: Return-to-Desk Outcome / Inbox

- Return from Room to Desk.
- Show outcome/inbox summary.
- Support restart/new opportunity loop.

### P32: VS2 Readiness Review

- Smoke test full Desk -> Room -> Desk loop.
- Audit modifiers.
- Decide whether to tag VS2.

## Risks

- Scope creep into a job board or career sim.
- Too many run-state variables too early.
- Overwriting the clean VS1 room flow with Desk assumptions.
- Making the Desk feel like a menu instead of a playable emotional space.
- Carry-over modifiers making P24/P25 outcome balance drift too far.
- Scene transition complexity if run state is not kept small.

## What Not To Build Yet

- Multiple companies.
- Procedural job listings.
- Full CV editor.
- Open-world apartment/office.
- Calendar system.
- Salary negotiation system.
- Multiple concurrent opportunities.
- Save/load campaign structure.
- New interview question categories.
- VS1 room art overhaul.
- Scoring threshold retune.
- AI/freeform recruiter dialogue.

## Recommended Next Coding Prompt

`Final Round - Room Prototype P26: VS2 Desk Architecture Skeleton`

Prompt shape:

- Create a new Desk scene or scene stub.
- Add `CandidateState` / `FinalRoundRunState` data model.
- Add neutral direct-to-Room fallback so VS1 still works standalone.
- Add placeholder Desk controller with laptop interaction shell.
- Do not implement job listing content choices yet.
- Do not modify VS1 scoring thresholds.
- Run `dotnet build "Assembly-CSharp.csproj"`.
