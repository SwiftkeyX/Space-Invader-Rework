using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// The marching invader grid. Spawns the wave on level start, moves the whole
/// formation in lockstep, reverses + steps down at the edges, speeds up as it
/// thins, and fires OnFormationCleared when empty. Owns its Enemy children
/// (pooled). See EnemyFormation.md GDD.
/// </summary>
public class EnemyFormation : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private Enemy enemyPrefab;

    [Header("Bounds")]
    [SerializeField] private float leftBound = -7f;
    [SerializeField] private float rightBound = 7f;
    [SerializeField] private float topY = 4f;
    [Tooltip("If the formation descends to this Y, the run is lost.")]
    [SerializeField] private float playerLineY = -3.5f;

    public event Action OnFormationCleared;

    private readonly List<Enemy> _living = new();
    public IReadOnlyList<Enemy> LivingEnemies => _living;

    private IObjectPool<Enemy> _pool;
    private int _startCount;
    private float _baseSpeed, _maxSpeed, _stepDown;
    private int _direction = 1;
    private bool _active;

    private void Awake()
    {
        _pool = new ObjectPool<Enemy>(
            () => Instantiate(enemyPrefab),
            e => e.gameObject.SetActive(true),
            e => e.gameObject.SetActive(false),
            e => Destroy(e.gameObject),
            false, 64, 256);

        // Pre-warm: zero Instantiate calls when a wave spawns.
        if (enemyPrefab != null)
        {
            var warm = new Enemy[64];
            for (int i = 0; i < 64; i++) warm[i] = _pool.Get();
            for (int i = 0; i < 64; i++) _pool.Release(warm[i]);
        }
    }

    private void OnEnable()
    {
        if (levelManager != null) levelManager.OnLevelStarted += HandleLevelStarted;
    }

    private void OnDisable()
    {
        if (levelManager != null) levelManager.OnLevelStarted -= HandleLevelStarted;
    }

    private void HandleLevelStarted(LevelData data)
    {
        ClearAll();
        _baseSpeed = data.baseMarchSpeed;
        _maxSpeed = data.maxMarchSpeed;
        _stepDown = data.stepDownDistance;
        _direction = 1;

        float gridWidth = (data.columns - 1) * data.horizontalSpacing;
        float startX = -gridWidth / 2f;
        for (int row = 0; row < data.rows; row++)
        {
            for (int col = 0; col < data.columns; col++)
            {
                var e = _pool.Get();
                e.transform.SetParent(transform, false);
                e.transform.position = new Vector3(startX + col * data.horizontalSpacing, topY - row * data.verticalSpacing, 0f);
                e.Spawn(data.HealthForRow(row), data.pointsPerEnemy);
                e.OnDeath += HandleEnemyDeath;
                _living.Add(e);
            }
        }
        _startCount = _living.Count;
        _active = _startCount > 0;
    }

    private void HandleEnemyDeath(Enemy e)
    {
        e.OnDeath -= HandleEnemyDeath;
        _living.Remove(e);
        _pool.Release(e);
        if (_active && _living.Count == 0)
        {
            _active = false;
            OnFormationCleared?.Invoke();
        }
    }

    private void Update()
    {
        if (!_active || _living.Count == 0) return;

        float dx = _direction * ComputeSpeed() * Time.deltaTime;
        var (minX, maxX, minY) = ComputeBounds();

        if (CheckEdge(dx, minX, maxX))
            StepDown(minY);
        else
            MarchStep(dx);
    }

    private float ComputeSpeed()
    {
        float frac = (float)_living.Count / Mathf.Max(1, _startCount);
        return Mathf.Lerp(_maxSpeed, _baseSpeed, frac);
    }

    private (float minX, float maxX, float minY) ComputeBounds()
    {
        float minX = float.MaxValue, maxX = float.MinValue, minY = float.MaxValue;
        foreach (var e in _living)
        {
            var p = e.transform.position;
            if (p.x < minX) minX = p.x;
            if (p.x > maxX) maxX = p.x;
            if (p.y < minY) minY = p.y;
        }
        return (minX, maxX, minY);
    }

    private bool CheckEdge(float dx, float minX, float maxX)
    {
        return (_direction > 0 && maxX + dx >= rightBound) || (_direction < 0 && minX + dx <= leftBound);
    }

    private void MarchStep(float dx)
    {
        foreach (var e in _living)
            e.transform.position += Vector3.right * dx;
    }

    private void StepDown(float minY)
    {
        _direction = -_direction;
        foreach (var e in _living)
            e.transform.position += Vector3.down * _stepDown;

        if (minY - _stepDown <= playerLineY)
        {
            _active = false;
            if (GameManager.Instance != null) GameManager.Instance.ForceGameOver();
        }
    }

    private void ClearAll()
    {
        foreach (var e in _living)
        {
            e.OnDeath -= HandleEnemyDeath;
            _pool.Release(e);
        }
        _living.Clear();
        _active = false;
    }
}
