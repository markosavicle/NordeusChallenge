using System.Collections.Generic;
using UnityEngine;

namespace NordeusChallenge.Unity
{
    [System.Serializable]
    public class MonsterData : ICombatant
    {
        public int id;
        public string name;
        public int hp;
        public int maxHp;
        public int defense;
        public int attack;
        public int speed;
        public int luck;
        public List<MoveData> moves = new List<MoveData>();

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
    }
}
