using System;
using System.Collections.Generic;
using UnityEngine;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// Base class for all combatants (Hero, Monster). Handles stats, health, and status effects.
    /// Stats: HP, Attack, Defense, Speed (turn order), Luck (crit chance %).
    /// </summary>
    [System.Serializable]
    public class Entity : ICombatant
    {
        public int id;
        public string name;

        // --- PRO FIX: EVENTS ---
        // This allows the UI to "listen" for changes without the Entity needing to find the UI.
        public Action OnStatsChanged;

        // Core Stats
        [SerializeField] protected int hp;
        [SerializeField] protected int maxHp;
        [SerializeField] protected int attack;
        [SerializeField] protected int defense;
        [SerializeField] protected int speed;
        [SerializeField] protected int luck; // Crit chance: 1-100%

        // Status Effects (applied during battle)
        [NonSerialized] public List<StatusEffect> activeStatuses = new List<StatusEffect>();

        // Temporary stat modifiers (duration-based)
        [NonSerialized] public int tempAttackBonus = 0;
        [NonSerialized] public int tempDefenseBonus = 0;

        // Combat state
        [NonSerialized] public int shieldPoints = 0;

        public string Name => name;

        public int Hp 
        { 
            get => hp; 
            set 
            {
                hp = Mathf.Clamp(value, 0, maxHp);
                OnStatsChanged?.Invoke(); // Auto-update UI whenever HP is set
            }
        }

        public int MaxHp 
        { 
            get => maxHp; 
            set 
            {
                maxHp = value;
                OnStatsChanged?.Invoke();
            }
        }

        public int Attack 
        { 
            get => attack + tempAttackBonus; 
            set => attack = value; 
        }

        public int Defense 
        { 
            get => defense + tempDefenseBonus; 
            set => defense = value; 
        }

        public int Speed { get => speed; set => speed = value; }
        public int Luck { get => luck; set => luck = Mathf.Clamp(value, 0, 100); }

        public bool IsAlive() => hp > 0;

        public void TakeDamage(int amount)
        {
            // Shield blocks damage first
            int blocked = Mathf.Min(amount, shieldPoints);
            amount -= blocked;
            shieldPoints -= blocked;

            // Remaining damage applies to HP property (triggers OnStatsChanged)
            Hp -= amount; 
        }

        public void Heal(int amount)
        {
            Hp += amount; // Triggers OnStatsChanged
        }

        public void AddShield(int amount)
        {
            shieldPoints += amount;
            OnStatsChanged?.Invoke();
        }

        public void ClearShield()
        {
            shieldPoints = 0;
            OnStatsChanged?.Invoke();
        }

        public void ApplyStatus(StatusEffectType type, int duration, int value = 0)
        {
            activeStatuses.Add(new StatusEffect(type, duration, value));

            if (type == StatusEffectType.BuffAttack)
                tempAttackBonus += value;
            else if (type == StatusEffectType.DebuffDefense)
                tempDefenseBonus -= value;
            
            OnStatsChanged?.Invoke();
        }

        public bool ProcessStatuses(out List<string> statusLog)
        {
            statusLog = new List<string>();
            bool canAct = true;

            for (int i = activeStatuses.Count - 1; i >= 0; i--)
            {
                var status = activeStatuses[i];

                if (status.type == StatusEffectType.Stun && status.duration > 0)
                {
                    statusLog.Add($"{name} is stunned!");
                    canAct = false;
                }

                if (status.type == StatusEffectType.Poison && status.duration > 0)
                {
                    int poisonDamage = 5 + status.value;
                    TakeDamage(poisonDamage); // TakeDamage handles Hp change and OnStatsChanged trigger
                    statusLog.Add($"{name} takes {poisonDamage} poison damage!");
                }

                status.duration--;
                if (status.duration <= 0)
                {
                    if (status.type == StatusEffectType.BuffAttack)
                        tempAttackBonus -= status.value;
                    else if (status.type == StatusEffectType.DebuffDefense)
                        tempDefenseBonus += status.value;

                    activeStatuses.RemoveAt(i);
                    statusLog.Add($"{name}'s {status.type} ends.");
                }
            }

            OnStatsChanged?.Invoke(); // Final refresh for duration changes
            return canAct;
        }

        public void ResetBattleState()
        {
            this.Hp = this.MaxHp;
            activeStatuses.Clear();
            shieldPoints = 0;
            tempAttackBonus = 0;
            tempDefenseBonus = 0;
            OnStatsChanged?.Invoke();
        }
    }

    [Serializable]
    public class StatusEffect
    {
        public StatusEffectType type;
        public int duration;
        public int value;

        public StatusEffect(StatusEffectType type, int duration, int value = 0)
        {
            this.type = type;
            this.duration = duration;
            this.value = value;
        }
    }

    public enum StatusEffectType { None, Poison, Stun, BuffAttack, DebuffDefense }
    public enum MoveEffectType { Attack, Skill }
}