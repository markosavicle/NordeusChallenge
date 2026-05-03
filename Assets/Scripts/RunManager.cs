using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// RunManager tracks the current game state:
    /// - Current hero
    /// - Current monster for battle
    /// - Hero progression (learned moves, level, stats)
    /// 
    /// Singleton pattern: Access via RunManager.Instance
    /// Persists across scenes during gameplay.
    /// 
    /// Game Flow (Single Monster at a Time):
    /// MainMenu -> HeroSelect (create hero) -> MonsterSelect (pick any monster) -> 
    /// Battle (fight one monster) -> Return to MonsterSelect (pick another or go back)
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [SerializeField] private Hero hero;
        private Monster currentMonster;

        public Hero Hero => hero;
        public Monster CurrentMonster => currentMonster;
        public int CurrentFightNumber => 1;
        public int TotalFights => 1;
        public string LastLearnedMoveName { get; private set; } = "";
        public bool DidLevelUpLastBattle { get; private set; } = false;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>
        /// Start a new run with the given hero.
        /// Called from HeroSelectManager after player creates hero.
        /// </summary>
        public void StartNewRun(Hero newHero)
        {
            hero = newHero;
            hero.ResetBattleState();
            currentMonster = null;

            Debug.Log($"[RunManager] New run started with {hero.Name}");
        }

        /// <summary>
        /// Set the current monster for the next battle.
        /// Called from MonsterSelectManager when player clicks a monster.
        /// </summary>
        public void SetCurrentMonster(MonsterScriptableObject monsterTemplate)
        {
            if (hero == null)
            {
                Debug.LogError("[RunManager] Hero is null! Start a new run first.");
                return;
            }

            currentMonster = new Monster();
            currentMonster.Initialize(monsterTemplate);
            Debug.Log($"[RunManager] Current monster set to: {currentMonster.Name}");
        }

        /// <summary>
        /// Record the outcome of the battle.
        /// If hero won: learn a random move from the defeated monster and gain XP.
        /// </summary>
        public void RecordBattleOutcome(bool heroWon)
        {
            if (currentMonster == null) return;

            // Reset results from previous battle
            LastLearnedMoveName = "";
            DidLevelUpLastBattle = false;

            if (heroWon)
            {
                // --- SMART MOVE LEARNING ---
                var monsterMoves = currentMonster.AvailableMoves; 
                List<MoveScriptableObject> unlearnedMoves = new List<MoveScriptableObject>();

                foreach (var m in monsterMoves)
                {
                    if (!hero.LearnedMoves.Contains(m))
                    {
                        unlearnedMoves.Add(m);
                    }
                }

                if (unlearnedMoves.Count > 0)
                {
                    MoveScriptableObject newMove = unlearnedMoves[Random.Range(0, unlearnedMoves.Count)];
                    hero.LearnMove(newMove);
                    LastLearnedMoveName = newMove.name; 
                }

                // --- XP & LEVEL UP CHECK ---
                int levelBefore = hero.level;
                hero.GainXP(currentMonster.XpReward);
                
                // This is the proper logic: check if the current level increased
                if (hero.level > levelBefore)
                {
                    DidLevelUpLastBattle = true;
                }
            }
            
            currentMonster = null;
        }

        /// <summary>
        /// Get hero's current stats for display.
        /// </summary>
        public string GetHeroStats()
        {
            if (hero == null)
                return "No Hero";

            return $"Name: {hero.Name}\n" +
                   $"Level: {hero.level}\n" +
                   $"HP: {hero.Hp}/{hero.MaxHp}\n" +
                   $"ATK: {hero.Attack}\n" +
                   $"DEF: {hero.Defense}\n" +
                   $"SPD: {hero.Speed}\n" +
                   $"LCK: {hero.Luck}";
        }

        /// <summary>
        /// Get hero's learned moves for display.
        /// </summary>
        public List<MoveScriptableObject> GetHeroLearnedMoves()
        {
            if (hero == null)
                return new List<MoveScriptableObject>();

            return new List<MoveScriptableObject>(hero.LearnedMoves);
        }

    }
}
