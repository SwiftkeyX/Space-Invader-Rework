using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Visual feedback hub — screen shake, hit-stop, and pooled particle bursts.
/// Lives in Bootstrap so effects persist across additive scene loads.
/// Uses unscaled time throughout so hit-stop (timeScale=0) never breaks shake.
/// See JuiceManager.md GDD.
/// </summary>
public class JuiceManager : MonoBehaviour
{
    // -------------------------------------------------------------------------
    // Inspector fields
    // -------------------------------------------------------------------------

    [Header("Shake — Kill")]
    [SerializeField] private float killShakeIntensity = 0.08f;
    [SerializeField] private float killShakeDuration  = 0.12f;

    [Header("Shake — Hit")]
    [SerializeField] private float hitShakeIntensity  = 0.3f;
    [SerializeField] private float hitShakeDuration   = 0.25f;

    [Header("Shake — Clear")]
    [SerializeField] private float clearShakeIntensity = 0.5f;
    [SerializeField] private float clearShakeDuration  = 0.4f;

    [Header("Hit-Stop")]
    [SerializeField] private float hitStopDuration = 0.08f;

    [Header("Burst Pools")]
    [SerializeField] private int killBurstPoolSize  = 12;
    [SerializeField] private int hitBurstPoolSize   = 6;
    [SerializeField] private int clearBurstPoolSize = 3;

    [Header("Burst Prefabs")]
    [SerializeField] private ParticleSystem killBurstPrefab;
    [SerializeField] private ParticleSystem hitBurstPrefab;
    [SerializeField] private ParticleSystem clearBurstPrefab;

    [Header("References")]
    [SerializeField] private SceneLoader sceneLoader;

    // -------------------------------------------------------------------------
    // Private state
    // -------------------------------------------------------------------------

    private Camera _cam;
    private Vector3 _basePos;

    // Shake state — multiple simultaneous shakes combine by max intensity
    private float _shakeIntensity;
    private float _shakeStartIntensity;
    private float _shakeDuration;
    private float _shakeElapsed;

    // Hit-stop
    private Coroutine _hitStopCoroutine;

    // Particle pools
    private readonly List<ParticleSystem> _killBursts  = new();
    private readonly List<ParticleSystem> _hitBursts   = new();
    private readonly List<ParticleSystem> _clearBursts = new();

    // Cross-scene refs
    private LevelManager      _levelManager;
    private PlayerShipContext _playerShip;

    // Max shake clamp (prevents runaway at L6 density)
    private const float MaxShakeIntensity = 0.6f;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _cam = Camera.main;
        if (_cam != null) _basePos = _cam.transform.position;

        if (killBurstPrefab  != null) WarmPool(_killBursts,  killBurstPrefab,  killBurstPoolSize);
        if (hitBurstPrefab   != null) WarmPool(_hitBursts,   hitBurstPrefab,   hitBurstPoolSize);
        if (clearBurstPrefab != null) WarmPool(_clearBursts, clearBurstPrefab, clearBurstPoolSize);
    }

    // Static event — safe to subscribe in OnEnable (no instance dependency).
    private void OnEnable()  => Enemy.OnEnemyKilled += HandleEnemyKilled;
    private void OnDisable() => Enemy.OnEnemyKilled -= HandleEnemyKilled;

    private void Start()
    {
        if (sceneLoader != null)
            sceneLoader.OnSceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (sceneLoader != null)
            sceneLoader.OnSceneLoaded -= HandleSceneLoaded;
        UnsubscribeFromGameplayScene();
    }

    private void Update()
    {
        if (_cam == null) return;

        if (_shakeElapsed < _shakeDuration)
        {
            _shakeElapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(_shakeElapsed / _shakeDuration);
            _shakeIntensity = Mathf.Lerp(_shakeStartIntensity, 0f, progress);
            Vector2 offset = UnityEngine.Random.insideUnitCircle * _shakeIntensity;
            _cam.transform.position = _basePos + (Vector3)offset;
        }
        else
        {
            _shakeIntensity = 0f;
            // Restore exact rest position so no drift accumulates
            _cam.transform.position = _basePos;
        }
    }

    // -------------------------------------------------------------------------
    // Scene-load driven cross-scene subscriptions (AudioManager pattern)
    // -------------------------------------------------------------------------

    private void HandleSceneLoaded(SceneLoader.SceneLabel label)
    {
        if (label == SceneLoader.SceneLabel.Gameplay)
            SubscribeToGameplayScene();
        else
            UnsubscribeFromGameplayScene();
    }

    private void SubscribeToGameplayScene()
    {
        _levelManager = FindFirstObjectByType<LevelManager>();
        _playerShip   = FindFirstObjectByType<PlayerShipContext>();

        if (_levelManager != null)
            _levelManager.OnLevelCleared += HandleLevelCleared;

        if (_playerShip != null)
        {
            _playerShip.Stat.OnPlayerHit   += HandlePlayerHit;
            _playerShip.Stat.OnPlayerDeath += HandlePlayerDeath;
        }
    }

    private void UnsubscribeFromGameplayScene()
    {
        if (_levelManager != null)
        {
            _levelManager.OnLevelCleared -= HandleLevelCleared;
            _levelManager = null;
        }
        if (_playerShip != null)
        {
            _playerShip.Stat.OnPlayerHit   -= HandlePlayerHit;
            _playerShip.Stat.OnPlayerDeath -= HandlePlayerDeath;
            _playerShip = null;
        }
    }

    // -------------------------------------------------------------------------
    // Event handlers
    // -------------------------------------------------------------------------

    private void HandleEnemyKilled(int _, Vector3 pos)
    {
        PlayBurst(_killBursts, pos);
        Shake(killShakeIntensity, killShakeDuration);
        // No hit-stop on enemy kills
    }

    private void HandlePlayerHit()
    {
        Vector3 pos = _playerShip != null ? _playerShip.transform.position : Vector3.zero;
        PlayBurst(_hitBursts, pos);
        Shake(hitShakeIntensity, hitShakeDuration);
        TriggerHitStop();
    }

    private void HandlePlayerDeath()
    {
        Vector3 pos = _playerShip != null ? _playerShip.transform.position : Vector3.zero;
        PlayBurst(_hitBursts, pos);
        Shake(hitShakeIntensity * 1.5f, hitShakeDuration * 1.5f);
        TriggerHitStop();
    }

    private void HandleLevelCleared(int _)
    {
        PlayBurst(_clearBursts, Vector3.zero);
        Shake(clearShakeIntensity, clearShakeDuration);
    }

    // -------------------------------------------------------------------------
    // Screen shake
    // -------------------------------------------------------------------------

    private void Shake(float intensity, float duration)
    {
        float clamped = Mathf.Min(intensity, MaxShakeIntensity);

        // Combine by picking max intensity; extend duration
        if (clamped > _shakeIntensity || _shakeElapsed >= _shakeDuration)
        {
            _shakeStartIntensity = Mathf.Max(clamped, _shakeIntensity);
            _shakeDuration       = Mathf.Max(duration, _shakeDuration - _shakeElapsed);
            _shakeElapsed        = 0f;
        }
        else
        {
            // Running shake is stronger — extend duration only
            _shakeDuration = Mathf.Max(_shakeDuration, _shakeElapsed + duration);
        }
    }

    // -------------------------------------------------------------------------
    // Hit-stop
    // -------------------------------------------------------------------------

    private void TriggerHitStop()
    {
        if (_hitStopCoroutine != null)
            StopCoroutine(_hitStopCoroutine);
        _hitStopCoroutine = StartCoroutine(HitStopRoutine());
    }

    private IEnumerator HitStopRoutine()
    {
        Time.timeScale = 0f;
        try
        {
            yield return new WaitForSecondsRealtime(hitStopDuration);
        }
        finally
        {
            Time.timeScale = 1f;
            _hitStopCoroutine = null;
        }
    }

    // -------------------------------------------------------------------------
    // Particle pools
    // -------------------------------------------------------------------------

    private void WarmPool(List<ParticleSystem> pool, ParticleSystem prefab, int size)
    {
        for (int i = 0; i < size; i++)
        {
            var ps = Instantiate(prefab, transform);
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            pool.Add(ps);
        }
    }

    private void PlayBurst(List<ParticleSystem> pool, Vector3 pos)
    {
        foreach (var ps in pool)
        {
            if (ps != null && !ps.isPlaying)
            {
                ps.transform.position = pos;
                ps.Play();
                return;
            }
        }
        // All busy — skip; no GC allocation
    }
}
