using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// MainMenuManager handles the main menu scene.
    /// Displays options to create new game, load game, or exit.
    /// </summary>
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private Button newGameButton;
        [SerializeField] private Button loadGameButton;
        [SerializeField] private Button exitButton;
        [SerializeField] private TextMeshProUGUI titleText;

        private void Start()
        {
            if (newGameButton != null)
                newGameButton.onClick.AddListener(OnNewGamePressed);
            if (loadGameButton != null)
                loadGameButton.onClick.AddListener(OnLoadGamePressed);
            if (exitButton != null)
                exitButton.onClick.AddListener(OnExitPressed);
        }

        public void OnNewGamePressed()
        {
            Debug.Log("[MainMenuManager] New Game pressed");
            // New game flow: Create hero, select monsters, start battle
            // Transition to HeroSelect scene
            SceneManager.LoadScene("HeroSelect");
        }

        public void OnLoadGamePressed()
        {
            Debug.Log("[MainMenuManager] Load Game pressed");
            // Load game flow: Show save slots
            // For now, transition to MainMenu (legacy save system)
            // In production: Create LoadGamePanel with slot selection
            SceneManager.LoadScene("MainMenu");
        }

        public void OnExitPressed()
        {
            Debug.Log("[MainMenuManager] Exit pressed");
            Application.Quit();
        }
    }
}
