using UnityEngine;

/// <summary>Effect type for a power-up pick. Maps onto PlayerShip's modifiable stat surface.</summary>
public enum PowerUpEffect
{
    RapidFire,    // reduces fire cooldown
    MultiShot,    // +1 simultaneous bullet (spread)
    PowerShot,    // +projectile damage
    Swift,        // +move speed
    ExtraLife,    // +1 life (via GameManager.AddLife)
    BulletSpeed,  // +projectile speed
}

/// <summary>
/// Data asset for one power-up in the catalog. Authored by designers; no code logic lives here.
/// See PowerUpSystem.md GDD.
/// </summary>
[CreateAssetMenu(menuName = "SpaceInvader/Power Up Data", fileName = "PowerUpData")]
public class PowerUpData : ScriptableObject
{
    public string id;
    public string displayName;
    [TextArea] public string description;
    public PowerUpEffect effect;
    [Tooltip("Magnitude of the effect (e.g. 0.25 = +25% fire rate, 1 = +1 damage).")]
    public float magnitude = 1f;
}
