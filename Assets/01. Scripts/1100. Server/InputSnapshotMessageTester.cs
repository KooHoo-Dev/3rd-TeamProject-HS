using UnityEngine;

namespace HelloServer  
{
    public class InputSnapshotMessageTester : MonoBehaviour
    {
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
            RoomClient.Instance.SnapshotMessage.ItemKeys = itemKeys;
            RoomClient.Instance.SnapshotMessage.ItemCounts = itemCounts;
            RoomClient.Instance.SnapshotMessage.CurrentBridgeCount = currentBridgeCount;
        }
    }
}