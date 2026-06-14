using System;
using UnityEngine;

/// <summary>
/// Drives the 6 fixed levels. On the current level it broadcasts OnLevelStarted
/// (carrying the LevelData) so the enemy systems build the wave; when the
/// formation is cleared it fires OnLevelCleared and tells GameManager to advance
/// (or win at level 6). All difficulty values live in LevelData assets.
/// See LevelManager.md GDD.
/// </summary>
public class LevelManager : MonoBehaviour
{
    [SerializeField] private EnemyFormation formation;
    [SerializeField] private LevelData[] levels = new LevelData[6];
    [Tooltip("Auto-start a run when GameLogic loads (v1 has no menu yet).")]
    [SerializeField] private bool autoStartRun = true;

    public event Action<LevelData> OnLevelStarted;
    public event Action<int> OnLevelCleared;

    private void OnEnable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelChanged += HandleLevelChanged;
        if (formation != null)
            formation.OnFormationCleared += HandleFormationCleared;
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnLevelChanged -= HandleLevelChanged;
        if (formation != null)
            formation.OnFormationCleared -= HandleFormationCleared;
    }

    private void Start()
    {
        if (autoStartRun && GameManager.Instance != null && GameManager.Instance.State != GameState.Running)
            GameManager.Instance.StartRun();
    }

    private void HandleLevelChanged(int levelIndex)
    {
        var data = GetLevel(levelIndex);
        if (data == null)
        {
            Debug.LogError($"[LevelManager] No LevelData assigned for level {levelIndex}.");
            return;
        }
        OnLevelStarted?.Invoke(data);
    }

    private void HandleFormationCleared()
    {
        int index = GameManager.Instance != null ? GameManager.Instance.CurrentLevelIndex : 0;
        OnLevelCleared?.Invoke(index);
        if (GameManager.Instance != null)
            GameManager.Instance.HandleLevelCleared();
    }

    private LevelData GetLevel(int oneBasedIndex)
    {
        int i = oneBasedIndex - 1;
        if (levels == null || i < 0 || i >= levels.Length) return null;
        return levels[i];
    }
}
