using System;
using UnityEngine;

namespace WeGotOne
{
    public class BridgeManager : SingletonBase<BridgeManager>
    {
        [field: SerializeField] public Bridge[] BridgeObjects { get; private set; }
        private Transform bridgeParent;
        private const int PLANK_ITEM_KEY = 0;
        [SerializeField] private Transform bridgeTransform;
        
        public bool IsEnd => CurrentBridgeCount() == BridgeObjects.Length;

        // 게임 매니저나 나중에 다른 UI에게 전파하는 용도 델리게이트
        public event Action OnBridgeBuild;

        // Awake -> OnInitialize 로 전환 (SingletonBase)에 맞게
        protected override void OnInitialize()
        {
            base.OnInitialize();
            BridgeObjects = bridgeTransform.GetComponentsInChildren<Bridge>(true);
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

        public void BridgeBuild()
        {
            if (IsEnd) return;

            if (InventoryItemDB.Instance.TryConsume(PLANK_ITEM_KEY, 1))
            {
                BridgeObjects[CurrentBridgeCount()].gameObject.SetActive(true);
                OnBridgeBuild?.Invoke();
            }
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
                ItemScriptableObject item = holder.ThisItem;

                if (item == null) return;

                if (InventoryItemDB.Instance.Add(item.Key, 1))
                {
                    holder.OnPickup();
                }
            }
        }


        #region test methods

        // 테스트 메소드 (나무 비용 없이)
        public void BuildBridgeWithoutCost()
        {
            if (IsEnd) return;
            BridgeObjects[CurrentBridgeCount()].gameObject.SetActive(true);
            OnBridgeBuild?.Invoke();
        }
        
        #endregion
    }
}
