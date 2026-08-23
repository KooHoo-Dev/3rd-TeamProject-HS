using _99._KimSeungGu;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Study_Inventory
{
    public class Wood : MonoBehaviour, IScriptableObjectKind
    {
        [field: SerializeField]
        public ItemScriptableObject ThisItem {get; private set;}

        public void OnPickup()
        {
            gameObject.SetActive(false);
        }
    }

}

