using System;
using Cysharp.Threading.Tasks;
using HelloServer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WeGotOne
{
    public class OptionManager : SingletonBase<OptionManager>
    {
        [SerializeField] private GameObject optionCanvas;
        [SerializeField] private GameObject mainButton;
        [SerializeField] private GameObject respawnButton;
        
        public float BgmVolume { get; private set; }
        public float SfxVolume { get; private set; }

        public int CurrentWidth { get; private set; }
        public int CurrentHeight { get; private set; }

        public bool RespawnRequest = false;

        private string menu = "MainMenu";
        
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
            }
        }
        
        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        
        public void OpenOption()
        {
            optionCanvas.SetActive(true);
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            UpdateMainButton();
        }
        
        public void CloseOption()
        {
            optionCanvas.SetActive(false);
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

        public void MainMenu()
        {
            // await RoomClient.Instance.QuitGame();
            RoomClient.Instance.QuitGame().Forget();
            SceneManager.LoadScene(menu);
            optionCanvas.SetActive(false);
        }
        
        public async void QuitGame()
        {
            try
            {
                await RoomClient.Instance.QuitGame();
                Application.Quit();
            }
            catch (Exception e)
            {
                throw new Exception("QuitGame Failed", e);
            }
        }
        
        public void RespawnButton()
        {
            RoomClient.Instance.RequestRespawn();
            optionCanvas.SetActive(false);
        }
        
        private void UpdateMainButton()
        {
            string currentSceneName = SceneManager.GetActiveScene().name;
            bool isMainMenu = currentSceneName == menu;

            mainButton.SetActive(!isMainMenu);
        }
    }
}
