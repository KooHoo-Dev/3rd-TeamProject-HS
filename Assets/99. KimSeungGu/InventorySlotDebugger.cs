using UnityEngine;
using UnityEngine.InputSystem;

namespace Study_Inventory
{
    public class InventorySlotDebugger : MonoBehaviour
    {
        public InventorySlot Slot;
        public InventoryItem TestItem;

        private void Start()
        {
            Slot.SetItem(null);
        }

        private void Update()
        {
            Slot.SetItem(TestItem);
            
            
            Slot.SetItem(null);
        }
    }

}

