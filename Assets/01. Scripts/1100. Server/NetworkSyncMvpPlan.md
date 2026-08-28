# 네트워크 동기화 MVP 설계

## 목표

호스트 권한형으로 빠르게 동작을 검증한다.

- 게스트: 입력만 서버에 전송하고, 호스트가 계산한 화면 결과만 수신한다.
- 서버: 게스트 입력을 호스트에 전달하고, 호스트 스냅샷/이벤트를 게스트에 중계한다.
- 호스트: 모든 물리, 충돌, 상태 전이, 게임 규칙을 시뮬레이션한다.

게스트에 세부 상태를 복제하지 않음으로써 물리 및 랜덤 로직의 불일치를 줄인다. 테스트 중 화면상 부자연스러운 부분이 확인될 때만 필요한 데이터를 추가한다.

## 메시지 구분

| 메시지 | 송신 흐름 | 목적 |
| --- | --- | --- |
| `InputCommand` | 게스트 -> 서버 -> 호스트 | 플레이어 입력 전달 |
| `WorldSnapshot` | 호스트 -> 서버 -> 게스트 | 현재 화면에 보이는 월드 결과 표시 |
| `WorldEvent` | 호스트 -> 서버 -> 게스트 | 획득, 다리 건설, 낮밤 변경, 승패 같은 불연속 변화 전달 |

연속적으로 변하는 위치/회전은 매 변경마다 이벤트를 보내지 않고 일정 주기(초기에는 10~20Hz)의 스냅샷으로 보낸다. 이벤트는 확정된 한 번의 변화에만 사용한다.

## 게스트 입력

게스트는 자신의 위치를 보내지 않는다. 아래처럼 호스트가 재현할 수 있는 입력 명령만 보낸다.

```text
InputCommand
- sequence
- moveX, moveY
- leftGrabHeld, rightGrabHeld
- leftGrabPressed, leftGrabReleased
- rightGrabPressed, rightGrabReleased
- buildPressed
```

호스트는 자신의 입력과 게스트 입력을 같은 입력 형식으로 처리하고, 실제 플레이어 물리 객체에 적용한다.

## 플레이어 표시 데이터

플레이어의 속도나 내부 상태를 우선 전송하지 않는다. 호스트가 계산한 보이는 포즈만 보낸다.

```text
PlayerVisualSnapshot
- playerId
- parts[]
  - partId
  - position
  - rotation
  - localScale (실제로 변할 때)
- hunger (게이지가 화면에 표시되는 경우)
```

팔과 관절이 있는 플레이어이므로 몸통 위치 하나만으로는 게스트 화면을 재현할 수 없다. 몸통, 좌/우 상완, 좌/우 하완 등 화면에 보이는 각 부위의 Transform을 보낸다.

게스트는 `PlayerInputController`의 직접 입력 적용과 플레이어 물리 시뮬레이션을 중지한다. 두 스냅샷 사이의 position/rotation을 보간해서 화면에만 표시한다. 따라서 속도를 보내지 않아도 물리 차이로 플레이어가 다르게 보이는 문제를 줄일 수 있다.

## 물고기 표시 데이터

물고기의 FSM 상태, 저항도, 속도, 난수, 충돌 판정은 호스트 내부 로직으로 유지한다. 게스트는 물고기 FSM/이동/충돌 로직을 실행하지 않고 시각 결과만 적용한다.

```text
FishVisualSnapshot
- fishId
- active
- position
- rotation
- localScale
```

- `fishId`: 어떤 물고기에 적용할지 식별한다.
- `active`: 획득/디스폰된 물고기를 화면에서도 끈다.
- `localScale`: 현재 좌우 방향이 `scale.x` 부호로 표현되므로 필요하다.

향후 기절/탈진 등에 따라 별도 애니메이션이 화면에 보이게 되면 그때 `visualState` 하나를 추가한다.

## 게임 승리/패배

게스트에는 호스트의 세부 판정 과정이나 인원 수를 전송하지 않는다.

```text
GameResult
- None
- Won
- Lost
```

호스트는 기존 `GameManager` 로직으로 승패를 확정하고 `GameResultChanged` 이벤트를 보낸다. 게스트는 결과 UI를 표시하고 입력을 막는다.

최신 `WorldSnapshot`에도 `gameResult` 하나를 포함한다. 중간 입장, 재접속, 이벤트 처리 누락 시에도 게스트가 현재 결과 UI를 복구할 수 있기 때문이다.

## 최초 WorldSnapshot 형태

```text
WorldSnapshot
- snapshotSequence 또는 hostTick
- players: PlayerVisualSnapshot[]
- fish: FishVisualSnapshot[]
- gameResult: None | Won | Lost
```

`snapshotSequence` 또는 `hostTick`은 오래된 데이터를 버리고, 게스트가 두 최신 스냅샷 사이를 보간하는 데 사용한다.

## 필수 동작 규칙

1. 게스트는 입력을 보내지만 로컬 물리/게임 규칙 결과를 확정하지 않는다.
2. 호스트만 물리, 충돌, 배고픔 감소, 물고기 FSM, 트리거, 승패를 계산한다.
3. 게스트는 수신한 스냅샷으로 Transform과 UI만 반영한다.
4. 모든 화면상 상태는 안정적인 ID를 사용한다. 플레이어는 서버 `User.Id`, 물고기는 씬/프리팹의 고유 ID를 사용한다.

## 테스트 기준

- 게스트의 키 입력이 게스트 화면의 물리를 즉시 바꾸지 않고, 호스트 스냅샷 수신 후에만 반영된다.
- 같은 물고기가 호스트와 게스트에서 같은 위치, 회전, 방향, 활성 상태로 보인다.
- 호스트가 승리/패배를 확정하면 모든 게스트가 같은 결과 UI를 표시한다.
- 수신 지연이 있어도 스냅샷 보간으로 플레이어와 물고기 움직임이 과도하게 끊기지 않는다.

