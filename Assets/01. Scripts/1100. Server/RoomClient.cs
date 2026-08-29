using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine.InputSystem;
using WeGotOne;

namespace HelloServer
{
    
    // RoomClient가 서버와의 네트워크 작업을 담당한다.
    // 송신, 수신 등을 모두 처리하는 객체
    // 씬에 종속적이지 않음
    public class RoomClient : SingletonBase<RoomClient>
    {
        [Header("서버")]
        // 웹소켓 주소. http 가 아니라 ws로 시작.
        [SerializeField] private string url = "ws://localhost:5000/room";
        // 서버에 알릴 내 이름.
        [SerializeField] private string nickName = "이름없음";
        // 들어갈 방.
        [SerializeField] private string roomCode = "ABCD";
        
        [Header("장면")]
        // 내가 조종하는 캐릭터. 씬에 미리 놓아 둔다.
        // 8/28 수정 : 예시 플레이어인 LocalPlayer를 제거하고 프로젝트에서 사용하는 플레이어 컨트롤러로 변경
        // 남이 들어올 때마다 이걸로 하나씩 만든다.
        [SerializeField] private PlayerController remotePrefab;
        private PlayerController localPlayer;

        [Header("보내기")]
        // 초당 몇 번 내 위치를 보낼지.
        // 서버가 뿌리는 횟수와는 별개. (그쪽은 appsettings.json)
        // 보내는 쪽이 더 자주 보내도 되지만, 받는 쪽이 그만큼 자주 보게 되지는 않는다.
        [SerializeField] private float sendPerSecond = 20f;

        [SerializeField] private Transform spawnPoint;
        // 서버가 정해 준 내 정보. welcome 을 받기 전에는 비어 있다.
        public User Me { get; private set; }
        
        // 화면에 그리는 일은 RoomLobby 가 한다. 여기는 선을 타는 일만 한다.
        public int RemoteCount => remotes.Count;
        public PlayerController Local => localPlayer;

        public GuestInputGroupMessage GuestInputGroupMessage { get; private set; }
        public GuestInputMessage GuestInputMessage { get; private set; }
        public SnapshotMessage SnapshotMessage { get; private set; }
        
        private ClientWebSocket socket;
        
        // 남들. 번호로 찾는다.
        // private readonly Dictionary<string, RemotePlayer> remotes = new Dictionary<string, RemotePlayer>();
        
        // SSG 커스텀 : 플레이어 맞춰서 편집
        private readonly Dictionary<string, PlayerController> remotes = new Dictionary<string, PlayerController>();

        // 오간 말. 너무 쌓이지 않게 앞에서부터 버린다.
        private readonly List<string> chatLog = new List<string>();

        // 채팅 정보 필드 폐기
        // private const int ChatKeep = 12;

        // 한 소켓에 동시에 두 번 보내면 안 된다.
        private readonly SemaphoreSlim sendLock = new SemaphoreSlim(1, 1);
        private ItemObjectPool itemPool;

        #region unity method

        protected override void OnInitialize()
        {
            base.OnInitialize();
            // true로 해둬야 백그라운드에서도 데이터 수신 가능
            Application.runInBackground = true;
            itemPool = FindFirstObjectByType<ItemObjectPool>();
        }

        private void OnDestroy()
        {
            socket?.Dispose();
        }

        #endregion

        public void Join(string code, string who)
        {
            if (IsBusy) return;

            roomCode = code;
            nickName = who;

            RunAsync().Forget();
        }

        public bool IsBusy { get; private set; }

        // 서버가 다듬어 알려 준 방 코드. abcd 로 쳐도 ABCD 가 옴.
        public string RoomCode { get; private set; }

        // 방에서 오간 말. 로비가 그대로 그린다.
        public IReadOnlyList<string> ChatLog => chatLog;

        // 접속하고, 이름을 알리고, 끊길 때까지 듣는다.
        private async UniTaskVoid RunAsync()
        {
            IsBusy = true;

            socket = new ClientWebSocket();

            // 방 코드는 주소에 실어 보낸다.  ws://localhost:5000/room?code=ABCD
            string full = $"{url}?code={UnityWebRequest.EscapeURL(roomCode)}";

            try
            {
                // socket을 통해 해당 주소로 접속한다. UnitTask를 사용한다
                await socket.ConnectAsync(new Uri(full), destroyCancellationToken).AsUniTask();
            }
            catch (Exception e)
            {
                // 서버가 안 켜져 있는 것은 오류가 아니라 흔한 일이다. 수업 중에 반드시 일어난다.
                Debug.LogWarning($"[방] 접속하지 못했다. 서버가 켜져 있는지, 방 코드가 맞는지 확인 : {e.Message}");

                IsBusy = false;
                return;
            }

            Debug.Log($"[방] {full} 에 접속.  : {nickName}");
            // 첫 마디는 반드시 hello 다. 서버가 이걸 받아야 번호를 줌.
            await SendAsync(new HelloMessage { NickName = nickName });
            // 보내기는 따로 돈다. 듣는 동안에도 계속 보내야함.
            SendLoop().Forget();
            await ReceiveLoopAsync();

            // 여기까지 오면 끊긴 것.
            IsBusy = false;
            Me = null;
        }

        #region 듣기

        // 끊길 때까지 계속 듣는다.
        private async UniTask ReceiveLoopAsync()
        {
            byte[] buffer = new byte[4096];
            StringBuilder builder = new StringBuilder();

            while (socket.State == WebSocketState.Open)
            {
                WebSocketReceiveResult result;

                try
                {
                    // .AsUniTask() 를 붙이는 이유
                    // : 이 await 뒤에서 유니티 오브젝트를 만진다.
                    //  붙여 두면 본 스레드로 돌아와서 이어진다. 안 붙이면 다른 스레드일 수 있고,
                    //  거기서 transform 을 만지는 순간 유니티가 예외를 던진다.
                    result = await socket
                        .ReceiveAsync(new ArraySegment<byte>(buffer), destroyCancellationToken)
                        .AsUniTask();
                    // 호스트가 게스트 입력 받아오거나
                    // 게스트가 호스트 스냅샷 받아오거나
                    // 하는 시점을 Update 이전으로 고정
                    await UniTask.Yield(PlayerLoopTiming.Update);
                }
                catch (OperationCanceledException)
                {
                    // 이 오브젝트가 사라졌다
                    return;   
                }
                catch (WebSocketException)
                {
                    Debug.LogWarning("[방] 연결이 끊겼다");
                    return;
                }

                if (result.MessageType == WebSocketMessageType.Close) return;

                builder.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));

                // 긴 글자는 여러 조각으로 나뉘어 온다. 끝 조각이 올 때까지 이어 붙인다.
                if (result.EndOfMessage == false) continue;

                string json = builder.ToString();
                builder.Clear();

                Handle(json);
            }
        }

        // 받은 글자 하나를 종류에 따라 나눠 준다.
        private void Handle(string json)
        {
            // JsonUtility => Newtonsfot.Json의 Json Utility로 변경됨.
            
            string type = JsonConvert.DeserializeObject<TypeOnly>(json).Type;

            switch (type)
            {
                case "welcome": OnWelcome(JsonConvert.DeserializeObject<WelcomeMessage>(json)); break;
                case "join": OnJoin(JsonConvert.DeserializeObject<JoinMessage>(json)); break;
                case "leave": OnLeave(JsonConvert.DeserializeObject<LeaveMessage>(json)); break;
                case "inputGroup": OnGuestInputGroup(JsonConvert.DeserializeObject<GuestInputGroupMessage>(json)); break;
                case "snapshot": OnSnapshot(JsonConvert.DeserializeObject<SnapshotMessage>(json)); break;
                default: Debug.Log($"[방] 등록되지 않은 종류(type) : {type}"); break;
            }
        }

        private void OnSnapshot(SnapshotMessage message)
        {
            if (Me.IsHost)
            {
                SnapshotMessage = message;
                return;
            }
            
            foreach (PlayerState state in message.Players)
            {
                // snapshot에 자기자신도 있어서 제거
                // if (state.Id == Me.Id)
                // {
                //     localPlayer.ApplyPosition(state, state.IsLeftGrab, state.IsRightGrab);
                // }
                if (remotes.TryGetValue(state.Id, out PlayerController remote))
                {
                    remote.ApplySnapshot(state, state.IsLeftGrab, state.IsRightGrab);
                }
            }
            
            // 임시 비활성화 (json 형태로 로그 출력으로 대체)
            // [추가] 수신된 스냅샷의 전체 현황을 한눈에 볼 수 있도록 로그를 찍어줍니다.
            // int playerCount = message.Players != null ? message.Players.Length : 0;
            // int fishCount = message.Fishes != null ? message.Fishes.Length : 0;
            // int inventoryKindCount = message.ItemKeys != null ? message.ItemKeys.Length : 0;
            //
            // Debug.Log($"[방] 스냅샷 수신! " +
            //           $"플레이어: {playerCount}명 | " +
            //           $"물고기(아이템): {fishCount}개 | " +
            //           $"인벤토리 등록: {inventoryKindCount}종류 | " +
            //           $"다리 개수: {message.CurrentBridgeCount}개 | " +
            //           $"경과 시간: {message.ElapsedTime:F1}초 " +
            //           $"(GameOver: {message.IsGameOver}, GameWon: {message.IsGameWon})");
            Debug.Log(GetJson(message));
            
            SnapshotMessage = message;
        }

        private void OnGuestInputGroup(GuestInputGroupMessage message)
        {
            if (Me.IsHost == false)
            {
                Debug.LogWarning("[방] 게스트가 게스트 입력 모음을 받았다. 서버 코드 문제");
                return;
            }

            List<GuestInputMessage> inputs = message.Inputs;
            for (int i = 0; i < inputs.Count; i++)
            {
                GuestInputMessage input = inputs[i];
                if (input == null)
                {
                    continue;
                }
                
                if (remotes.TryGetValue(input.Id, out PlayerController remote) == false) continue;
                remote.ApplyInput(new Vector2(input.X, input.Y), input.IsLeftShiftHold, input.IsRightShiftHold);
            }
            
            GuestInputGroupMessage = message;
        }

        private void OnWelcome(WelcomeMessage message)
        {
            Me = message.User;
            RoomCode = message.RoomCode;

            // 자기 자신 포함 모두 스폰
            // 자기 자신은 입력값을 전달해줘야함
            // 입력값 전달은 호스트 여부에 따라 달라짐
            // 호스트이면 즉시 반영
            // 게스트이면 호스트 스냅샷만 반영 (입력값 비활성화)
            Spawn(message.User);
            foreach (User user in message.Users) Spawn(user);
            
            remotes.TryGetValue(Me.Id, out PlayerController localPlayer);
            this.localPlayer = localPlayer;

            localPlayer.Setup(Me);
        }

        private void OnJoin(JoinMessage message)
        {
            Spawn(message.User);
            Debug.Log($"[방] {message.User.NickName}({message.User.Id}) 이(가) 들어왔다");
        }

        private void OnLeave(LeaveMessage message)
        {
            if (remotes.TryGetValue(message.Id, out PlayerController remote) == false) return;

            remotes.Remove(message.Id);
            Destroy(remote.gameObject);

            Debug.Log($"[방] {message.Id} 이(가) 나갔다");
        }

        private void Spawn(User user)
        {
            // 이미 있으면 만들지 않는다. welcome 과 join 이 겹칠 수 있다.
            if (remotes.ContainsKey(user.Id)) return;

            PlayerController remote = Instantiate(remotePrefab, spawnPoint.position, Quaternion.identity);
            
            Rigidbody2D[] rigid = remote.GetComponentsInChildren<Rigidbody2D>();

            for (int i = 0; i < rigid.Length; i++)
            {
                if(Me.IsHost == false)
                    rigid[i].bodyType = RigidbodyType2D.Kinematic;
            }

            remote.Setup(user);

            remotes[user.Id] = remote;
        }

        #endregion

        #region 보내기

        private async UniTaskVoid SendLoop()
        {
            int delay = (int)(1000f / sendPerSecond);

            while (destroyCancellationToken.IsCancellationRequested == false)
            {
                // Update, FixedUpdate 이후 (게임 로직 처리 이후) 
                // 호스트는 스냅샷
                // 게스트는 게스트 인풋 전송
                // WhenAll로하면 Delay가 나중에 완료되어서 PostLateUpdate 시점에 실행될거라 보장할 수 없음
                await UniTask.Delay(delay, cancellationToken: destroyCancellationToken);
                await UniTask.Yield(PlayerLoopTiming.LastUpdate);

                // 아직 번호를 못 받았으면 보낼 것이 없다.
                if (Me == null) continue;

                // 호스트일때는 호스트 스냅샷
                await SendSnapshotOrInputMessage();
            }
        }

        private async UniTask SendSnapshotOrInputMessage()
        {
            if (Me.IsHost)
            {
                // 1. 스냅샷 객체 생성
                var snapshot = new SnapshotMessage();

                // 2. [물고기/아이템 연동]: 씬의 ItemObjectPool에서 활성화된 아이템들을 가져와 배열로 변환
                if (itemPool != null)
                {
                    snapshot.Fishes = itemPool.PackActiveFishes().ToArray();
                    snapshot.Planks = itemPool.PackActivePlanks().ToArray();
                }

                // 3. [플레이어 상태 연동]: 나와 다른 게스트 플레이어들의 상태 채우기
                var playerStates = new List<PlayerState>();
        
                // remotes에 자기 자신도 있어서 제거
                // AddPlayerState(playerStates, Me.Id);

                // 접속한 다른 원격 유저(게스트)들의 현재 씬 위치 추가
                foreach (var pair in remotes)
                {
                    if (pair.Value != null)
                    {
                        AddPlayerState(playerStates, pair.Key);
                    }
                }
                snapshot.Players = playerStates.ToArray();
                
                // 4. [기타 월드 상태]: 인벤토리 자원 및 건설 현황 등 (구현된 시스템에 맞춰 연결)
                // snapshot.ItemKeys = InventoryManager.Instance.GetKeys();
                // snapshot.ItemCounts = InventoryManager.Instance.GetCounts();
                // snapshot.CurrentBridgeCount = BridgeManager.Instance.BridgeObjects.Length;

                // 5. 서버로 패킷 전송
                SnapshotMessage = snapshot;
                await SendAsync(snapshot);
            }
            else
            {
                // 게스트는 자신의 키보드 입력을 전송
                Vector2 input = SimpleInput.GetMoveAxisRaw();
                Keyboard keyboard = Keyboard.current;
                await SendInputMessages(input, keyboard.leftShiftKey.isPressed, keyboard.rightShiftKey.isPressed, keyboard.fKey.isPressed);
            }
        }


        private async UniTask SendInputMessages(Vector2 input, bool leftGrab, bool rightGrab, bool buildKey)
        {
            // 플레이어 스스로의 입력 넣기
            GuestInputMessage = new GuestInputMessage
            {
                Id = Me.Id,
                X = input.x,
                Y = input.y,
                IsLeftShiftHold = leftGrab,
                IsRightShiftHold = rightGrab,
                IsBuildKeyHold = buildKey,
            };
            await SendAsync(GuestInputMessage);
        }

        public void Say(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return;

            SendAsync(new ChatMessage { Text = text.Trim() }).Forget();
        }

        // 한 마디를 보낸다. 보내는 곳이 둘(SendLoop, Say)이라 lock을 걸어서 한 번에 하나씩 내보낸다.
        private async UniTask SendAsync(object message)
        {
            if (socket == null || socket.State != WebSocketState.Open) return;

            // JsonUtility => Newtonsfot.Json의 Json Utility로 변경됨.
            byte[] bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));
            // 아래 라인은 바꾸기 전 코드
            //byte[] bytes = Encoding.UTF8.GetBytes(JsonUtility.ToJson(message));

            await sendLock.WaitAsync();

            try
            {
                await socket
                    .SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, destroyCancellationToken)
                    .AsUniTask();
            }
            catch (OperationCanceledException)
            {
                // 이 오브젝트가 사라졌다.
            }
            catch (WebSocketException)
            {
                // 보내는 순간 끊겼다. 듣는 쪽 루프가 알아서 빠져나온다.
            }
            finally
            {
                sendLock.Release();
            }
        }

        #endregion

        private static Color ColorOf(string id)
        {
            int hash = 0;

            foreach (char c in id) hash = hash * 31 + c;

            return Color.HSVToRGB(Mathf.Abs(hash % 360) / 360f, 0.65f, 0.95f);
        }
        
        private void AddPlayerState(List<PlayerState> playerStates, string playerId)
        {
            // 내(호스트) 위치 정보 추가
            playerStates.Add(new PlayerState 
            { 
                Id = playerId, 
                Body = new PartState
                {
                    X = remotes[playerId].Body.transform.position.x,
                    Y = remotes[playerId].Body.transform.position.y,
                    Rotation = new Rotation()
                    {
                        X = remotes[playerId].Body.transform.rotation.x,
                        Y = remotes[playerId].Body.transform.rotation.y,
                        Z =  remotes[playerId].Body.transform.rotation.z,
                        W =  remotes[playerId].Body.transform.rotation.w,
                    }
                },
                LeftArmTop = new PartState
                {
                    X = remotes[playerId].LeftUpperArm.transform.position.x,
                    Y = remotes[playerId].LeftUpperArm.transform.position.y,
                    Rotation = new Rotation()
                    {
                        X = remotes[playerId].LeftUpperArm.transform.rotation.x,
                        Y = remotes[playerId].LeftUpperArm.transform.rotation.y,
                        Z =  remotes[playerId].LeftUpperArm.transform.rotation.z,
                        W =  remotes[playerId].LeftUpperArm.transform.rotation.w,
                    }

                },
                RightArmTop = new PartState
                {
                    X = remotes[playerId].RightUpperArm.transform.position.x,
                    Y = remotes[playerId].RightUpperArm.transform.position.y,
                    Rotation = new Rotation()
                    {
                        X = remotes[playerId].RightUpperArm.transform.rotation.x,
                        Y = remotes[playerId].RightUpperArm.transform.rotation.y,
                        Z =  remotes[playerId].RightUpperArm.transform.rotation.z,
                        W =  remotes[playerId].RightUpperArm.transform.rotation.w,
                    }

                },
                LeftArmBottom = new PartState
                {
                    X = remotes[playerId].LeftBottomArm.transform.position.x,
                    Y = remotes[playerId].LeftBottomArm.transform.position.y,
                    Rotation = new Rotation()
                    {
                        X = remotes[playerId].LeftBottomArm.transform.rotation.x,
                        Y = remotes[playerId].LeftBottomArm.transform.rotation.y,
                        Z =  remotes[playerId].LeftBottomArm.transform.rotation.z,
                        W =  remotes[playerId].LeftBottomArm.transform.rotation.w,
                    }
                },
                RightArmBottom = new PartState
                {
                    X = remotes[playerId].RightBottomArm.transform.position.x,
                    Y = remotes[playerId].RightBottomArm.transform.position.y,
                    Rotation = new Rotation()
                    {
                        X = remotes[playerId].RightBottomArm.transform.rotation.x,
                        Y = remotes[playerId].RightBottomArm.transform.rotation.y,
                        Z =  remotes[playerId].RightBottomArm.transform.rotation.z,
                        W =  remotes[playerId].RightBottomArm.transform.rotation.w,
                    }
                },
                IsLeftGrab = remotes[playerId].IsLeftGrab,
                IsRightGrab = remotes[playerId].IsRightGrab,
            });
        }

        private string GetJson(object message)
        {
            return JsonConvert.SerializeObject(message, Formatting.Indented);       
        }
        
        [ContextMenu("Debug Snapshot")]
        private void DebugSnapshot()
        {
            Debug.Log(
                JsonConvert.SerializeObject(
                    SnapshotMessage,
                    Formatting.Indented
                )
            );
        }

        [ContextMenu("Debug InputGroup")]
        private void DebugInputGroup()
        {
            Debug.Log(
                JsonConvert.SerializeObject(
                    GuestInputGroupMessage,
                    Formatting.Indented
            ));
        }

        [ContextMenu("Debug GuestInput")]
        private void DebugGuestInput()
        {
            Debug.Log(
                JsonConvert.SerializeObject(
                    GuestInputMessage,
                    Formatting.Indented
                )
            );       
        }
    }
}
