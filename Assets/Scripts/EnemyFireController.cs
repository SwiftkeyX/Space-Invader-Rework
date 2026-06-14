using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// Selects shooters from the formation and spawns enemy projectiles, scaling
/// shooter count / interval / bullet speed per level. Enemy bullets travel
/// straight down in v1 (aimed patterns are a later extension). Spawns pooled
/// enemy projectiles. See EnemyFireController.md GDD.
/// </summary>
public class EnemyFireController : MonoBehaviour
{
    [SerializeField] private LevelManager levelManager;
    [SerializeField] private EnemyFormation formation;
    [SerializeField] private Projectile projectilePrefab;

    private int _activeShooters;
    private float _fireInterval;
    private float _bulletSpeed;
    private int _bulletDamage;
    private float _bulletLifetime;
    private float _muzzleOffset;
    private float _timer;
    private bool _active;
    private IObjectPool<Projectile> _pool;

    private void Awake()
    {
        _pool = new ObjectPool<Projectile>(
            () => Instantiate(projectilePrefab),
            p => p.gameObject.SetActive(true),
            p => p.gameObject.SetActive(false),
            p => Destroy(p.gameObject),
            false, 64, 512);

        // Pre-warm: zero Instantiate calls during sustained enemy fire.
        if (projectilePrefab != null)
        {
            var warm = new Projectile[32];
            for (int i = 0; i < 32; i++) warm[i] = _pool.Get();
            for (int i = 0; i < 32; i++) _pool.Release(warm[i]);
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
        _activeShooters = Mathf.Max(1, data.activeShooters);
        _fireInterval   = Mathf.Max(0.1f, data.fireInterval);
        _bulletSpeed    = data.enemyBulletSpeed;
        _bulletDamage   = data.enemyBulletDamage;
        _bulletLifetime = data.enemyBulletLifetime;
        _muzzleOffset   = data.enemyMuzzleOffset;
        _timer          = _fireInterval;
        _active         = true;
    }

    private void Update()
    {
        if (!_active || formation == null) return;

        var living = formation.LivingEnemies;
        if (living.Count == 0) return;

        _timer -= Time.deltaTime;
        if (_timer > 0f) return;
        _timer = _fireInterval;

        int shooters = Mathf.Min(_activeShooters, living.Count);
        for (int i = 0; i < shooters; i++)
        {
            var shooter = living[Random.Range(0, living.Count)];
            if (shooter == null || !shooter.IsAlive) continue;

            var p = _pool.Get();
            p.transform.SetPositionAndRotation(shooter.transform.position + Vector3.down * _muzzleOffset, Quaternion.identity);
            p.Init(Team.Enemy, Vector2.down, _bulletSpeed, _bulletDamage, _bulletLifetime, _pool);
        }
    }
}
