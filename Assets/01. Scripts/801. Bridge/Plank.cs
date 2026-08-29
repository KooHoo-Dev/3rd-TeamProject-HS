using UnityEngine;

namespace WeGotOne
{
    public class Plank : Collectible
    {
        [field: SerializeField] public bool IsPickUp { get; set; } = false;

        public override void OnPickup()
        {
            IsPickUp = true;
            base.OnPickup();
        }
    }
}