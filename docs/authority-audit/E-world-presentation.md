# E — 시간·게임 결과·UI·VFX/SFX 정적 감사

## 실행 연결과 수신 순서

`InGame`에는 활성 `GameManager` (`Assets/00. Scenes/InGame.unity:1771-1809`)와 `DayLoop` (`InGame.unity:8690-8724`)가 있으며, 결과 UI가 들어 있는 `GameResultCanvas` prefab 인스턴스도 있다 (`InGame.unity:4568-4689`). `Victory Zone` prefab은 씬에 인스턴스화됐지만 root GameObject가 비활성화되어 있고 VictoryZone component도 disabled override 상태다 (`InGame.unity:4831-4908`).

호스트는 `SendSnapshotOrInputMessage`에서 `IsDay`, bridge count, active/arrived counts를 생산한다 (`Assets/01. Scripts/1100. Server/RoomClient.cs:572-595`). 게스트는 같은 snapshot에서 Bridge → WorldItem → GameManager → Inventory → DayLoop 순으로 소비한다 (`RoomClient.cs:255-290`). 이 순서는 GameManager가 필요한 bridge/active/arrived 값은 모두 적용하지만, `GameManager.ApplySnapshot`이 게스트에서 최종 조건을 다시 계산한다는 결함을 막지 못한다.

## 권한/표현 커버리지

| 기능/컴포넌트 | 씬·생성 경로 | 진입점·변경 | 호스트 근거 | 게스트 소비/차단 근거 | UI/VFX/SFX | 판정 |
| --- | --- | --- | --- | --- | --- | --- |
| 게임 종료·승리 | InGame GameManager + GameResultCanvas | host arrived/starvation/bridge event → `EvaluateGameEnd` | VictoryZone/arrival과 starvation handlers에는 `CanRunLocalGameLogic`; host snapshot produces counts (`VictoryZone.cs:9-45`, `GameManager.cs:168-227`, `RoomClient.cs:589-591`) | guest `ApplySnapshot` writes counts **then calls `EvaluateGameEnd` without guard** (`GameManager.cs:160-166`) | game over/win root activation triggers Directors `OnEnable` sequences (`GameManager.cs:77-103`; `GameOverDirector.cs:63-166`; `GameWinDirector.cs:56-118`) | 위반 E-P1-01 |
| VictoryZone contact | currently disabled prefab instance | trigger stay/exit → arrived set/count | `CanRunLocalGameLogic` permits host | guest return after role determined | eventual result UI via snapshot | 적합(현재 씬 비활성); 활성화 시 정적 적합 |
| DayLoop cycle | active InGame object | Update increments UI clock; host alone changes at period | cached `isHost` set after `Me`; only host transition branch invokes `SetDayType` (`DayLoop.cs:49-77`) | guest advances local elapsed/UI but `DayApplyFromNetwork` accepts only network state | transition tween on changed state | 적합(권한), timeline/trigger 후보 |
| DayTrigger | only OldMap prefabs found | trigger → `SetDayType`, destroy trigger | **no role check** | guest can call same method if instantiated | day transition and `OnDayChanged` | 미사용 확인(현재 InGame has no referenced OldMap GUID); E-P2-02 if a runtime path loads it |
| spawn reactions | SpawnManager | Bridge/Day event → release/spawn/barricade | `Start` waits for role and exits guest before subscriptions | no guest subscription | not presentation; C owns simulation | 적합(정적) (`SpawnManager.cs:33-63,78-114`) |
| inventory/bridge UI | InGame UI components | count/build event → render slots/value | snapshot mirrors host counts | render-only listeners | text/UI; bridge has build SFX/VFX | D-P2-01 for one-shot timing; otherwise appropriate |
| player collision SFX | Player prefab `SoundController` (`Assets/04. Prefabs/Player/Player.prefab:741`) | trigger checks local RB speed/layer | no host gate in callback | guest RBs made Kinematic during spawn but component remains enabled (`RoomClient.cs:477-490`) | splash/rock SFX local | 추가 확인 E-P2-03 |
| game over/win sequences | GameResultCanvas | game result roots enabled | result currently comes from local evaluation | both clients can play their locally enabled root's sequence | audio/camera/tweens | follows E-P1-01; display itself is valid only after host result signal |
| local inventory tab | InGame UIManager | LateUpdate key toggles UI | non-gameplay local setting | no state mutation outside canvas active | UI only | 적합 (`UIManager.cs:10-19`) |

## 후보

### E-P1-01 / 게스트가 스냅샷 count로 승패 조건을 재평가하고 결과 UI를 확정

- 분류: 게스트 게임 로직
- 확인 수준: 정적으로 확정
- 우선순위: P1
- 실행 환경 / 씬·프리팹 / 역할: 활성 `InGame` GameManager와 GameResultCanvas; `Me.IsHost == false`인 게스트가 snapshot을 수신.
- 진입 조건과 호출 사슬: host game state → `SnapshotMessage.ActivePlayerCount`/`ArrivedPlayerCount` (`RoomClient.cs:589-595`) → guest `OnSnapshot` (`RoomClient.cs:255-290`) → `GameManager.ApplySnapshot` assigns count and invokes `EvaluateGameEnd` (`GameManager.cs:160-166`) → computes `BridgeManager.IsEnd && arrivedPlayerCount == totalPlayerCount` / `activePlayerCount == 0` and sets result root active (`GameManager.cs:11-15,77-103`) → Director `OnEnable` runs audio/camera/tween (`GameOverDirector.cs:63-166`; `GameWinDirector.cs:56-118`).
- 게스트에서 실행/변경되는 것: host result is not transmitted; the guest independently chooses game-over versus win (including simultaneous-condition precedence), sets `isGameFinished`, and starts the result presentation.
- 관련 호스트 확정/메시지: active/arrived counts and bridge count are replicated, but `Protocol.SnapshotMessage` lacks an authoritative result enum/event (`Protocol.cs:124-148`).
- 사용자 영향: snapshot application order or local `totalPlayerCount` lifecycle can make guests reach a different terminal UI; even equal visuals violate the stated host-only rule evaluation.
- 확인한 반증: `CheckPlayerWin` and starvation/arrival mutation have `CanRunLocalGameLogic` gates (`GameManager.cs:70-75,168-227`), but `ApplySnapshot` has none. Thus no outer guard prevents the shown guest path.
- 최소 재현: host and guest enter, then cause all players to arrive after last bridge, and separately exhaust active players. Log guest `EvaluateGameEnd`/root activation and compare to host. Expected: guest consumes an explicit host result only. Actual runtime evidence: not collected.
- 수정 방향: host result state/event must be produced and guest display must consume it without evaluating predicates. Do not remove guest Directors/SFX; they remain presentation.
- 검증 제약: static only; no Unity execution performed.

### E-P2-02 / DayTrigger가 활성화되면 게스트 접촉으로 낮밤을 확정하고 트리거를 로컬 삭제

- 분류: 게스트 게임 로직 | 초기화/수신 경계
- 확인 수준: 추가 확인 필요 — code path is definitive but current InGame references none of the three OldMap prefab GUIDs containing it.
- 우선순위: P2 if any runtime map load/scene replacement uses it.
- 호출 사슬: `DayTrigger.OnTriggerEnter2D` accepts PlayerRoot → `DayLoop.SetDayType` → updates state, invokes all `OnDayChanged` subscribers, starts transition tween, then `Destroy(gameObject)` (`DayTrigger.cs:10-18`; `DayLoop.cs:82-91,133-150`).
- 근거/반증: `SetDayType` has no host assertion; `DayLoop` only guards its timed Update branch. `DayTrigger` components occur in `FullMap_V3/V4/FullMap_Only_Platforms` (`rg` evidence), while direct GUID search found none in InGame; therefore it is not reported as a current-scene confirmed violation.
- 최소 재현: load each OldMap through its actual supported path as guest, cross DayTrigger, and observe local state/trigger object and host snapshot. Expected: guest never authors transition/delete.

### E-P2-03 / 게스트 충돌 SFX remains based on local physics callback and velocity

- 분류: 표현 누락/중복 | 게스트 게임플레이 물리 의존
- 확인 수준: 추가 확인 필요
- 우선순위: P2
- 호출 사슬: Player prefab SoundController → `OnTriggerEnter2D` → `rb.linearVelocity.magnitude >= 10` and Water-layer test → `AudioManager.PlaySfx` (`SoundController.cs:10-25`; prefab evidence `Player.prefab:741`). Guest spawn changes every player child RB to Kinematic but does not disable SoundController (`RoomClient.cs:477-490`).
- 게스트 영향: no splash/rock event or velocity exists in `PlayerState`/`SnapshotMessage` (`Protocol.cs:27-42,124-148`). If guest trigger callbacks occur for its kinematic replicas, SFX is generated by a non-authoritative local velocity; if not, host collision SFX has no transfer path and is absent. Either outcome needs runtime observation rather than an assumption about Unity callback behavior.
- 최소 재현: enable instrumentation only in isolated validation, drive host body into water/rock while guest has no local input, record guest callback, velocity and SFX invocation. Expected: host-authoritative impact event drives exactly one local SFX.

## 사건별 표현 전달표

| 사건/지속 상태 | 호스트 확정 지점 | 메시지 필드/사건 | 게스트 소비 지점 | UI/VFX/SFX | 최초/반복/종료 | 누락·로컬 의존 | 증거 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 잡기/놓침 | player/hand authority (B) | `PlayerState.IsLeftGrab/IsRightGrab` | `PlayerController.ApplySnapshot` invokes `HandleGrab` | hand SFX/particle arise from HandController side effects | repetition semantics need B validation | likely state-to-gameplay path, B owns | `PlayerController.cs:361-409`; `HandController.cs:122-203` |
| 획득 | Bridge host collision | count + item active list, no event | inventory/world apply | UI updates; GetItem SFX host-only call | count idempotent; no guest pickup event | guest SFX unproven | D report / `BridgeManager.cs:105-119` |
| 다리 건설 | `BridgeBuild` | count only | `ApplySnapshot` | SFX+particle+activation | increases replay as events | late join/merged count D-P2-01 | `BridgeManager.cs:91-134` |
| 물/돌 충돌 | local Physics callback currently | none | `SoundController` local callback | splash/rock SFX | callback-based | E-P2-03 | `SoundController.cs:16-25` |
| mine knockback | host MineController | player transforms only | player snapshot | no mine impact event found | no explicit lifecycle | VFX/SFX unconfirmed | `MineSystem.cs:27-55`; `Protocol.cs:124-148` |
| 낮밤 지속/전환 | host DayLoop timer | `IsDay` | `DayApplyFromNetwork` → `SetDayType` if changed | UI clock, sprite/light tween | state-change guard prevents repeat transition | guest clock locally progresses; no elapsed value | `DayLoop.cs:58-106,111-150` |
| 승패 | should be host GameManager | active/arrived/bridge state only | guest `ApplySnapshot` re-evaluates | result roots, Directors' SFX/camera/tween | `isGameFinished` prevents repeats | E-P1-01 | `GameManager.cs:77-103,160-166` |
| UI tab/volume | local input/options | none required | UIManager/AudioManager | UI only | local | no gameplay feedback found | `UIManager.cs:10-19`; `AudioManager.cs:108-159` |

## 전달 및 미확인 범위

- D: bridge event is emitted on snapshot and has the late-join/merged-count presentation problem D-P2-01.
- B/C: all player/fish one-shot visuals require separate audit; the current protocol carries transforms plus limited booleans/state names, not generic effect event IDs.
- A: role-unknown windows are material: several `CanRunLocalGameLogic` properties treat `Me == null` as allowed (`GameManager.cs:220-227`, `VictoryZone.cs:37-44`), while `RoomClient` loads scene before welcome (`RoomClient.cs:152-163,339-383`). Whether general gameplay runs in that window requires A's lifecycle audit.
- No scene transition is invoked by GameOverDirector/GameWinDirector; they only play local presentation sequences. Menu/retry wiring and all animation events were not inspected in this D/E slice.
