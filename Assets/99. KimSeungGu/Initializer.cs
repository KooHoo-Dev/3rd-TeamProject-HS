using UnityEngine;

namespace Study_Inventory
{
    public class User
    {
        public static User Instance { get; private set; }

        public string Name { get; private set; }
        public int Money { get; private set; }

        private User(string name, int money)
        {
            Name = name;
            Money = money;
        }

        // 싱글톤 초기화: MainSystem에서 한 번만 호출합니다.
        public static void Initialize(string name, int money)
        {
            Instance = new User(name, money);
        }

        public bool CanAfford(int amount)
        {
            return Money >= amount;
        }

        public void IncreaseMoney(int amount)
        {
            Money += amount;
        }

        // 함수를 따로 두는 이유:
        // 1) price를 음수로 입력하는 인터페이스를 좋아하지 않음
        // 2) 의외로 차감 시 디테일한 처리(검증/로그 등)가 필요할 때가 있음
        public void DecreaseMoney(int amount)
        {
            Money -= amount;
        }
    }

    // 초기화 진행을 위해 존재하는 컴포넌트입니다
    // 여러분이 만드실 게임에서는 MainSystem에서 User를 초기화 하세요
    public class Initializer : MonoBehaviour
    {
        private void Awake()
        {
            // 가상의 거래를 진행할 유저를 생성합니다.
            User.Initialize("Jay", 9999);
        }
    }

}

