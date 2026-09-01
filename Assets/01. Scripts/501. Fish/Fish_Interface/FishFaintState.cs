using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace WeGotOne.Fish_Interface
{
    public class FishFaintState : IFishState
    {
        private FishStateManager owner;
        private float baseFaintTime = 6.0f;
        private float recoveryOffset; 
        
        private float finalFaintDuration;
        private float faintTimer;
        public void EnterAsync(FishStateManager owner)
        {
            this.owner = owner;
            recoveryOffset = owner.MaxResistance * 0.15f;
            finalFaintDuration = Mathf.Max(1.5f, baseFaintTime - recoveryOffset);
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;
            
            faintTimer += Time.deltaTime;
            
            if (faintTimer >= finalFaintDuration)
            {
                // 정신 차리고 회복
                owner.ResetResistance();
                owner.ChangeState(new FishIdleState());
            }
        }

        public void ExitAsync()
        {
        }
    }
}