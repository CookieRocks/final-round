# Final Round Prototype v1.0 RC12 Balance Audit

Generated from `Assets/Resources/FinalRound/Questions/RC11`.

This is an inspection report only. No question wording, score deltas, outcome thresholds, or gameplay behavior were changed.

## Validation Summary

- Source assets found: 12
- Valid questions: 12
- Context/customer scenario: 4
- Technical/security judgement: 4
- Commercial/executive pressure: 4
- Validation warnings: 0

All loaded `InterviewQuestionData` assets passed structural validation:

- Non-empty question ID
- Valid category
- Speaker name/title present
- Question text present
- Exactly 4 answer options
- Non-empty answer text
- Score deltas within expected range `-3` to `3`

## Simulation Scope

- Question combinations: 64
- Answer paths per question combination: 64
- Total simulated answer paths: 4,096
- Starting scores used:
  - Technical: 4
  - Commercial: 4
  - Rapport: 4
  - Energy: 6
- Score clamp used: 0 to 10
- Outcome thresholds matched the current interview flow.

## Outcome Distribution

| Outcome | Count | Share |
| --- | ---: | ---: |
| Strong Pass | 1,877 | 45.8% |
| Pass | 1,300 | 31.7% |
| Hold | 758 | 18.5% |
| Reject | 161 | 3.9% |

Notes:

- All outcomes are reachable.
- Strong Pass is possible.
- Reject is possible.
- Strong Pass appears very often for a final-round pressure prototype.
- Reject appears rarely.

## Score Distribution Summary

| Dimension | Average | Min | Max |
| --- | ---: | ---: | ---: |
| Technical | 7.35 | 2 | 10 |
| Commercial | 7.91 | 1 | 10 |
| Rapport | 6.20 | 0 | 10 |
| Energy | 5.50 | 3 | 9 |

Overall strongest dimension:

- Commercial, average final score `7.91`.

Overall weakest dimension:

- Energy, average final score `5.50`.

## Category Balance

| Category | Avg Technical Delta | Avg Commercial Delta | Avg Rapport Delta | Avg Energy Delta |
| --- | ---: | ---: | ---: | ---: |
| Context/customer scenario | 0.94 | 1.00 | 1.13 | 0.00 |
| Technical/security judgement | 1.56 | 1.19 | 0.44 | -0.25 |
| Commercial/executive pressure | 0.88 | 1.88 | 0.69 | -0.25 |

Non-zero delta counts by dimension:

| Dimension | Non-zero answer deltas |
| --- | ---: |
| Technical | 39 |
| Commercial | 38 |
| Rapport | 39 |
| Energy | 18 |

Notes:

- Category identities are readable: technical questions favor Technical, commercial questions favor Commercial, and context questions lean Rapport.
- Energy is affected far less often than the other dimensions.
- Technical and Commercial trend high, which helps explain the large Strong Pass share.

## Question-Level Notes

| Question ID | Category | Speaker | Notes |
| --- | --- | --- | --- |
| CTX-01 | Context/customer scenario | Hiring Manager | Clear trade-offs. No dominant answer warning. |
| CTX-02 | Context/customer scenario | Hiring Manager | No meaningful trade-off warning: every answer has a non-negative total delta and only one answer contains any downside. |
| CTX-03 | Context/customer scenario | Hiring Manager | Clear trade-offs. No dominant answer warning. |
| CTX-04 | Context/customer scenario | Hiring Manager | Clear trade-offs. No dominant answer warning. |
| TECH-01 | Technical/security judgement | Principal Security Architect | Dominant answer warning: answer 3 is never lower than any other answer across all score dimensions. |
| TECH-02 | Technical/security judgement | Principal Security Architect | Clear trade-offs. No dominant answer warning. |
| TECH-03 | Technical/security judgement | Principal Security Architect | Dominant answer warning: answer 1 is never lower than any other answer across all score dimensions. |
| TECH-04 | Technical/security judgement | Principal Security Architect | No meaningful trade-off warning: every answer has a non-negative total delta and only one answer contains any downside. |
| COMM-01 | Commercial/executive pressure | Sales Director | Dominant answer warning: answer 4 is never lower than any other answer across all score dimensions. |
| COMM-02 | Commercial/executive pressure | Sales Director | Clear trade-offs. No dominant answer warning. |
| COMM-03 | Commercial/executive pressure | Sales Director | Clear trade-offs. No dominant answer warning. |
| COMM-04 | Commercial/executive pressure | Sales Director | Clear trade-offs. No dominant answer warning. |

## Answer-Level Warnings

- `COMM-01` answer 4 dominates the other options across every score dimension.
- `TECH-01` answer 3 dominates the other options across every score dimension.
- `TECH-03` answer 1 dominates the other options across every score dimension.
- `CTX-02` has limited downside across the answer set.
- `TECH-04` has limited downside across the answer set.

## Outcome Reachability Checks

- Outcome never appears: none.
- Outcome appears too often: Strong Pass is high at `45.8%`.
- Strong Pass impossible: no.
- Reject impossible: no.
- Score dimension rarely affected: Energy is the clear outlier at `18` non-zero answer deltas, compared with `38-39` for the other dimensions.
- Category heavily favors one dimension:
  - Technical/security judgement heavily favors Technical.
  - Commercial/executive pressure heavily favors Commercial.
  - This is directionally appropriate, but the positive totals may be too generous.

## Recommended Tuning Areas

- Reduce the Strong Pass rate by adding more trade-offs to high-scoring answers, especially in Technical and Commercial questions.
- Increase the Reject/Hold pressure by making weak answers more costly or by lowering positive deltas on safe answers.
- Give Energy a clearer design role. It is currently touched much less often than Technical, Commercial, and Rapport.
- Review dominant answers before expanding the bank:
  - `COMM-01` answer 4
  - `TECH-01` answer 3
  - `TECH-03` answer 1
- Add more answers that are strong in one dimension but visibly weak in another.
- Consider whether the starting Energy value of `6` is doing useful work, since Energy rarely moves and never falls below `3` in the current full-bank simulation.

No automatic tuning changes were applied.
