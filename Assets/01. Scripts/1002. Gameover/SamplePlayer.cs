using System;
using UnityEngine;

namespace WeGotOne
{
    public class SamplePlayer : PlayerHunger
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