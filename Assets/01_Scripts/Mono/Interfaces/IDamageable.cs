public interface IDamageable
{
    public bool IsDead { get; }

    public void AfflictDamage (float amount);
}