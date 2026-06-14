using System;
using UnityEngine;

// Numeric stats + damage surface for the player ship.
// As a MonoBehaviour it is found via GetComponent<IDamageable>() on projectile collision.
[RequireComponent(typeof(Collider2D))]
public class PlayerShipStat : MonoBehaviour, IDamageable
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 9f;
    [SerializeField] private float leftBound = -8f;
    [SerializeField] private float rightBound = 8f;

    [Header("Firing")]
    [SerializeField] private float baseFireCooldown = 0.25f;
    [SerializeField] private float projectileSpeed = 18f;
    [SerializeField] private int projectileDamage = 1;
    [SerializeField] private float projectileLifetime = 3f;

    [Header("Survival")]
    [SerializeField] private int   maxHealth     = 1;
    [SerializeField] private float invulnDuration = 1.0f;

    [Header("Power-up caps")]
    [SerializeField] private float minFireCooldown = 0.05f;
    [SerializeField] private float maxMoveSpeed = 20f;
    [SerializeField] private float maxProjectileSpeed = 40f;
    [SerializeField] private int maxMultiShot = 5;
    [SerializeField] private int maxProjectileDamage = 10;

    /// <summary>Fired on a vulnerable hit (feedback hook: flash/SFX/shake).</summary>
    public event Action OnPlayerHit;
    /// <summary>Fired when a hit costs a life. GameManager.HandlePlayerDeath subscribes.</summary>
    public event Action OnPlayerDeath;

    public Team Team => Team.Player;

    public float MoveSpeed      { get; private set; }
    public float LeftBound      { get; private set; }
    public float RightBound     { get; private set; }
    public float FireCooldown   { get; private set; }
    public float ProjectileSpeed  { get; private set; }
    public int   ProjectileDamage { get; private set; }
    public float ProjectileLifetime { get; private set; }
    public int   MultiShot      { get; private set; }

    public int   MaxHealth        => maxHealth;
    public int   Health           { get; private set; }
    public float InvulnDuration   => invulnDuration;
    public bool  IsInvulnerable   { get; set; }
    public float InvulnTimer      { get; set; }

    private void Awake()
    {
        MoveSpeed         = moveSpeed;
        LeftBound         = leftBound;
        RightBound        = rightBound;
        FireCooldown      = baseFireCooldown;
        ProjectileSpeed   = projectileSpeed;
        ProjectileDamage  = projectileDamage;
        ProjectileLifetime = projectileLifetime;
        MultiShot         = 1;
        Health            = maxHealth;
    }

    /// <summary>Called by Projectile via IDamageable. Blocked during invuln; otherwise reduces Health and starts the invuln window. Fires OnPlayerDeath when Health reaches 0 and resets Health to MaxHealth.</summary>
    public void TakeDamage(int damage)
    {
        if (IsInvulnerable) return;
        Health = Mathf.Max(0, Health - damage);
        OnPlayerHit?.Invoke();
        if (Health <= 0)
        {
            OnPlayerDeath?.Invoke();
            Health = maxHealth;
        }
        IsInvulnerable = true;
        InvulnTimer    = invulnDuration;
    }

    public void ModifyFireCooldown(float delta)    => FireCooldown    = Mathf.Max(minFireCooldown,    FireCooldown    - delta);
    public void ModifyMultiShot(int delta)         => MultiShot       = Mathf.Min(MultiShot           + delta, maxMultiShot);
    public void ModifyProjectileDamage(int delta)  => ProjectileDamage = Mathf.Min(ProjectileDamage  + delta, maxProjectileDamage);
    public void ModifyMoveSpeed(float delta)       => MoveSpeed       = Mathf.Min(MoveSpeed           + delta, maxMoveSpeed);
    public void ModifyProjectileSpeed(float delta) => ProjectileSpeed  = Mathf.Min(ProjectileSpeed    + delta, maxProjectileSpeed);
}
