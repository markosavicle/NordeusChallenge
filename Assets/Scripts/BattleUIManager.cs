using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Unity.VectorGraphics;

namespace NordeusChallenge.Unity
{
    public class BattleUIManager : MonoBehaviour
    {
        [Header("Battle UI")]
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private Image heroHPBar;
        [SerializeField] private TextMeshProUGUI monsterNameText;
        [SerializeField] private Image monsterHPBar;
        [SerializeField] private TextMeshProUGUI battleLogText;
        [SerializeField] private Button[] moveButtons;
        [SerializeField] private SVGImage monsterGraphic;

        [Header("Results Overlay")]
        [SerializeField] private CanvasGroup resultsOverlay;
        [SerializeField] private TextMeshProUGUI resultsText;
        [SerializeField] private Button continueButton;

        private BattleStateMachine battleStateMachine;
        private Hero hero;
        private Monster monster;

        private void Start()
        {
            battleStateMachine = GetComponent<BattleStateMachine>();
            if (battleStateMachine == null)
                battleStateMachine = Object.FindFirstObjectByType<BattleStateMachine>();

            hero = RunManager.Instance?.Hero;
            monster = RunManager.Instance?.CurrentMonster;

            if (hero == null || monster == null)
            {
                Debug.LogError("[BattleUIManager] Hero or Monster is null!");
                return;
            }

            if (monster != null && monsterGraphic != null && monster.template != null)
            {
                // Assign the sprite from the monster's template
                monsterGraphic.sprite = monster.template.monsterIcon;
                
                // Optional: Ensure it maintains the correct aspect ratio
                monsterGraphic.preserveAspect = true;
            }

            // --- PRO FIX: SUBSCRIBE TO STAT CHANGES ---
            // Now the UI "listens" to the data. No more FindObjectOfType in Entity.cs!
            hero.OnStatsChanged += RefreshStatsDisplay;
            monster.OnStatsChanged += RefreshStatsDisplay;

            if (battleStateMachine != null)
            {
                battleStateMachine.OnBattleLogUpdated.AddListener(OnBattleLogUpdated);
                battleStateMachine.OnHeroTurnStart.AddListener(OnHeroTurnStart);
                battleStateMachine.OnMonsterTurnStart.AddListener(OnMonsterTurnStart);
                battleStateMachine.OnBattleEnd.AddListener(OnBattleEnd);
            }

            if (resultsOverlay != null)
                resultsOverlay.alpha = 0;
            
            if (continueButton != null)
                continueButton.onClick.AddListener(OnContinuePressed);

            RefreshStatsDisplay();
            PopulateMoveButtons();

            if (battleStateMachine != null)
                battleStateMachine.StartBattle();
        }

        // --- PRO FIX: CLEANUP ---
        // Always unsubscribe from C# actions to prevent memory leaks and ghost updates
        private void OnDestroy()
        {
            if (hero != null) hero.OnStatsChanged -= RefreshStatsDisplay;
            if (monster != null) monster.OnStatsChanged -= RefreshStatsDisplay;
        }

        // REMOVED: Update() logic for HP bars. 
        // We handle this inside RefreshStatsDisplay now for better performance.

        public void PopulateMoveButtons()
        {
            if (hero == null) hero = RunManager.Instance?.Hero;
            if (hero == null) return;

            var battleMoves = hero.GetBattleMoves(); 

            for (int i = 0; i < moveButtons.Length; i++)
            {
                if (i < battleMoves.Count && battleMoves[i] != null)
                {
                    moveButtons[i].interactable = true;
                    moveButtons[i].gameObject.SetActive(true);
                    moveButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = battleMoves[i].name;
                    moveButtons[i].onClick.RemoveAllListeners();
                    var move = battleMoves[i];
                    moveButtons[i].onClick.AddListener(() => battleStateMachine.ExecuteHeroMove(move));
                }
                else
                {
                    moveButtons[i].gameObject.SetActive(false);
                }
            }
        }

        public void RefreshStatsDisplay()
        {
            // Update Hero UI
            if (hero != null)
            {
                if (heroNameText != null)
                    heroNameText.text = $"{hero.Name} - HP: {hero.Hp}/{hero.MaxHp}";
                
                if (heroHPBar != null)
                    heroHPBar.fillAmount = (float)hero.Hp / hero.MaxHp;
            }

            // Update Monster UI
            if (monster != null)
            {
                if (monsterNameText != null)
                    monsterNameText.text = $"{monster.Name} - HP: {monster.Hp}/{monster.MaxHp}";

                if (monsterHPBar != null)
                    monsterHPBar.fillAmount = (float)monster.Hp / monster.MaxHp;
            }
        }

        public void OnBattleLogUpdated(string message)
        {
            if (battleLogText != null)
                battleLogText.text = message;
        }

        public void OnHeroTurnStart()
        {
            foreach (var button in moveButtons)
                if (button != null) button.interactable = true;
        }

        public void OnMonsterTurnStart()
        {
            foreach (var button in moveButtons)
                if (button != null) button.interactable = false;

            if (battleStateMachine != null)
                Invoke(nameof(ExecuteMonsterMove), 1.5f);
        }

        private void ExecuteMonsterMove()
        {
            if (battleStateMachine != null)
                battleStateMachine.ExecuteMonsterMove();
        }

        public void OnBattleEnd(string winnerName)
        {
            foreach (var button in moveButtons)
                if (button != null) button.interactable = false;

            StartCoroutine(ShowResultsOverlay(winnerName));
        }

        private IEnumerator ShowResultsOverlay(string winnerName)
        {
            yield return new WaitForSeconds(1.5f);

            bool heroWon = winnerName == hero.Name;
            string resultMessage;
            if (heroWon)
            {
                int xpGained = monster.XpReward; 
                resultMessage = $"VICTORY!\n\nXP Gained: {xpGained}\nLevel: {hero.level}";
                
                // Use the proper flag from RunManager
                if (RunManager.Instance.DidLevelUpLastBattle)
                {
                    resultMessage += "\n<color=yellow>LEVEL UP!</color>";
                }

                string learnedMove = RunManager.Instance.LastLearnedMoveName;
                if (!string.IsNullOrEmpty(learnedMove))
                {
                    resultMessage += $"\n<color=cyan>Learned: {learnedMove.Trim()}</color>";
                }
            }
            else
            {
                resultMessage = $"DEFEAT!\n\n{monster.Name} wins this battle.";
            }

            if (resultsText != null)
            {
                resultsText.text = resultMessage;
            }

            yield return StartCoroutine(FadeInOverlay());
        }

        private IEnumerator FadeInOverlay()
        {
            if (resultsOverlay == null) yield break;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                resultsOverlay.alpha = Mathf.Clamp01(elapsed / duration);
                yield return null;
            }
            resultsOverlay.alpha = 1f;
        }

        public void OnContinuePressed()
        {
            SceneManager.LoadScene("MonsterSelect");
        }
    }
}