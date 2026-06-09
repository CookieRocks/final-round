# Final Round RC18 Interviewer Presence

## Purpose

RC18 adds optional human interviewer prefab slots to the room prototype. The goal is human presence: three clearly readable people behind the table, without adding a full character animation, voice, lip-sync, or facial-rigging pipeline.

## Supported Asset Types

- Static seated full-body character prefabs.
- Static standing full-body character prefabs, if the table hides the lower body convincingly from the seated camera.
- Minimally animated character prefabs, as long as they do not require runtime controller setup.
- Torso, bust, or cutout-style office professional prefabs.
- Stylised or semi-stylised human assets that match the low-poly room tone.

Avoid hyper-real humans unless the rest of the scene is upgraded to match them. Cheap realism can feel uncanny beside the current stylised room.

## Prefab Slots

Open `Assets/Scenes/InterviewRoom.unity`, select `TheRoomPrototypeController`, then assign prefabs under `RC18 Interviewer Presence`:

- `Hiring Manager Prefab`
- `Principal Security Architect Prefab`
- `Sales Director Prefab`

Each role has its own tuning fields:

- `Position Offset`
- `Rotation Offset`
- `Scale Multiplier`

Assigned character prefabs also use shared fit controls:

- `Auto Fit Interviewer Character Prefabs`
- `Interviewer Character Fit By Height`
- `Interviewer Character Target Height`
- `Interviewer Character Target Bounds`
- `Interviewer Character Target Center`
- `Interviewer Character Floor Y`
- `Disable Assigned Interviewer Animators`
- `Add Chair Back For Assigned Interviewer Prefabs`

The imported low-poly man at `Assets/Prefabs/low-poly-ordinary-man-in-shirt-and-pants/source/for_sketchfab2.fbx` can be assigned directly to a role slot. If the model imports with awkward scale or pivot, keep auto-fit and height-fit enabled first, then use the per-role offsets for final polish. For this pass, prefer using him as a static interview-presence asset rather than solving a seated animation pipeline.

## Fallback Behaviour

- If a role prefab is assigned, the room instantiates that prefab for the matching interviewer.
- If a role prefab is empty, the room now creates a simple procedural seated bust for that interviewer.
- If a shared `Interviewer Panel Prefab` is assigned, that panel can still be used as a compatibility fallback.
- The old rectangular panel/avatar system remains available as the final generated fallback path, but the default no-prefab room should now read as three human interviewer figures.

The old `Interviewer Panel Prefab` slot still works as a shared fallback before the generated panel geometry.

## Transform Tuning Guidance

Start with:

- Position Offset: `(0, 0, 0)`
- Rotation Offset: `(0, 0, 0)`
- Scale Multiplier: `(1, 1, 1)`
- Auto Fit Interviewer Character Prefabs: enabled
- Interviewer Character Fit By Height: enabled
- Disable Assigned Interviewer Animators: enabled
- Add Chair Back For Assigned Interviewer Prefabs: enabled

Then tune in Play mode:

- If the character faces away, adjust Y rotation, usually `180`.
- If a full-body prefab sinks or floats, adjust Y position or the shared `Interviewer Character Floor Y`.
- If a bust/cutout appears too low, raise Y position.
- If a full-body character appears tiny, keep `Interviewer Character Fit By Height` enabled and raise `Interviewer Character Target Height`.
- If a character is too large for the table, reduce `Interviewer Character Target Height` or the role scale uniformly.
- If a standing full-body character dominates the room, reduce `Interviewer Character Target Bounds` height slightly before using role scale.
- If a standing full-body character reads as standing rather than seated, lower that role's Y position slightly until the table and chair back sell a seated composition.
- Keep each role close to its default X position so the panel identity remains readable.

## Reaction Emphasis

Characters keep the existing reaction API. Reactions are intentionally subtle:

- slight scale pulse
- slight lean
- small emphasis light change
- nameplate highlight

There is no lip sync, voice, facial rigging, animation controller, or dialogue animation in this pass.

Assigned character prefabs have their `Animator` and legacy `Animation` components disabled by default. This prevents imported idle sway from fighting the serious interview tone. Turn `Disable Assigned Interviewer Animators` off only for assets with a calm seated idle that has already been checked in the room.

## Good Asset Criteria

Good RC18 interviewer assets should:

- read clearly as office professionals from the seated camera view
- have calm, neutral posture
- work without complex runtime setup
- use materials that are readable in a dark corporate room
- avoid exaggerated cyberpunk styling
- avoid uncanny photorealism

The best fit is stylised corporate interviewers: jacket, shirt, simple silhouette, clean materials, and readable head/torso shape.
