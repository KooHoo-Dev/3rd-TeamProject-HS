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

        public void Enter(FishStateManager owner)
        {
            this.owner = owner;
            recoveryOffset = owner.MaxResistance * 0.15f;
            finalFaintDuration = Mathf.Max(1.5f, baseFaintTime - recoveryOffset);
            this.owner.FaintDuration = finalFaintDuration;
            Debug.Log("Faint Enter");
        }

        public void Tick()
        {
            if (owner == null || owner.FishMovement == null) return;

            faintTimer += Time.deltaTime;

            if (faintTimer >= finalFaintDuration)
            {
                Exit();
            }
        }

        public void Exit()
        {
            // 정신 차리고 회복
            owner.ResetResistance();
            owner.ChangeState(new FishIdleState());
        }
    }
}