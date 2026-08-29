using UnityEditor;
using UnityEngine;

namespace WeGotOne
{
    public static class ArmControllerVisualizer
    {
        private const float ArrowLength = 1.2f;
        private const float ArrowHeadLength = 0.22f;
        private const float ArrowHeadWidth = 0.12f;
        private const float ZeroDirectionRadius = 0.08f;
        
        // 디버그 선 끄고 싶으면 false 로 변경할 것
        private static bool isDrawInputDirection = true;

        [DrawGizmo(GizmoType.Selected | GizmoType.NonSelected)]
                                                
        private static void DrawInputDirection(ArmController controller, GizmoType gizmoType)
        {
            if (isDrawInputDirection == false)
                return;
            
            Vector2 inputDirection = controller.InputDirection;
            Vector3 origin = controller.transform.position;

            Handles.color = inputDirection == Vector2.zero ? Color.gray : Color.cyan;
            Handles.Label(origin + Vector3.up * 0.15f,
                $"InputDirection: ({inputDirection.x:0.00}, {inputDirection.y:0.00})");

            if (inputDirection == Vector2.zero)
            {
                Handles.DrawWireDisc(origin, Vector3.forward, ZeroDirectionRadius);
                return;
            }

            Vector3 direction = inputDirection;
            Vector3 end = origin + direction * ArrowLength;
            Vector3 perpendicular = new Vector3(-direction.y, direction.x, 0f);

            Handles.DrawAAPolyLine(3f, origin, end);
            Handles.DrawAAPolyLine(3f, end, end - direction * ArrowHeadLength + perpendicular * ArrowHeadWidth);
            Handles.DrawAAPolyLine(3f, end, end - direction * ArrowHeadLength - perpendicular * ArrowHeadWidth);
        }
    }
}
