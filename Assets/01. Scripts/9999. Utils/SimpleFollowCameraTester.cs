using UnityEngine;
using UnityEngine.InputSystem;

namespace WeGotOne
{
    [RequireComponent(typeof(SimpleFollowCamera))]
    public class SimpleFollowCameraTester : MonoBehaviour
    {
        private SimpleFollowCamera followCam;

        [Header("추적 타겟 설정")]
        [SerializeField]
        [Tooltip("인스펙터에서 플레이어를 직접 할당하거나, 비워두면 시작 시 자동으로 'Player' 태그를 가진 대상을 찾습니다.")]
        private Transform target;
        
        

        private void Awake()
        {
            followCam = GetComponent<SimpleFollowCamera>();
            
        }
        
        
        

        private void Update()
        {
            if(SimpleInput.GetKeyDown(Key.F1))
            {
                followCam.ChangeState(SimpleFollowCamera.States.Stopped);
            }
            else if (SimpleInput.GetKeyDown(Key.F2))
            {
                followCam.ChangeState(SimpleFollowCamera.States.Holding);
            }
            else if (SimpleInput.GetKeyDown(Key.F3))
            {
                // 2. [보완] 타겟이 확실히 존재하는지 먼저 검증한 뒤 카메라에 할당합니다.
                if (target != null)
                {
                    followCam.Target = target;
                    followCam.ChangeState(SimpleFollowCamera.States.Following);
                }
                else
                {
                    Debug.LogError("[Tester] 카메라 타겟(target)이 지정되지 않아 Following 상태로 전환할 수 없습니다!");
                }
            }
            else if (SimpleInput.GetKeyDown(Key.F4))
            {
                followCam.ChangeState(SimpleFollowCamera.States.Following);
            }
        }
    }

}

