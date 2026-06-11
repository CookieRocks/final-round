# Final Round - Vertical Slice VS3: The Aftermath Planning

## Summary

`Final Round - Vertical Slice VS3: The Aftermath` is a proposed post-rejection slice.

The goal is to make rejection playable without turning the game into a revenge fantasy. After a Reject outcome, the player returns to The Desk, reads the rejection email, and can enter a surreal empty version of The Room where they destroy symbolic objects tied to job hunting, ambiguity, and corporate judgement.

Working mode:

`The Rage Room`

Design target:

> Catharsis, absurdity, disappointment, dark humour, and recovery. Not revenge.

## Safety And Tone Boundary

Hard rules:

- No harming people.
- No recruiters, interviewers, civilians, or human targets present.
- No firearms.
- No blood or gore.
- No workplace attack fantasy.
- No realistic revenge framing.
- Destroy objects, text, symbols, and corporate phrases only.

The player is not attacking a real office or real people. They are processing a rejection through a distorted memory of The Room.

The mode should feel symbolic:

- empty;
- surreal;
- emotionally heightened;
- slightly absurd;
- clear that the targets are systems, phrases, forms, and props.

## Player Flow

Recommended first VS3 flow:

```text
Reject outcome in The Room
  -> Return to Desk
  -> Northbridge Mail rejection message
  -> New aftermath option appears
  -> Enter The Aftermath / Clear the Room / Process Rejection
  -> Load aftermath version of The Room
  -> Smash symbolic objects
  -> Catharsis / Composure meter fills
  -> Return to Desk
  -> Choose next action
```

Desk post-aftermath choices:

- `Apply Again`
- `Take a Break`
- `Ask for Feedback`
- `Start New Run`

Recommended first label:

`Clear the Room`

It reads more symbolic and less aggressive than `Rage Room`, while still supporting the internal design name.

## Emotional Target

The tone should combine:

- frustration;
- absurdity;
- disappointment;
- catharsis;
- dark humour;
- recovery.

What it should not become:

- revenge;
- punishment;
- power fantasy against people;
- workplace violence;
- grim realism.

The player should feel:

> "That was awful. I need to get it out of my system, then decide what to do next."

Not:

> "I am going back to hurt someone."

## Rage Room Rules

### Destructible Targets

Safe symbolic targets:

- empty interviewer chairs;
- interview table;
- laptop;
- feedback forms;
- nameplates;
- rejection email fragments;
- job-ad panels;
- floating corporate phrases;
- whiteboard;
- assessment tablets;
- scorecard shards;
- calendar invites;
- "we went with another candidate" stamps;
- "circle back" signs;
- "strong profile, but..." placards.

### Forbidden Targets

Must not be present:

- people;
- recruiter;
- interviewers;
- civilians;
- character mannequins that read as people;
- human silhouettes used as targets;
- blood/gore props;
- firearms;
- realistic workplace weapons.

If placeholder characters are needed for technical reasons, they should be disabled or removed in the aftermath scene.

## Prototype Mechanics

Recommended simple mechanic:

- first-person movement in the altered Room;
- left-click or `E` to swing/use the symbolic object;
- forward raycast checks for a destructible object;
- destructible object component tracks:
  - intact object;
  - broken object or disabled fragments;
  - catharsis value;
  - optional one-line text reaction;
- object breaks on hit;
- Catharsis / Composure meter fills;
- session ends when meter is full or a short timer completes;
- player returns to Desk.

Avoid physics-heavy destruction in the first prototype unless it is already cheap and stable. A prefab swap, hidden intact mesh, visible broken mesh, and small particle/audio feedback is enough.

## Weapon / Object Framing

Avoid realistic weapon obsession.

Recommended object:

`Feedback Hammer`

Why:

- clearly symbolic;
- tied to the interview/feedback theme;
- readable in UI and debug logs;
- less militarized than a sledgehammer;
- funnier than a generic bat;
- still communicates impact.

Alternates for later:

- `Compliance Mallet`
- `Foam Assessment Baton`
- `Stress Sledge`

For VS3, use `Feedback Hammer`.

## Visual Treatment

Reuse The Room, but make it feel like a memory rather than a real office.

Direction:

- empty room;
- interviewers absent;
- nameplates left behind;
- chairs slightly displaced or too neatly arranged;
- darker or overlit lighting;
- walls covered in rejection phrases;
- floating fragments of the rejection email;
- whiteboard filled with vague feedback language;
- job-ad panels hanging where windows or screens should be;
- corporate phrases drift, flicker, or repeat.

Possible phrases:

- `We went with another candidate`
- `Strong profile`
- `Not quite the fit`
- `We'll keep your details on file`
- `Competitive process`
- `After careful consideration`
- `Circle back`
- `More closely aligned`
- `No feedback available`

Keep it theatrical and abstract. The room should not look like a real workplace being attacked.

## State Integration

First prototype trigger:

- appears only after `Reject`.

Do not trigger for:

- `StrongPass`;
- `Pass`;
- `Hold`.

Future possibility:

- `Hold` could later unlock a Waiting Room variant about ambiguity and silence.
- `Pass` / `StrongPass` should route to next-step or offer/negotiation content, not rage-room content.

CandidateState integration:

- add a small aftermath flag later, for example `aftermathProcessed`;
- keep direct `InterviewRoom` fallback protected;
- keep Reject routing additive so existing Desk inbox still works.

## Rewards And Consequences

Recommended first effects:

- small Energy restore;
- small Stress reduction if Stress exists later;
- small Candidate Confidence recovery or stabilisation;
- unlock clean `Apply Again` path.

Do not make it a major optimization puzzle.

Suggested first prototype values if using current CandidateState fields:

- Energy `+1`;
- Candidate Confidence `+1`;
- no score threshold changes;
- no retroactive Room outcome changes.

The emotional reward matters more than the stat reward.

## Recommended Scope

VS3 should prove one question:

> Can rejection recovery become a playable symbolic aftermath without breaking the serious hiring-pipeline tone?

In scope:

- one Reject-only aftermath path;
- one altered Room mode;
- simple hit/destructible object loop;
- one meter;
- return-to-Desk continuation choices;
- clear safety boundaries.

Out of scope:

- new jobs;
- new companies;
- new recruiter content;
- Room score retuning;
- new interview questions;
- realistic destruction simulation;
- human targets;
- campaign-wide mental-health system;
- fully branching recovery outcomes.

## Milestone Breakdown

### P35 - Aftermath Planning

This document.

Deliverables:

- VS3 design boundary;
- player flow;
- object rules;
- mechanics proposal;
- state integration plan;
- next implementation prompt.

### P36 - Rage Room Scene / Mode Skeleton

Goal:

- add an aftermath mode without gameplay destruction yet.

Scope:

- create or runtime-generate altered empty Room state;
- remove/disable interviewer targets;
- add aftermath entry and exit path;
- add simple objective text;
- add placeholder Catharsis / Composure meter;
- no scoring changes.

### P37 - Destructible Objects Prototype

Goal:

- make symbolic object smashing work.

Scope:

- `AftermathDestructible` component;
- raycast hit interaction;
- intact/broken swap;
- catharsis value;
- short text reaction;
- session completion when meter fills.

### P38 - Desk Integration After Reject

Goal:

- connect aftermath mode into the VS2 loop after Reject.

Scope:

- rejection inbox shows `Clear the Room` or `Process Rejection`;
- aftermath completion returns to Desk;
- add `Apply Again`, `Take a Break`, `Ask for Feedback`, `Start New Run`;
- small CandidateState recovery effect if appropriate.

### P39 - VS3 Readiness Review

Goal:

- validate that VS3 is playable, safe, and emotionally coherent.

Scope:

- test Reject-only routing;
- confirm no people or human targets appear;
- confirm direct Room fallback remains protected;
- confirm Desk loop still works for non-Reject outcomes;
- decide whether to promote to `Final Round - Vertical Slice VS3: The Aftermath`.

## Recommended Next Implementation Prompt

`Final Round - Prototype P36: Rage Room Scene / Mode Skeleton`

Use this prompt:

```text
Implement P36 for Final Round: Rage Room Scene / Mode Skeleton.

Read first:
- FinalRound_GamePlan_Current.md
- Docs/FinalRound_VS3_TheAftermath_Planning.md
- Docs/FinalRound_VS2_TheDesk.md
- Docs/FinalRound_P31_ReturnToDeskInbox.md
- Docs/FinalRound_P25_VS1ReadinessReview.md

Goal:
Add a safe Reject-only aftermath mode skeleton without destructible gameplay yet.

Requirements:
- Do not change Room scoring, thresholds, questions, answer deltas, jobs, or recruiter content.
- Do not add people or human targets to the aftermath mode.
- Do not add firearms, gore, or revenge framing.
- Reuse or runtime-generate an empty distorted Room state.
- Hide/remove interviewer placeholders in aftermath mode.
- Add a Desk post-Reject option such as Clear the Room.
- Load/enter aftermath mode from Reject inbox only.
- Show objective text and a placeholder Catharsis / Composure meter.
- Add a Return to Desk path.
- Keep direct InterviewRoom fallback working.
- Document files changed and test steps.
- Run dotnet build "Assembly-CSharp.csproj".

Do not implement destructible objects yet; that is P37.
```

## Open Questions

- Should the aftermath mode be a separate scene or a mode within `InterviewRoom`?
- Should the Desk call the option `Clear the Room`, `Process Rejection`, or `Enter The Aftermath`?
- Should the meter be named `Catharsis`, `Composure`, or switch from Catharsis to Composure as it fills?
- Should `Ask for Feedback` be a real action in VS3 or just a disabled/placeholder next-step choice?
- Should the altered Room be reachable only once per Reject run?

## Recommendation

Proceed with VS3 as:

`Final Round - Vertical Slice VS3: The Aftermath`

Keep the first implementation narrow:

- Reject only;
- symbolic objects only;
- empty altered Room;
- simple meter;
- return to Desk;
- no new pipeline content.

Move the expanded Recruiter Screen to VS4 unless future playtest feedback says recruiter depth is more urgent than rejection recovery.
