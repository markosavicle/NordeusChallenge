using System.Collections.Generic;
using UnityEngine;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// Hero entity for the player character.
    /// Extends Entity with progression system (leveling, XP, stat points).
    /// Maintains a list of learned moves (from defeated monsters).
    /// </summary>
    public class Hero : Entity
    {
        public int level = 1;
        public int xp = 0;
        public int xpToNextLevel => 10 * level;
        public int pendingStatPoints = 0;

        [SerializeField] private List<MoveScriptableObject> learnedMoves = new List<MoveScriptableObject>();
        [SerializeField] private MoveScriptableObject[] equippedMoveSlots = new MoveScriptableObject[4]; // 4 battle slots

        public IReadOnlyList<MoveScriptableObject> LearnedMoves => learnedMoves.AsReadOnly();
        public const int MAX_EQUIPPED_MOVES = 4;
        private const int startingHp = 100;
        private const int startingAttack = 10;
        private const int startingDefense = 5;
        private const int startingSpeed = 5;
        private const int startingLuck = 1;

        public Hero()
        {
            id = 1;
            name = "Hero";
            hp = maxHp = startingHp;
            attack = startingAttack;
            defense = startingDefense;
            speed = startingSpeed;
            luck = startingLuck;
            level = 1;
            equippedMoveSlots = new MoveScriptableObject[4];
        }

        /// <summary>
        /// Learn a new move from a defeated monster.
        /// If move already learned, it's not added again.
        /// </summary>
        public void LearnMove(MoveScriptableObject move)
        {
            if (move == null) return;
            if (learnedMoves.Contains(move))
            {
                Debug.Log($"{name} already knows {move.name}!");
                return;
            }

            learnedMoves.Add(move);
            Debug.Log($"{name} learned {move.name}!");
        }

        /// <summary>
        /// Equip a move to a specific battle slot (0-3).
        /// Only 4 moves can be equipped at once, one per slot.
        /// </summary>
        public void EquipMoveToSlot(int slot, MoveScriptableObject move)
        {
            if (slot < 0 || slot >= MAX_EQUIPPED_MOVES)
            {
                Debug.LogWarning($"Invalid slot: {slot}. Must be 0-{MAX_EQUIPPED_MOVES - 1}");
                return;
            }

            if (move != null && !learnedMoves.Contains(move))
            {
                Debug.LogWarning($"Hero hasn't learned {move.name}!");
                return;
            }

            equippedMoveSlots[slot] = move;
            Debug.Log($"Slot {slot + 1}: {(move != null ? move.name : "Empty")}");
        }

        /// <summary>
        /// Get the move in a specific battle slot.
        /// </summary>
        public MoveScriptableObject GetSlottedMove(int slot)
        {
            if (slot < 0 || slot >= MAX_EQUIPPED_MOVES)
                return null;

            return equippedMoveSlots[slot];
        }

        /// <summary>
        /// Get all equipped moves in battle order (slots 0-3).
        /// </summary>
        public List<MoveScriptableObject> GetBattleMoves()
        {
            var battleMoves = new List<MoveScriptableObject>();
            for (int i = 0; i < equippedMoveSlots.Length; i++)
            {
                if (equippedMoveSlots[i] != null) // If these are all null, you get 0 buttons!
                    battleMoves.Add(equippedMoveSlots[i]);
            }
            return battleMoves;
        }

        /// <summary>
        /// Gain XP and handle level-up.
        /// XP threshold: 10 * current_level
        /// Reward: 3 stat points per level
        /// </summary>
        public void GainXP(int amount)
        {
            xp += amount;
            int xpThreshold = 10 * level;

            while (xp >= xpThreshold)
            {
                xp -= xpThreshold;
                level++;
                pendingStatPoints += 3;
                Debug.Log($"{name} reached level {level}!");
                xpThreshold = 10 * level;
            }
        }

        /// <summary>
        /// Allocate pending stat points to a specific stat.
        /// </summary>
        public void AllocateStatPoint(string statName, int points)
        {
            if (points < 0 || points > pendingStatPoints) return;

            switch (statName.ToLower())
            {
                case "hp":
                    maxHp += points * 10;
                    hp = Mathf.Min(hp + points * 10, maxHp);
                    break;
                case "attack":
                    attack += points;
                    break;
                case "defense":
                    defense += points;
                    break;
                case "speed":
                    speed += points;
                    break;
                case "luck":
                    luck += points;
                    break;
                default:
                    return;
            }

            pendingStatPoints -= points;
        }

        /// <summary>
        /// Reset move equipment and learned moves for a new run.
        /// </summary>
        public void ResetLearnedMoves()
        {
            learnedMoves.Clear();
            for (int i = 0; i < equippedMoveSlots.Length; i++)
                equippedMoveSlots[i] = null;
        }

        /// <summary>
        /// Reset stat point allocation (for stat respec).
        /// </summary>
        public void ResetStatAllocation()
        {
            // Revert stats to starting values
            maxHp = startingHp;
            hp = startingHp;
            attack = startingAttack;
            defense = startingDefense;
            speed = startingSpeed;
            luck = startingLuck;

            // Return all points earned through leveling (3 points per level above 1)
            pendingStatPoints = (level - 1) * 3;
            Debug.Log($"Reset stats to Base. Available points: {pendingStatPoints}");
        }

        /// <summary>
        /// Get equipped move in a specific slot for display.
        /// </summary>
        public string GetSlotDisplayText(int slot)
        {
            if (slot < 0 || slot >= MAX_EQUIPPED_MOVES)
                return "Empty";

            var move = equippedMoveSlots[slot];
            return move != null ? move.name : "Empty";
        }
    }
}
