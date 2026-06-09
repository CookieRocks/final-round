# Final Round RC16 Asset Checklist

## Goal

RC16 prepares the runtime-generated interview room to accept imported prefabs while preserving the generated greybox fallback. The room should feel more credible and lived-in without becoming a full art-production milestone.

## Required Asset Slots

Assign these on `The Room Prototype Controller` under `RC16 Room Asset Slots`. Leave any slot empty to use the generated fallback.

| Slot | Purpose | Suggested Scale Notes | Source Recommendation |
| --- | --- | --- | --- |
| Meeting Table Prefab | Main panel table, anchors the room | About 4.6m wide, 1.7m deep, 0.75m high | Asset pack is fine |
| Candidate Chair Prefab | Sit-down target and player-side silhouette | Seat around 0.45m high, back around 1.0m | Asset pack is fine, custom if it needs a signature look |
| Interviewer Chair Panel Prefab | Replaces each interviewer placeholder/panel | Around 1.0m wide, 1.3m tall | Simple custom or pack chair/silhouette |
| Laptop Prefab | Connects final outcome email to the table | Around 0.65m wide, screen open toward player | Custom recommended if branded |
| Notepad Prefab | Small corporate table detail | Around 0.45m by 0.3m | Asset pack or simple custom |
| Water Glass Prefab | Small table detail and scale reference | Around 0.08m radius, 0.18m high | Asset pack is fine |
| Wall Screen Prefab | Corporate display behind the panel | Around 2.3m wide, 0.85m high | Asset pack or simple custom |
| Whiteboard Prefab | Security architecture context | Around 1.1m wide, 0.75m high | Custom recommended for Final Round diagrams |
| Room Sign Prefab | Makes doorway and meeting-room purpose clearer | Around 0.85m wide, 0.35m high | Custom recommended |
| Door/Doorframe Prefab | Clarifies entrance composition | Door opening about 2.2m wide, 2.3m high | Asset pack is fine |
| Plant Or Corner Prop Prefab | Adds quiet office life to a corner | Keep below 1.2m tall | Asset pack is fine |
| Ceiling Light Prefab | Adds believable room source lighting | Around 1.2m by 0.35m panel | Asset pack is fine |

## Optional Assets

- Subtle carpet strip for the hallway.
- Table cable or charger near the laptop.
- Small wall clock, kept dim and non-distracting.
- Visitor badge on the table or candidate chair.
- Muted acoustic wall panels.

## Style Direction

- Modern enterprise meeting room, not luxury boardroom.
- Dark neutral materials: graphite walls, muted blue-grey chair fabric, warm but subdued wood table.
- Small teal/cyan light accents can remain, but should feel like screen glow or wayfinding.
- Avoid bright office stock-photo energy. The mood should stay tense, quiet, and corporate.

## Scale and Pivot Notes

- Use Unity meters.
- Apply transforms before export/import where possible.
- Put pivots at the practical placement point:
  - table at center/base
  - chair at floor center under seat
  - wall assets at center of the wall-facing back plane
  - laptop at base center on table
- Keep imported prefabs roughly aligned to their generated fallback positions first, then tune locally in prefab space.

## Assets Worth Sourcing From Packs

- Generic meeting table.
- Generic office chairs.
- Water glass.
- Plant/corner prop.
- Door/doorframe.
- Ceiling light fixture.
- Notepad if unbranded.

## Assets Worth Making Custom In Blender

- Final Round meeting room sign.
- Northbridge Mail laptop/screen frame.
- Interviewer nameplates.
- Security architecture whiteboard.
- Candidate visitor badge.

These are small, visible, and brand/tone-specific. They will do more for the identity of the prototype than replacing every piece of furniture.

## Unity Setup

1. Open the scene containing `The Room Prototype Controller`.
2. Assign imported prefabs to the `RC16 Room Asset Slots` fields.
3. Leave any missing asset slot empty to use the generated fallback object.
4. Keep `Room Life Enabled` on for the subtle pulse/flicker pass.
5. Optionally assign a quiet loop to `Room Hum Clip` and keep `Room Hum Volume` below `0.1`.
6. Press Play and verify the full loop still works: walk in, sit, answer six questions, view outcome email, open scorecard, restart.

