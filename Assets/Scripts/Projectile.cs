using UnityEngine;
using UnityEngine.Pool;

/// <summary>Which side fired a projectile. A projectile only damages the opposing team.</summary>
public enum Team { Player, Enemy }

/// <summary>
/// Shared bullet behaviour for both player and enemy fire. Moves in a straight
/// line, expires on lifetime/off-screen, and on collision deals damage to the
/// opposing team via a direct call. Pooled — never Instantiate/Destroy in play.
/// See Projectile.md GDD.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : MonoBehaviour
{
    private Team _team;
    private Vector2 _direction = Vector2.up;
    private float _speed;
    private int _damage;
    private float _lifetime;
    private float _age;
    private IObjectPool<Projectile> _pool;
    private bool _active;

    private const float OffscreenBound = 20f;

    public void Init(Team team, Vector2 direction, float speed, int damage, float lifetime, IObjectPool<Projectile> pool)
    {
        _team = team;
        _direction = direction.sqrMagnitude > 0.0001f ? direction.normalized : Vector2.up;
        _speed = speed;
        _damage = damage;
        _lifetime = lifetime;
        _pool = pool;
        _age = 0f;
        _active = true;
    }

    private void Update()
    {
        if (!_active) return;

        transform.position += (Vector3)(_direction * (_speed * Time.deltaTime));
        _age += Time.deltaTime;

        var p = transform.position;
        if (_age >= _lifetime || Mathf.Abs(p.y) > OffscreenBound || Mathf.Abs(p.x) > OffscreenBound)
            Release();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_active) return;

        if (_team == Team.Player)
        {
            var enemy = other.GetComponentInParent<Enemy>();
            if (enemy != null) { enemy.TakeDamage(_damage); Release(); }
        }
        else
        {
            var player = other.GetComponentInParent<PlayerShip>();
            if (player != null) { player.TakeHit(); Release(); }
        }
    }

    private void Release()
    {
        if (!_active) return;
        _active = false;
        if (_pool != null) _pool.Release(this);
        else gameObject.SetActive(false);
    }
}
