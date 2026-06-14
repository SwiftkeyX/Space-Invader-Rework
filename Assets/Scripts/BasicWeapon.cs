using UnityEngine;

/// <summary>Standard player weapon. Fires a spread pattern; MultiShot=1 fires dead-straight (angle=0).</summary>
public class BasicWeapon : Weapon
{
    protected override void Fire(PlayerShipStat stat)
    {
        Vector3 spawn = muzzle != null ? muzzle.position : transform.position;
        const float SpreadDeg = 15f;
        float halfSpan = SpreadDeg * (stat.MultiShot - 1) / 2f;
        for (int i = 0; i < stat.MultiShot; i++)
        {
            float angle = -halfSpan + SpreadDeg * i;
            var dir = (Vector2)(Quaternion.Euler(0f, 0f, angle) * Vector2.up);
            SpawnProjectile(spawn, dir, stat);
        }
    }

    private void SpawnProjectile(Vector3 pos, Vector2 dir, PlayerShipStat stat)
    {
        var p = Pool.Get();
        p.transform.SetPositionAndRotation(pos, Quaternion.identity);
        p.Init(Team.Player, dir, stat.ProjectileSpeed, stat.ProjectileDamage, stat.ProjectileLifetime, Pool);
    }
}
