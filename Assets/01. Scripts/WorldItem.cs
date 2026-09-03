using UnityEngine;

namespace WeGotOne
{
    public class WorldItem : MonoBehaviour
    {
        private const float SnapshotInterval = 1f / 20f;

        [field: SerializeField] public ItemScriptableObject ThisItem { get; private set; }

        public int ItemId => ThisItem != null ? ThisItem.Key : -1;
        public int WorldItemId { get; private set; } = -1;
        public bool IsPickUp { get; private set; }

        private bool isSnapshotInitialized;
        private float interpolationTime;
        private Vector2 previousSnapshotPosition;
        private Quaternion previousSnapshotRotation;
        private Vector2 targetSnapshotPosition;
        private Quaternion targetSnapshotRotation;

        private void Update()
        {
            if (isSnapshotInitialized == false)
                return;

            interpolationTime += Time.deltaTime;

            float t = Mathf.Clamp01(interpolationTime / SnapshotInterval);
            transform.position = new Vector3(
                Mathf.Lerp(previousSnapshotPosition.x, targetSnapshotPosition.x, t),
                Mathf.Lerp(previousSnapshotPosition.y, targetSnapshotPosition.y, t),
                transform.position.z);
            transform.rotation = Quaternion.Lerp(previousSnapshotRotation, targetSnapshotRotation, t);
        }

        public void AssignWorldItemId(int worldItemId)
        {
            WorldItemId = worldItemId;
        }

        // 안 쓸거 같은데 일단 유지
        // 나중에 집고 있을때 추가효과 줄때 활용 가능
        public void SetIsPickUp(bool value)
        {
            IsPickUp = value;
        }

        public void ApplyTransformSnapshot(Vector2 position, Quaternion rotation, bool applyImmediately)
        {
            if (applyImmediately || isSnapshotInitialized == false)
            {
                transform.position = new Vector3(position.x, position.y, transform.position.z);
                transform.rotation = rotation;

                previousSnapshotPosition = position;
                previousSnapshotRotation = rotation;
                targetSnapshotPosition = position;
                targetSnapshotRotation = rotation;

                interpolationTime = SnapshotInterval;
                isSnapshotInitialized = true;
                return;
            }

            previousSnapshotPosition = targetSnapshotPosition;
            previousSnapshotRotation = targetSnapshotRotation;
            targetSnapshotPosition = position;
            targetSnapshotRotation = rotation;
            interpolationTime = 0f;
        }

        public void ResetTransformSnapshot()
        {
            isSnapshotInitialized = false;
            interpolationTime = 0f;
        }

        public void DeactivateFromSnapshot()
        {
            ResetTransformSnapshot();
            gameObject.SetActive(false);
        }

        public void OnPickup()
        {
            IsPickUp = true;
            ResetTransformSnapshot();
            gameObject.SetActive(false);
            FishObjectPool.Instance.Release(this);
        }
    }
}
