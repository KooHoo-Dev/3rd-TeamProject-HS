using System;
using UnityEngine;

namespace WeGotOne
{
    public class SamplePlayer : MonoBehaviour
    {
        public event Action<bool> OnPlayerActivationChanged;
        
        public bool IsOutOfEnergy { get; set; } = false;

        #region unity events

        private void OnEnable()
        {
            GameManager.Instance.SubscribePlayerActivationChanged(this);
        }
        
        private void OnDisable()
        {
            GameManager.Instance.UnsubscribePlayerActivationChanged(this);
        }

        #endregion

        #region public methods

        public void SetPlayerActive()
        {
            IsOutOfEnergy = false;
            OnPlayerActivationChanged?.Invoke(true);
        }
        
        public void SetPlayerOutOfEnergy()
        {
            IsOutOfEnergy = true;
            OnPlayerActivationChanged?.Invoke(false);
        }

        #endregion
    }
}