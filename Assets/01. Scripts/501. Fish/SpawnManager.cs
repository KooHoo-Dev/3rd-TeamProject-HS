using System;
using System.Collections;
using System.Collections.Generic;
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

        [SerializeField] private List<StageEntry> stages;
        private Transform barricadeGroup;
        private List<Transform> barricades = new List<Transform>();

        private BridgeManager bridgeManager;
        private DayLoop dayLoop;
        private FishObjectPool fishObjectPool;
        private StageEntry currentStage;
        private int currentGroupIndex;
        
        private bool IsLastStage(StageEntry entry) => stages.IndexOf(entry) == stages.Count - 1;

        private IEnumerator Start()
        {
            yield return new WaitUntil(() => ItemManager.Instance.IsInitialized);
            yield return new WaitUntil(() => RoomClient.Instance != null && RoomClient.Instance.Me != null);

            bridgeManager = BridgeManager.Instance;
            bridgeManager.OnBridgeBuild += HandleBridgeBuild;
            currentStage = FindStage(bridgeManager.CurrentBridgeCount);
            if (RoomClient.Instance.Me.IsHost == false)
            {
                yield break;
            }

            dayLoop = DayLoop.Instance;
            fishObjectPool = FishObjectPool.Instance;
            
            // 바리케이드 정보 초기화
            barricadeGroup = GameObject.Find("Barricade Group").transform;
            foreach (Transform child in barricadeGroup)
            {
                barricades.Add(child);
            }
            barricades[0].gameObject.SetActive(true);

            ValidateStages();

            dayLoop.OnDayChanged += HandleDayChanged;

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
            StageEntry nextStage = FindStage(bridgeManager.CurrentBridgeCount);
            if (nextStage == currentStage)
            {
                return;
            }
            
            GameManager.Instance.IsCamTargetChange = true;
            currentStage = nextStage;
            
            if (RoomClient.Instance.Me.IsHost == false)
                return;

            ChangeBarricade(currentStage, nextStage);

            fishObjectPool.ReleaseAllActiveFish();
            
            currentGroupIndex = 0;
            SpawnCurrentGroup();
        }

        private void ChangeBarricade(StageEntry currentStage, StageEntry nextStage)
        {
            barricades[FindStageIndex(currentStage)].gameObject.SetActive(false);
            
            // 마지막 스테이지가 아니면 다음 바리케이드 활성화
            if (IsLastStage(nextStage) == false)
                barricades[FindStageIndex(nextStage)].gameObject.SetActive(true);
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

        private int FindStageIndex(StageEntry entry)
        {
            return stages.IndexOf(entry);
        }

        private void ValidateStages()
        {
            if (stages == null || stages.Count == 0)
            {
                throw new InvalidOperationException("Fish Spawn Stage가 설정되지 않았습니다.");
            }

            if (stages.Count - 1 != barricades.Count)
            {
                throw new InvalidOperationException($"바리케이드 개수가 유효하지 않습니다. 바리케이드 개수가 스테이지 개수보다 1개 적어야함. 스테이지 수({stages.Count}), 바리케이드 수({barricades.Count})");
            }

            for (int i = 0; i < stages.Count; i++)
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
