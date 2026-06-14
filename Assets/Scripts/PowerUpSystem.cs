using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Roguelike upgrade layer. Subscribes to LevelManager.OnLevelCleared and builds a
/// random offer of power-ups for the player to pick. Fires OnPowerUpOffered so
/// UIManager can show the pick screen; fires OnPowerUpChosen so PlayerShip applies
/// the run-scoped effect. All state resets on run start (D3). See PowerUpSystem.md.
/// </summary>
public class PowerUpSystem : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private PowerUpData[] catalog;
    [SerializeField] private int offerSize = 3;

    /// <summary>Fired between levels (1–5 cleared) with the offer set. UIManager shows pick screen.</summary>
    public event Action<PowerUpData[]> OnPowerUpOffered;
    /// <summary>Fired when the player picks. PlayerShip and others apply the effect.</summary>
    public event Action<PowerUpData> OnPowerUpChosen;

    private PowerUpData[] _currentOffer;
    private readonly List<PowerUpData> _chosenThisRun = new();

    private void OnEnable()
    {
        if (levelManager != null) levelManager.OnLevelCleared += HandleLevelCleared;
        if (GameManager.Instance != null) GameManager.Instance.OnRunStarted += ResetRun;
    }

    private void OnDisable()
    {
        if (levelManager != null) levelManager.OnLevelCleared -= HandleLevelCleared;
        if (GameManager.Instance != null) GameManager.Instance.OnRunStarted -= ResetRun;
    }

    private void HandleLevelCleared(int level)
    {
        // No offer after the final level (that clear is the run win).
        if (GameManager.Instance != null && level >= GameManager.Instance.TotalLevels) return;

        _currentOffer = BuildOffer();
        if (_currentOffer.Length == 0) return;

        Debug.Log($"[PowerUpSystem] Offering {_currentOffer.Length} power-ups after level {level}: " +
                  string.Join(", ", Array.ConvertAll(_currentOffer, p => p.displayName)));
        OnPowerUpOffered?.Invoke(_currentOffer);
        // UIManager (Tier 3) will call SelectPowerUp when the player picks.
        // Until UIManager is wired, the offer fires but the next level auto-starts.
    }

    /// <summary>Called by UIManager when the player selects a card.</summary>
    public void SelectPowerUp(PowerUpData chosen)
    {
        if (chosen == null) return;
        _chosenThisRun.Add(chosen);
        Debug.Log($"[PowerUpSystem] Chosen: {chosen.displayName}");
        OnPowerUpChosen?.Invoke(chosen);
    }

    private PowerUpData[] BuildOffer()
    {
        if (catalog == null || catalog.Length == 0) return Array.Empty<PowerUpData>();

        // Filter out extra_life if player is already at max lives.
        int currentLives = GameManager.Instance != null ? GameManager.Instance.Lives : int.MaxValue;
        int maxLives = GameManager.Instance != null ? GameManager.Instance.MaxLives : int.MaxValue;

        var pool = new List<PowerUpData>(catalog.Length);
        foreach (var p in catalog)
        {
            if (p == null) continue;
            if (p.effect == PowerUpEffect.ExtraLife && currentLives >= maxLives) continue;
            pool.Add(p);
        }

        int count = Mathf.Min(offerSize, pool.Count);
        var offer = new PowerUpData[count];
        for (int i = 0; i < count; i++)
        {
            int pick = UnityEngine.Random.Range(i, pool.Count);
            (pool[i], pool[pick]) = (pool[pick], pool[i]); // Fisher-Yates partial shuffle
            offer[i] = pool[i];
        }
        return offer;
    }

    private void ResetRun()
    {
        _chosenThisRun.Clear();
        _currentOffer = null;
    }
}
