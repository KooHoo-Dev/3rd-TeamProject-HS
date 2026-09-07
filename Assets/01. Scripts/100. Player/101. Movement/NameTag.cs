using UnityEngine;

namespace WeGotOne
{
    public class NameTag : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 1.5f, 0);

        void LateUpdate()
        {
            transform.position = target.position + offset;
            transform.rotation = Quaternion.identity;
        }
    }
}