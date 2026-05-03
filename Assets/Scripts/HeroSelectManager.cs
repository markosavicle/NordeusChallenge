using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// HeroSelectManager handles hero creation and startup.
    /// Players enter a hero name and confirm to proceed to MonsterSelect.
    /// </summary>
    public class HeroSelectManager : MonoBehaviour
    {
        [SerializeField] private TMP_InputField heroNameInput;
        [SerializeField] private Button startRunButton;
        [SerializeField] private Button backButton;

        private void Start()
        {
            if (startRunButton != null)
                startRunButton.onClick.AddListener(OnStartRunPressed);
            if (backButton != null)
                backButton.onClick.AddListener(OnBackPressed);
        }

        public void OnStartRunPressed()
        {
            if (heroNameInput == null || string.IsNullOrWhiteSpace(heroNameInput.text))
            {
                Debug.LogWarning("[HeroSelectManager] Hero name is required!");
                return;
            }

            string heroName = heroNameInput.text;
            Debug.Log($"[HeroSelectManager] Starting run with hero: {heroName}");

            // Create hero and start run
            Hero hero = new Hero { name = heroName };
            
            // Give hero starter moves
            MoveScriptableObject slashMove = Resources.Load<MoveScriptableObject>("Moves/Slash");
            MoveScriptableObject shieldMove = Resources.Load<MoveScriptableObject>("Moves/Shield");
            
            if (slashMove != null)
            {
                hero.LearnMove(slashMove);
                hero.EquipMoveToSlot(0, slashMove);
            }
            else
            {
                Debug.LogWarning("[HeroSelectManager] Slash move not found in Resources/Moves/");
            }
            
            if (shieldMove != null)
            {
                hero.LearnMove(shieldMove);
                hero.EquipMoveToSlot(1, shieldMove);
            }
            else
            {
                Debug.LogWarning("[HeroSelectManager] Shield move not found in Resources/Moves/");
            }
            
            if (RunManager.Instance != null)
            {
                RunManager.Instance.StartNewRun(hero);
                Debug.Log($"[HeroSelectManager] RunManager initialized with {heroName}");
            }
            else
            {
                Debug.LogError("[HeroSelectManager] RunManager.Instance is null!");
            }

            // Proceed to monster selection
            SceneManager.LoadScene("MonsterSelect");
        }

        public void OnBackPressed()
        {
            Debug.Log("[HeroSelectManager] Back pressed");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
