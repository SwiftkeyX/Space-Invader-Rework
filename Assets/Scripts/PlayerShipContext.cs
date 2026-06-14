using System;
using UnityEngine;

/// <summary>
/// Blackboard + lifecycle coordinator for the player ship. Owns the BT root;
/// Update() is a guard + _btRoot.Tick(dt). Implements IDamageable so Projectile
/// can call TakeDamage() without knowing the concrete type. See PlayerShip.md GDD.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerShipContext : MonoBehaviour, IDamageable
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
    [SerializeField] private float invulnDuration = 1.0f;

    [Header("Power-up caps")]
    [SerializeField] private float minFireCooldown = 0.05f;
    [SerializeField] private float maxMoveSpeed = 20f;
    [SerializeField] private float maxProjectileSpeed = 40f;
    [SerializeField] private int maxMultiShot = 5;
    [SerializeField] private int maxProjectileDamage = 10;

    [Header("References")]
    [SerializeField] private Weapon weapon;
    [SerializeField] private PowerUpSystem powerUpSystem;

    /// <summary>Fired on a vulnerable hit (feedback hook: flash/SFX/shake).</summary>
    public event Action OnPlayerHit;
    /// <summary>Fired when a hit costs a life. GameManager.HandlePlayerDeath subscribes.</summary>
    public event Action OnPlayerDeath;

    public Team Team => Team.Player;

    // Blackboard — read/written by BT nodes.
    public PlayerShipStat Stat => _stat;
    public Weapon Weapon => weapon;
    public SpriteRenderer Sprite { get; private set; }
    public bool IsInvulnerable { get; set; }
    public float InvulnTimer { get; set; }
    public float InvulnDuration => invulnDuration;

    private PlayerShipStat _stat;
    private BTNode _btRoot;

    private void Awake()
    {
        Sprite = GetComponentInChildren<SpriteRenderer>();
        _stat = new PlayerShipStat(
            moveSpeed, leftBound, rightBound,
            baseFireCooldown, projectileSpeed, projectileDamage, projectileLifetime,
            minFireCooldown, maxMoveSpeed, maxProjectileSpeed, maxMultiShot, maxProjectileDamage);
        _btRoot = new BTParallel(
            new MoveAction(this),
            new FireAction(this),
            new InvulnOverlayAction(this));
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            OnPlayerDeath += GameManager.Instance.HandlePlayerDeath;
        if (powerUpSystem != null)
            powerUpSystem.OnPowerUpChosen += ApplyPowerUp;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            OnPlayerDeath -= GameManager.Instance.HandlePlayerDeath;
        if (powerUpSystem != null)
            powerUpSystem.OnPowerUpChosen -= ApplyPowerUp;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Running) return;
        _btRoot.Tick(Time.deltaTime);
    }

    /// <summary>Called by Projectile via IDamageable. Blocked during invuln; otherwise costs a life and starts the invuln window.</summary>
    public void TakeDamage(int damage)
    {
        if (IsInvulnerable) return;
        OnPlayerHit?.Invoke();
        OnPlayerDeath?.Invoke();
        IsInvulnerable = true;
        InvulnTimer = invulnDuration;
    }

    private void ApplyPowerUp(PowerUpData data)
    {
        switch (data.effect)
        {
            case PowerUpEffect.ExtraLife:   GameManager.Instance?.AddLife(1); break;
            case PowerUpEffect.RapidFire:   _stat.ModifyFireCooldown(data.magnitude); break;
            case PowerUpEffect.MultiShot:   _stat.ModifyMultiShot((int)data.magnitude); break;
            case PowerUpEffect.PowerShot:   _stat.ModifyProjectileDamage((int)data.magnitude); break;
            case PowerUpEffect.Swift:       _stat.ModifyMoveSpeed(data.magnitude); break;
            case PowerUpEffect.BulletSpeed: _stat.ModifyProjectileSpeed(data.magnitude); break;
        }
        weapon?.ApplyPowerUp(data);
        Debug.Log($"[PlayerShipContext] Applied {data.displayName} — effect={data.effect} mag={data.magnitude}");
    }
}
