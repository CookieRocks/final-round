# Final Round - Prototype P32: VS2 Readiness Review

## Summary

P32 reviews whether the connected Desk pipeline is ready to be promoted to:

`Final Round - Vertical Slice VS2: The Desk`

Recommendation: ready to promote to VS2 after one final manual Unity smoke test pass.

No gameplay systems, question data, Room thresholds, answer deltas, Room art, or Room stage structure were changed for P32.

## Sources Read

- `FinalRound_GamePlan_Current.md`
- `Docs/FinalRound_VS2_TheDesk_Planning.md`
- `Docs/FinalRound_P27_DeskScenePrototype.md`
- `Docs/FinalRound_P28_JobListingAndApplicationChoices.md`
- `Docs/FinalRound_P29_RecruiterScreen.md`
- `Docs/FinalRound_P30_RunStateModifiersIntoRoom.md`
- `Docs/FinalRound_P30_ModifierSanityReport.md`
- `Docs/FinalRound_P31_ReturnToDeskInbox.md`
- `Docs/FinalRound_P25_VS1ReadinessReview.md`
- `Assets/Scripts/FinalRoundRunState.cs`
- `Assets/Scripts/CandidateState.cs`
- `Assets/Scripts/DeskPrototypeController.cs`
- `Assets/Scripts/CybersecurityPresalesInterviewFlow.cs`
- `Assets/Scripts/RoomModifierResolver.cs`
- `Assets/Scripts/InterviewGameManager.cs`

## Current Complete VS2 Flow

The current connected flow is:

1. Main menu.
2. `Start Job Search`.
3. `DeskScene`.
4. Generated desk and laptop shell.
5. Laptop opens through keyboard or click.
6. Northbridge Cyber Systems job listing.
7. Application strategy selection.
8. CandidateState update.
9. Maya Patel recruiter screen.
10. Recruiter response CandidateState updates.
11. Continue to `InterviewRoom`.
12. The Room detects active CandidateState.
13. P30 Room modifiers are resolved and applied.
14. Six-question VS1 interview runs.
15. Outcome email appears.
16. `Return to Desk`.
17. Desk opens Northbridge Mail / Inbox.
18. Process Summary can review the run.
19. Start New Run clears state and resets the Desk.

## Checklist Result

| Check | Result | Notes |
| --- | --- | --- |
| Main menu appears | Code verified | Existing `InterviewGameManager` main menu remains active. |
| Start Job Search loads DeskScene | Code verified | `StartJobSearch()` loads `DeskScene`. |
| DeskScene generated shell appears | Code verified, manually observed previously | `DeskPrototypeController` generates camera, desk, laptop, wall/floor, light, UI, and EventSystem. |
| Laptop opens with E | Code verified, manually observed previously | Input System path checks `Keyboard.current.eKey`. |
| Laptop opens with Space | Code verified, manually observed previously | Input System path checks `Keyboard.current.spaceKey`. |
| Laptop opens with mouse click | Code verified, manually observed previously | Raycast checks the laptop base transform. |
| Job listing is readable | Recently manually observed | P28 listing sections remain available. |
| Application strategy can be selected | Code verified, manually observed previously | Strategy buttons select a choice and enable confirm. |
| CandidateState changes after strategy confirm | Code verified | `ConfirmApplication()` applies clamped deltas and sets `applicationChoiceId`. |
| Recruiter screen appears after application | Code verified, manually observed previously | Recruiter button requires application confirmation. |
| Recruiter responses work | Code verified, manually observed previously | Three prompts update response IDs and deltas. |
| CandidateState changes after recruiter responses | Code verified | `SelectRecruiterResponse()` applies clamped deltas and records `recruiterResponseIds`. |
| Continue to Interview loads InterviewRoom | Code verified, manually observed previously | `GoToInterviewRoom()` loads configured scene. |
| Room detects active CandidateState | Code verified | `TryGetActiveState()` gates active Desk runs. |
| Room applies P30 modifiers | Code verified | `ResolveAndApplyRoomModifiers()` applies small starting modifiers. |
| Room debug shows CandidateState and modifiers | Code verified | F1 debug includes CandidateState and Room modifier summary. |
| Stage intro/tone variants appear where expected | Code verified | Stage 1 intro and Stage 2 Architect pressure variants are state-driven. |
| Six-question Room interview works | Existing VS1/P25 verified | No P32 changes to question selection or stage structure. |
| Outcome email appears | Existing VS1/P31 verified | P31 only added return-to-Desk path and spacing polish. |
| Return to Desk appears only for active Desk run | Code verified | Button is hidden unless active run has `roomOutcome`. |
| Return to Desk loads DeskScene | Code verified, manually observed previously | `ReturnToDesk()` loads `DeskScene`. |
| Desk opens to Northbridge Mail / Inbox | Code verified, recently manually observed | `ShowCompletedRunInboxIfAvailable()` runs on Desk start. |
| Inbox copy matches Room outcome | Code verified | Copy branches on `StrongPass`, `Pass`, `Hold`, `Reject`. |
| Process Summary includes run details | Code verified, recently manually observed | Shows job, application, recruiter path, responses, outcome, state values, modifier summary. |
| Start New Run clears CandidateState | Code verified | Uses `FinalRoundRunState.Instance.ResetRun()`. |
| Main Menu path works | Code verified | Clears run state and loads `InterviewRoom`, which hosts the main menu. |
| Direct InterviewRoom launch works as neutral VS1 | Code verified | No singleton means `TryGetActiveState()` fails and modifiers are skipped. |
| Direct DeskScene launch works | Code verified | Opening laptop creates a neutral active Desk run. |
| Restart from Room works | Existing VS1 code path | It restarts the Room run; active Desk state is intentionally preserved for same-opportunity replay. |

## Run-State Safety Notes

- `FinalRoundRunState` is a single `DontDestroyOnLoad` singleton.
- Duplicate `FinalRoundRunState` objects self-destroy in `Awake()`.
- `CreateNeutralRun()` creates a new active Desk run with `HasActiveDeskRun = true`.
- `ResetRun()` clears the state back to neutral with `HasActiveDeskRun = false`.
- `TryGetActiveState()` only returns true for active Desk runs.
- Desk-to-Room and Room-to-Desk transitions preserve CandidateState through the singleton.
- `roomOutcome` is written only when the Room outcome email is shown.
- Desk inbox appears only when an active state exists and `roomOutcome` is populated.
- `Return to Desk` is hidden when there is no completed active Desk run.
- `Main Menu` from the Desk intentionally clears the active run before loading `InterviewRoom`.

One behaviour to keep intentionally documented: Room `Restart` restarts the interview while preserving the active Desk state. This is useful for replaying the same opportunity, but a future production flow may want separate `Retry Interview` and `Abandon Run` wording.

## Modifier Safety Notes

P30 modifier safety still holds:

- starting modifiers are clamped to `-2..+2`;
- most ordinary paths resolve to `-1`, `0`, or `+1`;
- positive Desk paths improve opening conditions but do not guarantee StrongPass;
- low-energy / low-trust paths are harder but recoverable through strong answers;
- high-overclaim paths create tone pressure and sharper Architect scrutiny rather than immediate punishment;
- direct Room launch remains neutral because no active CandidateState exists.

No P24/P25 thresholds were changed.

## UI / UX Triage

| Issue | Classification | Notes |
| --- | --- | --- |
| Desk laptop UI is functional but runtime-generated | Polish later | Good enough for VS2; visual art pass can wait. |
| Job listing is readable but text-heavy | Polish later | Works for one authored role; future slicing may add richer layout. |
| Application strategy clarity | Non-blocking | Choices are understandable and show consequences in debug/feedback. |
| Recruiter screen clarity | Non-blocking | The three-prompt screen reads as a prototype message exchange. |
| Outcome inbox readability | Non-blocking | P31/P32 spacing pass made it readable. |
| Process Summary usefulness | Non-blocking | Shows the right state trail; still slightly debug-flavoured by design. |
| Room outcome email spacing | Non-blocking | Tuned during P31 follow-up; should remain in smoke-test checklist. |
| Main Menu returns through InterviewRoom | Technical debt | Acceptable for VS2 because no separate menu scene exists. |
| Debug panels expose internal state | Intentionally deferred | Useful while run-state behaviour is still being tested. |

## Hygiene Review

Repository status during review:

- `Assets/Scenes/DeskScene.unity` has known null-slot repair churn.
- `Assets/Settings/UniversalRenderPipelineGlobalSettings.asset` is dirty.
- `ProjectSettings/GraphicsSettings.asset` is dirty.
- No untracked files were found.

Build Settings include:

- `Assets/Scenes/InterviewRoom.unity`
- `Assets/Scenes/DeskScene.unity`

Text scan notes:

- No missing script references were found in scenes or prefabs.
- Existing `m_Script: {fileID: 0}` entries were found in `Assets/Settings/DefaultVolumeProfile.asset`; these appear to be pre-existing settings/profile entries, not a P32 blocker.
- `ProjectSettings/ProjectSettings.asset` still references `Assets/Scenes/SampleScene.unity` as the template default scene. This is not the active build scene path.
- No untracked imported asset debris was present.

## VS2 Blockers

No code blocker was found in static review.

Manual smoke-test blockers before promotion:

- Run the full Main Menu -> Desk -> Room -> Desk loop once in Unity.
- Confirm `Return to Desk` is hidden in a direct Room run.
- Confirm `Start New Run` clears the Desk inbox state.
- Confirm Room email and Desk process summary remain readable at target resolution.

## Non-Blocking Limitations

- The Desk scene is still a generated prototype shell, not final art.
- The laptop UI is not a full email/job-board client.
- There is one company, one role, and one recruiter.
- There is no prep/action choice yet despite being part of the original wider VS2 sketch.
- Room modifiers are small and intentionally indirect.
- Main Menu is still hosted by `InterviewRoom`.
- Unity player build/distribution still depends on local Unity licensing and build setup.

## Technical Debt

- Separate main-menu scene may eventually be cleaner than using `InterviewRoom` as the menu host.
- `DeskPrototypeController` owns a lot of UI generation and state logic; future VS2 expansion may justify splitting content rendering from flow state.
- Question assets still use legacy `RC11` resource path.
- Score saturation remains a known Room scoring-model concern from P24/P25.
- Existing URP/Graphics settings drift should be reconciled separately, not inside P32.

## Recommendation

Ready to promote to:

`Final Round - Vertical Slice VS2: The Desk`

Condition: one final manual Unity smoke test pass confirms the loop and button visibility at runtime.

## Recommended Next Milestone

`Final Round - Vertical Slice VS2: The Desk`

Suggested promotion scope:

- update visible build label to `Final Round VS2: The Desk`;
- create `Docs/FinalRound_VS2_TheDesk.md`;
- tag the milestone;
- avoid new gameplay, content, scoring changes, or UI redesign.

If the final smoke test finds a runtime issue, handle it as a narrow P33 hardening pass before VS2 promotion.

## Validation

Run:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```
