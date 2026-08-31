using System;
using UnityEngine;



namespace WeGotOne
{   
    [RequireComponent(typeof(Collider2D))]
    public class WaterLayerAlert : MonoBehaviour
    {
        [Header("물 레이어 설정")]
        [SerializeField] private LayerMask waterLayer;
        
        // 현재 물 안에 있는지 여부를 외부에서 읽을 수 있는 프로퍼티
        public bool IsInWater { get; private set; } = true;
        
        // 물에 들어가고 나갈 때 발생하는 알림 이벤트
        public event Action OnEnteredWater;
        public event Action OnExitedWater;

        private void OnTriggerEnter2D(Collider2D collision)
        {   
            // 이 코드에서 숫자 1은 이진수(Binary)로 0000 0001을 의미합니다
            // :왼쪽으로 밀어내기 전의 출발선(첫 번째 비트)을 잡기 위한 기준 숫자
            
            // 부딪힌 오브젝트의 레이어가 물 레이어인지 확인
            if (((1 << collision.gameObject.layer) & waterLayer) != 0 && enabled)
            {
                IsInWater = true;
                OnEnteredWater?.Invoke();// 물에 들어와있음 알림 전송
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            // 벗어난 오브젝트의 레이어가 물 레이어인지 확인
            if (((1 << collision.gameObject.layer) & waterLayer) != 0 && enabled)
            {
                IsInWater = false;
                Debug.Log($"{collision.name} 이 물 밖으로 나왔다");
                OnExitedWater?.Invoke();
            }
        }

    }
}