using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    public abstract class Collectible : MonoBehaviour
    {
        [field: SerializeField] public ItemScriptableObject ThisItem { get; private set; }
        
        // 물고기나 나무 현재 붙잡고 있음을 표시하기 위한 용도인가?
        // 일단 활용가능한 필드이니 그대로 유지
        public bool IsPickUp { get; set; } = false;

        private const float SnapshotInterval = 1f / 20f;

        private bool isSnapshotInitialized;
        private float interpolationTime;
        private Vector2 previousSnapshotPosition;
        private Quaternion previousSnapshotRotation;
        private Vector2 targetSnapshotPosition;
        private Quaternion targetSnapshotRotation;

        private void Update()
        {
            // 일단 비활성화 하겠습니다.
            // if (SimpleInput.GetKeyDown(Key.Digit2))
            //     OnPickup();

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

        protected void ApplyTransformSnapshot(Vector2 position, Quaternion rotation, bool applyImmediately)
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

        protected void ResetTransformSnapshot()
        {
            isSnapshotInitialized = false;
            interpolationTime = 0f;
        }

        public virtual void OnPickup()
        {
            gameObject.SetActive(false);
        }
    }
}
