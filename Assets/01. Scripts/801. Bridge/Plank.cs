using UnityEngine;

namespace WeGotOne
{
    public class Plank : Collectible
    {
        public override void OnPickup()
        {
            IsPickUp = true;
            base.OnPickup();
        }
    }
}