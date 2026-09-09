# A — 네트워크·역할·생성·수신 경계 정적 감사

기준: 2026-09-09, `6c54ff6` (작업 시작 시 `git status --short`: `?? docs/`만 존재). 제품 코드·씬·프리팹은 변경하지 않았고 Unity 실행·빌드·테스트도 하지 않았다.

## 범위와 실제 연결

`ProjectSettings/EditorBuildSettings.asset:6-10`의 활성 시작점은 `Assets/00. Scenes/MainMenu.unity`, `Assets/00. Scenes/InGame.unity`다. MainMenu는 `Assets/04. Prefabs/Network/RoomClient.prefab` 인스턴스를 포함한다 (`MainMenu.unity:2520-2588`); 해당 프리팹은 `RoomClient`를 활성화하고 `Player.prefab`을 `remotePrefab`에 연결한다 (`RoomClient.prefab:15-53`). `SingletonBase<T>.Awake`가 이를 `DontDestroyOnLoad`한다 (`Assets/01. Scripts/9999. Utils/SingletonBase.cs:50-59`). 따라서 `RunAsync`의 씬 전환 뒤에도 같은 RoomClient가 남는다.

`InGame.unity`에는 활성 `EasyRoomClient`도 직렬화되어 있다 (`Assets/00. Scenes/InGame.unity:7070-7100`). 이 구성요소는 `Start`에서 `FindAnyObjectByType<RoomClient>()` 후 `Join`한다 (`Assets/01. Scripts/1100. Server/EasyRoomClient.cs:13-21`). MainMenu의 `GameStartOption.StartGame`도 같은 `Join` 진입점이다 (`Assets/01. Scripts/000. Managers/GameStartOption.cs:36-48`). 전자는 InGame에서 이미 연결 중인 persistent RoomClient에 다시 Join을 호출할 수 있지만 `RoomClient.Join`의 `IsBusy` 조기 반환(`RoomClient.cs:108-119`)으로 중복 접속을 막는다. 실제 MainMenu 버튼/오브젝트 활성화와 EasyRoomClient가 각각 어떤 배포 흐름에서 사용되는지는 실행 없이 확정할 수 없다.

| 프로세스/객체 상태 | 실제 생성·권한 경계 | 입력/물리/결과 처리 | 판정 |
| --- | --- | --- | --- |
| 접속 전 또는 역할 미확정 (`Me == null`) | `RunAsync`는 socket 접속 직후 InGame을 먼저 load하고 (`RoomClient.cs:129-155`), 그 뒤 hello 및 welcome을 기다린다. | 여러 게임 컴포넌트의 `CanRunLocalGameLogic`이 `Me == null`일 때 `true`를 반환한다. 현재 InGame에는 PlayerController 직렬화 참조가 없고 Spawn은 welcome 뒤다. | **추가 확인 필요 A-001** |
| 호스트 / 호스트 자신의 PlayerController | `OnWelcome`에서 Me 설정 후 `Spawn` (`RoomClient.cs:339-381`). Spawn은 호스트 자신의 `PlayerInputBranch`만 켠다 (`RoomClient.cs:477-481`). | 로컬 Update 입력과 `inputGroup`의 원격 입력을 모두 호스트가 `ApplyInput`으로 적용 (`RoomClient.cs:293-315`, `PlayerInputBranch.cs:16-27`). 20 Hz 루프가 모든 `remotes`를 snapshot으로 패킹 (`RoomClient.cs:503-595`). | 적합(하위 플레이어 물리의 세부 적합성은 B) |
| 호스트 / 게스트 소유 PlayerController | 같은 `Spawn`; `user.IsHost == false && Me.IsHost`로 InputBranch 비활성화 (`RoomClient.cs:477-481`). | 호스트가 relay한 입력을 해당 `remote.ApplyInput`에 전달한다 (`RoomClient.cs:301-312`). | 적합(중계 서버의 ID 검증은 미확인) |
| 게스트 / 자기 PlayerController 및 타인 PlayerController | `Spawn` 뒤 InputBranch를 끄고 모든 자식 Rigidbody2D를 Kinematic으로 설정 (`RoomClient.cs:477-490`). | snapshot은 각 PlayerController에 전달된다 (`RoomClient.cs:255-290`). `ApplySnapshot`의 실제 잡기/조인트 경로는 B가 판정해야 한다 (`PlayerController.cs:361-408`, `:428-450`). | 물리 중단만으로 적합 확정 불가; B 전달 |
| 연결 종료·명시적 나가기 | `QuitGame`은 socket close 뒤 `ClearInfo`만 수행 (`RoomClient.cs:218-234`); ReceiveLoop 종료도 `Me = null`만 수행 (`:160-163`). `ClearInfo`는 spawned Player를 destroy하고 컬렉션을 비운다 (`:317-337`). | `SnapshotMessage`, guest-input caches, world/bridge/inventory/GameManager 상태와 SendLoop는 이 경로에서 명시적으로 reset/cancel되지 않는다. `Me == null`이 곧 로컬 게임 로직 허용이다. | **추가 확인 필요 A-003** |

## 메시지 계약과 적용 순서

`Protocol.cs:104-122`의 C2S 메시지는 ID·이동축·좌/우 잡기·건설키만 포함한다. 게스트 프로세스에서 `SendInputMessages`가 이 값을 읽어 전송하고 (`RoomClient.cs:597-621`), 호스트는 `inputGroup` 수신 때 ID로 `remotes`를 찾아 `ApplyInput`한다 (`:293-315`). 이 클라이언트 저장소에는 중계 서버 구현 또는 입력 ID를 송신 socket/세션에 묶는 검증이 없다. 따라서 **클라이언트에서 확인되는 범위**는 “host가 remote player까지 시뮬레이션한다”까지이며, guest가 자기 ID 외 입력을 위조할 수 없는지는 미확인이다.

호스트는 snapshot 하나에 player part pose/stamina/grab (`Protocol.cs:27-42`; `RoomClient.cs:674-743`), fish/plank transform (`Protocol.cs:65-84`, `WorldItemSnapshotSynchronizer.cs:27-72`), inventory, bridge count, day, active/arrived/player tutorial state (`Protocol.cs:124-148`, `RoomClient.cs:566-591`)를 담는다. 수신 guest의 고정 순서는 다음과 같다.

```text
ReceiveLoopAsync (Update 이전 yield) → Handle("snapshot")
→ OnSnapshot
  → PlayerController.ApplySnapshot (각 player)
  → BridgeManager.ApplySnapshot
  → WorldItemSnapshotSynchronizer.ApplySnapshot
  → GameManager.ApplySnapshot
  → InventoryItemDB.ApplySnapshot
  → DayLoop.DayApplyFromNetwork / TutorialManager.UpdateReadyState
```

근거: `RoomClient.cs:168-215`, `:236-290`. 전송과 수신 모두 WebSocket text message를 사용하며 (`RoomClient.cs:173-214`, `:632-660`), UDP식 임의 재정렬/손실을 가정하지 않았다. 다만 응용계층 snapshot sequence·session ID·최초 snapshot marker는 Protocol에 없다 (`Protocol.cs:124-148`). stale snapshot 및 session 분리의 실제 중계 동작은 검증 필요다.

| 필드/결과 | 호스트 생산자 | guest 소비자 | 표현/규칙 판정 |
| --- | --- | --- | --- |
| `Players` body/arms/stamina/grab | `AddPlayerState` (`RoomClient.cs:674-743`) | `PlayerController.ApplySnapshot` (`:263-268`, `PlayerController.cs:361-408`) | pose 보간은 표시로 보이나 stamina/grab 부작용은 B 확인 필요 |
| `Fishes`/`Planks` | `PackActiveWorldItems` (`WorldItemSnapshotSynchronizer.cs:27-72`) | `ApplySnapshot` → pool/register/SetActive/lock/apply (`:74-201`) | spawn/OnEnable/FSM 및 월드 물리 C/D 확인 필요 |
| bridge count | `BridgeManager.CurrentBridgeCount` (`RoomClient.cs:589`) | `BridgeManager.ApplySnapshot` (`:271-273`) | direct guest result evaluation is HG-001; bridge-event ordering needs runtime confirmation |
| active/arrived count | `GameManager` property (`RoomClient.cs:590-591`) | `GameManager.ApplySnapshot` (`:280`, `GameManager.cs:160-166`) | guest가 `EvaluateGameEnd` 실행: E 소유 후보 |
| inventory entries | `InventoryItemDB.EnumerateCounts` (`RoomClient.cs:577-587`) | `InventoryItemDB.ApplySnapshot` (`:281`, `InventoryItemDB.cs:106-144`) | count/UI 변경; pickup/consume 재실행 D 확인 필요 |
| day/tutorial | `DayLoop`/`TutorialManager` reads (`RoomClient.cs:566-575`) | `DayApplyFromNetwork`/`UpdateReadyState` (`:282-290`) | 낮밤 gameplay, UI/event side effects E 확인 필요 |

`FishState.CurrentState`는 전송되지만 (`Protocol.cs:65-74`, `WorldItemSnapshotSynchronizer.cs:44-53`) 이 A 범위에서 확인된 `Fish.ApplySnapshot`은 transform scale/position/rotation만 적용한다 (`Assets/01. Scripts/501. Fish/Fish.cs:15-22`). 상태별 VFX/SFX와 FSM 정지/표현은 C/E가 확인해야 한다.

## 이슈

### A-001 / 역할 확정 전 `Me == null` 권한 모델의 실행 범위

- 분류: 초기화/수신 경계
- 확인 수준: 추가 확인 필요
- 우선순위: P2
- 실행 환경 / 씬·프리팹 / 객체·사용자 역할: MainMenu의 persistent RoomClient로 접속한 미래 guest. socket 연결 성공 후 `InGame`이 활성화되고 welcome이 오기 전.
- 진입 조건과 호출 사슬: `GameStartOption.JoinRoom` 또는 `EasyRoomClient.Start` → `RoomClient.Join` → `RunAsync` → `ConnectAsync` → `SceneManager.LoadSceneAsync(1)` → InGame Awake/OnEnable/Start/Update/FixedUpdate → welcome → `OnWelcome`에서만 `Me`에 guest/host role 대입.
- 근거 파일:줄 (권한 경계와 실제 부작용을 모두 제시): `RoomClient.cs:129-158`, `:339-354`; persistent lifecycle `SingletonBase.cs:50-59`; 일반-게임 허용 predicate `PlayerStamina.cs:52-72,274-282` (물 안/밖 stamina 감소·회복), `PlayerController.cs:87-95,291-294` (arm-force update), `GameManager.cs:45-53,220-227` (bridge event subscription). InGame에 활성 EasyRoomClient가 존재하는 것도 `InGame.unity:7070-7100`에서 확인했다.
- 게스트에서 실행/변경되는 것 또는 전달되지 않는 표현: predicates are permissive and GameManager can subscribe before role assignment. However, current InGame has no serialized PlayerController and players are spawned only in OnWelcome; the observed bridge callback checks the role again. A gameplay-state mutation before welcome is therefore not statically established.
- 관련 호스트 확정 지점과 메시지 생산·수신·소비 경로: welcome이 유일하게 `Me`를 정하고 (`RoomClient.cs:339-344`), snapshot은 그 뒤 `OnSnapshot`에서만 적용된다 (`:255-290`). 이를 전에는 host 결과가 존재하지 않는다.
- 사용자에게 보이는 영향과 발생 조건: guest의 접속 지연이 길수록 시작 stamina/물리/상태 변화 및 이후 승패 event 구독 상태가 host와 달라질 수 있다. 정확한 실제 변화량은 B/E 런타임 계측이 필요하다.
- 확인한 반증: tutorial은 `TutorialManager`가 `Me != null`까지 대기 (`TutorialManager.cs:82-83`)한다. `PlayerController` GUID is absent from InGame and Spawn occurs after welcome (`RoomClient.cs:339-381`). `CheckPlayerWin` also checks the role at invocation (`GameManager.cs:70-74`).
- 최소 재현 절차 / 기대 결과 / 실제 결과 또는 아직 필요한 관측: guest로 접속하고 hello/welcome 또는 첫 snapshot을 2~3초 지연한다. 그 동안 guest에서 PlayerStamina decrease/recover, arm force, GameManager subscription 및 Rigidbody/Joint 변화를 role·frame별로 기록한다. 기대: tutorial 외 gameplay rule/physics는 0회. 실제: 미실행.
- 수정 방향 한두 문장 (구현하지 않음): 로컬 오프라인 실행과 “네트워크 접속 중이지만 role 미확정”을 별도 상태로 모델링하고, 후자에서는 일반 게임 규칙/물리를 시작·구독하지 않도록 역할 확정 전에 잠근다.
- 검증 방법 / 로그·증거 위치 / 미확인 제약: 별도 실행 검증에서 scene load/welcome/first snapshot 순서와 각 lifecycle 호출을 계측한다. 정확한 scene 오브젝트 활성 순서는 Unity 실행 없이 확정하지 않았다.
- 관련 담당 / 중복 원인 ID: B (stamina/arm), E (GameManager), scope lifecycle. A-003과 같은 `Me == null` 권한 모델이 근본 원인이다.

### A-002 / bridge 수신 순서가 `HG-001`을 증폭할 수 있는지의 확인 항목

- 분류: 초기화/수신 경계
- 확인 수준: 추가 확인 필요 (독립 이슈로는 병합됨)
- 우선순위: P2
- 실행 환경 / 씬·프리팹 / 객체·사용자 역할: guest가 bridge count 증가와 active/arrived player count 변경을 같은 snapshot에서 수신하는 InGame.
- 진입 조건과 호출 사슬: `OnSnapshot` → `BridgeManager.ApplySnapshot(message.CurrentBridgeCount)` → bridge count 증가 시 `OnBridgeBuild?.Invoke()` → guest의 `GameManager.OnEnable` subscription → `CheckPlayerWin` → `EvaluateGameEnd` → UI 활성화와 `isGameFinished = true`; 그 뒤에야 `GameManager.ApplySnapshot(active, arrived)`가 호출된다.
- 근거 파일:줄 (권한 경계와 실제 부작용을 모두 제시): 수신 순서 `RoomClient.cs:271-281`; bridge event `BridgeManager.cs:91-103`; subscription and host predicate `GameManager.cs:45-75,220-227`; guest end evaluation and irreversible flag/UI `GameManager.cs:77-103,160-166`.
- 게스트에서 실행/변경되는 것 또는 전달되지 않는 표현: direct `GameManager.ApplySnapshot` evaluation is the confirmed issue HG-001. The bridge-event branch is not independently confirmed because `CheckPlayerWin` rechecks `CanRunLocalGameLogic` after role assignment.
- 관련 호스트 확정 지점과 메시지 생산·수신·소비 경로: host sends bridge count and player counts together (`RoomClient.cs:589-595`), but consumer ordering separates them. 별도의 game outcome/result field는 `SnapshotMessage`에 없다 (`Protocol.cs:124-148`).
- 사용자에게 보이는 영향과 발생 조건: bridge 완성과 생존/도착 count가 가까운 시점에 같은 snapshot으로 바뀌면 guest가 host와 다른 game-over/win UI를 영구 표시할 수 있다.
- 확인한 반증: `GameManager.OnEnable`은 Me가 guest로 확정된 뒤에는 subscribe하지 않는 것이 의도지만, A-001의 pre-welcome 시점에 `CanRunLocalGameLogic`이 true여서 subscribe할 수 있다 (`GameManager.cs:45-53,220-227`). 또한 직접 `GameManager.ApplySnapshot`도 Evaluate를 호출하므로, subscription 경로가 없더라도 guest-side rule re-evaluation 자체는 E가 별도 판정해야 한다. 이것은 bridge update의 순서 결함을 반증하지 않는다.
- 최소 재현 절차 / 기대 결과 / 실제 결과 또는 아직 필요한 관측: host에서 마지막 bridge build와 active/arrived count 변화를 같은 send interval에 발생시킨다. guest의 `BridgeManager.currentBridgeCount`, counts, `EvaluateGameEnd` arguments, UI activation, `isGameFinished`를 순서대로 기록한다. 기대: host 확정 결과 하나만 표시. 실제: 미실행.
- 수정 방향 한두 문장 (구현하지 않음): guest 수신 batch를 모두 상태로 반영한 뒤 host-확정 outcome만 표시하거나, 최소한 수신 중 bridge side-effect가 game-rule evaluation을 호출하지 않도록 분리한다.
- 검증 방법 / 로그·증거 위치 / 미확인 제약: E가 host의 실제 outcome 확정 여부와 UI/scene transition을 함께 검증해야 한다. 이 정적 호출 순서는 확정이나, runtime에서 pre-welcome subscription이 항상 생기는지는 별도 관측이 필요하다.
- 관련 담당 / 중복 원인 ID: E (GameManager/victory presentation), D (bridge application). A-001과 함께 lifecycle audit 필요.

### A-003 / 종료·재입장 시 역할/수신 캐시 및 scene gameplay state의 세션 경계 미확인

- 분류: 초기화/수신 경계
- 확인 수준: 추가 확인 필요
- 우선순위: P2
- 실행 환경 / 씬·프리팹 / 객체·사용자 역할: guest 또는 host가 `QuitGame`, WebSocket close, leave message 후 MainMenu로 돌아가 다시 접속하는 지원 흐름.
- 진입 조건과 호출 사슬: Button/Option manager → `QuitGame` → socket close → `ClearInfo`; 또는 `ReceiveLoopAsync`가 close/exception으로 return → `RunAsync` end → `Me = null`. 다음 `Join`은 same persistent RoomClient에서 새 socket/SendLoop를 시작한다.
- 근거 파일:줄 (권한 경계와 실제 부작용을 모두 제시): exit path `RoomClient.cs:160-163,191-204,218-234`; ClearInfo scope `:317-337`; new loop `:108-158,503-522`; local-logic predicate `PlayerController.cs:87-95`, `PlayerStamina.cs:274-282`, `GameManager.cs:220-227`; scene return callers `ButtonManager.cs:31-32`, `OptionManager.cs:77-78`.
- 게스트에서 실행/변경되는 것 또는 전달되지 않는 표현: ClearInfo does not clear `SnapshotMessage`, `GuestInputMessage`, `GuestInputGroupMessage`, `worldItemSynchronizer`, or scene managers/world state. `Me == null` then permits local logic. However, scene reload and destroy timing may reset relevant scene singletons, so persistent cross-session gameplay is not statically proven.
- 관련 호스트 확정 지점과 메시지 생산·수신·소비 경로: no session/version field exists in `SnapshotMessage` (`Protocol.cs:124-148`); old cached player states are observable by camera/debug consumers until overwritten.
- 사용자에게 보이는 영향과 발생 조건: a reconnect can expose stale camera target/snapshot state or let gameplay run locally between disconnect and next welcome; exact user-facing persistence depends on scene lifetime.
- 확인한 반증: ClearInfo does destroy spawned players and clears `remotes`, camera IDs, arrived IDs (`RoomClient.cs:317-337`). `QuitGame` explicitly loads no scene; callers do (`ButtonManager.cs:31-32`, `OptionManager.cs:77-78`). These reduce but do not prove absence of stale cache/loop effects.
- 최소 재현 절차 / 기대 결과 / 실제 결과 또는 아직 필요한 관측: start host+guest, establish snapshot, leave guest by both UI routes and remote socket close, wait 2 seconds, rejoin another room. Record live SendLoop count, Me/IsBusy, cache identity, spawned objects, manager state, and any gameplay lifecycle calls. Expected: no prior session state affects gameplay before new role/first snapshot. Actual: 미실행.
- 수정 방향 한두 문장 (구현하지 않음): define a single disconnect/session-reset boundary that invalidates role and cached network state before gameplay resumes; do not rely on `Me == null` as host authority.
- 검증 방법 / 로그·증거 위치 / 미확인 제약: requires two-process connection/reconnection with relay behavior. No server source exists in this checkout.
- 관련 담당 / 중복 원인 ID: scope lifecycle, B/C/D/E for their scene state; shares root model with A-001.

## 커버리지 및 전달

| 기능/컴포넌트 | 씬·프리팹/생성 경로 | 진입점 | 권한 변경 또는 물리 동작 | 호스트 근거 | 게스트 차단/복제 근거 | 표현 전달 근거 | 판정 | 증거/미확인 이유 | 담당 |
| --- | --- | --- | --- | --- | --- | --- | --- | --- | --- |
| socket lifecycle/role | MainMenu RoomClient prefab → persistent singleton → InGame | `Join`/`RunAsync`/welcome/leave | scene load before role; destroy spawned players on exit | `Me.IsHost` branches | no spawned player before welcome confirmed | n/a | 미확인 | A-001; `RoomClient.cs:108-163,317-381` | A |
| remote input authority | spawned `Player.prefab` | host `inputGroup` | remote `ApplyInput` | `OnGuestInputGroup` host return guard | guests warned/return | later snapshot pose | 적합 (server identity 미확인) | `RoomClient.cs:293-315,477-490` | A/B |
| guest player replication | spawned `Player.prefab` | guest snapshot | kinematic all child Rigidbodies; pose application | host packs all remotes | input branch disabled, kinematic | `Players` → ApplySnapshot | 미확인 | grab/FSM/child scripts require B | A→B |
| world replication | InGame registry/pool | guest snapshot | SetActive, pool get/release, fish lock | host packs active registry | only FishMovement lock observed | fish/plank transform snapshot | 미확인 | C/D must inspect OnEnable/FSM/colliders | A→C/D |
| bridge/player outcome batch | guest snapshot | Bridge then GameManager apply | direct GameManager result evaluation | host packs both fields | no outcome-only field | UI indirectly | 위반 | HG-001; bridge event ordering is A-002 confirmation item | A→E/D |
| inventory/day/tutorial batch | guest snapshot | after GameManager | snapshot state assignment/events | host packs fields | consumer-specific checks outside A | UI/day/tutorial paths | 미확인 | D/E ownership | A→D/E |
| relay trust/session ordering | external relay absent from checkout | C2S/S2C WebSocket | ID association, stale messages | unavailable | unavailable | unavailable | 미확인 | protocol has no sequence/session field; no server source | A/scope |
| debug/test helpers | InGame `NetworkDebugOnGUI`; no serialized `InputSnapshotMessageTester` reference found | ContextMenu/OnGUI | cache mutation/inspection | n/a | not gameplay path proven | debug display | 미사용 확인 (tester); presentation-only (debug GUI) | `InputSnapshotMessageTester.cs:45-110`; scene grep found only NetworkDebugOnGUI | A |

### 타 담당 전달사항

- **B:** guest `PlayerController.ApplySnapshot` calls `body.ApplySnapshot` then `HandleGrab`, and `HandleGrab` calls `BottomArmController.Grab/Release` (`PlayerController.cs:361-408,428-450`). A’s Kinematic/InputBranch guard is insufficient proof that grab, joint, contact, force or SFX are display-only. Also assess `FixedUpdate → UpdateArmForce` on guest (`:291-294,484-516`) and pre-welcome A-001 effects.
- **C:** `WorldItemSnapshotSynchronizer.ApplyFishes` calls pool `Get`, `SetActive(true)`, then `LockFishForGuest`, then apply transform (`WorldItemSnapshotSynchronizer.cs:99-146,194-201`). Inspect lifecycle/FSM execution that occurs before LockForGuest. `CurrentState` is packed but not consumed by Fish.ApplySnapshot (`Fish.cs:15-22`), so state-dependent display likely needs separate evidence.
- **D:** guest bridge snapshot runs before inventory snapshot and invokes build event (`RoomClient.cs:271-281`; `BridgeManager.cs:91-103`). Inspect whether bridge pickup/build particles and inventory `ItemCountChanged` cause guest gameplay or duplicate presentation.
- **E:** `GameManager.ApplySnapshot` itself calls `EvaluateGameEnd` on guest (`GameManager.cs:160-166`); combined with A-002, determine whether outcome must be a host field and whether UI/VFX/SFX gets an authoritative event. Day/tutorial snapshot consumers execute last (`RoomClient.cs:282-290`); inspect ordering and repeat behavior.

## 정적 검증 한계

중계 서버 구현, Unity object activation order, two-process WebSocket sequence, actual prefab child components, and user-visible VFX/SFX execution were not run. 이 문서는 정적 호출 경로와 직렬화 연결만 판정한다; runtime reproduction is required for A-001/A-003 timing and all transferred B–E effects.
