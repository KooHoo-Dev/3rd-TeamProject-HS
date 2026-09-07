using System.Collections.Generic;
using UnityEngine;

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
        private float nextRefreshTime;
        
        private void Awake()
        {
            FindArmControllers();
        }

        private void Update()
        {
            if (IntegratedDebugUI.ShowArmDebug && Time.unscaledTime >= nextRefreshTime)
            {
                FindArmControllers();
                nextRefreshTime = Time.unscaledTime + 0.5f;
            }
        }

        // 통합 UI가 영역과 스크롤 상태를 소유하므로 독립 OnGUI 창과 충돌하지 않습니다.
        public static void DrawPanel(Rect area, ArmController[] arms, ref Vector2 scrollPosition,
            GUIStyle headingStyle, GUIStyle textStyle)
        {
            GUILayout.BeginArea(area, GUI.skin.box);
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            GUILayout.Label("ARM INPUT DEBUG", headingStyle);
            if (arms.Length == 0)
                GUILayout.Label("No active arms. Join the room to spawn players.", textStyle);

            foreach (ArmController arm in arms)
            {
                // 퇴장으로 파괴된 팔은 다음 목록 갱신 전에 사라질 수 있습니다.
                if (arm == null)
                    continue;

                PlayerController player = arm.GetComponentInParent<PlayerController>();
                string owner = player != null && player.User != null ? player.User.NickName : arm.transform.root.name;
                GUILayout.Label($"{owner} / {arm.name}", textStyle);
                GUILayout.Label($"InputDirection: ({arm.InputDirection.x:0.00}, {arm.InputDirection.y:0.00})", textStyle);
                Rect diagram = GUILayoutUtility.GetRect(100f, 76f, GUILayout.ExpandWidth(true));
                DrawDirection(diagram, arm.InputDirection);
                GUILayout.Space(8);
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private static void DrawDirection(Rect area, Vector2 direction)
        {
            Vector2 origin = area.center;
            Color previousColor = GUI.color;
            GUI.color = Color.gray;
            DrawLine(origin - Vector2.right * 32, origin + Vector2.right * 32);
            DrawLine(origin - Vector2.up * 32, origin + Vector2.up * 32);
            if (direction == Vector2.zero)
            {
                GUI.DrawTexture(new Rect(origin.x - 3, origin.y - 3, 6, 6), Texture2D.whiteTexture);
            }
            else
            {
                GUI.color = Color.cyan;
                Vector2 forward = new Vector2(direction.x, -direction.y).normalized;
                Vector2 tip = origin + forward * 30;
                Vector2 side = new Vector2(-forward.y, forward.x);
                DrawLine(origin, tip);
                DrawLine(tip, tip - forward * 10 + side * 6);
                DrawLine(tip, tip - forward * 10 - side * 6);
            }
            GUI.color = previousColor;
        }

        private static void DrawLine(Vector2 start, Vector2 end)
        {
            Matrix4x4 previousMatrix = GUI.matrix;
            Vector2 delta = end - start;
            GUIUtility.RotateAroundPivot(Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg, start);
            GUI.DrawTexture(new Rect(start.x, start.y - 1, delta.magnitude, 2), Texture2D.whiteTexture);
            GUI.matrix = previousMatrix;
        }

#if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (!IntegratedDebugUI.ShowArmDebug || controllers.Count == 0)
                return;

            foreach (ArmController controller in controllers)
            {
                if (controller != null)
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
