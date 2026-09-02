using UnityEngine;

namespace WeGotOne.Fish_Interface
{   
    /// <summary>
    /// 플레이어의 손(HandControllerV2)에 의해 잡히거나, 놓치거나, 풀려날 수 있는 모든 오브젝트의 규약을 정의합니다.
    /// </summary>
    public interface ICatchable
    {
        /// <summary>
        /// 플레이어가 지정된 유예 시간 내에 대상을 성공적으로 붙잡았을 때 호출됩니다.
        /// </summary>
        /// <param name="hand">대상을 붙잡은 플레이어의 손 컴포넌트</param>
        void OnGrabbed();

        /// <summary>
        /// 플레이어가 붙잡고 있던 대상을 놓아주었을 때 호출됩니다.
        /// </summary>
        /// <param name="hand">대상을 놓아준 플레이어의 손 컴포넌트</param>
        void OnReleased();

        /// <summary>
        /// 플레이어의 손이 대상에 접촉했으나 제한 시간 내에 잡기 입력을 하지 못하고 스쳐 지나갔을 때 호출됩니다.
        /// </summary>
        void OnMissed();
    }
}