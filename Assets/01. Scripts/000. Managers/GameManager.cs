using System;
using UnityEngine;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using HelloServer;

namespace WeGotOne
{
    public class GameManager: SceneSingleton<GameManager>
    {
        public bool IsBridgeComplete => BridgeManager.Instance.IsEnd;
        public bool IsAllPlayerArrivedAtLand => arrivedPlayerCount == totalPlayerCount;
        public int OutOfEnergyPlayerCount => totalPlayerCount - activePlayerCount;
        public int ActivePlayerCount => activePlayerCount;
        public int ArrivedPlayerCount => arrivedPlayerCount;
        
        // 플레이어 상태 변화
        [SerializeField] private RectTransform gameOverUI;
        [SerializeField] private RectTransform gameWinUI;
        
        // 게임 중인 전체 플레이어 수
        // 처음에 방 파고 시작할때 설정하도록
        // 지금은 테스트 클래스가 강제로 4로 초기화
        [SerializeField] private int totalPlayerCount;
        
        // VictoryZone(육지)에 도착한 플레이어 수
        [SerializeField] private int arrivedPlayerCount = 0;
        [SerializeField] private int activePlayerCount;
        private readonly HashSet<string> arrivedPlayerIds = new HashSet<string>();
        private bool isGameFinished;
        
        #region unity events

        protected override void OnInitialize()
        {
            base.OnInitialize();
            gameOverUI.gameObject.SetActive(false);
            gameWinUI.gameObject.SetActive(false);
        }

        private async void OnEnable()
        {
            try
            {
                if (CanRunLocalGameLogic == false) return;

                await UniTask.WaitUntil(() => BridgeManager.IsInitialized);
                BridgeManager.Instance.OnBridgeBuild += CheckPlayerWin;
            }
            catch (Exception e)
            {
                throw new Exception("GameManager OnEnable() Failed", e);
            }
        }

        private void OnDisable()
        {
            if (CanRunLocalGameLogic == false) return;
            if (BridgeManager.Instance == null) return;
            
            BridgeManager.Instance.OnBridgeBuild -= CheckPlayerWin;
        }

        #endregion

        private void CheckPlayerWin()
        {
            if (CanRunLocalGameLogic == false) return;

            EvaluateGameEnd();
        }

        private void EvaluateGameEnd()
        {
            if (isGameFinished) return;

            bool isGameOver = activePlayerCount == 0;
            bool isGameWon = IsBridgeComplete && IsAllPlayerArrivedAtLand;

            if (isGameOver && isGameWon)
            {
                Debug.LogError("게임 오버와 승리 조건이 동시에 충족되었습니다. 게임 오버로 처리합니다.");
                gameOverUI.gameObject.SetActive(true);
                isGameFinished = true;
                return;
            }

            if (isGameOver)
            {
                gameOverUI.gameObject.SetActive(true);
                isGameFinished = true;
                return;
            }

            if (isGameWon)
            {
                gameWinUI.gameObject.SetActive(true);
                isGameFinished = true;
            }
        }

        #region public methods
        public void SetTotalPlayerCount(int count)
        {
            totalPlayerCount = count;
            activePlayerCount = count;
        }

        public void ResetNetworkPlayerCounts()
        {
            totalPlayerCount = 0;
            activePlayerCount = 0;
            arrivedPlayerCount = 0;
            arrivedPlayerIds.Clear();
            isGameFinished = false;
            
            if (gameOverUI != null)
            {
                gameOverUI.gameObject.SetActive(false);
            }

            if (gameWinUI != null)
            {
                gameWinUI.gameObject.SetActive(false);
            }
        }

        public void AddPlayer()
        {
            totalPlayerCount++;
            activePlayerCount++;
        }

        public void RemovePlayer()
        {
            totalPlayerCount = Mathf.Max(0, totalPlayerCount - 1);
        }

        public void RemovePlayer(string playerId, bool isStarved, bool isArrived)
        {
            RemovePlayer();
            arrivedPlayerIds.Remove(playerId);

            if (isStarved == false)
            {
                activePlayerCount = Mathf.Max(0, activePlayerCount - 1);
            }

            if (isArrived)
            {
                arrivedPlayerCount = Mathf.Max(0, arrivedPlayerCount - 1);
            }

            EvaluateGameEnd();
        }

        public void ApplySnapshot(int snapshotActivePlayerCount, int snapshotArrivedPlayerCount)
        {
            activePlayerCount = snapshotActivePlayerCount;
            arrivedPlayerCount = snapshotArrivedPlayerCount;

            EvaluateGameEnd();
        }

        public void AddArrivedPlayerCount(string playerId)
        {
            if (CanRunLocalGameLogic == false) return;
            if (string.IsNullOrEmpty(playerId)) return;
            if (arrivedPlayerIds.Add(playerId) == false) return;

            arrivedPlayerCount++;
            EvaluateGameEnd();
        }

        public void RemoveArrivedPlayerCount(string playerId)
        {
            if (CanRunLocalGameLogic == false) return;
            if (string.IsNullOrEmpty(playerId)) return;
            if (arrivedPlayerIds.Remove(playerId) == false) return;

            arrivedPlayerCount--;
            if (arrivedPlayerCount < 0)
            {
                Debug.LogError($"arrivedPlayerCount가 음수로 설정되었습니다. arrivedPlayerCount: {arrivedPlayerCount}. 0으로 재설정.");
                arrivedPlayerCount = 0;
            }
        }

        #endregion

        public void SubscribePlayerStarveStateChanged(PlayerStamina playerStamina)
        {
            playerStamina.OnPlayerStarveStateChanged += HandlePlayerStarveStateChanged;
        }
        
        public void UnsubscribeStarveStateChanged(PlayerStamina playerStamina)
        {
            playerStamina.OnPlayerStarveStateChanged -= HandlePlayerStarveStateChanged;
        }

        private void HandlePlayerStarveStateChanged(bool isStarved)
        {
            if (CanRunLocalGameLogic == false) return;

            if (isStarved)
            {
                activePlayerCount--;
            }
            else
            {
                activePlayerCount++;
            }

            EvaluateGameEnd();
        }

        private bool CanRunLocalGameLogic
        {
            get
            {
                return RoomClient.IsInitialized == false ||
                       RoomClient.Instance.Me == null ||
                       RoomClient.Instance.Me.IsHost;
            }
        }
    }
}
