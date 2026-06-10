# Final Round - Room Prototype P21: Character Material And Overlay Composition

## What changed

P21 is a visual/material/composition pass for the room prototype. It keeps the legacy RC20 interviewer placement and seated-table illusion, but reduces the glowing mannequin read and moves stage intro text away from interviewer faces.

- Assigned interviewer prefab instances now receive a restrained per-role material tint at runtime.
- The tint is intentionally strong enough to beat the pale imported texture, while still staying in muted corporate tones.
- Runtime material handling can optionally replace imported texture maps with flat role tint on instantiated copies, but this is disabled by default because it can make the figures too dark.
- Runtime material handling also suppresses common shiny/emissive properties when available.
- The panel fill light default is lower, and the per-interviewer emphasis light default is softer.
- Stage intro transition text now appears as a compact lower-third strip instead of a large centered panel over the interviewer faces.
- Role differentiation remains subtle through tint, nameplate color, desk prop accents, and existing per-role placement.

## Inspector tuning

On `TheRoomPrototypeController`:

- `Hiring Manager Character Tint`
- `Principal Security Architect Character Tint`
- `Sales Director Character Tint`
- `Assigned Interviewer Character Tint Strength`
- `Interviewer Character Light Intensity`
- `Interviewer Key Light Intensity`
- `Replace Assigned Interviewer Textures With Tint`

Existing legacy RC20 placement fields remain available:

- per-role position offset
- per-role rotation offset
- per-role scale multiplier
- character target height
- character floor Y

On `CybersecurityPresalesInterviewFlow` if the component is added in-editor:

- `Stage Intro Overlay Vertical Offset`
- `Stage Intro Overlay Max Width`
- `Stage Intro Overlay Opacity`

In the current room scene, `CybersecurityPresalesInterviewFlow` is added at runtime by `TheRoomPrototypeController`, so these overlay values use script defaults unless the component is explicitly placed in the scene.

## Material and tint approach

The imported low-poly man asset may expose only a single material slot. P21 therefore avoids submesh editing or asset surgery. It applies a muted role tint to instantiated renderer materials at runtime:

- Hiring Manager: warmer neutral tint
- Principal Security Architect: cooler technical tint
- Sales Director: sharper commercial tint

If a future character prefab has multiple renderers/materials, the same role tint is applied uniformly. The runtime pass also sets emission to black and lowers smoothness/metallic values when those material properties exist. Texture replacement remains available as a fallback toggle, but the default approach preserves the imported texture and applies a stronger tint over it.

## Overlay composition

The stage intro/interviewer line overlay previously sat near the center of the screen and could cover the interviewer panel and faces. P21 changes it to a lower-third strip with reduced opacity and smaller text. This preserves the seated camera view and keeps the panel readable during stage introductions.

## Known limitations

- The interviewer characters are still standing prefabs hidden by table occlusion.
- There is no true seated pose, rigging, animation pipeline, lip sync, or facial animation.
- Single-material character imports cannot distinguish jacket, shirt, skin, and hair without source asset changes.
- Tinting is a compromise: it reduces the white mannequin look but does not create true jacket/shirt/skin/hair material separation.
- The lower-third overlay can still compete with the table area on unusual aspect ratios.

## Why this avoids rigging and animation

P21 only changes renderer material color, light intensity, generated prop accents, and UI panel placement. It does not alter skeletons, animation clips, controllers, gameplay state, scoring, question selection, reactions, or outcome logic.
