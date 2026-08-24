using System;

namespace WeGotOne
{
    public class PlayerTracker
    {
        // 플레이어 -> 트래커 쪽으로 상태변화를 쏴줘야함
        // 트래커가 플레이어 쪽에 구독을 해야된다
        public int ActivePlayerCount { get; set; }
        public event Action<int> OnActivePlayerCountChanged;
        public void HandlePlayerActivation()
        {
            ActivePlayerCount++;
            OnActivePlayerCountChanged?.Invoke(ActivePlayerCount);
        }

        public void HandlePlayerDeactivation()
        {
            ActivePlayerCount--;
            OnActivePlayerCountChanged?.Invoke(ActivePlayerCount);
        }
    }
}