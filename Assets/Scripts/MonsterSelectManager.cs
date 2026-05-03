using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// MonsterSelectManager handles monster selection for battles.
    /// Players click monster buttons to fight that monster.
    /// Includes Hero Stats button to view hero progression between battles.
    /// </summary>
    public class MonsterSelectManager : MonoBehaviour
    {
        [SerializeField] private Button[] monsterButtons;
        [SerializeField] private MonsterScriptableObject[] monsterTemplates;
        [SerializeField] private Button heroStatsButton;
        [SerializeField] private Button backButton;

        private void Start()
        {
            // Validate array lengths match
            if (monsterButtons == null || monsterButtons.Length == 0)
            {
                Debug.LogError("[MonsterSelectManager] No monster buttons assigned!");
                return;
            }

            if (monsterTemplates == null || monsterTemplates.Length != monsterButtons.Length)
            {
                Debug.LogError("[MonsterSelectManager] Monster templates array must match button count!");
                return;
            }

            // Setup monster buttons
            for (int i = 0; i < monsterButtons.Length; i++)
            {
                int index = i; // Local copy for closure
                var button = monsterButtons[i];
                var monsterTemplate = monsterTemplates[i];

                if (button != null && monsterTemplate != null)
                {
                    // Set button text/label to monster name
                    var textComp = button.GetComponentInChildren<TextMeshProUGUI>();
                    if (textComp != null)
                        textComp.text = monsterTemplate.name;

                    // Add click listener
                    button.onClick.AddListener(() => OnMonsterSelected(monsterTemplate));
                }
            }

            if (heroStatsButton != null)
                heroStatsButton.onClick.AddListener(OnHeroStatsPressed);
            if (backButton != null)
                backButton.onClick.AddListener(OnBackPressed);

            Debug.Log("[MonsterSelectManager] Monster select screen ready");
        }

        public void OnMonsterSelected(MonsterScriptableObject monster)
        {
            Debug.Log($"[MonsterSelectManager] Starting battle with {monster.name}");

            // Set up RunManager with current monster
            if (RunManager.Instance != null)
            {
                RunManager.Instance.SetCurrentMonster(monster);
            }

            // Proceed to Battle scene
            SceneManager.LoadScene("Battle");
        }

        public void OnHeroStatsPressed()
        {
            Debug.Log("[MonsterSelectManager] Hero Stats button pressed");
            // Load HeroStats scene
            SceneManager.LoadScene("HeroStats");
        }

        public void OnBackPressed()
        {
            Debug.Log("[MonsterSelectManager] Back pressed");
            SceneManager.LoadScene("HeroSelect");
        }
    }
}
