# Final Round Blender Signature Props Brief

## Purpose

Use Blender only for small, high-identity props that make the room feel like Final Round. Generic furniture and office dressing can come from existing asset packs.

Export recommendation for all props: `FBX` with applied scale, Unity meters, simple materials assigned, and pivots placed for easy scene placement. `glTF` is also acceptable if the Unity import workflow is already comfortable.

## 1. Final Round Meeting Room Sign

Purpose: Makes the doorway readable and gives the room a corporate identity before the player sits down.

Approximate dimensions: `0.85m W x 0.35m H x 0.03m D`.

Modelling instructions:
- Make a shallow rectangular wall plaque with a slight bevel.
- Add raised or inset text reading `FINAL ROUND`.
- Add a tiny secondary strip or badge area for a room number, but keep it illegible or minimal.

Material notes:
- Brushed dark metal or matte smoked acrylic.
- Text in off-white or muted teal.
- Low roughness variation only; no bright emissive.

Where it appears: Right side of the meeting-room entrance, near the existing generated sign position.

## 2. Northbridge Mail Laptop / Screen Frame

Purpose: Connects the post-interview outcome email to an in-room device instead of a generic overlay.

Approximate dimensions: base `0.65m W x 0.42m D x 0.035m H`, screen `0.65m W x 0.4m H x 0.03m D`.

Modelling instructions:
- Model an open laptop at roughly a 105-degree hinge angle.
- Keep the screen as a flat separate mesh named clearly, e.g. `Screen_Surface`.
- Add a small understated `Northbridge Mail` mark on the bezel or base.

Material notes:
- Matte black or dark graphite body.
- Screen material can be dark teal/blue with optional low emissive in Unity.
- Avoid detailed keys unless quick; a simple keyboard texture plane is enough.

Where it appears: On the meeting table, angled toward the candidate/seated camera.

## 3. Interviewer Nameplates

Purpose: Reinforces the panel structure and makes the three placeholder interviewers more legible.

Approximate dimensions: `0.42m W x 0.11m H x 0.06m D` each.

Modelling instructions:
- Make a simple tent-card or acrylic desk nameplate.
- Create three variants or one reusable mesh with separate material/text:
  - Hiring Manager
  - Principal Security Architect
  - Sales Director
- Keep text large enough to read from the seated view.

Material notes:
- Dark acrylic backing.
- Off-white text.
- A thin muted teal line is acceptable.

Where it appears: On the interviewer side of the table or attached to the placeholder panel backing.

## 4. Security Architecture Whiteboard

Purpose: Adds cybersecurity presales context without adding gameplay.

Approximate dimensions: `1.15m W x 0.75m H x 0.04m D`.

Modelling instructions:
- Model a simple framed whiteboard.
- Add low-detail diagram marks: cloud, firewall box, arrows, risk callout rectangle.
- These can be geometry strips or a simple texture.

Material notes:
- Off-white board surface with slight roughness.
- Dark grey marker lines.
- One muted teal highlight box for visual continuity.

Where it appears: Back wall behind or beside the interviewer panel.

## 5. Candidate Visitor Badge

Purpose: Adds human presence and a slightly uncomfortable corporate detail.

Approximate dimensions: badge `0.08m W x 0.12m H`, lanyard optional.

Modelling instructions:
- Model a thin plastic badge card.
- Add simple blocks for photo, barcode, and text fields.
- Optional lanyard can be a simple curve/mesh loop resting on the table.

Material notes:
- Slightly glossy plastic.
- Desaturated off-white badge surface.
- Small teal or grey header bar.

Where it appears: On the table near the candidate notepad, or hanging from the candidate chair.

