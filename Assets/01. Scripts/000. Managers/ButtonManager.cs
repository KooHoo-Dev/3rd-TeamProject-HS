using UnityEngine;

namespace WeGotOne
{
    public class ButtonManager : MonoBehaviour
    {
        [SerializeField] private GameObject optionCanvas;
        [SerializeField] private GameObject gameCanvas;
        public void OnMatching()
        {
            gameCanvas.SetActive(true);
        }
        
        public void OnSettings()
        {
            optionCanvas.SetActive(true);
        }
        
        public void OnExit()
        {
            Application.Quit();
        }
    }
}