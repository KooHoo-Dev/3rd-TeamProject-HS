using UnityEngine;

namespace WeGotOne
{
    public class OptionManager : SingletonBase<OptionManager>
    {
        [SerializeField] private GameObject optionCanvas;
        public float BgmVolume { get; private set; }
        public float SfxVolume { get; private set; }

        public int CurrentWidth { get; private set; }
        public int CurrentHeight { get; private set; }

        private void Start()
        {
            BgmVolume = AudioManager.Instance.bgmVolume;
            SfxVolume = AudioManager.Instance.sfxVolume;

            CurrentWidth = Screen.width;
            CurrentHeight = Screen.height;
        }

        private void Update()
        {
            if(SimpleInput.GetKeyDown(UnityEngine.InputSystem.Key.Escape))
            {
                optionCanvas.SetActive(!optionCanvas.activeInHierarchy);
                Time.timeScale = optionCanvas.activeSelf ? 0f : 1f;
            }
        }
        
        public void OpenOption()
        {
            optionCanvas.SetActive(true);
            Time.timeScale = 0f;
        }

        public void CloseOption()
        {
            optionCanvas.SetActive(false);
            Time.timeScale = 1f;
        }

        public void SetBgmVolume(float value)
        {
            BgmVolume = value;
            AudioManager.Instance.SetBgmVolume(value);
        }

        public void SetSfxVolume(float value)
        {
            SfxVolume = value;
            AudioManager.Instance.SetSfxVolume(value);
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}