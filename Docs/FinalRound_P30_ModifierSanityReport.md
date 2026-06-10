# Final Round - P30 Modifier Sanity Report

## Purpose

P30 adds small CandidateState-driven modifiers to The Room.

This is a sanity check, not a full P24-style outcome retune. P30 intentionally keeps:

- P24 outcome thresholds unchanged;
- question assets unchanged;
- answer score deltas unchanged;
- six-question structure unchanged.

## Profiles Reviewed

### Neutral Direct Start

Source:

- no active `FinalRoundRunState`

Applied modifiers:

- Technical +0
- Commercial +0
- Rapport +0
- Energy +0
- intro tone: neutral
- architect pressure: neutral
- reaction warmth: +0
- email context: none

Expected direction:

- identical to VS1 direct Room play.

Assessment:

- safe baseline.

### Positive CandidateState

Representative source:

- Role Fit +2
- Recruiter Trust +2
- Candidate Confidence +1
- Energy +0
- Overclaim Risk +0
- Technical Readiness +2
- Rapport Momentum +2

Applied modifiers:

- Technical +2
- Commercial +1
- Rapport +2
- Energy +0
- intro tone: warm
- architect pressure: neutral
- reaction warmth: +1
- email context: early screen gave the panel a useful starting point

Expected direction:

- better opening position and warmer room tone;
- still must answer well to reach Strong Pass because P24 thresholds require balanced final scores.

Assessment:

- strong but not automatic; acceptable for a clearly positive Desk path.

### Risky / High-Overclaim CandidateState

Representative source:

- Role Fit +2
- Recruiter Trust -1
- Candidate Confidence +2
- Energy -1
- Overclaim Risk +2
- Technical Readiness +0
- Rapport Momentum -1

Applied modifiers:

- Technical +0
- Commercial +1
- Rapport +0
- Energy +0
- intro tone: detail pressure
- architect pressure: sharper
- reaction warmth: +0
- email context: panel noted gaps between early positioning and scenario depth

Expected direction:

- stronger tone pressure without immediate score punishment;
- weak technical answers may draw sharper Architect reactions.

Assessment:

- matches design intent: overclaiming creates scrutiny rather than an instant penalty.

### Low-Energy / Low-Trust CandidateState

Representative source:

- Role Fit -1
- Recruiter Trust -2
- Candidate Confidence -2
- Energy -2
- Overclaim Risk +0
- Technical Readiness -1
- Rapport Momentum -1

Applied modifiers:

- Technical +0
- Commercial +0
- Rapport -1
- Energy -2
- intro tone: limited signal
- architect pressure: neutral
- reaction warmth: -1
- email context: panel felt the later stages lost some momentum

Expected direction:

- colder room, lower Energy start, and harder path to Pass;
- still recoverable through strong answers.

Assessment:

- meaningfully negative but not a forced Reject.

### Mixed CandidateState

Representative source:

- Role Fit +1
- Recruiter Trust +1
- Candidate Confidence -1
- Energy +1
- Overclaim Risk -1
- Technical Readiness +1
- Rapport Momentum +1

Applied modifiers:

- Technical +0
- Commercial +0
- Rapport +0
- Energy +0
- intro tone: neutral
- architect pressure: neutral
- reaction warmth: +0
- email context: none

Expected direction:

- mostly neutral Room entry.

Assessment:

- acceptable; mild Desk choices should not distort Room balance.

## Balance Safety Notes

- Starting modifiers are clamped to `-2..+2`.
- Most individual state values map to `-1`, `0`, or `+1`.
- Overclaim Risk mostly affects tone and Architect pressure.
- The resolver only applies when an active CandidateState exists.
- Direct `InterviewRoom` launch remains neutral.

## Recommendation

Leave P30 as-is after playtest unless one of these happens:

- positive Desk paths feel like guaranteed Strong Pass;
- low-energy paths feel unrecoverable;
- Architect pressure reads too punitive;
- the email context line feels too explicit or repetitive.

If modifier-driven outcomes feel too swingy, handle it in a later audit milestone rather than changing P24 thresholds inside P30.
