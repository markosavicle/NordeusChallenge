using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// HeroStatsScreenManager displays and manages hero stats, XP progression, stat allocation, and move equipping.
    /// Left side: Stat allocation with +/- buttons
    /// Right side: Move equipping to 4 battle slots
    /// </summary>
    public class HeroStatsScreenManager : MonoBehaviour
    {
        [Header("Header")]
        [SerializeField] private TextMeshProUGUI heroNameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Button backButton;

        [Header("XP Progress")]
        [SerializeField] private TextMeshProUGUI xpText;
        [SerializeField] private Image xpBar;

        [Header("Stat Allocation - Left Panel")]
        [SerializeField] private TextMeshProUGUI availablePointsText;
        [SerializeField] private TextMeshProUGUI hpText;
        [SerializeField] private TextMeshProUGUI attackText;
        [SerializeField] private TextMeshProUGUI defenseText;
        [SerializeField] private TextMeshProUGUI speedText;
        [SerializeField] private TextMeshProUGUI luckText;
        [SerializeField] private Button[] hpIncreaseButtons;    // +HP buttons
        [SerializeField] private Button[] hpDecreaseButtons;    // -HP buttons
        [SerializeField] private Button[] attackIncreaseButtons; // +ATK buttons
        [SerializeField] private Button[] attackDecreaseButtons; // -ATK buttons
        [SerializeField] private Button[] defenseIncreaseButtons; // +DEF buttons
        [SerializeField] private Button[] defenseDecreaseButtons; // -DEF buttons
        [SerializeField] private Button[] speedIncreaseButtons;  // +SPD buttons
        [SerializeField] private Button[] speedDecreaseButtons;  // -SPD buttons
        [SerializeField] private Button[] luckIncreaseButtons;   // +LCK buttons
        [SerializeField] private Button[] luckDecreaseButtons;   // -LCK buttons
        [SerializeField] private Button confirmStatsButton;
        [SerializeField] private Button resetStatsButton;

        [Header("Move Equipping - Right Panel")]
        [SerializeField] private GameObject moveButtonPrefabNew; // Prefab for learned move buttons
        [SerializeField] private Transform learnedMovesContainer;
        [SerializeField] private Button[] moveSlotButtons; // 4 buttons for the 4 battle slots
        [SerializeField] private TextMeshProUGUI[] moveSlotTexts; // Text components in each slot button

        private Hero hero;
        private int tempAllocatedPoints = 0; // Temp points for current allocation session
    
    // Track temp stat allocations during this session
    private Dictionary<string, int> tempStatAllocations = new Dictionary<string, int>()
    {
        { "hp", 0 },
        { "attack", 0 },
        { "defense", 0 },
        { "speed", 0 },
        { "luck", 0 }
    };
    
    // Track selected slot and move for equipping
    private int selectedSlotForEquip = -1;
    private MoveScriptableObject selectedMoveForEquip = null;
    

        private void Start()
        {
            hero = RunManager.Instance?.Hero;
            if (hero == null)
            {
                Debug.LogError("[HeroStatsScreenManager] Hero is null!");
                return;
            }

            SetupButtons();
            RefreshDisplay();
        }

        private void SetupButtons()
        {
            // Back button
            if (backButton != null)
                backButton.onClick.AddListener(OnBackPressed);

            // Stat buttons
            SetupStatButtons();

            // Confirm/Reset buttons
            if (confirmStatsButton != null)
                confirmStatsButton.onClick.AddListener(ConfirmStatAllocation);
            if (resetStatsButton != null)
                resetStatsButton.onClick.AddListener(ResetStatAllocation);

            // Move slot buttons
            if (moveSlotButtons != null)
            {
                for (int i = 0; i < moveSlotButtons.Length && i < Hero.MAX_EQUIPPED_MOVES; i++)
                {
                    int slot = i;
                    moveSlotButtons[i].onClick.AddListener(() => SelectSlotForEquip(slot));
                }
            }
        }

        private void SetupStatButtons()
        {
            // HP buttons
            if (hpIncreaseButtons != null && hpIncreaseButtons.Length > 0)
                hpIncreaseButtons[0].onClick.AddListener(() => TempAllocateStat("hp", 1));
            if (hpDecreaseButtons != null && hpDecreaseButtons.Length > 0)
                hpDecreaseButtons[0].onClick.AddListener(() => TempAllocateStat("hp", -1));

            // Attack buttons
            if (attackIncreaseButtons != null && attackIncreaseButtons.Length > 0)
                attackIncreaseButtons[0].onClick.AddListener(() => TempAllocateStat("attack", 1));
            if (attackDecreaseButtons != null && attackDecreaseButtons.Length > 0)
                attackDecreaseButtons[0].onClick.AddListener(() => TempAllocateStat("attack", -1));

            // Defense buttons
            if (defenseIncreaseButtons != null && defenseIncreaseButtons.Length > 0)
                defenseIncreaseButtons[0].onClick.AddListener(() => TempAllocateStat("defense", 1));
            if (defenseDecreaseButtons != null && defenseDecreaseButtons.Length > 0)
                defenseDecreaseButtons[0].onClick.AddListener(() => TempAllocateStat("defense", -1));

            // Speed buttons
            if (speedIncreaseButtons != null && speedIncreaseButtons.Length > 0)
                speedIncreaseButtons[0].onClick.AddListener(() => TempAllocateStat("speed", 1));
            if (speedDecreaseButtons != null && speedDecreaseButtons.Length > 0)
                speedDecreaseButtons[0].onClick.AddListener(() => TempAllocateStat("speed", -1));

            // Luck buttons
            if (luckIncreaseButtons != null && luckIncreaseButtons.Length > 0)
                luckIncreaseButtons[0].onClick.AddListener(() => TempAllocateStat("luck", 1));
            if (luckDecreaseButtons != null && luckDecreaseButtons.Length > 0)
                luckDecreaseButtons[0].onClick.AddListener(() => TempAllocateStat("luck", -1));
        }

        private void RefreshDisplay()
        {
            if (hero == null) return;

            // Header
            if (heroNameText != null)
                heroNameText.text = hero.Name;
            if (levelText != null)
                levelText.text = $"Level {hero.level}";

            // XP progress
            if (xpText != null)
                xpText.text = $"XP: {hero.xp}/{hero.xpToNextLevel}";
            if (xpBar != null)
                xpBar.fillAmount = (float)hero.xp / hero.xpToNextLevel;

            // Available points
            int availablePoints = hero.pendingStatPoints - tempAllocatedPoints;
            if (availablePointsText != null)
                availablePointsText.text = $"Available: {availablePoints}";

            // Stats display
            if (hpText != null)
                hpText.text = $"{hero.MaxHp}";
            if (attackText != null)
                attackText.text = $"{hero.Attack}";
            if (defenseText != null)
                defenseText.text = $"{hero.Defense}";
            if (speedText != null)
                speedText.text = $"{hero.Speed}";
            if (luckText != null)
                luckText.text = $"{hero.Luck}";

            // Move slots display
            DisplayMoveSlots();
            DisplayLearnedMoves();
        }

        private void TempAllocateStat(string statType, int points)
        {
            int availablePoints = hero.pendingStatPoints - tempAllocatedPoints;

            if (points > 0 && availablePoints <= 0) return;

            if (tempStatAllocations.ContainsKey(statType))
            {
                // Prevent decreasing a stat below 0 for THIS session
                if (points < 0 && tempStatAllocations[statType] <= 0) return;

                tempStatAllocations[statType] += points;
                tempAllocatedPoints += points;
            }

            RefreshDisplay(); // This now updates texts instantly
        }

        private void ConfirmStatAllocation()
        {
            if (tempAllocatedPoints <= 0)
            {
                Debug.Log("[HeroStatsScreenManager] No stat points allocated.");
                return;
            }

            // Apply all stat allocations
            foreach (var allocation in tempStatAllocations)
            {
                if (allocation.Value > 0)
                {
                    hero.AllocateStatPoint(allocation.Key, allocation.Value);
                    Debug.Log($"Allocated {allocation.Value} to {allocation.Key}");
                }
            }

            // Reset temp tracking
            tempAllocatedPoints = 0;
            foreach (var key in new List<string>(tempStatAllocations.Keys))
                tempStatAllocations[key] = 0;

            Debug.Log("[HeroStatsScreenManager] Stat allocation confirmed!");
            RefreshDisplay();
        }

        private void ResetStatAllocation()
        {
            // Calculation: Total points ever earned - Points already permanently spent
            // Assuming you start at level 1 and get 3 points per level up
            int pointsPerLevel = 3; 
            int totalPointsEarned = (hero.level - 1) * pointsPerLevel;
            
            // You'll need a way to track 'spentPoints' in your Hero class, 
            // or simply reset the Hero's base stats and set pendingStatPoints = totalPointsEarned.
            
            // Simple version for your current setup:
            tempAllocatedPoints = 0;
            foreach (var key in new List<string>(tempStatAllocations.Keys))
                tempStatAllocations[key] = 0;
            
            Debug.Log("Resetting current session points.");
            RefreshDisplay();
        }

        private void SelectSlotForEquip(int slot)
        {
            if (selectedSlotForEquip == slot)
            {
                // Clicking same slot again - unselect
                selectedSlotForEquip = -1;
                selectedMoveForEquip = null;
                RefreshDisplay();
                return;
            }

            selectedSlotForEquip = slot;
            Debug.Log($"[HeroStatsScreenManager] Selected slot {slot + 1} for equipping");
            RefreshDisplay();
        }

        private void DisplayMoveSlots()
        {
            if (moveSlotButtons == null || moveSlotTexts == null)
                return;

            for (int i = 0; i < moveSlotButtons.Length && i < Hero.MAX_EQUIPPED_MOVES; i++)
            {
                var slottedMove = hero.GetSlottedMove(i);
                
                if (moveSlotTexts[i] != null)
                    moveSlotTexts[i].text = hero.GetSlotDisplayText(i);

                // Highlight selected slot
                var colors = moveSlotButtons[i].colors;
                if (i == selectedSlotForEquip)
                    colors.normalColor = Color.yellow;
                else
                    colors.normalColor = Color.white;
                moveSlotButtons[i].colors = colors;
            }
        }

       private void DisplayLearnedMoves()
        {
            if (learnedMovesContainer == null || moveButtonPrefabNew == null)
                return;

            // 1. Clear existing children to prevent UI stacking
            foreach (Transform child in learnedMovesContainer)
                Destroy(child.gameObject);

            // 2. Create buttons for learned moves
            foreach (var move in hero.LearnedMoves)
            {
                GameObject go = Instantiate(moveButtonPrefabNew, learnedMovesContainer);
                
                var textComp = go.GetComponentInChildren<TextMeshProUGUI>();
                if (textComp != null)
                {
                    textComp.text = move.name;
                }

                var buttonComp = go.GetComponent<Button>();
                if (buttonComp != null)
                {
                    buttonComp.onClick.RemoveAllListeners();
                    buttonComp.onClick.AddListener(() => TryEquipMove(move));
                }
            }
        }

        private void TryEquipMove(MoveScriptableObject move)
        {
            if (selectedSlotForEquip < 0)
            {
                Debug.LogWarning("[HeroStatsScreenManager] Select a slot first!");
                return;
            }

            // FIX: Check if this move is already equipped in ANY other slot
            for (int i = 0; i < Hero.MAX_EQUIPPED_MOVES; i++)
            {
                if (hero.GetSlottedMove(i) == move)
                {
                    Debug.LogWarning($"[HeroStatsScreenManager] {move.name} is already equipped in slot {i + 1}!");
                    return; // Exit here so we don't equip it twice
                }
            }

            hero.EquipMoveToSlot(selectedSlotForEquip, move);
            selectedSlotForEquip = -1;
            RefreshDisplay();
        }

        public void OnBackPressed()
        {
            Debug.Log("[HeroStatsScreenManager] Back to previous scene");
            // If this is an overlay panel, just deactivate it
            if (gameObject.name == "HeroStatsPanel")
            {
                gameObject.SetActive(false);
            }
            else
            {
                // If it's a full scene, load previous scene
                SceneManager.LoadScene("MonsterSelect");
            }
        }
    }
}
