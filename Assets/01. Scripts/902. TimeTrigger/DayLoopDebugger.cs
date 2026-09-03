namespace WeGotOne
{
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class DayLoopDebugger : MonoBehaviour
    {
        void Update()
        {
            // T 키를 누르면 강제로 밤낮을 토글하여 연출과 동기화를 즉시 테스트
            if (Keyboard.current.tKey.wasPressedThisFrame) 
            {
                var nextType = DayLoop.Instance.CurrentDayType == DayLoop.DayType.Day 
                    ? DayLoop.DayType.Night : DayLoop.DayType.Day;
                DayLoop.Instance.SetDayType(nextType);
                Debug.Log($"[디버그] 강제 밤낮 전환 실행: {nextType}");
            }
        }
    }
}