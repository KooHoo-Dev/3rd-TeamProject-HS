using System;
using Cysharp.Threading.Tasks;
using HelloServer;
using UnityEngine;

namespace WeGotOne
{
    [RequireComponent(typeof(Rigidbody2D))]
    public sealed class MineFishController : MonoBehaviour
    {
        [Header("Mine References")]
        [SerializeField] private Transform mine;
        [SerializeField] private Transform ropeAnchor;

        [Header("Collision Ignore Pairs")]
        [SerializeField] private Collider2D[] fishColliders;
        [SerializeField] private Collider2D[] mineColliders;

        [Header("Joint")]
        // [SerializeField] private float ropeLength = 8f;

        private Rigidbody2D fishBody;
        private Rigidbody2D mineBody;
        private DistanceJoint2D mineJoint;
        private LineRenderer mineLineRenderer;

        private Vector3 initialMineLocalPosition;
        private Quaternion initialMineLocalRotation;
        private bool isGuestVisualMode;
        private bool isHostResetPending;

        #region unity methods

        private void Awake()
        {
            fishBody = GetComponent<Rigidbody2D>();
            mineLineRenderer = GetComponent<LineRenderer>();
            mineBody = mine.GetComponent<Rigidbody2D>();
            mineJoint = mine.GetComponent<DistanceJoint2D>();
        
            initialMineLocalPosition = transform.InverseTransformPoint(mine.position);
            initialMineLocalRotation = Quaternion.Inverse(transform.rotation) * mine.rotation;
        
        }
        
        private void OnEnable()
        {
            isGuestVisualMode = false;
            mine.gameObject.SetActive(true);
            mineBody.simulated = false;
            mineJoint.enabled = false;
            isHostResetPending = true;
        }
        
        private void OnDisable()
        {
            isHostResetPending = false;
            mineJoint.enabled = false;
            mineBody.linearVelocity = Vector2.zero;
            mineBody.angularVelocity = 0f;
            mineBody.simulated = false;
            mine.gameObject.SetActive(false);
        }
        
        private async void Start()
        {
            await UniTask.WaitUntil(() => RoomClient.Instance != null && RoomClient.Instance.Me != null && WorldItemRegistry.IsInitialized);
            if (RoomClient.Instance.Me.IsHost)
            {
                EnterHostPhysicsMode();
                WorldItemRegistry.Instance.RegisterNewDynamic(mine.GetComponent<WorldItem>(), out var _);
            }
            else
            {
                EnterGuestVisualMode();
            }
            
            mine.SetParent(null, true);
        }
        
        private void FixedUpdate()
        {
            if (isGuestVisualMode)
                return;
        
            if (isHostResetPending)
            {
                ResetForHostPhysics();
                isHostResetPending = false;
                // return;
            }
        
            // SyncConnectedAnchor();
        }
        
        private void LateUpdate()
        {
            // mineJoint의 anchor의 x, y
            mineLineRenderer.SetPosition(0, transform.position);
            mineLineRenderer.SetPosition(1, mine.transform.position);
        }

        #endregion

        public void EnterGuestVisualMode()
        {
            isGuestVisualMode = true;
            isHostResetPending = false;
            mine.gameObject.SetActive(true);
            mineJoint.enabled = false;
            mineBody.linearVelocity = Vector2.zero;
            mineBody.angularVelocity = 0f;
            mineBody.simulated = false;
        }

        public void EnterHostPhysicsMode()
        {
            isGuestVisualMode = false;
            isHostResetPending = true;
        }

        private void ResetForHostPhysics()
        {
            mine.gameObject.SetActive(true);
            mineBody.simulated = false;
            mineJoint.enabled = false;

            mineBody.position = transform.TransformPoint(initialMineLocalPosition);
            mineBody.rotation = (transform.rotation * initialMineLocalRotation).eulerAngles.z;
            mineBody.linearVelocity = Vector2.zero;
            mineBody.angularVelocity = 0f;
            mineBody.bodyType = RigidbodyType2D.Dynamic;

            mineJoint.autoConfigureConnectedAnchor = false;
            mineJoint.autoConfigureDistance = false;
            mineJoint.maxDistanceOnly = true;
            // mineJoint.distance = ropeLength;
            mineJoint.connectedBody = fishBody;
            SyncConnectedAnchor();
            IgnoreFishMineCollisions();

            mineBody.simulated = true;
            mineJoint.enabled = true;
        }

        private void SyncConnectedAnchor()
        {
            mineJoint.connectedAnchor = fishBody.transform.InverseTransformPoint(ropeAnchor.position);
        }

        private void IgnoreFishMineCollisions()
        {
            foreach (Collider2D fishCollider in fishColliders)
            {
                foreach (Collider2D mineCollider in mineColliders)
                {
                    Physics2D.IgnoreCollision(fishCollider, mineCollider, true);
                }
            }
        }
    }
}
