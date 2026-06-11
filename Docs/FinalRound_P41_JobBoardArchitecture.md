# Final Round - Prototype P41: Job Board Architecture / Data Model

## Summary

P41 prepares the existing Desk/job-listing architecture for `Final Round - VS4: The Job Board`.

This is architecture plumbing only. It does not add the final Job Board UI, final Helios/Redgate content, new Room variants, new recruiter branches, Room scoring changes, question data changes, threshold changes, or Aftermath flow changes.

The existing Northbridge Desk -> application -> recruiter -> Room -> Desk loop remains the default/fallback path.

## Files Changed

- `Assets/Scripts/FinalRoundVs2DataModels.cs`
- `Assets/Scripts/CandidateState.cs`
- `Assets/Scripts/DeskPrototypeController.cs`
- `Docs/FinalRound_P41_JobBoardArchitecture.md`
- `FinalRound_GamePlan_Current.md`

## JobListingData Fields Added

`JobListingData` now has VS4-ready fields for a compact authored Job Board:

- `jobCardSummary`
- `difficultyProfile`
- `recruiterName`
- `recruiterTitle`
- `recruiterCompany`
- `defaultRoleFitDelta`
- `defaultRecruiterTrustDelta`
- `defaultCandidateConfidenceDelta`
- `defaultEnergyDelta`
- `defaultOverclaimRiskDelta`
- `defaultTechnicalReadinessDelta`
- `defaultRapportMomentumDelta`
- `roomContextLine`
- `outcomeContextLine`
- `processSummaryNote`

Existing listing fields remain in place:

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

The added default deltas use the same clamped prototype modifier range as existing application and recruiter choices.

## SelectedJobId Flow

`DeskPrototypeController` can now hold:

- `defaultJobListing`
- `availableJobListings`
- a runtime `selectedJobListing`

The active run still uses `CandidateState.SelectedJobId` as the source of truth.

When a Desk run begins:

1. A neutral `CandidateState` is created.
2. The Desk selects the fallback job.
3. `CandidateState.SelectedJobId` is set from the selected job.
4. The selected job's default deltas are applied once.

P41 also adds:

- `SelectJobById(string jobId)`
- `ViewSelectedListing()`
- selected-job lookup helpers
- available-job fallback helpers
- debug display showing available job count and active job ID/company

The final three-card Job Board UI is intentionally deferred to P43.

## Default Job Modifiers

Job default modifiers are applied once per active run.

`CandidateState` now records:

- `JobDefaultDeltasAppliedJobId`

This prevents the same job defaults from being re-applied if the player reopens the listing, backs out of the application view, or returns to the listing during the same run.

The default modifiers are intended to set context, not decide the outcome. They should remain small and should not dominate application choices, recruiter choices, or Room performance.

## Fallback Behaviour

Fallback order:

1. Use the current active `selectedJobId` if it maps to an assigned job.
2. Use `defaultJobListing`.
3. Use the first non-null entry in `availableJobListings`.
4. Use the existing Northbridge placeholder constants.

If no Job Board list is assigned, the existing Northbridge single-job path still works.

The Desk listing section readers now resolve through the selected active job instead of reading only `defaultJobListing`.

## Downstream Hooks Prepared

P41 prepares hooks for later selected-job carry-through:

- recruiter name/title/company display falls back to Maya/Northbridge;
- outcome inbox can include `outcomeContextLine`;
- process summary can include selected job company, role, difficulty profile, and `processSummaryNote`;
- debug output shows active job context;
- `roomContextLine` exists on `JobListingData` for P44 Room intro/context work.

Full copy integration is intentionally deferred to P44.

## Placeholder Job Board State

`DeskPrototypeState` now includes:

- `JobBoard`

P41 does not build the final Job Board UI. The state and selection methods are present so P43 can add the three-card interface without changing the run-state shape again.

## How To Test

1. Start from the main menu.
2. Choose `Start Job Search`.
3. Open the Desk laptop.
4. Confirm the existing Northbridge listing still appears.
5. Choose an application strategy.
6. Confirm the application.
7. Complete the recruiter screen.
8. Continue to The Room.
9. Finish the interview and return to Desk.
10. Confirm inbox and process summary still work.
11. Force or reach Reject and confirm `Clear the Room` still appears only for Reject.
12. Complete Aftermath and confirm post-aftermath choices still work.
13. Press `F1` at the Desk and confirm debug shows active job and available job count.

## Intentionally Deferred

- final three-card Job Board UI;
- final Helios/Redgate authored listings;
- job card visual layout;
- job-specific application feedback;
- full job-specific recruiter copy;
- job-specific Room intro copy;
- job-specific outcome email pass;
- new Room variants;
- new recruiter mechanics;
- Room scoring or threshold changes;
- question data changes;
- Aftermath changes.

## Recommended P42 Prompt

```text
Next milestone:
Final Round - Prototype P42: Three Authored Job Listings

Read:
- Docs/FinalRound_P41_JobBoardArchitecture.md
- Docs/FinalRound_VS4_JobBoard_Planning.md
- Docs/FinalRound_VS2_TheDesk.md
- FinalRound_GamePlan_Current.md

Goal:
Author the three VS4 job listings as data while keeping the existing Desk flow and Room scoring unchanged.

Tasks:
1. Keep Northbridge as the balanced baseline.
2. Add Helios Cloud Platform placeholder/authored listing data.
3. Add Redgate Financial Risk placeholder/authored listing data.
4. Give each job:
   - job card summary
   - difficulty profile
   - recruiter identity
   - green flags
   - red flags
   - process notes
   - small default CandidateState deltas
   - room/outcome/process context lines
5. Do not build final Job Board UI yet.
6. Do not change Room scoring, question data, thresholds, Aftermath flow, or recruiter mechanics.
7. Create Docs/FinalRound_P42_ThreeAuthoredJobListings.md.
8. Run:
   dotnet build "Assembly-CSharp.csproj"
   dotnet build "Assembly-CSharp-Editor.csproj"

Deliver:
- files changed
- job listings authored
- default modifier summary
- fallback behaviour
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
