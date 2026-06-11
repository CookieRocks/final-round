# Final Round - Vertical Slice VS4: The Job Board

_Date promoted: 2026-06-11_  
_Status: Promoted vertical slice_

## Summary

`Final Round - Vertical Slice VS4: The Job Board` adds a small authored opportunity-choice layer to the existing hiring pipeline.

VS4 proves that Final Round becomes more replayable and strategic when the player chooses between a few distinct roles before entering the shared Desk -> application -> recruiter -> Room -> outcome loop.

This slice deliberately remains compact:

- three authored job listings;
- one selected job per run;
- selected-job defaults applied once;
- shared application and recruiter mechanics;
- shared six-question Room interview;
- selected-job context carried through copy and summary;
- existing Reject-only Aftermath preserved.

VS4 does not add procedural jobs, new interview questions, Room variants, new scoring rules, or recruiter branches.

## Player Flow

```text
Main Menu
  -> Start Job Search
  -> Desk
  -> Opportunity Board
  -> choose one of three authored jobs
  -> View Listing
  -> Application Strategy
  -> Recruiter Screen
  -> The Room
  -> Outcome Email
  -> Return to Desk
  -> Inbox / Process Summary
  -> optional Reject-only Aftermath
  -> Start New Run
```

The player can switch jobs before submitting an application. Once the application is confirmed, the selected job is locked for the run.

## Available Jobs

### Northbridge Cyber Systems

- Role: `Senior Solutions Engineer - Security Presales`
- Job ID: `northbridge-security-presales`
- Recruiter: Maya Patel, Senior Talent Partner
- Profile: Balanced baseline

Northbridge is the familiar baseline. It maps cleanly to the existing Room structure and tests customer context, technical judgement, commercial framing, and communication discipline evenly.

Risk/reward:

- lowest novelty risk;
- clearest fit for the current Room content;
- still has broad senior scope, vague process pacing, and stakeholder ambiguity.

Default CandidateState effects:

- all default deltas are neutral.

### Helios Cloud Platform

- Role: `Cloud Security Solutions Consultant`
- Job ID: `helios-cloud-security-consultant`
- Recruiter: Iris Chen, Cloud Security Talent Partner
- Profile: Technical stretch / architecture-heavy

Helios is the stretch opportunity. It looks more ambitious and architecture-heavy, but it raises the risk that earlier claims will be tested in detail.

Risk/reward:

- stronger technical-growth fantasy;
- clearer platform/security architecture pressure;
- higher overclaim risk;
- more demanding technical scrutiny.

Default CandidateState effects:

- Candidate Confidence +1
- Energy -1
- Overclaim Risk +1
- Technical Readiness -1

### Redgate Financial Risk

- Role: `Risk & Compliance Presales Consultant`
- Job ID: `redgate-risk-compliance-presales`
- Recruiter: Eleanor Shaw, Risk Solutions Recruiting Lead
- Profile: Commercial / compliance bureaucracy

Redgate shifts pressure toward regulated stakeholders, careful claims, legal/process ambiguity, and commercial patience.

Risk/reward:

- lower deep cloud architecture demand than Helios;
- stronger stakeholder and process pressure;
- more legal/compliance ambiguity;
- better fit for careful commercial positioning.

Default CandidateState effects:

- Role Fit +1
- Energy -1
- Technical Readiness -1
- Rapport Momentum +1

## Selected-Job Carry-Through

The selected job affects the run through context and small default state changes, not through new scoring rules.

Carry-through points:

- `CandidateState.SelectedJobId` is the source of truth.
- `CandidateState.JobDefaultDeltasAppliedJobId` prevents repeated default-delta stacking.
- Job listing sections show selected job content.
- Application feedback references the selected company/role profile.
- Recruiter identity and intro copy use selected job recruiter fields.
- Room intro can include selected job `roomContextLine`.
- Room outcome email can include selected job `outcomeContextLine`.
- Desk inbox sender, subject, and context reflect selected job.
- Process Summary shows selected job ID, company, role, profile, recruiter, process note, application choice, recruiter path/responses, Room outcome, and Aftermath status.

No Room question selection, score deltas, thresholds, or six-question stage structure changed for VS4.

## Controls

### Desk

- `E`: open laptop.
- `Space`: open laptop.
- Mouse click on laptop: open laptop.
- Mouse: select job cards, listing sections, application strategies, recruiter replies, and Desk actions.
- `F1`: toggle Desk debug readout.
- `Esc`: Desk pause menu.

Desk actions include:

- `View Listing`
- `Back to Job Board`
- `Application Strategy`
- `Confirm Application`
- `Recruiter Message`
- `Continue to Interview`
- `Process Summary`
- `Start New Run`
- `Main Menu`

### Room

- Move/look before sitting: existing first-person controls.
- `E`: sit when prompted at the interview chair.
- Mouse: select interview answers and outcome buttons.
- `Esc`: pause menu.
- `F1`: Room debug panel.

### Aftermath

After a Reject outcome:

- `Clear the Room` appears at the Desk.
- `WASD`: move.
- Hold right mouse: look.
- Left click / `E` / `Space`: Feedback Hammer interaction.
- Return to Desk when finished.

## Fallback Behaviour

Fallback remains safe:

- If the full three-job board is unavailable or invalid, Desk falls back to Northbridge/default copy.
- Desk can recover the three VS4 job assets from `Resources/FinalRound/VS4/JobListings`.
- If selected job data is missing, selected-job copy falls back to Northbridge/default text.
- Direct `InterviewRoom` launch remains neutral and does not apply Desk modifiers or selected-job context.
- Direct `DeskScene` remains usable.
- Reject-only Aftermath remains gated behind an active Desk run and Reject outcome.

## Known Limitations

- Job Board UI is functional prototype quality, not final art.
- Job card comparison could use stronger visual hierarchy.
- Job default deltas are intentionally small and may be subtle without debug.
- Recruiter screen uses one shared interaction structure for all companies.
- Room interview questions remain shared across all jobs.
- Room outcome email and Desk inbox are still prototype UI surfaces.
- Main Menu is still hosted by `InterviewRoom`.
- Room score saturation remains inherited technical debt.
- Question assets still live under the legacy `RC11` Resources path.

## Intentionally Deferred

- Additional jobs or procedural job generation.
- Full CV/profile editor.
- Salary negotiation.
- Separate recruiter branches per company.
- Job-specific recruiter questions.
- Job-specific Room question banks.
- New Room variants.
- Room scoring changes.
- Outcome threshold changes.
- Aftermath flow changes.
- Final Desk/Job Board art pass.

## Future Expansion Ideas

- Add clearer visual risk/reward comparison on job cards.
- Add a lightweight prep choice after recruiter screen.
- Add job-specific recruiter follow-up branches.
- Add a Waiting/Inbox slice after outcome or Hold.
- Add more company archetypes once the three-job loop has enough playtest signal.
- Add a candidate profile/archetype layer that changes how jobs feel before applying.
- Add job-specific Room variants only after the shared pipeline is stable.

## Validation

Promotion readiness was reviewed in:

- `Docs/FinalRound_P45_VS4ReadinessReview.md`

P45 found no VS4 blockers and recommended promotion.

