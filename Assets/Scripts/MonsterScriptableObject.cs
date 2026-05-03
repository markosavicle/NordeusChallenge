using System.Collections.Generic;
using UnityEngine;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// Monster ScriptableObject for defining enemy types.
    /// Contains base stats and available moves.
    /// Each monster defeated grants the player one random move from its move list.
    /// </summary>
    [CreateAssetMenu(fileName = "New Monster", menuName = "Nordeus Challenge/Monster")]
    public class MonsterScriptableObject : ScriptableObject
    {
        public int id;
        [TextArea] public string description;

        // Combat Stats
        public int maxHp = 100;
        public int attack = 10;
        public int defense = 5;
        public int speed = 5;
        public int luck = 5; // Critical hit chance %

        // Available Moves
        [SerializeField] public List<MoveScriptableObject> availableMoves = new List<MoveScriptableObject>();

        // XP Reward
        public int xpReward = 10;

        public Sprite monsterIcon;

        public override string ToString() => name;

        /// <summary>
        /// Returns a random move from this monster's available moves.
        /// Used by AI and move-learning system.
        /// </summary>
        public MoveScriptableObject GetRandomMove()
        {
            if (availableMoves.Count == 0) return null;
            return availableMoves[Random.Range(0, availableMoves.Count)];
        }
    }
}
