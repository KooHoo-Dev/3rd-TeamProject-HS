# Host–guest authority audit scope

## Audit basis

- Date: 2026-09-09
- Baseline commit: `6c54ff66a02b3514e285be94f366e40040fec330` (`6c54ff6`)
- Initial working tree: `docs/` was untracked; no tracked product-file change was made during this audit.
- Method: static inspection only. Unity was not launched; no build or test was run.
- Authority contract: `RoomClient.Instance.Me.IsHost` describes the local process. A `PlayerController.User` identifies the represented player and does not grant local simulation authority.

## Entry assets and assignment

`ProjectSettings/EditorBuildSettings.asset` enables `Assets/00. Scenes/MainMenu.unity` and `Assets/00. Scenes/InGame.unity`. The InGame scene directly references `GameManager` and `SpawnManager`; the Network RoomClient prefab is `Assets/04. Prefabs/Network/RoomClient.prefab`.

| Runtime surface | Entry / lifecycle boundary | Audit owner | Status |
| --- | --- | --- | --- |
| Room connection, role assignment, player lifecycle, snapshots | `RoomClient` receive dispatch → `OnWelcome` / `OnSnapshot` / `OnGuestInputGroup` | A | See `A-network.md` |
| Player simulation and presentation | `RoomClient.Spawn` → player prefab `Awake` / `OnEnable` / `Setup`; guest snapshots | B | See `B-player.md` |
| Fish and dynamic world objects | `SpawnManager` / `FishObjectPool` and `WorldItemSnapshotSynchronizer` | C | See `C-fish.md` |
| Resources, plank and bridge | collision/input paths and `WorldItemSnapshotSynchronizer.ApplySnapshot` | D | See `D-items-bridge.md` |
| Day, finish state and presentation | `DayLoop`, `VictoryZone`, `GameManager`, result/UI/audio paths | E | See `E-world-presentation.md` |

## Process-role map

| Process and represented object | Input | Gameplay simulation / physics | Snapshot handling |
| --- | --- | --- | --- |
| Host, host player | Local input is applied | Host computes it | Produces full snapshot |
| Host, guest player | Guest intent arrives in `GuestInputGroupMessage` | Host applies that input to the host-side remote player | Produces full snapshot |
| Guest, own player replica | Sends input intent | Must be display-only | Receives host state |
| Guest, other-player replicas | No input | Must be display-only | Receives host state |
| Before `welcome` / after teardown | No role is established | Must be separately proven safe; `Me == null` is not evidence of host authority | No authoritative snapshot yet |

## Narrow exclusions and unverified boundaries

- Tutorial-only behavior beneath `Assets/01. Scripts/1403. Tutorials` is outside the gameplay audit except where it initializes or enters the general-game path.
- Only the Octopus projectile firing path is excluded. Shared fish movement, state, pools, and player interaction remain in scope.
- Relay-server source and deployment configuration were not found in the client tree; server-side validation and WebSocket session behavior are unverified.
- Scene/prefab reference coverage is static. Serialized UnityEvents, animation events, and every child component need runtime confirmation where this report marks an item `미확인`.
- Reconnect and host migration are not assumed supported. Their actual behavior is recorded only where a client path was statically observable.

## Execution order observed on the guest snapshot path

`RoomClient.OnSnapshot` applies players → bridge count → fish/plank world state → game counts → inventory → day state. Any consumer that turns an applied state into a new rule decision or one-shot event is assessed in the domain report; this order is relevant to the `HG-E-001` candidate in `findings.md`.
