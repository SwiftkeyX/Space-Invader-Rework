using UnityEngine;

// Numeric stats for the player ship. Power-ups mutate these at runtime; PlayerShipContext and Weapon read them.
// Distinct from PlayerShipState, which tracks runtime conditions (invuln, alive).
public class PlayerShipStat
{
    public float MoveSpeed { get; private set; }
    public float LeftBound { get; }
    public float RightBound { get; }
    public float FireCooldown { get; private set; }
    public float ProjectileSpeed { get; private set; }
    public int ProjectileDamage { get; private set; }
    public float ProjectileLifetime { get; }
    public int MultiShot { get; private set; }

    private readonly float _minFireCooldown;
    private readonly float _maxMoveSpeed;
    private readonly float _maxProjectileSpeed;
    private readonly int _maxMultiShot;
    private readonly int _maxProjectileDamage;

    public PlayerShipStat(
        float moveSpeed, float leftBound, float rightBound,
        float fireCooldown, float projectileSpeed, int projectileDamage, float projectileLifetime,
        float minFireCooldown, float maxMoveSpeed, float maxProjectileSpeed, int maxMultiShot, int maxProjectileDamage)
    {
        MoveSpeed = moveSpeed;
        LeftBound = leftBound;
        RightBound = rightBound;
        FireCooldown = fireCooldown;
        ProjectileSpeed = projectileSpeed;
        ProjectileDamage = projectileDamage;
        ProjectileLifetime = projectileLifetime;
        MultiShot = 1;
        _minFireCooldown = minFireCooldown;
        _maxMoveSpeed = maxMoveSpeed;
        _maxProjectileSpeed = maxProjectileSpeed;
        _maxMultiShot = maxMultiShot;
        _maxProjectileDamage = maxProjectileDamage;
    }

    public void ModifyFireCooldown(float delta)    => FireCooldown = Mathf.Max(_minFireCooldown, FireCooldown - delta);
    public void ModifyMultiShot(int delta)         => MultiShot = Mathf.Min(MultiShot + delta, _maxMultiShot);
    public void ModifyProjectileDamage(int delta)  => ProjectileDamage = Mathf.Min(ProjectileDamage + delta, _maxProjectileDamage);
    public void ModifyMoveSpeed(float delta)       => MoveSpeed = Mathf.Min(MoveSpeed + delta, _maxMoveSpeed);
    public void ModifyProjectileSpeed(float delta) => ProjectileSpeed = Mathf.Min(ProjectileSpeed + delta, _maxProjectileSpeed);
}
