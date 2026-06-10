# Final Round Versioning

Final Round uses milestone labels to describe the maturity and intent of a build. Older `RC` labels from the room prototype period are legacy checkpoint names and should not be rewritten in git history.

## Labels

`EXP` means experiment or spike. Use it for throwaway research, risky technical probes, or short-lived feature tests that may not ship.

`P` means prototype milestone. Use it for iterative prototype checkpoints such as `Final Round - Room Prototype P21` and `Final Round - Room Prototype P22`.

`VS` means vertical slice. Use it when a contained slice represents the intended end-to-end experience, for example `Final Round - Vertical Slice VS1: The Room`.

`Alpha` means a broader playable version where major systems are present but content, balance, and polish are still expected to change.

`Beta` means feature/content lock testing. Use it when the focus is validation, bug fixing, compatibility, balancing, and usability rather than new feature construction.

`RC` means real release candidate only. Use it only when the project is believed to be release-ready except for final verification and blocking fixes.

## Current convention

The current vertical slice milestone is:

- `Final Round - Vertical Slice VS1: The Room`

Prototype milestone labels remain valid for future prototype-only checkpoints:

- `Final Round - Room Prototype P21`
- `Final Round - Room Prototype P22`

## Legacy naming

Existing commits, tags, archived docs, and reports that use `RC` should remain as historical records unless there is a specific low-risk reason to update a currently active document.
