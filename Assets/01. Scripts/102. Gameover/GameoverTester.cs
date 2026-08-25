using System.Collections.Generic;
using UnityEngine;

namespace WeGotOne
{
    public class GameoverTester : MonoBehaviour
    {
        // SamplePlayer들을 가지고 있고
        // 하나씩 순서대로 무력화, 활성화 가능
        [SerializeField] private List<SamplePlayer> playerList = new List<SamplePlayer>();
        
        private void Start()
        {
            SamplePlayer[] players = GetComponentsInChildren<SamplePlayer>();
            for (int i = 0; i < players.Length; ++i)
            {
                playerList.Add(players[i]);
            }
            GameManager.Instance.SetTotalPlayerCount(playerList.Count);
        }

        [ContextMenu("Activate Any Player")]
        public void ActivateAnyPlayer()
        {
            for (int i = 0; i < playerList.Count; i++)
            {
                if (playerList[i].IsOutOfEnergy)
                {
                    playerList[i].SetPlayerActive();
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
                if (playerList[i].IsOutOfEnergy == false)
                {
                    playerList[i].SetPlayerOutOfEnergy();
                    Debug.Log($"active - {GameManager.Instance.ActivePlayerCount}, outOfEnergy - {GameManager.Instance.OutOfEnergyPlayerCount}");
                    return;
                }
            }
            Debug.Log($"All players are out of energy: active - {GameManager.Instance.ActivePlayerCount}, outOfEnergy - {GameManager.Instance.OutOfEnergyPlayerCount}");
        }
        
        [ContextMenu("Set Total Player Count")]
        public void SetTotalPlayerCount()
        {
            GameManager.Instance.SetTotalPlayerCount(playerList.Count);
        }
    }
}