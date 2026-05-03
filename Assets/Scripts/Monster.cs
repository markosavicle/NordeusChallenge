using System.Collections.Generic;
using UnityEngine;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// Monster entity for enemy combatants.
    /// Spawned from MonsterScriptableObject template during battle.
    /// </summary>
    public class Monster : Entity
    {
        public MonsterScriptableObject template;
        [SerializeField] private List<MoveScriptableObject> availableMoves = new List<MoveScriptableObject>();
        [SerializeField] private List<int> lastMovesUsed = new List<int>(); // Track last 3 moves to prevent repeats

        public IReadOnlyList<MoveScriptableObject> AvailableMoves => availableMoves.AsReadOnly();
        public int XpReward => template != null ? template.xpReward : 10;

        public Monster() { }

        /// <summary>
        /// Initialize monster from a ScriptableObject template.
        /// Sets all stats and moves from the template.
        /// </summary>
        public void Initialize(MonsterScriptableObject monsterTemplate)
        {
            template = monsterTemplate;
            id = monsterTemplate.id;
            name = monsterTemplate.name;
            hp = maxHp = monsterTemplate.maxHp;
            attack = monsterTemplate.attack;
            defense = monsterTemplate.defense;
            speed = monsterTemplate.speed;
            luck = monsterTemplate.luck;

            availableMoves = new List<MoveScriptableObject>(monsterTemplate.availableMoves);
            lastMovesUsed.Clear();

            ResetBattleState();
        }

        /// <summary>
        /// AI: Choose a move, avoiding repeating the last 2 moves used.
        /// Returns a random available move.
        /// </summary>
        public MoveScriptableObject ChooseMove()
        {
            if (availableMoves.Count == 0) return null;

            // Get moves not in recent history
            var candidateMoves = new List<MoveScriptableObject>();
            foreach (var move in availableMoves)
            {
                // Check if this move is in the last 2 used
                bool isRecent = false;
                int checkCount = Mathf.Min(2, lastMovesUsed.Count);
                for (int i = 0; i < checkCount; i++)
                {
                    if (lastMovesUsed[lastMovesUsed.Count - 1 - i] == move.id)
                    {
                        isRecent = true;
                        break;
                    }
                }

                if (!isRecent)
                    candidateMoves.Add(move);
            }

            // If all moves are recent, reset history
            if (candidateMoves.Count == 0)
            {
                lastMovesUsed.Clear();
                candidateMoves = availableMoves;
            }

            var chosen = candidateMoves[Random.Range(0, candidateMoves.Count)];
            lastMovesUsed.Add(chosen.id);

            // Keep history to last 3 moves
            if (lastMovesUsed.Count > 3)
                lastMovesUsed.RemoveAt(0);

            return chosen;
        }

        /// <summary>
        /// Get a random move from this monster's arsenal.
        /// Used for move-learning system when monster is defeated.
        /// </summary>
        public MoveScriptableObject GetRandomMove()
        {
            if (availableMoves.Count == 0) return null;
            return availableMoves[Random.Range(0, availableMoves.Count)];
        }
    }
}
