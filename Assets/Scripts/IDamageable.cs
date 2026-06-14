/// <summary>Implemented by any entity that can be hit by a projectile. Decouples Projectile from concrete target types.</summary>
public interface IDamageable
{
    Team Team { get; }
    void TakeDamage(int damage);
}
