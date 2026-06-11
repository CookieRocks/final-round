# Final Round - VS2 Playtest README

## What This Is

Final Round is a prototype job-search pressure game.

This VS2 playtest build demonstrates the connected Desk-to-Room loop:

```text
Main Menu
  -> Start Job Search
  -> The Desk
  -> Northbridge Jobs listing
  -> Application Strategy
  -> Maya Patel recruiter screen
  -> The Room
  -> Outcome email
  -> Return to Desk
  -> Northbridge Mail / Inbox
  -> Process Summary / Start New Run
```

This is a prototype, not a finished game. Some art, UI, and text presentation are intentionally rough.

## How To Run

1. Unzip the playtest package.
2. Open the extracted folder.
3. Run `FinalRound.exe`.
4. Start with `Start Job Search` for the intended VS2 flow.

Use `Debug: Start Room Directly` only if you want to test the Room as a standalone interview slice.

The checked-in repository package is:

```text
FinalRound_VS2_Playtest.zip
```

## Controls

### Desk

- `E`: open the laptop.
- `Space`: open the laptop.
- Mouse click on laptop: open the laptop.
- Mouse: choose listing sections, application strategies, recruiter replies, and buttons.
- `F1`: toggle the Desk debug readout.

### Room

- `WASD`: move before sitting.
- Mouse: look before sitting and select UI choices.
- `E`: sit when the chair prompt appears.
- `Esc`: pause/resume.
- `F1`: toggle Room debug tools.

## What To Try

Please try at least one full run:

1. Start Job Search.
2. Read the listing.
3. Pick and confirm an application strategy.
4. Complete the recruiter screen.
5. Continue to the interview room.
6. Sit down and complete the six-question interview.
7. Read the outcome email.
8. Return to Desk.
9. Review the inbox and process summary.
10. Start a new run if you want to test a different application/recruiter path.

## Feedback To Collect

Useful feedback:

- Did you understand what to do next at each step?
- Did the Desk choices feel meaningful before the interview?
- Did the recruiter screen feel plausible?
- Did The Room feel tense but fair?
- Were any screens hard to read?
- Did any button or transition fail?
- Did the final outcome feel connected to your earlier choices?
- Did anything feel confusing, broken, too slow, or too abrupt?

## Known Limitations

- The Desk scene is prototype art.
- The laptop UI is functional rather than final.
- There is one company, one role, and one recruiter.
- There is no separate menu scene yet; the menu is hosted by the Room scene.
- Debug tools remain available through `F1`.
- Characters in The Room are placeholders and are not fully animated or truly seated.
- There is no voice, lip sync, or facial animation.
- The scorecard and outcome email are prototype UI.
- Some internal scoring saturation remains technical debt.

Thanks for playing and testing the hiring loop.
