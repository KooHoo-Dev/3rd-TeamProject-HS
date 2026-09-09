# Runtime validation plan — not executed

No Unity execution, batch test, build, or test was run for this audit.

| Scenario | Observe | Expected |
| --- | --- | --- |
| delayed welcome / first snapshot | scene objects, PlayerController spawn timing, GameManager subscriptions per process role | guest has no gameplay calls before role/first result |
| host+guest grab | hand target selection, catch callback, joints, fish state, VFX/SFX | host only executes mechanics; guest shows received result |
| fish spawn and pool reuse for each production prefab | FSM OnEnable/Enter/Tick, child triggers, rigidbody state | guest performs no AI, RNG, hunger, attacks, or force |
| terminal state and same-snapshot bridge/count change | host result versus guest root/UI activation | guest consumes a host-confirmed result, exactly once |
| late join/merged bridge counts | bridge particle/SFX start count | historical builds do not replay as fresh events |
| collision/MineFish effects | guest local callback/velocity and effect invocations | effects originate from host result, not guest collision |
| leave/rejoin | caches, SendLoop count, role and gameplay callbacks | old session never authorizes or leaks into new gameplay |

For each run, record role, session, object/world ID, call site, pre/post value, snapshot count, and presentation invocation separately. A visual match alone is not a pass.
