# D — 아이템·인벤토리·다리·채굴 정적 감사

## 조사 범위와 연결

기준 커밋은 `6c54ff6`이며, 이 작업 트리에는 조사 산출물 `docs/`만 미추적 상태였다. Unity 실행·빌드·테스트는 하지 않았다. 실제 시작 씬 `Assets/00. Scenes/InGame.unity`에는 `WorldItemRegistry`와 `WorldItemSnapshotSynchronizer`가 활성 컴포넌트로 배치되어 있다 (`InGame.unity:6397-6420`), BridgeManager도 씬 오브젝트로 배치되어 있다 (`InGame.unity:10660-10676`).

호스트 경로는 `RoomClient.SendSnapshotOrInputMessage`가 월드 아이템, 인벤토리, 다리 수를 `SnapshotMessage`에 생산하는 것으로 끝난다 (`Assets/01. Scripts/1100. Server/RoomClient.cs:525-595`). 게스트 수신은 `OnSnapshot`에서 다리 → 월드 → 게임 결과 → 인벤토리 순으로 적용한다 (`RoomClient.cs:255-290`). `SnapshotMessage`에는 plank의 위치/각도/수거 여부와 inventory count, bridge count만 있고 획득·소비·건설의 사건 번호는 없다 (`Protocol.cs:76-90,124-148`).

## 권한/월드 생명주기 커버리지

| 기능/컴포넌트 | 씬·생성 경로 | 진입점·변경 | 호스트 근거 | 게스트 차단/복제 근거 | 표현 전달 | 판정 | 증거/제약 |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 물고기·아이템의 다리 반입 | 활성 Bridge collider | `OnCollisionEnter2D` → `HandleItemPickup` → `InventoryItemDB.Add`/`WorldItem.OnPickup` | 충돌 함수 첫 줄이 `Me.IsHost` 검사 (`Bridge.cs:10-34`); 호스트만 snapshot 생산 | 게스트는 Bridge 충돌에서 즉시 return | 활성 목록에서 사라진 world item을 게스트가 비활성/풀 반환 (`WorldItemSnapshotSynchronizer.cs:141-146,184-190`) | 적합(정적) | `HandleItemPickup` 자체에는 권한 검사가 없으나 모든 제품 호출자는 Bridge의 호스트 제한 경로뿐이다 (`BridgeManager.cs:105-119`; `rg` 호출자 확인). |
| 비용 계산·소비·건설 | Bridge collider + 호스트 원격 플레이어 입력 | `OnCollisionStay2D` → `BridgeBuild` → consume plan → `TryConsumeAll` → count/event | Bridge의 호스트 검사 (`Bridge.cs:37-47`); host가 guest input을 remote에 적용 (`RoomClient.cs:293-315`) | 게스트는 입력만 전송하고 snapshot count를 `SetCount`로 복제 (`RoomClient.cs:597-621`, `InventoryItemDB.cs:106-144`) | bridge count 증가 수신 시 coroutine이 SFX/VFX 후 bridge를 활성화 (`BridgeManager.cs:91-134`) | 적합(권한), 표현 후보 | `TryConsumeAll`에 내부 권한 검사가 없으나 제품 진입점은 위 호스트 collision 하나로 확인. ContextMenu/debug 호출은 별도 미사용 확인 필요. |
| 인벤토리 UI | InGame UI/Inventory (직렬화된 구독은 런타임 `Start`) | `ItemCountChanged` → `Inventory.RenderItem`; 요구 UI는 count와 build value를 읽음 | host가 count를 변경 | snapshot `ApplySnapshot`은 값이 다를 때만 `ItemCountChanged` | slot/요구량/누적 가치 렌더만 수행 | 적합(정적) | `Inventory.cs:22-31,134-150`, `BridgeRequirementUI.cs:24-38,53-123`; UI가 `TryConsume`/`BridgeBuild`를 호출하지 않는다. |
| 고정 plank | `WorldItemRegistry.Awake`가 `Plank Group` 재귀 등록 | snapshot `ApplyPlanks`가 transform/`IsPickUp` 적용, 누락 시 deactivate | host pack은 활성 fixed plank만 생산 | guest 시작 시 고정 plank RB를 Kinematic; apply는 보간/활성 상태만 바꿈 | 위치 보간·가시성 | 적합(정적) | `WorldItemRegistry.cs:17-21,70-104`; `WorldItemSnapshotSynchronizer.cs:14-25,87-97,150-191`; 단, 자식 collider의 모든 상호작용은 런타임 확인 필요. |
| 동적 fish item 반환 | fish pool → registry → host snapshot | 수신 목록에서 빠진 ID는 registry 제거 후 pool release | host active item pack | guest release는 host 목록 부재에 따른 표시 객체 생명주기 | 없음(객체 소멸 표시) | 적합(정적) | `WorldItemSnapshotSynchronizer.cs:99-147`; 물고기 FSM/풀 안전성은 C 담당 범위. |
| MineController 넉백 | `6. MineFish.prefab` 및 `Map/Mine.prefab`에 컴포넌트 존재 | collision → 방향 계산 → player RB velocity | `IsHost`는 `RoomClient.Me.IsHost`를 모두 요구하고 callback 첫 분기에서 return | 게스트는 force/velocity를 쓰지 않음 | player snapshot 위치로 결과 표시 | 적합(정적), 표현 미확인 | `MineSystem.cs:22-55`; MineFish prefab은 fish spawning 경로로 사용될 수 있으나 scene 외 `Map/Mine.prefab`의 실제 연결은 확인 못 함. 넉백/폭발 SFX·VFX 사건 필드는 protocol에 없다. |

## 후보

### D-P2-01 / 늦은 입장 또는 여러 단계 수신에서 다리 건설 연출을 과거 사건처럼 재생

- 분류: 표현 누락/중복
- 확인 수준: 정적으로 확정(연출 시작 조건), 실제 사용자 영향은 실행 확인 필요
- 우선순위: P2
- 실행 환경 / 씬·프리팹 / 역할: `InGame`; 게스트가 첫 snapshot에서 현재 bridge count보다 작은 로컬 count를 갖거나 한 snapshot에서 둘 이상 증가를 수신.
- 호출 사슬: host `BridgeBuild` → `currentBridgeCount++`/snapshot `CurrentBridgeCount` (`BridgeManager.cs:63-89`, `RoomClient.cs:589`) → guest `OnSnapshot` → `BridgeManager.ApplySnapshot` (`RoomClient.cs:271-281`) → 증가한 각 index마다 `BuildParticle` → bridge build SFX, shared particle active, 2초 뒤 bridge active (`BridgeManager.cs:91-134`).
- 게스트에서 실행/전달되지 않는 것: 게스트가 비용을 재소비하지는 않지만, 현재 상태 count만으로 과거 건설을 새 사건으로 해석한다. `BuildParticle`은 단일 `buildParticle` 인스턴스를 동시에 재배치/활성화하므로 여러 index가 한 수신에 합쳐지면 각 연출의 독립성도 보장하지 않는다.
- 관련 호스트 데이터: `CurrentBridgeCount`만 전달되고 build event/sequence는 없다 (`Protocol.cs:124-148`).
- 확인한 반증: 같은 또는 감소 count는 coroutine을 시작하지 않고 (`BridgeManager.cs:93-102`), host collision/비용 처리로 재진입하지 않는다. 따라서 권한 침범이 아니라 one-shot 표현 문제다.
- 최소 재현: 이미 여러 다리가 완성된 방에 게스트를 입장시키고 첫 snapshot을 관찰; 이어 host가 snapshot 간 두 개를 완성하도록 만든다. 기대: 초기 상태는 즉시 복원하고 새 각 건설만 정확히 한 번 연출. 실제 관측은 미실행.
- 수정 방향: 상태 복원과 신규 build event를 구분할 수 있도록 snapshot 수신 초기화/사건 식별을 설계한다. 구현하지 않음.

## D→E 전달: 자원/연출 표

| 사건/지속 상태 | 호스트 확정 | 전송 값 | 게스트 소비 | 현재 UI/VFX/SFX | 최초/반복/종료 | 판정 |
| --- | --- | --- | --- | --- | --- | --- |
| 아이템 획득·소비 | `Bridge.HandleItemPickup`/`BridgeBuild` | inventory entries와 active plank/fish 목록 | `ApplySnapshot` → `InventoryItemDB.SetCount`; object deactivate | inventory/requirement UI 갱신. GetItem SFX는 host `HandleItemPickup`에서만 호출 | count 변경은 중복 수신에 idempotent; 획득 SFX 전달 필드 없음 | UI/월드 상태 적합, 게스트 획득 SFX 누락 가능(실행 확인) |
| 다리 건설 | `BridgeBuild` | `CurrentBridgeCount` | `ApplySnapshot` → `BuildParticle` | SFX, particle, 2초 후 active | 증가만 재생; 늦은 입장/합쳐진 증가 문제는 D-P2-01 | 후보 |
| Mine 넉백 | `MineController` collision | player part transform뿐 | `PlayerController.ApplySnapshot` (B) | 별도 mine knockback/impact event 없음 | 상태값만 반복 보간 | SFX/VFX 미확인 |

## 미확인/다른 담당 전달

- `WorldItemSnapshotSynchronizer`가 guest fish를 `SetActive(true)` 한 뒤 `LockFishForGuest`를 호출한다 (`WorldItemSnapshotSynchronizer.cs:132-136`). 활성화 중 발생하는 물고기 FSM·child physics는 C 담당에서 확인 필요하다.
- `BridgeManager.OnBridgeBuild`는 게스트 snapshot에서도 발생한다 (`BridgeManager.cs:91-102`). E 담당은 GameManager와 UI 구독자가 이를 게임 규칙으로 다시 해석하지 않는지 확인해야 한다.
- `MineFishController`는 guest visual mode에서 child RB simulation과 joint를 끈다 (`MineFishController.cs:88-97`), 그러나 root fish snapshot에는 mine child pose나 detonated/impact event가 없다. C/E가 prefab과 실제 physics 표현을 검증해야 한다.
