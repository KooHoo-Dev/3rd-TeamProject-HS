using System.Collections.Generic;
using UnityEngine;

namespace WeGotOne
{
    public class GameoverTester : MonoBehaviour
    {
        // PlayerHunger를 가지고 있고
        // 하나씩 순서대로 무력화, 활성화 가능
        // 테스트하려면 playerList에 플레이어의 PlayerHunger 컴포넌트를 주입해야함
        [SerializeField] private List<PlayerStamina> playerList = new List<PlayerStamina>();
        
        private void Start()
        {
            SetTotalPlayerCount();
        }

        [ContextMenu("Activate Any Player")]
        public void ActivateAnyPlayer()
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].IsStarved)
                {
                    playerList[i].ResetStamina();
                    Debug.Log($"active - {GameManager.Instance.ActivePlayerCount}, outOfEnergy - {GameManager.Instance.OutOfEnergyPlayerCount}");
                    return;
                }
            }
            Debug.Log($"All players are active: active - {GameManager.Instance.ActivePlayerCount}, outOfEnergy - {GameManager.Instance.OutOfEnergyPlayerCount}");
        }
        
        [ContextMenu("Make Any Player Out Of Energy")]
        public void MakeAnyPlayerOutOfEnergy()
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].IsStarved == false)
                {
                    playerList[i].MakeStarved();
                    Debug.Log($"active - {GameManager.Instance.ActivePlayerCount}, outOfEnergy - {GameManager.Instance.OutOfEnergyPlayerCount}");
                    return;
                }
            }
            Debug.Log($"All players are out of energy: active - {GameManager.Instance.ActivePlayerCount}, outOfEnergy - {GameManager.Instance.OutOfEnergyPlayerCount}");
        }
        
        [ContextMenu("Set Total Player Count")]
        public void SetTotalPlayerCount()
        {
            playerList = new List<PlayerStamina>(FindObjectsByType<PlayerStamina>(FindObjectsSortMode.None));
            GameManager.Instance.SetTotalPlayerCount(playerList.Count);
        }
    }
}