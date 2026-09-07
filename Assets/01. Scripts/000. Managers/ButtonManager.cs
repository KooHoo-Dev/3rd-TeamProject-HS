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
        public void OnMatching()
        {
            if(gameCanvas != null)
                gameCanvas.SetActive(true);
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