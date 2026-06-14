using System;
using UnityEngine;

/// <summary>
/// Lifecycle coordinator for the player ship. Thin Update() delegates to
/// PlayerShipState (i-frames/conditions), PlayerShipStat (numeric values), and
/// Weapon (fire logic). Implements IDamageable so Projectile can hit it without
/// knowing the concrete type. See PlayerShip.md GDD.
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

    private PlayerShipStat _stat;
    private PlayerShipState _state;
    private SpriteRenderer _sprite;

    private void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _stat = new PlayerShipStat(
            moveSpeed, leftBound, rightBound,
            baseFireCooldown, projectileSpeed, projectileDamage, projectileLifetime,
            minFireCooldown, maxMoveSpeed, maxProjectileSpeed, maxMultiShot, maxProjectileDamage);
        _state = new PlayerShipState(invulnDuration);
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

        var input = GameManager.Instance.Input;
        Move(input);
        weapon?.HandleFire(input != null && input.FireHeld, _stat, Time.deltaTime);
        _state.Tick(Time.unscaledDeltaTime);
        FlashIfInvuln();
    }

    private void Move(InputManager input)
    {
        float axis = input != null ? input.MoveAxis : 0f;
        var pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + axis * _stat.MoveSpeed * Time.deltaTime, _stat.LeftBound, _stat.RightBound);
        transform.position = pos;
    }

    private void FlashIfInvuln()
    {
        if (_sprite == null) return;
        _sprite.color = _state.IsInvulnerable
            ? new Color(1f, 1f, 1f, Mathf.PingPong(Time.unscaledTime * 8f, 1f) * 0.7f + 0.3f)
            : Color.white;
    }

    /// <summary>Called by Projectile via IDamageable. Any hit costs one life unless blocked by i-frames (D2).</summary>
    public void TakeDamage(int damage)
    {
        if (_state.TakeHit()) return;
        OnPlayerHit?.Invoke();
        OnPlayerDeath?.Invoke();
    }

    /// <summary>Called via PowerUpSystem.OnPowerUpChosen. Delegates to stat or GameManager.</summary>
    public void ApplyPowerUp(PowerUpData data)
    {
        if (data.effect == PowerUpEffect.ExtraLife)
        {
            GameManager.Instance?.AddLife(1);
            return;
        }
        _stat.Apply(data);
        weapon?.ApplyPowerUp(data);
        Debug.Log($"[PlayerShipContext] Applied {data.displayName} — effect={data.effect} mag={data.magnitude}");
    }
}
