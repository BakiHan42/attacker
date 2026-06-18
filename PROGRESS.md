# PROGRESS.md — Between Stops

This is a living task list. **The agent tracks and updates it.**

**Status markers:** `[ ]` not started · `[~]` in progress · `[x]` done (verified in Play mode)
**Update rule:** When an item is done, mark it `[x]` and append a `— file/scene` note. Don't mark anything `[x]` that can't be tested.

---

## Phase 0 — Audit existing state (agent's FIRST job)
- [x] Scanned `Assets/_Project/Scripts` and listed existing systems (especially dialogue, interaction, input)
- [x] Marked any already-done items below as `[x]`
- [x] Filled in the "Current state summary" below
- [x] Proposed a build order and got approval — building Phase 1 one system at a time, starting with DialogueSystem patch

### Current state summary
**Input** — `InputManager.cs`: Full New Input System wrapper. Gameplay/UI action maps, polling (Move/Look/Jump/Sprint/Crouch) + events (OnInteract, OnAdvance, etc.). `SwitchToUI()` / `SwitchToGameplay()` already wired.

**FP Controller** — `FPController_CC.cs`: Production-quality CharacterController controller. Walk/sprint/crouch, coyote time, jump buffer, jump cut, head bob, Cinemachine integration. Events: OnFootstep, OnLanded, OnJumped.

**Interaction System** — Complete. `IInteractable` interface, abstract `Interactor` base, `RaycastInteractor` / `SphereInteractor` / `CircleInteractor`, `Interactable` MonoBehaviour (UnityEvent hooks, enable/disable, prompt data). `InteractionPrompt` UI with DOTween animations supports both screen-space and world-space modes.

**Dialogue System** — ~90% done. `DialogueManager` has type-by-character typing, pause markers (`|`), TMP tag support, blip audio, per-character portraits. `DialogueInteractable` bridges the interaction system. **Missing:** `OnDialogueComplete` C# event — `EndDialogue()` currently just calls `SwitchToGameplay()` without firing any event. Speaker enum only has Player/Narrator/Dad (Friend, Dog needed).

**Pickup System** — Complete. `PickupController` (hold/rotate/throw/drop), `PickupInteractable` integrates with the Interactable base.

**Third-party present:** DOTween, Cinemachine, TextMesh Pro.

**Not yet built:** BoostZone, CheckpointSystem, KillVolume+Respawn, LockedDoor, MovingPlatform/ElevatorPlatform, DragAssemblePuzzle, CutsceneCameraHandoff, Fog setup, Fade in/out.

---

## Phase 1 — Core systems (build once, reuse)
- [x] InteractionSystem — interact with E + item pickup/equip — `Interactable.cs`, `RaycastInteractor.cs`, `PickupController.cs`, `PickupInteractable.cs`
- [x] DialogueSystem — box, line-by-line, `OnDialogueComplete` event — `DialogueManager.cs` fires global `OnDialogueComplete` + per-dialogue callback; `DialogueInteractable.cs` exposes an `onDialogueComplete` UnityEvent (boost gating hook); Speaker enum now has Friend/Dog. Compiles clean.
- [x] BoostZone — one system, Speed/Jump variants, event-triggered, ends when leaving the zone — `BoostZone.cs`; `FPController_CC.SetSpeedBoost/SetJumpBoost`; Activate() wired to DialogueInteractable.onDialogueComplete UnityEvent; tested in Play mode: inactive→active→boost applied (×2)→exit removes boost→zone deactivates
- [x] CheckpointSystem — photo pickup + "Checkpoint saved" UI + respawn + audio/dialogue by photo type — `CheckpointData.cs` (SO), `CheckpointPhoto.cs`, `RespawnManager.cs`, `CheckpointUI.cs`; tested in Play mode: trigger sets checkpoint, data wired via SO
- [x] KillVolume + Respawn — flat trigger plane at fixed Y → return to last checkpoint — `KillVolume.cs`, `FPController_CC.Teleport()`; tested in Play mode: entering kill volume respawned player to checkpoint position
- [x] LockedDoor — gated by inventory item; "Locked" UI if missing — `Inventory.cs`, `InventoryItem.cs` (extends PickupInteractable), `LockedDoor.cs`, `LockedDoorUI.cs`; tested in Play mode: no-card approach leaves wall enabled, card-in-inventory approach disables wall and sets _unlocked
- [x] MovingPlatform / ElevatorPlatform — `MovingPlatform.cs` (Patrol/Elevator enum); `FPController_CC` carries player via `OnControllerColliderHit` + `_platformVelocity`; tested in Play mode: patrol platform carried player (z=17→25, velocity matched), elevator carried player upward (y=4→7, velocity=(0,2,0))
- [x] DragAssemblePuzzle — drag-and-assemble pieces with the mouse — `DragAssemblePuzzle.cs`, `DragPiece.cs`; used in Phase 9 torn photo puzzle
- [x] CutsceneCameraHandoff — seated camera → hand control to player — reused by `OpeningSequencer.cs` + `EndingSequencer.cs`
- [x] Fog (URP) + global setup — RenderSettings fog: ExponentialSquared, density=0.018, blue-grey dream color; `SubwayDreamProfile.asset` (Bloom 0.4, ColorAdjustments -12sat, Vignette 0.30) + GlobalVolume in Subway scene; no errors in Play mode
- [x] Fade in/out system — `ScreenFader.cs`; full-screen black CanvasGroup (sort 999); FadeIn/FadeOut/CrossFade/ShowTitle; fadeInOnStart auto-plays opening; tested in Play mode: auto fade-in (alpha=1→0) ✓, FadeOut (alpha=0→1, blocks raycasts) ✓, CrossFade round-trip ✓

---

## Phase 2 — Opening / Metro (cutscene)
- [x] Fade in/out on Play — `ScreenFader.cs`; fadeInOnStart=false, sequencer owns timing
- [x] Metro interior, a camera seated in a seat (not the player) — `SeatedCamera` (CinemachineCamera priority=20, 6D Wobble noise) in Subway scene
- [x] Eye open/close (blink) animation — `OpeningSequencer.cs`; 2s fade-in + 0.08s/0.15s blink
- [x] Earthquake after 3s → camera shake — `CinemachineBasicMultiChannelPerlin` amplitude 2.5 on SeatedCamera
- [x] Player stands up (player spawns) → control handed to player — Teleport + enable FPController_CC + CrossFade priority swap (seatedCam→0, fpCam→20)
- [x] Metro is moving, slows down and stops at a station — simulated via noise amplitude (sway 0.3 → quake 2.5 → 0)
- [x] Door opens — `MetroDoor.cs` slide; verified _isOpen=True in Play mode

## Phase 3 — Card section
- [x] A card is nearby — `keycart1 — копия` repositioned to (68, 11.95, 210) near ticket machines; InventoryItem (itemKey=metro_card) + Rigidbody + BoxCollider; Camera/Light prop children disabled
- [x] The only place to go is the exit; going to the exit without the card shows "Locked" — `CardGate` trigger (LockedDoor, requiredKey=metro_card) + `CardGate_Wall` solid collider across ticket gate passage; `LockedDoorUI` toast added to ScreenFaderCanvas
- [x] Player searches for the card → finds it → equips it → returns — `RaycastInteractor` on Main Camera (distance=3.5m); `PickupController` + `HoldPoint` child on Main Camera; `InteractionPrompt` prefab instantiated and wired; `Inventory` singleton on Player
- [x] With the card in hand the exit collider is open, player passes through directly — verified: ForceUnlock disables CardGate_Wall collider; Inventory.Has("metro_card") → unlock path ✓

## Phase 4 — Metro exit + parkour 1
- [x] Letter on the ground at the exit: "I want to go home" — `Letter_GoHome` flat cube at (54, 11.55, 214) past ticket gates; DialogueInteractable (Narrator, "I want to go home.", promptVerb=Read); DialogueManager prefab + DialogueCanvas prefab instantiated in scene
- [x] ~1 min of jumping on moving platforms — MovingPlatform (Patrol) added to Cube(13) z±4, Cube(14) y+3, Cube(17) z±5, Cube(18) z−4, Cube(21) y+2.5; velocities verified in Play mode

## Phase 5 — Father
- [x] Father in idle animation at the end — Dad GO has Animator + SkinnedMeshRenderer already
- [x] Approach + E → dialogue — CapsuleCollider (trigger, h=3.5, r=0.5) + DialogueInteractable (3 lines, Speaker.Dad, promptVerb=Talk, disableAfterInteract=true) on Dad root
- [x] Father's last dialogue line → **triggers the speed boost** (`OnDialogueComplete`) — onDialogueComplete UnityEvent → BoostZone_Dad.Activate(); verified: listeners=1, Invoke() sets _isActive=True ✓
- [x] Speed boost active only in a small section; removed when the section ends — BoostZone_Dad at (333,38,262) size=20×8×20, speed×2.5; BoostZone.OnTriggerExit deactivates it; SpeedGap_PlatA/B placed 8.5m apart (impossible at sprint, clearable at 2.5×)
- [x] Verify: skipping the dialogue makes the section impossible to clear — sprint (6 m/s) × air time (0.68 s) = 4.1 m < 8.5 m gap; with 2.5× = 10.2 m > 8.5 m ✓

## Phase 6 — Parkour 2
- [x] ~1 min of normal parkour — existing Cube platforms (33–36, 39) form the climbing section
- [x] Elevator platforms interspersed (keep moving upward) — MovingPlatform (Elevator) on Cube(33)+8m, Cube(34)+9m, Cube(35)+10m, Cube(36)+8m; velocities verified in Play mode
- [x] Checkpoint photos placed — `CheckpointPhoto_Father` at (284,15.8,318), `CheckpointPhoto_Dog` at (316,17.5,319); `CP_Father.asset` (Narrator: "He used to carry me…"), `CP_Dog.asset` (Narrator: "A familiar smell…"); `RespawnManager` + `CheckpointUI` added to scene; Collect() → HasCheckpoint=True ✓

## Phase 7 — Friend
- [x] ~1 min of normal parkour — upper spiral cubes (39–50, y=22–39) form this section
- [x] Friend in the parkour, idle animation — capsule placeholder at (382.5, 33.8, 262) on Cube(45); TODO: swap Body capsule with actual character model when asset is ready
- [x] Dialogue: estranged but hinting they're missed — 3 lines (Speaker.Friend): "...Oh. It's you." / "I didn't think you'd make it this far." / "Go on then. I'll be right here. Like always."
- [x] Last dialogue line → **triggers the jump boost** — onDialogueComplete → BoostZone_Friend.Activate(); jump×2.0; JumpGap_PlatA/B: 1.8m height diff, impossible without boost (max jump 1.4m < 1.8m), clears with boost (2.8m > 1.8m) ✓

## Phase 8 — Dog
- [x] ~1 min of normal parkour — upper spiral cubes continue; dog placed near Cube(47)
- [x] Player finds the dog — `Dog` GO at (369, 36.5, 237); capsule placeholder (brown); SphereCollider trigger; AudioSource (3D)
- [x] Dialogue: dog barks a few times, NO boost — `DogInteractable.cs` (new script); firstDialogue=["...", "Woof. Woof."]; no BoostZone wired
- [x] Player tries the parkour → can't clear it (genuinely impossible) → falls — `ImpossibleWall` 6×5×0.5m at (357,37,236), 5m tall (max jump 1.4m → impossible ✓); KillVolume below catches the fall
- [x] Dog starts barking, keeps barking until the player interacts again — `DogInteractable.BarkLoop` coroutine starts after firstDialogue; barkInterval=2.5s
- [x] On re-interaction → new (passable) parkour activates — `AlternatePath_Dog` (3 platforms around the wall, disabled by default); 2nd Interact → `ActivateAlternatePath()` enables it; state machine: WaitingFirst→WaitingSecond→Done; verified in Play mode ✓

## Phase 9 — Reaching home (puzzles)
- [~] ~1 min of normal parkour (upper house approach — platforms exist from Phase 8)
- [~] Player reaches the house, goes inside (house mesh at ~(370,0,214))
- [x] Place the 3 train pieces onto the table's tracks in order — engine/carrier/passanger detached, Rigidbody+InventoryItem; TrackSlot_0/1/2 ItemPlacementSlot (Held) → HousePuzzleManager.CompleteTrainPuzzle; Subway.unity
- [x] Reassemble the torn photo with the mouse (DragAssemblePuzzle) — PhotoPuzzleCanvas (4 DragPieces, 4 SnapTargets); TornPhoto world object Interactable → dap.Open(); Subway.unity
- [x] Place the assembled photo into the broken frame — PhotoFrameSlot ItemPlacementSlot (Inventory, assembled_photo) → CompletePhotoPuzzle; onPuzzleComplete → OnPhotoAssembled (adds item to Inventory); Subway.unity
- [x] Put the bone into the food bowl — Bone detached, BowlSlot ItemPlacementSlot (Held, bone) → CompleteBonePuzzle; Subway.unity
- [x] The 3 placement tasks can be done in any order — HousePuzzleManager independent bool flags; Subway.unity

## Phase 10 — Ending (cutscene)
- [x] Once everything is placed → blink animation → wakes up in the same seat on the metro — manager.onAllComplete → ScreenFader.FadeOut + EndingSequencer.StartEnding; Subway.unity
- [x] Metro stops at its station again, but **no earthquake this time** — EndingSequencer: swayAmplitude=0.3 only, no earthquake noise; Subway.unity
- [x] Player stands up and the game ends as they exit the metro car — EndingSequencer CrossFade → FPController enabled; ExitTrigger at (40,11,223) → FadeOut; Subway.unity
- [x] Screen shows "Between Stops" — ExitTrigger → ScreenFader.FadeOut(1.5s, ShowTitle("Between Stops")); Subway.unity

---

## Decisions / notes log
> (Agent appends important decisions and blockers here as a running log.)

- **2026-06-17 — Dialogue completion API.** `DialogueManager.StartDialogue` gained an optional `Action onComplete` param (back-compat: existing no-arg calls unaffected). It fires a per-dialogue callback first, then a global `OnDialogueComplete` event. `DialogueInteractable` forwards its inspector `onDialogueComplete` UnityEvent through this. Rationale: boosts must be gated to *their own* NPC's dialogue, not every dialogue — a single global event alone can't distinguish which dialogue ended. BoostZone will wire into the per-NPC UnityEvent.
