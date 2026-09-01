using System.Collections.Generic;
using UnityEngine;

namespace HelloServer  
{
    public class InputSnapshotMessageTester : MonoBehaviour
    {
        [System.Serializable]
        private struct TestInventoryEntry
        {
            public int itemId;
            public int count;
        }

        // 1. [보완] 유니티 인스펙터에서 물고기 데이터를 쉽게 편집할 수 있도록 헬퍼 구조체 선언
        [System.Serializable]
        public struct TestFishState
        {
            public int worldItemId;
            public int itemId;
            public float x;                // 가상 X 위치
            public float y;                // 가상 Y 위치
            public float scaleX;           // 좌우 대칭 (1 or -1)
            public float angle;            // 회전 각도
            public string currentState;    // 상태 (Active, Idle, Caught 등)
        }
        
        // 2. [보완] 인스펙터에서 마우스 클릭으로 편집 가능한 물고기 리스트
        [SerializeField] private List<TestFishState> testFishes = new List<TestFishState>();
        
        [Header("Snapshot Message Test Data")]
        [SerializeField] private TestInventoryEntry[] inventoryEntries;
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
            if (inventoryEntries == null)
            {
                RoomClient.Instance.SnapshotMessage.InventoryEntries = new InventoryEntryState[0];
            }
            else
            {
                InventoryEntryState[] entries = new InventoryEntryState[inventoryEntries.Length];
                for (int i = 0; i < inventoryEntries.Length; i++)
                {
                    entries[i] = new InventoryEntryState
                    {
                        ItemId = inventoryEntries[i].itemId,
                        Count = inventoryEntries[i].count,
                    };
                }

                RoomClient.Instance.SnapshotMessage.InventoryEntries = entries;
            }
            RoomClient.Instance.SnapshotMessage.CurrentBridgeCount = currentBridgeCount;
            
            // 3. [보완] 인스펙터의 편집용 데이터를 실제 프로토콜 규격(FishState) 배열로 변환하여 삽입
            if (testFishes != null)
            {
                List<FishState> fishStates = new List<FishState>();
                foreach (var tf in testFishes)
                {
                    fishStates.Add(new FishState
                    {
                        WorldItemId = tf.worldItemId,
                        ItemId = tf.itemId,
                        X = tf.x,
                        Y = tf.y,
                        ScaleX = tf.scaleX,
                        Angle = tf.angle,
                        CurrentState = tf.currentState
                    });
                }
                
                RoomClient.Instance.SnapshotMessage.Fishes = fishStates.ToArray();
            }
            else
            {
                RoomClient.Instance.SnapshotMessage.Fishes = System.Array.Empty<FishState>();
            }

            Debug.Log($"[Tester] {(testFishes?.Count ?? 0)}개의 테스트 물고기 데이터가 가상 스냅샷에 주입되었습니다!");
        }
    }
    
}
