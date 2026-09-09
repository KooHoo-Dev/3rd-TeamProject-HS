# Coverage matrix

| Feature | Entry / asset | Host evidence | Guest block or replica evidence | Presentation evidence | Result |
| --- | --- | --- | --- | --- | --- |
| Role and player lifecycle | RoomClient prefab → InGame | host consumes grouped input | `Me == null` is permissive; current scene spawns players only after welcome | snapshot later applies state | additional confirmation HG-002 |
| Player pose | spawned Player prefab | host packs parts | interpolation targets | transforms | suitable only for pose |
| Grab | player snapshot | host `ApplyInput` | snapshot calls mechanics | SFX/particle locally coupled | violation HG-003 |
| Stamina/death | PlayerStamina | host after role | `Me == null` gap | stamina UI | covered by HG-002 |
| Fish root movement | pool/snapshot | host SpawnManager | `LockForGuest` disables FishMovement | transform snapshot | insufficient: HG-004 |
| Fish FSM/species | fish prefabs | host intended | manager/children remain enabled | state VFX/SFX unverified | violation HG-004 |
| Pickup/build | Bridge collider | host-only collision | guest snapshot count/object replica | UI and build effect | authority suitable; HG-005 presentation |
| Win/loss | GameManager/GameResultCanvas | host count updates | guest re-evaluates | Directors | violation HG-001 |
| Day loop | active InGame DayLoop | host timed transition | guest state/tween | UI/tween | static suitable; OldMap trigger excluded from current scene |

Uncovered without runtime/asset expansion: dynamic map loading, serialized AnimationEvents/UnityEvents not reached by these call chains, external relay validation, reconnect, all species prefab child-collider runtime state.
