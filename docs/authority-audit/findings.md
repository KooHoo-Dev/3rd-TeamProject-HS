# Consolidated findings

This is a static audit, not a claim that the game is compliant or runtime-verified.

| ID | Priority | Level | Finding |
| --- | --- | --- | --- |
| HG-001 | P1 | Statically confirmed | Guest `GameManager.ApplySnapshot` independently evaluates win/loss and enables result UI; no authoritative outcome is sent. See `E-world-presentation.md`. |
| HG-002 | P2 | Additional confirmation required | InGame loads before welcome and several predicates treat `Me == null` as locally authoritative. The current InGame scene has no serialized `PlayerController`; the observed pre-welcome GameManager subscription is guarded again when invoked, so a guest gameplay-state mutation was not statically proven. See `A-network.md`. |
| HG-003 | P1 | Statically confirmed | Guest player snapshot invokes real grab/release mechanics and target callbacks. See `B-player.md`. |
| HG-004 | P1 | Statically confirmed | Guest fish activation starts/continues FSM and unguarded species mechanics; movement lock is insufficient. See `C-fish.md`. |
| HG-005 | P2 | Static behavior confirmed; impact needs run | Bridge-count state replays build one-shot VFX/SFX on late join or merged increments. See `D-items-bridge.md`. |
| HG-006 | P2 | Additional confirmation required | Guest collision SFX and MineFish child state lack authoritative event/child-state evidence. See `E-world-presentation.md`, `C-fish.md`. |
| HG-007 | P2 | Additional confirmation required | Disconnect/rejoin session boundary/cache behavior requires a multi-process run. See `A-network.md`. |

Priority remediation order: make snapshot consumers presentation-only (especially grab and game result), then stop guest fish FSM/child mechanics without removing host-derived VFX/SFX. Validate the role-unknown window before treating it as a product defect. The following protocol/event design is intentionally not specified by this audit.
