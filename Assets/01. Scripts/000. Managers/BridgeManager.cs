using System;
using System.Collections;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    public class BridgeManager : SceneSingleton<BridgeManager>
    {
        [field: SerializeField] public Bridge[] BridgeObjects { get; private set; }
        private Transform bridgeParent;
        private const int PLANK_ITEM_KEY = 0;
        [SerializeField] private Transform bridgeTransform;
        
        private BridgeTable bridgeTable;
        private int currentBridgeCount;
        
        [SerializeField] private float particleTime = 2.0f;
        [SerializeField] private GameObject buildParticle;
        
        
        public bool IsEnd => CurrentBridgeCount() >= BridgeObjects.Length;

        // 게임 매니저나 나중에 다른 UI에게 전파하는 용도 델리게이트
        public event Action OnBridgeBuild;

        // Awake -> OnInitialize 로 전환 (SingletonBase)에 맞게
        protected override void OnInitialize()
        {
            base.OnInitialize();
        }

        private async void Start()
        {
            try
            {
                BridgeObjects = bridgeTransform.GetComponentsInChildren<Bridge>(true);
                await UniTask.WaitUntil(() => ItemManager.Instance.IsInitialized);
                bridgeTable = await BridgeTable.LoadAsync(ItemManager.Instance.Items);
            }
            catch (Exception exception)
            {
                Debug.LogError($"{gameObject.name}: BridgeTable.LoadAsync() Failed");
                Debug.LogException(exception);
            }
        }

        public int CurrentBridgeCount()
        {
            int currentCount = 0;

            for (int i = 0; i < BridgeObjects.Length; i++)
            {
                if (BridgeObjects[i].gameObject.activeSelf)
                {
                    currentCount++;
                }
            }

            return currentCount;
        }

        public bool TryGetNextBridgeRef(out BridgeRef bridgeRef)
        {
            bridgeRef = null;

            if (bridgeTable == null || IsEnd)
            {
                return false;
            }

            return bridgeTable.TryGet(CurrentBridgeCount() + 1, out bridgeRef);
        }

        public void BridgeBuild()
        {
            if (IsEnd) return;

            currentBridgeCount = CurrentBridgeCount();
            int nextBridgeIndex = currentBridgeCount + 1;
            if (bridgeTable.TryGet(nextBridgeIndex, out BridgeRef bridgeRef) == false)
            {
                return;
            }

            InventoryItemDB inventoryItemDB = InventoryItemDB.Instance;
            ItemManager itemManager = ItemManager.Instance;
            if (BridgeBuildCostCalculator.TryCreateConsumePlan(
                    bridgeRef,
                    inventoryItemDB.EnumerateCounts(),
                    itemManager.Items,
                    out System.Collections.Generic.IReadOnlyDictionary<int, int> consumePlan) == false ||
                inventoryItemDB.TryConsumeAll(consumePlan) == false)
            {
                return;
            }

            StartCoroutine(BuildParticle());
            OnBridgeBuild?.Invoke();
        }

        public void ApplySnapshot(int messageCurrentBridgeCount)
        {
            for (int i = 0; i < messageCurrentBridgeCount; i++)
            {
                BridgeObjects[i].gameObject.SetActive(true);
            }
        }
        
        public void HandleItemPickup(Collision2D other)
        {
            if (other.gameObject.TryGetComponent<WorldItem>(out var holder))
            {
                ItemRef item = holder.ThisItem;

                if (item == null) return;

                if (InventoryItemDB.Instance.Add(item.ItemId, 1))
                {
                    AudioManager.Instance.PlaySfx(AudioManager.Sfx.GetItem);
                    holder.OnPickup();
                }
            }
        }

        private IEnumerator BuildParticle()
        {
            AudioManager.Instance.PlaySfx(AudioManager.Sfx.BridgeBuild);
            buildParticle.transform.position = BridgeObjects[currentBridgeCount].transform.position;
            buildParticle.SetActive(true);
            
            yield return new WaitForSeconds(particleTime);
            
            buildParticle.SetActive(false);
            BridgeObjects[currentBridgeCount].gameObject.SetActive(true);
        }

        #region test methods

        // 테스트 메소드 (나무 비용 없이)
        [ContextMenu("Build Bridge Without Cost")]
        public void BuildBridgeWithoutCost()
        {
            if (IsEnd || RoomClient.Instance.Me.IsHost == false) return;
            BridgeObjects[CurrentBridgeCount()].gameObject.SetActive(true);
            OnBridgeBuild?.Invoke();
        }
        
        #endregion
    }
}
