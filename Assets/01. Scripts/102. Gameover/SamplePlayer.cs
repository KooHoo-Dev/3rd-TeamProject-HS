using System;
using UnityEngine;

namespace WeGotOne
{
    public class SamplePlayer : MonoBehaviour
    {
        public event Action OnPlayerOutOfEnergy;
        public event Action OnPlayerActive;
        
        public bool IsOutOfEnergy { get; set; } = false;

        #region unity events

        private void OnEnable()
        {
            GameManager.Instance.SubscribePlayerActiveEvent( this );
            GameManager.Instance.SubscribePlayerInactiveEvent( this );
        }
        
        private void OnDisable()
        {
            GameManager.Instance.UnsubscribePlayerActiveEvent(this);
            GameManager.Instance.UnsubscribePlayerInactiveEvent(this);
        }

        #endregion

        #region public methods

        public void SetPlayerActive()
        {
            IsOutOfEnergy = false;
            OnPlayerActive?.Invoke();
        }
        
        public void SetPlayerOutOfEnergy()
        {
            IsOutOfEnergy = true;
            OnPlayerOutOfEnergy?.Invoke();
        }

        #endregion
    }
}