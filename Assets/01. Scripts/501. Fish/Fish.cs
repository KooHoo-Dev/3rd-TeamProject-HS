using UnityEngine;
using HelloServer;

namespace WeGotOne
{
    public class Fish : MonoBehaviour
    {
        private WorldItem worldItem;

        private void Awake()
        {
            worldItem = GetComponent<WorldItem>();
        }

        public void ApplySnapshot(FishState state, bool applyImmediately)
        {
            transform.localScale = new Vector3(state.ScaleX, transform.localScale.y, transform.localScale.z);
            worldItem.ApplyTransformSnapshot(
                new Vector2(state.X, state.Y),
                Quaternion.Euler(0f, 0f, state.Angle),
                applyImmediately);
        }
    }
}
