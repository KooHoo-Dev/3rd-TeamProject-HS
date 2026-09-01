using UnityEngine;
using HelloServer;

namespace WeGotOne
{
    public class Plank : MonoBehaviour
    {
        private WorldItem worldItem;

        private void Awake()
        {
            worldItem = GetComponent<WorldItem>();
        }

        public void ApplySnapshot(PlankState state, bool applyImmediately)
        {
            worldItem.SetIsPickUp(state.IsPickUp);
            worldItem.ApplyTransformSnapshot(
                new Vector2(state.X, state.Y),
                Quaternion.Euler(0f, 0f, state.Angle),
                applyImmediately);
        }
    }
}
