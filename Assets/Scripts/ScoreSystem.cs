using System;
using UnityEngine;

/// <summary>
/// Tracks the run score. Subscribes to Enemy.OnEnemyKilled (static event) and
/// GameManager.OnRunStarted (reset). Fires OnScoreChanged so UIManager can
/// update the HUD. Optionally applies a combo multiplier that ramps on rapid
/// consecutive kills and decays after ComboWindow seconds (unscaled, hit-stop-safe).
/// See ScoreSystem.md GDD.
/// </summary>
public class ScoreSystem : MonoBehaviour
{
    [Header("Combo")]
    [Tooltip("Multiplier gained per chained kill. Set 0 to disable combos.")]
    [SerializeField] private float comboStep = 0.1f;
    [SerializeField] private float maxCombo = 4f;
    [Tooltip("Seconds without a kill before combo decays to 1 (unscaled).")]
    [SerializeField] private float comboWindow = 1.5f;

    public int Score { get; private set; }
    public float ComboMultiplier { get; private set; } = 1f;

    public event Action<int> OnScoreChanged;    // current score
    public event Action<float> OnComboChanged;  // current multiplier (1 = no combo)

    private int _comboCount;
    private float _comboTimer;
    private bool _comboActive;

    private void OnEnable()
    {
        Enemy.OnEnemyKilled += HandleEnemyKilled;
        if (GameManager.Instance != null)
            GameManager.Instance.OnRunStarted += ResetScore;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyKilled -= HandleEnemyKilled;
        if (GameManager.Instance != null)
            GameManager.Instance.OnRunStarted -= ResetScore;
    }

    private void Update()
    {
        if (!_comboActive) return;

        _comboTimer -= Time.unscaledDeltaTime; // unscaled: survives hit-stop
        if (_comboTimer <= 0f) DecayCombo();
    }

    private void HandleEnemyKilled(int points, Vector3 _)
    {
        if (comboStep > 0f)
        {
            _comboCount++;
            ComboMultiplier = 1f + Mathf.Min(_comboCount * comboStep, maxCombo - 1f);
            _comboTimer = comboWindow;
            _comboActive = true;
            OnComboChanged?.Invoke(ComboMultiplier);
        }

        Score += Mathf.RoundToInt(points * ComboMultiplier);
        OnScoreChanged?.Invoke(Score);
    }

    private void DecayCombo()
    {
        _comboCount = 0;
        ComboMultiplier = 1f;
        _comboActive = false;
        OnComboChanged?.Invoke(ComboMultiplier);
    }

    private void ResetScore()
    {
        Score = 0;
        DecayCombo();
        OnScoreChanged?.Invoke(Score);
    }
}
