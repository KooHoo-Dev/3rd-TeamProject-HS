using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace WeGotOne._100._Player
{
    public class CheatMod : MonoBehaviour
    {
        [Header("배고픔 - H 누르면 배고픔 줄지 않음")]
        [Header("배고픔 설정")]
        [SerializeField] private PlayerHunger 배고픔;
        
        [Header("P 눌러서 하단에 팔 제대로 잡히는지 확인하고")]
        [Header("힘에 입력값 넣은 다음에 1,2,3,4로 각 팔에 적용")]
        [Header("힘 설정")]
        [SerializeField] private PlayerRoot 플레이어힘;
        [SerializeField] private int 힘 = 0;
        [SerializeField] private float 중력 = 0f;
        
        [Header("뭐 들어갓는지 확인용. 안넣어도 됨.")]
        [SerializeField] private ArmController 오른상완;
        [field: SerializeField] public float 오른상완에적용중인힘 { get; private set; }
        [field: SerializeField] public float 오른상완에적용중인중력 { get; private set; }
        [SerializeField] private ArmController 오른하완;
        [field: SerializeField] public float 오른하완에적용중인힘 { get; private set; }
        [field: SerializeField] public float 오른하완에적용중인중력 { get; private set; }
        [SerializeField] private ArmController 왼상완;
        [field: SerializeField] public float 왼상완에적용중인힘 { get; private set; }
        [field: SerializeField] public float 왼상완에적용중인중력 { get; private set; }
        [SerializeField] private ArmController 왼하완;
        [field: SerializeField] public float 왼하완에적용중인힘 { get; private set; }
        [field: SerializeField] public float 왼하완에적용중인중력 { get; private set; }
        
        private void Update()
        {
            if (SimpleInput.GetKey(Key.H))
            {
                MaxHunger();
            }
            
            if (SimpleInput.GetKey(Key.P))
            {
                GetArm();
            }
            
            if (SimpleInput.GetKey(Key.Digit0))
            {
                if (오른상완 != null)
                    오른상완.SetForce(힘);
                if (오른하완 != null)
                    오른하완.SetForce(힘);
                if (왼상완 != null)
                    왼상완.SetForce(힘);
                if (왼하완 != null)
                    왼하완.SetForce(힘);
            }
            
            if (SimpleInput.GetKey(Key.Digit9))
            {
                if (오른상완 != null)
                    오른상완.GetComponent<Rigidbody2D>().gravityScale = 중력;
                if (오른하완 != null)
                    오른하완.GetComponent<Rigidbody2D>().gravityScale = 중력;
                if (왼상완 != null)
                    왼상완.GetComponent<Rigidbody2D>().gravityScale = 중력;
                if (왼하완 != null)
                    왼하완.GetComponent<Rigidbody2D>().gravityScale = 중력;
            }
            
            오른상완에적용중인힘 = 오른상완 != null ? 오른상완.currentForce : 0;
            오른하완에적용중인힘 = 오른하완 != null ? 오른하완.currentForce : 0;
            왼상완에적용중인힘 = 왼상완 != null ? 왼상완.currentForce : 0;
            왼하완에적용중인힘 = 왼하완 != null ? 왼하완.currentForce : 0;
            
            오른상완에적용중인중력 = 오른상완 != null ? 오른상완.GetComponent<Rigidbody2D>().gravityScale : 0;
            오른하완에적용중인중력 = 오른하완 != null ? 오른하완.GetComponent<Rigidbody2D>().gravityScale : 0;
            왼상완에적용중인중력 = 왼상완 != null ? 왼상완.GetComponent<Rigidbody2D>().gravityScale : 0;
            왼하완에적용중인중력 = 왼하완 != null ? 왼하완.GetComponent<Rigidbody2D>().gravityScale : 0;
        }

        private void MaxHunger()
        {
            배고픔.IncreaseHunger(100);
            배고픔.SetDecreaseMultiplier(0f);
        }

        private void GetArm()
        {
            ArmController[] armControllers = 플레이어힘.GetComponentsInChildren<ArmController>();
            // 그냥 하이어라키 순서대로 가져왓어요
            오른상완 = armControllers[0];
            오른하완 = armControllers[1];
            왼상완 = armControllers[2];
            왼하완 = armControllers[3];
        }
    }
}