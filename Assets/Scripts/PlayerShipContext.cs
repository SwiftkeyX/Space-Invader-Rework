using System;
using UnityEngine;

/// <summary>
/// Lifecycle coordinator for the player ship. Update() delegates entirely to the active
/// BasePlayerShipState via _state.Tick(dt). Implements IDamageable so Projectile can hit it
/// without knowing the concrete type. See PlayerShip.md GDD.
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

    // Surface exposed to state classes.
    public PlayerShipStat Stat => _stat;
    public Weapon Weapon => weapon;
    public ActivePlayerState NormalState { get; private set; }

    private PlayerShipStat _stat;
    private BasePlayerShipState _state;
    private SpriteRenderer _sprite;

    private void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _stat = new PlayerShipStat(
            moveSpeed, leftBound, rightBound,
            baseFireCooldown, projectileSpeed, projectileDamage, projectileLifetime,
            minFireCooldown, maxMoveSpeed, maxProjectileSpeed, maxMultiShot, maxProjectileDamage);
        NormalState = new ActivePlayerState(this);
        _state = NormalState;
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
        _state.Tick(Time.deltaTime);
    }

    /// <summary>Transitions to a new behavioural state. Called by state classes.</summary>
    public void TransitionTo(BasePlayerShipState next)
    {
        _state = next;
        _state.OnEnter();
    }

    /// <summary>Moves the ship horizontally within bounds. Called by state classes.</summary>
    public void PerformMove(InputManager input, float dt)
    {
        float axis = input != null ? input.MoveAxis : 0f;
        var pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + axis * _stat.MoveSpeed * dt, _stat.LeftBound, _stat.RightBound);
        transform.position = pos;
    }

    /// <summary>Sets sprite transparency. Called by state classes for blink feedback.</summary>
    public void SetSpriteAlpha(float alpha)
    {
        if (_sprite != null)
            _sprite.color = new Color(1f, 1f, 1f, alpha);
    }

    /// <summary>Called by Projectile via IDamageable. Blocked by InvulnPlayerState; otherwise costs a life and starts invuln.</summary>
    public void TakeDamage(int damage)
    {
        if (_state is InvulnPlayerState) return;
        OnPlayerHit?.Invoke();
        OnPlayerDeath?.Invoke();
        TransitionTo(new InvulnPlayerState(this, invulnDuration));
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
