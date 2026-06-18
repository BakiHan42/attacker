# CLAUDE.md — Between Stops

## Project
**Between Stops** — a Unity 3D first-person dream platformer. In a metro dream the player wants to "go home"; they encounter their father, a friend, and a dog, reach the house, set the table, and wake up back on the metro. A continuously upward-climbing, atmosphere- and narrative-driven parkour.

The full game flow and task list lives in **`PROGRESS.md`**. Read it first every session.

## Stack
- **Unity** — confirm the version from `ProjectSettings/ProjectVersion.txt`.
- **Render pipeline: URP**
- **Input: New Input System** (package). Do **NOT** use `Input.GetKey` / `Input.GetAxis`. Use an Input Actions asset + PlayerInput or a generated C# wrapper.
- **Unity MCP is connected** — you can edit scenes / GameObjects / components. So you handle scene setup too, not just scripts.
- Language: C#

## How to work (important)
1. **Audit the existing code first.** This is NOT a fresh project — dialogue, interaction, and input systems already exist. Don't rewrite them: check `Assets/Scripts` for anything similar, read it, and extend it.
2. Work through `PROGRESS.md` top to bottom. When an item is done, mark it `[x]` and add a one-line note (which file / which scene).
3. Never mark an item `[x]` unless it's actually testable in Play mode.
4. Before any big architectural decision (changing the event layer, folder structure, adding a package), **ask me**.
5. Testing: there's no CLI build; verification happens in the Unity Editor Play mode. You may add PlayMode tests (Unity Test Framework) where useful, but it's not required.

## Architecture & conventions
- **Event-based, loosely coupled.** Systems don't reference each other directly; they communicate via events. Use the existing event setup; if there is none, set up a simple ScriptableObject-event / C# event pattern and let me know.
- **Data = ScriptableObject.** Dialogue lines, checkpoint photo data, and boost-zone parameters are not hardcoded — they live in ScriptableObjects.
- **One system, many uses.** Never write the same mechanic twice (the boost rule below is critical).
- Folders: `Assets/_Project/Scripts/<System>/`, scenes in `Assets/_Project/Scenes/`.
- Naming and code style: **follow the conventions already in the codebase.**

## Core systems (build once, reuse)
- **InteractionSystem** — interact with E (raycast/trigger), item pickup & equip. Card, letter, train piece, bone, and photo all use this.
- **DialogueSystem** — dialogue box, line-by-line advance; **fires an `OnDialogueComplete` event when finished** (boosts hook into this).
- **BoostZone** — ONE system; `Speed` and `Jump` differ only by parameters. Activated by an event, deactivated when the player leaves the zone.
- **CheckpointSystem** — picking up a photo on the ground → "Checkpoint saved" UI (bottom-right) + records the respawn point + plays audio/dialogue based on the photo type.
- **KillVolume** — a single flat trigger plane at a fixed Y; entering it respawns the player at the last checkpoint.
- **LockedDoor** — passable only if the required item is in the inventory; otherwise shows "Locked" UI.
- **MovingPlatform / ElevatorPlatform** — carries the player; elevators keep moving them upward.
- **DragAssemblePuzzle** — drag-and-assemble pieces with the mouse (the torn photo).
- **CutsceneCameraHandoff** — seated cutscene camera → player spawns → control handed to the player controller.
- **Fog** — URP. For distance fog use RenderSettings fog (Lighting window). True "downward / height fog" is not built-in in URP; keep it simple, and ask me before switching to a custom shader/volume if height fog is needed.

## Design rules you must NOT break
- **Boost gating:** Speed and jump boosts are triggered ONLY by the last line of the relevant dialogue (`OnDialogueComplete`). No other trigger. If the player skips the dialogue, they must not be able to clear that parkour section — this is intentional.
- **The impossible parkour must truly be impossible** (the dog section). The alternative parkour only activates after the second interaction with the dog.
- **Killspace is a single flat plane**, at one fixed vertical level.
- **Checkpoint photos are dual-purpose:** both a respawn point and a narrative beat (audio/dialogue).
