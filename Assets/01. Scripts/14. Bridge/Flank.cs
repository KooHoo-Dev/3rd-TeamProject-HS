using UnityEngine;

namespace WeGotOne
{
    public class Flank : MonoBehaviour, IPickupable
    {
        [field: SerializeField]
        public ItemScriptableObject ThisItem {get; private set;}

        public void OnPickup()
        {
            gameObject.SetActive(false);
        }
    }

}

