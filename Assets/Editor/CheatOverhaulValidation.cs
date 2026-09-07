using System;
using System.Collections.Generic;
using System.Reflection;
using HelloServer;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object = UnityEngine.Object;

namespace WeGotOne.Editor
{
    // Run with Unity -batchmode -executeMethod WeGotOne.Editor.CheatOverhaulValidation.Run.
    // This deliberately uses a separate editor process, never the user's unsaved scene.
    public static class CheatOverhaulValidation
    {
        private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
        private static int checks;

        public static void Run()
        {
            if (!Application.isBatchMode)
                throw new InvalidOperationException("Run this validation in a separate batchmode editor.");

            try
            {
                ValidateScene("Assets/00. Scenes/InGame.unity");
                ValidateScene("Assets/00. Scenes/InGame With Tuto.unity");
                EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                ValidateControlsAndPlayers();
                Debug.Log($"CHEAT_OVERHAUL_VALIDATION_PASS: {checks} checks");
                EditorApplication.Exit(0);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        }

        private static void ValidateScene(string path)
        {
            EditorSceneManager.OpenScene(path);
            IntegratedDebugUI ui = Object.FindFirstObjectByType<IntegratedDebugUI>();
            Check(ui != null && ui.GetComponent<NetworkDebugOnGUI>() != null, path + ": debug UI wiring");
            Check(!Get<bool>(ui, "isDebug"), path + ": debug is opt-in");
            WorldItem[] prefabs = Get<WorldItem[]>(Object.FindFirstObjectByType<FishObjectPool>(), "fishPrefabs");
            for (int id = 1; id <= 9; id++)
            {
                int itemId = id;
                Check(Array.Exists(prefabs, prefab => prefab != null && prefab.ItemId == itemId &&
                    prefab.GetComponent<Fish>() != null && prefab.GetComponent<FishMovement>() != null &&
                    AssetDatabase.GetAssetPath(prefab).StartsWith("Assets/04. Prefabs/Fish/")),
                    path + $": fish {id} is wired to its prefab");
            }
        }

        private static void ValidateControlsAndPlayers()
        {
            GameObject root = new GameObject("Validation root");
            root.SetActive(false);
            RoomClient room = root.AddComponent<RoomClient>();
            typeof(SceneSingleton<RoomClient>).GetField("instance", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, room);
            typeof(SceneSingleton<RoomClient>).GetField("isInitialized", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, true);
            User host = new User { Id = "u2", NickName = "Host", IsHost = true };
            typeof(RoomClient).GetProperty("Me").SetValue(room, host);

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/04. Prefabs/Player/Player.prefab");
            PlayerController player = Object.Instantiate(prefab).GetComponent<PlayerController>();
            typeof(PlayerController).GetProperty("User").SetValue(player, host);
            PlayerController guest = Object.Instantiate(prefab).GetComponent<PlayerController>();
            typeof(PlayerController).GetProperty("User").SetValue(guest,
                new User { Id = "u10", NickName = "Guest", IsHost = false });

            // Dictionary insertion order intentionally differs from the nameplate order.
            Dictionary<string, PlayerController> players = Get<Dictionary<string, PlayerController>>(room, "remotes");
            players.Add("u10", guest);
            players.Add("u2", player);
            Invoke(room, "UpdateUserText", true, "u10");
            Invoke(room, "UpdateUserText", true, "u2");
            Check(room.GetPlayerByNumber(1) == player && room.GetPlayerByNumber(2) == guest,
                "Player numbers follow numeric room IDs, not dictionary insertion order");

            PlayerStamina stamina = player.Body;
            int recovered = 0;
            stamina.OnPlayerStarveStateChanged += starved => { if (!starved) recovered++; };
            stamina.SetDebugStamina(0f);
            stamina.SetDebugStamina(10000f);
            Check(stamina.CurrentStamina == 10000f && recovered == 1, "10000 stamina revives a starved player");
            stamina.IncreaseStamina(10f);
            Check(stamina.CurrentStamina == 10000f, "Recovery does not discard extra debug stamina");
            stamina.DecreaseStamina(10f);
            Check(stamina.CurrentStamina == 9990f, "Debug stamina still consumes normally");
            stamina.SetDebugStamina(stamina.MaxStamina);
            Check(stamina.CurrentStamina == stamina.MaxStamina, "Restore exactly maxStamina");
            stamina.ApplySnapshot(10000f);
            Check(stamina.CurrentStamina == 10000f, "Snapshot preserves 10000 stamina on a guest");

            Rigidbody2D[] parts = player.GetComponentsInChildren<Rigidbody2D>();
            Vector2[] positions = Array.ConvertAll(parts, part => part.position);
            foreach (Rigidbody2D part in parts)
            {
                part.linearVelocity = Vector2.one * 7f;
                part.angularVelocity = 50f;
            }
            Vector2 destination = (Vector2)player.Body.transform.position + new Vector2(25f, -13f);
            Vector2 offset = destination - (Vector2)player.Body.transform.position;
            player.DebugTeleport(destination);
            Check(parts.Length >= 5, "Teleport test covers body and all arms");
            for (int i = 0; i < parts.Length; i++)
                Check(Vector2.Distance(parts[i].position, positions[i] + offset) < 0.001f &&
                    parts[i].linearVelocity == Vector2.zero && parts[i].angularVelocity == 0f,
                    "Teleport preserves relative pose and clears velocity: " + parts[i].name);

            IntegratedDebugUI ui = new GameObject("Debug UI").AddComponent<IntegratedDebugUI>();
            Invoke(ui, "OnEnable");
            Keyboard keyboard = InputSystem.AddDevice<Keyboard>();
            Press(ui, keyboard, Key.F1);
            Check(!Get<bool>(ui, "isOpen"), "F1 does nothing until isDebug is checked");
            Press(ui, keyboard);
            Set(ui, "isDebug", true);
            Press(ui, keyboard, Key.F1);
            Check(Get<bool>(ui, "isOpen"), "F1 opens the UI");
            Press(ui, keyboard, Key.F1);
            Check(Get<bool>(ui, "isOpen"), "Holding F1 does not close the UI");
            Press(ui, keyboard);
            Check(Get<bool>(ui, "isOpen"), "Releasing F1 leaves the UI open");
            Press(ui, keyboard, Key.F1);
            Check(!Get<bool>(ui, "isOpen"), "The next F1 press closes the UI");

            Set(ui, "teleportEnabled", true);
            Press(ui, keyboard, Key.Digit2);
            Check(IntegratedDebugUI.IsTeleportModeActive && Get<PlayerController>(ui, "selectedPlayer") == guest,
                "Teleport selection works with the UI closed");
            host.IsHost = false;
            Check(!IntegratedDebugUI.IsTeleportModeActive, "Guests cannot run authoritative cheats");
            host.IsHost = true;
            Set(ui, "isDebug", false);
            Press(ui, keyboard);
            Check(!IntegratedDebugUI.IsTeleportModeActive && Get<PlayerController>(ui, "selectedPlayer") == null,
                "Unchecking isDebug clears teleport mode and selection");
            InputSystem.RemoveDevice(keyboard);
        }

        private static void Press(IntegratedDebugUI ui, Keyboard keyboard, params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
            Invoke(ui, "Update");
        }

        private static T Get<T>(object target, string name) => (T)target.GetType().GetField(name, Fields).GetValue(target);
        private static void Set(object target, string name, object value) => target.GetType().GetField(name, Fields).SetValue(target, value);
        private static void Invoke(object target, string name, params object[] arguments) =>
            target.GetType().GetMethod(name, Fields).Invoke(target, arguments);

        private static void Check(bool condition, string description)
        {
            if (!condition)
                throw new InvalidOperationException("FAILED: " + description);
            checks++;
        }
    }
}
