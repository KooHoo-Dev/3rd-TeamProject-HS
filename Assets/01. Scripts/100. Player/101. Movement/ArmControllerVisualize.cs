using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace WeGotOne
{
    public class ArmControllerVisualizer : MonoBehaviour
    {
        private const float ArrowLength = 1.2f;
        private const float ArrowHeadLength = 0.22f;
        private const float ArrowHeadWidth = 0.12f;
        private const float ZeroDirectionRadius = 0.08f;

        [SerializeField] private List<ArmController> controllers = new List<ArmController>();
        [SerializeField] private bool isDebugMode = true;
        
        private void Awake()
        {
            FindArmControllers();
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!isDebugMode || controllers.Count == 0)
                return;

            foreach (ArmController controller in controllers)
            {
                DrawInputDirectionGizmos(controller);
            }
        }

        private void DrawInputDirectionGizmos(ArmController controller)
        {
            Vector2 inputDirection = controller.InputDirection;
            Vector3 origin = controller.transform.position;

            Handles.color =
                inputDirection == Vector2.zero
                    ? Color.gray
                    : Color.cyan;

            Handles.Label(
                origin + Vector3.up * 0.15f,
                $"InputDirection: ({inputDirection.x:0.00}, {inputDirection.y:0.00})"
            );

            if (inputDirection == Vector2.zero)
            {
                Handles.DrawWireDisc(
                    origin,
                    Vector3.forward,
                    ZeroDirectionRadius
                );

                return;
            }

            Vector3 direction = inputDirection.normalized;
            Vector3 end = origin + direction * ArrowLength;
            Vector3 perpendicular =
                new(-direction.y, direction.x, 0f);

            Handles.DrawAAPolyLine(3f, origin, end);

            Handles.DrawAAPolyLine(
                3f,
                end,
                end - direction * ArrowHeadLength
                + perpendicular * ArrowHeadWidth
            );

            Handles.DrawAAPolyLine(
                3f,
                end,
                end - direction * ArrowHeadLength
                    - perpendicular * ArrowHeadWidth
            );
        }

#endif

        [ContextMenu("Find ArmController")]
        private void FindArmControllers()
        {
            controllers = new List<ArmController>(FindObjectsByType<ArmController>
                (FindObjectsInactive.Include, FindObjectsSortMode.InstanceID));
        }
    }
}