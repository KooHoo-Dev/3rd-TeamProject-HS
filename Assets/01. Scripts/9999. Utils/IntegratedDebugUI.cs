using HelloServer;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace WeGotOne
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(NetworkDebugOnGUI))]
    public sealed class IntegratedDebugUI : MonoBehaviour
    {
        [SerializeField] private bool isDebug;

        private const float ReferenceWidth = 1920f;
        private const float ReferenceHeight = 1080f;
        private static readonly Key[] PlayerKeys =
        {
            Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5,
            Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9
        };
        private static readonly string[] FishNames =
        {
            "1. Clownfish", "2. PinkFish", "3. Shark", "4. Squid", "5. Mantis shrimp",
            "6. MineFish", "7. Octopus", "8. Manta ray", "9. Seahorse"
        };

        private static IntegratedDebugUI instance;
        private bool isOpen;
        private bool showNetwork;
        private bool showArms;
        private bool teleportEnabled;
        private PlayerController selectedPlayer;
        private NetworkDebugOnGUI networkDebug;
        private ArmController[] arms = System.Array.Empty<ArmController>();
        private float nextArmRefreshTime;
        private Vector2 controlsScroll;
        private Vector2 armsScroll;
        private GUIStyle headingStyle;
        private GUIStyle textStyle;
        private GUIStyle buttonStyle;
        private GUIStyle toggleStyle;

        public static bool IsTeleportModeActive => instance != null && instance.isActiveAndEnabled &&
            instance.isDebug && instance.teleportEnabled && HasHostAuthority;
        public static bool ShowArmDebug => instance != null && instance.isActiveAndEnabled &&
            instance.isDebug && instance.isOpen && instance.showArms;
        private static bool HasHostAuthority => RoomClient.IsInitialized &&
            RoomClient.Instance.Me != null && RoomClient.Instance.Me.IsHost;

        private void OnEnable()
        {
            instance = this;
            networkDebug = GetComponent<NetworkDebugOnGUI>();
        }

        private void OnDisable()
        {
            isOpen = false;
            teleportEnabled = false;
            selectedPlayer = null;
            if (instance == this)
                instance = null;
        }

        private void Update()
        {
            if (!isDebug)
            {
                isOpen = false;
                teleportEnabled = false;
                selectedPlayer = null;
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f1Key.wasPressedThisFrame)
                isOpen = !isOpen;

            if (ShowArmDebug && Time.unscaledTime >= nextArmRefreshTime)
            {
                arms = FindObjectsByType<ArmController>(FindObjectsSortMode.InstanceID);
                nextArmRefreshTime = Time.unscaledTime + 0.5f;
            }

            // F1로 창을 닫은 뒤에도 체크한 순간이동 모드는 유지합니다.
            if (!IsTeleportModeActive || keyboard == null)
                return;

            RoomClient room = RoomClient.Instance;
            for (int i = 0; i < Mathf.Min(room.RemoteCount, PlayerKeys.Length); i++)
            {
                if (keyboard[PlayerKeys[i]].wasPressedThisFrame)
                    selectedPlayer = room.GetPlayerByNumber(i + 1);
            }

            if (keyboard.fKey.wasPressedThisFrame && selectedPlayer != null && Mouse.current != null)
            {
                Camera camera = Camera.main;
                Vector3 screenPosition = Mouse.current.position.ReadValue();
                screenPosition.z = camera.WorldToScreenPoint(selectedPlayer.Body.transform.position).z;
                selectedPlayer.DebugTeleport(camera.ScreenToWorldPoint(screenPosition));
            }
        }

        private void OnGUI()
        {
            if (!isDebug || !isOpen)
                return;

            InitStyles();
            Matrix4x4 previousMatrix = GUI.matrix;
            float scale = Mathf.Min(Screen.width / ReferenceWidth, Screen.height / ReferenceHeight);
            Vector3 offset = new Vector3((Screen.width - ReferenceWidth * scale) * 0.5f,
                (Screen.height - ReferenceHeight * scale) * 0.5f, 0f);
            GUI.matrix = Matrix4x4.TRS(offset, Quaternion.identity, Vector3.one * scale);

            GUI.Box(new Rect(20, 20, 1880, 60), GUIContent.none);
            GUI.Label(new Rect(36, 32, 750, 36), "DEBUG / CHEATS    |    F1: Toggle", headingStyle);
            showNetwork = GUI.Toggle(new Rect(1300, 36, 270, 30), showNetwork, "Network debug", toggleStyle);
            showArms = GUI.Toggle(new Rect(1610, 36, 270, 30), showArms, "Arm input debug", toggleStyle);

            DrawControls();
            if (showNetwork)
                networkDebug.DrawPanel(new Rect(440, 100, showArms ? 820 : 1460, 960));
            if (showArms)
                ArmControllerVisualizer.DrawPanel(new Rect(showNetwork ? 1280 : 440, 100,
                    showNetwork ? 620 : 1460, 960), arms, ref armsScroll, headingStyle, textStyle);

            GUI.matrix = previousMatrix;
        }

        private void InitStyles()
        {
            if (headingStyle != null)
                return;

            headingStyle = new GUIStyle(GUI.skin.label) { fontSize = 22, fontStyle = FontStyle.Bold };
            textStyle = new GUIStyle(GUI.skin.label) { fontSize = 17, wordWrap = true };
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 17, fixedHeight = 38 };
            toggleStyle = new GUIStyle(GUI.skin.toggle) { fontSize = 18, wordWrap = true };
        }

        private void DrawControls()
        {
            GUILayout.BeginArea(new Rect(20, 100, 400, 960), GUI.skin.box);
            controlsScroll = GUILayout.BeginScrollView(controlsScroll);
            GUILayout.Label("CONTROLS", headingStyle);
            if (GUILayout.Button("Reload current scene", buttonStyle))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().path);
                GUIUtility.ExitGUI();
            }
            GUILayout.Label("Reload returns this client to the scene's initial state / lobby.", textStyle);
            GUILayout.Space(14);

            bool previousEnabled = GUI.enabled;
            GUI.enabled = previousEnabled && HasHostAuthority;
            GUILayout.Label(HasHostAuthority ? "HOST CHEATS" : "Join as HOST to use cheats", headingStyle);
            teleportEnabled = GUILayout.Toggle(teleportEnabled, "Teleport player (number -> F)", toggleStyle);
            GUILayout.Label("Select 1-9, point at the world, then press F. Works with the UI closed.", textStyle);
            if (selectedPlayer != null)
                GUILayout.Label($"Target: {selectedPlayer.User.NickName} ({selectedPlayer.User.Id})", textStyle);
            if (RoomClient.IsInitialized && RoomClient.Instance.Me != null)
            {
                for (int i = 1; i <= RoomClient.Instance.RemoteCount; i++)
                {
                    PlayerController player = RoomClient.Instance.GetPlayerByNumber(i);
                    GUILayout.Label($"{i}. {player.User.NickName}  [{player.Body.CurrentStamina:0.0}]", textStyle);
                }
            }

            GUILayout.Space(14);
            if (GUILayout.Button("All stamina = 10000", buttonStyle))
                SetAllStamina(false);
            if (GUILayout.Button("All stamina = maxStamina", buttonStyle))
                SetAllStamina(true);
            if (GUILayout.Button("All items +10", buttonStyle))
                AddAllItemsToInventory();

            GUILayout.Space(14);
            if (GUILayout.Button("Toggle day / night", buttonStyle))
                DayLoop.Instance.DayNightTransition();
            if (GUILayout.Button("Build next bridge (free)", buttonStyle))
                BridgeManager.Instance.BuildBridgeWithoutCost();

            GUILayout.Space(14);
            GUILayout.Label("SPAWN FISH", headingStyle);
            GUILayout.Label("Spawns beside your player (3 units right).", textStyle);
            for (int row = 0; row < 3; row++)
            {
                GUILayout.BeginHorizontal();
                for (int column = 0; column < 3; column++)
                {
                    int index = row * 3 + column;
                    if (GUILayout.Button(new GUIContent((index + 1).ToString(), FishNames[index]), buttonStyle))
                        SpawnFish(index + 1);
                }
                GUILayout.EndHorizontal();
            }
            foreach (string fishName in FishNames)
                GUILayout.Label(fishName, textStyle);

            GUI.enabled = previousEnabled;
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private void SetAllStamina(bool restoreMaximum)
        {
            foreach (PlayerStamina stamina in FindObjectsByType<PlayerStamina>(FindObjectsSortMode.None))
                stamina.SetDebugStamina(restoreMaximum ? stamina.MaxStamina : 10000f);
        }

        private void AddAllItemsToInventory()
        {
            foreach (ItemRef item in ItemManager.Instance.Items)
                InventoryItemDB.Instance.Add(item.ItemId, 10);
        }

        private void SpawnFish(int itemId)
        {
            FishObjectPool pool = FishObjectPool.Instance;
            WorldItem fish = pool.Get(itemId);
            fish.transform.position = RoomClient.Instance.Local.Body.transform.position + Vector3.right * 3f;
            fish.GetComponent<FishMovement>().SetWaterArea(
                GameObject.Find("WaterBorderLine").GetComponent<Collider2D>());
            if (!pool.GetComponent<WorldItemRegistry>().RegisterNewDynamic(fish, out _))
                throw new System.InvalidOperationException($"Debug fish {itemId} could not be registered.");
        }
    }
}
