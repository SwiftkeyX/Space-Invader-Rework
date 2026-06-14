using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// The player's ship. Horizontal-only movement (D1/N1), fires upward, survives
/// via lives + i-frames (D2). Reads intent from InputManager (resolved through
/// GameManager), spawns pooled player projectiles, and announces hits/deaths.
/// See PlayerShip.md GDD. (Power-up modifiers wired in Tier 3.)
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class PlayerShip : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 9f;
    [SerializeField] private float leftBound = -8f;
    [SerializeField] private float rightBound = 8f;

    [Header("Firing")]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Transform muzzle;
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

    /// <summary>Fired on a vulnerable hit (feedback hook: flash/SFX/shake).</summary>
    public event Action OnPlayerHit;
    /// <summary>Fired when a hit costs a life (GameManager decrements lives).</summary>
    public event Action OnPlayerDeath;

    private float _fireTimer;
    private float _invulnTimer;
    private bool _invulnerable;
    private IObjectPool<Projectile> _pool;
    private SpriteRenderer _sprite;
    private int _multiShot = 1; // extra simultaneous bullets
    private PowerUpSystem _powerUpSystem;

    private void Awake()
    {
        _sprite = GetComponentInChildren<SpriteRenderer>();
        _pool = new ObjectPool<Projectile>(
            () => Instantiate(projectilePrefab),
            p => p.gameObject.SetActive(true),
            p => p.gameObject.SetActive(false),
            p => Destroy(p.gameObject),
            false, 32, 256);

        // Pre-warm: zero Instantiate calls on the first-shot hot path.
        if (projectilePrefab != null)
        {
            var warm = new Projectile[32];
            for (int i = 0; i < 32; i++) warm[i] = _pool.Get();
            for (int i = 0; i < 32; i++) _pool.Release(warm[i]);
        }
    }

    private void Start()
    {
        if (GameManager.Instance != null)
            OnPlayerDeath += GameManager.Instance.HandlePlayerDeath;

        _powerUpSystem = FindFirstObjectByType<PowerUpSystem>();
        if (_powerUpSystem != null)
            _powerUpSystem.OnPowerUpChosen += ApplyPowerUp;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
            OnPlayerDeath -= GameManager.Instance.HandlePlayerDeath;
        if (_powerUpSystem != null)
            _powerUpSystem.OnPowerUpChosen -= ApplyPowerUp;
    }

    private void Update()
    {
        if (GameManager.Instance == null || GameManager.Instance.State != GameState.Running) return;

        var input = GameManager.Instance.Input;

        // Movement (horizontal only).
        float axis = input != null ? input.MoveAxis : 0f;
        var pos = transform.position;
        pos.x = Mathf.Clamp(pos.x + axis * moveSpeed * Time.deltaTime, leftBound, rightBound);
        transform.position = pos;

        // Firing (held + cooldown).
        _fireTimer -= Time.deltaTime;
        bool fire = input != null && input.FireHeld;
        if (fire && _fireTimer <= 0f)
        {
            Fire();
            _fireTimer = baseFireCooldown;
        }

        // I-frames use unscaled time (survive hit-stop / timeScale = 0).
        if (_invulnerable)
        {
            _invulnTimer -= Time.unscaledDeltaTime;
            if (_sprite != null)
            {
                var c = _sprite.color;
                c.a = Mathf.PingPong(Time.unscaledTime * 8f, 1f) * 0.7f + 0.3f;
                _sprite.color = c;
            }
            if (_invulnTimer <= 0f) EndInvuln();
        }
    }

    private void Fire()
    {
        Vector3 spawn = muzzle != null ? muzzle.position : transform.position;
        if (_multiShot <= 1)
        {
            var p = _pool.Get();
            p.transform.SetPositionAndRotation(spawn, Quaternion.identity);
            p.Init(Team.Player, Vector2.up, projectileSpeed, projectileDamage, projectileLifetime, _pool);
        }
        else
        {
            float spread = 15f; // degrees between shots
            float halfSpan = spread * (_multiShot - 1) / 2f;
            for (int i = 0; i < _multiShot; i++)
            {
                float angle = -halfSpan + spread * i;
                var dir = Quaternion.Euler(0, 0, angle) * Vector2.up;
                var p = _pool.Get();
                p.transform.SetPositionAndRotation(spawn, Quaternion.identity);
                p.Init(Team.Player, dir, projectileSpeed, projectileDamage, projectileLifetime, _pool);
            }
        }
    }

    /// <summary>Called via PowerUpSystem.OnPowerUpChosen. Applies run-scoped stat modifier.</summary>
    public void ApplyPowerUp(PowerUpData data)
    {
        switch (data.effect)
        {
            case PowerUpEffect.RapidFire:
                baseFireCooldown = Mathf.Max(minFireCooldown, baseFireCooldown - data.magnitude);
                break;
            case PowerUpEffect.MultiShot:
                _multiShot = Mathf.Min(_multiShot + (int)data.magnitude, maxMultiShot);
                break;
            case PowerUpEffect.PowerShot:
                projectileDamage = Mathf.Min(projectileDamage + (int)data.magnitude, maxProjectileDamage);
                break;
            case PowerUpEffect.Swift:
                moveSpeed = Mathf.Min(moveSpeed + data.magnitude, maxMoveSpeed);
                break;
            case PowerUpEffect.BulletSpeed:
                projectileSpeed = Mathf.Min(projectileSpeed + data.magnitude, maxProjectileSpeed);
                break;
            case PowerUpEffect.ExtraLife:
                GameManager.Instance?.AddLife(1);
                break;
        }
        Debug.Log($"[PlayerShip] Applied {data.displayName} — effect={data.effect} mag={data.magnitude}");
    }

    /// <summary>Called directly by an enemy projectile on collision.</summary>
    public void TakeHit()
    {
        if (_invulnerable) return; // I-frame invariant (D2): zero damage, no life cost.

        OnPlayerHit?.Invoke();
        OnPlayerDeath?.Invoke();
        BeginInvuln();
    }

    private void BeginInvuln()
    {
        _invulnerable = true;
        _invulnTimer = invulnDuration;
    }

    private void EndInvuln()
    {
        _invulnerable = false;
        if (_sprite != null)
        {
            var c = _sprite.color;
            c.a = 1f;
            _sprite.color = c;
        }
    }
}
