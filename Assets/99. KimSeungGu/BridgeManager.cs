using Jay;
using UnityEngine;

namespace _99._KimSeungGu
{
    public class BridgeManager : SingletonBase<BridgeManager>
    {
        [field: SerializeField] public GameObject[] BridgePrefab { get; private set; }
        
        public bool IsEnd => CurrentBridgeCount() == BridgePrefab.Length;
        
        public void BridgeConstruction() => BridgePrefab[CurrentBridgeCount()].SetActive(true);

        public int CurrentBridgeCount()
        {
            int currentCount = 0;

            for (int i = 0; i < BridgePrefab.Length; i++)
            {
                if (BridgePrefab[i].activeSelf)
                {
                    currentCount++;
                }
            }

            return currentCount;
        }
    }
}