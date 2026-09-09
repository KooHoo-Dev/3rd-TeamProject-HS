# B — player, arms, stamina, and water (static audit)

No Unity execution, build, or test was performed. `RoomClient.Spawn` disables `PlayerInputBranch` and makes each child Rigidbody2D kinematic for a guest (`Assets/01. Scripts/1100. Server/RoomClient.cs:477-490`), but this is not a complete gameplay-logic gate.

## Coverage

| Surface | Host path | Guest path | Finding |
| --- | --- | --- | --- |
| Input and physics | Host applies local/relayed guest input through `PlayerController.ApplyInput` | Guest sends intent and receives pose snapshots | Host simulation contract is present; pre-role window is A-001 |
| Pose interpolation | Host packs body/arm state | `ApplySnapshot` stores targets; `Update` interpolates transforms | Display-only portion is appropriate (`PlayerController.cs:274-289,361-408`) |
| Stamina/death | `PlayerStamina.Update` changes stamina only under `CanRunLocalGameLogic` | Snapshot assigns stamina | A-001: `Me == null` makes this gameplay path eligible before role assignment (`PlayerStamina.cs:52-72,274-282`) |
| Grab/release | Host input calls `HandleGrab` | Snapshot also calls `HandleGrab` | **B-P1-01** |

## B-P1-01 / guest snapshot applies a real grab rather than a display state

- Classification: guest gameplay logic / gameplay physics
- Confidence: statically confirmed; runtime impact measurement remains required.
- Chain: guest `RoomClient.OnSnapshot` → `PlayerController.ApplySnapshot` → `HandleGrab` (`RoomClient.cs:263-268`; `PlayerController.cs:361-408,428-450`) → `BottomArmController.Grab` → `HandControllerV2.Grab` → target selection and `ICatchable.OnGrabbed` (`HandController.cs:76-99,122-151`). The same code emits grab SFX/particle and may start/cancel the miss timer (`HandController.cs:136-205`).
- Why this violates the contract: snapshot booleans are converted into collision-dependent selection, catch callbacks, timers, and state transitions, not merely a visual hand pose. Kinematic bodies do not prove those calls and callbacks are harmless.
- Counter-check: `ApplySnapshot` correctly preserves transform interpolation; that does not constrain `HandleGrab`. No guest-role guard exists on the displayed call chain.
- Minimal runtime observation (not executed): with a guest receiving a held-grab snapshot, record `HandControllerV2.Grab`, chosen target, joint/catch callback, fish state change, particle/SFX. Expected guest rule/physics callbacks: zero; expected visual state: host-derived only.
- Direction only: separate received hand presentation from host-only grab mechanics; retain host-derived SFX/VFX on the guest.

## Presentation transfer note

`PlayerState` carries only grab booleans and pose/stamina (`Protocol.cs:27-42`). It has no grab target ID or one-shot event identity. Thus a display-only replacement needs an explicit way to identify/drive the represented visual target; this report does not prescribe a protocol design.
