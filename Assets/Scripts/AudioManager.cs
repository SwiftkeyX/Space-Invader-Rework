using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Centralized audio hub. Plays SFX and music in response to game events.
/// Lives in Bootstrap so music persists across additive scene loads.
/// SFX use a pooled AudioSource pool (zero steady-state GC).
/// Repetitive SFX are throttled by MinRetriggerInterval (unscaled time, hit-stop safe).
/// Each clip has its own Inspector volume knob. See AudioManager.md GDD.
/// </summary>
public class AudioManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SceneLoader sceneLoader;

    [Header("SFX Clips")]
    [SerializeField] private AudioClip sfxEnemyKill;
    [SerializeField] private AudioClip sfxPlayerHit;
    [SerializeField] private AudioClip sfxPlayerDeath;
    [SerializeField] private AudioClip sfxLevelClear;
    [SerializeField] private AudioClip sfxLevelStart;
    [SerializeField] private AudioClip sfxPowerUp;

    [Header("SFX Volumes")]
    [SerializeField][Range(0f, 1f)] private float volEnemyKill   = 0.7f;
    [SerializeField][Range(0f, 1f)] private float volPlayerHit   = 1.0f;
    [SerializeField][Range(0f, 1f)] private float volPlayerDeath = 1.0f;
    [SerializeField][Range(0f, 1f)] private float volLevelClear  = 0.9f;
    [SerializeField][Range(0f, 1f)] private float volLevelStart  = 0.8f;
    [SerializeField][Range(0f, 1f)] private float volPowerUp     = 0.9f;

    [Header("Music Clips")]
    [SerializeField] private AudioClip musicGameplay;
    [SerializeField] private AudioClip musicGameOver;
    [SerializeField] private AudioClip musicVictory;

    [Header("Music Volumes")]
    [SerializeField][Range(0f, 1f)] private float volMusicGameplay = 0.6f;
    [SerializeField][Range(0f, 1f)] private float volMusicGameOver = 0.6f;
    [SerializeField][Range(0f, 1f)] private float volMusicVictory  = 0.75f;

    [Header("SFX Pool")]
    [SerializeField] private int poolSize = 16;

    [Header("Throttle")]
    [SerializeField] private float killRetriggerInterval = 0.05f;

    private AudioSource _musicSource;
    private readonly List<AudioSource> _sfxPool = new();
    private float _lastKillTime = -999f;

    // Cross-scene subscriptions (GameLogic systems found on scene load)
    private LevelManager       _levelManager;
    private PlayerShipContext  _playerShip;
    private PowerUpSystem      _powerUpSystem;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

    private void Awake()
    {
        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop        = true;
        _musicSource.playOnAwake = false;

        for (int i = 0; i < poolSize; i++)
        {
            var child = new GameObject($"SFX_{i}");
            child.transform.SetParent(transform, false);
            var src = child.AddComponent<AudioSource>();
            src.loop        = false;
            src.playOnAwake = false;
            _sfxPool.Add(src);
        }
    }

    // Static event — safe to subscribe in OnEnable (no instance dependency).
    private void OnEnable()  => Enemy.OnEnemyKilled += HandleEnemyKilled;
    private void OnDisable() => Enemy.OnEnemyKilled -= HandleEnemyKilled;

    private void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRunStarted += HandleRunStarted;
            GameManager.Instance.OnRunEnded   += HandleRunEnded;
        }
        if (sceneLoader != null)
            sceneLoader.OnSceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnRunStarted -= HandleRunStarted;
            GameManager.Instance.OnRunEnded   -= HandleRunEnded;
        }
        if (sceneLoader != null)
            sceneLoader.OnSceneLoaded -= HandleSceneLoaded;
        UnsubscribeFromGameplayScene();
    }

    // -------------------------------------------------------------------------
    // Scene-load driven cross-scene subscriptions
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
        _levelManager  = FindFirstObjectByType<LevelManager>();
        _playerShip    = FindFirstObjectByType<PlayerShipContext>();
        _powerUpSystem = FindFirstObjectByType<PowerUpSystem>();

        if (_levelManager != null)
        {
            _levelManager.OnLevelStarted += HandleLevelStarted;
            _levelManager.OnLevelCleared += HandleLevelCleared;
        }
        if (_playerShip != null)
        {
            _playerShip.OnPlayerHit   += HandlePlayerHit;
            _playerShip.OnPlayerDeath += HandlePlayerDeath;
        }
        if (_powerUpSystem != null)
            _powerUpSystem.OnPowerUpChosen += HandlePowerUpChosen;
    }

    private void UnsubscribeFromGameplayScene()
    {
        if (_levelManager != null)
        {
            _levelManager.OnLevelStarted -= HandleLevelStarted;
            _levelManager.OnLevelCleared -= HandleLevelCleared;
            _levelManager = null;
        }
        if (_playerShip != null)
        {
            _playerShip.OnPlayerHit   -= HandlePlayerHit;
            _playerShip.OnPlayerDeath -= HandlePlayerDeath;
            _playerShip = null;
        }
        if (_powerUpSystem != null)
        {
            _powerUpSystem.OnPowerUpChosen -= HandlePowerUpChosen;
            _powerUpSystem = null;
        }
    }

    // -------------------------------------------------------------------------
    // Event handlers
    // -------------------------------------------------------------------------

    private void HandleEnemyKilled(int _, Vector3 __)
    {
        float now = Time.realtimeSinceStartup;
        if (now - _lastKillTime < killRetriggerInterval) return;
        _lastKillTime = now;
        PlaySFX(sfxEnemyKill, volEnemyKill);
    }

    private void HandlePlayerHit()                    => PlaySFX(sfxPlayerHit,   volPlayerHit);
    private void HandlePlayerDeath()                  => PlaySFX(sfxPlayerDeath, volPlayerDeath);
    private void HandlePowerUpChosen(PowerUpData _)   => PlaySFX(sfxPowerUp,     volPowerUp);
    private void HandleLevelStarted(LevelData _)      => PlaySFX(sfxLevelStart,  volLevelStart);
    private void HandleLevelCleared(int _)            => PlaySFX(sfxLevelClear,  volLevelClear);

    private void HandleRunStarted() => PlayMusic(musicGameplay, volMusicGameplay);

    private void HandleRunEnded(GameState result) =>
        PlayMusic(result == GameState.Won ? musicVictory : musicGameOver,
                  result == GameState.Won ? volMusicVictory : volMusicGameOver);

    // -------------------------------------------------------------------------
    // Playback helpers
    // -------------------------------------------------------------------------

    private void PlaySFX(AudioClip clip, float volume)
    {
        if (clip == null) return;
        var src = AcquireSource();
        if (src == null) return;
        src.volume = volume;
        src.clip   = clip;
        src.Play();
    }

    private void PlayMusic(AudioClip clip, float volume)
    {
        if (_musicSource == null) return;
        if (clip == null) { _musicSource.Stop(); return; }
        if (_musicSource.clip == clip && _musicSource.isPlaying) return;
        _musicSource.clip   = clip;
        _musicSource.volume = volume;
        _musicSource.Play();
    }

    private AudioSource AcquireSource()
    {
        foreach (var src in _sfxPool)
            if (!src.isPlaying) return src;
        return null;
    }
}
