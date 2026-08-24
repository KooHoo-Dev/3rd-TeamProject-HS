using Study_Inventory;
using Study.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _99._KimSeungGu
{
    public class Player : MonoBehaviour
    {
        private Inventory targetInventory;
        
        private void Start()
        {
            targetInventory = Inventory.Instance;
        }
        
        
    }
}