# C — fish, FSM, pool, and species mechanics (static audit)

No Unity execution, build, or test was performed. Real fish prefabs reference `FishStateManager`, including clownfish, PinkFish, Shark, squid, mantis shrimp, MineFish, Octopus, and Manta ray (`Assets/04. Prefabs/Fish/*.prefab`; serialized-reference search recorded during audit).

## C-P1-01 / guest pool activation starts fish FSM before only movement is locked

- Classification: guest gameplay logic / initialization boundary
- Confidence: statically confirmed.
- Chain: guest `RoomClient.OnSnapshot` → `WorldItemSnapshotSynchronizer.ApplyFishes` → `FishObjectPool.Get` → `InitializeFish` → `SetActive(true)` → `FishStateManager.OnEnable` → `Random.Range`, hunger reset, `ChangeState(new FishIdleState())`; only after activation does `LockFishForGuest` call `FishMovement.LockForGuest` (`RoomClient.cs:274-278`; `WorldItemSnapshotSynchronizer.cs:99-136,194-201`; `FishObjectPool.cs:25-50,109-114`; `FishStateManager.cs:103-129`; `FishMovement.cs:125-134`).
- Ongoing guest logic: `FishStateManager` remains enabled; its `Update` decrements hunger and ticks the FSM, and its trigger can consume bait/change state (`FishStateManager.cs:136-170`). `LockForGuest` only kinematic-locks and disables `FishMovement`; it does not disable the state manager or its child mechanics.
- Counter-check: `Fish.ApplySnapshot` itself is transform-only (`Fish.cs:15-22`), and fish root movement is subsequently disabled. Those facts do not undo `OnEnable` initialization or stop the surviving FSM.
- User effect: pool reuse/new replicated fish can run random AI, hunger, and contact-driven state changes on a guest before/after its transform is applied.
- Minimal runtime observation (not executed): spawn and pool-reuse each production fish type on a guest; record `OnEnable`, FSM `Enter/Tick`, RNG, trigger/collider callbacks and child rigidbody changes. Expected gameplay calls: zero.

## Species coverage and follow-up

| Surface | Static result |
| --- | --- |
| Shark collider | `OnTriggerEnter2D` calls `FishStateManager.ChangeState(new Shark(...))` without a local-role guard (`SharkCollider.cs:11-29`); reachable if guest child collider callbacks remain active. Included in C-P1-01 validation. |
| Electronic Manta | trigger starts paralysis and cooldown coroutines, mutating `PlayerController.IsParalysis` (`ElectronicArea.cs:17-48`); no local-role guard. Included in C-P1-01 validation. |
| Mantis shrimp | trigger computes direction, starts particle coroutine, and applies player impulse (`ShrimpPunch.cs:21-61`); no local-role guard. Included in C-P1-01 validation. |
| MineFish child body | guest visual-mode method exists, but no caller was found; snapshot lock targets only root `FishMovement`. Child mine simulation/entry path is **additional confirmation required** (`MineFishController.cs:42-125`; `WorldItemSnapshotSynchronizer.cs:194-201`). |
| Octopus projectile | projectile firing is excluded by the requested narrow exception. Shared pool/FSM/activation remains included in C-P1-01. |

The protocol sends `FishState.CurrentState`, but `Fish.ApplySnapshot` does not consume it (`Protocol.cs:65-74`; `Fish.cs:15-22`). State-specific VFX/SFX transfer is therefore unverified, not assumed present.
