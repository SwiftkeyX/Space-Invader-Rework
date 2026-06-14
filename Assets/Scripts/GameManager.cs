using System;
using UnityEngine;

/// <summary>
/// Run lifecycle states. See GameManager.md GDD.
/// </summary>
public enum GameState
{
    Boot,
    Running,
    Won,
    Lost
}

/// <summary>
/// The only singleton in the project. Owns run state (lives, current level
/// index, run start/end) and broadcasts every change as a C# event. Lives in
/// Bootstrap.unity. Never uses DontDestroyOnLoad; never loads scenes directly
/// (requests SceneLoader). See GameManager.md.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Run Config")]
    [Tooltip("Lives the player starts a run with (D2). Primary Phase 3 feel knob.")]
    [SerializeField] private int startingLives = 3;
    [Tooltip("Fixed level count for a run (D5). Do not change for v1.")]
    [SerializeField] private int totalLevels = 6;

    [Header("References")]
    [SerializeField] private SceneLoader sceneLoader;
    [SerializeField] private InputManager inputManager;

    /// <summary>Cross-scene access to the persistent InputManager (resolution hub; avoids Find).</summary>
    public InputManager Input => inputManager;

    [Header("Lives")]
    [SerializeField] private int maxLives = 5;

    public int Lives { get; private set; }
    public int MaxLives => maxLives;
    public int CurrentLevelIndex { get; private set; }
    public GameState State { get; private set; } = GameState.Boot;
    public int TotalLevels => totalLevels;

    // Run-state events. Single source of truth for the rest of the game.
    public event Action OnRunStarted;
    public event Action<GameState> OnRunEnded;   // carries Won or Lost
    public event Action<int> OnLivesChanged;     // carries current lives
    public event Action<int> OnLevelChanged;     // carries current level index (1-based)

    private UIManager _uiManager;

    private void Awake()
    {
        // Single-singleton invariant: a duplicate self-destructs.
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (sceneLoader != null)
            sceneLoader.OnSceneLoaded += HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
        if (sceneLoader != null)
            sceneLoader.OnSceneLoaded -= HandleSceneLoaded;
        if (_uiManager != null)
            _uiManager.OnRestartRequested -= RequestRestart;
    }

    private void HandleSceneLoaded(SceneLoader.SceneLabel label)
    {
        if (label != SceneLoader.SceneLabel.Gameplay) return;
        _uiManager = FindFirstObjectByType<UIManager>();
        if (_uiManager != null)
            _uiManager.OnRestartRequested += RequestRestart;
    }

    /// <summary>Begin a fresh run (lives reset, level 1, Running).</summary>
    public void StartRun()
    {
        Lives = startingLives;
        CurrentLevelIndex = 1;
        State = GameState.Running;
        OnRunStarted?.Invoke();
        OnLivesChanged?.Invoke(Lives);
        OnLevelChanged?.Invoke(CurrentLevelIndex);
    }

    /// <summary>
    /// Subscribed to PlayerShip.OnPlayerDeath (wired in Tier 2). Each call costs
    /// one life; reaching zero ends the run as a loss.
    /// </summary>
    public void HandlePlayerDeath()
    {
        if (State != GameState.Running) return;
        Lives = Mathf.Max(0, Lives - 1);
        OnLivesChanged?.Invoke(Lives);
        if (Lives <= 0) EndRun(GameState.Lost);
    }

    /// <summary>
    /// Subscribed to LevelManager.OnLevelCleared via event (wired in LevelManager.OnEnable).
    /// Advances to the next level, or wins the run when the final level is cleared.
    /// </summary>
    public void HandleLevelCleared(int _)
    {
        if (State != GameState.Running) return;
        if (CurrentLevelIndex >= totalLevels)
        {
            EndRun(GameState.Won);
            return;
        }
        CurrentLevelIndex++;
        OnLevelChanged?.Invoke(CurrentLevelIndex);
    }

    /// <summary>Adds lives (capped at MaxLives). Called by PowerUpSystem for extra_life power-up.</summary>
    public void AddLife(int count)
    {
        if (State != GameState.Running) return;
        Lives = Mathf.Min(Lives + count, maxLives);
        OnLivesChanged?.Invoke(Lives);
    }

    /// <summary>
    /// Immediate loss (e.g. the enemy formation reached the player line).
    /// See EnemyFormation.md — formation-reaches-line fail.
    /// </summary>
    public void ForceGameOver()
    {
        if (State != GameState.Running) return;
        Lives = 0;
        OnLivesChanged?.Invoke(Lives);
        EndRun(GameState.Lost);
    }

    private void EndRun(GameState result)
    {
        State = result;
        OnRunEnded?.Invoke(result);
    }

    /// <summary>Subscribed to UIManager.OnRestartRequested (wired in Tier 3).</summary>
    public void RequestRestart()
    {
        // Do not call StartRun() here — LevelManager.autoStartRun handles it once
        // the reloaded GameLogic scene's Start() fires (State will be Lost/Won then,
        // so the autoStartRun guard passes correctly).
        if (sceneLoader != null) sceneLoader.ReloadGameplay();
    }
}
