using System;
using UnityEngine;


namespace WeGotOne
{
    public class FishMovement : MonoBehaviour
    {
        
        // prefab 으로 ObjectPulling 할 경우 위치 값을 세팅해야 하기 때문에 
        // spawn 위치를 배치 된 값으로 정하자.

        public Transform spawnPoint;

        
        private float moveSpeed; // 물고기의 이동 속도
        private Vector3 patrolPoint; // 순찰 포인트
        private float registance; // 저항도
        private Animator anim;
        private Rigidbody rb; 
        
        
        private void Awake()
        {   
            // 물고기의 생성 지점은 배치된 지점
            spawnPoint = gameObject.transform;
            anim = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
        }

       
        public void patrolMove()
        {
            

        }
        
        
        
        
        
        
        


    }
}



