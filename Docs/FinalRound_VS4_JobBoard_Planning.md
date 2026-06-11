# Final Round - VS4 Planning: The Job Board

## Summary

`Final Round - VS4: The Job Board` should test whether Final Round becomes more replayable and strategic when the player chooses which opportunity to pursue before entering the existing Desk -> recruiter -> Room pipeline.

VS4 should not become a giant procedural job-search system. The first slice should use three authored opportunities with distinct trade-offs, reuse the current Desk/laptop foundation, and carry the selected role into the existing application, recruiter, Room, inbox, and process-summary flow.

Recommended first scope:

- three job listings total;
- one selected job per run;
- current Northbridge role kept as the balanced baseline;
- two new authored roles with different risk/reward profiles;
- existing `CandidateState` remains the shared run-state model;
- existing application/recruiter/Room pipeline is reused wherever possible.

## Player Flow

Target VS4 flow:

```text
Main Menu
  -> Start Job Search
  -> Desk opens to Job Board
  -> review 3 job cards
  -> select one opportunity
  -> selected job loads listing sections
  -> choose application strategy
  -> recruiter screen reuses the existing structure with selected-job copy
  -> selected job applies starting CandidateState modifiers
  -> Room receives selected-job context and existing run modifiers
  -> outcome email returns to Desk
  -> inbox and process summary reference selected job
  -> optional Aftermath remains Reject-only
  -> post-aftermath choices remain available after completed aftermath
```

The player should be able to understand the job choice before committing, but the board should remain compact. This is a three-card decision, not a browsable marketplace.

## Job Choice Design

### 1. Northbridge Cyber Systems

| Field | Plan |
| --- | --- |
| Company | Northbridge Cyber Systems |
| Role title | Senior Solutions Engineer - Security Presales |
| Job ID | `northbridge-security-presales` |
| Summary | Existing balanced cybersecurity presales role. The candidate supports enterprise security deals, handles discovery, explains trade-offs, and survives a mixed technical/commercial final panel. |
| Risk/reward profile | Balanced baseline. No single pressure dominates, but the candidate must remain credible across technical judgement, customer empathy, and commercial framing. |
| Red flags | Broad senior scope; vague process pacing; enterprise stakeholder drag; high expectation to be polished and technical at once. |
| Green flags | Clear fit for the current Room content; familiar recruiter flow; balanced technical/commercial expectations; useful baseline for comparing other roles. |
| Starting CandidateState effects | Mostly neutral. Possible `RoleFit +1` if VS4 needs a baseline nudge; otherwise keep all default modifiers at `0`. |
| Room tone/modifier effects | Current neutral/balanced Room profile. The panel tests customer context, technical judgement, and commercial pressure evenly. |

### 2. Helios Cloud Platform

| Field | Plan |
| --- | --- |
| Company | Helios Cloud Platform |
| Role title | Cloud Security Solutions Consultant |
| Job ID | `helios-cloud-security-consultant` |
| Summary | Architecture-heavy cloud security consulting role focused on platform security, customer workshops, identity, data exposure, and executive-level risk translation. |
| Risk/reward profile | Higher technical pressure and higher overclaim risk, with stronger compensation/process appeal. Good for players willing to take a stretch role and defend details later. |
| Red flags | Broad cloud architecture expectations; easy to overstate depth; "hands-on enough" ambiguity; more detail pressure from technical stakeholders. |
| Green flags | Better perceived compensation; stronger platform brand; clearer technical growth path; strong appeal if the player wants a stretch opportunity. |
| Starting CandidateState effects | `CandidateConfidence +1`, `Energy -1`, `OverclaimRisk +1`, `TechnicalReadiness -1` by default. Application choices can offset this with preparation. |
| Room tone/modifier effects | More architecture-heavy intro copy; Architect pressure more likely if Overclaim Risk rises; outcome copy can mention gaps between platform positioning and scenario depth. |

### 3. Redgate Financial Risk

| Field | Plan |
| --- | --- |
| Company | Redgate Financial Risk |
| Role title | Risk & Compliance Presales Consultant |
| Job ID | `redgate-risk-compliance-presales` |
| Summary | Commercial and compliance-focused presales role supporting regulated financial customers, legal stakeholders, risk committees, procurement, and audit-sensitive buying processes. |
| Risk/reward profile | Lower deep technical demand than Helios, but higher commercial, compliance, stakeholder, and bureaucracy pressure. Better for players who can stay patient and precise. |
| Red flags | Process bureaucracy; legal ambiguity; committee buying; slow decisions; careful wording needed around compliance promises. |
| Green flags | Clearer business value; lower deep technical expectation; stable regulated buyer profile; strong fit for a commercially careful candidate. |
| Starting CandidateState effects | `RecruiterTrust +1`, `RapportMomentum +1`, `Energy -1`, `TechnicalReadiness +0`, `OverclaimRisk +0`. If a later field exists for bureaucracy/stress, this role should increase it. |
| Room tone/modifier effects | Less raw architecture pressure, more Sales Director/compliance stakeholder pressure. Intro and inbox copy can reference regulated customers, careful claims, and process drag. |

## Data Model Changes

Existing `JobListingData` already supports the core listing sections:

- `jobId`
- `companyName`
- `roleTitle`
- `summary`
- `responsibilities`
- `requirements`
- `niceToHaves`
- `salaryRange`
- `processNotes`
- `redFlags`
- `greenFlags`
- `defaultRoomProfileId`

Minimal VS4 additions should make this single-job model usable on a board without creating a procedural system.

Recommended additions:

- `jobCardSummary`: short copy for the three-card board.
- `difficultyProfile`: lightweight label such as `Balanced`, `Technical Stretch`, or `Compliance / Commercial`.
- `recruiterName`: display sender for the selected opportunity.
- `recruiterTitle`: optional display context for the recruiter screen.
- `defaultRoleFitDelta`
- `defaultRecruiterTrustDelta`
- `defaultCandidateConfidenceDelta`
- `defaultEnergyDelta`
- `defaultOverclaimRiskDelta`
- `defaultTechnicalReadinessDelta`
- `defaultRapportMomentumDelta`
- `roomProfileId`: can replace or alias `defaultRoomProfileId` later, but avoid churn if the existing field is already wired.
- `roomContextLine`: one selected-job line for Room intro/debug copy.
- `outcomeContextLine`: one selected-job line for outcome email/process summary.
- `processSummaryNote`: one selected-job line for the Desk summary.

Fields to avoid for VS4:

- procedural salary ranges;
- weighted random job generation;
- large company profiles;
- full recruiter conversation trees per company;
- full interview-question replacement per role.

## Recruiter Handling

Recommended VS4 approach:

Use one shared recruiter-screen mechanic with job-specific identity and copy pulled from the selected job.

This means:

- keep the current recruiter-screen interaction structure;
- do not build three separate recruiter branches;
- show the selected recruiter's name/company in the UI;
- use generic response choices where possible;
- add a small amount of job-specific prompt text for each role.

This is better than reusing Maya for all jobs because Maya is now strongly tied to Northbridge. It is also cheaper than creating a fully authored recruiter per company.

Practical first pass:

- Northbridge: Maya Patel, Northbridge Recruiting.
- Helios: a new recruiter name and sender line, but same mechanics.
- Redgate: a new recruiter name and sender line, but same mechanics.

The deeper Recruiter Screen can remain a later vertical slice.

## UI Approach

Low-risk Desk UI:

- the Desk opens to a compact `Job Board` state;
- show three job cards in one screen;
- each card shows company, role, difficulty profile, short summary, 2 green flags, and 2 red flags;
- selecting a card sets `selectedJobId`;
- a `View Listing` or `Continue` action opens the existing listing detail view;
- the existing listing detail sections remain the place for longer copy;
- no infinite scroll, search filters, saved jobs, or marketplace UI.

The player should be comparing authored trade-offs, not managing a spreadsheet.

## State Integration

`selectedJobId` should become the key that drives the rest of the run.

### CandidateState

When a job is selected:

- set `CandidateState.SelectedJobId`;
- apply the job's default CandidateState deltas once;
- then apply application and recruiter choice deltas as VS2 already does.

The job's default modifiers should be small and readable. VS4 should not retune Room thresholds.

### Application Choices

Application choices can remain mostly shared:

- honest/balanced;
- technically assertive;
- commercially polished;
- cautious/low-energy;
- overclaiming.

The feedback copy can mention the selected job:

- Helios punishes overclaiming more sharply.
- Redgate rewards careful stakeholder/process language.
- Northbridge remains balanced.

### Recruiter Copy

Recruiter UI should read from selected job fields:

- company name;
- role title;
- recruiter name;
- process notes;
- one job-specific screen prompt or intro line.

The response-choice mechanics can stay shared.

### Room Intro / Context

The Room should receive:

- selected job ID for debug;
- company/role context for intro copy;
- existing `RoomModifierResolver` output;
- optional `roomContextLine`.

Examples:

- Northbridge: balanced enterprise security panel.
- Helios: architecture scrutiny and cloud-platform claims.
- Redgate: regulated stakeholder/compliance pressure.

### Outcome Inbox Copy

The outcome email should include selected-job context without changing outcome thresholds:

- company and role in subject/header;
- existing outcome body;
- optional selected-job context line;
- existing modifier context line from `RoomModifierResolver`.

### Process Summary

The summary should show:

- selected job;
- company;
- role title;
- job risk/reward profile;
- application choice;
- recruiter path;
- Room outcome;
- aftermath completion if relevant.

## Milestone Breakdown

### P41: Job Board Architecture / Data Model

- Extend `JobListingData` minimally for board cards and default job modifiers.
- Add selected-job lookup in the Desk controller.
- Ensure existing Northbridge single-job flow still works.
- No new authored job content beyond placeholders if needed.

### P42: Three Authored Job Listings

- Author Northbridge, Helios, and Redgate listing data.
- Add card summary, green flags, red flags, process notes, and default modifiers.
- Keep application, recruiter, and Room mechanics shared.

### P43: Job Selection UI

- Add compact three-card Job Board view to the Desk.
- Selecting a job opens the existing listing detail state.
- Keep Start New Run, Main Menu, and Escape menu behaviour intact.

### P44: Selected-Job Carry-Through

- Carry selected job into application feedback, recruiter copy, Room intro/context, outcome inbox, and process summary.
- Keep Room scoring, question data, thresholds, and Aftermath flow unchanged.
- Verify non-selected jobs do not leak into the active run.

### P45: VS4 Readiness Review

- Verify all three job paths reach Room and return to Desk.
- Verify Reject-only Aftermath still works.
- Verify non-Reject paths still do not expose `Clear the Room`.
- Classify blockers, polish, technical debt, and deferred work.
- Decide whether to promote to `Final Round - Vertical Slice VS4: The Job Board`.

## Safety And Scope Boundaries

Do not add in VS4:

- dozens of jobs;
- procedural job generation;
- full CV editor;
- salary negotiation;
- calendar systems;
- new interview room variants;
- new Room scoring;
- changed question data;
- changed outcome thresholds;
- changed Aftermath flow.

VS4 is about choosing between three authored opportunities and seeing that choice matter lightly across the existing pipeline.

## Recommended Implementation Prompt

`Final Round - Prototype P41: Job Board Architecture / Data Model`

Prompt:

```text
Next milestone:
Final Round - Prototype P41: Job Board Architecture / Data Model

Read:
- Docs/FinalRound_VS4_JobBoard_Planning.md
- Docs/FinalRound_VS2_TheDesk.md
- Docs/FinalRound_P30_RunStateModifiersIntoRoom.md
- FinalRound_GamePlan_Current.md

Goal:
Prepare the existing Desk/job-listing model for a small three-role Job Board without adding full VS4 content yet.

Tasks:
1. Extend JobListingData minimally for:
   - job card summary
   - difficulty profile
   - recruiter name/title
   - default CandidateState deltas
   - room context line
   - outcome/process summary notes
2. Add Desk-side selected-job lookup/state plumbing.
3. Keep the current Northbridge path working as the default/fallback.
4. Do not add final Helios/Redgate content yet unless placeholder data is needed for compilation.
5. Do not change Room scoring, questions, thresholds, Aftermath flow, or recruiter mechanics.
6. Create Docs/FinalRound_P41_JobBoardArchitecture.md.
7. Run:
   dotnet build "Assembly-CSharp.csproj"
   dotnet build "Assembly-CSharp-Editor.csproj"

Deliver:
- files changed
- data model fields added
- how selectedJobId is handled
- test steps
- build result
- final git status

Do not commit automatically.
```
