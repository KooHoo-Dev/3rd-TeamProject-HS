using System;
using UnityEngine;

namespace WeGotOne
{
    public class FishMoveTester : MonoBehaviour
    {
        private FishMovement fishMovement;
        
        [SerializeField]
        private Transform patrolPoint;
        
        private void Awake()
        {
            fishMovement = GetComponent<FishMovement>();
        }

        private void Start()
        {
            fishMovement.spawnPoint = gameObject.transform;
            
            fishMovement.patrolMove();
            
        }
        
    }
}