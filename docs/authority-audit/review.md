# Independent consolidation review

Static cross-check only; no Unity run/build/test. Duplicate candidates were merged by root cause.

| Candidate | Review result | Reason |
| --- | --- | --- |
| A-002 and E-P1-01 | Merged as `HG-001` | Both identify guest `GameManager.ApplySnapshot` calling `EvaluateGameEnd`; the bridge event is an ordering amplifier, not a separate root issue. |
| A-001 and B stamina/arm behavior | Downgraded to `HG-002` (additional confirmation) | InGame loads before welcome and `Me == null` permits local logic, but no PlayerController is serialized in the current scene; players are spawned after welcome. The pre-welcome GameManager subscription calls a second role guard when the event fires. |
| B-P1-01 | Retained as `HG-003` | Snapshot calls collision-dependent grab mechanics, distinct from simple transform replication. |
| C-P1-01 and species callbacks | Retained as `HG-004` | Activation/FSM root cause covers fish AI plus unguarded species child callbacks. |
| D-P2-01, E-P2-03, Mine visuals | Retained as presentation candidates | State-only protocol cannot prove exactly-once event reproduction; no Unity callback behavior was assumed. |
| OldMap DayTrigger | Not a current confirmed issue | No direct InGame reference was found; retain only if that map is dynamically loaded. |

Normal-path counterexamples were retained: host applies grouped guest input to host-side remote players; SpawnManager exits before subscribing on a confirmed guest; bridge pickup/build collision entry is host-gated; inventory count rendering is snapshot-driven. The current InGame Build Bridge button calls `BuildBridgeWithoutCost`, whose own `Me.IsHost` guard rejects guests (`InGame.unity:7831-7847`; `BridgeManager.cs:142-147`). These are not treated as defects solely because lower-level methods lack their own role guard.
