# Final Round RC16 Assigning Prefabs

## Where To Assign Prefabs

1. Open `Assets/Scenes/InterviewRoom.unity`.
2. In the Hierarchy, select `InterviewRoom > TheRoomPrototypeController`.
3. In the Inspector, find `The Room Prototype Controller`.
4. Expand or scroll to `RC16 Room Asset Slots`.
5. Drag prefab assets from the Project window into the matching fields.

Each field can be left empty. Empty fields use the generated fallback object so the prototype remains playable without imported assets.

## Available Slots

- Meeting Table Prefab
- Candidate Chair Prefab
- Interviewer Panel Prefab
- Laptop Prefab
- Notepad Prefab
- Water Glass Prefab
- Wall Screen Prefab
- Whiteboard Prefab
- Room Sign Prefab
- Door/Doorframe Prefab
- Corner Prop/Plant Prefab
- Ceiling Light Prefab

## Recommended Test Order

Test one prefab at a time, starting with `Laptop Prefab`.

1. Assign only `Laptop Prefab`.
2. Press Play.
3. Confirm the room still generates.
4. Sit down and complete the interview.
5. Confirm the outcome email still appears and restart still works.
6. Add the next prefab slot and repeat.

This makes it easier to identify scale, pivot, or material issues in a single imported asset.

## Chair Pivot Fix

Some office-chair prefabs use a pivot at the wheel base, while the generated fallback chair uses visual center positions. If the imported candidate chair floats or sinks, select `TheRoomPrototypeController` and tune:

- `Snap Candidate Chair Prefab To Floor`
- `Candidate Chair Prefab Floor Y`
- `Candidate Chair Prefab Position Offset`
- `Candidate Chair Prefab Rotation Offset`
- `Candidate Chair Prefab Scale`

Keep `Snap Candidate Chair Prefab To Floor` enabled for imported office-chair prefabs. The controller measures the prefab renderer bounds at runtime and moves the visible bottom to `Candidate Chair Prefab Floor Y`. Use `Candidate Chair Prefab Position Offset` only for small visual nudges after snapping.

## Startup Validation

On Play, the controller logs the RC16 slot status to the Console. Each slot reports either `assigned` with the prefab name or `fallback`.
