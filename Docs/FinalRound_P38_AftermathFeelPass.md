# Final Round - Prototype P38: Aftermath Feel Pass

## Summary

P38 improves the feel of the `AftermathRoom` symbolic destruction loop without changing its safety boundary or core routing.

P37 proved that the functional loop works:

```text
Reject outcome
  -> Return to Desk
  -> Clear the Room
  -> AftermathRoom
  -> process symbolic objects
  -> fill Composure
  -> Return to Desk
```

P38 keeps that loop intact and adds readability, impact, and mood feedback so the `Feedback Hammer` feels less flat.

## Safety And Tone Boundary

Rules preserved:

- no people;
- no recruiters, interviewers, civilians, mannequins, or human targets;
- no firearms;
- no blood or gore;
- no revenge framing;
- no realistic workplace violence framing;
- no heavy physics destruction.

The player is processing rejection by clearing phrases, props, forms, and symbolic room objects. The target is the language/system, not people.

## What Was Added

Updated scripts:

- `Assets/Scripts/AftermathDestructible.cs`
- `Assets/Scripts/AftermathRoomController.cs`

Docs:

- `Docs/FinalRound_P38_AftermathFeelPass.md`
- `FinalRound_GamePlan_Current.md`

## Impact Feedback Changes

Successful hits now add:

- subtle object scale punch;
- stronger punch on destruction;
- small camera shake on hit;
- stronger camera shake on destruction;
- tiny hit pause using unscaled timing;
- visual confirmation even when the object is not destroyed yet.

The feedback remains symbolic and restrained. The implementation avoids combat language and avoids framing the interaction as weapon damage.

## Audio Hooks

`AftermathRoomController` now exposes optional inspector hooks:

- `swingClip`
- `hitClip`
- `destroyClip`
- `completionClip`

Volume tuning fields:

- `swingVolume`
- `hitVolume`
- `destroyVolume`
- `completionVolume`

If no clips are assigned, the system works silently.

## Particle / Visual Burst Changes

P38 adds procedural low-risk burst pieces:

- small cube flecks on successful non-destroying hits;
- larger symbolic fragments on destruction;
- fragments drift and shrink using unscaled time;
- generated colliders are removed so fragments do not become physical hazards.

No blood, gore, realistic impact effects, or heavy physics are used.

## Better Processed Visuals

Generated processed states now read less like a flat material swap:

- more uneven fragment pieces;
- tilted processed state;
- short hit punch before/after processing;
- completion shifts the room background slightly quieter.

This is still procedural placeholder art, but it should communicate "processed" more clearly.

## Target Readability

P38 adds:

- a center reticle;
- reticle color/shape change when aimed at a destructible;
- subtle target scale highlight;
- target label text:

```text
Process: [symbolic object]
```

This keeps the language focused on processing/clearing rather than attacking.

## Room Mood Changes

The generated room is slightly more surreal:

- stronger teal-tinted overhead light;
- more rejection-language phrases on the walls;
- completion darkens/softens the background so the room feels quieter.

P38 does not import asset packs or add final room art.

## Composure Feedback

Composure still starts at `0%` and reaches `100%`.

P38 adds:

- meter pulse when Composure increases;
- stronger Return to Desk highlight at completion;
- milestone text:
  - 25%: `The wording starts to lose its grip.`
  - 50%: `The room feels less loud.`
  - 75%: `You can hear yourself think again.`
  - 100%: `The room is quieter now.`

Return to Desk remains available at all times.

## Feedback Timing

Defaults:

- hit text: about `1.25s`;
- destroyed text: about `2.8s`;
- milestone text: about `3.0s`;
- completion text: at least `4.0s`, then persistent until exit.

## Inspector Tuning Fields

P38 adds tuning fields:

- `hitShakeAmount`
- `destroyShakeAmount`
- `shakeDuration`
- `hitPauseDuration`
- `hitTextDuration`
- `destroyTextDuration`
- `burstPieceCount`
- `composurePulseAmount`
- audio clip and volume fields listed above

Defaults are intentionally modest.

## Still Placeholder

Still not final:

- no authored hammer model;
- no real sound pass unless clips are assigned;
- no authored particles;
- no final aftermath room art;
- no final break animations;
- no post-aftermath Desk action expansion.

## How To Test

Fast debug path:

1. Start from the main menu.
2. Choose `Debug: Start Aftermath`.
3. Confirm `AftermathRoom` loads with active Feedback Hammer controls.
4. Aim at symbolic objects and confirm the target label appears.
5. Left click, `E`, or `Space` to process targets.
6. Confirm hit punch, camera shake, and visual flecks appear.
7. Destroy objects and confirm processed fragments appear.
8. Confirm Composure pulses and milestone lines appear.
9. Fill Composure to `100%`.
10. Confirm `The room is quieter now.` appears and Return to Desk is more strongly highlighted.
11. Return to Desk and confirm aftermath completion still works.

Full flow:

1. Start from `Start Job Search`.
2. Complete Desk, recruiter, and Room flow.
3. Achieve or force `Reject`.
4. Return to Desk.
5. Choose `Clear the Room`.
6. Confirm the same P38 feedback behaviour in `AftermathRoom`.
7. Confirm non-Reject outcomes still do not show `Clear the Room`.
8. Open `AftermathRoom` directly and confirm safe no-active-run protection still works.

## Recommended P39 Prompt

`Final Round - Prototype P39: VS3 Readiness Review`

Suggested scope:

- validate Reject-only routing;
- confirm no people or human targets appear;
- test direct-open protection;
- test debug Aftermath shortcut;
- test Composure completion and Desk return;
- verify VS1/VS2 direct paths still work;
- decide whether VS3 is ready to promote or needs another feel/art pass.

## Validation

Run:

```text
dotnet build "Assembly-CSharp.csproj"
dotnet build "Assembly-CSharp-Editor.csproj"
```

Validation result:

- `dotnet build "Assembly-CSharp.csproj"`: passed, 0 warnings, 0 errors.
- `dotnet build "Assembly-CSharp-Editor.csproj"`: passed, 0 warnings, 0 errors.
