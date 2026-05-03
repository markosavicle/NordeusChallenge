using UnityEngine;
using UnityEngine.Events;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// BattleStateMachine controls the turn-based battle flow.
    /// States: Idle -> HeroTurn -> MonsterTurn -> (victory/defeat check) -> Busy -> repeat
    /// 
    /// Ensures:
    /// - Only valid moves can be executed
    /// - Turn order is respected
    /// - Damage is calculated correctly
    /// - Battle ends when hero or monster dies
    /// </summary>
    public class BattleStateMachine : MonoBehaviour
    {
        [SerializeField] private BattleState currentState;
        [SerializeField] private Hero hero;
        [SerializeField] private Monster monster;

        private BattleLog battleLog = new BattleLog();

        // Events for UI updates
        public UnityEvent<string> OnBattleLogUpdated = new UnityEvent<string>();
        public UnityEvent OnHeroTurnStart = new UnityEvent();
        public UnityEvent OnMonsterTurnStart = new UnityEvent();
        public UnityEvent<string> OnBattleEnd = new UnityEvent<string>(); // Winner name

        public BattleState CurrentState => currentState;
        public BattleLog BattleLog => battleLog;
        public bool IsHeroTurn => currentState == BattleState.HeroTurn;
        public bool IsMonsterTurn => currentState == BattleState.MonsterTurn;

        private void Start()
        {
            if (RunManager.Instance != null)
            {
                hero = RunManager.Instance.Hero;
                monster = RunManager.Instance.CurrentMonster;
            }

            if (hero == null || monster == null)
            {
                Debug.LogError($"[Battle] Missing Data! Hero: {hero != null}, Monster: {monster != null}");
                return;
            }

        }

        /// <summary>
        /// Initialize and start a new battle.
        /// </summary>
        public void StartBattle()
        {
            hero.ResetBattleState();
            monster.ResetBattleState();
            battleLog.Clear();

            LogAction("Battle started!");
            LogAction($"{hero.Name} vs {monster.Name}");

            DetermineTurnOrder();
        }

        /// <summary>
        /// Determine who goes first based on Speed stat.
        /// Higher speed = goes first.
        /// </summary>
        private void DetermineTurnOrder()
        {
            if (hero.Speed >= monster.Speed)
            {
                currentState = BattleState.HeroTurn;
                LogAction($"{hero.Name} is faster and goes first!");
            }
            else
            {
                currentState = BattleState.MonsterTurn;
                LogAction($"{monster.Name} is faster and goes first!");
            }

            OnStateChanged();
        }

        /// <summary>
        /// Hero executes a move. Only valid during HeroTurn state.
        /// </summary>
        public void ExecuteHeroMove(MoveScriptableObject move)
        {
            if (currentState != BattleState.HeroTurn)
            {
                Debug.LogWarning("Cannot execute move during " + currentState);
                return;
            }

            if (move == null)
            {
                Debug.LogError("Move is null!");
                return;
            }

            ExecuteMove(hero, monster, move);

            // Check if battle is over
            if (!monster.IsAlive())
            {
                EndBattle(hero.Name);
                return;
            }

            // Switch to monster turn
            currentState = BattleState.MonsterTurn;
            OnStateChanged();
        }

        /// <summary>
        /// Monster executes a move. Only valid during MonsterTurn state.
        /// AI chooses the move automatically.
        /// </summary>
        public void ExecuteMonsterMove()
        {
            if (currentState != BattleState.MonsterTurn)
            {
                Debug.LogWarning("Cannot execute move during " + currentState);
                return;
            }

            MoveScriptableObject move = monster.ChooseMove();
            if (move == null)
            {
                LogAction($"{monster.Name} has no moves available!");
                return;
            }

            ExecuteMove(monster, hero, move);

            // Check if battle is over
            if (!hero.IsAlive())
            {
                EndBattle(monster.Name);
                return;
            }

            // Switch to hero turn
            currentState = BattleState.HeroTurn;
            OnStateChanged();
        }

        /// <summary>
        /// Core damage calculation and effect application.
        /// Damage = (Attacker.Attack + Move.BaseDamage) - (Defender.Defense / 2)
        /// Critical = 2x damage if Random(1-100) <= Attacker.Luck
        /// Shield blocks damage first.
        /// </summary>
        private void ExecuteMove(Entity attacker, Entity defender, MoveScriptableObject move)
        {
            // Process attacker's status effects first (could stun, deal poison damage, etc)
            if (!attacker.ProcessStatuses(out var attackerStatusLog))
            {
                foreach (var log in attackerStatusLog)
                    LogAction(log);
                return; // Attacker is stunned, skips turn
            }

            foreach (var log in attackerStatusLog)
                LogAction(log);

            // Execute the move
            LogAction($"{attacker.Name} uses {move.name}!");

            if (move.effectType == MoveEffectType.Attack)
            {
                ExecuteAttack(attacker, defender, move);
            }
            else if (move.effectType == MoveEffectType.Skill)
            {
                ExecuteSkill(attacker, defender, move);
            }

            // Process defender's status effects (damage over time, etc)
            if (defender.ProcessStatuses(out var defenderStatusLog))
            {
                foreach (var log in defenderStatusLog)
                    LogAction(log);
            }
            else
            {
                foreach (var log in defenderStatusLog)
                    LogAction(log);
            }
        }

        /// <summary>
        /// Execute an attack move.
        /// Damage = (Attacker.Attack + Move.BaseDamage) - (Defender.Defense / 2)
        /// Critical multiplier: 2x if crit chance succeeds
        /// </summary>
        private void ExecuteAttack(Entity attacker, Entity defender, MoveScriptableObject move)
        {
            // Calculate base damage: (Attack + MovePower) - (Defense/2)
            int attackerAttack = attacker.Attack;
            int baseDamage = attackerAttack + move.baseDamage;
            int defenseReduction = defender.Defense / 2;
            int damage = Mathf.Max(1, baseDamage - defenseReduction);

            // Check for critical hit
            bool isCritical = Random.Range(1, 101) <= attacker.Luck;
            if (isCritical)
            {
                damage *= 2;
                LogAction($"CRITICAL HIT! 2x damage");
            }

            // Deal damage (shield blocks first)
            defender.TakeDamage(damage);
            // ADD THIS LINE to notify the UI specifically
            OnBattleLogUpdated.Invoke($"{defender.Name} takes {damage} damage!"); 
            
            // To fix the HP bar instantly, find your BattleUIManager and call Refresh
            FindFirstObjectByType<BattleUIManager>().RefreshStatsDisplay();
            
            LogAction($"{attacker.Name} deals {damage} damage to {defender.Name}.");
            LogAction($"{defender.Name} has {defender.Hp}/{defender.MaxHp} HP remaining.");

            // Apply status effect if the attack has one
            if (move.statusEffectType != StatusEffectType.None && move.statusDuration > 0)
            {
                defender.ApplyStatus(move.statusEffectType, move.statusDuration, move.buffDebuffAmount);
                LogAction($"{defender.Name} is afflicted with {move.statusEffectType}!");
            }
        }

        /// <summary>
        /// Execute a skill move (heal, shield, buff, debuff, apply status).
        /// </summary>
        private void ExecuteSkill(Entity attacker, Entity defender, MoveScriptableObject move)
        {
            if (move.healAmount > 0)
            {
                attacker.Heal(move.healAmount);
                LogAction($"{attacker.Name} heals for {move.healAmount} HP!");
                LogAction($"{attacker.Name} has {attacker.Hp}/{attacker.MaxHp} HP.");
            }

            if (move.shieldAmount > 0)
            {
                attacker.AddShield(move.shieldAmount);
                LogAction($"{attacker.Name} gains {move.shieldAmount} shield!");
            }

            if (move.statusEffectType != StatusEffectType.None)
            {
                Entity target = move.statusEffectType == StatusEffectType.BuffAttack ? attacker : defender;
                target.ApplyStatus(move.statusEffectType, move.statusDuration, move.buffDebuffAmount);
                LogAction($"{target.Name} is affected by {move.statusEffectType}!");
            }
        }

        /// <summary>
        /// End the battle and determine winner.
        /// </summary>
        private void EndBattle(string winner)
        {
            currentState = BattleState.Victory;
            LogAction($"{winner} wins the battle!");
            OnBattleEnd.Invoke(winner);

            // Record outcome in RunManager
            bool heroWon = (winner == hero.Name);
            if (RunManager.Instance != null)
            {
                RunManager.Instance.RecordBattleOutcome(heroWon);
            }
        }

        /// <summary>
        /// Log a message to the battle log and notify UI.
        /// </summary>
        private void LogAction(string message)
        {
            battleLog.Add(message);
            OnBattleLogUpdated.Invoke(message);
            Debug.Log("[Battle] " + message);
        }

        private void OnStateChanged()
        {
            if (currentState == BattleState.HeroTurn)
                OnHeroTurnStart.Invoke();
            else if (currentState == BattleState.MonsterTurn)
                OnMonsterTurnStart.Invoke();
        }
    }

    public enum BattleState
    {
        Idle,        // Waiting to start
        HeroTurn,    // Player can select a move
        MonsterTurn, // Monster AI chooses move
        Victory,     // Battle over, someone won
        Defeat       // Battle over, hero defeated
    }

    /// <summary>
    /// Simple wrapper for battle log messages.
    /// </summary>
    public class BattleLog
    {
        private System.Collections.Generic.List<string> messages = new System.Collections.Generic.List<string>();

        public void Add(string message) => messages.Add(message);
        public void Clear() => messages.Clear();
        public System.Collections.Generic.IReadOnlyList<string> Messages => messages.AsReadOnly();
        public override string ToString() => string.Join("\n", messages);
    }
}
