using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace WeGotOne._100._Player
{
    public class CheatMod : MonoBehaviour
    {
        [Header("배고픔 설정")]
        [SerializeField] private PlayerHunger 배고픔;
        
        [Header("힘 설정")]
        [SerializeField] private PlayerRoot 플레이어힘;
        [SerializeField] private int 힘 = 0;
        
        [Header("뭐 들어갓는지 확인용. 안넣어도 됨.")]
        [SerializeField] private ArmController 오른상완;
        [field: SerializeField] public float 오른상완에적용중인힘 { get; private set; }
        [SerializeField] private ArmController 오른하완;
        [field: SerializeField] public float 오른하완에적용중인힘 { get; private set; }
        [SerializeField] private ArmController 왼상완;
        [field: SerializeField] public float 왼상완에적용중인힘 { get; private set; }
        [SerializeField] private ArmController 왼하완;
        [field: SerializeField] public float 왼하완에적용중인힘 { get; private set; }
        
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
            
            if (SimpleInput.GetKey(Key.Digit1))
            {
                if (오른상완 != null)
                    오른상완.SetForce(힘);
            }
            
            if (SimpleInput.GetKey(Key.Digit2))
            {
                if (오른하완 != null)
                    오른하완.SetForce(힘);
            }
            
            if (SimpleInput.GetKey(Key.Digit0))
            {
                if (왼상완 != null)
                    왼상완.SetForce(힘);
            }
            
            if (SimpleInput.GetKey(Key.Digit9))
            {
                if (왼상완 != null)
                    왼상완.SetForce(힘);
            }
            
            오른상완에적용중인힘 = 오른상완 != null ? 오른상완.currentForce : 0;
            오른하완에적용중인힘 = 오른하완 != null ? 오른하완.currentForce : 0;
            왼상완에적용중인힘 = 왼상완 != null ? 왼상완.currentForce : 0;
            왼하완에적용중인힘 = 왼하완 != null ? 왼하완.currentForce : 0;
        }

        private void MaxHunger()
        {
            배고픔.IncreaseHunger(100);
            배고픔.SetDecreaseMultiplier(0f);
        }

        private void GetArm()
        {
            ArmController[] armControllers = GetComponentsInChildren<ArmController>();
            // 그냥 하이어라키 순서대로 가져왓어요
            오른상완 = armControllers[0];
            오른하완 = armControllers[1];
            왼상완 = armControllers[2];
            왼하완 = armControllers[3];
        }
    }
}