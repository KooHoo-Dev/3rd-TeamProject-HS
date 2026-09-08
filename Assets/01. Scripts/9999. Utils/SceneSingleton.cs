using UnityEngine;

namespace WeGotOne
{
    public abstract class SceneSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        protected static T instance;
        private static readonly object lockObj = new object();
        private static bool isInitialized = false;

        public static bool IsInitialized => isInitialized && instance != null;

        public static T Instance => instance;

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = this as T;
                // DontDestroyOnLoad(gameObject);

                OnInitialize();
                isInitialized = true;
            }
            else if (instance != this)
            {
                Debug.LogWarning($"[Singleton] 중복 인스턴스 감지됨: {gameObject.name} 파괴");
                Destroy(gameObject);
            }
        }

        // "종료 중"을 표시하는 곳은 여기 하나뿐이다.
        // (게임 종료 시 유니티가 OnDestroy 보다 먼저 불러 준다)
        protected virtual void OnApplicationQuit()
        {
        }

        protected virtual void OnDestroy()
        {
            if (instance == this)
            {
                OnDispose();
                instance = null;
                isInitialized = false;

                // 여기서 "종료 중"으로 표시하면 안 된다.
                // 씬을 옮기느라 파괴된 것뿐일 수도 있고, 그때는 다음 씬에서 다시 만들어져야 한다.
            }
        }

        /// <summary>
        /// 싱글톤 초기화 시점에서 호출됨 (오버라이드 가능)
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 싱글톤 제거 시점에서 호출됨 (오버라이드 가능)
        /// </summary>
        protected virtual void OnDispose() { }

        /// <summary>
        /// 현재 인스턴스를 강제로 제거하고 다시 초기화 가능 상태로 되돌림. 다시 싱글톤 호출해야함 (어떤 문제있는지 잘 모름)
        /// </summary>
        public static void ResetInstance()
        {
            if (instance != null)
            {
                (instance as SceneSingleton<T>)?.OnDispose();

#if UNITY_EDITOR
                DestroyImmediate((instance as MonoBehaviour)?.gameObject);
#else
                Destroy((instance as MonoBehaviour)?.gameObject);
#endif
                instance = null;
                isInitialized = false;
            }
        }
    }
}
