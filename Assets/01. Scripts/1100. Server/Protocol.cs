using System.Collections.Generic;

namespace HelloServer
{
    public class User
    {
        public string Id { get; set; }
        public string NickName { get; set; }
        public bool IsHost { get; set; }
    }

    // 받은 글자가 어떤 종류인지 나타내는 데이터 객체
    // 일반적으로 Header라고 부릅니다.
    // 전달받은 데이터의 종류만 먼저 읽고, 알맞은 처리를 합니다.
    public class TypeOnly
    {
        public string Type { get; set; }
    }

    // 위치 상태를 나타내는 데이터 객체 한줄
    // 강사님 코드는 비활성화
    public class PlayerState
    {
        public string Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }
    
    // 물고기 코드
    public class FishState
    {
        public string Id { get; set; }       // 물고기 고유 식별 번호 (풀에서 생성 시 부여)
        public float X { get; set; }          // 현재 위치 X
        public float Y { get; set; }          // 현재 위치 Y
        public float ScaleX { get; set; }     // 좌우 반전 스케일값 (ApplyingFacing 결과 동기화용)
        public float Angle { get; set; }      // 머리 회전 각도 (발버둥 칠 때 회전값 동기화용)
        public string CurrentState { get; set; }     // 현재 상태 이름 (Idle, Caught, Faint, Runaway 등)
    }
    

    #region 클라이언트 -> 서버 (C2S)

    public class HelloMessage
    {
        public string Type { get; set; } = "hello";
        public string NickName { get; set; }
    }

    public class MoveMessage
    {
        public string Type { get; set; } = "move";
        public string Id { get; set; }
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class GuestInputMessage
    {
        public string Type { get; set; } = "input";
        public string Id { get; set; }

        public float X { get; set; }
        public float Y { get; set; }

        public bool IsLeftShiftHold { get; set; }
        public bool IsRightShiftHold { get; set; }
    }

    public class GuestInputGroupMessage
    {
        public string Type { get; set; } = "inputGroup";

        public List<GuestInputMessage> Inputs { get; set; } = new();
    }

    // Host가 만든 Physics 상태를 Guest에게 전달할 때 사용하는 메시지.
    public class SnapshotMessage
    {
        public string Type { get; set; } = "snapshot";

        // 아직 정해지지 않은 Physics 데이터는 그대로 전달한다.
        public string testData { get; set; } = "testsnapshot";
        
        
    }

    public class StateMessage
    {
        public string Type { get; set; } = "state";
        public string Id { get; set; }
        public string TestMessage { get; set; }
        // 호스트의 유의미한 모든 정보
        // 플레이어 (위치, 무력화 상태)
        public PlayerState[] Players { get; set; }
        // 물고기 (위치, 상태)
        public FishState[] Fishes { get; set; } 
        // 공용 인벤토리 내 자원량
        // 다리 
        // 시간 
        // 게임 오버 척도, 게임 승리 척도
    }

    #endregion

    #region 서버 -> 클라이언트 (S2C)

    public class WelcomeMessage
    {
        public string Type { get; set; } = "welcome";

        public string RoomCode { get; set; }

        public User User { get; set; }
        public User[] Users { get; set; }
    }

    public class JoinMessage
    {
        public string Type { get; set; } = "join";
        public User User { get; set; }
    }

    public class LeaveMessage
    {
        public string Type { get; set; } = "leave";
        public string Id { get; set; }
    }

    public class ChatMessage
    {
        public string Type { get; set; } = "chat";
        public string Id { get; set; }
        public string NickName { get; set; }
        public string Text { get; set; }
    }

    // 강사님 코드 비활성화
    // public class StateMessage
    // {
    //     public string Type { get; set; } = "state";
    //     public PlayerState[] States { get; set; }
    // }

    #endregion
        
}
