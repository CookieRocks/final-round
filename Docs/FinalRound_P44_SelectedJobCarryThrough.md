# Final Round - P44 Selected-Job Carry-Through

_Date: 2026-06-11_  
_Status: Implemented for prototype validation_

## Purpose

P44 makes the selected VS4 job visibly carry through the existing Desk, recruiter, Room, outcome inbox, and process summary flow.

This is a context and copy pass. It does not create a new interview system, retune the Room, add questions, add jobs, add recruiter branches, or change Aftermath.

## Files Read

- `FinalRound_GamePlan_Current.md`
- `Docs/FinalRound_VS4_JobBoard_Planning.md`
- `Docs/FinalRound_P41_JobBoardArchitecture.md`
- `Docs/FinalRound_P42_ThreeAuthoredJobListings.md`
- `Docs/FinalRound_P43_JobSelectionUI.md`
- `Docs/FinalRound_VS2_TheDesk.md`
- `Docs/FinalRound_P30_RunStateModifiersIntoRoom.md`
- `Docs/FinalRound_P40_PostAftermathDeskChoices.md`

## What Changed

- Application confirmation now adds selected-job-aware feedback.
- Recruiter screen identity uses the selected listing's `recruiterName`, `recruiterTitle`, and `recruiterCompany`.
- Recruiter intro copy references selected company, role, and profile pressure.
- Recruiter completion copy references the selected company panel and `roomContextLine`.
- Room modifier resolution now supplements existing CandidateState-derived context with selected-job `roomContextLine` and `outcomeContextLine`.
- Room stage intro can display the selected-job context line before the existing panel intro.
- Room outcome email sender uses selected-job recruiter/company when the run came from Desk.
- Desk outcome inbox subject includes the selected role.
- Desk inbox sender and context line use selected-job data.
- Process Summary now shows selected job ID, company, role, difficulty profile, recruiter identity, process note, application choice, recruiter path/responses, Room outcome, and Aftermath status.

## Selected-Job Copy Points

### Application

Application strategy feedback now adds a short selected-job line:

- Northbridge: balanced security presales fit for a mixed technical/commercial panel.
- Helios: cloud architecture credibility, with specific detail the panel can test.
- Redgate: regulated stakeholders and careful commercial judgement.

### Recruiter

The same recruiter screen structure is reused, but identity and intro copy are selected-job-aware:

- Northbridge: Maya Patel, Senior Talent Partner.
- Helios: Iris Chen, Cloud Security Talent Partner.
- Redgate: Eleanor Shaw, Risk Solutions Recruiting Lead.

Recruiter path IDs now reflect the selected recruiter name while preserving the shared screen mechanics.

### Room

`RoomModifierResolver` now loads the selected job from `Resources/FinalRound/VS4/JobListings` and adds:

- `roomContextLine` to the Room intro/debug summary;
- `outcomeContextLine` to the outcome email context, alongside existing CandidateState-derived context.

No numeric Room modifier mapping was changed.

### Outcome Inbox

Desk inbox now uses:

- selected recruiter name/company in the sender line;
- selected role in the subject line;
- selected-job `outcomeContextLine` where populated;
- existing CandidateState context as a fallback/additional signal.

### Process Summary

The summary now includes selected-job context that can be compared across Northbridge, Helios, and Redgate runs.

## Fallback Behaviour

If selected job data is missing:

- Desk falls back to Northbridge/default listing copy.
- Room direct launch remains neutral.
- Room selected-job context is omitted rather than crashing.
- Existing Northbridge single-listing path remains usable.

## Test Matrix

Run each path with Northbridge, Helios, and Redgate:

1. Start a fresh Desk run.
2. Select the job card.
3. Open the listing and confirm selected job sections appear.
4. Choose an application strategy.
5. Confirm application and verify the selected-job feedback line.
6. Open recruiter screen and verify recruiter name, title, subject, and intro copy.
7. Complete recruiter screen and verify the final invite references the selected company/panel context.
8. Enter the Room and verify the selected-job Room context appears in the opening intro.
9. Complete or skip the Room and verify the Room outcome sender reflects the selected recruiter/company.
10. Return to Desk and verify inbox sender, subject, role/company line, and context copy.
11. Open Process Summary and verify job ID, company, role, profile, recruiter, application, responses, outcome, and Aftermath status.
12. If Reject, enter Aftermath and verify Aftermath still works without wrong job copy.

## Intentionally Deferred

- Separate recruiter branches per company.
- Job-specific recruiter questions.
- Job-specific interview question banks.
- New Room variants.
- Room scoring changes.
- Question data changes.
- Outcome threshold changes.
- Aftermath flow changes.
- VS4 readiness review, deferred to P45.

## Validation

Required:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

