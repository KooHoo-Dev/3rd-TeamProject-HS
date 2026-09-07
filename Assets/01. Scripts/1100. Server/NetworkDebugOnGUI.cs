using UnityEngine;

namespace HelloServer
{
    public class NetworkDebugOnGUI : MonoBehaviour
    {
        private Vector2 _scrollPosition;

        private bool _showInputGroupDetails;
        private bool _showPlayerDetails;
        private bool _showFishDetails;
        private bool _showPlankDetails;

        private GUIStyle _labelStyle;
        private GUIStyle _headerStyle;

        private void InitStyles()
        {
            if (_labelStyle != null)
                return;

            _labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                wordWrap = true
            };

            _headerStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold
            };
        }

        public void DrawPanel(Rect area)
        {
            InitStyles();

            if (!RoomClient.IsInitialized)
            {
                GUI.Box(area, "NETWORK DEBUG: RoomClient is not in this scene.");
                return;
            }

            GUILayout.BeginArea(
                area,
                GUI.skin.box
            );

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            DrawHeader();
            DrawGuestInput();
            DrawInputGroup();
            DrawSnapshot();

            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void DrawHeader()
        {
            bool isHost =
                RoomClient.Instance.Me != null && RoomClient.Instance.Me.IsHost;

            GUILayout.Label("NETWORK DEBUG", _headerStyle);

            GUILayout.Label(
                $"Role : {(isHost ? "HOST" : "GUEST")}",
                _labelStyle
            );

            GUILayout.Space(10);
        }

        private void DrawGuestInput()
        {
            GuestInputMessage input =
                RoomClient.Instance.GuestInputMessage;

            GUILayout.Label("LOCAL INPUT", _headerStyle);

            if (input == null)
            {
                GUILayout.Label("null", _labelStyle);
                GUILayout.Space(10);
                return;
            }

            GUILayout.Label(
                $"Id={input.Id}  " +
                $"X={input.X:F2}  " +
                $"Y={input.Y:F2}",
                _labelStyle
            );

            GUILayout.Label(
                $"LeftShift={input.IsLeftShiftHold}  " +
                $"RightShift={input.IsRightShiftHold}  " +
                $"Build={input.IsBuildKeyHold}",
                _labelStyle
            );

            GUILayout.Space(10);
        }

        private void DrawInputGroup()
        {
            GuestInputGroupMessage inputGroup =
                RoomClient.Instance.GuestInputGroupMessage;

            GUILayout.Label("INPUT GROUP", _headerStyle);

            if (inputGroup == null)
            {
                GUILayout.Label("null", _labelStyle);
                GUILayout.Space(10);
                return;
            }

            int count = inputGroup.Inputs?.Count ?? 0;

            GUILayout.Label(
                $"Inputs : {count}",
                _labelStyle
            );

            _showInputGroupDetails = GUILayout.Toggle(
                _showInputGroupDetails,
                "Show InputGroup Details"
            );

            if (_showInputGroupDetails && inputGroup.Inputs != null)
            {
                foreach (GuestInputMessage input in inputGroup.Inputs)
                {
                    if (input == null)
                        continue;

                    GUILayout.Label(
                        $"[{input.Id}] " +
                        $"X={input.X:F2}, Y={input.Y:F2}, " +
                        $"L={input.IsLeftShiftHold}, " +
                        $"R={input.IsRightShiftHold}, " +
                        $"Build={input.IsBuildKeyHold}",
                        _labelStyle
                    );
                }
            }

            GUILayout.Space(10);
        }

        private void DrawSnapshot()
        {
            SnapshotMessage snapshot =
                RoomClient.Instance.SnapshotMessage;

            GUILayout.Label("SNAPSHOT", _headerStyle);

            if (snapshot == null)
            {
                GUILayout.Label("null", _labelStyle);
                return;
            }

            int playerCount = snapshot.Players?.Length ?? 0;
            int fishCount = snapshot.Fishes?.Length ?? 0;
            int plankCount = snapshot.Planks?.Length ?? 0;

            GUILayout.Label(
                $"Players : {playerCount}",
                _labelStyle
            );

            GUILayout.Label(
                $"Fishes : {fishCount}",
                _labelStyle
            );

            GUILayout.Label(
                $"Planks : {plankCount}",
                _labelStyle
            );

            GUILayout.Label(
                $"Bridge Count : {snapshot.CurrentBridgeCount}",
                _labelStyle
            );

            GUILayout.Label(
                $"IsDay : {snapshot.IsDay}",
                _labelStyle
            );

            GUILayout.Label(
                $"Active Players : {snapshot.ActivePlayerCount}",
                _labelStyle
            );

            GUILayout.Label(
                $"Arrived Players : {snapshot.ArrivedPlayerCount}",
                _labelStyle
            );

            DrawInventory(snapshot);

            GUILayout.Space(5);

            _showPlayerDetails = GUILayout.Toggle(
                _showPlayerDetails,
                "Show Player Details"
            );

            if (_showPlayerDetails)
            {
                DrawPlayers(snapshot.Players);
            }

            _showFishDetails = GUILayout.Toggle(
                _showFishDetails,
                "Show Fish Details"
            );

            if (_showFishDetails)
            {
                DrawFishes(snapshot.Fishes);
            }

            _showPlankDetails = GUILayout.Toggle(
                _showPlankDetails,
                "Show Plank Details"
            );

            if (_showPlankDetails)
            {
                DrawPlanks(snapshot.Planks);
            }
        }

        private void DrawInventory(SnapshotMessage snapshot)
        {
            GUILayout.Label("Inventory", _labelStyle);

            if (snapshot.InventoryEntries == null)
            {
                GUILayout.Label("  null", _labelStyle);
                return;
            }

            for (int i = 0; i < snapshot.InventoryEntries.Length; i++)
            {
                InventoryEntryState entry = snapshot.InventoryEntries[i];
                if (entry == null)
                {
                    GUILayout.Label($"  Entry {i}: null", _labelStyle);
                    continue;
                }

                GUILayout.Label(
                    $"  Item {entry.ItemId} : {entry.Count}",
                    _labelStyle
                );
            }
        }

        private void DrawPlayers(PlayerState[] players)
        {
            if (players == null)
                return;

            GUILayout.Space(5);

            foreach (PlayerState player in players)
            {
                if (player == null)
                    continue;

                GUILayout.Label(
                    $"Player [{player.Id}]",
                    _headerStyle
                );

                GUILayout.Label(
                    $"Grab : Left={player.IsLeftGrab}, Right={player.IsRightGrab}",
                    _labelStyle
                );

                DrawPart("Body", player.Body);
                DrawPart("LeftArmTop", player.LeftArmTop);
                DrawPart("LeftArmBottom", player.LeftArmBottom);
                DrawPart("RightArmTop", player.RightArmTop);
                DrawPart("RightArmBottom", player.RightArmBottom);

                GUILayout.Space(5);
            }
        }

        private void DrawPart(string name, PartState part)
        {
            if (part == null)
            {
                GUILayout.Label(
                    $"  {name}: null",
                    _labelStyle
                );
                return;
            }

            GUILayout.Label(
                $"  {name}: Pos({part.X:F2}, {part.Y:F2})",
                _labelStyle
            );

            if (part.Rotation != null)
            {
                GUILayout.Label(
                    $"    Rot(" +
                    $"{part.Rotation.X:F2}, " +
                    $"{part.Rotation.Y:F2}, " +
                    $"{part.Rotation.Z:F2}, " +
                    $"{part.Rotation.W:F2})",
                    _labelStyle
                );
            }
        }

        private void DrawFishes(FishState[] fishes)
        {
            if (fishes == null)
                return;

            GUILayout.Space(5);

            foreach (FishState fish in fishes)
            {
                if (fish == null)
                    continue;

                GUILayout.Label(
                    $"[WorldItemId={fish.WorldItemId}] " +
                    $"ItemId={fish.ItemId}, " +
                    $"Pos=({fish.X:F2}, {fish.Y:F2}), " +
                    $"ScaleX={fish.ScaleX:F2}, " +
                    $"Angle={fish.Angle:F2}, " +
                    $"State={fish.CurrentState}",
                    _labelStyle
                );
            }
        }

        private void DrawPlanks(PlankState[] planks)
        {
            if (planks == null)
                return;

            GUILayout.Space(5);

            foreach (PlankState plank in planks)
            {
                if (plank == null)
                    continue;

                GUILayout.Label(
                    $"[WorldItemId={plank.WorldItemId}] " +
                    $"ItemId={plank.ItemId}, " +
                    $"Pos=({plank.X:F2}, {plank.Y:F2}), " +
                    $"Angle={plank.Angle:F2}, " +
                    $"IsPickUp={plank.IsPickUp}",
                    _labelStyle
                );
            }
        }
    }
}
