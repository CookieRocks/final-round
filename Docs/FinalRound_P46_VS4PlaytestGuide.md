# Final Round - VS4 Playtest Guide

_Milestone: Prototype P46 - VS4 Playtest Packaging and Feedback Pass_  
_Build target: Final Round VS4 - The Job Board_

## What This Is

Final Round is a prototype job-search pressure game about choosing opportunities, navigating ambiguous hiring steps, entering a final interview, and dealing with the outcome.

This VS4 playtest focuses on the connected Job Board loop:

```text
Main Menu
  -> Start Job Search
  -> The Desk
  -> Opportunity Board
  -> choose Northbridge, Helios, or Redgate
  -> Job Listing
  -> Application Strategy
  -> Recruiter Screen
  -> The Room
  -> Outcome Email
  -> Return to Desk
  -> Inbox / Process Summary
  -> optional Reject-only Aftermath
  -> Start New Run
```

This is still prototype quality. Some UI, room presentation, art, sound, and text layout are rough by design.

## How To Run

1. Unzip the playtest package.
2. Open the extracted folder.
3. Run `FinalRound.exe`.
4. Choose `Start Job Search` for the intended VS4 flow.

Use direct/debug entry points only if you are deliberately testing a specific scene.

Expected package name:

```text
FinalRound_VS4_Playtest.zip
```

## Controls

### Desk

- `E`: open the laptop.
- `Space`: open the laptop.
- Mouse click on laptop: open the laptop.
- Mouse: select job cards, listing sections, application strategies, recruiter replies, and buttons.
- `Esc`: Desk pause menu.
- `F1`: toggle Desk debug readout.

### Room

- `WASD`: move before sitting.
- Mouse: look before sitting and select UI choices.
- `E`: sit when the chair prompt appears.
- `Esc`: pause/resume.
- `F1`: toggle Room debug tools.

### Aftermath

After a Reject outcome:

- `Clear the Room`: enter the symbolic aftermath room from the Desk.
- `WASD`: move.
- Hold right mouse: look.
- Left click / `E` / `Space`: Feedback Hammer interaction.
- Return to Desk when finished.

## Available Jobs

### Northbridge Cyber Systems

- Role: Senior Solutions Engineer - Security Presales
- Profile: Balanced baseline
- Recruiter: Maya Patel

Northbridge is the familiar baseline. It tests a balanced mix of customer context, technical judgement, and commercial pressure.

### Helios Cloud Platform

- Role: Cloud Security Solutions Consultant
- Profile: Technical stretch / architecture-heavy
- Recruiter: Iris Chen

Helios is the stretch option. It offers stronger cloud architecture appeal but should create more technical scrutiny and overclaim risk.

### Redgate Financial Risk

- Role: Risk & Compliance Presales Consultant
- Profile: Commercial / compliance bureaucracy
- Recruiter: Eleanor Shaw

Redgate shifts pressure toward regulated stakeholders, careful claims, process drag, and commercial patience.

## What To Test

Please try at least one full run:

1. Start Job Search.
2. Open the laptop.
3. Review the three job cards.
4. Select a job.
5. View the listing.
6. Choose and confirm an application strategy.
7. Complete the recruiter screen.
8. Continue to The Room.
9. Sit down and complete the six-question interview.
10. Read the outcome email.
11. Return to Desk.
12. Review the inbox and process summary.
13. Start a new run and try a different job if you have time.

If you receive a Reject outcome, also try:

1. Return to Desk.
2. Choose `Clear the Room`.
3. Complete the Aftermath room.
4. Return to Desk and review the post-aftermath choices.

## Feedback Questions

### General

- Did you understand what the game wanted you to do?
- Did the Desk / laptop flow make sense?
- Did the Job Board feel like a choice, not just a menu?

### Job Board

- Could you tell the difference between Northbridge, Helios, and Redgate?
- Which job did you pick first, and why?
- Did the red/green signals help?
- Did one job feel obviously better or obviously pointless?
- Did the job cards have too much text, too little, or about right?

### Carry-Through

- Did the selected job feel remembered later?
- Did the recruiter identity/copy match the job you chose?
- Did the Room intro/context feel connected to the job?
- Did the inbox/process summary reflect your chosen job?

### Room

- Did The Room still feel tense?
- Was it clear what to do when entering the Room?
- Was mouse sensitivity okay?
- Did reaction text stay visible long enough?

### Aftermath

- If you saw a Reject, did Clear the Room make sense?
- Did the Aftermath feel cathartic, weird, or unnecessary?
- Did it feel safely symbolic rather than revenge-y?

### Replay

- Would you try another job?
- What would you expect another role to change?
- What did you want more of?

## Known Limitations

- Job Board UI is functional prototype quality.
- Job cards may still be text-heavy.
- Application and recruiter mechanics are shared across jobs.
- Room questions are shared across jobs.
- Room art, characters, and UI remain prototype quality.
- Outcome email and inbox presentation are not final.
- Aftermath uses symbolic prototype interactions, procedural fragments, and placeholder mood assets.
- No authored sound pass, final animation pass, or final room art pass is included.
- Main Menu is still hosted by `InterviewRoom`.
- Debug tools remain available.

## Safety Boundary

The Aftermath mode is symbolic and cathartic, not a workplace attack fantasy.

It should contain:

- symbolic objects;
- corporate phrases;
- rejection fragments;
- empty room furniture.

It should not contain:

- people;
- recruiters;
- interviewers;
- civilians;
- mannequins or human targets;
- firearms;
- blood or gore;
- revenge framing.

