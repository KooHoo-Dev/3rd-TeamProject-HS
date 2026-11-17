using Cysharp.Threading.Tasks;
using HelloServer;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WeGotOne
{
    public class ButtonManager : MonoBehaviour
    {
        [SerializeField] private GameObject optionCanvas;
        [SerializeField] private GameObject gameCanvas;
        [SerializeField] private string singlePlayNickName = "꼬마돌";

        public void OnMatching()
        {
            if(gameCanvas != null)
                gameCanvas.SetActive(true);
        }

        // 서버 접속 없이 인게임 씬으로 바로 이동해 혼자 플레이한다.
        public void OnSinglePlay()
        {
            RoomClient.Instance.StartSinglePlay(singlePlayNickName, Color.white);
        }
        
        public void OnSettings()
        {
            if(optionCanvas != null)
                optionCanvas.SetActive(true);
        }
        
        public void OnExit()
        {
            Application.Quit();
        }
        
        public void OnMainMenu()
        {
            RoomClient.Instance.QuitGame().Forget();
            SceneManager.LoadScene("MainMenu");
        }
    }
}