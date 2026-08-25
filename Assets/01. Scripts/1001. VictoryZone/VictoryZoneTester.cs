using UnityEngine;

namespace WeGotOne
{
    public class VictoryZoneTester : MonoBehaviour
    {
        [SerializeField] private GameObject[] bridgeObjects;
        [SerializeField] private int testTotalPlayerCount = 4;

        [ContextMenu("Build Bridge")]
        public void BuildBridge()
        {
            BridgeManager.Instance.BuildBridgeWithoutCost();
        }
        
        [ContextMenu("Deactivate All Bridge Blocks")]
        public void DeactivateAllBridgeBlocks()
        {
            foreach (var bridgeObject in bridgeObjects)
            {
                bridgeObject.SetActive(false);
            }
        }
        
        [ContextMenu("Set Total Player Count")]
        public void SetTotalPlayerCount()
        {
            GameManager.Instance.SetTotalPlayerCount(testTotalPlayerCount);
        }
    }
}