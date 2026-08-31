using System;
using UnityEngine;

namespace WeGotOne
{
    // 테스트 씬을 위한 테스트 코드 수정... (PlayerHunger 상속으로 GameoverTester 유지)
    public class SamplePlayer : PlayerStamina
    {
        public bool IsStarved { get; set; } = false;

        #region unity events

        private void OnEnable()
        {
            GameManager.Instance.SubscribePlayerStarveStateChanged(this);
        }
        
        private void OnDisable()
        {
            GameManager.Instance.UnsubscribeStarveStateChanged(this);
        }

        #endregion
    }
}