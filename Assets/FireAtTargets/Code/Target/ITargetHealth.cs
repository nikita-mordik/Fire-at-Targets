namespace FreedLOW.FireAtTargets.Code.Target
{
    public interface ITargetHealth
    {
        int MaxHealth { get; }
        int CurrentHealth { get; }

        void TakeDamage(int damage);
    }
}