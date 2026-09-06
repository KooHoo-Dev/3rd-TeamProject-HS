using System;
using UnityEngine;
using WeGotOne.Fish_Interface;

namespace WeGotOne
{
    public enum SquidPartType { Tentacle, Body }

    [RequireComponent(typeof(Collider2D))]
    public class SquidPartSensor : MonoBehaviour
    {
        [SerializeField] private SquidPartType partType;
        [SerializeField] private FishStateManager manager;

        private void Awake()
        {
            manager = GetComponentInParent<FishStateManager>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            // 1. 다리(Tentacle) 센서: 플레이어 감지
            if (partType == SquidPartType.Tentacle)
            {
                // 충돌체의 부모 계층에서 PlayerRoot 마커 컴포넌트 찾기
                PlayerRoot targetPlayer = other.gameObject.GetComponentInParent<PlayerRoot>();
                
                if (targetPlayer != null)
                {
                    // 매니저에게 플레이어를 잡았다고 알림
                    manager.OnPlayerCaught(targetPlayer.transform);
                }
            }
            // 2. 몸통(Body) 센서: 바닥 감지
            else if (partType == SquidPartType.Body)
            {

            }
        }
    }
}