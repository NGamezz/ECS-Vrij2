public interface IDamageable
{
    public bool Dead { get; }

    public void AfflictDamage ( float amount );
}