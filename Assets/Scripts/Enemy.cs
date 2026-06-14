using System;
using UnityEngine;

/// <summary>
/// A single invader. Holds HP and a point value, takes damage from player
/// projectiles, and on death broadcasts so the formation, score, audio, and
/// juice react. Does not move itself (EnemyFormation moves it) or fire itself
/// (EnemyFireController). See Enemy.md GDD.
/// </summary>
public class Enemy : MonoBehaviour
{
    /// <summary>Broadcast for any-enemy-killed reactions (score, audio, juice). Payload: points, world position.</summary>
    public static event Action<int, Vector3> OnEnemyKilled;

    /// <summary>Per-instance death, used by the owning formation to track count + recycle.</summary>
    public event Action<Enemy> OnDeath;

    private int _health;
    private int _points;
    private bool _alive;

    public bool IsAlive => _alive;

    /// <summary>Configure on spawn from the level's row config.</summary>
    public void Spawn(int health, int points)
    {
        _health = Mathf.Max(1, health);
        _points = points;
        _alive = true;
    }

    /// <summary>Called directly by a player projectile on collision.</summary>
    public void TakeDamage(int amount)
    {
        if (!_alive) return;
        _health -= amount;
        if (_health <= 0) Die();
    }

    private void Die()
    {
        _alive = false;
        OnEnemyKilled?.Invoke(_points, transform.position);
        OnDeath?.Invoke(this);
    }
}
