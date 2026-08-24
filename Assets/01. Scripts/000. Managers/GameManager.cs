using System;
using UnityEngine;

namespace WeGotOne
{
    public class GameManager: SingletonBase<GameManager>
    {
        public bool IsBridgeComplete => BridgeManager.Instance.IsEnd;
        public bool IsAllPlayerArrivedAtLand => arrivedPlayerCount == totalPlayerCount;
        
        [SerializeField] private RectTransform gameOverUI;
        [SerializeField] private RectTransform gameWinUI;
        
        // 게임 중인 전체 플레이어 수
        // 처음에 방 파고 시작할때 설정하도록
        // 지금은 테스트 클래스가 강제로 4로 초기화
        [SerializeField] private int totalPlayerCount;
        
        // VictoryZone(육지)에 도착한 플레이어 수
        [SerializeField] private int arrivedPlayerCount = 0;

        #region unity events

        protected override void OnInitialize()
        {
            base.OnInitialize();
            gameOverUI.gameObject.SetActive(false);
            gameWinUI.gameObject.SetActive(false);
        }
        
        private void OnEnable()
        {
            BridgeManager.Instance.OnBridgeBuild += CheckPlayerWin;
        }

        private void OnDisable()
        {
            BridgeManager.Instance.OnBridgeBuild -= CheckPlayerWin;
        }

        #endregion

        // 지금은 유니티 상으로 주입해서 게임오버, 게임 승리 UI 띄우도록 설정
        // 추후 UI 관련 매니저를 호출하는 방식으로 변경
        // 다리가 완성되고 플레이어 모두가 육지에 도착하면 승리
        private void CheckPlayerWin()
        {
            if (IsBridgeComplete && IsAllPlayerArrivedAtLand)
            {
                gameWinUI.gameObject.SetActive(true);
            }
        }

        #region public methods
        // 패배 처리
        public void HandleLose()
        {
            
        }

        public void SetTotalPlayerCount(int count)
        {
            totalPlayerCount = count;
        }

        public void AddArrivedPlayerCount()
        {
            arrivedPlayerCount++;
            if (IsAllPlayerArrivedAtLand)
            {
                CheckPlayerWin();
            }
        }

        public void RemoveArrivedPlayerCount()
        {
            arrivedPlayerCount--;
            if (arrivedPlayerCount < 0)
            {
                Debug.LogError($"arrivedPlayerCount가 음수로 설정되었습니다. arrivedPlayerCount: {arrivedPlayerCount}. 0으로 재설정.");
                arrivedPlayerCount = 0;
            }
        }
        
        #endregion
    }
}