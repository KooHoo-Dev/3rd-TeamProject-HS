using Study_Inventory;
using UnityEngine;

namespace _99._KimSeungGu
{
    public class Fish : MonoBehaviour
    {
       [field: SerializeField]
        public ItemScriptableObject ThisItem {get; private set;}
    }
}