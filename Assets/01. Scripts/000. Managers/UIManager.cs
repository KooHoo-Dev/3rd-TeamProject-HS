using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private GameObject inventoryUI;
        
        private void Awake()
        {
            inventoryUI.SetActive(false);
        }

        private void LateUpdate()
        {
            bool isInventoryOpen = SimpleInput.GetKey(Key.Tab);
            inventoryUI.SetActive(isInventoryOpen);
        }
    }
}