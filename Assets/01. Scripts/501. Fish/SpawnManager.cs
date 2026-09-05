using System;
using System.Collections;
using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    public class SpawnManager : MonoBehaviour
    {
        [Serializable]
        private class StageEntry
        {
            [SerializeField] private int minBridgeCount;
            [SerializeField] private FishSpawnerGroup[] groups;

            public int MinBridgeCount => minBridgeCount;
            public FishSpawnerGroup[] Groups => groups;
        }

        [SerializeField] private StageEntry[] stages;

        private BridgeManager bridgeManager;
        private DayLoop dayLoop;
        private FishObjectPool fishObjectPool;
        private StageEntry currentStage;
        private int currentGroupIndex;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => ItemManager.Instance.IsInitialized);
            yield return new WaitUntil(() => RoomClient.Instance != null && RoomClient.Instance.Me != null);

            if (RoomClient.Instance.Me.IsHost == false)
            {
                yield break;
            }

            bridgeManager = BridgeManager.Instance;
            dayLoop = DayLoop.Instance;
            fishObjectPool = FishObjectPool.Instance;

            ValidateStages();

            bridgeManager.OnBridgeBuild += HandleBridgeBuild;
            dayLoop.OnDayChanged += HandleDayChanged;

            currentStage = FindStage(bridgeManager.CurrentBridgeCount());
            currentGroupIndex = 0;
            SpawnCurrentGroup();
        }

        private void OnDestroy()
        {
            if (bridgeManager != null)
            {
                bridgeManager.OnBridgeBuild -= HandleBridgeBuild;
            }

            if (dayLoop != null)
            {
                dayLoop.OnDayChanged -= HandleDayChanged;
            }
        }

        private void HandleBridgeBuild()
        {
            StageEntry nextStage = FindStage(bridgeManager.CurrentBridgeCount());
            if (nextStage == currentStage)
            {
                return;
            }

            fishObjectPool.ReleaseAllActiveFish();
            currentStage = nextStage;
            currentGroupIndex = 0;
            SpawnCurrentGroup();
        }

        private void HandleDayChanged()
        {
            fishObjectPool.ReleaseAllActiveFish();
            currentGroupIndex = (currentGroupIndex + 1) % currentStage.Groups.Length;
            SpawnCurrentGroup();
        }

        private void SpawnCurrentGroup()
        {
            currentStage.Groups[currentGroupIndex].Spawn();
        }

        private StageEntry FindStage(int bridgeCount)
        {
            StageEntry selectedStage = null;

            foreach (StageEntry stage in stages)
            {
                if (stage.MinBridgeCount <= bridgeCount &&
                    (selectedStage == null || stage.MinBridgeCount > selectedStage.MinBridgeCount))
                {
                    selectedStage = stage;
                }
            }

            if (selectedStage == null)
            {
                throw new InvalidOperationException(
                    $"BridgeCount {bridgeCount}에 해당하는 Fish Spawn Stage가 없습니다.");
            }

            return selectedStage;
        }

        private void ValidateStages()
        {
            if (stages == null || stages.Length == 0)
            {
                throw new InvalidOperationException("Fish Spawn Stage가 설정되지 않았습니다.");
            }

            for (int i = 0; i < stages.Length; i++)
            {
                StageEntry stage = stages[i];
                if (stage == null || stage.MinBridgeCount < 0 ||
                    stage.Groups == null || stage.Groups.Length == 0)
                {
                    throw new InvalidOperationException($"Fish Spawn Stage 설정이 올바르지 않습니다: {i}");
                }

                foreach (FishSpawnerGroup group in stage.Groups)
                {
                    if (group == null)
                    {
                        throw new InvalidOperationException(
                            $"Fish Spawn Stage {i}에 비어 있는 FishSpawnerGroup이 있습니다.");
                    }
                }

                for (int j = 0; j < i; j++)
                {
                    if (stages[j].MinBridgeCount == stage.MinBridgeCount)
                    {
                        throw new InvalidOperationException(
                            $"중복된 MinBridgeCount가 있습니다: {stage.MinBridgeCount}");
                    }
                }
            }
        }
    }
}
