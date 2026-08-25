using UnityEngine;

namespace WeGotOne
{
    public class VictoryZoneTester : MonoBehaviour
    {
        [SerializeField] private int testTotalPlayerCount = 4;

        [ContextMenu("Build Bridge")]
        public void BuildBridge()
        {
            BridgeManager.Instance.BuildBridgeWithoutCost();
        }
        
        [ContextMenu("Set Total Player Count")]
        public void SetTotalPlayerCount()
        {
            GameManager.Instance.SetTotalPlayerCount(testTotalPlayerCount);
        }
    }
}