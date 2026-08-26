using System;
using UnityEngine;

namespace WeGotOne
{   
    [RequireComponent(typeof(Collider2D))]
    public class FishCollisionHandler : MonoBehaviour
    {
        // 물고기 지형 충돌 감지를 담당
        // 물리적인 충격(relativeVelocity) 을 감지하고,
        // 지정된 레이어 및 임계치 조건 만족 시 이벤트 발생


        
        [Header("지형 충돌 설정")]
        [SerializeField] 
        [Tooltip("충돌을 감지할 지형(장애물) 레이어")]
        private LayerMask terrainLayer;

        [SerializeField] [Tooltip("저항도가 깎이기 시작하는 최소 충격 강도")]
        private float minImpactThreshold = 5f;
        
        // 충격 강도 (relativeVelocity.magnitude) 를 외부에 전달할 이벤트
        public event Action<float> OnTerrainSmashed;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            // 충돌한 오브젝트가 설정된 지형 레이어에 포함되는 지 검사
            if (((1 << collision.gameObject.layer) & terrainLayer) == 0) return;
            
            // 충돌 순간의 상대 속도 차이( 충격 강도) 계산
            float impactStrengh = collision.relativeVelocity.magnitude;
            
            // 단순 스침이 아닌 유의미한 충격일 때만 신호 발송
            if (impactStrengh >= minImpactThreshold)
            {
                OnTerrainSmashed?.Invoke(impactStrengh);
            }
        }
    }
}