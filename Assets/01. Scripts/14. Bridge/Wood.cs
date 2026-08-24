using UnityEngine;

namespace WeGotOne
{
    public class Wood : MonoBehaviour, IPickupable
    {
        [field: SerializeField]
        public ItemScriptableObject ThisItem {get; private set;}

        public void OnPickup()
        {
            gameObject.SetActive(false);
        }
    }

}

