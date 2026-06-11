# Final Round - P43 Job Selection UI

_Date: 2026-06-11_  
_Status: Implemented for prototype validation_

## Purpose

P43 adds the first playable VS4 Job Board interaction to the Desk laptop. The goal is to let the player choose one of three authored opportunities before entering the existing listing, application, recruiter, Room, outcome, and optional Aftermath flow.

This is still a small authored board, not a procedural job marketplace.

## Files Read

- `FinalRound_GamePlan_Current.md`
- `Docs/FinalRound_VS4_JobBoard_Planning.md`
- `Docs/FinalRound_P41_JobBoardArchitecture.md`
- `Docs/FinalRound_P42_ThreeAuthoredJobListings.md`
- `Docs/FinalRound_VS2_TheDesk.md`
- `Docs/FinalRound_P40_PostAftermathDeskChoices.md`

## What Changed

- Fresh Desk runs now open to a three-card Job Board when three valid `JobListingData` assets are available.
- Each card shows the company, role title, difficulty profile, recruiter, one green signal, and one risk signal.
- The board uses a neutral `Opportunity Board` header instead of a company-specific header before selection.
- Selecting a card sets the active job for the run and shows:
  - `Selected: [Company] - [Role]`
- The existing listing view now uses the selected job's data.
- `View Listing` is disabled until a job has been selected.
- `Choose Application Strategy` is hidden while on the Job Board and remains unavailable until a job is selected.
- A `Back to Job Board` button appears before application submission.
- After an application is submitted, job switching is blocked with:
  - `Application already submitted. Start a new run to choose another role.`
- The Desk debug readout now includes available job count, selected job ID, selected company, applied job-default ID, and application confirmation state.

## Job Selection Behavior

P43 uses a conservative run reset when the player changes jobs before submitting an application:

1. Create a neutral Desk run state.
2. Clear application and recruiter progress.
3. Select the newly chosen job.
4. Apply that job's default CandidateState deltas once.

This prevents multiple job default modifiers from stacking during pre-application browsing.

Once an application is confirmed, the selected job is locked for the run. The player can still start a new run through the existing Desk controls.

## CandidateState Handling

- `CandidateState.SelectedJobId` remains the source of truth for the active run.
- `CandidateState.JobDefaultDeltasAppliedJobId` records which job defaults were applied.
- Job defaults apply once per selected run state.
- Switching jobs before application submission resets to neutral and applies only the new job's defaults.
- Existing Room scoring, questions, thresholds, and Aftermath state are unchanged.

## Fallback Behavior

If the full Job Board is unavailable, invalid, duplicated, or has fewer than three valid listings, the Desk falls back to the existing Northbridge listing path.

Northbridge remains the default/fallback role.

The Desk scene may also recover from missing `availableJobListings` inspector references by loading the three P42 assets from:

- `Resources/FinalRound/VS4/JobListings`

When those resource assets are present, they are ordered as Northbridge, Helios, and Redgate for the three-card board.

## Test Steps

1. Start a fresh run from the main menu.
2. Enter the Desk laptop.
3. Confirm the Job Board appears with three role cards.
4. Confirm `View Listing` and `Choose Application Strategy` are unavailable until a role is selected.
5. Select Northbridge, Helios, and Redgate before submission and confirm the selected line changes each time.
6. Select a role, open `View Listing`, and confirm the listing sections match the selected job.
7. Use `Back to Job Board`, choose a different role, and confirm the run uses the new selection.
8. Choose an application strategy and confirm the application.
9. Try to return to the Job Board or switch jobs and confirm switching is blocked.
10. Continue through recruiter, Room, outcome inbox, and optional Aftermath to confirm existing flows still work.
11. Temporarily remove or invalidate Job Board listings in the inspector and confirm the Northbridge fallback still opens.

## Deferred

- Stronger selected-job flavor in recruiter copy.
- Stronger selected-job Room intro/context lines.
- Job-specific outcome inbox copy.
- Job-specific process summary copy.
- Final VS4 readiness review.
- Any new Room scoring, question, threshold, or room variant work.
