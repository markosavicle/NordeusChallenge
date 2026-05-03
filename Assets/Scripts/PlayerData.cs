using System.Collections.Generic;
using UnityEngine;

namespace NordeusChallenge.Unity
{
    [System.Serializable]
    public class PlayerData : ICombatant
    {
        public int id;
        public string name;
        public int hp;
        public int maxHp;
        public int level = 1;
        public int xp;
        public int defense;
        public int attack = 10;
        public int speed = 5;
        public int luck = 1;
        public int pendingStatPoints;
        public Dictionary<string, int> pendingAllocations = new Dictionary<string, int>
        {
            { "hp", 0 },
            { "attack", 0 },
            { "defense", 0 },
            { "speed", 0 },
            { "luck", 0 }
        };

        public List<MoveData> learnedMoves = new List<MoveData>();

        public string Name => name;
        public int Hp { get => hp; set => hp = value; }
        public int MaxHp { get => maxHp; set => maxHp = value; }
        public int Attack { get => attack; set => attack = value; }
        public int Defense { get => defense; set => defense = value; }
        public int Speed { get => speed; set => speed = value; }
        public int Luck { get => luck; set => luck = value; }

        public bool IsAlive() => hp > 0;

        public void TakeDamage(int amount)
        {
            hp = Mathf.Max(0, hp - amount);
        }

        public void Heal(int amount)
        {
            hp = Mathf.Min(maxHp, hp + amount);
        }

        public void ApplyAllocations()
        {
            if (pendingAllocations.TryGetValue("hp", out int hpPoints))
            {
                maxHp += hpPoints * 10;
                hp += hpPoints * 10;
            }

            if (pendingAllocations.TryGetValue("attack", out int attackPoints)) attack += attackPoints;
            if (pendingAllocations.TryGetValue("defense", out int defensePoints)) defense += defensePoints;
            if (pendingAllocations.TryGetValue("speed", out int speedPoints)) speed += speedPoints;
            if (pendingAllocations.TryGetValue("luck", out int luckPoints)) luck += luckPoints;

            pendingAllocations["hp"] = 0;
            pendingAllocations["attack"] = 0;
            pendingAllocations["defense"] = 0;
            pendingAllocations["speed"] = 0;
            pendingAllocations["luck"] = 0;
        }

        public void GainXp(int amount)
        {
            xp += amount;
            while (xp >= 10 * level)
            {
                xp -= 10 * level;
                level++;
                pendingStatPoints += 3;
            }
        }
    }
}
