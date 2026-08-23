 
# 물고기 시스템 설계 

물고기에게 필요한 기능
 기본 이동: 물고기가 정지하지 않고 일정 지점을 왕복하여 이동
 플레이어가 손으로 물고기에 닿았을 때 이동값(patrol) 을 초기화 시키고 플레이어에 움직임에 따라
 움직여야 한다.
        // 물고기의 기본 스탯은 Data Transfer Object 로 주고 받는 구조로 만들 예정 
        
        // 상태 머신 ?!
        // 물고기의 idle, patrol, runaway ,knockdown 4가지 로 나눠야 한다. + 잡힌 상태 Caught
        
        // idle : 물고기가 patrol 하기 전 대기 상태
        // patrol : 물고기의 순찰 상태
        // runaway : 플레이어가 물고기 근처에 와 잡지 못하면 도망치는 상태
        // knockdwon : 물고기의 저항력이 완전히 소모된 후에 기절상태
        // CaughtState : 물고
         고려해야 할 것: 멀티 플레이 기준이기 때문에 물고기 자체의 위치값을 public 으로 지정해야 할듯 


```csharp
FishStateManager (MonoBehaviour)
├─ IFishState 인터페이스
│    ├─ FishIdleState
│    ├─ FishPatrolState
│    ├─ FishCaughtState
│    ├─ FishRunawayState
│    └─ FishKnockdownState
├─ FishMovement (이동/애니메이션 실행 담당)
└─ FishStatData (DTO, 저항도/속도 등 수치)
```


## 역할 분리 기준:

FishStateManager: 상태 전이만 담당. "언제 바뀌는가"
FishMovement: 실제 Transform 이동/DOTween 등 실행. "어떻게 움직이는가"
FishStatData: 순수 데이터. 로직 없음


## 3. 상태별 수행 요소 (완료 조건 기준)

### FishIdleState

진입 시: 일정 시간(예: 1~2초) 대기 타이머 시작
종료 조건: 타이머 만료 → Patrol로 전이
완료 기준: 대기 중엔 Transform이 움직이지 않아야 함

### FishPatrolState

진입 시: 순찰 포인트 목록 중 다음 목표점 설정
매 프레임: 목표점 방향으로 이동, 도착 판정
도착 시: 다음 포인트로 갱신 
전이 조건: 플레이어 손 콜라이더와 접촉 → Caught
완료 기준: 정지 없이 계속 왕복,

### FishCaughtState (신규)

진입 시: 저항도를 최대치로 세팅, 물고기를 손 Transform에 종속(또는 위치 추적)
매 프레임: 저항도를 서서히 감소시키는 타이머 진행 (플렝이어가 잡고 있는 조건이 만족 될 때만)
특수 이벤트: 벽/지형 충돌 감지 시 저항도 큰 폭 차감 (OnCollisionEnter 등)
저항도가 일정 수치 이상이면 플레이어 이동에 저항력을 가하는 값을 FishMovement 쪽에 전달 (플레이어 컨트롤러가 참조할 수 있게)
전이 조건:
저항도 0 도달 → Knockdown
손에서 벗어남(그립 해제) → Runaway (탈출 시점의 저항도값을 Runaway 상태에 넘겨줘야 즉시/지연 분기 가능)
완료 기준: 저항도가 프레임마다 감소하는 게 눈에 보여야 하고, 벽 충돌 시 감소폭이 확실히 커야 함

### FishRunawayState

진입 시: Caught에서 넘겨받은 저항도 값으로 분기
저항도 높음 → 즉시 도주 벡터 계산 후 빠르게 이동 시작
저항도 낮음 → 짧은 지연 타이머 후 도주 시작
전이 조건: 일정 거리/시간 이상 도망치면 → Idle (또는 바로 Patrol)
완료 기준: 두 케이스(즉시/지연)가 실제로 다르게 동작해야 함

### FishKnockdownState

진입 시: 이동 정지, 포획 확정 이벤트 발생 (인벤토리 시스템에 전달할 훅)
완료 기준: 이 상태 진입 후 플레이어가 다시 자원처럼 옮길 수 있는 상태가 됨 (Rigidbody kinematic 처리 등 고려)