using Study_Inventory;
using UnityEngine;

namespace _99._KimSeungGu
{
    public class Fish : MonoBehaviour, IScriptableObjectKind
    {
       [field: SerializeField]
        public ItemScriptableObject ThisItem {get; private set;}

        public void OnPickup()
        {
            gameObject.SetActive(false);
        }
    }
}