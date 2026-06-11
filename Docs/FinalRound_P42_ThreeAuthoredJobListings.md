# Final Round - Prototype P42: Three Authored Job Listings

## Summary

P42 adds the three authored VS4 job listings as `JobListingData` assets and adds an idempotent Unity Editor repair menu:

```text
Final Round > Create/Repair VS4 Job Listings
```

This is content/data work only. It does not add the final Job Board UI, change Room scoring, change question data, change outcome thresholds, change Aftermath flow, add new interview room variants, or create separate recruiter mechanics.

## Assets Added

Job listing assets:

- `Assets/Resources/FinalRound/VS4/JobListings/northbridge-security-presales.asset`
- `Assets/Resources/FinalRound/VS4/JobListings/helios-cloud-security-consultant.asset`
- `Assets/Resources/FinalRound/VS4/JobListings/redgate-risk-compliance-presales.asset`

Editor tool:

- `Assets/Editor/FinalRoundVs4JobListingCreator.cs`

The editor menu can create or repair all three assets and assign them to `DeskPrototypeController.availableJobListings` in `DeskScene` if the scene exists.

## Job Summaries

### Northbridge Cyber Systems

- Job ID: `northbridge-security-presales`
- Role: `Senior Solutions Engineer - Security Presales`
- Difficulty profile: `Balanced baseline`
- Recruiter: Maya Patel, Senior Talent Partner

Northbridge remains closest to the existing VS2 role. It is balanced across customer discovery, technical judgement, commercial pressure, and communication discipline.

Risk/reward:

- familiar baseline;
- cleanest match for existing Room content;
- still carries vague workload, compensation, and ownership boundaries.

Default CandidateState deltas:

```text
Role Fit +0
Recruiter Trust +0
Candidate Confidence +0
Energy +0
Overclaim Risk +0
Technical Readiness +0
Rapport Momentum +0
```

### Helios Cloud Platform

- Job ID: `helios-cloud-security-consultant`
- Role: `Cloud Security Solutions Consultant`
- Difficulty profile: `Technical stretch / architecture-heavy`
- Recruiter: Iris Chen, Cloud Security Talent Partner

Helios is the stretch role. It has stronger upside and clearer compensation/process appeal, but the expectations invite sharper technical scrutiny and make overclaiming more dangerous.

Risk/reward:

- more attractive and ambitious;
- more architecture-heavy;
- higher proof pressure if the candidate implies too much depth.

Default CandidateState deltas:

```text
Role Fit +0
Recruiter Trust +0
Candidate Confidence +1
Energy -1
Overclaim Risk +1
Technical Readiness -1
Rapport Momentum +0
```

### Redgate Financial Risk

- Job ID: `redgate-risk-compliance-presales`
- Role: `Risk & Compliance Presales Consultant`
- Difficulty profile: `Commercial / compliance bureaucracy`
- Recruiter: Eleanor Shaw, Risk Solutions Recruiting Lead

Redgate is the compliance and stakeholder-pressure role. It is less deep-technical than Helios, but it carries more process, legal, procurement, and careful-claims pressure.

Risk/reward:

- better fit for careful commercial communication;
- slower and more bureaucratic;
- lower cloud architecture demand but higher stakeholder/process ambiguity.

Default CandidateState deltas:

```text
Role Fit +1
Recruiter Trust +0
Candidate Confidence +0
Energy -1
Overclaim Risk +0
Technical Readiness -1
Rapport Momentum +1
```

## Recruiter Handling

P42 uses the minimal VS4 approach:

- each listing has its own recruiter identity fields;
- the existing recruiter-screen system is still shared;
- no new recruiter branch, new recruiter mechanics, or role-specific recruiter question set is added.

Full selected-job recruiter copy is deferred to P44.

## Create / Repair Menu

Run this in Unity:

```text
Final Round > Create/Repair VS4 Job Listings
```

The menu:

- creates or repairs the three `JobListingData` assets;
- keeps job IDs stable and unique;
- fills all required VS4 fields;
- validates that the three listings exist;
- assigns Northbridge as `defaultJobListing`;
- assigns all three jobs to `availableJobListings`;
- preserves existing fallback behaviour.

DeskScene assignment requires running the menu inside Unity. The command uses `SerializedObject` and `EditorSceneManager`; no manual scene YAML editing is required.

## Validation Notes

Expected validation:

- all three assets exist;
- job IDs are unique;
- required display fields are populated;
- default deltas are small and within the prototype clamp range;
- Northbridge remains the default/fallback listing;
- existing Desk -> application -> recruiter -> Room -> Desk flow remains unchanged when Northbridge is selected.

## Intentionally Deferred

- final three-card Job Board UI;
- clickable job cards;
- selected-job application feedback;
- deeper selected-job recruiter copy;
- selected-job Room intro/copy pass;
- selected-job outcome inbox copy polish;
- new Room variants;
- new questions;
- scoring or threshold changes;
- Aftermath changes.

## Recommended P43 Prompt

```text
Next milestone:
Final Round - Prototype P43: Job Selection UI

Read:
- Docs/FinalRound_P42_ThreeAuthoredJobListings.md
- Docs/FinalRound_P41_JobBoardArchitecture.md
- Docs/FinalRound_VS4_JobBoard_Planning.md
- FinalRound_GamePlan_Current.md

Goal:
Add a compact three-card Job Board view at the Desk using the authored VS4 job listings.

Tasks:
1. Add a low-risk Job Board state/view to DeskPrototypeController.
2. Show Northbridge, Helios, and Redgate as three compact job cards.
3. Let the player select one role and continue to the existing listing detail view.
4. Keep existing Northbridge fallback working.
5. Do not change application/recruiter mechanics beyond selected-job selection.
6. Do not change Room scoring, question data, thresholds, Aftermath flow, or add new room variants.
7. Create Docs/FinalRound_P43_JobSelectionUI.md.
8. Run:
   dotnet build "Assembly-CSharp.csproj"
   dotnet build "Assembly-CSharp-Editor.csproj"

Deliver:
- files changed
- UI behaviour
- selectedJobId flow
- test steps
- build result
- final git status

Do not commit automatically.
```

## Validation

Required:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

Validation result:

- `dotnet build "Assembly-CSharp.csproj"`: passed, 0 warnings, 0 errors.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: passed, 0 warnings, 0 errors.
