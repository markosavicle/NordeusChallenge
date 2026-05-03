using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// ResultsScreenManager displays battle outcome and progression.
    /// Shows fight progress, learned moves, hero progression, and next actions.
    /// </summary>
    public class ResultsScreenManager : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI resultText;
        [SerializeField] private TextMeshProUGUI fightCounterText;
        [SerializeField] private TextMeshProUGUI winsText;
        [SerializeField] private TextMeshProUGUI heroStatsText;
        [SerializeField] private TextMeshProUGUI moveLearnedText;

        [SerializeField] private Button nextButton;
        [SerializeField] private Button mainMenuButton;

        private void Start()
        {
            if (nextButton != null)
                nextButton.onClick.AddListener(OnNextPressed);
            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(OnMainMenuPressed);

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            RunManager run = RunManager.Instance;
            if (run == null)
            {
                Debug.LogError("[ResultsScreenManager] RunManager.Instance is null!");
                return;
            }

            Hero hero = run.Hero;
            if (hero == null)
            {
                Debug.LogError("[ResultsScreenManager] Hero is null!");
                return;
            }

            // Show battle result
            int currentFight = run.CurrentFightNumber - 1; // -1 because we just finished
            int totalFights = run.TotalFights;
#pragma warning disable CS0618
            int wins = run.GetWinCount();

            if (resultText != null)
            {
                if (currentFight >= totalFights || !run.RunComplete)
                    resultText.text = "VICTORY!";
                else
                    resultText.text = "ALL BATTLES COMPLETED!";
            }
#pragma warning restore CS0618

            // Show fight progress
            if (fightCounterText != null)
                fightCounterText.text = $"Fight {currentFight}/{totalFights}";

            if (winsText != null)
                winsText.text = $"Wins: {wins}";

            // Show hero progression
            if (heroStatsText != null)
            {
                heroStatsText.text = $"Level: {hero.level}\n" +
                                   $"HP: {hero.MaxHp}\n" +
                                   $"ATK: {hero.Attack}\n" +
                                   $"DEF: {hero.Defense}\n" +
                                   $"SPD: {hero.Speed}\n" +
                                   $"LCK: {hero.Luck}\n" +
                                   $"Moves Learned: {hero.LearnedMoves.Count}";
            }

            // Show last learned move
            if (moveLearnedText != null && hero.LearnedMoves.Count > 0)
            {
                var lastMove = hero.LearnedMoves[hero.LearnedMoves.Count - 1];
                moveLearnedText.text = $"You learned: {lastMove.name}!";
            }

            // Update next button text
            if (nextButton != null)
            {
                var nextButtonText = nextButton.GetComponentInChildren<TextMeshProUGUI>();
                if (nextButtonText != null)
                {
#pragma warning disable CS0618
                    if (currentFight >= totalFights || run.RunComplete)
                        nextButtonText.text = "View Summary";
                    else
                        nextButtonText.text = "Next Fight";
#pragma warning restore CS0618
                }
            }
        }

        public void OnNextPressed()
        {
            RunManager run = RunManager.Instance;
            if (run == null)
            {
                Debug.LogError("[ResultsScreenManager] RunManager is null!");
                return;
            }

#pragma warning disable CS0618
            if (run.RunComplete || run.CurrentFightNumber > run.TotalFights)
            {
                Debug.Log("[ResultsScreenManager] Run complete, showing summary...");
                // TODO: Create a summary scene
                SceneManager.LoadScene("MainMenu");
            }
            else
            {
                Debug.Log($"[ResultsScreenManager] Proceeding to next fight...");
                run.SetupCurrentMonster();
                SceneManager.LoadScene("Battle");
            }
#pragma warning restore CS0618
        }

        public void OnMainMenuPressed()
        {
            Debug.Log("[ResultsScreenManager] Returning to Main Menu");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
