using UnityEngine;

namespace NordeusChallenge.Unity
{
    // NOTE: MoveEffectType and StatusEffect enums are now defined in Entity.cs
    // This class is kept for backward compatibility with legacy code.
    // Use MoveScriptableObject for new code.

    [System.Serializable]
    public class MoveData
    {
        public int id;
        public string name;
        public int damage;
        public string type;
        public MoveEffectType effectType;
        public int effectValue;
        public LegacyStatusEffect statusEffect; // Legacy enum, kept for old code
        public int duration;
        public bool isEquipped;

        [System.Obsolete("Use MoveScriptableObject instead")]
        public MoveData(int id, string name, int damage, string type, MoveEffectType effectType, int effectValue, LegacyStatusEffect statusEffect, int duration)
        {
            this.id = id;
            this.name = name;
            this.damage = damage;
            this.type = type;
            this.effectType = effectType;
            this.effectValue = effectValue;
            this.statusEffect = statusEffect;
            this.duration = duration;
            this.isEquipped = false;
        }
    }

    /// <summary>
    /// Legacy status effect enum for backward compatibility.
    /// Use StatusEffectType in Entity.cs for new code.
    /// </summary>
    public enum LegacyStatusEffect { None, Heal, Shield, Poison, Stun, BuffAttack, DebuffDefense }
}

