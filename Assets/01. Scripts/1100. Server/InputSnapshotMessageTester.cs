using System.Collections.Generic;
using UnityEngine;

namespace HelloServer  
{
    public class InputSnapshotMessageTester : MonoBehaviour
    {
        // 1. [보완] 유니티 인스펙터에서 물고기 데이터를 쉽게 편집할 수 있도록 헬퍼 구조체 선언
        [System.Serializable]
        public struct TestFishState
        {
            public string id;              // 예: "Slot_0", "Slot_1"
            public int fishTypeIndex;      // 물고기 종류 번호
            public float x;                // 가상 X 위치
            public float y;                // 가상 Y 위치
            public float scaleX;           // 좌우 대칭 (1 or -1)
            public float angle;            // 회전 각도
            public string currentState;    // 상태 (Active, Idle, Caught 등)
        }
        
        // 2. [보완] 인스펙터에서 마우스 클릭으로 편집 가능한 물고기 리스트
        [SerializeField] private List<TestFishState> testFishes = new List<TestFishState>();
        
        [Header("Snapshot Message Test Data")]
        [SerializeField] private int[] itemKeys;
        [SerializeField] private int[] itemCounts;
        [SerializeField] private int currentBridgeCount;

        [Header("Guest Input Message Test Data")]
        [SerializeField] private string guestInputId;
        [SerializeField] private float guestInputX;
        [SerializeField] private float guestInputY;
        [SerializeField] private bool guestInputIsLeft;
        [SerializeField] private bool guestInputIsRight;
        [SerializeField] private bool guestInputIsBuild;
        
        
        
        [ContextMenu("SetGuestInput")]
        public void SetGuestInput()
        {
            // (혹시 모를 Null 방지 처리를 해주는 것이 안전합니다)
            if (RoomClient.Instance.GuestInputMessage == null) return;

            RoomClient.Instance.GuestInputMessage.Id = guestInputId;
            RoomClient.Instance.GuestInputMessage.X = guestInputX;
            RoomClient.Instance.GuestInputMessage.Y = guestInputY;
            RoomClient.Instance.GuestInputMessage.IsLeftShiftHold = guestInputIsLeft;
            RoomClient.Instance.GuestInputMessage.IsRightShiftHold = guestInputIsRight;
            RoomClient.Instance.GuestInputMessage.IsBuildKeyHold = guestInputIsBuild;
        }

        [ContextMenu("SetSnapshotTestData")]
        public void SetSnapshotData()
        {
            if (RoomClient.Instance.SnapshotMessage == null) return;

            // 기존 데이터 세팅
            RoomClient.Instance.SnapshotMessage.ItemKeys = itemKeys;
            RoomClient.Instance.SnapshotMessage.ItemCounts = itemCounts;
            RoomClient.Instance.SnapshotMessage.CurrentBridgeCount = currentBridgeCount;
            
            // 3. [보완] 인스펙터의 편집용 데이터를 실제 프로토콜 규격(FishState) 배열로 변환하여 삽입
            if (testFishes != null)
            {
                List<FishState> fishStates = new List<FishState>();
                foreach (var tf in testFishes)
                {
                    fishStates.Add(new FishState
                    {
                        Id = tf.id,
                        FishTypeIndex = tf.fishTypeIndex,
                        X = tf.x,
                        Y = tf.y,
                        ScaleX = tf.scaleX,
                        Angle = tf.angle,
                        CurrentState = tf.currentState
                    });
                }
                
                RoomClient.Instance.SnapshotMessage.Fishes = fishStates.ToArray();
            }

            Debug.Log($"[Tester] {testFishes.Count}개의 테스트 물고기 데이터가 가상 스냅샷에 주입되었습니다!");
        }
    }
    
}