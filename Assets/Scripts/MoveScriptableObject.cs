using UnityEngine;

namespace NordeusChallenge.Unity
{
    /// <summary>
    /// Move ScriptableObject for defining attacks and skills.
    /// 
    /// Damage Calculation:
    /// Physical Damage = (Attacker.Attack + Move.BaseDamage) - (Defender.Defense / 2)
    /// Critical Hit = 2x damage if roll (1-100) <= Attacker.Luck
    /// Shield = Blocks damage first, then excess applies to HP
    /// 
    /// Non-Attack moves (Skills): Heal, Shield, Status Effects
    /// </summary>
    [CreateAssetMenu(fileName = "New Move", menuName = "Nordeus Challenge/Move")]
    public class MoveScriptableObject : ScriptableObject
    {
        public int id;
        [TextArea] public string description;
        
        // Combat Properties
        public MoveType moveType = MoveType.Physical;
        public int baseDamage; // Added to attacker's Attack stat
        
        // Effect Type (Attack = damage, Skill = heal/shield/status)
        public MoveEffectType effectType = MoveEffectType.Attack;
        public StatusEffectType statusEffectType = StatusEffectType.None;
        public int statusDuration = 0;
        
        // Skill Effect Values
        public int healAmount = 0;
        public int shieldAmount = 0;
        public int buffDebuffAmount = 0; // For attack buffs and defense debuffs
        
        public override string ToString() => name;
        
        // Helper property for existing code compatibility
        public string MoveName => name;
    }

    public enum MoveType
    {
        Physical,
        Magical
    }
}
