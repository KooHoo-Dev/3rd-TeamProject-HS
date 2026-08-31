using UnityEngine;
using HelloServer;

namespace WeGotOne
{
    public class Fish : Collectible
    {
        public void ApplySnapshot(FishState state)
        {
            bool wasActive = gameObject.activeSelf;

            transform.localScale = new Vector3(state.ScaleX, transform.localScale.y, transform.localScale.z);
            gameObject.SetActive(state.IsActive);

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
    }
}
