using System;
using UnityEngine;

namespace WeGotOne
{
    public class FishMoveTester : MonoBehaviour
    {
        private FishMovement fishMovement;
        
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