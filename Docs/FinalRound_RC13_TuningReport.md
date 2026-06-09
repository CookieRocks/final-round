# Final Round Prototype v1.0 RC13 Tuning Report

Generated after tuning the existing 12-question ScriptableObject bank.

No new questions, systems, room changes, UI layout changes, or data format changes were made.

## Before Outcome Distribution

From `Docs/FinalRound_RC12_BalanceAudit.md`:

| Outcome | Count | Share |
| --- | ---: | ---: |
| Strong Pass | 1,877 | 45.8% |
| Pass | 1,300 | 31.7% |
| Hold | 758 | 18.5% |
| Reject | 161 | 3.9% |

## After Outcome Distribution

After RC13 score and threshold tuning:

| Outcome | Count | Share | Target |
| --- | ---: | ---: | --- |
| Strong Pass | 502 | 12.3% | 10-20% |
| Pass | 1,755 | 42.8% | 35-45% |
| Hold | 1,301 | 31.8% | 25-35% |
| Reject | 538 | 13.1% | 8-15% |

All four outcomes remain reachable.

## Score Averages

| Dimension | Before Avg | After Avg | After Min | After Max |
| --- | ---: | ---: | ---: | ---: |
| Technical | 7.35 | 6.56 | 1 | 10 |
| Commercial | 7.91 | 6.99 | 1 | 10 |
| Rapport | 6.20 | 5.20 | 0 | 10 |
| Energy | 5.50 | 5.88 | 2 | 9 |

Energy now has a clearer role:

| Dimension | Before non-zero answer deltas | After non-zero answer deltas |
| --- | ---: | ---: |
| Technical | 39 | 35 |
| Commercial | 38 | 38 |
| Rapport | 39 | 40 |
| Energy | 18 | 43 |

## Outcome Threshold Changes

Strong Pass was made more demanding:

- Before: Technical `>= 7`, Commercial `>= 6`, Rapport `>= 5`, Total `>= 27`
- After: Technical `>= 8`, Commercial `>= 7`, Rapport `>= 6`, Total `>= 29`

Pass now requires enough presence to keep the room engaged:

- Before: Total `>= 22`, Technical `>= 5`, Commercial `>= 5`
- After: Total `>= 22`, Technical `>= 5`, Commercial `>= 5`, Energy `>= 5`

Hold moved up slightly:

- Before: Total `>= 17`
- After: Total `>= 19`

## Changes Made Per Question

- `CTX-01`: Reduced the strongest board-risk discovery answers slightly and added Energy to confident framing.
- `CTX-02`: Added real downside to vendor-fatigue responses; safe-but-generic answers now lose Energy or Rapport.
- `CTX-03`: Reduced over-generous executive summary scoring and added Energy differences for concise vs passive handling.
- `CTX-04`: Reduced Rapport/Commercial on the safest trust-building answers and added Energy to active boundary-setting.
- `TECH-01`: Removed the dominant validation-path answer by adding commercial/energy cost; tuning-model answer now has stronger presence but lower rapport.
- `TECH-02`: Reduced Commercial on privacy answers and penalized evasive executive framing with Energy loss.
- `TECH-03`: Removed the dominant SIEM-fit answer by lowering Commercial/Rapport and adding Energy cost for cautious framing.
- `TECH-04`: Added meaningful downside around over-explaining, UI-first proof, and architecture-heavy proof.
- `COMM-01`: Removed answer 4 dominance by reducing Technical/Rapport and adding Energy cost; early discounting is now more visibly weak.
- `COMM-02`: Added trade-offs between executive pressure, trust, and close-plan ownership.
- `COMM-03`: Reduced overly generous CFO timing answers and added Energy cost to generic urgency framing.
- `COMM-04`: Reduced over-generous legal/data-residency answers and added Energy differences for ownership vs passivity.

## Remaining Warnings

Dominant-answer warnings:

- None detected in the final simulation.

Limited-trade-off warnings:

- None detected in the final simulation.

## Target Assessment

The target distribution was achieved:

- Strong Pass is now earned rather than common.
- Pass remains achievable with balanced, credible answers.
- Hold is common for mixed performances.
- Reject is meaningfully possible when answers are repeatedly weak, vague, risky, or passive.
- Energy now participates in most answer choices and in the Pass threshold.

## Build Result

`dotnet build "Assembly-CSharp.csproj"` passed with `0 Warning(s)` and `0 Error(s)`.
