using UnityEngine;
using HelloServer;

namespace WeGotOne
{
    public class Plank : Collectible
    {
        public void ApplySnapshot(PlankState state)
        {
            bool wasActive = gameObject.activeSelf;

            gameObject.SetActive(state.IsActive);
            IsPickUp = state.IsPickUp;

            if (state.IsActive == false)
            {
                ResetTransformSnapshot();
                return;
            }

            ApplyTransformSnapshot(
                new Vector2(state.X, state.Y),
                Quaternion.Euler(0f, 0f, state.Angle),
                wasActive == false);
        }

        public override void OnPickup()
        {
            IsPickUp = true;
            base.OnPickup();
        }
    }
}
