namespace NordeusChallenge.Unity
{
    public interface ICombatant
    {
        string Name { get; }
        int Hp { get; set; }
        int MaxHp { get; set; }
        int Attack { get; set; }
        int Defense { get; set; }
        int Speed { get; set; }
        int Luck { get; set; }

        bool IsAlive();
        void TakeDamage(int amount);
        void Heal(int amount);
    }
}
