using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Base class for all player weapons. Owns the projectile pool and fire-rate timer.
/// Subclasses define the fire pattern (single-shot, spread, laser, etc.).
/// </summary>
public abstract class Weapon : MonoBehaviour
{
    [SerializeField] protected Transform muzzle;
    [SerializeField] private Projectile projectilePrefab;

    protected IObjectPool<Projectile> Pool { get; private set; }
    private float _cooldownTimer;

    protected virtual void Awake()
    {
        Pool = new ObjectPool<Projectile>(
            () => Instantiate(projectilePrefab),
            p => p.gameObject.SetActive(true),
            p => p.gameObject.SetActive(false),
            p => Destroy(p.gameObject),
            false, 32, 256);

        // Pre-warm: zero Instantiate calls on the first-shot hot path.
        if (projectilePrefab != null)
        {
            var warm = new Projectile[32];
            for (int i = 0; i < 32; i++) warm[i] = Pool.Get();
            for (int i = 0; i < 32; i++) Pool.Release(warm[i]);
        }
    }

    /// <summary>Call every frame from PlayerShipContext. Ticks the cooldown and fires when held and ready.</summary>
    public void HandleFire(bool fireHeld, PlayerShipStat stat, float dt)
    {
        _cooldownTimer -= dt;
        if (fireHeld && _cooldownTimer <= 0f)
        {
            Fire(stat);
            _cooldownTimer = stat.FireCooldown;
        }
    }

    /// <summary>Subclasses spawn projectiles here. Pool and muzzle are already initialised.</summary>
    protected abstract void Fire(PlayerShipStat stat);

    /// <summary>Override to handle weapon-relevant power-up effects not captured in PlayerShipStat.</summary>
    public virtual void ApplyPowerUp(PowerUpData data) { }
}
