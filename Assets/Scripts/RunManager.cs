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

            if (heroWon)
            {
                // --- SMART MOVE LEARNING ---
                // Get all moves the monster has
                var monsterMoves = currentMonster.AvailableMoves; // Ensure Monster has a GetMoves() returning a List
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
                    // Pick a random one from only the ones we DON'T know
                    MoveScriptableObject newMove = unlearnedMoves[Random.Range(0, unlearnedMoves.Count)];
                    hero.LearnMove(newMove);
                    Debug.Log($"[RunManager] Hero learned NEW move: {newMove.name}");
                }
                else
                {
                    Debug.Log("[RunManager] Hero already knows all moves from this monster.");
                }

                // XP reward
                hero.GainXP(currentMonster.XpReward);
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

        /// <summary>
        /// DEPRECATED: Compatibility method for legacy ResultsScreenManager.
        /// Returns 0 as wins are not tracked in single-monster flow.
        /// </summary>
        [System.Obsolete("Use single-battle system instead. Kept for backward compatibility.")]
        public int GetWinCount()
        {
            return 0;
        }

        /// <summary>
        /// DEPRECATED: Compatibility property for legacy ResultsScreenManager.
        /// Always returns false in single-monster flow (no "run" concept).
        /// </summary>
        [System.Obsolete("Use single-battle system instead. Kept for backward compatibility.")]
        public bool RunComplete => false;

        /// <summary>
        /// DEPRECATED: Compatibility method for legacy ResultsScreenManager.
        /// Does nothing in single-monster flow.
        /// </summary>
        [System.Obsolete("Use SetCurrentMonster() instead.")]
        public void SetupCurrentMonster()
        {
            // Legacy method - does nothing in new system
        }
    }
}
